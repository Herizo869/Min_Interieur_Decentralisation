# Collectivites.Api

API ASP.NET Core 8 de la plateforme de gestion, suivi et visualisation géographique
des collectivités territoriales (chapitre 6 du cahier des charges).

## Architecture

```
Controllers / Services / Data
├── Controllers/   → Points d'entrée HTTP (CollectivitesController…)
├── Services/      → Logique métier (CollectiviteService…)
├── Data/          → AppDbContext (EF Core + PostGIS)
└── Models/
    ├── Entities/  → Collectivité (Commune/Département/Région/EPCI),
    │                Signalement (Litige/Doléance), ProjetDotation,
    │                Indicateur, Utilisateur, Historique
    └── Enums/     → Rôles, statuts, catégories
```

## Prérequis

- .NET SDK 8 (ou supérieur, rétrocompatible)
- PostgreSQL avec l'extension PostGIS :
  - soit local (PostgreSQL 18 installé sur la machine de dev),
  - soit via Docker : `docker compose up -d` à la racine du dépôt.

## Configuration

La chaîne de connexion se trouve dans `appsettings.json` :
`Host=localhost;Port=5432;Database=collectivites;Username=postgres;Password=...`

## Démarrage

```bash
cd backend
dotnet run
```

Swagger (documentation de l'API) : https://localhost:PORT/swagger

## Base de données

- **Migration initiale déjà générée** : `Migrations/InitialCreate` (création de
  l'extension PostGIS + 7 tables, géométries SRID 4326).
- Pour installer PostgreSQL + PostGIS via Docker (WSL2 requis) : voir
  [`docs/SETUP_DOCKER_WSL2.md`](../docs/SETUP_DOCKER_WSL2.md).
- Une fois la base disponible : `dotnet ef database update`

## État d'avancement (MVP)

- ✅ Squelette ASP.NET Core 8 (architecture Controllers / Services / Data)
- ✅ Contexte EF Core + Npgsql + NetTopologySuite (PostGIS)
- ✅ Entités du modèle de données (chapitre 4) + énumérations
- ✅ Migration initiale générée
- ✅ Recherche de collectivités (UC-04) et fiche collectivité (UC-03)
- ✅ Import du référentiel (UC-05) : GeoJSON seul (création/mise à jour par CodeAdministratif,
  rapport d'erreurs par ligne) — Shapefile non supporté
- ✅ Projets & dotations (UC-06) : CRUD complet (montant, devise, statut, dates),
  filtre par collectivité — réservé aux utilisateurs authentifiés
- ✅ Indicateurs (UC-07) : CRUD complet (type, valeur, unité, source, dateRelevé),
  filtres par collectivité et par type
- ✅ Doléances citoyennes (UC-11/12) : dépôt public géolocalisé, rattachement
  automatique ST_Contains, numéro de suivi, traitement par statut, traçabilité (Historique)
- ✅ Litiges (UC-14/10) : signalement manuel, calcul automatique de la zone de conflit
  (intersection NTS), traitement par statut, traçabilité (Historique)
- ⏳ Détection automatique des litiges (UC-09), exports (UC-08)
- ✅ Authentification (UC-01) : login JWT + bcrypt, seed admin (admin / Admin@1234)
- ✅ Gestion des utilisateurs (UC-02) : CRUD comptes, rôles, désactivation,
  changement de mot de passe, périmètre d'accès — réservé au rôle Administrateur
- ⏳ Réinitialisation mot de passe (UC-13)
- ⏳ Tableau de bord (UC-15)
- ⏳ Frontend React + TypeScript + Leaflet/MapLibre
