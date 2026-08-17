using Collectivites.Api.Data;
using Collectivites.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Collectivites.Api.Services;

/// <summary>Implémentation du référentiel des collectivités (module 3.2).</summary>
public class CollectiviteService(AppDbContext db) : ICollectiviteService
{
    public async Task<List<Collectivite>> RechercherAsync(string? recherche, string? type, CancellationToken ct = default)
    {
        var query = db.Collectivites.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var terme = recherche.Trim();
            query = query.Where(c =>
                EF.Functions.ILike(c.Nom, $"%{terme}%") ||
                c.CodeAdministratif.StartsWith(terme));
        }

        // Filtre par sous-type (commune / département / région / epci)
        if (!string.IsNullOrWhiteSpace(type))
        {
            var typeNormalise = type.Trim().ToLowerInvariant();
            query = typeNormalise switch
            {
                "commune" => query.OfType<Commune>(),
                "departement" or "département" => query.OfType<Departement>(),
                "region" or "région" => query.OfType<Region>(),
                "epci" => query.OfType<Epci>(),
                _ => query
            };
        }

        return await query
            .OrderBy(c => c.Nom)
            .Take(100)
            .ToListAsync(ct);
    }

    public async Task<Collectivite?> ObtenirParIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Collectivites
            .AsNoTracking()
            .Include(c => c.ProjetsDotations)
            .Include(c => c.Indicateurs)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }
}
