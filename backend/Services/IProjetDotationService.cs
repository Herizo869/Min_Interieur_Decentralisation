using Collectivites.Api.Models.Dtos;

namespace Collectivites.Api.Services;

/// <summary>Services métier des projets financés / dotations (UC-06).</summary>
public interface IProjetDotationService
{
    /// <summary>Liste les projets/dotations, éventuellement filtrés par collectivité.</summary>
    Task<List<ProjetDotationResponse>> ListerAsync(Guid? collectiviteId = null, CancellationToken ct = default);

    /// <summary>Fiche d'un projet/dotation (null si introuvable).</summary>
    Task<ProjetDotationResponse?> ObtenirParIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Crée un projet/dotation rattaché à une collectivité.</summary>
    /// <exception cref="InvalidOperationException">Données invalides ou collectivité introuvable.</exception>
    Task<ProjetDotationResponse> CreerAsync(ProjetDotationRequest demande, CancellationToken ct = default);

    /// <summary>Modifie un projet/dotation existant.</summary>
    /// <exception cref="InvalidOperationException">Données invalides ou collectivité introuvable.</exception>
    Task<ProjetDotationResponse?> ModifierAsync(Guid id, ProjetDotationRequest demande, CancellationToken ct = default);

    /// <summary>Supprime un projet/dotation.</summary>
    Task<bool> SupprimerAsync(Guid id, CancellationToken ct = default);
}
