using System.Security.Claims;
using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Collectivites.Api.Controllers;

/// <summary>Gestion des comptes utilisateurs (UC-02) — réservé à l'administrateur.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrateur")]
public class UtilisateursController(IUtilisateurService service) : ControllerBase
{
    /// <summary>Lister tous les comptes utilisateurs (UC-02).</summary>
    [HttpGet]
    public async Task<IActionResult> Lister(CancellationToken ct)
        => Ok(await service.ListerAsync(ct));

    /// <summary>Fiche d'un utilisateur (UC-02).</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtenir(Guid id, CancellationToken ct)
    {
        var utilisateur = await service.ObtenirParIdAsync(id, ct);
        return utilisateur is null ? NotFound() : Ok(utilisateur);
    }

    /// <summary>Créer un compte utilisateur (UC-02).</summary>
    [HttpPost]
    public async Task<IActionResult> Creer([FromBody] CreerUtilisateurRequest demande, CancellationToken ct)
    {
        try
        {
            var utilisateur = await service.CreerAsync(demande, ct);
            return CreatedAtAction(nameof(Obtenir), new { id = utilisateur.Id }, utilisateur);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Modifier un compte (nom, rôle, actif, mot de passe, périmètre d'accès) (UC-02).</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Modifier(Guid id, [FromBody] ModifierUtilisateurRequest demande, CancellationToken ct)
    {
        try
        {
            var utilisateur = await service.ModifierAsync(id, demande, ct);
            return utilisateur is null ? NotFound() : Ok(utilisateur);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Désactiver un compte (suppression logique) (UC-02).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Desactiver(Guid id, CancellationToken ct)
    {
        var utilisateurCourantId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var desactive = await service.DesactiverAsync(id, utilisateurCourantId, ct);
            return desactive ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
