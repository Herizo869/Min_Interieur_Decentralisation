using Collectivites.Api.Models.Dtos;

namespace Collectivites.Api.Services;

/// <summary>Service de tableau de bord — synthèses chiffrées de tous les modules (UC-15).</summary>
public interface IDashboardService
{
    /// <summary>Récupère toutes les statistiques agrégées pour le tableau de bord.</summary>
    Task<TableauDeBordResponse> ObtenirStatistiquesAsync(CancellationToken ct = default);
}
