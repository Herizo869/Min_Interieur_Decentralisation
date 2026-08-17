using System.Text.Json;
using Collectivites.Api.Data;
using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

namespace Collectivites.Api.Services;

/// <summary>Implémentation de l'import du référentiel des collectivités (UC-05).</summary>
public class CollectiviteImportService(AppDbContext db) : ICollectiviteImportService
{
    public async Task<ImportReferentielResultat> ImporterAsync(Stream flux, string? type, CancellationToken ct = default)
    {
        var collection = LireFichier(flux, ct);

        if (!string.Equals(collection.Type, "FeatureCollection", StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException("Le fichier doit être un GeoJSON de type « FeatureCollection ».");
        }

        var typeImport = NormaliserType(type);
        if (typeImport is null)
        {
            throw new FormatException("Le paramètre « type » est requis : commune, departement, region ou epci.");
        }

        if (collection.Features.Count == 0)
        {
            throw new FormatException("Le fichier ne contient aucune entité.");
        }

        var resultat = new ImportReferentielResultat();
        var lignesValides = new List<(GeoJsonFeature Feature, string Code)>();

        // 1ʳᵉ passe : validation de chaque entité (structure + géométrie)
        foreach (var (feature, ligne) in collection.Features.Select((f, i) => (f, i + 1)))
        {
            var code = ExtrairePropriete(feature, "code", "codeadministratif", "codeinsee", "code_insee", "pcode", "admcode", "codcommune", "codecommune");
            if (string.IsNullOrWhiteSpace(code))
            {
                resultat.DetailsErreurs.Add(new ErreurImport { Ligne = ligne, Raison = "Propriété « code » manquante ou vide." });
                continue;
            }

            var nom = ExtrairePropriete(feature, "nom", "name", "libelle", "ncc");
            if (string.IsNullOrWhiteSpace(nom))
            {
                resultat.DetailsErreurs.Add(new ErreurImport { Ligne = ligne, Raison = "Propriété « nom » manquante ou vide." });
                continue;
            }

            if (feature.Geometry is null)
            {
                resultat.DetailsErreurs.Add(new ErreurImport { Ligne = ligne, Raison = "Géométrie manquante." });
                continue;
            }

            if (!feature.Geometry.IsValid)
            {
                resultat.DetailsErreurs.Add(new ErreurImport { Ligne = ligne, Raison = "Géométrie invalide." });
                continue;
            }

            if (feature.Geometry is not Polygon and not MultiPolygon)
            {
                resultat.DetailsErreurs.Add(new ErreurImport
                {
                    Ligne = ligne,
                    Raison = $"Géométrie non polygonale ({feature.Geometry.GeometryType}) : un contour de collectivité doit être un polygone."
                });
                continue;
            }

            lignesValides.Add((feature, code.Trim()));
        }

        // Scénario alternatif : erreurs détectées → rejet de l'import avec la liste des lignes en erreur
        if (resultat.DetailsErreurs.Count > 0)
        {
            return resultat;
        }

        // 2ᵉ passe : création ou mise à jour (clé métier : CodeAdministratif)
        foreach (var (feature, code) in lignesValides)
        {
            var existant = await db.Collectivites.FirstOrDefaultAsync(c => c.CodeAdministratif == code, ct);
            if (existant is null)
            {
                db.Collectivites.Add(CreerCollectivite(feature, code, typeImport.Value));
                resultat.Importees++;
            }
            else
            {
                MettreAJour(existant, feature);
                resultat.MisesAJour++;
            }
        }

        await db.SaveChangesAsync(ct);
        return resultat;
    }

    private static GeoJsonFeatureCollection LireFichier(Stream flux, CancellationToken ct)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new GeoJsonConverterFactory());

