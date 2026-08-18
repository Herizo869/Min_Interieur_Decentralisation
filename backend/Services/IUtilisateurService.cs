using Collectivites.Api.Models.Dtos;

namespace Collectivites.Api.Services;

/// <summary>Services de gestion des utilisateurs (UC-02, réservé à l'administrateur).</summary>
public interface IUtilisateurService
{
    /// <summary>Liste tous les comptes utilisateurs.</summary>
    Task<List<UtilisateurResponse>> ListerAsync(CancellationToken ct = default);

    /// <summary>Fiche d'un utilisateur (null si introuvable).</summary>
    Task<UtilisateurResponse?> ObtenirParIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Crée un compte (mot de passe haché bcrypt, périmètre d'accès).</summary>
    /// <exception cref="InvalidOperationException">Identifiant déjà utilisé ou mot de passe manquant.</exception>
    Task<UtilisateurResponse> CreerAsync(CreerUtilisateurRequest demande, CancellationToken ct = default);

    /// <summary>Modifie un compte (nom, rôle, actif, mot de passe, périmètre d'accès).</summary>
    /// <exception cref="InvalidOperationException">Identifiant déjà utilisé.</exception>
    Task<UtilisateurResponse?> ModifierAsync(Guid id, ModifierUtilisateurRequest demande, CancellationToken ct = default);

    /// <summary>Désactive un compte (suppression logique).</summary>
    /// <exception cref="InvalidOperationException">Tentative de désactivation de son propre compte.</exception>
    Task<bool> DesactiverAsync(Guid id, Guid utilisateurCourantId, CancellationToken ct = default);
}
