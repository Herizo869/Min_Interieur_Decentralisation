import { useEffect, useState, useMemo, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { MapContainer, TileLayer, GeoJSON, ZoomControl } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { MapPin, Search, Layers, Info, X, ExternalLink } from 'lucide-react'
import {
  useCollectivitesGeoJson,
  TYPES_COLLECTIVITE,
  type TypeCollectivite,
  type CollectiviteFeature,
} from '../../hooks/useCollectivites'
import '../../index.css'

declare global {
  interface Window {
    __navigate?: (path: string) => void
  }
}

// ─── Couleurs par type de collectivité ────────────────────
const TYPE_COLORS: Record<string, string> = {
  Commune: '#3b82f6',
  Departement: '#f59e0b',
  Region: '#10b981',
  Epci: '#8b5cf6',
}

const TYPE_LABELS: Record<string, string> = {
  Commune: 'Commune',
  Departement: 'Département',
  Region: 'Région',
  Epci: 'EPCI',
}

// ─── Style GeoJSON par défaut ────────────────────────────
function styleFeature(feature?: GeoJSON.Feature): L.PathOptions {
  const type = feature?.properties?.type ?? 'Commune'
  const color = TYPE_COLORS[type] || '#94a3b8'
  return {
    color,
    weight: 1.5,
    opacity: 0.8,
    fillColor: color,
    fillOpacity: 0.15,
  }
}

// ─── Popup formaté ────────────────────────────────────────
function featurePopup(feature: CollectiviteFeature): L.Popup {
  const p = feature.properties
  const typeName = TYPE_LABELS[p.type] || p.type
  const color = TYPE_COLORS[p.type] || '#94a3b8'

  const html = `
    <div style="font-family:Inter,system-ui,sans-serif;min-width:200px;">
      <div style="display:flex;align-items:center;gap:8px;margin-bottom:8px;">
        <span style="
          display:inline-block;width:10px;height:10px;border-radius:50%;
          background:${color};flex-shrink:0;
        "></span>
        <strong style="font-size:14px;color:#0f172a;">${p.nom}</strong>
      </div>
      <div style="font-size:13px;color:#475569;line-height:1.6;margin-bottom:10px;">
        <div><span style="color:#64748b;">Code :</span> ${p.codeAdministratif}</div>
        <div><span style="color:#64748b;">Type :</span> ${typeName}</div>
        <div><span style="color:#64748b;">Population :</span> ${p.population.toLocaleString('fr-FR')}</div>
      </div>
      <button onclick="window.__navigate('/collectivites/${p.id}')" style="
        display:inline-flex;align-items:center;gap:6px;
        padding:6px 12px;border-radius:6px;border:none;
        background:#2563eb;color:#fff;font-size:13px;font-weight:500;
        cursor:pointer;font-family:Inter,system-ui,sans-serif;
      ">
        Voir la fiche
        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/><polyline points="15 3 21 3 21 9"/><line x1="10" y1="14" x2="21" y2="3"/></svg>
      </button>
    </div>
  `

  return L.popup({ maxWidth: 300 }).setContent(html)
}

// ─── Composant principal ──────────────────────────────────
export default function CollectivitesPage() {
  const navigate = useNavigate()
  const { geoData, loading, error, chargerGeoJson } = useCollectivitesGeoJson()
  const [typeFiltre, setTypeFiltre] = useState<TypeCollectivite | ''>('')
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedFeature, setSelectedFeature] = useState<CollectiviteFeature | null>(null)
  const [mapKey, setMapKey] = useState(0)

  // Exposer navigate globalement pour le onclick des popups Leaflet
  useEffect(() => {
    window.__navigate = navigate
    return () => { window.__navigate = undefined }
  }, [navigate])

  // Charger les données initiales + à chaque changement de filtre
  useEffect(() => {
    chargerGeoJson(typeFiltre || undefined)
  }, [typeFiltre, chargerGeoJson])

  // Filtre local par nom (recherche textuelle)
  const filteredFeatures = useMemo(() => {
    if (!geoData?.features) return []
    if (!searchTerm.trim()) return geoData.features

    const term = searchTerm.trim().toLowerCase()
    return geoData.features.filter(
      (f) =>
        f.properties.nom.toLowerCase().includes(term) ||
        f.properties.codeAdministratif.toLowerCase().includes(term)
    )
  }, [geoData, searchTerm])

  // GeoJSON filtré pour la carte
  const filteredGeoData: GeoJSON.FeatureCollection = useMemo(
    () => ({
      type: 'FeatureCollection',
      features: filteredFeatures,
    }),
    [filteredFeatures]
  )

  // Stats
  const stats = useMemo(() => {
    if (!geoData?.features) return { total: 0, byType: {} }
    const byType: Record<string, number> = {}
    geoData.features.forEach((f) => {
      const t = f.properties.type
      byType[t] = (byType[t] || 0) + 1
    })
    return { total: geoData.features.length, byType }
  }, [geoData])

  const handleFeatureClick = useCallback((_e: L.LeafletMouseEvent, feature: CollectiviteFeature) => {
    setSelectedFeature(feature)
  }, [])

  const resetSearch = useCallback(() => {
    setSearchTerm('')
  }, [])

  return (
    <div className="page page-map">
      {/* ── Barre supérieure : filtres + recherche ── */}
      <div className="map-toolbar">
        <div className="map-toolbar-left">
          <h2>
            <MapPin size={20} />
            Carte interactive
          </h2>

          {/* Filtres par type */}
          <div className="map-type-filters">
            {TYPES_COLLECTIVITE.map((t) => (
              <button
                key={t.value}
                className={`map-type-chip ${typeFiltre === t.value ? 'active' : ''}`}
                onClick={() => {
                  setTypeFiltre(t.value)
                  setMapKey((k) => k + 1) // force remount du GeoJSON layer
                }}
              >
                {t.value && (
                  <span
                    className="chip-dot"
                    style={{ background: TYPE_COLORS[t.label] || TYPE_COLORS[Object.keys(TYPE_COLORS).find((k) => k.startsWith(t.label.slice(0, 3))) || ''] || '#94a3b8' }}
                  />
                )}
                {t.label}
                {t.value && stats.byType[t.label] != null && (
                  <span className="chip-count">{stats.byType[t.label]}</span>
                )}
              </button>
            ))}
          </div>
        </div>

        <div className="map-toolbar-right">
          <div className="map-search-wrapper">
            <Search size={16} />
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Rechercher par nom ou code…"
              className="map-search-input"
            />
            {searchTerm && (
              <button className="map-search-clear" onClick={resetSearch}>
                <X size={14} />
              </button>
            )}
          </div>
        </div>
      </div>

      {/* ── Carte ── */}
      <div className="map-wrapper">
        {loading && (
          <div className="map-overlay">
            <div className="map-spinner" />
            <span>Chargement des collectivités…</span>
          </div>
        )}

        {error && (
          <div className="map-error">
            <Layers size={40} />
            <p>{error}</p>
            <button className="btn-outline" onClick={() => chargerGeoJson(typeFiltre || undefined)}>
              Réessayer
            </button>
          </div>
        )}

        <MapContainer
          center={[-19.0, 46.5]} // Centre de Madagascar
          zoom={6}
          minZoom={5}
          maxZoom={18}
          maxBounds={[[-27.0, 42.0], [-10.0, 52.0]]}
          maxBoundsViscosity={1.0}
          zoomControl={false}
          className="map-leaflet"
        >
          <ZoomControl position="bottomright" />
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />

          <GeoJSON
            key={mapKey}
            data={filteredGeoData}
            style={styleFeature}
            onEachFeature={(feature, layer) => {
              const popup = featurePopup(feature as unknown as CollectiviteFeature)
              layer.bindPopup(popup.getContent() as string)
              layer.on({
                click: (e) => {
                  // Zoom sur la feature cliquée
                  const map = e.target._map
                  const bounds = e.target.getBounds()
                  map.fitBounds(bounds, { padding: [50, 50], maxZoom: 18 })
                  handleFeatureClick(e, feature as unknown as CollectiviteFeature)
                },
                mouseover: (e) => {
                  e.target.setStyle({ weight: 3, fillOpacity: 0.3 })
                  e.target.bringToFront()
                },
                mouseout: (e) => {
                  e.target.setStyle(styleFeature(feature))
                },
              })
            }}
          />
        </MapContainer>

        {/* ── Légende ── */}
        <div className="map-legend">
          <div className="map-legend-title">
            <Layers size={14} />
            Types
          </div>
          {Object.entries(TYPE_COLORS).map(([type, color]) => (
            <div key={type} className="map-legend-item">
              <span className="legend-dot" style={{ background: color }} />
              <span>{TYPE_LABELS[type]}</span>
            </div>
          ))}
        </div>

        {/* ── Compteur ── */}
        <div className="map-counter">
          <Info size={14} />
          {filteredFeatures.length} collectivité{filteredFeatures.length > 1 ? 's' : ''} affichée{filteredFeatures.length > 1 ? 's' : ''}
        </div>

        {/* ── Panneau détail (quand une feature est sélectionnée) ── */}
        {selectedFeature && (
          <div className="map-detail-panel">
            <div className="map-detail-header">
              <h3>{selectedFeature.properties.nom}</h3>
              <button onClick={() => setSelectedFeature(null)}>
                <X size={16} />
              </button>
            </div>
            <div className="map-detail-body">
              <div className="detail-row">
                <span className="detail-label">Code administratif</span>
                <span className="detail-value">{selectedFeature.properties.codeAdministratif}</span>
              </div>
              <div className="detail-row">
                <span className="detail-label">Type</span>
                <span className="detail-value">
                  <span
                    className="detail-type-badge"
                    style={{ background: TYPE_COLORS[selectedFeature.properties.type] || '#94a3b8' }}
                  >
                    {TYPE_LABELS[selectedFeature.properties.type] || selectedFeature.properties.type}
                  </span>
                </span>
              </div>
              <div className="detail-row">
                <span className="detail-label">Population</span>
                <span className="detail-value">
                  {selectedFeature.properties.population.toLocaleString('fr-FR')}
                </span>
              </div>
            </div>
            <div className="map-detail-footer">
              <button
                className="btn-modern btn-sm"
                onClick={() => navigate(`/collectivites/${selectedFeature.properties.id}`)}
              >
                Voir la fiche complète
                <ExternalLink size={14} />
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
