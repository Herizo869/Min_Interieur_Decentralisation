using Collectivites.Api.Data;
using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Models.Entities;
using Collectivites.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Collectivites.Api.Services;

/// <summary>Implémentation des litiges de limites territoriales (UC-10, UC-14).</summary>
public class LitigeService(AppDbContext db) : ILitigeService
{
    public async Task<LitigeResponse> SignalerAsync(SignalerLitigeRequest demande, string auteur, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(demande.Description))
        {
            throw new InvalidOperationException("La description du litige est obligatoire.");
        }

        if (demande.Geometrie is null)
        {
            throw new InvalidOperationException("La géométrie du constat terrain est obligatoire.");
        }

        demande.Geometrie.SRID = 4326;

        if (demande.CollectiviteAId == demande.CollectiviteBId)
        {
            throw new InvalidOperationException("Les deux collectivités doivent être différentes.");
        }

        // Chargement des deux collectivités (tracked, pour calcul en mémoire)
        var collectiviteA = await db.Collectivites.FindAsync([demande.CollectiviteAId], ct);
        if (collectiviteA is null)
        {
            throw new InvalidOperationException("La première collectivité n'existe pas.");
        }

        var collectiviteB = await db.Collectivites.FindAsync([demande.CollectiviteBId], ct);
        if (collectiviteB is null)
        {
            throw new InvalidOperationException("La deuxième collectivité n'existe pas.");
        }

        // Calcul de la zone de conflit = intersection des contours (PostGIS ST_Intersection)
        var zoneConflit = collectiviteA.Contour.Intersection(collectiviteB.Contour);

        if (zoneConflit.IsEmpty)
        {
            throw new InvalidOperationException("Ces deux collectivités n'ont aucune zone de chevauchement.");
        }

        zoneConflit.SRID = 4326;

        var litige = new Litige
        {
            Id = Guid.NewGuid(),
            Description = demande.Description,
            Statut = StatutLitige.Signale,
            DateCreation = DateTime.UtcNow,
            Geometrie = demande.Geometrie,
            ZoneConflit = zoneConflit,
            CollectiviteAId = demande.CollectiviteAId,
            CollectiviteBId = demande.CollectiviteBId
        };

        db.Litiges.Add(litige);

        // Traçabilité (chapitre 5) : création du litige
        db.Historiques.Add(new Historique
        {
            Id = Guid.NewGuid(),
            Entite = "Litige",
            EntiteId = litige.Id,
            Action = "création",
            Auteur = auteur,
            Date = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);

        return VersReponse(litige, collectiviteA.Nom, collectiviteB.Nom);
    }

    public async Task<List<LitigeResponse>> ListerAsync(Guid? collectiviteId = null, StatutLitige? statut = null, CancellationToken ct = default)
    {
        var query = db.Litiges
            .AsNoTracking()
            .Include(l => l.CollectiviteA)
            .Include(l => l.CollectiviteB)
            .AsQueryable();

        if (collectiviteId.HasValue)
        {
            query = query.Where(l => l.CollectiviteAId == collectiviteId.Value || l.CollectiviteBId == collectiviteId.Value);
        }

        if (statut.HasValue)
        {
            query = query.Where(l => l.Statut == statut.Value);
        }

        return await query
            .OrderByDescending(l => l.DateCreation)
            .Select(l => VersReponse(l, l.CollectiviteA.Nom, l.CollectiviteB.Nom))
            .ToListAsync(ct);
    }

    public async Task<LitigeResponse?> ObtenirParIdAsync(Guid id, CancellationToken ct = default)
    {
        var litige = await db.Litiges
            .AsNoTracking()
            .Include(l => l.CollectiviteA)
            .Include(l => l.CollectiviteB)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        return litige is null ? null : VersReponse(litige, litige.CollectiviteA!.Nom, litige.CollectiviteB!.Nom);
    }

    public async Task<LitigeResponse?> ChangerStatutAsync(Guid id, StatutLitige statut, string auteur, CancellationToken ct = default)
    {
        var litige = await db.Litiges.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (litige is null)
        {
            return null;
        }

        var ancienStatut = litige.Statut;
        litige.Statut = statut;

        db.Historiques.Add(new Historique
        {
            Id = Guid.NewGuid(),
            Entite = "Litige",
            EntiteId = litige.Id,
            Action = $"changement de statut : {ancienStatut} → {statut}",
            Auteur = auteur,
            Date = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);

        var collectiviteA = await db.Collectivites.FindAsync([litige.CollectiviteAId], ct);
        var collectiviteB = await db.Collectivites.FindAsync([litige.CollectiviteBId], ct);
        return VersReponse(litige, collectiviteA?.Nom, collectiviteB?.Nom);
    }

    public async Task<DetectionResultatResponse> DetecterAsync(CancellationToken ct = default)
    {
        // 1. Charger toutes les collectivités (contours inclus)
        var collectivites = await db.Collectivites.AsNoTracking().ToListAsync(ct);

        // 2. Paires existantes : ne pas recréer un litige déjà signalé
        var pairesExistantes = (await db.Litiges.AsNoTracking().ToListAsync(ct))
            .Select(l => ClePaire(l.CollectiviteAId, l.CollectiviteBId))
            .ToHashSet();

        var resultat = new DetectionResultatResponse();

        // 3. Comparaison pairwise (O(n²) — raisonnable pour ~300 communes pilotes)
        for (var i = 0; i < collectivites.Count; i++)
        {
            for (var j = i + 1; j < collectivites.Count; j++)
            {
                var a = collectivites[i];
                var b = collectivites[j];

                if (!a.Contour.Intersects(b.Contour))
                {
                    continue;
                }

                var cle = ClePaire(a.Id, b.Id);
                if (pairesExistantes.Contains(cle))
                {
                    continue;
                }

                // Calcul de la zone de conflit = intersection géométrique
                var intersection = a.Contour.Intersection(b.Contour);
                intersection.SRID = 4326;

                var litige = new Litige
                {
                    Id = Guid.NewGuid(),
                    Description = $"Chevauchement détecté automatiquement entre « {a.Nom} » et « {b.Nom} »",
                    Statut = StatutLitige.Signale,
                    DateCreation = DateTime.UtcNow,
                    ZoneConflit = intersection,
                    // Géométrie = centroïde de la zone de conflit (point répresentatif)
                    Geometrie = intersection.Centroid,
                    CollectiviteAId = a.Id,
                    CollectiviteBId = b.Id
                };

                db.Litiges.Add(litige);

                db.Historiques.Add(new Historique
                {
                    Id = Guid.NewGuid(),
                    Entite = "Litige",
                    EntiteId = litige.Id,
                    Action = "création (détection automatique)",
                    Auteur = "Système",
                    Date = DateTime.UtcNow
                });

                resultat.Litiges.Add(VersReponse(litige, a.Nom, b.Nom));
            }
        }

        await db.SaveChangesAsync(ct);
        resultat.Detectes = resultat.Litiges.Count;
        return resultat;
    }

    private static string ClePaire(Guid a, Guid b) => a.CompareTo(b) < 0 ? $"{a}|{b}" : $"{b}|{a}";

    private static LitigeResponse VersReponse(Litige l, string nomA, string nomB) => new()
    {
        Id = l.Id,
        Description = l.Description,
        Statut = l.Statut.ToString(),
        DateCreation = l.DateCreation,
        CollectiviteAId = l.CollectiviteAId,
        CollectiviteANom = nomA,
        CollectiviteBId = l.CollectiviteBId,
        CollectiviteBNom = nomB,
        ZoneConflit = l.ZoneConflit,
        Geometrie = l.Geometrie
    };
}
