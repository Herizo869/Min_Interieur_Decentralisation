using Collectivites.Api.Data;
using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Collectivites.Api.Services;

/// <summary>Implémentation des projets financés / dotations (UC-06).</summary>
public class ProjetDotationService(AppDbContext db) : IProjetDotationService
{
    public async Task<List<ProjetDotationResponse>> ListerAsync(Guid? collectiviteId = null, CancellationToken ct = default)
    {
        var query = db.ProjetsDotations.AsNoTracking().Include(p => p.Collectivite).AsQueryable();

        if (collectiviteId.HasValue)
        {
            query = query.Where(p => p.CollectiviteId == collectiviteId.Value);
        }

        return await query
            .OrderByDescending(p => p.DateDebut)
            .Select(p => VersReponse(p))
            .ToListAsync(ct);
    }

    public async Task<ProjetDotationResponse?> ObtenirParIdAsync(Guid id, CancellationToken ct = default)
    {
        var projet = await db.ProjetsDotations
            .AsNoTracking()
            .Include(p => p.Collectivite)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return projet is null ? null : VersReponse(projet);
    }

    public async Task<ProjetDotationResponse> CreerAsync(ProjetDotationRequest demande, CancellationToken ct = default)
    {
        Valider(demande);

        if (!await db.Collectivites.AnyAsync(c => c.Id == demande.CollectiviteId, ct))
        {
            throw new InvalidOperationException("La collectivité bénéficiaire n'existe pas.");
        }

        var projet = new ProjetDotation
        {
            Id = Guid.NewGuid(),
            Intitule = demande.Intitule,
            Montant = demande.Montant,
            Devise = demande.Devise,
            Statut = demande.Statut,
            DateDebut = demande.DateDebut,
            DateFin = demande.DateFin,
            CollectiviteId = demande.CollectiviteId
        };

        db.ProjetsDotations.Add(projet);
        await db.SaveChangesAsync(ct);

        projet.Collectivite = (await db.Collectivites.FindAsync([demande.CollectiviteId], ct))!;
        return VersReponse(projet);
    }

    public async Task<ProjetDotationResponse?> ModifierAsync(Guid id, ProjetDotationRequest demande, CancellationToken ct = default)
    {
        Valider(demande);

        var projet = await db.ProjetsDotations.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (projet is null)
        {
            return null;
        }

        if (!await db.Collectivites.AnyAsync(c => c.Id == demande.CollectiviteId, ct))
        {
            throw new InvalidOperationException("La collectivité bénéficiaire n'existe pas.");
        }

        projet.Intitule = demande.Intitule;
        projet.Montant = demande.Montant;
        projet.Devise = demande.Devise;
        projet.Statut = demande.Statut;
        projet.DateDebut = demande.DateDebut;
        projet.DateFin = demande.DateFin;
        projet.CollectiviteId = demande.CollectiviteId;

        await db.SaveChangesAsync(ct);

        projet.Collectivite = (await db.Collectivites.FindAsync([demande.CollectiviteId], ct))!;
        return VersReponse(projet);
    }

    public async Task<bool> SupprimerAsync(Guid id, CancellationToken ct = default)
    {
        var projet = await db.ProjetsDotations.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (projet is null)
        {
            return false;
        }

        db.ProjetsDotations.Remove(projet);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static void Valider(ProjetDotationRequest demande)
    {
        if (string.IsNullOrWhiteSpace(demande.Intitule))
        {
            throw new InvalidOperationException("L'intitulé du projet est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(demande.Devise))
        {
            throw new InvalidOperationException("La devise est obligatoire (ex. MGA, EUR).");
        }

        if (demande.Montant < 0)
        {
            throw new InvalidOperationException("Le montant ne peut pas être négatif.");
        }

        if (demande.DateFin.HasValue && demande.DateFin < demande.DateDebut)
        {
            throw new InvalidOperationException("La date de fin doit être postérieure à la date de début.");
        }
    }

    private static ProjetDotationResponse VersReponse(ProjetDotation p) => new()
    {
        Id = p.Id,
        Intitule = p.Intitule,
        Montant = p.Montant,
        Devise = p.Devise,
        Statut = p.Statut.ToString(),
        DateDebut = p.DateDebut,
        DateFin = p.DateFin,
        CollectiviteId = p.CollectiviteId,
        CollectiviteNom = p.Collectivite?.Nom ?? string.Empty
    };
}
