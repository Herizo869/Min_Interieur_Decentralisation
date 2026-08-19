# 🗺️ Collectivités Territoriales — Madagascar

Application de gestion des collectivités territoriales de Madagascar avec carte interactive, doléances citoyennes, litiges de limites et exports CSV.

---

## 📋 Sommaire

1. [Prérequis](#1--prérequis)
2. [Base de données PostgreSQL + PostGIS](#2--base-de-données-postgresql--postgis)
3. [Backend API (.NET 8)](#3--backend-api-net-8)
4. [Frontend (React + Vite)](#4--frontend-react--vite)
5. [Lancement rapide (Docker)](#5--lancement-rapide-docker)
6. [Comptes par défaut](#6--comptes-par-défaut)
7. [Routes de l'application](#7--routes-de-lapplication)
8. [API Backend](#8--api-backend)
9. [Architecture du projet](#9--architecture-du-projet)

---

## 1 — Prérequis

| Outil | Version minimale | Vérification |
|-------|-----------------|--------------|
| .NET SDK | 8.0 | `dotnet --version` |
| Node.js | 18+ | `node --version` |
| npm | 9+ | `npm --version` |
| PostgreSQL | 14+ | `psql --version` |
| PostGIS | 3.x | `psql -U postgres -d collectivites -c "SELECT PostGIS_Version();"` |
| Docker Desktop *(optionnel)* | 24+ | `docker --version` |

---

## 2 — Base de données PostgreSQL + PostGIS

### Option A : PostgreSQL installé en local

```bash
# 1. Créer la base de données
psql -U postgres -c "CREATE DATABASE collectivites;"

# 2. Ajouter l'extension PostGIS
psql -U postgres -d collectivites -c "CREATE EXTENSION IF NOT EXISTS postgis;"

# 3. Vérifier PostGIS
psql -U postgres -d collectivites -c "SELECT PostGIS_Version();"
```

> ⚠️ Adapte l'utilisateur (`postgres`) et le mot de passe selon ta configuration.

### Option B : Via Docker (recommandé)

```bash
# Lancer PostgreSQL + PostGIS en conteneur
docker run -d \
  --name collectivites-db \
  -e POSTGRES_DB=collectivites \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=1234 \
  -p 5433:5432 \
  postgis/postgis:16-3.4

# Vérifier que le conteneur tourne
docker ps

# Tester la connexion
psql -U postgres -h localhost -p 5433 -d collectivites -c "SELECT PostGIS_Version();"
```

### Chaîne de connexion

Le fichier `backend/appsettings.json` contient :

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5433;Database=collectivites;Username=postgres;Password=1234"
}
```

> Adapte le port, l'utilisateur et le mot de passe si ta config est différente.

---

## 3 — Backend API (.NET 8)

```bash
cd backend

# Restaurer les packages NuGet
dotnet restore

# Lancer l'API (port 5242)
dotnet run
```

> Au premier démarrage :
> - Les migrations sont appliquées automatiquement (`MigrateAsync`)
> - Un utilisateur **Administrateur** est créé par défaut
> - Swagger est disponible en mode développement

| Ressource | URL |
|-----------|-----|
| API | http://localhost:5242 |
| Swagger UI | http://localhost:5242/swagger |

---

## 4 — Frontend (React + Vite)

```bash
cd frontend

# Installer les dépendances
npm install

# Lancer en mode développement (port 5173)
npm run dev
```

| Commande | Description |
|----------|-------------|
| `npm run dev` | Serveur de développement (HMR) |
| `npm run build` | Build de production |
| `npm run preview` | Aperçu du build de production |
| `npm run lint` | Linting avec oxlint |

| Ressource | URL |
|-----------|-----|
| Frontend | http://localhost:5173 |
| Doléance publique | http://localhost:5173/doleances/deposer |

---

## 5 — Lancement rapide (Docker)

### Lancer PostgreSQL via Docker

```bash
docker run -d \
  --name collectivites-db \
  -e POSTGRES_DB=collectivites \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=1234 \
  -p 5433:5432 \
  postgis/postgis:16-3.4
```

### Lancer Backend + Frontend

```bash
# Terminal 1 — Backend
cd backend && dotnet run

# Terminal 2 — Frontend
cd frontend && npm run dev
```

### Arrêter le conteneur

```bash
docker stop collectivites-db
docker start collectivites-db   # pour le relancer
docker rm collectivites-db      # pour le supprimer
```

---

## 6 — Comptes par défaut

| Rôle | Identifiant | Mot de passe |
|------|-------------|--------------|
| **Administrateur** | `admin` | `Admin@1234` |

> Ce compte est créé automatiquement au premier démarrage du backend.

---

## 7 — Routes de l'application

### Routes publiques (sans authentification)

| Route | Description |
|-------|-------------|
| `/login` | Connexion |
| `/forgot-password` | Mot de passe oublié |
| `/reset-password` | Réinitialisation du mot de passe |
| `/doleances/deposer` | Dépôt d'une doléance citoyenne |

### Routes protégées (authentification requise)

| Route | Description |
|-------|-------------|
| `/dashboard` | Tableau de bord |
| `/collectivites` | Carte interactive des collectivités |
| `/collectivites/:id` | Fiche détaillée d'une collectivité |
| `/projets` | Gestion des projets & dotations |
| `/indicateurs` | Gestion des indicateurs chiffrés |
| `/litiges` | Litiges de limites territoriales |
| `/doleances` | Liste des doléances (agent/admin) |
| `/exports` | Exports CSV |
| `/utilisateurs` | Gestion des utilisateurs (admin) |

---

## 8 — API Backend

### Authentification

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| POST | `/api/Auth/login` | Connexion, retourne un JWT |
| POST | `/api/Auth/forgot-password` | Demande de réinitialisation |
| POST | `/api/Auth/reset-password` | Réinitialisation du mot de passe |

### Collectivités

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/api/Collectivites` | Liste (filtre : recherche, type) |
| GET | `/api/Collectivites/{id}` | Fiche détaillée |
| GET | `/api/Collectivites/geojson` | Données GeoJSON pour la carte |

### Projets & Dotations

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/api/ProjetsDotations` | Liste (filtre : collectiviteId) |
| GET | `/api/ProjetsDotations/{id}` | Fiche |
| POST | `/api/ProjetsDotations` | Créer |
| PUT | `/api/ProjetsDotations/{id}` | Modifier |
| DELETE | `/api/ProjetsDotations/{id}` | Supprimer |

### Indicateurs

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/api/Indicateurs` | Liste (filtres : collectiviteId, type) |
| GET | `/api/Indicateurs/{id}` | Fiche |
| POST | `/api/Indicateurs` | Créer |
| PUT | `/api/Indicateurs/{id}` | Modifier |
| DELETE | `/api/Indicateurs/{id}` | Supprimer |

### Litiges de limites

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/api/Litiges` | Liste (filtres : collectiviteId, statut) |
| GET | `/api/Litiges/{id}` | Fiche avec zone de conflit |
| POST | `/api/Litiges` | Signaler un litige |
| PUT | `/api/Litiges/{id}/statut` | Changer le statut |
| POST | `/api/Litiges/detecter` | Détection auto des chevauchements (admin) |

### Doléances citoyennes

| Méthode | Endpoint | Auth | Description |
|---------|----------|------|-------------|
| POST | `/api/Doleances` | ❌ | Déposer une doléance (public) |
| GET | `/api/Doleances/suivi/{numero}` | ❌ | Suivi par numéro (public) |
| GET | `/api/Doleances` | ✅ | Liste (agent/admin) |
| GET | `/api/Doleances/{id}` | ✅ | Fiche (agent/admin) |
| PUT | `/api/Doleances/{id}/statut` | ✅ | Changer le statut |

### Exports CSV

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/api/Exports?resource=projets` | Export projets (ou litiges, indicateurs, doleances) |

### Utilisateurs (admin)

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/api/Utilisateurs` | Liste |
| POST | `/api/Utilisateurs` | Créer |
| PUT | `/api/Utilisateurs/{id}` | Modifier |
| DELETE | `/api/Utilisateurs/{id}` | Supprimer |

---

## 9 — Architecture du projet

```
├── backend/                    # API .NET 8
│   ├── Controllers/            # Routes API
│   ├── Data/                   # DbContext Entity Framework
│   ├── Migrations/             # Migrations EF Core
│   ├── Models/
│   │   ├── Dtos/               # DTOs requête/réponse
│   │   ├── Entities/           # Entités EF Core
│   │   ├── Enums/              # Énumérations
│   │   └── Options/            # Options de configuration
│   ├── Services/               # Logique métier
│   └── Program.cs              # Point d'entrée
│
├── frontend/                   # React 19 + Vite + TypeScript
│   ├── src/
│   │   ├── components/layout/  # Layout, Header, Sidebar
│   │   ├── contexts/           # AuthContext
│   │   ├── hooks/              # Hooks API (useProjets, useLitiges...)
│   │   ├── pages/
│   │   │   ├── auth/           # Login, ForgotPassword, ResetPassword
│   │   │   ├── collectivites/  # Carte Leaflet + Fiche détaillée
│   │   │   ├── projets/        # CRUD Projets & Dotations
│   │   │   ├── indicateurs/    # CRUD Indicateurs
│   │   │   ├── litiges/        # Litiges + Carte zone conflit
│   │   │   ├── doleances/      # Dépôt public + Liste agent
│   │   │   ├── exports/        # Exports CSV
│   │   │   ├── dashboard/      # Tableau de bord
│   │   │   └── utilisateurs/   # Gestion admin
│   │   ├── services/           # Axios API client
│   │   └── index.css           # Styles globaux
│   └── package.json
│
└── README.md
```

---

## 🛠️ Commandes utiles

```bash
# Backend — Build
cd backend && dotnet build

# Backend — Lancer les tests
cd backend && dotnet test

# Frontend — Build production
cd frontend && npm run build

# Frontend — Lint
cd frontend && npm run lint

# Docker — Arrêter le conteneur DB
docker stop collectivites-db

# Docker — Supprimer le conteneur DB
docker rm -f collectivites-db
```

---

## 📝 Notes techniques

- **Backend** : .NET 8, Entity Framework Core, PostgreSQL + PostGIS, JWT auth, BCrypt
- **Frontend** : React 19, TypeScript, Vite 8, React Router 7, Leaflet, Recharts, Zod
- **Carte** : Leaflet avec GeoJSON, couche OpenStreetMap
- **Export CSV** : Séparateur `;`, encodage UTF-8 avec BOM (compatible Excel FR)
- **Doléance** : Rattachement automatique à la collectivité via `ST_Contains` (PostGIS)
- **Litiges** : Détection automatique des chevauchements via `ST_Intersects` / `ST_Intersection`
