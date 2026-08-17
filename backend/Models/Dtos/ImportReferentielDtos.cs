using System.Text.Json;
using NetTopologySuite.Geometries;

namespace Collectivites.Api.Models.Dtos;

/// <summary>Racine d'un fichier GeoJSON de type FeatureCollection (UC-05).</summary>
public class GeoJsonFeatureCollection
{
    public string Type { get; set; } = string.Empty;

    public List<GeoJsonFeature> Features { get; set; } = new();
}

/// <summary>Entité GeoJSON : géométrie + propriétés attributaires.</summary>
public class GeoJsonFeature
{
    public string Type { get; set; } = string.Empty;

    public Geometry? Geometry { get; set; }

    public Dictionary<string, JsonElement>? Properties { get; set; }
}

/// <summary>Rapport d'import du référentiel (UC-05).</summary>
public class ImportReferentielResultat
{
    /// <summary>Nombre de collectivités créées.</summary>
    public int Importees { get; set; }

    /// <summary>Nombre de collectivités existantes mises à jour.</summary>
    public int MisesAJour { get; set; }

    /// <summary>Nombre de lignes en erreur (l'import est alors rejeté, scénario alternatif UC-05).</summary>
    public int Erreurs => DetailsErreurs.Count;

    public List<ErreurImport> DetailsErreurs { get; set; } = new();
}

/// <summary>Erreur relevée sur une ligne du fichier GeoJSON.</summary>
public class ErreurImport
{
    /// <summary>Numéro de ligne (1-based) dans le fichier.</summary>
    public int Ligne { get; set; }

    public string Raison { get; set; } = string.Empty;
}