        try
        {
            return JsonSerializer.DeserializeAsync<GeoJsonFeatureCollection>(flux, options, ct)
                       .GetAwaiter()
                       .GetResult()
                   ?? throw new FormatException("Le fichier GeoJSON est vide.");
        }
        catch (JsonException ex)
        {
            throw new FormatException($"Fichier GeoJSON invalide : {ex.Message}", ex);
        }
    }

    private static Collectivite CreerCollectivite(GeoJsonFeature feature, string code, TypeCollectiviteImport type)
    {
        var population = ExtrairePopulation(feature) ?? 0;

        Collectivite collectivite = type switch
        {
            TypeCollectiviteImport.Commune => new Commune
            {
                CodePostal = ExtrairePropriete(feature, "codepostal", "code_postal", "postal") ?? string.Empty
            },
            TypeCollectiviteImport.Departement => new Departement
            {
                Prefecture = ExtrairePropriete(feature, "prefecture", "cheflieu") ?? string.Empty
            },
            TypeCollectiviteImport.Region => new Region
            {
                ChefLieu = ExtrairePropriete(feature, "cheflieu", "chef_lieu") ?? string.Empty
            },
            TypeCollectiviteImport.Epci => new Epci
            {
                Siren = ExtrairePropriete(feature, "siren", "nif") ?? string.Empty,
                Nature = ExtrairePropriete(feature, "nature") ?? string.Empty
            },
            _ => throw new InvalidOperationException($"Type de collectivité inconnu : {type}.")
        };

        collectivite.CodeAdministratif = code;
        collectivite.Nom = ExtrairePropriete(feature, "nom", "name", "libelle", "ncc") ?? string.Empty;
        collectivite.Population = population;
        collectivite.Contour = NormaliserGeometrie(feature.Geometry!);
        return collectivite;
    }

    private static void MettreAJour(Collectivite existant, GeoJsonFeature feature)
    {
        existant.Nom = ExtrairePropriete(feature, "nom", "name", "libelle", "ncc") ?? existant.Nom;
        existant.Population = ExtrairePopulation(feature) ?? existant.Population;
        existant.Contour = NormaliserGeometrie(feature.Geometry!);

        // Champs spécifiques selon le sous-type existant
        switch (existant)
        {
            case Commune commune:
                commune.CodePostal = ExtrairePropriete(feature, "codepostal", "code_postal", "postal") ?? commune.CodePostal;
                break;
            case Departement departement:
                departement.Prefecture = ExtrairePropriete(feature, "prefecture", "cheflieu") ?? departement.Prefecture;
                break;
            case Region region:
                region.ChefLieu = ExtrairePropriete(feature, "cheflieu", "chef_lieu") ?? region.ChefLieu;
                break;
            case Epci epci:
                epci.Siren = ExtrairePropriete(feature, "siren", "nif") ?? epci.Siren;
                epci.Nature = ExtrairePropriete(feature, "nature") ?? epci.Nature;
                break;
        }
    }

    /// <summary>Force le SRID 4326 (WGS84, RFC 7946) et normalise un polygone en multipolygone (colonne PostGIS).</summary>
    private static Geometry NormaliserGeometrie(Geometry geometrie)
    {
        geometrie.SRID = 4326;
        return geometrie is Polygon polygone
            ? new GeometryFactory().CreateMultiPolygon(new[] { polygone })
            : geometrie;
    }

    private static TypeCollectiviteImport? NormaliserType(string? type)
    {
        return type?.Trim().ToLowerInvariant() switch
        {
            "commune" => TypeCollectiviteImport.Commune,
            "departement" or "département" => TypeCollectiviteImport.Departement,
            "region" or "région" => TypeCollectiviteImport.Region,
            "epci" => TypeCollectiviteImport.Epci,
            _ => null
        };
    }

    private static string? ExtrairePropriete(GeoJsonFeature feature, params string[] cles)
    {
        if (feature.Properties is null)
        {
            return null;
        }

        foreach (var cle in cles)
        {
            foreach (var (cleFichier, valeur) in feature.Properties)
            {
                if (!string.Equals(cleFichier, cle, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (valeur.ValueKind == JsonValueKind.Null)
                {
                    return null;
                }

                return valeur.ValueKind == JsonValueKind.String
                    ? valeur.GetString()
                    : valeur.ToString();
            }
        }

        return null;
    }

    private static int? ExtrairePopulation(GeoJsonFeature feature)
    {
        var valeur = ExtrairePropriete(feature, "population", "pop", "habitants");
        return int.TryParse(valeur, out var nombre) ? nombre : null;
    }

    private enum TypeCollectiviteImport
    {
        Commune,
        Departement,
        Region,
        Epci
    }
}
