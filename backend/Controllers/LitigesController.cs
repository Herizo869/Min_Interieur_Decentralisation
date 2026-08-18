using System.Security.Claims;
using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Models.Enums;
using Collectivites.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Collectivites.Api.Controllers;

/// <summary>Litiges de limites territoriales (UC-10, UC-14) — agents et administrateurs authentifiés.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LitigesController(ILitigeService service) : ControllerBase
{
    /// <summary>Signaler un litige à partir d'un constat terrain (UC-14).</summary>
    [HttpPost]
    public async Task<IActionResult> Signaler([FromBody] SignalerLitigeRequest demande, CancellationToken ct)
    {
        var auteur = User.FindFirstValue(ClaimTypes.Name) ?? "inconnu";
        try
        {
            var litige = await service.SignalerAsync(demande, auteur, ct);
            return CreatedAtAction(nameof(Obtenir), new { id = litige.Id }, litige);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Lister les litiges (filtres optionnels : collectivité, statut) (UC-10).</summary>
    [HttpGet]
    public async Task<IActionResult> Lister([FromQuery] Guid? collectiviteId, [FromQuery] StatutLitige? statut, CancellationToken ct)
        => Ok(await service.ListerAsync(collectiviteId, statut, ct));

    /// <summary>Fiche d'un litige (UC-10).</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtenir(Guid id, CancellationToken ct)
    {
        var litige = await service.ObtenirParIdAsync(id, ct);
        return litige is null ? NotFound() : Ok(litige);
    }

    /// <summary>Changer le statut d'un litige (UC-10).</summary>
    [HttpPut("{id:guid}/statut")]
    public async Task<IActionResult> ChangerStatut(Guid id, [FromBody] ModifierStatutLitigeRequest demande, CancellationToken ct)
    {
        var auteur = User.FindFirstValue(ClaimTypes.Name) ?? "inconnu";
        var litige = await service.ChangerStatutAsync(id, demande.Statut, auteur, ct);
        return litige is null ? NotFound() : Ok(litige);
    }
}
