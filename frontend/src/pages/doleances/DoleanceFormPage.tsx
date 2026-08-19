import { useState, useCallback, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  MapPin, Send, AlertTriangle, CheckCircle, ArrowLeft, X
} from 'lucide-react'
import { MapContainer, TileLayer, Marker, useMapEvents } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { useDoleances, CATEGORIES, type DeposerDoleanceRequest } from '../../hooks/useDoleances'

// Fix Leaflet default icon issue in webpack/vite
delete (L.Icon.Default.prototype as unknown as Record<string, unknown>)._getIconUrl
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
})

const CATEGORIES_LABELS: Record<string, string> = {
  Voirie: '🛣️ Voirie',
  Eclairage: '💡 Éclairage public',
  Environnement: '🌿 Environnement',
  Assainissement: '💧 Assainissement',
  Autre: '📝 Autre',
}

function MapClickHandler({ onPointSelect }: { onPointSelect: (lat: number, lng: number) => void }) {
  useMapEvents({
    click(e) {
      onPointSelect(e.latlng.lat, e.latlng.lng)
    },
  })
  return null
}

const EMPTY_FORM: Omit<DeposerDoleanceRequest, 'point'> & { point: DeposerDoleanceRequest['point'] | null } = {
  description: '',
  categorie: '',
  auteur: '',
  point: null,
}

