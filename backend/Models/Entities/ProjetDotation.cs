using Collectivites.Api.Models.Enums;

namespace Collectivites.Api.Models.Entities;

/// <summary>Projet financé ou dotation attribuée à une collectivité (UC-06).</summary>
public class ProjetDotation
{
    public Guid Id { get; set; }

    public string Intitule { get; set; } = string.Empty;

    public decimal Montant { get; set; }

    /// <summary>Devise du montant (ex. MGA, EUR) — chapitre 4.</summary>
    public string Devise { get; set; } = string.Empty;

    public StatutProjet Statut { get; set; }

    public DateTime DateDebut { get; set; }

    public DateTime? DateFin { get; set; }

    // Navigation vers la collectivité bénéficiaire
    public Guid CollectiviteId { get; set; }
    public Collectivite Collectivite { get; set; } = null!;
}
