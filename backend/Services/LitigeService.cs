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
