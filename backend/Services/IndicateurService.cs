using Collectivites.Api.Data;
using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Collectivites.Api.Services;

/// <summary>Implémentation des indicateurs chiffrés (UC-07).</summary>
public class IndicateurService(AppDbContext db) : IIndicateurService
{
    public async Task<List<IndicateurResponse>> ListerAsync(Guid? collectiviteId = null, string? type = null, CancellationToken ct = default)
    {
        var query = db.Indicateurs.AsNoTracking().Include(i => i.Collectivite).AsQueryable();

        if (collectiviteId.HasValue)
        {
            query = query.Where(i => i.CollectiviteId == collectiviteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(i => i.Type == type.Trim());
        }

        return await query
            .OrderByDescending(i => i.DateReleve)
            .Select(i => VersReponse(i))
            .ToListAsync(ct);
    }

    public async Task<IndicateurResponse?> ObtenirParIdAsync(Guid id, CancellationToken ct = default)
    {
        var indicateur = await db.Indicateurs
            .AsNoTracking()
            .Include(i => i.Collectivite)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        return indicateur is null ? null : VersReponse(indicateur);
    }

    public async Task<IndicateurResponse> CreerAsync(IndicateurRequest demande, CancellationToken ct = default)
    {
        Valider(demande);

        if (!await db.Collectivites.AnyAsync(c => c.Id == demande.CollectiviteId, ct))
        {
            throw new InvalidOperationException("La collectivité concernée n'existe pas.");
        }

        var indicateur = new Indicateur
        {
            Id = Guid.NewGuid(),
            Type = demande.Type,
            Valeur = demande.Valeur,
            Unite = demande.Unite,
            Source = demande.Source,
            DateReleve = demande.DateReleve,
            CollectiviteId = demande.CollectiviteId
        };

        db.Indicateurs.Add(indicateur);
        await db.SaveChangesAsync(ct);

        indicateur.Collectivite = (await db.Collectivites.FindAsync([demande.CollectiviteId], ct))!;
        return VersReponse(indicateur);
    }

    public async Task<IndicateurResponse?> ModifierAsync(Guid id, IndicateurRequest demande, CancellationToken ct = default)
    {
        Valider(demande);

        var indicateur = await db.Indicateurs.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (indicateur is null)
        {
            return null;
        }

        if (!await db.Collectivites.AnyAsync(c => c.Id == demande.CollectiviteId, ct))
        {
            throw new InvalidOperationException("La collectivité concernée n'existe pas.");
        }

        indicateur.Type = demande.Type;
        indicateur.Valeur = demande.Valeur;
        indicateur.Unite = demande.Unite;
        indicateur.Source = demande.Source;
        indicateur.DateReleve = demande.DateReleve;
        indicateur.CollectiviteId = demande.CollectiviteId;

        await db.SaveChangesAsync(ct);

        indicateur.Collectivite = (await db.Collectivites.FindAsync([demande.CollectiviteId], ct))!;
        return VersReponse(indicateur);
    }

    public async Task<bool> SupprimerAsync(Guid id, CancellationToken ct = default)
    {
        var indicateur = await db.Indicateurs.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (indicateur is null)
        {
            return false;
        }

        db.Indicateurs.Remove(indicateur);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static void Valider(IndicateurRequest demande)
    {
        if (string.IsNullOrWhiteSpace(demande.Type))
        {
            throw new InvalidOperationException("Le type d'indicateur est obligatoire (ex. population, budget).");
        }

        if (string.IsNullOrWhiteSpace(demande.Unite))
        {
            throw new InvalidOperationException("L'unité de mesure est obligatoire (ex. habitants, €, %).");
        }

        if (string.IsNullOrWhiteSpace(demande.Source))
        {
            throw new InvalidOperationException("La source de la valeur est obligatoire (ex. INSTAT, préfecture).");
        }
    }

    private static IndicateurResponse VersReponse(Indicateur i) => new()
    {
        Id = i.Id,
        Type = i.Type,
        Valeur = i.Valeur,
        Unite = i.Unite,
        Source = i.Source,
        DateReleve = i.DateReleve,
        CollectiviteId = i.CollectiviteId,
        CollectiviteNom = i.Collectivite?.Nom ?? string.Empty
    };
}
