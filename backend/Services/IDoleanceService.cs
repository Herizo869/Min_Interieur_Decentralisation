using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Models.Enums;

namespace Collectivites.Api.Services;

/// <summary>Services métier des doléances citoyennes (UC-11, UC-12).</summary>
public interface IDoleanceService
{
    /// <summary>
    /// Dépose une doléance (UC-11) : rattachement automatique à la collectivité
    /// dont le contour contient le point (ST_Contains) et génération du numéro de suivi.
    /// </summary>
    /// <exception cref="InvalidOperationException">Données invalides ou point hors de toute collectivité.</exception>
    Task<DoleanceResponse> DeposerAsync(DeposerDoleanceRequest demande, CancellationToken ct = default);

    /// <summary>Suivi citoyen par numéro de dossier (UC-12).</summary>
    Task<DoleanceResponse?> SuivreParNumeroAsync(string numeroSuivi, CancellationToken ct = default);

    /// <summary>Liste des doléances (UC-12), filtres optionnels : collectivité, statut, catégorie.</summary>
    Task<List<DoleanceResponse>> ListerAsync(Guid? collectiviteId = null, StatutDoleance? statut = null, CategorieDoleance? categorie = null, CancellationToken ct = default);

    /// <summary>Fiche d'une doléance (UC-12).</summary>
    Task<DoleanceResponse?> ObtenirParIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Change le statut d'une doléance et trace l'action dans l'historique (UC-12).</summary>
    Task<DoleanceResponse?> ChangerStatutAsync(Guid id, StatutDoleance statut, string auteur, CancellationToken ct = default);
}
