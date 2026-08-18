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
}
