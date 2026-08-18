using System.ComponentModel.DataAnnotations;

namespace Collectivites.Api.Models.Dtos;

/// <summary>Demande de réinitialisation de mot de passe — étape 1 : envoyer un token (UC-13).</summary>
public class DemanderReinitialisationRequest
{
    /// <summary>Identifiant de l'utilisateur qui souhaite réinitialiser son mot de passe.</summary>
    [Required(ErrorMessage = "L'identifiant est requis.")]
    public string Identifiant { get; set; } = string.Empty;
}

/// <summary>Réinitialisation du mot de passe — étape 2 : utiliser le token (UC-13).</summary>
public class ReinitialiserMotDePasseRequest
{
    /// <summary>Identifiant de l'utilisateur.</summary>
    [Required(ErrorMessage = "L'identifiant est requis.")]
    public string Identifiant { get; set; } = string.Empty;

    /// <summary>Token à usage unique reçu par email (ou à usage interne en dev).</summary>
    [Required(ErrorMessage = "Le token est requis.")]
    public string Token { get; set; } = string.Empty;

    /// <summary>Nouveau mot de passe (min. 8 caractères).</summary>
    [Required(ErrorMessage = "Le nouveau mot de passe est requis.")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
    public string NouveauMotDePasse { get; set; } = string.Empty;
}

/// <summary>Réponse à une demande de réinitialisation.</summary>
public class ReinitialiserMotDePasseResponse
{
    /// <summary>Message de confirmation.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Token généré (renvoyé en dev uniquement ; en prod il serait envoyé par email).
    /// </summary>
    public string? Token { get; set; }
}
