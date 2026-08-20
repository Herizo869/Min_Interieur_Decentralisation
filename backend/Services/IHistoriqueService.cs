using Collectivites.Api.Models.Entities;

namespace Collectivites.Api.Services;

/// <summary>Service de consultation de l'historique / audit (UC-16).</summary>
public interface IHistoriqueService
{
    /// <summary>Liste les entrées d'historique avec filtres optionnels.</summary>
    Task<List<Historique>> ListerAsync(
        string? entite = null,
        Guid? entiteId = null,
        DateTime? dateDebut = null,
        DateTime? dateFin = null,
        int skip = 0,
        int take = 100,
        CancellationToken ct = default);
}
