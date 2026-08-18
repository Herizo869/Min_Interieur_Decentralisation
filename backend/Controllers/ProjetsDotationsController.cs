using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Collectivites.Api.Controllers;

/// <summary>Gestion des projets financés et dotations (UC-06) — agents et administrateurs authentifiés.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjetsDotationsController(IProjetDotationService service) : ControllerBase
{
    /// <summary>Lister les projets/dotations (filtre optionnel par collectivité) (UC-06).</summary>
    [HttpGet]
    public async Task<IActionResult> Lister([FromQuery] Guid? collectiviteId, CancellationToken ct)
        => Ok(await service.ListerAsync(collectiviteId, ct));

    /// <summary>Fiche d'un projet/dotation (UC-06).</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtenir(Guid id, CancellationToken ct)
    {
        var projet = await service.ObtenirParIdAsync(id, ct);
        return projet is null ? NotFound() : Ok(projet);
    }

    /// <summary>Créer un projet/dotation rattaché à une collectivité (UC-06).</summary>
    [HttpPost]
    public async Task<IActionResult> Creer([FromBody] ProjetDotationRequest demande, CancellationToken ct)
    {
        try
        {
            var projet = await service.CreerAsync(demande, ct);
            return CreatedAtAction(nameof(Obtenir), new { id = projet.Id }, projet);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Modifier un projet/dotation (UC-06).</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Modifier(Guid id, [FromBody] ProjetDotationRequest demande, CancellationToken ct)
    {
        try
        {
            var projet = await service.ModifierAsync(id, demande, ct);
            return projet is null ? NotFound() : Ok(projet);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Supprimer un projet/dotation (UC-06).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Supprimer(Guid id, CancellationToken ct)
    {
        var supprime = await service.SupprimerAsync(id, ct);
        return supprime ? NoContent() : NotFound();
    }
}
