namespace Collectivites.Api.Models.Entities;

/// <summary>
/// Historique (audit) des modifications d'un litige ou d'une doléance.
/// Répond à l'exigence de traçabilité du chapitre 5 (auteur, date, action).
/// </summary>
public class Historique
{
    public Guid Id { get; set; }

    /// <summary>Entité concernée (ex. "Litige", "Doleance").</summary>
    public string Entite { get; set; } = string.Empty;

    /// <summary>Identifiant de l'entité concernée.</summary>
    public Guid EntiteId { get; set; }

    /// <summary>Action réalisée (ex. "création", "changement de statut").</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Auteur de la modification.</summary>
    public string Auteur { get; set; } = string.Empty;

    public DateTime Date { get; set; }
}
