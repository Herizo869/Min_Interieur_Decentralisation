using Collectivites.Api.Data;
using Collectivites.Api.Models.Entities;
using Collectivites.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Collectivites.Api.Data;

/// <summary>
/// Données de démonstration étendues pour Madagascar — regions, départements,
/// communes, EPCI, projets, indicateurs, litiges, doléances, utilisateurs et historique.
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

        // ── EPCI ──
        var epcis = CreerEpcis(communes);
        db.Epcis.AddRange(epcis);
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
    //  Régions (12 régions de Madagascar)
    // ═══════════════════════════════════════════
    private static List<Region> CreerRegions() =>
    [
        new() { Id = Guid.NewGuid(), Nom = "Analamanga", CodeAdministratif = "R01", Population = 3_250_000, ChefLieu = "Antananarivo", Contour = PolygoneSimple(-18.8, 47.3, 0.6, 0.8) },
        new() { Id = Guid.NewGuid(), Nom = "Atsinanana", CodeAdministratif = "R02", Population = 1_850_000, ChefLieu = "Toamasina", Contour = PolygoneSimple(-17.5, 49.0, 0.8, 1.0) },
        new() { Id = Guid.NewGuid(), Nom = "Vakinankaratra", CodeAdministratif = "R03", Population = 1_500_000, ChefLieu = "Antsirabe", Contour = PolygoneSimple(-20.0, 47.0, 0.5, 0.7) },
        new() { Id = Guid.NewGuid(), Nom = "Betsiboka", CodeAdministratif = "R04", Population = 780_000, ChefLieu = "Mahajanga", Contour = PolygoneSimple(-16.5, 46.0, 0.7, 0.9) },
        new() { Id = Guid.NewGuid(), Nom = "Sofia", CodeAdministratif = "R05", Population = 1_200_000, ChefLieu = "Antsiranana", Contour = PolygoneSimple(-14.0, 49.0, 0.6, 0.8) },
        new() { Id = Guid.NewGuid(), Nom = "Amoron'i Mania", CodeAdministratif = "R06", Population = 890_000, ChefLieu = "Ambositra", Contour = PolygoneSimple(-21.0, 47.2, 0.5, 0.6) },
        new() { Id = Guid.NewGuid(), Nom = "Atsimo-Atsinanana", CodeAdministratif = "R07", Population = 720_000, ChefLieu = "Farafangana", Contour = PolygoneSimple(-23.0, 47.8, 0.5, 0.7) },
        new() { Id = Guid.NewGuid(), Nom = "Haute Matsiatra", CodeAdministratif = "R08", Population = 1_100_000, ChefLieu = "Fianarantsoa", Contour = PolygoneSimple(-22.0, 47.5, 0.6, 0.7) },
        new() { Id = Guid.NewGuid(), Nom = "Melaky", CodeAdministratif = "R09", Population = 350_000, ChefLieu = "Maintirano", Contour = PolygoneSimple(-18.0, 44.0, 0.8, 0.6) },
        new() { Id = Guid.NewGuid(), Nom = "Menabe", CodeAdministratif = "R10", Population = 620_000, ChefLieu = "Morondava", Contour = PolygoneSimple(-20.5, 44.3, 0.7, 0.8) },
        new() { Id = Guid.NewGuid(), Nom = "Anosy", CodeAdministratif = "R11", Population = 540_000, ChefLieu = "Toliara", Contour = PolygoneSimple(-23.5, 44.0, 0.8, 0.9) },
        new() { Id = Guid.NewGuid(), Nom = "Ihorombe", CodeAdministratif = "R12", Population = 310_000, ChefLieu = "Ihosy", Contour = PolygoneSimple(-22.5, 46.0, 0.5, 0.6) },
    ];

    // ═══════════════════════════════════════════
    //  Départements (3 sous chaque région)
    // ═══════════════════════════════════════════
    private static List<Departement> CreerDepartements(List<Region> regions)
    {
        var deps = new List<Departement>();
        var details = new[]
        {
            // R01 Analamanga
            (RegionIdx: 0, Nom: "Antananarivo-Renivohitra", Code: "D01", Pop: 1_300_000, Pref: "Antananarivo", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 0, Nom: "Amoron'i Mania", Code: "D02", Pop: 850_000, Pref: "Ambositra", OffsetX: 0.3, OffsetY: 0.3),
            (RegionIdx: 0, Nom: "Analamanga-Sud", Code: "D03", Pop: 1_100_000, Pref: "Antananarivo", OffsetX: 0.15, OffsetY: 0.15),
            // R02 Atsinanana
            (RegionIdx: 1, Nom: "Toamasina-I", Code: "D04", Pop: 1_100_000, Pref: "Toamasina", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 1, Nom: "Antsinanana-Nord", Code: "D05", Pop: 750_000, Pref: "Fénérive Est", OffsetX: 0.4, OffsetY: 0.3),
            (RegionIdx: 1, Nom: "Antsinanana-Sud", Code: "D06", Pop: 600_000, Pref: "Mahanoro", OffsetX: 0.2, OffsetY: 0.5),
            // R03 Vakinankaratra
            (RegionIdx: 2, Nom: "Antsirabe-I", Code: "D07", Pop: 900_000, Pref: "Antsirabe", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 2, Nom: "Vakinankaratra-Sud", Code: "D08", Pop: 600_000, Pref: "Ambatolampy", OffsetX: 0.2, OffsetY: 0.2),
            (RegionIdx: 2, Nom: "Ambositra-II", Code: "D09", Pop: 450_000, Pref: "Ambositra", OffsetX: 0.35, OffsetY: 0.1),
            // R04 Betsiboka
            (RegionIdx: 3, Nom: "Mahajanga-I", Code: "D10", Pop: 500_000, Pref: "Mahajanga", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 3, Nom: "Betsiboka-Nord", Code: "D11", Pop: 280_000, Pref: "Ambanja", OffsetX: 0.3, OffsetY: 0.3),
            (RegionIdx: 3, Nom: "Betsiboka-Sud", Code: "D12", Pop: 200_000, Pref: "Maevatanana", OffsetX: 0.15, OffsetY: 0.5),
            // R05 Sofia
            (RegionIdx: 4, Nom: "Antsiranana-I", Code: "D13", Pop: 650_000, Pref: "Antsiranana", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 4, Nom: "Diana", Code: "D14", Pop: 550_000, Pref: "Ambilobe", OffsetX: 0.25, OffsetY: 0.2),
            (RegionIdx: 4, Nom: "Sava", Code: "D15", Pop: 400_000, Pref: "Sambava", OffsetX: 0.4, OffsetY: 0.0),
            // R06 Amoron'i Mania
            (RegionIdx: 5, Nom: "Ambositra-I", Code: "D16", Pop: 350_000, Pref: "Ambositra", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 5, Nom: "Fandriana", Code: "D17", Pop: 280_000, Pref: "Fandriana", OffsetX: 0.2, OffsetY: 0.15),
            (RegionIdx: 5, Nom: "Ambatofinandrahana", Code: "D18", Pop: 260_000, Pref: "Ambatofinandrahana", OffsetX: 0.1, OffsetY: 0.3),
            // R07 Atsimo-Atsinanana
            (RegionIdx: 6, Nom: "Farafangana", Code: "D19", Pop: 350_000, Pref: "Farafangana", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 6, Nom: "Vangaindrano", Code: "D20", Pop: 220_000, Pref: "Vangaindrano", OffsetX: 0.3, OffsetY: 0.15),
            (RegionIdx: 6, Nom: "Midongy", Code: "D21", Pop: 150_000, Pref: "Midongy Atsimo", OffsetX: 0.15, OffsetY: 0.35),
            // R08 Haute Matsiatra
            (RegionIdx: 7, Nom: "Fianarantsoa-I", Code: "D22", Pop: 500_000, Pref: "Fianarantsoa", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 7, Nom: "Haute Matsiatra-Nord", Code: "D23", Pop: 350_000, Pref: "Ambalavao", OffsetX: 0.25, OffsetY: 0.25),
            (RegionIdx: 7, Nom: "Lalangina", Code: "D24", Pop: 250_000, Pref: "Isorana", OffsetX: 0.1, OffsetY: 0.4),
            // R09 Melaky
            (RegionIdx: 8, Nom: "Maintirano", Code: "D25", Pop: 180_000, Pref: "Maintirano", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 8, Nom: "Antsalova", Code: "D26", Pop: 90_000, Pref: "Antsalova", OffsetX: 0.3, OffsetY: 0.15),
            (RegionIdx: 8, Nom: "Soalala", Code: "D27", Pop: 80_000, Pref: "Soalala", OffsetX: 0.15, OffsetY: 0.3),
            // R10 Menabe
            (RegionIdx: 9, Nom: "Morondava", Code: "D28", Pop: 350_000, Pref: "Morondava", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 9, Nom: "Miandrivazo", Code: "D29", Pop: 170_000, Pref: "Miandrivazo", OffsetX: 0.25, OffsetY: 0.2),
            (RegionIdx: 9, Nom: "Mahabo", Code: "D30", Pop: 100_000, Pref: "Mahabo", OffsetX: 0.1, OffsetY: 0.4),
            // R11 Anosy
            (RegionIdx: 10, Nom: "Toliara-I", Code: "D31", Pop: 300_000, Pref: "Toliara", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 10, Nom: "Fort-Dauphin", Code: "D32", Pop: 150_000, Pref: "Fort-Dauphin", OffsetX: 0.35, OffsetY: 0.3),
            (RegionIdx: 10, Nom: "Amboasary", Code: "D33", Pop: 90_000, Pref: "Amboasary Sud", OffsetX: 0.45, OffsetY: 0.15),
            // R12 Ihorombe
            (RegionIdx: 11, Nom: "Ihosy", Code: "D34", Pop: 180_000, Pref: "Ihosy", OffsetX: 0.0, OffsetY: 0.0),
            (RegionIdx: 11, Nom: "Iakora", Code: "D35", Pop: 80_000, Pref: "Iakora", OffsetX: 0.2, OffsetY: 0.15),
            (RegionIdx: 11, Nom: "Begogo", Code: "D36", Pop: 50_000, Pref: "Begogo", OffsetX: 0.1, OffsetY: 0.3),
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
    //  Communes (3 sous chaque département)
    // ═══════════════════════════════════════════
    private static List<Commune> CreerCommunes(List<Departement> departements)
    {
        var communes = new List<Commune>();
        int codeNum = 1;

        // Pour chaque département, créer 3 communes
        foreach (var dep in departements)
        {
            int idx = departements.IndexOf(dep);
            var noms = GetCommuneNames(idx);
            var pops = GetCommunePopulations(idx);

            for (int i = 0; i < noms.Length; i++)
            {
                double offX = i * 0.12;
                double offY = i * 0.10;
                communes.Add(new Commune
                {
                    Id = Guid.NewGuid(),
                    Nom = noms[i],
                    CodeAdministratif = $"C{codeNum:D3}",
                    Population = pops[i],
                    CodePostal = $"10{codeNum:D2}",
                    Contour = PolygoneSimple(
                        dep.Contour.EnvelopeInternal.MinY + offY,
                        dep.Contour.EnvelopeInternal.MinX + offX,
                        0.10, 0.12)
                });
                codeNum++;
            }
        }
        return communes;
    }

    private static string[] GetCommuneNames(int depIdx) => depIdx switch
    {
        0 => ["Antananarivo", "Ambohidratrimo", "Andohatapenaka"],
        1 => ["Ambositra", "Fandriana", "Ambatofinandrahana"],
        2 => ["Antananarivo-Sud", "Manjakandriana", "Ambohimanga"],
        3 => ["Toamasina", "Vohibinany", "Fénérive Est"],
        4 => ["Fénérive Est", "Mahanoro", "Brickaville"],
        5 => ["Mahanoro", "Nosy Varika", "Mananjary"],
        6 => ["Antsirabe", "Soaviaritra", "Betafo"],
        7 => ["Ambatolampy", "Morarano Gare", "Ankazobe"],
        8 => ["Ambositra-II", "Fandriana-II", "Isandra"],
        9 => ["Mahajanga", "Marovoay", "Mitsinjo"],
        10 => ["Ambanja", "Nosy Be", "Ambilobe"],
        11 => ["Maevatanana", "Betsileo", "Kandreho"],
        12 => ["Antsiranana", "Ramena", "Joffreville"],
        13 => ["Ambilobe", "Ampisikina", "Daraina"],
        14 => ["Sambava", "Andapa", "Befandriana"],
        15 => ["Ambositra-III", "Ilafay", "Fandriana-III"],
        16 => ["Fandriana-IV", "Miarinarivo", "Ambahatra"],
        17 => ["Ambatofinandrahana-II", "Sahavato", "Ifanadiana"],
        18 => ["Farafangana", "Mangatsiotra", " Ivato"],
        19 => ["Vangaindrano", "Manakara", "Kianjavato"],
        20 => ["Midongy Atsimo", "Bekily", "Taolagnaro"],
        21 => ["Fianarantsoa-II", "Ambohimasay", "Isorana-II"],
        22 => ["Ambalavao", "Begogo-II", "Ianakafy"],
        23 => ["Isorana-III", "Ampilanatra", "Fianarantsoa-Sud"],
        24 => ["Maintirano-II", "Antsalova-II", "Soalala-II"],
        25 => ["Antsalova-III", "Mitsinjo-II", "Marovoay-II"],
        26 => ["Soalala-III", "Boriziny", "Mampikony"],
        27 => ["Morondava-II", "Belon'i Tsiribihina", "Mahabo-II"],
        28 => ["Miandrivazo", "Satrokala", "Mandabe"],
        29 => ["Mahabo-III", "Benenitra", "Ehoala"],
        30 => ["Toliara-II", "Mahanarivo", "Toliara-Sud"],
        31 => ["Fort-Dauphin", "Sainte-Luce", "Lamboara"],
        32 => ["Amboasary Sud", "Berafia", "Mandena"],
        33 => ["Ihosy-II", "Iakora-II", "Begogo-III"],
        34 => ["Iakora-III", "Ivato-II", "Benenitra-II"],
        35 => ["Begogo-IV", "Isalo", "Zahavato"],
        _ => [$"Commune-{depIdx + 1}A", $"Commune-{depIdx + 1}B", $"Commune-{depIdx + 1}C"],
    };

    private static int[] GetCommunePopulations(int depIdx) => depIdx switch
    {
        0 => [1_275_000, 420_000, 180_000],
        1 => [150_000, 95_000, 80_000],
        2 => [350_000, 120_000, 210_000],
        3 => [350_000, 120_000, 95_000],
        4 => [95_000, 85_000, 60_000],
        5 => [110_000, 75_000, 90_000],
        6 => [250_000, 85_000, 65_000],
        7 => [80_000, 55_000, 40_000],
        8 => [70_000, 60_000, 50_000],
        9 => [220_000, 75_000, 45_000],
        10 => [45_000, 35_000, 55_000],
        11 => [30_000, 20_000, 15_000],
        12 => [130_000, 25_000, 15_000],
        13 => [55_000, 30_000, 20_000],
        14 => [40_000, 35_000, 25_000],
        15 => [50_000, 30_000, 25_000],
        16 => [45_000, 35_000, 20_000],
        17 => [40_000, 25_000, 30_000],
        18 => [80_000, 45_000, 35_000],
        19 => [60_000, 50_000, 40_000],
        20 => [30_000, 25_000, 20_000],
        21 => [190_000, 85_000, 55_000],
        22 => [40_000, 25_000, 18_000],
        23 => [35_000, 30_000, 45_000],
        24 => [25_000, 15_000, 12_000],
        25 => [18_000, 12_000, 10_000],
        26 => [14_000, 10_000, 8_000],
        27 => [60_000, 35_000, 20_000],
        28 => [25_000, 15_000, 10_000],
        29 => [18_000, 12_000, 8_000],
        30 => [100_000, 50_000, 30_000],
        31 => [45_000, 20_000, 15_000],
        32 => [25_000, 15_000, 10_000],
        33 => [40_000, 20_000, 12_000],
        34 => [18_000, 12_000, 8_000],
        35 => [10_000, 8_000, 6_000],
        _ => [20_000, 15_000, 10_000],
    };

    // ═══════════════════════════════════════════
    //  EPCI (groupements de communes — 10 EPCIs)
    // ═══════════════════════════════════════════
    private static List<Epci> CreerEpcis(List<Commune> communes)
    {
        var epcis = new List<Epci>();

        // EPCIs basés sur des paires de communes adjacentes
        var pairs = new (int A, int B, string Nom, string Code, string Nature)[]
        {
            (1, 2, "Communauté urbaine Antananarivo-Nord", "E01", "Communauté urbaine"),
            (4, 5, "Communauté urbaine Toamasina", "E02", "Communauté urbaine"),
            (7, 8, "District d'Antsirabe-II", "E03", "District"),
            (10, 11, "SIVOM Mahajanga", "E04", "SIVOM"),
            (13, 14, "Communauté Antsiranana-Nord", "E05", "Communauté de communes"),
            (16, 17, "SIVOM Ambositra", "E06", "SIVOM"),
            (19, 20, "Communauté Farafangana", "E07", "Communauté de communes"),
            (22, 23, "SIVOM Fianarantsoa", "E08", "SIVOM"),
            (28, 29, "Communauté Menabe", "E09", "Communauté de communes"),
            (31, 32, "District Anosy", "E10", "District"),
        };

        foreach (var (a, b, nom, code, nature) in pairs)
        {
            if (a >= communes.Count || b >= communes.Count) continue;
            var c1 = communes[a];
            var c2 = communes[b];
            epcis.Add(new Epci
            {
                Id = Guid.NewGuid(),
                Nom = nom,
                CodeAdministratif = code,
                Population = c1.Population + c2.Population,
                Siren = $"200{code[^2..]}{rng.Next(10000, 99999)}",
                Nature = nature,
                Contour = PolygoneSimple(
                    Math.Min(c1.Contour.EnvelopeInternal.MinY, c2.Contour.EnvelopeInternal.MinY) - 0.01,
                    Math.Min(c1.Contour.EnvelopeInternal.MinX, c2.Contour.EnvelopeInternal.MinX) - 0.01,
                    0.35, 0.45)
            });
        }
        return epcis;
    }

    // ═══════════════════════════════════════════
    //  Utilisateurs (10 comptes)
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
        new()
        {
            Id = Guid.NewGuid(), Nom = "Fanja Razakamanantsoa",
            Identifiant = "fanja", Role = Role.Administrateur,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Agent@1234"), Actif = true
        },
        new()
        {
            Id = Guid.NewGuid(), Nom = "Mamy Razafindrabe",
            Identifiant = "mamy", Role = Role.Agent,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Agent@1234"), Actif = true
        },
        new()
        {
            Id = Guid.NewGuid(), Nom = "Lalao Rasoamanarivo",
            Identifiant = "lalao", Role = Role.Agent,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Agent@1234"), Actif = true
        },
        new()
        {
            Id = Guid.NewGuid(), Nom = "Désiré Rakotondrasoa",
            Identifiant = "desire", Role = Role.Agent,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Agent@1234"), Actif = true
        },
        new()
        {
            Id = Guid.NewGuid(), Nom = "Voahangy Ratsimbazafy",
            Identifiant = "voahangy", Role = Role.Agent,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Agent@1234"), Actif = true
        },
        new()
        {
            Id = Guid.NewGuid(), Nom = "Jean Paul Rabenirina",
            Identifiant = "jprabenirina", Role = Role.Administrateur,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Agent@1234"), Actif = false
        },
    ];

    // ═══════════════════════════════════════════
    //  Projets & Dotations (60+)
    // ═══════════════════════════════════════════
    private static List<ProjetDotation> CreerProjets(List<Commune> communes)
    {
        var projets = new List<ProjetDotation>();
        var rng = new Random(42);

        var specs = new (string Intitule, decimal Montant, StatutProjet Statut)[]
        {
            // ── Infrastructures routières (15) ──
            ("Réhabilitation RN7 Antananarivo – Toamasina", 45_000_000m, StatutProjet.EnCours),
            ("Route communale Ambositra – Fandriana", 55_000_000m, StatutProjet.EnCours),
            ("Voirie urbaine Ambalavao", 25_000_000m, StatutProjet.EnCours),
            ("Pavage rue Ranaivo Antsirabe", 38_000_000m, StatutProjet.EnCours),
            ("Bitumage route Mahajanga – Mitsinjo", 62_000_000m, StatutProjet.EnPreparation),
            ("Route nationale RN12 Toliara – Fort-Dauphin", 85_000_000m, StatutProjet.EnPreparation),
            ("Réhabilitation route Ambanja – Nosy Be", 42_000_000m, StatutProjet.EnCours),
            ("Voirie communale Sambava", 18_000_000m, StatutProjet.Termine),
            ("Route rurale Ihosy – Iakora", 30_000_000m, StatutProjet.EnPreparation),
            ("Pont rivière Tsiribihina Morondava", 55_000_000m, StatutProjet.EnCours),
            ("Bitumage avenue 26 Juin Antsiranana", 28_000_000m, StatutProjet.Termine),
            ("Route forestière Maintirano – Antsalova", 22_000_000m, StatutProjet.EnPreparation),
            ("Pavage place centrale Fianarantsoa", 15_000_000m, StatutProjet.Termine),
            ("Route côtière Mahanoro – Brickaville", 48_000_000m, StatutProjet.EnCours),
            ("Chaussée effondrée Fandriana centre", 12_000_000m, StatutProjet.Termine),

            // ── Éducation (12) ──
            ("Construction école primaire Ambohidratrimo", 85_000_000m, StatutProjet.EnCours),
            ("Bibliothèque universitaire Antananarivo", 40_000_000m, StatutProjet.Termine),
            ("Collège Ambanja", 42_000_000m, StatutProjet.EnPreparation),
            ("Réhabilitation lycée Toamasina", 52_000_000m, StatutProjet.Termine),
            ("École maternelle Mahajanga", 28_000_000m, StatutProjet.EnCours),
            ("Lycée professionnel Antsirabe", 65_000_000m, StatutProjet.EnPreparation),
            ("Construction CP privacy Sambava", 35_000_000m, StatutProjet.Termine),
            ("École primaire Fort-Dauphin", 30_000_000m, StatutProjet.EnCours),
            ("Réhabilitation collège Toliara", 45_000_000m, StatutProjet.EnPreparation),
            ("Bibliothèque communale Fianarantsoa", 22_000_000m, StatutProjet.Termine),
            ("École technique Miandrivazo", 38_000_000m, StatutProjet.EnCours),
            ("Centre de formation Ambositra", 25_000_000m, StatutProjet.Termine),

            // ── Eau & assainissement (10) ──
            ("Adduction eau potable Antsirabe", 120_000_000m, StatutProjet.EnPreparation),
            ("Assainissement Nosy Be", 75_000_000m, StatutProjet.EnPreparation),
            ("Potable eau Fénérive Est", 68_000_000m, StatutProjet.Termine),
            ("Réseau d'assainissement Mahajanga", 95_000_000m, StatutProjet.EnCours),
            ("Adduction eau Fort-Dauphin", 80_000_000m, StatutProjet.EnCours),
            ("Assainissement Sambava", 45_000_000m, StatutProjet.Termine),
            ("Réseau eau potable Ihosy", 55_000_000m, StatutProjet.EnPreparation),
            ("Fosses septiques communautaires Morondava", 35_000_000m, StatutProjet.EnCours),
            ("Station de traitement eau Toliara", 110_000_000m, StatutProjet.EnPreparation),
            ("Canalisation Ambositra", 28_000_000m, StatutProjet.Termine),

            // ── Santé (8) ──
            ("Centre de santé Fianarantsoa", 95_000_000m, StatutProjet.EnCours),
            ("Clinique communale Antsiranana", 78_000_000m, StatutProjet.EnPreparation),
            ("Hôpital district Ambositra", 150_000_000m, StatutProjet.EnCours),
            ("Poste de santé Sambava", 25_000_000m, StatutProjet.Termine),
            ("Centre médical Mahajanga", 85_000_000m, StatutProjet.EnPreparation),
            ("Pharmacie communale Ihosy", 15_000_000m, StatutProjet.Termine),
            ("Maternité Morondava", 45_000_000m, StatutProjet.EnCours),
            ("Dispensaire Ambanja", 30_000_000m, StatutProjet.Termine),

            // ── Marchés & équipements (8) ──
            ("Marché couvert Toamasina", 60_000_000m, StatutProjet.Termine),
            ("Marché artisanal Ambositra", 28_000_000m, StatutProjet.Termine),
            ("Stade municipal Antsiranana", 150_000_000m, StatutProjet.EnPreparation),
            ("Marché municipal Mahajanga", 42_000_000m, StatutProjet.EnCours),
            ("Centre commercial Fianarantsoa", 65_000_000m, StatutProjet.EnPreparation),
            ("Gare routière Antsirabe", 35_000_000m, StatutProjet.Termine),
            ("Marché poisson Fort-Dauphin", 20_000_000m, StatutProjet.EnCours),
            ("Terrain sportif Sambava", 18_000_000m, StatutProjet.Termine),

            // ── Éclairage & énergie (7) ──
            ("Réseau d'éclairage public Mahajanga", 32_000_000m, StatutProjet.Termine),
            ("Éclairage solaire Ambatolampy", 18_000_000m, StatutProjet.EnCours),
            ("Éclairage avenue Indépendance Toamasina", 25_000_000m, StatutProjet.EnCours),
            ("Panneaux solaires Sambava", 40_000_000m, StatutProjet.EnPreparation),
            ("Éclairage communautaire Maintirano", 12_000_000m, StatutProjet.Termine),
            ("Borne solaire Miandrivazo", 15_000_000m, StatutProjet.EnCours),
            ("Éclairage route nationale Toliara", 28_000_000m, StatutProjet.EnPreparation),

            // ── Dotations (8) ──
            ("Dotation de fonctionnement Mahajanga", 50_000_000m, StatutProjet.Termine),
            ("Dotation d'investissement Fianarantsoa", 35_000_000m, StatutProjet.Termine),
            ("Subvention école privée Antananarivo", 22_000_000m, StatutProjet.Termine),
            ("Dotation urgence cyclone Morondava", 80_000_000m, StatutProjet.Termine),
            ("Subvention cooperative agricole Ambositra", 15_000_000m, StatutProjet.Termine),
            ("Dotationroute communale Toliara", 30_000_000m, StatutProjet.EnCours),
            ("Subvention marché artisanal Antsiranana", 12_000_000m, StatutProjet.Termine),
            ("Dotation équipement Ihosy", 18_000_000m, StatutProjet.Termine),
        };

        foreach (var (intitule, montant, statut) in specs)
        {
            var commune = communes[rng.Next(communes.Count)];
            projets.Add(new ProjetDotation
            {
                Id = Guid.NewGuid(),
                Intitule = intitule,
                Montant = montant,
                Devise = "MGA",
                Statut = statut,
                DateDebut = DateTime.UtcNow.AddDays(-rng.Next(30, 365)),
                DateFin = statut == StatutProjet.Termine ? DateTime.UtcNow.AddDays(-rng.Next(1, 30)) : DateTime.UtcNow.AddDays(rng.Next(60, 540)),
                CollectiviteId = commune.Id
            });
        }
        return projets;
    }

    // ═══════════════════════════════════════════
    //  Indicateurs (300+)
    // ═══════════════════════════════════════════
    private static List<Indicateur> CreerIndicateurs(List<Commune> communes)
    {
        var indicateurs = new List<Indicateur>();
        var rng = new Random(42);

        var types = new (string Type, string Unite, string Source, decimal Min, decimal Max)[]
        {
            ("Taux de scolarisation", "%", "INSTAT", 65m, 85m),
            ("Taux d'accès à l'eau potable", "%", "DREDD", 30m, 60m),
            ("Taux d'accès à l'électricité", "%", "JIRAMA", 15m, 45m),
            ("Nombre d'infrastructures sanitaires", "unité", "DPS", 2m, 12m),
            ("Longueur routes bitumées", "km", "DTR", 5m, 35m),
            ("Superficie agricole", "ha", "DRIAE", 500m, 5000m),
            ("Densité de population", "hab/km²", "INSTAT", 50m, 800m),
            ("Taux de pauvreté", "%", "INSTAT", 40m, 80m),
            ("Taux d'alphabétisation", "%", "INSTAT", 55m, 90m),
            ("Nombre de marchés", "unité", "DREAL", 1m, 5m),
            ("Superficie urbanisée", "ha", "DPU", 100m, 2000m),
            ("Nombre de ponts", "unité", "DTR", 0m, 8m),
            ("Taux de couverture vaccinale", "%", "DPS", 60m, 90m),
            ("Nombre d'agents de santé", "unité", "DPS", 5m, 30m),
            ("Production agricole annuelle", "tonnes", "DRIAE", 100m, 5000m),
            ("Taux de chômage", "%", "INSTAT", 15m, 45m),
            ("Nombre d'entreprises", "unité", "DIEC", 10m, 200m),
            ("Superficie forestière", "ha", "DREF", 500m, 50000m),
            ("Nombre d'écoles", "unité", "MINED", 5m, 40m),
            ("Taux de rationnement en eau", "%", "ONE", 20m, 70m),
        };

        foreach (var commune in communes)
        {
            // 8-12 indicateurs par commune (plus qu'avant)
            var nb = rng.Next(8, 13);
            var selected = types.OrderBy(_ => rng.Next()).Take(nb);
            foreach (var (type, unite, source, min, max) in selected)
            {
                // 2-3 relevés par type de date différentes
                var nbReleves = rng.Next(1, 4);
                for (int r = 0; r < nbReleves; r++)
                {
                    indicateurs.Add(new Indicateur
                    {
                        Id = Guid.NewGuid(),
                        Type = type,
                        Valeur = Math.Round(min + (decimal)rng.NextDouble() * (max - min), 2),
                        Unite = unite,
                        Source = source,
                        DateReleve = DateTime.UtcNow.AddDays(-rng.Next(1, 365)),
                        CollectiviteId = commune.Id
                    });
                }
            }
        }
        return indicateurs;
    }

    // ═══════════════════════════════════════════
    //  Litiges de limites (25)
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
            "Limite contestée le long de la route nationale RN7 — chaque commune revendique la juridiction.",
            "Chevauchement de parcelles forestières classées entre deux communes limitrophes.",
            "Zone industrielle contestée — attribution floue entre deux collectivités.",
            "Limite floue autour du lac communal — pêcheurs des deux côtés se disputent l'accès.",
            "Conflit sur la gestion du marché couvert — juridiction partagée non définie.",
            "Chevauchement des zones de pêche lacustre entre deux communes riveraines.",
            "Litige frontalier sur les terres de la vallée de l'Onilahy — bornes disparues.",
            "Zone de conflit autour du carrefour RN7/RN35 — deux communes se disputent le point.",
            "Limite contestée le long de la rivière Mananjary — cours d'eau mal délimité.",
            "Chevauchement des zones d'activité économique entre deux EPCI limitrophes.",
            "Conflit sur la juridiction du port de pêche — deux communes revendiquent l'autorité.",
            "Zone forestière contestée — limite ancienne non respectée lors de la démarcation moderne.",
            "Litige sur les terrains communalisés du quartier analakely — bornes contradictoires.",
            "Chevauchement des périmètres d'assainissement entre deux communes urbaines.",
            "Conflit de limite autour du terrain communal sportif — attribution contestée.",
            "Zone de conflit le long de la route nationale RN12 — chaque commune revendique la route.",
            "Limite floue autour du marché artisanal — juridiction partagée non définie.",
            "Chevauchement des zones d'irrigation entre deux communes agricoles.",
            "Litige sur la gestion du cimetière communal — deux communes se disputent le terrain.",
        };

        // Paires de communes adjacentes (25 paires)
        var paires = new[]
        {
            (0, 1), (2, 3), (4, 5), (6, 7), (8, 9),
            (10, 11), (12, 13), (14, 15), (16, 17), (18, 19),
            (20, 21), (22, 23), (24, 25), (26, 27), (28, 29),
            (30, 31), (32, 33), (34, 35), (36, 37), (38, 39),
            (40, 41), (42, 43), (44, 45), (46, 47), (48, 49),
        };

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
                DateCreation = DateTime.UtcNow.AddDays(-rng.Next(10, 300)),
                Geometrie = Gf.CreatePoint(centre),
                ZoneConflit = zoneConflit,
                CollectiviteAId = communes[a].Id,
                CollectiviteBId = communes[b].Id
            });
        }
        return litiges;
    }

    // ═══════════════════════════════════════════
    //  Doléances citoyennes (50)
    // ═══════════════════════════════════════════
    private static List<Doleance> CreerDoleances(List<Commune> communes)
    {
        var doleances = new List<Doleance>();
        var rng = new Random(42);

        var specs = new (string Desc, CategorieDoleance Cat, StatutDoleance Stat, string Auteur)[]
        {
            // ── Voirie (12) ──
            ("Route nationale RN7 en mauvais état entre Ambositra et Fandriana, nids-de-poule dangereux.", CategorieDoleance.Voirie, StatutDoleance.Nouveau, "Andry R."),
            ("Pont en bois dangereux sur la rivière Tsiribihina, risque d'effondrement.", CategorieDoleance.Voirie, StatutDoleance.Nouveau, "Désiré K."),
            ("Trottoirs inexistants rue Ranaivo, les piétons marchent sur la chaussée.", CategorieDoleance.Voirie, StatutDoleance.EnCours, "Paul B."),
            ("Feu rouge en panne à la carrefour RN7/RN35, accidents fréquents.", CategorieDoleance.Voirie, StatutDoleance.EnCours, "Hanta T."),
            ("Chaussée effondrée avenue du 26 Juin, circulation impossible.", CategorieDoleance.Voirie, StatutDoleance.Resolu, "Lova M."),
            ("Route communale Ambositra – Fandriana impraticable pendant la saison des pluies.", CategorieDoleance.Voirie, StatutDoleance.Nouveau, "Nirina R."),
            ("Caniveau bouché rue du marché, inondation à chaque pluie.", CategorieDoleance.Voirie, StatutDoleance.EnCours, "Fara A."),
            ("Panneau de signalisation défectueux à la sortie de Mahajanga.", CategorieDoleance.Voirie, StatutDoleance.Resolu, "Jean R."),
            ("Trottoirs dégradés avenue Indépendance Toamasina.", CategorieDoleance.Voirie, StatutDoleance.Nouveau, "Claude B."),
            ("Pont suspendu dangereux sur la rivière Mananjary.", CategorieDoleance.Voirie, StatutDoleance.EnCours, "Hélène P."),
            ("Chaussée dangereuse route nationale Toliara – Fort-Dauphin.", CategorieDoleance.Voirie, StatutDoleance.Nouveau, "Patrick V."),
            ("Feu de signalisation en panne carrefour Analakely.", CategorieDoleance.Voirie, StatutDoleance.Resolu, "Michel D."),

            // ── Éclairage (10) ──
            ("Éclairage public défectueux avenue de l'Indépendance, zone dangereuse la nuit.", CategorieDoleance.Eclairage, StatutDoleance.EnCours, "Fara M."),
            ("Éclairage public installé mais jamais allumé depuis 6 mois.", CategorieDoleance.Eclairage, StatutDoleance.Nouveau, "Lalao A."),
            ("Lampadaires cassés sur toute la rue du Marché, quartier plongé dans l'obscurité.", CategorieDoleance.Eclairage, StatutDoleance.Resolu, "Nirina B."),
            ("Éclairage solaire défectueux à Ambatolampy, panneaux vandalisés.", CategorieDoleance.Eclairage, StatutDoleance.Nouveau, "Tovo K."),
            ("Lampadaire tombé sur la chaussée rue Principale Sambava.", CategorieDoleance.Eclairage, StatutDoleance.EnCours, "Rija T."),
            ("Éclairage insuffisant place du marché Mahajanga, risques d'insécurité.", CategorieDoleance.Eclairage, StatutDoleance.Nouveau, "Fanja L."),
            ("Réseau électrique défectueux quartier Analakely, pannes fréquentes.", CategorieDoleance.Eclairage, StatutDoleance.EnCours, "Mamy R."),
            ("Éclairage public manquant route d'accès au port Toamasina.", CategorieDoleance.Eclairage, StatutDoleance.Resolu, "Désiré K."),
            ("Panneaux solaires démontés et volés à Ihosy.", CategorieDoleance.Eclairage, StatutDoleance.Nouveau, "Voahangy R."),
            ("Éclairage public intermittent avenue du 26 Juin Antsiranana.", CategorieDoleance.Eclairage, StatutDoleance.EnCours, "Hery R."),

            // ── Environnement (10) ──
            ("Décharge sauvage près du marché, pollution et odeurs insupportables.", CategorieDoleance.Environnement, StatutDoleance.Resolu, "Jean R."),
            ("Arbre menaçant de tomber sur la route principale, pas d'intervention depuis 2 semaines.", CategorieDoleance.Environnement, StatutDoleance.EnCours, "Robert T."),
            ("Dépotoir sauvage au bord de la rivière, eaux contaminées.", CategorieDoleance.Environnement, StatutDoleance.Resolu, "Njiva H."),
            ("Poubelle municipale renversée depuis une semaine, déchets sur la voie publique.", CategorieDoleance.Environnement, StatutDoleance.Nouveau, "Fidy M."),
            ("Déforestation illégale dans la forêt communale Ambanja.", CategorieDoleance.Environnement, StatutDoleance.EnCours, "Claude B."),
            ("Pollution industrielle rivière Amboniloha, poissons morts signalés.", CategorieDoleance.Environnement, StatutDoleance.Nouveau, "Hélène P."),
            ("Feu de brousse non maîtrisé près du village Miandrivazo.", CategorieDoleance.Environnement, StatutDoleance.Resolu, "Paul B."),
            ("Déchets plastiques accumulés sur la plage de Mahavelona.", CategorieDoleance.Environnement, StatutDoleance.EnCours, "Fara A."),
            ("Érosion des berges rivière Fandriana, risque d'effondrement.", CategorieDoleance.Environnement, StatutDoleance.Nouveau, "Jean R."),
            ("Coupe illégale de bois dans la forêt classée Maintirano.", CategorieDoleance.Environnement, StatutDoleance.EnCours, "Désiré K."),

            // ── Assainissement (10) ──
            ("Fuite d'eau potable depuis 3 jours dans le quartier Tanambao.", CategorieDoleance.Assainissement, StatutDoleance.EnCours, "Hélène P."),
            ("Canal d'assainissement bouché, eaux stagnantes et risque sanitaire.", CategorieDoleance.Assainissement, StatutDoleance.Nouveau, "Sylvie R."),
            ("Fossé de drainage comblé, inondation à chaque pluie.", CategorieDoleance.Assainissement, StatutDoleance.EnCours, "Patrick V."),
            ("Fosse septique débordante au quartier Analakely, odeurs insupportables.", CategorieDoleance.Assainissement, StatutDoleance.Nouveau, "Tovo K."),
            ("Réseau d'assainissement vétuste Mahajanga, débordements fréquents.", CategorieDoleance.Assainissement, StatutDoleance.EnCours, "Mamy R."),
            ("Adduction eau potable coupée depuis 48h à Fénérive Est.", CategorieDoleance.Assainissement, StatutDoleance.Nouveau, "Lalao A."),
            ("Égout débordant rue Principale Antsirabe.", CategorieDoleance.Assainissement, StatutDoleance.Resolu, "Nirina B."),
            ("Station de traitement eau à saturation à Sambava.", CategorieDoleance.Assainissement, StatutDoleance.Nouveau, "Fanja L."),
            ("Fuite canalisation principale Ambositra, perte d'eau massive.", CategorieDoleance.Assainissement, StatutDoleance.EnCours, "Rija T."),
            ("Eaux usées rejetées rivière Ihosy, contamination.", CategorieDoleance.Assainissement, StatutDoleance.Nouveau, "Voahangy R."),

            // ── Autre (8) ──
            ("Panne de courant électrique récurrente dans tout le quartier Analakely.", CategorieDoleance.Autre, StatutDoleance.Nouveau, "Marie N."),
            ("École sans fenêtres depuis la tempête, enfants exposés.", CategorieDoleance.Autre, StatutDoleance.Nouveau, "Clément R."),
            ("Signalétique de rue inexistante, les secours ont du mal à localiser les adresses.", CategorieDoleance.Autre, StatutDoleance.EnCours, "Fanja L."),
            ("Clôture du terrain communal vandalisée, terrain envahi par des bâtisseurs clandestins.", CategorieDoleance.Autre, StatutDoleance.Resolu, "Michel D."),
            ("Service d'état civil défaillant, documents en retard depuis des semaines.", CategorieDoleance.Autre, StatutDoleance.Nouveau, "Hery R."),
            ("Bibliothèque municipale fermée depuis 3 mois, pas de personnel.", CategorieDoleance.Autre, StatutDoleance.EnCours, "Naina R."),
            ("Parking municipal non entretenu, nids-de-poule partout.", CategorieDoleance.Autre, StatutDoleance.Nouveau, "Tojo R."),
            ("Service technique municipale en panne d'équipement.", CategorieDoleance.Autre, StatutDoleance.Resolu, "Mamy R."),
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
                DateCreation = DateTime.UtcNow.AddDays(-rng.Next(1, 180)),
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
        var auteurs = new[] { "Rija Andrianarivelo", "Hery Rabearimanana", "Naina Razafindrabe", "Tojo Ramanantsoa", "Fanja Razakamanantsoa", "Mamy Razafindrabe" };

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
            if (l.Statut == StatutLitige.Clos)
            {
                entries.Add(new Historique
                {
                    Id = Guid.NewGuid(),
                    Entite = "Litige",
                    EntiteId = l.Id,
                    Action = "changement de statut : Arbitré → Clos",
                    Auteur = "Rija Andrianarivelo",
                    Date = l.DateCreation.AddDays(rng.Next(60, 120))
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

    private static readonly Random rng = new(42);
}
