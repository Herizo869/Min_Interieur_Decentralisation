import { useState, useEffect, useCallback } from 'react'
import {
  MessageSquareWarning, Search, Filter, ExternalLink, CheckCircle, Clock,
  AlertTriangle, Loader2, X, RefreshCw, MapPin, Copy, Check, Table, Map
} from 'lucide-react'
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { useDoleances, CATEGORIES, type Doleance } from '../../hooks/useDoleances'
import { useCollectivites, type Collectivite } from '../../hooks/useCollectivites'

const STATUTS = ['Deposee', 'EnCours', 'Traitee', 'Rejetee', 'Archivee'] as const

const STATUT_CONFIG: Record<string, { label: string; color: string; bg: string; icon: typeof Clock }> = {
  Deposee:  { label: 'Déposée',  color: '#d97706', bg: '#fef3c7', icon: Clock },
  EnCours:  { label: 'En cours', color: '#2563eb', bg: '#eff6ff', icon: Loader2 },
  Traitee:  { label: 'Traitée',  color: '#16a34a', bg: '#f0fdf4', icon: CheckCircle },
  Rejetee:  { label: 'Rejetée',  color: '#dc2626', bg: '#fef2f2', icon: AlertTriangle },
  Archivee: { label: 'Archivée', color: '#64748b', bg: '#f8fafc', icon: Clock },
}

const CATEGORIES_LABELS: Record<string, string> = {
  Voirie: 'Voirie',
  Eclairage: 'Eclairage',
  Environnement: 'Environnement',
  Assainissement: 'Assainissement',
  Autre: 'Autre',
}

const CATEGORIES_COLORS: Record<string, string> = {
  Voirie: '#f59e0b',
  Eclairage: '#3b82f6',
  Environnement: '#10b981',
  Assainissement: '#6366f1',
  Autre: '#64748b',
}

function createDoleanceIcon(couleur: string): L.DivIcon {
  return L.divIcon({
    className: 'doleance-marker-icon',
    html: `<div style="width:24px;height:24px;border-radius:50%;background:${couleur};border:3px solid #fff;box-shadow:0 2px 6px rgba(0,0,0,0.3);"></div>`,
    iconSize: [24, 24],
    iconAnchor: [12, 12],
  })
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('fr-FR', { day: '2-digit', month: 'short', year: 'numeric' })
}

