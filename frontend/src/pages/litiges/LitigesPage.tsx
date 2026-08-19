import { useState, useEffect, useCallback } from 'react'
import {
  X, MapPin, AlertTriangle, Eye, Shield, Play,
  Calendar, Building2, Filter, Search, ArrowRight
} from 'lucide-react'
import { MapContainer, TileLayer, GeoJSON, useMap } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { useLitiges, type Litige } from '../../hooks/useLitiges'
import { useCollectivites, type Collectivite } from '../../hooks/useCollectivites'
import { useAuth } from '../../contexts/AuthContext'

const STATUTS = [
  { value: '', label: 'Tous les statuts', color: '#64748b' },
  { value: 'Signale', label: 'Signalé', color: '#dc2626' },
  { value: 'EnInstruction', label: 'En instruction', color: '#d97706' },
  { value: 'Arbitre', label: 'Arbitré', color: '#2563eb' },
  { value: 'Clos', label: 'Clos', color: '#16a34a' },
]

const STATUT_COLORS: Record<string, string> = {
  Signale: '#dc2626',
  EnInstruction: '#d97706',
  Arbitre: '#2563eb',
  Clos: '#16a34a',
}

const STATUT_BG: Record<string, string> = {
  Signale: '#fef2f2',
  EnInstruction: '#fffbeb',
  Arbitre: '#eff6ff',
  Clos: '#f0fdf4',
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('fr-FR', { day: '2-digit', month: 'short', year: 'numeric' })
}

// Composant interne pour recentrer la carte sur la zone de conflit
function MapCenter({ geometry }: { geometry: GeoJSON.Geometry }) {
  const map = useMap()
  useEffect(() => {
    try {
      const geojson = L.geoJSON({ type: 'Feature', geometry, properties: {} } as GeoJSON.Feature)
      map.fitBounds(geojson.getBounds().pad(0.3))
    } catch { /* ignore */ }
  }, [map, geometry])
  return null
}

