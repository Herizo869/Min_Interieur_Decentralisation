namespace Collectivites.Api.Models.Dtos;

/// <summary>Réponse du tableau de bord (UC-15) — synthèses chiffrées de tous les modules.</summary>
public class TableauDeBordResponse
{
    /// <summary>Statistiques des collectivités.</summary>
    public CollectiviteStats Collectivites { get; set; } = new();

    /// <summary>Statistiques des projets et dotations.</summary>
    public ProjetStats Projets { get; set; } = new();

    /// <summary>Statistiques des indicateurs.</summary>
    public IndicateurStats Indicateurs { get; set; } = new();

    /// <summary>Statistiques des litiges de limites.</summary>
    public LitigeStats Litiges { get; set; } = new();

    /// <summary>Statistiques des doléances citoyennes.</summary>
    public DoleanceStats Doléances { get; set; } = new();

    /// <summary>Statistiques des utilisateurs.</summary>
    public UtilisateurStats Utilisateurs { get; set; } = new();
}

/// <summary>Synthèse des collectivités.</summary>
public class CollectiviteStats
{
    /// <summary>Total de collectivités.</summary>
    public int Total { get; set; }

    /// <summary>Décompte par type (commune, département, région, epci).</summary>
    public Dictionary<string, int> ParType { get; set; } = new();
}

/// <summary>Synthèse des projets et dotations.</summary>
public class ProjetStats
{
    /// <summary>Total de projets.</summary>
    public int Total { get; set; }

    /// <summary>Décompte par statut (EnPreparation, EnCours, Termine).</summary>
    public Dictionary<string, int> ParStatut { get; set; } = new();

    /// <summary>Montant total de tous les projets (toutes devises confondues).</summary>
    public decimal MontantTotal { get; set; }

    /// <summary>Moyenne des montants.</summary>
    public decimal MontantMoyen { get; set; }
}

/// <summary>Synthèse des indicateurs.</summary>
public class IndicateurStats
{
    /// <summary>Total d'indicateurs.</summary>
    public int Total { get; set; }

    /// <summary>Décompte par type d'indicateur.</summary>
    public Dictionary<string, int> ParType { get; set; } = new();

    /// <summary>Nombre de collectivités couvertes par au moins un indicateur.</summary>
    public int CollectivitesCouvertes { get; set; }
}

/// <summary>Synthèse des litiges de limites.</summary>
public class LitigeStats
{
    /// <summary>Total de litiges.</summary>
    public int Total { get; set; }

    /// <summary>Décompte par statut (Signale, EnInstruction, Arbitre, Clos).</summary>
    public Dictionary<string, int> ParStatut { get; set; } = new();

    /// <summary>Litiges encore ouverts (Signale + EnInstruction).</summary>
    public int Ouverts { get; set; }
}

/// <summary>Synthèse des doléances citoyennes.</summary>
public class DoleanceStats
{
    /// <summary>Total de doléances.</summary>
    public int Total { get; set; }

    /// <summary>Décompte par statut (Nouveau, EnCours, Resolu).</summary>
    public Dictionary<string, int> ParStatut { get; set; } = new();

    /// <summary>Décompte par catégorie.</summary>
    public Dictionary<string, int> ParCategorie { get; set; } = new();

    /// <summary>Doléances en attente (Nouveau).</summary>
    public int EnAttente { get; set; }
}

/// <summary>Synthèse des utilisateurs.</summary>
public class UtilisateurStats
{
    /// <summary>Total d'utilisateurs.</summary>
    public int Total { get; set; }

    /// <summary>Décompte par rôle.</summary>
    public Dictionary<string, int> ParRole { get; set; } = new();

    /// <summary>Comptes actifs.</summary>
    public int Actifs { get; set; }
}
