using System.Security.Claims;
using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Collectivites.Api.Controllers;

/// <summary>Authentification des utilisateurs (UC-01).</summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>S'authentifier et obtenir un jeton JWT (UC-01).</summary>
    /// <param name="demande">Identifiant et mot de passe.</param>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest demande, CancellationToken ct)
    {
        try
        {
            var reponse = await authService.AuthentifierAsync(demande, ct);
            return Ok(reponse);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Identifiant ou mot de passe invalide." });
        }
    }

    /// <summary>Utilisateur courant (vérification du jeton).</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new
    {
        UtilisateurId = User.FindFirstValue(ClaimTypes.NameIdentifier),
        Identifiant = User.FindFirstValue("identifiant"),
        Nom = User.FindFirstValue(ClaimTypes.Name),
        Role = User.FindFirstValue(ClaimTypes.Role)
    });

    /// <summary>Modifier son propre profil (nom, mot de passe).</summary>
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> ModifierProfil([FromBody] ModifierProfilRequest demande, CancellationToken ct)
    {
        var utilisateurId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var resultat = await authService.ModifierProfilAsync(utilisateurId, demande, ct);
            return Ok(resultat);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Demander la réinitialisation du mot de passe (UC-13) — étape 1.</summary>
    /// <param name="demande">Identifiant de l'utilisateur.</param>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] DemanderReinitialisationRequest demande, CancellationToken ct)
    {
        try
        {
            var reponse = await authService.DemanderReinitialisationAsync(demande, ct);
            return Ok(reponse);
        }
        catch (InvalidOperationException)
        {
            // Même message que l'identifiant inconnu — pas d'énumération de comptes
            return Ok(new ReinitialiserMotDePasseResponse
            {
                Message = "Si cet identifiant existe, un token de réinitialisation a été généré."
            });
        }
    }

    /// <summary>Réinitialiser le mot de passe avec le token (UC-13) — étape 2.</summary>
    /// <param name="demande">Identifiant, token et nouveau mot de passe.</param>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ReinitialiserMotDePasseRequest demande, CancellationToken ct)
    {
        try
        {
            await authService.ReinitialiserMotDePasseAsync(demande, ct);
            return Ok(new { message = "Mot de passe réinitialisé avec succès. Vous pouvez vous reconnecter." });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Token invalide ou expiré." });
        }
    }
}
