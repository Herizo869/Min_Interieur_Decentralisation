using Collectivites.Api.Data;
using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Models.Entities;
using Collectivites.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Collectivites.Api.Services;

/// <summary>
/// Implémentation du tableau de bord (UC-15) :
/// requêtes agrégées sur toutes les tables pour produire des synthèses chiffrées.
/// Utilise les DbSet spécifiques (Communes, Departements…) au lieu du GroupBy TPH
/// qui ne peut pas être traduit en SQL.
/// </summary>
public class DashboardService(AppDbContext db) : IDashboardService
{
    public async Task<TableauDeBordResponse> ObtenirStatistiquesAsync(CancellationToken ct = default)
    {
        // ── Collectivités (TPH : requêtes par DbSet spécifique) ──
        var nbCommunes = await db.Communes.AsNoTracking().CountAsync(ct);
        var nbDepartements = await db.Departements.AsNoTracking().CountAsync(ct);
        var nbRegions = await db.Regions.AsNoTracking().CountAsync(ct);
        var nbEpcis = await db.Epcis.AsNoTracking().CountAsync(ct);

        var collectiviteStats = new CollectiviteStats
        {
            Total = nbCommunes + nbDepartements + nbRegions + nbEpcis,
            ParType = new Dictionary<string, int>
            {
                { "Commune", nbCommunes },
                { "Departement", nbDepartements },
                { "Region", nbRegions },
                { "Epci", nbEpcis }
            }
        };

        // ── Projets & Dotations ──
        var projets = await db.ProjetsDotations.AsNoTracking().ToListAsync(ct);

        var projetStats = new ProjetStats
        {
            Total = projets.Count,
            ParStatut = projets
                .GroupBy(p => p.Statut.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            MontantTotal = projets.Count > 0 ? projets.Sum(p => p.Montant) : 0,
            MontantMoyen = projets.Count > 0 ? projets.Average(p => p.Montant) : 0
        };

        // ── Indicateurs ──
        var indicateurs = await db.Indicateurs.AsNoTracking().ToListAsync(ct);
        var collectivitesCouvertes = indicateurs.Select(i => i.CollectiviteId).Distinct().Count();

        var indicateurStats = new IndicateurStats
        {
            Total = indicateurs.Count,
            ParType = indicateurs
                .GroupBy(i => i.Type)
                .ToDictionary(g => g.Key, g => g.Count()),
            CollectivitesCouvertes = collectivitesCouvertes
        };

        // ── Litiges (TPH : requête sur le DbSet spécifique) ──
        var litiges = await db.Litiges.AsNoTracking().ToListAsync(ct);

        var litigeStats = new LitigeStats
        {
            Total = litiges.Count,
            ParStatut = litiges
                .GroupBy(l => l.Statut.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            Ouverts = litiges.Count(l =>
                l.Statut == StatutLitige.Signale || l.Statut == StatutLitige.EnInstruction)
        };

        // ── Doléances (TPH : requête sur le DbSet spécifique) ──
        var doleances = await db.Doleances.AsNoTracking().ToListAsync(ct);

        var doleanceStats = new DoleanceStats
        {
            Total = doleances.Count,
            ParStatut = doleances
                .GroupBy(d => d.Statut.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            ParCategorie = doleances
                .GroupBy(d => d.Categorie.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            EnAttente = doleances.Count(d => d.Statut == StatutDoleance.Nouveau)
        };

        // ── Utilisateurs ──
        var utilisateurs = await db.Utilisateurs.AsNoTracking().ToListAsync(ct);

        var utilisateurStats = new UtilisateurStats
        {
            Total = utilisateurs.Count,
            ParRole = utilisateurs
                .GroupBy(u => u.Role.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            Actifs = utilisateurs.Count(u => u.Actif)
        };

        return new TableauDeBordResponse
        {
            Collectivites = collectiviteStats,
            Projets = projetStats,
            Indicateurs = indicateurStats,
            Litiges = litigeStats,
            Doléances = doleanceStats,
            Utilisateurs = utilisateurStats
        };
    }
}
