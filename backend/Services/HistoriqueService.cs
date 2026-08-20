using Collectivites.Api.Data;
using Collectivites.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Collectivites.Api.Services;

/// <summary>Consultation de l'historique / audit (UC-16).</summary>
public class HistoriqueService(AppDbContext db) : IHistoriqueService
{
    public async Task<List<Historique>> ListerAsync(
        string? entite = null,
        Guid? entiteId = null,
        DateTime? dateDebut = null,
        DateTime? dateFin = null,
        int skip = 0,
        int take = 100,
        CancellationToken ct = default)
    {
        var query = db.Historiques.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entite))
            query = query.Where(h => h.Entite == entite);

        if (entiteId.HasValue)
            query = query.Where(h => h.EntiteId == entiteId.Value);

        if (dateDebut.HasValue)
            query = query.Where(h => h.Date >= dateDebut.Value);

        if (dateFin.HasValue)
            query = query.Where(h => h.Date <= dateFin.Value);

        return await query
            .OrderByDescending(h => h.Date)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }
}
