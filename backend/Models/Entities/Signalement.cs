using Collectivites.Api.Models.Enums;
using NetTopologySuite.Geometries;

namespace Collectivites.Api.Models.Entities;

/// <summary>
/// Classe abstraite de base des signalements (chapitre 4).
/// Hiérarchie par héritage : Litige de limites, Doléance citoyenne.
/// Le statut est spécialisé par sous-type (StatutLitige / StatutDoléance).
/// </summary>
public abstract class Signalement
{
    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime DateCreation { get; set; }

    /// <summary>Géométrie du signalement (point de la doléance, zone du litige…).</summary>
    public Geometry Geometrie { get; set; } = null!;
}

/// <summary>Litige de limites territoriales entre deux collectivités (UC-09, UC-10, UC-14).</summary>
public class Litige : Signalement
{
    public StatutLitige Statut { get; set; }

    /// <summary>Zone de conflit calculée (intersection des géométries).</summary>
    public Geometry ZoneConflit { get; set; } = null!;

    // Les deux collectivités en litige
    public Guid CollectiviteAId { get; set; }
    public Collectivite CollectiviteA { get; set; } = null!;

    public Guid CollectiviteBId { get; set; }
    public Collectivite CollectiviteB { get; set; } = null!;
}

/// <summary>Doléance citoyenne géolocalisée (UC-11, UC-12).</summary>
public class Doleance : Signalement
{
    public CategorieDoleance Categorie { get; set; }

    public StatutDoleance Statut { get; set; }

    /// <summary>Auteur de la doléance (dénomination simple, pas de données sensibles).</summary>
    public string Auteur { get; set; } = string.Empty;

    /// <summary>Numéro de suivi communiqué au citoyen (UC-11).</summary>
    public string NumeroSuivi { get; set; } = string.Empty;

    // Collectivité rattachée automatiquement (ST_Contains)
    public Guid CollectiviteRattacheeId { get; set; }
    public Collectivite CollectiviteRattachee { get; set; } = null!;
}
