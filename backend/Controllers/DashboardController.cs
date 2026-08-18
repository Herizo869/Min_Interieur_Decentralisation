using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Collectivites.Api.Controllers;

/// <summary>Tableau de bord — synthèses chiffrées de tous les modules (UC-15).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    /// <summary>
    /// Obtenir les statistiques du tableau de bord.
    /// Données agrégées : collectivités, projets, indicateurs, litiges, doléances, utilisateurs.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Obtenir(CancellationToken ct)
    {
        var stats = await dashboardService.ObtenirStatistiquesAsync(ct);
        return Ok(stats);
    }
}
