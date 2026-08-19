using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Collectivites.Api.Controllers;

/// <summary>API du référentiel des collectivités (UC-03, UC-04, UC-05).</summary>
[ApiController]
[Route("api/[controller]")]
public class CollectivitesController(ICollectiviteService service, ICollectiviteImportService importService) : ControllerBase
{
    /// <summary>Rechercher des collectivités par nom ou code administratif (UC-04).</summary>
    /// <param name="recherche">Nom ou début de code administratif.</param>
    /// <param name="type">Sous-type : commune, departement, region, epci.</param>
    [HttpGet]
    public async Task<IActionResult> Rechercher([FromQuery] string? recherche, [FromQuery] string? type, CancellationToken ct)
    {
        var resultats = await service.RechercherAsync(recherche, type, ct);
        return Ok(resultats.Select(c => new
        {
            c.Id,
            c.Nom,
            c.CodeAdministratif,
            c.Population,
            Type = c.GetType().Name
        }));
    }

    /// <summary>Rechercher les collectivités au format GeoJSON FeatureCollection (carte interactive).</summary>
    /// <param name="type">Filtre optionnel par sous-type.</param>
    [HttpGet("geojson")]
    public async Task<IActionResult> RechercherGeoJson([FromQuery] string? type, CancellationToken ct)
    {
        var collection = await service.RechercherGeoJsonAsync(type, ct);
        return Ok(collection);
    }

    /// <summary>Fiche détaillée d'une collectivité (UC-03/UC-04).</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenirParId(Guid id, CancellationToken ct)
    {
        var collectivite = await service.ObtenirParIdAsync(id, ct);
        if (collectivite is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            collectivite.Id,
            collectivite.Nom,
            collectivite.CodeAdministratif,
            collectivite.Population,
            Type = collectivite.GetType().Name,
            Projets = collectivite.ProjetsDotations.Select(p => new { p.Intitule, p.Montant, p.Devise, p.Statut }),
            Indicateurs = collectivite.Indicateurs.Select(i => new { i.Type, i.Valeur, i.Unite, i.DateReleve })
        });
    }

    /// <summary>Importer le référentiel des collectivités depuis un fichier GeoJSON (UC-05).</summary>
    /// <param name="fichier">Fichier GeoJSON de type FeatureCollection.</param>
    /// <param name="type">Sous-type importé : commune, departement, region, epci.</param>
    [HttpPost("import")]
    [RequestSizeLimit(104_857_600)] // 100 Mo max (département pilote voire pays entier)
    public async Task<IActionResult> Importer(IFormFile fichier, [FromQuery] string? type, CancellationToken ct)
    {
        if (fichier is null || fichier.Length == 0)
        {
            return BadRequest(new { message = "Aucun fichier fourni." });
        }

        var extension = Path.GetExtension(fichier.FileName).ToLowerInvariant();
        if (extension is not (".geojson" or ".json"))
        {
            return BadRequest(new { message = "Format non supporté : fichier GeoJSON attendu (.geojson ou .json)." });
        }

        ImportReferentielResultat resultat;
        try
        {
            await using var flux = fichier.OpenReadStream();
            resultat = await importService.ImporterAsync(flux, type, ct);
        }
        catch (FormatException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        // Scénario alternatif UC-05 : rejet de l'import avec la liste des lignes en erreur
        if (resultat.DetailsErreurs.Count > 0)
        {
            return BadRequest(resultat);
        }

        return Ok(resultat);
    }
}
