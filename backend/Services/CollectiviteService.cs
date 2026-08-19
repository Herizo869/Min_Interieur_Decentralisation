using Collectivites.Api.Data;
using Collectivites.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Features;

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
        query = AppliquerFiltreType(query, type);

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

    public async Task<FeatureCollection> RechercherGeoJsonAsync(string? type, CancellationToken ct = default)
    {
        var query = db.Collectivites.AsNoTracking();
        query = AppliquerFiltreType(query, type);

        var collectivites = await query.OrderBy(c => c.Nom).ToListAsync(ct);

        var collection = new FeatureCollection();
        foreach (var c in collectivites)
        {
            if (c.Contour is null) continue;

            var props = new AttributesTable
            {
                { "id", c.Id.ToString() },
                { "nom", c.Nom },
                { "codeAdministratif", c.CodeAdministratif },
                { "population", c.Population },
                { "type", c.GetType().Name }
            };

            collection.Add(new Feature(c.Contour, props));
        }

        return collection;
    }

    /// <summary>Applique le filtre TPH par sous-type de collectivité.</summary>
    private static IQueryable<Collectivite> AppliquerFiltreType(IQueryable<Collectivite> query, string? type)
    {
        if (string.IsNullOrWhiteSpace(type)) return query;

        var typeNormalise = type.Trim().ToLowerInvariant();
        return typeNormalise switch
        {
            "commune" => query.OfType<Commune>(),
            "departement" or "département" => query.OfType<Departement>(),
            "region" or "région" => query.OfType<Region>(),
            "epci" => query.OfType<Epci>(),
            _ => query
        };
    }
}
