using System.Text;
using Collectivites.Api.Models.Enums;
using Collectivites.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Collectivites.Api.Controllers;

/// <summary>Exports de données en CSV (UC-08) — agents et administrateurs authentifiés.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportsController(
    IDoleanceService doleanceService,
    ILitigeService litigeService,
    IProjetDotationService projetService,
    IIndicateurService indicateurService) : ControllerBase
{
    /// <summary>Exporter les données en CSV (UC-08).</summary>
    /// <param name="resource">Ressource : doleances, litiges, projets, indicateurs.</param>
    /// <param name="collectiviteId">Filtre optionnel par collectivité.</param>
    /// <param name="statutDoleance">Filtre optionnel par statut (doléances).</param>
    /// <param name="statutLitige">Filtre optionnel par statut (litiges).</param>
    [HttpGet]
    public async Task<IActionResult> Exporter(
        [FromQuery] string resource,
        [FromQuery] Guid? collectiviteId,
        [FromQuery] StatutDoleance? statutDoleance,
        [FromQuery] StatutLitige? statutLitige,
        CancellationToken ct)
    {
        return resource?.ToLowerInvariant() switch
        {
            "doleances" => await ExporterDoleances(collectiviteId, statutDoleance, ct),
            "litiges" => await ExporterLitiges(collectiviteId, statutLitige, ct),
            "projets" => await ExporterProjets(collectiviteId, ct),
            "indicateurs" => await ExporterIndicateurs(collectiviteId, ct),
            _ => BadRequest(new { message = "Ressource invalide. Valeurs acceptées : doleances, litiges, projets, indicateurs." })
        };
    }

    private async Task<IActionResult> ExporterDoleances(Guid? collectiviteId, StatutDoleance? statut, CancellationToken ct)
    {
        var donnees = await doleanceService.ListerAsync(collectiviteId, statut, ct: ct);
        var entetes = new[] { "Id", "Description", "Categorie", "Statut", "Auteur", "NumeroSuivi", "DateCreation", "Collectivite" };
        var lignes = donnees.Select(d => new[]
        {
            d.Id.ToString(),
            d.Description,
            d.Categorie,
            d.Statut,
            d.Auteur,
            d.NumeroSuivi,
            d.DateCreation.ToString("yyyy-MM-dd HH:mm"),
            d.CollectiviteRattacheeNom
        });
        return RetournerCsv(entetes, lignes, $"doleances_{DateTime.UtcNow:yyyy-MM-dd}");
    }

    private async Task<IActionResult> ExporterLitiges(Guid? collectiviteId, StatutLitige? statut, CancellationToken ct)
    {
        var donnees = await litigeService.ListerAsync(collectiviteId, statut, ct);
        var entetes = new[] { "Id", "Description", "Statut", "DateCreation", "CollectiviteA", "CollectiviteB" };
        var lignes = donnees.Select(l => new[]
        {
            l.Id.ToString(),
            l.Description,
            l.Statut,
            l.DateCreation.ToString("yyyy-MM-dd HH:mm"),
            l.CollectiviteANom,
            l.CollectiviteBNom
        });
        return RetournerCsv(entetes, lignes, $"litiges_{DateTime.UtcNow:yyyy-MM-dd}");
    }

    private async Task<IActionResult> ExporterProjets(Guid? collectiviteId, CancellationToken ct)
    {
        var donnees = await projetService.ListerAsync(collectiviteId, ct);
        var entetes = new[] { "Id", "Intitule", "Montant", "Devise", "Statut", "DateDebut", "DateFin", "Collectivite" };
        var lignes = donnees.Select(p => new[]
        {
            p.Id.ToString(),
            p.Intitule,
            p.Montant.ToString("F2"),
            p.Devise,
            p.Statut,
            p.DateDebut.ToString("yyyy-MM-dd"),
            p.DateFin?.ToString("yyyy-MM-dd") ?? "",
            p.CollectiviteNom
        });
        return RetournerCsv(entetes, lignes, $"projets_{DateTime.UtcNow:yyyy-MM-dd}");
    }

    private async Task<IActionResult> ExporterIndicateurs(Guid? collectiviteId, CancellationToken ct)
    {
        var donnees = await indicateurService.ListerAsync(collectiviteId, ct: ct);
        var entetes = new[] { "Id", "Type", "Valeur", "Unite", "Source", "DateReleve", "Collectivite" };
        var lignes = donnees.Select(i => new[]
        {
            i.Id.ToString(),
            i.Type,
            i.Valeur.ToString("F4"),
            i.Unite,
            i.Source,
            i.DateReleve.ToString("yyyy-MM-dd"),
            i.CollectiviteNom
        });
        return RetournerCsv(entetes, lignes, $"indicateurs_{DateTime.UtcNow:yyyy-MM-dd}");
    }

    /// <summary>Génère un fichier CSV (séparateur « ; » pour compatibilité Excel FR, encodé UTF-8 avec BOM).</summary>
    private static IActionResult RetournerCsv(string[] entetes, IEnumerable<string[]> lignes, string nomFichier)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(";", entetes));

        foreach (var ligne in lignes)
        {
            sb.AppendLine(string.Join(";", ligne.Select(EchapperCsv)));
        }

        var contenu = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return new FileContentResult(contenu, "text/csv; charset=utf-8")
        {
            FileDownloadName = $"{nomFichier}.csv"
        };
    }

    /// <summary>Échappe les champs contenant « ; » ou « " » (RFC 4180).</summary>
    private static string EchapperCsv(string valeur)
    {
        if (valeur.Contains(';') || valeur.Contains('"'))
        {
            return $"\"{valeur.Replace("\"", "\"\"")}\"";
        }
        return valeur;
    }
}
