using Collectivites.Api.Models.Enums;

namespace Collectivites.Api.Models.Dtos;

/// <summary>Création d'un compte utilisateur (UC-02).</summary>
public class CreerUtilisateurRequest
{
    public string Nom { get; set; } = string.Empty;

    public string Identifiant { get; set; } = string.Empty;

    public string MotDePasse { get; set; } = string.Empty;

    public Role Role { get; set; }

    /// <summary>Périmètre d'accès géographique (identifiants des collectivités autorisées).</summary>
    public List<Guid> CollectiviteIds { get; set; } = new();
}

/// <summary>Modification d'un compte utilisateur (UC-02).</summary>
public class ModifierUtilisateurRequest
{
    public string Nom { get; set; } = string.Empty;

    public Role Role { get; set; }

    public bool Actif { get; set; } = true;

    /// <summary>Nouveau mot de passe (vide ou null = inchangé).</summary>
    public string? MotDePasse { get; set; }

    /// <summary>Périmètre d'accès géographique (liste complète remplaçante).</summary>
    public List<Guid> CollectiviteIds { get; set; } = new();
}

/// <summary>Utilisateur renvoyé par l'API (UC-02) — jamais le hash du mot de passe.</summary>
public class UtilisateurResponse
{
    public Guid Id { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string Identifiant { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool Actif { get; set; }

    public List<Guid> CollectiviteIds { get; set; } = new();
}
