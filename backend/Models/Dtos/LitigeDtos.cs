using Collectivites.Api.Models.Enums;
using NetTopologySuite.Geometries;

namespace Collectivites.Api.Models.Dtos;

/// <summary>Signalement manuel d'un litige de limites (UC-14).</summary>
public class SignalerLitigeRequest
{
    public string Description { get; set; } = string.Empty;

    /// <summary>Géométrie du constat terrain (point ou zone d'observation, WGS84).</summary>
    public Geometry Geometrie { get; set; } = null!;

    /// <summary>Première collectivité en litige.</summary>
    public Guid CollectiviteAId { get; set; }

    /// <summary>Deuxième collectivité en litige (différente de A).</summary>
    public Guid CollectiviteBId { get; set; }
}

/// <summary>Changement de statut d'un litige (UC-10, agent/admin).</summary>
public class ModifierStatutLitigeRequest
{
    public StatutLitige Statut { get; set; }
}

/// <summary>Litige renvoyé par l'API (UC-14).</summary>
public class LitigeResponse
{
    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Statut { get; set; } = string.Empty;

    public DateTime DateCreation { get; set; }

    public Guid CollectiviteAId { get; set; }

    public string CollectiviteANom { get; set; } = string.Empty;

    public Guid CollectiviteBId { get; set; }

    public string CollectiviteBNom { get; set; } = string.Empty;

    /// <summary>Zone de conflit calculée (intersection des contours).</summary>
    public Geometry ZoneConflit { get; set; } = null!;

    /// <summary>Géométrie du constat terrain.</summary>
    public Geometry Geometrie { get; set; } = null!;
}
