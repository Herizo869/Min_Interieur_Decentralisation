using NetTopologySuite.Geometries;

namespace Collectivites.Api.Models.Entities;

/// <summary>
/// Classe abstraite de base des collectivités territoriales (chapitre 4).
/// Hiérarchie par héritage : Commune, Département, Région, EPCI.
/// </summary>
public abstract class Collectivite
{
    public Guid Id { get; set; }

    public string CodeInsee { get; set; } = string.Empty;

    public string Nom { get; set; } = string.Empty;

    public int Population { get; set; }

    /// <summary>Contour (géométrie PostGIS) de la collectivité.</summary>
    public Geometry Contour { get; set; } = null!;

    // Navigations
    public ICollection<ProjetDotation> ProjetsDotations { get; set; } = new List<ProjetDotation>();
    public ICollection<Indicateur> Indicateurs { get; set; } = new List<Indicateur>();
    public ICollection<Utilisateur> UtilisateursAcces { get; set; } = new List<Utilisateur>();
}

/// <summary>Commune (sous-type de Collectivité).</summary>
public class Commune : Collectivite
{
    public string CodePostal { get; set; } = string.Empty;
}

/// <summary>Département (sous-type de Collectivité).</summary>
public class Departement : Collectivite
{
    public string Prefecture { get; set; } = string.Empty;
}

/// <summary>Région (sous-type de Collectivité).</summary>
public class Region : Collectivite
{
    public string ChefLieu { get; set; } = string.Empty;
}

/// <summary>Établissement public de coopération intercommunale (sous-type de Collectivité).</summary>
public class Epci : Collectivite
{
    public string Siren { get; set; } = string.Empty;

    public string Nature { get; set; } = string.Empty;
}
