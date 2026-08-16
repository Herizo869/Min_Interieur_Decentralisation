using Collectivites.Api.Models.Entities;

namespace Collectivites.Api.Services;

/// <summary>Services métier du référentiel des collectivités (module 3.2).</summary>
public interface ICollectiviteService
{
    /// <summary>Recherche par nom ou code INSEE (UC-04).</summary>
    Task<List<Collectivite>> RechercherAsync(string? recherche, string? type, CancellationToken ct = default);

    /// <summary>Fiche détaillée d'une collectivité (UC-03/UC-04).</summary>
    Task<Collectivite?> ObtenirParIdAsync(Guid id, CancellationToken ct = default);
}
