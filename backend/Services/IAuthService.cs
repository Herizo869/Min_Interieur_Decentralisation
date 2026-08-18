using Collectivites.Api.Models.Dtos;

namespace Collectivites.Api.Services;

/// <summary>Services d'authentification de la plateforme (UC-01, UC-13).</summary>
public interface IAuthService
{
    /// <summary>
    /// Authentifie un utilisateur (vérification bcrypt) et émet un jeton JWT.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Identifiant ou mot de passe invalide.</exception>
    Task<LoginResponse> AuthentifierAsync(LoginRequest demande, CancellationToken ct = default);

    /// <summary>
    /// Demande de réinitialisation de mot de passe — génère un token à usage unique (UC-13).
    /// En dev, le token est retourné dans la réponse ; en prod il serait envoyé par email.
    /// </summary>
    /// <exception cref="InvalidOperationException">Identifiant inconnu.</exception>
    Task<ReinitialiserMotDePasseResponse> DemanderReinitialisationAsync(
        DemanderReinitialisationRequest demande, CancellationToken ct = default);

    /// <summary>
    /// Réinitialise le mot de passe avec le token reçu (UC-13).
    /// Le token est invalide une fois utilisé ou expiré.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Token invalide ou expiré.</exception>
    Task ReinitialiserMotDePasseAsync(
        ReinitialiserMotDePasseRequest demande, CancellationToken ct = default);
}
