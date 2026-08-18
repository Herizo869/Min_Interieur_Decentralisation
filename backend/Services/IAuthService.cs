using Collectivites.Api.Models.Dtos;

namespace Collectivites.Api.Services;

/// <summary>Services d'authentification de la plateforme (UC-01).</summary>
public interface IAuthService
{
    /// <summary>
    /// Authentifie un utilisateur (vérification bcrypt) et émet un jeton JWT.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Identifiant ou mot de passe invalide.</exception>
    Task<LoginResponse> AuthentifierAsync(LoginRequest demande, CancellationToken ct = default);
}
