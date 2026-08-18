using Collectivites.Api.Models.Dtos;

namespace Collectivites.Api.Services;

/// <summary>Services métier des indicateurs chiffrés (UC-07).</summary>
public interface IIndicateurService
{
    /// <summary>Liste les indicateurs, éventuellement filtrés par collectivité et/ou type.</summary>
    Task<List<IndicateurResponse>> ListerAsync(Guid? collectiviteId = null, string? type = null, CancellationToken ct = default);

    /// <summary>Fiche d'un indicateur (null si introuvable).</summary>
    Task<IndicateurResponse?> ObtenirParIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Crée un indicateur rattaché à une collectivité.</summary>
    /// <exception cref="InvalidOperationException">Données invalides ou collectivité introuvable.</exception>
    Task<IndicateurResponse> CreerAsync(IndicateurRequest demande, CancellationToken ct = default);

    /// <summary>Modifie un indicateur existant.</summary>
    /// <exception cref="InvalidOperationException">Données invalides ou collectivité introuvable.</exception>
    Task<IndicateurResponse?> ModifierAsync(Guid id, IndicateurRequest demande, CancellationToken ct = default);

    /// <summary>Supprime un indicateur.</summary>
    Task<bool> SupprimerAsync(Guid id, CancellationToken ct = default);
}
