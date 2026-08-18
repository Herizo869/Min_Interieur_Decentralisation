using Collectivites.Api.Data;
using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Models.Entities;
using Collectivites.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Collectivites.Api.Services;

/// <summary>Implémentation des doléances citoyennes (UC-11, UC-12).</summary>
public class DoleanceService(AppDbContext db) : IDoleanceService
{
    public async Task<DoleanceResponse> DeposerAsync(DeposerDoleanceRequest demande, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(demande.Description))
        {
            throw new InvalidOperationException("La description du problème est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(demande.Auteur))
        {
            throw new InvalidOperationException("Le nom de l'auteur est obligatoire.");
        }

        if (demande.Point is not Point point)
        {
            throw new InvalidOperationException("La localisation doit être un point (GeoJSON Point).");
        }

        // WGS84 obligatoire pour l'intersection avec les contours (SRID 4326)
        point.SRID = 4326;

        // Rattachement automatique à la collectivité dont le contour contient le point (UC-11)
        var collectivite = await db.Collectivites
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Contour.Contains(point), ct);

        if (collectivite is null)
        {
            throw new InvalidOperationException("Le point signalé ne se trouve sur aucune collectivité connue.");
        }

        var doleance = new Doleance
        {
            Id = Guid.NewGuid(),
            Description = demande.Description,
            Categorie = demande.Categorie,
            Auteur = demande.Auteur,
            Statut = StatutDoleance.Nouveau,
            DateCreation = DateTime.UtcNow,
            Geometrie = point,
            NumeroSuivi = await GenererNumeroSuiviAsync(ct),
            CollectiviteRattacheeId = collectivite.Id
        };

        db.Doleances.Add(doleance);

        // Traçabilité (chapitre 5) : création de la doléance
        db.Historiques.Add(new Historique
        {
            Id = Guid.NewGuid(),
            Entite = "Doleance",
            EntiteId = doleance.Id,
            Action = "création",
            Auteur = demande.Auteur,
            Date = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);

        doleance.CollectiviteRattachee = collectivite;
        return VersReponse(doleance);
    }

    public async Task<DoleanceResponse?> SuivreParNumeroAsync(string numeroSuivi, CancellationToken ct = default)
    {
        var doleance = await db.Doleances
            .AsNoTracking()
            .Include(d => d.CollectiviteRattachee)
            .FirstOrDefaultAsync(d => d.NumeroSuivi == numeroSuivi, ct);

        return doleance is null ? null : VersReponse(doleance);
    }

    public async Task<List<DoleanceResponse>> ListerAsync(Guid? collectiviteId = null, StatutDoleance? statut = null, CategorieDoleance? categorie = null, CancellationToken ct = default)
    {
        var query = db.Doleances.AsNoTracking().Include(d => d.CollectiviteRattachee).AsQueryable();

        if (collectiviteId.HasValue)
        {
            query = query.Where(d => d.CollectiviteRattacheeId == collectiviteId.Value);
        }

        if (statut.HasValue)
        {
            query = query.Where(d => d.Statut == statut.Value);
        }

        if (categorie.HasValue)
        {
            query = query.Where(d => d.Categorie == categorie.Value);
        }

        return await query
            .OrderByDescending(d => d.DateCreation)
            .Select(d => VersReponse(d))
            .ToListAsync(ct);
    }

    public async Task<DoleanceResponse?> ObtenirParIdAsync(Guid id, CancellationToken ct = default)
    {
        var doleance = await db.Doleances
            .AsNoTracking()
            .Include(d => d.CollectiviteRattachee)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

        return doleance is null ? null : VersReponse(doleance);
    }

    public async Task<DoleanceResponse?> ChangerStatutAsync(Guid id, StatutDoleance statut, string auteur, CancellationToken ct = default)
    {
        var doleance = await db.Doleances.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (doleance is null)
        {
            return null;
        }

        var ancienStatut = doleance.Statut;
        doleance.Statut = statut;

        // Traçabilité (chapitre 5) : tout changement de statut est conservé
        db.Historiques.Add(new Historique
        {
            Id = Guid.NewGuid(),
            Entite = "Doleance",
            EntiteId = doleance.Id,
            Action = $"changement de statut : {ancienStatut} → {statut}",
            Auteur = auteur,
            Date = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);

        doleance.CollectiviteRattachee = await db.Collectivites.FindAsync([doleance.CollectiviteRattacheeId], ct);
        return VersReponse(doleance);
    }

    private async Task<string> GenererNumeroSuiviAsync(CancellationToken ct)
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // sans O, I, 0, 1

        for (var tentative = 0; tentative < 3; tentative++)
        {
            var suffixe = string.Concat(Enumerable.Range(0, 6).Select(_ => alphabet[Random.Shared.Next(alphabet.Length)]));
            var numero = $"DOL-{DateTime.UtcNow:yyyy}-{suffixe}";

            if (!await db.Doleances.AnyAsync(d => d.NumeroSuivi == numero, ct))
            {
                return numero;
            }
        }

        throw new InvalidOperationException("Impossible de générer un numéro de suivi unique.");
    }

    private static DoleanceResponse VersReponse(Doleance d) => new()
    {
        Id = d.Id,
        Description = d.Description,
        Categorie = d.Categorie.ToString(),
        Statut = d.Statut.ToString(),
        Auteur = d.Auteur,
        NumeroSuivi = d.NumeroSuivi,
        DateCreation = d.DateCreation,
        CollectiviteRattacheeId = d.CollectiviteRattacheeId,
        CollectiviteRattacheeNom = d.CollectiviteRattachee?.Nom ?? string.Empty,
        Geometrie = d.Geometrie
    };
}
