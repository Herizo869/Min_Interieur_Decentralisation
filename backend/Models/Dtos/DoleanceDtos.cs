using Collectivites.Api.Models.Enums;
using NetTopologySuite.Geometries;

namespace Collectivites.Api.Models.Dtos;

/// <summary>Dépôt d'une doléance citoyenne (UC-11, public).</summary>
public class DeposerDoleanceRequest
{
    public string Description { get; set; } = string.Empty;

    public CategorieDoleance Categorie { get; set; }

    /// <summary>Dénomination simple de l'auteur (pas de données personnelles sensibles).</summary>
    public string Auteur { get; set; } = string.Empty;

    /// <summary>Localisation du problème — point GeoJSON (WGS84).</summary>
    public Geometry Point { get; set; } = null!;
}

/// <summary>Changement de statut d'une doléance (UC-12, agent/admin).</summary>
public class ModifierStatutDoleanceRequest
{
    public StatutDoleance Statut { get; set; }
}

/// <summary>Doléance renvoyée par l'API (UC-11/12).</summary>
public class DoleanceResponse
{
    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Categorie { get; set; } = string.Empty;

    public string Statut { get; set; } = string.Empty;

    public string Auteur { get; set; } = string.Empty;

    /// <summary>Numéro de dossier communiqué au citoyen (UC-11).</summary>
    public string NumeroSuivi { get; set; } = string.Empty;

    public DateTime DateCreation { get; set; }

    public Guid CollectiviteRattacheeId { get; set; }

    public string CollectiviteRattacheeNom { get; set; } = string.Empty;

    public Geometry Geometrie { get; set; } = null!;
}
