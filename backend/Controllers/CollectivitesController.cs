using Collectivites.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Collectivites.Api.Controllers;

/// <summary>API du référentiel des collectivités (UC-03, UC-04).</summary>
[ApiController]
[Route("api/[controller]")]
public class CollectivitesController(ICollectiviteService service) : ControllerBase
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
}
