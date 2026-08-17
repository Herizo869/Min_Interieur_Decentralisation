using Collectivites.Api.Models.Dtos;

namespace Collectivites.Api.Services;

/// <summary>Services métier d'import du référentiel des collectivités (module 3.2, UC-05).</summary>
public interface ICollectiviteImportService
{
    /// <summary>
    /// Importe un fichier GeoJSON (FeatureCollection) : validation de la structure et des
    /// géométries, puis création ou mise à jour des collectivités (clé : CodeAdministratif).
    /// </summary>
    /// <param name="flux">Contenu du fichier GeoJSON.</param>
    /// <param name="type">Sous-type importé : commune, departement, region, epci.</param>
    /// <exception cref="FormatException">Fichier invalide (structure, type manquant, aucune entité).</exception>
    Task<ImportReferentielResultat> ImporterAsync(Stream flux, string? type, CancellationToken ct = default);
}
