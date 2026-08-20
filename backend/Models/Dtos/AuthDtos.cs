namespace Collectivites.Api.Models.Dtos;

/// <summary>Requête de connexion (UC-01).</summary>
public class LoginRequest
{
    public string Identifiant { get; set; } = string.Empty;

    public string MotDePasse { get; set; } = string.Empty;
}

/// <summary>Modification du profil par l'utilisateur courant.</summary>
public class ModifierProfilRequest
{
    public string Nom { get; set; } = string.Empty;

    /// <summary>Nouveau mot de passe (vide ou null = inchangé).</summary>
    public string? MotDePasse { get; set; }

    /// <summary>Confirmation du nouveau mot de passe (requis si MotDePasse renseigné).</summary>
    public string? ConfirmationMotDePasse { get; set; }
}

/// <summary>Réponse de connexion (UC-01) : jeton JWT + informations utilisateur.</summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }

    public Guid UtilisateurId { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string Identifiant { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}
