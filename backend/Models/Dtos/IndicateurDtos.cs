namespace Collectivites.Api.Models.Dtos;

/// <summary>Création ou modification d'un indicateur chiffré (UC-07).</summary>
public class IndicateurRequest
{
    /// <summary>Type d'indicateur (ex. population, budget, taux).</summary>
    public string Type { get; set; } = string.Empty;

    public decimal Valeur { get; set; }

    /// <summary>Unité de mesure (ex. habitants, €, %).</summary>
    public string Unite { get; set; } = string.Empty;

    /// <summary>Provenance de la valeur (ex. INSTAT, préfecture).</summary>
    public string Source { get; set; } = string.Empty;

    public DateTime DateReleve { get; set; }

    /// <summary>Collectivité concernée.</summary>
    public Guid CollectiviteId { get; set; }
}

/// <summary>Indicateur renvoyé par l'API (UC-07).</summary>
public class IndicateurResponse
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public decimal Valeur { get; set; }

    public string Unite { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public DateTime DateReleve { get; set; }

    public Guid CollectiviteId { get; set; }

    public string CollectiviteNom { get; set; } = string.Empty;
}
