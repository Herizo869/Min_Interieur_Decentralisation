using Collectivites.Api.Models.Enums;

namespace Collectivites.Api.Models.Entities;

/// <summary>Agent ou administrateur de la plateforme (chapitre 2 et UC-02).</summary>
public class Utilisateur
{
    public Guid Id { get; set; }

    public string Nom { get; set; } = string.Empty;

    public Role Role { get; set; }

    /// <summary>Identifiant de connexion.</summary>
    public string Identifiant { get; set; } = string.Empty;

    /// <summary>Mot de passe hashé (bcrypt / Argon2).</summary>
    public string MotDePasseHash { get; set; } = string.Empty;

    /// <summary>Compte actif ou désactivé (UC-02 : désactivation des comptes).</summary>
    public bool Actif { get; set; } = true;

    /// <summary>Token de réinitialisation de mot de passe à usage unique (UC-13).</summary>
    public string? TokenReinitialisation { get; set; }

    /// <summary>Expiration du token de réinitialisation (UC-13).</summary>
    public DateTime? TokenExpiration { get; set; }

    /// <summary>Périmètre d'accès géographique de l'utilisateur (association *-* avec Collectivité).</summary>
    public ICollection<Collectivite> CollectivitesAcces { get; set; } = new List<Collectivite>();
}
