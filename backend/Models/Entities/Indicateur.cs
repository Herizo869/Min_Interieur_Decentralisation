namespace Collectivites.Api.Models.Entities;

/// <summary>Valeur chiffrée rattachée à une collectivité à une date donnée (UC-07).</summary>
public class Indicateur
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public decimal Valeur { get; set; }

    /// <summary>Unité de mesure de la valeur (ex. habitants, €, %) — chapitre 4.</summary>
    public string Unite { get; set; } = string.Empty;

    /// <summary>Provenance de la valeur (ex. INSTAT, préfecture) — chapitre 4.</summary>
    public string Source { get; set; } = string.Empty;

    public DateTime DateReleve { get; set; }

    // Navigation vers la collectivité concernée
    public Guid CollectiviteId { get; set; }
    public Collectivite Collectivite { get; set; } = null!;
}
