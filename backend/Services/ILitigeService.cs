using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Models.Enums;

namespace Collectivites.Api.Services;

/// <summary>Services métier des litiges de limites territoriales (UC-10, UC-14).</summary>
public interface ILitigeService
{
    /// <summary>Signale manuellement un litige (UC-14). Calcule la zone de conflit (intersection).</summary>
    /// <exception cref="InvalidOperationException">Données invalides, collectivités identiques ou sans chevauchement.</exception>
    Task<LitigeResponse> SignalerAsync(SignalerLitigeRequest demande, string auteur, CancellationToken ct = default);

    /// <summary>Liste les litiges, filtres optionnels : collectivité, statut.</summary>
    Task<List<LitigeResponse>> ListerAsync(Guid? collectiviteId = null, StatutLitige? statut = null, CancellationToken ct = default);

    /// <summary>Fiche d'un litige (null si introuvable).</summary>
    Task<LitigeResponse?> ObtenirParIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Change le statut d'un litige et trace l'action (UC-10).</summary>
    Task<LitigeResponse?> ChangerStatutAsync(Guid id, StatutLitige statut, string auteur, CancellationToken ct = default);
}
