using Collectivites.Api.Data;
using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Collectivites.Api.Services;

/// <summary>Implémentation de la gestion des utilisateurs (UC-02).</summary>
public class UtilisateurService(AppDbContext db) : IUtilisateurService
{
    public async Task<List<UtilisateurResponse>> ListerAsync(CancellationToken ct = default)
    {
        return await db.Utilisateurs
            .AsNoTracking()
            .Include(u => u.CollectivitesAcces)
            .OrderBy(u => u.Nom)
            .Select(u => VersReponse(u))
            .ToListAsync(ct);
    }

    public async Task<UtilisateurResponse?> ObtenirParIdAsync(Guid id, CancellationToken ct = default)
    {
        var utilisateur = await db.Utilisateurs
            .AsNoTracking()
            .Include(u => u.CollectivitesAcces)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        return utilisateur is null ? null : VersReponse(utilisateur);
    }

    public async Task<UtilisateurResponse> CreerAsync(CreerUtilisateurRequest demande, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(demande.Identifiant) || string.IsNullOrWhiteSpace(demande.MotDePasse))
        {
            throw new InvalidOperationException("Identifiant et mot de passe sont obligatoires.");
        }

        if (await db.Utilisateurs.AnyAsync(u => u.Identifiant == demande.Identifiant, ct))
        {
            throw new InvalidOperationException($"L'identifiant « {demande.Identifiant} » est déjà utilisé.");
        }

        var utilisateur = new Utilisateur
        {
            Id = Guid.NewGuid(),
            Nom = demande.Nom,
            Identifiant = demande.Identifiant,
            Role = demande.Role,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(demande.MotDePasse),
            CollectivitesAcces = await ChargerPerimetreAsync(demande.CollectiviteIds, ct)
        };

        db.Utilisateurs.Add(utilisateur);
        await db.SaveChangesAsync(ct);

        return VersReponse(utilisateur);
    }

    public async Task<UtilisateurResponse?> ModifierAsync(Guid id, ModifierUtilisateurRequest demande, CancellationToken ct = default)
    {
        var utilisateur = await db.Utilisateurs
            .Include(u => u.CollectivitesAcces)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (utilisateur is null)
        {
            return null;
        }

        utilisateur.Nom = demande.Nom;
        utilisateur.Role = demande.Role;
        utilisateur.Actif = demande.Actif;

        if (!string.IsNullOrWhiteSpace(demande.MotDePasse))
        {
            utilisateur.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(demande.MotDePasse);
        }

        // Périmètre d'accès remplacé intégralement
        utilisateur.CollectivitesAcces.Clear();
        utilisateur.CollectivitesAcces = await ChargerPerimetreAsync(demande.CollectiviteIds, ct);

        await db.SaveChangesAsync(ct);
        return VersReponse(utilisateur);
    }

    public async Task<bool> DesactiverAsync(Guid id, Guid utilisateurCourantId, CancellationToken ct = default)
    {
        if (id == utilisateurCourantId)
        {
            throw new InvalidOperationException("Impossible de désactiver votre propre compte.");
        }

        var utilisateur = await db.Utilisateurs.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (utilisateur is null)
        {
            return false;
        }

        utilisateur.Actif = false;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private async Task<List<Collectivite>> ChargerPerimetreAsync(List<Guid> collectiviteIds, CancellationToken ct)
    {
        if (collectiviteIds.Count == 0)
        {
            return new List<Collectivite>();
        }

        return await db.Collectivites
            .Where(c => collectiviteIds.Contains(c.Id))
            .ToListAsync(ct);
    }

    private static UtilisateurResponse VersReponse(Utilisateur u) => new()
    {
        Id = u.Id,
        Nom = u.Nom,
        Identifiant = u.Identifiant,
        Role = u.Role.ToString(),
        Actif = u.Actif,
        CollectiviteIds = u.CollectivitesAcces.Select(c => c.Id).ToList()
    };
}