export default function DoleancesPage() {
  const { lister, changerStatut } = useDoleances()
  const { rechercher } = useCollectivites()
  const [collectivites, setCollectivites] = useState<Collectivite[]>([])

  useEffect(() => {
    rechercher().then(setCollectivites).catch(() => {})
  }, [rechercher])

  const [doleances, setDoleances] = useState<Doleance[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Filtres
  const [search, setSearch] = useState('')
  const [statutFilter, setStatutFilter] = useState('')
  const [categorieFilter, setCategorieFilter] = useState('')
  const [collectiviteFilter, setCollectiviteFilter] = useState('')

  // Traitement
  const [processingId, setProcessingId] = useState<string | null>(null)
  const [copiedId, setCopiedId] = useState<string | null>(null)

  const fetchDoleances = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await lister(
        collectiviteFilter || undefined,
        statutFilter || undefined,
        categorieFilter || undefined,
      )
      setDoleances(data)
    } catch {
      setError('Erreur lors du chargement des doléances.')
    } finally {
      setLoading(false)
    }
  }, [lister, collectiviteFilter, statutFilter, categorieFilter])

  useEffect(() => { fetchDoleances() }, [fetchDoleances])

  async function handleStatutChange(id: string, newStatut: string) {
    setProcessingId(id)
    try {
      await changerStatut(id, newStatut)
      setDoleances((prev) =>
        prev.map((d) => (d.id === id ? { ...d, statut: newStatut } : d))
      )
    } catch {
      // silently fail
    } finally {
      setProcessingId(null)
    }
  }

  function handleCopyLink(numero: string) {
    const url = `${window.location.origin}/doleances/suivi`
    navigator.clipboard.writeText(url).then(() => {
      setCopiedId(numero)
      setTimeout(() => setCopiedId(null), 2000)
    })
  }

  // Vue
  const [viewMode, setViewMode] = useState<'table' | 'map'>('table')

  // Filtrage côté client (recherche texte)
  const filtered = doleances.filter((d) => {
    if (!search) return true
    const q = search.toLowerCase()
    return (
      d.numeroSuivi.toLowerCase().includes(q) ||
      d.description.toLowerCase().includes(q) ||
      d.auteur.toLowerCase().includes(q) ||
      d.categorie.toLowerCase().includes(q)
    )
  })

  const hasFilters = search || statutFilter || categorieFilter || collectiviteFilter

  return (
    <div className="doleances-admin-page">
      {/* Header */}
      <div className="doleances-header">
        <div>
          <h2>
            <MessageSquareWarning size={22} />
            Doléances citoyennes
          </h2>
          <p>Suivi et traitement des doléances déposées par les citoyens</p>
        </div>
        <div className="doleances-header-actions">
          <a
            href="/doleances/deposer"
            target="_blank"
            rel="noopener noreferrer"
            className="btn-outline"
            style={{ textDecoration: 'none' }}
          >
            <ExternalLink size={14} />
            Formulaire public
          </a>
          <button className="btn-outline" onClick={() => handleCopyLink('')}>
            <Copy size={14} />
            Copier le lien suivi
          </button>
        </div>
      </div>

      {/* Filtres */}
      <div className="doleances-filters">
        <div className="doleances-search">
          <Search size={16} />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Rechercher par numéro, auteur, description…"
          />
          {search && (
            <button className="filter-clear" onClick={() => setSearch('')}>
              <X size={12} />
            </button>
          )}
        </div>

        <div className="doleances-filter-group">
          <Filter size={14} />
          <select value={statutFilter} onChange={(e) => setStatutFilter(e.target.value)}>
            <option value="">Tous les statuts</option>
            {STATUTS.map((s) => (
              <option key={s} value={s}>{STATUT_CONFIG[s]?.label || s}</option>
            ))}
          </select>
        </div>

        <div className="doleances-filter-group">
          <select value={categorieFilter} onChange={(e) => setCategorieFilter(e.target.value)}>
            <option value="">Toutes les catégories</option>
            {CATEGORIES.map((c) => (
              <option key={c} value={c}>{CATEGORIES_LABELS[c] || c}</option>
            ))}
          </select>
        </div>

        <div className="doleances-filter-group">
          <select value={collectiviteFilter} onChange={(e) => setCollectiviteFilter(e.target.value)}>
            <option value="">Toutes les collectivités</option>
            {collectivites.map((c: Collectivite) => (
              <option key={c.id} value={c.id}>{c.nom}</option>
            ))}
          </select>
        </div>

        {/* Toggle vue table / carte */}
        <div className="doleances-view-toggle">
          <button
            className={`doleances-view-btn ${viewMode === 'table' ? 'active' : ''}`}
            onClick={() => setViewMode('table')}
            title="Vue tableau"
          >
            <Table size={16} />
          </button>
          <button
            className={`doleances-view-btn ${viewMode === 'map' ? 'active' : ''}`}
            onClick={() => setViewMode('map')}
            title="Vue carte"
          >
            <Map size={16} />
          </button>
        </div>

        <span className="doleances-count">
          {filtered.length} doléance{filtered.length !== 1 ? 's' : ''}
        </span>

        {hasFilters && (
          <button
            className="btn-outline"
            style={{ padding: '6px 12px', fontSize: '13px' }}
            onClick={() => { setSearch(''); setStatutFilter(''); setCategorieFilter(''); setCollectiviteFilter('') }}
          >
            <X size={12} />
            Réinitialiser
          </button>
        )}
      </div>

      {/* Tableau */}
      {loading && (
        <div className="doleances-loading">
          <Loader2 size={28} className="spin" />
          <span>Chargement des doléances…</span>
        </div>
      )}

      {error && (
        <div className="alert alert-danger">
          <AlertTriangle size={14} /> {error}
          <button onClick={fetchDoleances} style={{ marginLeft: 'auto', color: 'inherit' }}>
            <RefreshCw size={14} />
          </button>
        </div>
      )}

      {!loading && !error && filtered.length === 0 && (
        <div className="doleances-empty">
          <MessageSquareWarning size={36} />
          <h3>Aucune doléance trouvée</h3>
          <p>{hasFilters ? 'Essayez de modifier vos filtres.' : 'Aucune doléance déposée pour le moment.'}</p>
        </div>
      )}

      {!loading && !error && filtered.length > 0 && viewMode === 'map' && (
        <div className="doleances-map-wrapper">
          <MapContainer
            center={[-18.8, 47.5]}
            zoom={6}
            style={{ width: '100%', height: '100%' }}
            zoomControl={false}
          >
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />
            {filtered.map((d) => {
              if (!d.geometrie) return null
              const coords = (d.geometrie as GeoJSON.Point).coordinates as [number, number]
              const couleur = CATEGORIES_COLORS[d.categorie] || '#64748b'
              return (
                <Marker
                  key={d.id}
                  position={[coords[1], coords[0]]}
                  icon={createDoleanceIcon(couleur)}
                >
                  <Popup>
                    <div style={{ minWidth: 180 }}>
                      <div style={{ fontWeight: 700, fontSize: 14, marginBottom: 4 }}>
                        <code style={{ fontSize: 12 }}>{d.numeroSuivi}</code>
                      </div>
                      <div style={{ fontSize: 13, color: '#334155', marginBottom: 4 }}>
                        {d.description.length > 100 ? d.description.slice(0, 100) + '…' : d.description}
                      </div>
                      <div style={{ fontSize: 12, color: '#64748b' }}>
                        <span style={{ color: couleur, fontWeight: 600 }}>{CATEGORIES_LABELS[d.categorie] || d.categorie}</span>
                        {' · '}{d.auteur}<br />
                        {d.collectiviteRattacheeNom && <><MapPin size={11} style={{ verticalAlign: -2 }} /> {d.collectiviteRattacheeNom}</>}
                      </div>
                    </div>
                  </Popup>
                </Marker>
              )
            })}
          </MapContainer>
        </div>
      )}

      {!loading && !error && filtered.length > 0 && viewMode === 'table' && (
        <div className="doleances-table-wrapper">
          <table className="doleances-table">
            <thead>
              <tr>
                <th>Numéro</th>
                <th>Catégorie</th>
                <th>Auteur</th>
                <th>Collectivité</th>
                <th>Date</th>
                <th>Statut</th>
                <th>Traitement</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((d) => {
                const cfg = STATUT_CONFIG[d.statut] || STATUT_CONFIG.Deposee
                const SIcon = cfg.icon
                return (
                  <tr key={d.id}>
                    <td>
                      <div className="doleance-numero-cell">
                        <code>{d.numeroSuivi}</code>
                        <button
                          className="action-btn"
                          title="Copier le lien de suivi"
                          onClick={() => handleCopyLink(d.numeroSuivi)}
                        >
                          {copiedId === d.numeroSuivi ? <Check size={13} /> : <Copy size={13} />}
                        </button>
                      </div>
                    </td>
                    <td>{CATEGORIES_LABELS[d.categorie] || d.categorie}</td>
                    <td>{d.auteur}</td>
                    <td>
                      {d.collectiviteRattacheeNom ? (
                        <span className="cell-collectivite">
                          <MapPin size={12} />
                          {d.collectiviteRattacheeNom}
                        </span>
                      ) : (
                        <span className="cell-muted">—</span>
                      )}
                    </td>
                    <td className="cell-date">{formatDate(d.dateCreation)}</td>
                    <td>
                      <span className="doleances-badge" style={{ background: cfg.bg, color: cfg.color }}>
                        <SIcon size={12} />
                        {cfg.label}
                      </span>
                    </td>
                    <td>
                      <select
                        className="doleances-statut-select"
                        value={d.statut}
                        disabled={processingId === d.id}
                        onChange={(e) => handleStatutChange(d.id, e.target.value)}
                      >
                        {STATUTS.map((s) => (
                          <option key={s} value={s}>{STATUT_CONFIG[s]?.label || s}</option>
                        ))}
                      </select>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