export default function LitigesPage() {
  const { lister, changerStatut, detecter } = useLitiges()
  const { rechercher } = useCollectivites()
  const { user } = useAuth()

  const [litiges, setLitiges] = useState<Litige[]>([])
  const [collectivites, setCollectivites] = useState<Collectivite[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Filtres
  const [filtreStatut, setFiltreStatut] = useState('')
  const [filtreCollectivite, setFiltreCollectivite] = useState('')
  const [recherche, setRecherche] = useState('')

  // Détail
  const [detailLitige, setDetailLitige] = useState<Litige | null>(null)
  const [detailOpen, setDetailOpen] = useState(false)

  // Changement de statut
  const [changingStatut, setChangingStatut] = useState(false)

  // Détection
  const [detecting, setDetecting] = useState(false)
  const [detectionResult, setDetectionResult] = useState<{ detectes: number } | null>(null)

  const charger = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const [l, c] = await Promise.all([lister(), rechercher()])
      setLitiges(l)
      setCollectivites(c)
    } catch {
      setError('Erreur lors du chargement des litiges.')
    } finally {
      setLoading(false)
    }
  }, [lister, rechercher])

  useEffect(() => { charger() }, [charger])

  // Filtrage
  const litigesFiltres = litiges.filter((l) => {
    if (filtreStatut && l.statut !== filtreStatut) return false
    if (filtreCollectivite && l.collectiviteAId !== filtreCollectivite && l.collectiviteBId !== filtreCollectivite) return false
    if (recherche.trim()) {
      const term = recherche.trim().toLowerCase()
      if (
        !l.description.toLowerCase().includes(term) &&
        !l.collectiviteANom.toLowerCase().includes(term) &&
        !l.collectiviteBNom.toLowerCase().includes(term)
      ) return false
    }
    return true
  })

  // Ouvrir détail
  function ouvrirDetail(l: Litige) {
    setDetailLitige(l)
    setDetailOpen(true)
  }

  // Changer statut
  async function handleChangerStatut(nouveauStatut: string) {
    if (!detailLitige) return
    setChangingStatut(true)
    try {
      const updated = await changerStatut(detailLitige.id, nouveauStatut)
      setDetailLitige(updated)
      await charger()
    } catch {
      setError('Erreur lors du changement de statut.')
    } finally {
      setChangingStatut(false)
    }
  }

  // Détection automatique
  async function handleDetecter() {
    setDetecting(true)
    setDetectionResult(null)
    try {
      const result = await detecter()
      setDetectionResult(result)
      await charger()
    } catch {
      setError('Erreur lors de la détection automatique.')
    } finally {
      setDetecting(false)
    }
  }

  const isAdmin = user?.role === 'Administrateur'

  return (
    <div className="page litiges-page">
      {/* En-tête */}
      <div className="projets-header">
        <div>
          <h2>Litiges de limites</h2>
          <p>Suivi et traitement des conflits territoriaux entre collectivités</p>
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          {isAdmin && (
            <button
              className="btn-detect"
              onClick={handleDetecter}
              disabled={detecting}
            >
              <Play size={15} />
              {detecting ? 'Détection…' : 'Détecter auto'}
            </button>
          )}
        </div>
      </div>

      {/* Résultat détection */}
      {detectionResult && (
        <div className="detection-result">
          <Shield size={16} />
          <span>
            <strong>{detectionResult.detectes}</strong> chevauchement{detectionResult.detectes !== 1 ? 's' : ''} détecté{detectionResult.detectes !== 1 ? 's' : ''}
          </span>
          <button onClick={() => setDetectionResult(null)}>
            <X size={14} />
          </button>
        </div>
      )}

      {/* Filtres */}
      <div className="projets-filters">
        <div className="projets-search">
          <Search size={16} />
          <input
            type="text"
            placeholder="Rechercher par description ou collectivité…"
            value={recherche}
            onChange={(e) => setRecherche(e.target.value)}
          />
          {recherche && (
            <button className="filter-clear" onClick={() => setRecherche('')}>
              <X size={14} />
            </button>
          )}
        </div>
        <div className="projets-filter-group">
          <Filter size={14} />
          <select value={filtreStatut} onChange={(e) => setFiltreStatut(e.target.value)}>
            {STATUTS.map((s) => (
              <option key={s.value} value={s.value}>{s.label}</option>
            ))}
          </select>
          <select value={filtreCollectivite} onChange={(e) => setFiltreCollectivite(e.target.value)}>
            <option value="">Toutes les collectivités</option>
            {collectivites.map((c) => (
              <option key={c.id} value={c.id}>{c.nom}</option>
            ))}
          </select>
        </div>
        <span className="projets-count">
          {litigesFiltres.length} litige{litigesFiltres.length !== 1 ? 's' : ''}
        </span>
      </div>

      {/* Erreur */}
      {error && (
        <div className="alert alert-danger" style={{ marginBottom: 16 }}>
          <AlertTriangle size={14} /> {error}
          <button onClick={() => setError(null)} style={{ marginLeft: 'auto', color: 'inherit' }}>
            <X size={14} />
          </button>
        </div>
      )}

      {/* Tableau */}
      {loading ? (
        <div className="projets-loading">
          <div className="fiche-spinner" />
          <span>Chargement…</span>
        </div>
      ) : litigesFiltres.length === 0 ? (
        <div className="projets-empty">
          <MapPin size={40} />
          <h3>Aucun litige trouvé</h3>
          <p>
            {recherche || filtreStatut || filtreCollectivite
              ? 'Modifiez vos filtres pour voir des résultats.'
              : 'Aucun litige de limites n\'a été signalé.'}
          </p>
        </div>
      ) : (
        <div className="projets-table-wrapper">
          <table className="projets-table">
            <thead>
              <tr>
                <th>Description</th>
                <th>Collectivités</th>
                <th>Statut</th>
                <th>Date</th>
                <th style={{ width: 80 }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {litigesFiltres.map((l) => (
                <tr key={l.id}>
                  <td>
                    <span className="litige-desc-cell">{l.description}</span>
                  </td>
                  <td>
                    <span className="litige-parties-cell">
                      <Building2 size={13} />
                      {l.collectiviteANom}
                      <ArrowRight size={12} style={{ color: 'var(--text-light)' }} />
                      {l.collectiviteBNom}
                    </span>
                  </td>
                  <td>
                    <span
                      className="projets-badge"
                      style={{ background: STATUT_BG[l.statut] || '#f8fafc', color: STATUT_COLORS[l.statut] || '#64748b' }}
                    >
                      {STATUTS.find((s) => s.value === l.statut)?.label || l.statut}
                    </span>
                  </td>
                  <td>
                    <span className="cell-date">
                      <Calendar size={12} />
                      {formatDate(l.dateCreation)}
                    </span>
                  </td>
                  <td>
                    <button
                      className="action-btn action-edit"
                      title="Voir le détail"
                      onClick={() => ouvrirDetail(l)}
                    >
                      <Eye size={14} />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Panneau détail */}
      {detailOpen && detailLitige && (
        <div className="litige-detail-overlay" onClick={() => setDetailOpen(false)}>
          <div className="litige-detail-panel" onClick={(e) => e.stopPropagation()}>
            {/* Header */}
            <div className="litige-detail-header">
              <h3>
                <MapPin size={18} />
                Détail du litige
              </h3>
              <button className="modal-close" onClick={() => setDetailOpen(false)}>
                <X size={18} />
              </button>
            </div>

            {/* Contenu */}
            <div className="litige-detail-body">
              {/* Carte zone de conflit */}
              <div className="litige-map-container">
                <MapContainer
                  center={[-18.8792, 47.5079]}
                  zoom={10}
                  style={{ height: '100%', width: '100%' }}
                  zoomControl={false}
                >
                  <TileLayer
                    attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                    url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                  />
                  <MapCenter geometry={detailLitige.zoneConflit} />
                  <GeoJSON
                    key={detailLitige.id + '-zone'}
                    data={{
                      type: 'Feature',
                      geometry: detailLitige.zoneConflit,
                      properties: {},
                    } as GeoJSON.Feature}
                    style={{
                      color: '#dc2626',
                      weight: 2,
                      fillColor: '#fca5a5',
                      fillOpacity: 0.4,
                    }}
                  />
                </MapContainer>
                <div className="litige-map-label">
                  <AlertTriangle size={13} />
                  Zone de conflit
                </div>
              </div>

              {/* Infos */}
              <div className="litige-info-grid">
                <div className="litige-info-row">
                  <span className="litige-info-label">Statut</span>
                  <span
                    className="projets-badge"
                    style={{ background: STATUT_BG[detailLitige.statut], color: STATUT_COLORS[detailLitige.statut] }}
                  >
                    {STATUTS.find((s) => s.value === detailLitige.statut)?.label || detailLitige.statut}
                  </span>
                </div>
                <div className="litige-info-row">
                  <span className="litige-info-label">Date de création</span>
                  <span className="litige-info-value">{formatDate(detailLitige.dateCreation)}</span>
                </div>
                <div className="litige-info-row full-width">
                  <span className="litige-info-label">Collectivités impliquées</span>
                  <div className="litige-parties-detail">
                    <span className="litige-col-badge litige-col-a">
                      <Building2 size={12} />
                      {detailLitige.collectiviteANom}
                    </span>
                    <ArrowRight size={14} style={{ color: 'var(--text-light)', flexShrink: 0 }} />
                    <span className="litige-col-badge litige-col-b">
                      <Building2 size={12} />
                      {detailLitige.collectiviteBNom}
                    </span>
                  </div>
                </div>
                <div className="litige-info-row full-width">
                  <span className="litige-info-label">Description</span>
                  <p className="litige-description">{detailLitige.description}</p>
                </div>
              </div>

              {/* Actions changement de statut */}
              <div className="litige-statut-actions">
                <span className="litige-info-label">Changer le statut :</span>
                <div className="litige-statut-btns">
                  {STATUTS.filter((s) => s.value && s.value !== detailLitige.statut).map((s) => (
                    <button
                      key={s.value}
                      className="litige-statut-btn"
                      style={{ borderColor: s.color, color: s.color }}
                      onClick={() => handleChangerStatut(s.value)}
                      disabled={changingStatut}
                    >
                      {s.label}
                    </button>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
