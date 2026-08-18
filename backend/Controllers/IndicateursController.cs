using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Collectivites.Api.Controllers;

/// <summary>Gestion des indicateurs chiffrés des collectivités (UC-07) — agents et administrateurs authentifiés.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IndicateursController(IIndicateurService service) : ControllerBase
{
    /// <summary>Lister les indicateurs (filtres optionnels : collectivité, type) (UC-07).</summary>
    [HttpGet]
    public async Task<IActionResult> Lister([FromQuery] Guid? collectiviteId, [FromQuery] string? type, CancellationToken ct)
        => Ok(await service.ListerAsync(collectiviteId, type, ct));

    /// <summary>Fiche d'un indicateur (UC-07).</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtenir(Guid id, CancellationToken ct)
    {
        var indicateur = await service.ObtenirParIdAsync(id, ct);
        return indicateur is null ? NotFound() : Ok(indicateur);
    }

    /// <summary>Créer un indicateur rattaché à une collectivité (UC-07).</summary>
    [HttpPost]
    public async Task<IActionResult> Creer([FromBody] IndicateurRequest demande, CancellationToken ct)
    {
        try
        {
            var indicateur = await service.CreerAsync(demande, ct);
            return CreatedAtAction(nameof(Obtenir), new { id = indicateur.Id }, indicateur);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Modifier un indicateur (UC-07).</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Modifier(Guid id, [FromBody] IndicateurRequest demande, CancellationToken ct)
    {
        try
        {
            var indicateur = await service.ModifierAsync(id, demande, ct);
            return indicateur is null ? NotFound() : Ok(indicateur);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Supprimer un indicateur (UC-07).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Supprimer(Guid id, CancellationToken ct)
    {
        var supprime = await service.SupprimerAsync(id, ct);
        return supprime ? NoContent() : NotFound();
    }
}