export default function DoleanceFormPage() {
  const { deposer } = useDoleances()
  const navigate = useNavigate()
  const mapRef = useRef<L.Map | null>(null)

  const [form, setForm] = useState(EMPTY_FORM)
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [saving, setSaving] = useState(false)
  const [result, setResult] = useState<{ numeroSuivi: string } | null>(null)
  const [submitError, setSubmitError] = useState<string | null>(null)

  const handlePointSelect = useCallback((lat: number, lng: number) => {
    const point: GeoJSON.Point = { type: 'Point', coordinates: [lng, lat] }
    setForm((prev) => ({ ...prev, point }))
    // Centrer la carte sur le point
    if (mapRef.current) {
      mapRef.current.setView([lat, lng], Math.max(mapRef.current.getZoom(), 14))
    }
  }, [])

  function validate(): boolean {
    const errs: Record<string, string> = {}
    if (!form.description.trim()) errs.description = 'La description est obligatoire.'
    if (form.description.trim().length < 10) errs.description = 'La description doit faire au moins 10 caractères.'
    if (!form.categorie) errs.categorie = 'Sélectionnez une catégorie.'
    if (!form.auteur.trim()) errs.auteur = 'Votre nom est obligatoire.'
    if (!form.point) errs.point = 'Cliquez sur la carte pour signaler la localisation.'
    setErrors(errs)
    return Object.keys(errs).length === 0
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!validate() || !form.point) return
    setSaving(true)
    setSubmitError(null)
    try {
      const response = await deposer({
        description: form.description.trim(),
        categorie: form.categorie,
        auteur: form.auteur.trim(),
        point: form.point,
      })
      setResult({ numeroSuivi: response.numeroSuivi })
    } catch (err: unknown) {
      let msg = 'Une erreur est survenue lors du dépôt.'
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { data?: { message?: string } } }
        msg = axiosErr.response?.data?.message || msg
      }
      setSubmitError(msg)
    } finally {
      setSaving(false)
    }
  }

  // Succès
  if (result) {
    return (
      <div className="doleance-public-page">
        <div className="doleance-public-card doleance-success-card">
          <div className="doleance-success-icon">
            <CheckCircle size={48} />
          </div>
          <h2>Dépôt confirmé !</h2>
          <p className="doleance-success-text">
            Votre doléance a été enregistrée avec succès et rattachée à la collectivité correspondante.
          </p>
          <div className="doleance-numero-box">
            <span className="doleance-numero-label">Votre numéro de suivi</span>
            <code className="doleance-numero-value">{result.numeroSuivi}</code>
            <span className="doleance-numero-hint">
              Conservez ce numéro pour suivre l'avancement de votre dossier.
            </span>
          </div>
          <div className="doleance-success-actions">
            <button className="doleance-btn-primary" onClick={() => navigate('/')}>
              Retour à l'accueil
            </button>
            <button className="doleance-btn-outline" onClick={() => { setResult(null); setForm(EMPTY_FORM); setErrors({}) }}>
              Déposer une autre doléance
            </button>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="doleance-public-page">
      <div className="doleance-public-card">
        {/* Header */}
        <div className="doleance-form-header">
          <button className="doleance-back" onClick={() => navigate('/')}>
            <ArrowLeft size={16} />
          </button>
          <div>
            <h2>
              <MapPin size={20} />
              Déposer une doléance
            </h2>
            <p>Signalez un problème dans votre commune en le localisant sur la carte</p>
          </div>
        </div>

        {/* Erreur soumission */}
        {submitError && (
          <div className="alert alert-danger">
            <AlertTriangle size={14} /> {submitError}
            <button onClick={() => setSubmitError(null)} style={{ marginLeft: 'auto', color: 'inherit' }}>
              <X size={14} />
            </button>
          </div>
        )}

        <form className="doleance-form" onSubmit={handleSubmit}>
          {/* Carte */}
          <div className="doleance-map-section">
            <label className="doleance-label">
              <MapPin size={14} />
              Localisation du problème *
            </label>
            <p className="doleance-hint">
              Cliquez sur la carte pour placer un marqueur à l'emplacement exact du problème.
            </p>
            <div className={`doleance-map-wrapper ${errors.point ? 'input-error' : ''}`}>
              <MapContainer
                center={[-18.8792, 47.5079]}
                zoom={12}
                style={{ height: '100%', width: '100%' }}
                ref={(map) => { if (map) mapRef.current = map }}
              >
                <TileLayer
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                <MapClickHandler onPointSelect={handlePointSelect} />
                {form.point && (
                  <Marker position={[form.point.coordinates[1], form.point.coordinates[0]]} />
                )}
              </MapContainer>
              {!form.point && (
                <div className="doleance-map-placeholder">
                  <MapPin size={24} />
                  <span>Cliquez ici pour placer le marqueur</span>
                </div>
              )}
            </div>
            {errors.point && <span className="field-error">{errors.point}</span>}
            {form.point && (
              <div className="doleance-coords">
                📍 {form.point.coordinates[1].toFixed(5)}, {form.point.coordinates[0].toFixed(5)}
                <button type="button" className="doleance-clear-point" onClick={() => setForm((p) => ({ ...p, point: null }))}>
                  <X size={12} /> Supprimer
                </button>
              </div>
            )}
          </div>

          {/* Champs du formulaire */}
          <div className="doleance-fields">
            <div className="doleance-field">
              <label className="doleance-label">
                Catégorie *
              </label>
              <div className="doleance-categories">
                {CATEGORIES.map((cat) => (
                  <button
                    key={cat}
                    type="button"
                    className={`doleance-cat-chip ${form.categorie === cat ? 'active' : ''}`}
                    onClick={() => setForm((p) => ({ ...p, categorie: cat }))}
                  >
                    {CATEGORIES_LABELS[cat] || cat}
                  </button>
                ))}
              </div>
              {errors.categorie && <span className="field-error">{errors.categorie}</span>}
            </div>

            <div className="doleance-field">
              <label className="doleance-label" htmlFor="doleance-auteur">
                Votre nom *
              </label>
              <input
                id="doleance-auteur"
                type="text"
                value={form.auteur}
                onChange={(e) => setForm((p) => ({ ...p, auteur: e.target.value }))}
                placeholder="Ex: Jean Rakoto"
                className={errors.auteur ? 'input-error' : ''}
              />
              {errors.auteur && <span className="field-error">{errors.auteur}</span>}
            </div>

            <div className="doleance-field">
              <label className="doleance-label" htmlFor="doleance-desc">
                Description du problème *
              </label>
              <textarea
                id="doleance-desc"
                value={form.description}
                onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))}
                placeholder="Décrivez le problème en détail : nature, gravité, impacts..."
                rows={4}
                className={errors.description ? 'input-error' : ''}
              />
              {errors.description && <span className="field-error">{errors.description}</span>}
              <span className="doleance-char-count">
                {form.description.length} caractère{form.description.length !== 1 ? 's' : ''}
              </span>
            </div>
          </div>

          {/* Soumission */}
          <button type="submit" className="doleance-submit-btn" disabled={saving}>
            <Send size={16} />
            {saving ? 'Dépôt en cours…' : 'Déposer la doléance'}
          </button>
        </form>
      </div>
    </div>
  )
}
