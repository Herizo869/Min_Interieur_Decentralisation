using Collectivites.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Collectivites.Api.Controllers;

/// <summary>Historique / audit des modifications (UC-16).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HistoriquesController(IHistoriqueService service) : ControllerBase
{
    /// <summary>Lister les événements d'audit avec filtres optionnels.</summary>
    [HttpGet]
    public async Task<IActionResult> Lister(
        [FromQuery] string? entite,
        [FromQuery] Guid? entiteId,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken ct = default)
    {
        var resultats = await service.ListerAsync(entite, entiteId, dateDebut, dateFin, skip, take, ct);
        return Ok(resultats);
    }
}
