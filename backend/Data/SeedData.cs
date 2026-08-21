using Collectivites.Api.Data;
using Collectivites.Api.Models.Entities;
using Collectivites.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Collectivites.Api.Data;

/// <summary>
/// Données de démonstration pour Madagascar — regions, départements, communes,
/// projets, indicateurs, litiges, doléances, utilisateurs et historique.
/// </summary>
public static class SeedData
{
    private static readonly GeometryFactory Gf = new(new PrecisionModel(), 4326);

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Regions.AnyAsync()) return; // déjà peuplé

        // ── Régions ──
        var regions = CreerRegions();
        db.Regions.AddRange(regions);
        await db.SaveChangesAsync();

        // ── Départements ──
        var departements = CreerDepartements(regions);
        db.Departements.AddRange(departements);
        await db.SaveChangesAsync();

        // ── Communes ──
        var communes = CreerCommunes(departements);
        db.Communes.AddRange(communes);
        await db.SaveChangesAsync();

        // ── Utilisateurs ──
        var utilisateurs = CreerUtilisateurs();
        db.Utilisateurs.AddRange(utilisateurs);
        await db.SaveChangesAsync();

        // ── Projets & Dotations ──
        var projets = CreerProjets(communes);
        db.ProjetsDotations.AddRange(projets);
        await db.SaveChangesAsync();

        // ── Indicateurs ──
        var indicateurs = CreerIndicateurs(communes);
        db.Indicateurs.AddRange(indicateurs);
        await db.SaveChangesAsync();

        // ── Litiges ──
        var litiges = CreerLitiges(communes);
        db.Litiges.AddRange(litiges);
        db.Historiques.AddRange(litiges.Select(l => new Historique
        {
            Id = Guid.NewGuid(),
            Entite = "Litige",
            EntiteId = l.Id,
            Action = l.Statut == StatutLitige.Signale ? "création" : "changement de statut : Signale → En Instruction",
            Auteur = "Système",
            Date = l.DateCreation
        }));
        await db.SaveChangesAsync();

        // ── Doléances ──
        var doleances = CreerDoleances(communes);
        db.Doleances.AddRange(doleances);
        db.Historiques.AddRange(doleances.Select(d => new Historique
        {
            Id = Guid.NewGuid(),
            Entite = "Doleance",
            EntiteId = d.Id,
            Action = "création",
            Auteur = d.Auteur,
            Date = d.DateCreation
        }));
        await db.SaveChangesAsync();

        // ── Historique supplémentaire (actions de statut) ──
        var historiques = CreerHistoriques(litiges, doleances);
        db.Historiques.AddRange(historiques);
        await db.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════
    //  Régions (6 grandes régions de Madagascar)
    // ═══════════════════════════════════════════
    private static List<Region> CreerRegions() =>
    [
        new() { Id = Guid.NewGuid(), Nom = "Analamanga", CodeAdministratif = "R01", Population = 3_250_000, ChefLieu = "Antananarivo", Contour = PolygoneSimple(-18.8, 47.3, 0.6, 0.8) },
        new() { Id = Guid.NewGuid(), Nom = "Atsinanana", CodeAdministratif = "R02", Population = 1_850_000, ChefLieu = "Toamasina", Contour = PolygoneSimple(-17.5, 49.0, 0.8, 1.0) },
        new() { Id = Guid.NewGuid(), Nom = "Vakinankaratra", CodeAdministratif = "R03", Population = 1_500_000, ChefLieu = "Antsirabe", Contour = PolygoneSimple(-20.0, 47.0, 0.5, 0.7) },
        new() { Id = Guid.NewGuid(), Nom = "Betsiboka", CodeAdministratif = "R04", Population = 780_000, ChefLieu = "Mahajanga", Contour = PolygoneSimple(-16.5, 46.0, 0.7, 0.9) },
        new() { Id = Guid.NewGuid(), Nom = "Sofia", CodeAdministratif = "R05", Population = 1_200_000, ChefLieu = "Antsiranana", Contour = PolygoneSimple(-14.0, 49.0, 0.6, 0.8) },
        new() { Id = Guid.NewGuid(), Nom = "Atsimo-Atsinanana", CodeAdministratif = "R06", Population = 890_000, ChefLieu = "Fianarantsoa", Contour = PolygoneSimple(-22.0, 47.5, 0.6, 0.7) },
    ];

    // ═══════════════════════════════════════════
    //  Départements (2 sous chaque région)
    // ═══════════════════════════════════════════
    private static List<Departement> CreerDepartements(List<Region> regions)
    {
        var deps = new List<Departement>();
        var details = new[]
        {
            (RegionIdx: 0, Nom: "Antananarivo-Renivohitra", Code: "D01", Pop: 1_300_000, Pref: "Antananarivo", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 0, Nom: "Amoron'i Mania", Code: "D02", Pop: 850_000, Pref: "Ambositra", OffsetX: 0.3, OffsetY: 0.3),
            (RegionIdx: 1, Nom: "Toamasina-I", Code: "D03", Pop: 1_100_000, Pref: "Toamasina", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 1, Nom: "Antsinanana-Nord", Code: "D04", Pop: 750_000, Pref: "Fénérive Est", OffsetX: 0.4, OffsetY: 0.3),
            (RegionIdx: 2, Nom: "Antsirabe-I", Code: "D05", Pop: 900_000, Pref: "Antsirabe", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 2, Nom: "Vakinankaratra-Sud", Code: "D06", Pop: 600_000, Pref: "Ambatolampy", OffsetX: 0.2, OffsetY: 0.2),
            (RegionIdx: 3, Nom: "Mahajanga-I", Code: "D07", Pop: 500_000, Pref: "Mahajanga", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 3, Nom: "Betsiboka-Nord", Code: "D08", Pop: 280_000, Pref: "Ambanja", OffsetX: 0.3, OffsetY: 0.3),
            (RegionIdx: 4, Nom: "Antsiranana-I", Code: "D09", Pop: 650_000, Pref: "Antsiranana", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 4, Nom: "Diana", Code: "D10", Pop: 550_000, Pref: "Ambilobe", OffsetX: 0.25, OffsetY: 0.2),
            (RegionIdx: 5, Nom: "Fianarantsoa-I", Code: "D11", Pop: 500_000, Pref: "Fianarantsoa", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 5, Nom: "Haute Matsiatra", Code: "D12", Pop: 390_000, Pref: "Ambalavao", OffsetX: 0.25, OffsetY: 0.25),
        };

        foreach (var d in details)
        {
            deps.Add(new Departement
            {
                Id = Guid.NewGuid(),
                Nom = d.Nom,
                CodeAdministratif = d.Code,
                Population = d.Pop,
                Prefecture = d.Pref,
                Contour = PolygoneSimple(
                    regions[d.RegionIdx].Contour.EnvelopeInternal.MinY + d.OffsetY,
                    regions[d.RegionIdx].Contour.EnvelopeInternal.MinX + d.OffsetX,
                    0.25, 0.35)
            });
        }
        return deps;
    }

    // ═══════════════════════════════════════════
    //  Communes (2-3 sous chaque département)
    // ═══════════════════════════════════════════
    private static List<Commune> CreerCommunes(List<Departement> departements)
    {
        var communes = new List<Commune>();
        int codeNum = 1;

        var nomsParDep = new Dictionary<int, (string[] Noms, int[] Pops, double[] OffX, double[] OffY)>
        {
            [0] = (["Antananarivo", "Ambohidratrimo", "Andohatapenaka"],
                   [1_275_000, 420_000, 180_000],
                   [0.0, 0.15, 0.08],
                   [0.0, 0.12, 0.20]),
            [1] = (["Ambositra", "Fandriana", "Ambatofinandrahana"],
                   [150_000, 95_000, 80_000],
                   [0.0, 0.12, 0.24],
                   [0.0, 0.10, 0.20]),
            [2] = (["Toamasina", "Vohibinany", "Fénérive Est"],
                   [350_000, 120_000, 95_000],
                   [0.0, 0.18, 0.35],
                   [0.0, 0.15, 0.28]),
            [3] = (["Fénérive Est", "Mahanoro", "Brickaville"],
                   [95_000, 85_000, 60_000],
                   [0.0, 0.20, 0.10],
                   [0.0, 0.18, 0.25]),
            [4] = (["Antsirabe", "Soaviaritra", "Betafo"],
                   [250_000, 85_000, 65_000],
                   [0.0, 0.10, 0.20],
                   [0.0, 0.15, 0.08]),
            [5] = (["Ambatolampy", "Morarano Gare", "Ankazobe"],
                   [80_000, 55_000, 40_000],
                   [0.0, 0.12, 0.05],
                   [0.0, 0.10, 0.18]),
            [6] = (["Mahajanga", "Marovoay", "Mitsinjo"],
                   [220_000, 75_000, 45_000],
                   [0.0, 0.15, 0.25],
                   [0.0, 0.10, 0.05]),
            [7] = (["Ambanja", "Nosy Be", "Ambilobe"],
                   [45_000, 35_000, 55_000],
                   [0.0, 0.20, 0.10],
                   [0.0, 0.15, 0.22]),
            [8] = (["Antsiranana", "Ramena", "Joffreville"],
                   [130_000, 25_000, 15_000],
                   [0.0, 0.12, 0.08],
                   [0.0, 0.10, 0.20]),
            [9] = (["Ambilobe", "Ampisikina", "Daraina"],
                   [55_000, 30_000, 20_000],
                   [0.0, 0.15, 0.25],
                   [0.0, 0.12, 0.05]),
            [10] = (["Fianarantsoa", "Ambohimasay", "Isorana"],
                   [190_000, 85_000, 55_000],
                   [0.0, 0.10, 0.20],
                   [0.0, 0.15, 0.08]),
            [11] = (["Ambalavao", "Begogo", "Ianakafy"],
                   [40_000, 25_000, 18_000],
                   [0.0, 0.12, 0.22],
                   [0.0, 0.08, 0.16]),
        };

        foreach (var dep in departements)
        {
            int idx = departements.IndexOf(dep);
            if (!nomsParDep.ContainsKey(idx)) continue;

            var (noms, pops, offX, offY) = nomsParDep[idx];
            for (int i = 0; i < noms.Length; i++)
            {
                communes.Add(new Commune
                {
                    Id = Guid.NewGuid(),
                    Nom = noms[i],
                    CodeAdministratif = $"C{codeNum:D3}",
                    Population = pops[i],
                    CodePostal = $"10{codeNum:D2}",
                    Contour = PolygoneSimple(
                        dep.Contour.EnvelopeInternal.MinY + offY[i],
                        dep.Contour.EnvelopeInternal.MinX + offX[i],
                        0.10, 0.12)
                });
                codeNum++;
            }
        }
        return communes;
    }

    // ═══════════════════════════════════════════
    //  Utilisateurs (admin + agents)
    // ═══════════════════════════════════════════
    private static List<Utilisateur> CreerUtilisateurs() =>
    [
        new()
        {
            Id = Guid.NewGuid(), Nom = "Rija Andrianarivelo",
            Identifiant = "rija", Role = Role.Administrateur,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Agent@1234"), Actif = true
        },
        new()
        {
            Id = Guid.NewGuid(), Nom = "Hery Rabearimanana",
            Identifiant = "hery", Role = Role.Agent,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Agent@1234"), Actif = true
        },
        new()
        {
            Id = Guid.NewGuid(), Nom = "Naina Razafindrabe",
            Identifiant = "naina", Role = Role.Agent,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Agent@1234"), Actif = true
        },
        new()
        {
            Id = Guid.NewGuid(), Nom = "Tojo Ramanantsoa",
            Identifiant = "tojo", Role = Role.Agent,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Agent@1234"), Actif = true
        },
    ];

    // ═══════════════════════════════════════════
    //  Projets & Dotations
    // ═══════════════════════════════════════════
    private static List<ProjetDotation> CreerProjets(List<Commune> communes)
    {
        var projets = new List<ProjetDotation>();
        var rng = new Random(42);

        var specs = new[]
        {
            ("Réhabilitation RN7 Antananarivo – Toamasina", 45_000_000m, "MGA", StatutProjet.EnCours),
            ("Construction école primaire Ambohidratrimo", 85_000_000m, "MGA", StatutProjet.EnCours),
            ("Réseau d'éclairage public Mahajanga", 32_000_000m, "MGA", StatutProjet.Termine),
            ("Adduction eau potable Antsirabe", 120_000_000m, "MGA", StatutProjet.EnPreparation),
            ("Centre de santé Fianarantsoa", 95_000_000m, "MGA", StatutProjet.EnCours),
            ("Marché couvert Toamasina", 60_000_000m, "MGA", StatutProjet.Termine),
            ("Assainissement Nosy Be", 75_000_000m, "MGA", StatutProjet.EnPreparation),
            ("Route communale Ambositra – Fandriana", 55_000_000m, "MGA", StatutProjet.EnCours),
            ("Bibliothèque universitaire Antananarivo", 40_000_000m, "MGA", StatutProjet.Termine),
            ("Stade municipal Antsiranana", 150_000_000m, "MGA", StatutProjet.EnPreparation),
            ("Voirie urbaine Ambalavao", 25_000_000m, "MGA", StatutProjet.EnCours),
            ("Potable eau Fénérive Est", 68_000_000m, "MGA", StatutProjet.Termine),
            ("Collège Ambanja", 42_000_000m, "MGA", StatutProjet.EnPreparation),
            ("Pavage rue Ranaivo Antsirabe", 38_000_000m, "MGA", StatutProjet.EnCours),
            ("Marché artisanal Ambositra", 28_000_000m, "MGA", StatutProjet.Termine),
            ("Dotation de fonctionnement Mahajanga", 50_000_000m, "MGA", StatutProjet.Termine),
        };

        foreach (var (intitule, montant, devise, statut) in specs)
        {
            var commune = communes[rng.Next(communes.Count)];
            projets.Add(new ProjetDotation
            {
                Id = Guid.NewGuid(),
                Intitule = intitule,
                Montant = montant,
                Devise = devise,
                Statut = statut,
                DateDebut = DateTime.UtcNow.AddDays(-rng.Next(30, 365)),
                DateFin = statut == StatutProjet.Termine ? DateTime.UtcNow.AddDays(-rng.Next(1, 30)) : DateTime.UtcNow.AddDays(rng.Next(60, 540)),
                CollectiviteId = commune.Id
            });
        }
        return projets;
    }

    // ═══════════════════════════════════════════
    //  Indicateurs
    // ═══════════════════════════════════════════
    private static List<Indicateur> CreerIndicateurs(List<Commune> communes)
    {
        var indicateurs = new List<Indicateur>();
        var rng = new Random(42);

        var types = new[]
        {
            ("Taux de scolarisation", "%", "INSTAT", 65m, 85m),
            ("Taux d'accès à l'eau potable", "%", "DREDD", 30m, 60m),
            ("Taux d'accès à l'électricité", "%", "JIRAMA", 15m, 45m),
            ("Nombre d'infrastructures sanitaires", "unité", "DPS", 2m, 12m),
            ("Longueur routes bitumées", "km", "DTR", 5m, 35m),
            ("Superficie agricole", "ha", "DRIAE", 500m, 5000m),
            ("Densité de population", "hab/km²", "INSTAT", 50m, 800m),
            ("Taux de pauvreté", "%", "INSTAT", 40m, 80m),
        };

        foreach (var commune in communes)
        {
            // 4-6 indicateurs par commune
            var nb = rng.Next(4, 7);
            var selected = types.OrderBy(_ => rng.Next()).Take(nb);
            foreach (var (type, unite, source, min, max) in selected)
            {
                indicateurs.Add(new Indicateur
                {
                    Id = Guid.NewGuid(),
                    Type = type,
                    Valeur = Math.Round(min + (decimal)rng.NextDouble() * (max - min), 2),
                    Unite = unite,
                    Source = source,
                    DateReleve = DateTime.UtcNow.AddDays(-rng.Next(1, 180)),
                    CollectiviteId = commune.Id
                });
            }
        }
        return indicateurs;
    }

    // ═══════════════════════════════════════════
    //  Litiges de limites (chevauchements entre communes proches)
    // ═══════════════════════════════════════════
    private static List<Litige> CreerLitiges(List<Commune> communes)
    {
        var litiges = new List<Litige>();
        var rng = new Random(42);
        var statuts = new[] { StatutLitige.Signale, StatutLitige.EnInstruction, StatutLitige.Arbitre, StatutLitige.Clos };
        var descriptions = new[]
        {
            "Chevauchement des limites administratives entre les deux communes sur la zone des rizières de la vallée.",
            "Conflit de territoire autour du marché central — frontière contestée par les deux mairies.",
            "Zone de chevauchement détectée sur les terres agricoles entre les deux collectivités.",
            "Litige concernant la rivière Amboniloha — cours d'eau frontalier mal délimité.",
            "Conflit de périmètre sur les terrains communalisés du quartier Tanambao.",
            "Dépassement des limites reconnues lors de la numérisation du cadastre communal.",
        };

        // Paires de communes adjacentes (indices)
        var paires = new[] { (0, 1), (2, 3), (4, 5), (6, 7), (8, 9), (10, 11) };

        for (int i = 0; i < Math.Min(paires.Length, descriptions.Length); i++)
        {
            var (a, b) = paires[i];
            if (a >= communes.Count || b >= communes.Count) continue;

            var centre = new Coordinate(
                (communes[a].Contour.Centroid.X + communes[b].Contour.Centroid.X) / 2,
                (communes[a].Contour.Centroid.Y + communes[b].Contour.Centroid.Y) / 2);
            var zoneConflit = Carre(centre.X, centre.Y, 0.04);

            litiges.Add(new Litige
            {
                Id = Guid.NewGuid(),
                Description = descriptions[i],
                Statut = statuts[i % statuts.Length],
                DateCreation = DateTime.UtcNow.AddDays(-rng.Next(10, 200)),
                Geometrie = Gf.CreatePoint(centre),
                ZoneConflit = zoneConflit,
                CollectiviteAId = communes[a].Id,
                CollectiviteBId = communes[b].Id
            });
        }
        return litiges;
    }

    // ═══════════════════════════════════════════
    //  Doléances citoyennes
    // ═══════════════════════════════════════════
    private static List<Doleance> CreerDoleances(List<Commune> communes)
    {
        var doleances = new List<Doleance>();
        var rng = new Random(42);

        var specs = new[]
        {
            ("Route nationale RN7 en mauvais état entre Ambositra et Fandriana, nids-de-poule dangereux.", CategorieDoleance.Voirie, StatutDoleance.Nouveau, "Andry R."),
            ("Éclairage public défectueux avenue de l'Indépendance, zone dangereuse la nuit.", CategorieDoleance.Eclairage, StatutDoleance.EnCours, "Fara M."),
            ("Décharge sauvage près du marché, pollution et odeurs insupportables.", CategorieDoleance.Environnement, StatutDoleance.Resolu, "Jean R."),
            ("Fuite d'eau potable depuis 3 jours dans le quartier Tanambao.", CategorieDoleance.Assainissement, StatutDoleance.EnCours, "Hélène P."),
            ("Pont en bois dangereux sur la rivière Tsiribihina, risque d'effondrement.", CategorieDoleance.Voirie, StatutDoleance.Nouveau, "Désiré K."),
            ("Panne de courant électrique récurrente dans tout le quartierAnalakely.", CategorieDoleance.Autre, StatutDoleance.Nouveau, "Marie N."),
            ("Trottoirs inexistants rue Ranaivo, les piétons marchent sur la chaussée.", CategorieDoleance.Voirie, StatutDoleance.EnCours, "Paul B."),
            ("Canal d'assainissement bouché, eaux stagnantes et risque sanitaire.", CategorieDoleance.Assainissement, StatutDoleance.Nouveau, "Sylvie R."),
            ("Arbre menaçant de tomber sur la route principale, pas d'intervention depuis 2 semaines.", CategorieDoleance.Environnement, StatutDoleance.EnCours, "Robert T."),
            ("Éclairage public installé mais jamais allumé depuis 6 mois.", CategorieDoleance.Eclairage, StatutDoleance.Nouveau, "Lalao A."),
            ("Dépotoir sauvage au bord de la rivière, eaux contaminées.", CategorieDoleance.Environnement, StatutDoleance.Resolu, "Njiva H."),
            ("Fossé de drainage comblé, inondation à chaque pluie.", CategorieDoleance.Assainissement, StatutDoleance.EnCours, "Patrick V."),
            ("École sans fenêtres depuis la tempête, enfants exposés.", CategorieDoleance.Autre, StatutDoleance.Nouveau, "Clément R."),
            ("Feu rouge en panne à la carrefour RN7/RN35, accidents fréquents.", CategorieDoleance.Voirie, StatutDoleance.EnCours, "Hanta T."),
            ("Poubelle municipale renversée depuis une semaine, déchets sur la voie publique.", CategorieDoleance.Environnement, StatutDoleance.Nouveau, "Fidy M."),
        };

        foreach (var (desc, categorie, statut, auteur) in specs)
        {
            var commune = communes[rng.Next(communes.Count)];
            var pt = new Coordinate(
                commune.Contour.Centroid.X + (rng.NextDouble() - 0.5) * 0.05,
                commune.Contour.Centroid.Y + (rng.NextDouble() - 0.5) * 0.05);

            doleances.Add(new Doleance
            {
                Id = Guid.NewGuid(),
                Description = desc,
                Categorie = categorie,
                Statut = statut,
                Auteur = auteur,
                NumeroSuivi = $"DOL-{DateTime.UtcNow.Year}-{rng.Next(10000, 99999)}",
                DateCreation = DateTime.UtcNow.AddDays(-rng.Next(1, 120)),
                Geometrie = Gf.CreatePoint(pt),
                CollectiviteRattacheeId = commune.Id
            });
        }
        return doleances;
    }

    // ═══════════════════════════════════════════
    //  Historique (actions supplémentaires)
    // ═══════════════════════════════════════════
    private static List<Historique> CreerHistoriques(List<Litige> litiges, List<Doleance> doleances)
    {
        var entries = new List<Historique>();
        var rng = new Random(42);
        var auteurs = new[] { "Rija Andrianarivelo", "Hery Rabearimanana", "Naina Razafindrabe", "Tojo Ramanantsoa" };

        // Statut changes pour les litiges
        foreach (var l in litiges.Where(l => l.Statut != StatutLitige.Signale))
        {
            entries.Add(new Historique
            {
                Id = Guid.NewGuid(),
                Entite = "Litige",
                EntiteId = l.Id,
                Action = "changement de statut : Signale → En Instruction",
                Auteur = auteurs[rng.Next(auteurs.Length)],
                Date = l.DateCreation.AddDays(rng.Next(3, 15))
            });
            if (l.Statut == StatutLitige.Arbitre || l.Statut == StatutLitige.Clos)
            {
                entries.Add(new Historique
                {
                    Id = Guid.NewGuid(),
                    Entite = "Litige",
                    EntiteId = l.Id,
                    Action = "changement de statut : En Instruction → Arbitré",
                    Auteur = "Rija Andrianarivelo",
                    Date = l.DateCreation.AddDays(rng.Next(20, 60))
                });
            }
        }

        // Statut changes pour les doléances
        foreach (var d in doleances.Where(d => d.Statut != StatutDoleance.Nouveau))
        {
            entries.Add(new Historique
            {
                Id = Guid.NewGuid(),
                Entite = "Doleance",
                EntiteId = d.Id,
                Action = "changement de statut : Nouveau → En cours de traitement",
                Auteur = auteurs[rng.Next(auteurs.Length)],
                Date = d.DateCreation.AddDays(rng.Next(2, 10))
            });
            if (d.Statut == StatutDoleance.Resolu)
            {
                entries.Add(new Historique
                {
                    Id = Guid.NewGuid(),
                    Entite = "Doleance",
                    EntiteId = d.Id,
                    Action = "changement de statut : En cours → Résolu",
                    Auteur = "Hery Rabearimanana",
                    Date = d.DateCreation.AddDays(rng.Next(15, 45))
                });
            }
        }

        return entries;
    }

    // ═══════════════════════════════════════════
    //  Géométrie helpers
    // ═══════════════════════════════════════════
    private static Polygon PolygoneSimple(double x, double y, double w, double h) =>
        Gf.CreatePolygon([
            new Coordinate(y, x),       // GeoJSON: [lng, lat]
            new Coordinate(y + h, x),
            new Coordinate(y + h, x + w),
            new Coordinate(y, x + w),
            new Coordinate(y, x) // fermeture
        ]);

    private static Polygon Carre(double cx, double cy, double size) =>
        PolygoneSimple(cy - size / 2, cx - size / 2, size, size);
}
