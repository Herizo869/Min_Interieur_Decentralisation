using System.Security.Claims;
using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Models.Enums;
using Collectivites.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Collectivites.Api.Controllers;

/// <summary>Doléances citoyennes (UC-11, UC-12).</summary>
[ApiController]
[Route("api/[controller]")]
public class DoleancesController(IDoleanceService service) : ControllerBase
{
    /// <summary>Déposer une doléance géolocalisée (UC-11, public).</summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Deposer([FromBody] DeposerDoleanceRequest demande, CancellationToken ct)
    {
        try
        {
            var doleance = await service.DeposerAsync(demande, ct);
            return CreatedAtAction(nameof(Obtenir), new { id = doleance.Id }, doleance);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Suivi citoyen d'une doléance par numéro de dossier (UC-12, public).</summary>
    [HttpGet("suivi/{numeroSuivi}")]
    [AllowAnonymous]
    public async Task<IActionResult> Suivre(string numeroSuivi, CancellationToken ct)
    {
        var doleance = await service.SuivreParNumeroAsync(numeroSuivi.Trim(), ct);
        return doleance is null ? NotFound() : Ok(doleance);
    }

    /// <summary>Lister les doléances (UC-12) — agents et administrateurs authentifiés.</summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Lister([FromQuery] Guid? collectiviteId, [FromQuery] StatutDoleance? statut, [FromQuery] CategorieDoleance? categorie, CancellationToken ct)
        => Ok(await service.ListerAsync(collectiviteId, statut, categorie, ct));

    /// <summary>Fiche d'une doléance (UC-12) — agents et administrateurs authentifiés.</summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Obtenir(Guid id, CancellationToken ct)
    {
        var doleance = await service.ObtenirParIdAsync(id, ct);
        return doleance is null ? NotFound() : Ok(doleance);
    }

    /// <summary>Changer le statut d'une doléance (traitement, UC-12) — agents et administrateurs.</summary>
    [HttpPut("{id:guid}/statut")]
    [Authorize]
    public async Task<IActionResult> ChangerStatut(Guid id, [FromBody] ModifierStatutDoleanceRequest demande, CancellationToken ct)
    {
        var auteur = User.FindFirstValue(ClaimTypes.Name) ?? "inconnu";
        var doleance = await service.ChangerStatutAsync(id, demande.Statut, auteur, ct);
        return doleance is null ? NotFound() : Ok(doleance);
    }
}
