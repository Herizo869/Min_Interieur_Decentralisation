using Collectivites.Api.Models.Enums;

namespace Collectivites.Api.Models.Dtos;

/// <summary>Création ou modification d'un projet financé / dotation (UC-06).</summary>
public class ProjetDotationRequest
{
    public string Intitule { get; set; } = string.Empty;

    public decimal Montant { get; set; }

    /// <summary>Devise du montant (ex. MGA, EUR).</summary>
    public string Devise { get; set; } = string.Empty;

    public StatutProjet Statut { get; set; }

    public DateTime DateDebut { get; set; }

    public DateTime? DateFin { get; set; }

    /// <summary>Collectivité bénéficiaire.</summary>
    public Guid CollectiviteId { get; set; }
}

/// <summary>Projet financé / dotation renvoyé par l'API (UC-06).</summary>
public class ProjetDotationResponse
{
    public Guid Id { get; set; }

    public string Intitule { get; set; } = string.Empty;

    public decimal Montant { get; set; }

    public string Devise { get; set; } = string.Empty;

    public string Statut { get; set; } = string.Empty;

    public DateTime DateDebut { get; set; }

    public DateTime? DateFin { get; set; }

    public Guid CollectiviteId { get; set; }

    public string CollectiviteNom { get; set; } = string.Empty;
}
