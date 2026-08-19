import { useState, useEffect, useCallback } from 'react'
import {
  Download, FileText, MapPin, FolderOpen, BarChart3,
  Building2, Filter, AlertTriangle, CheckCircle
} from 'lucide-react'
import { useExports, type ExportResource } from '../../hooks/useExports'
import { useCollectivites, type Collectivite } from '../../hooks/useCollectivites'

const RESSOURCES: { value: ExportResource; label: string; icon: React.ReactNode; description: string; color: string }[] = [
  { value: 'projets', label: 'Projets & Dotations', icon: <FolderOpen size={20} />, description: 'Liste des projets financés et dotations', color: '#2563eb' },
  { value: 'indicateurs', label: 'Indicateurs', icon: <BarChart3 size={20} />, description: 'Données chiffrées des collectivités', color: '#8b5cf6' },
  { value: 'litiges', label: 'Litiges de limites', icon: <MapPin size={20} />, description: 'Conflits territoriaux entre collectivités', color: '#dc2626' },
  { value: 'doleances', label: 'Doléances citoyennes', icon: <FileText size={20} />, description: 'Signalements et plaintes citoyennes', color: '#d97706' },
]

const STATUTS_DOLEANCE = [
  { value: '', label: 'Tous les statuts' },
  { value: 'Ouverte', label: 'Ouverte' },
  { value: 'EnCours', label: 'En cours' },
  { value: 'Resolue', label: 'Résolue' },
  { value: 'Fermee', label: 'Fermée' },
]

const STATUTS_LITIGE = [
  { value: '', label: 'Tous les statuts' },
  { value: 'Signale', label: 'Signalé' },
  { value: 'EnInstruction', label: 'En instruction' },
  { value: 'Arbitre', label: 'Arbitré' },
  { value: 'Clos', label: 'Clos' },
]

export default function ExportsPage() {
  const { telecharger } = useExports()
  const { rechercher } = useCollectivites()

  const [ressource, setRessource] = useState<ExportResource>('projets')
  const [collectiviteId, setCollectiviteId] = useState('')
  const [statutDoleance, setStatutDoleance] = useState('')
  const [statutLitige, setStatutLitige] = useState('')
  const [collectivites, setCollectivites] = useState<Collectivite[]>([])
  const [loading, setLoading] = useState(false)
  const [success, setSuccess] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    rechercher().then(setCollectivites).catch(() => {})
  }, [rechercher])

  // Effacer le message de succès après 4s
  useEffect(() => {
    if (success) {
      const t = setTimeout(() => setSuccess(null), 4000)
      return () => clearTimeout(t)
    }
  }, [success])

  const handleExport = useCallback(async () => {
    setLoading(true)
    setError(null)
    setSuccess(null)
    try {
      await telecharger(ressource, collectiviteId || undefined, {
        statutDoleance: ressource === 'doleances' ? statutDoleance : undefined,
        statutLitige: ressource === 'litiges' ? statutLitige : undefined,
      })
      const label = RESSOURCES.find((r) => r.value === ressource)?.label || ressource
      setSuccess(`Export « ${label} » téléchargé avec succès.`)
    } catch {
      setError('Erreur lors de l\'export. Vérifiez votre connexion.')
    } finally {
      setLoading(false)
    }
  }, [ressource, collectiviteId, statutDoleance, statutLitige, telecharger])

  const ressourceInfo = RESSOURCES.find((r) => r.value === ressource)!

  return (
    <div className="page exports-page">
      {/* En-tête */}
      <div className="projets-header">
        <div>
          <h2>Exports CSV</h2>
          <p>Exportez les données de l'application au format CSV (séparateur « ; »)</p>
        </div>
      </div>

      <div className="exports-layout">
        {/* Sélecteur de ressource */}
        <div className="exports-card">
          <h3 className="exports-card-title">
            <FileText size={16} />
            1. Choisir la ressource
          </h3>
          <div className="exports-grid">
            {RESSOURCES.map((r) => (
              <button
                key={r.value}
                className={`exports-resource ${ressource === r.value ? 'active' : ''}`}
                onClick={() => setRessource(r.value)}
                style={{ '--res-color': r.color } as React.CSSProperties}
              >
                <span className="exports-resource-icon">{r.icon}</span>
                <span className="exports-resource-label">{r.label}</span>
                <span className="exports-resource-desc">{r.description}</span>
              </button>
            ))}
          </div>
        </div>

        {/* Filtres */}
        <div className="exports-card">
          <h3 className="exports-card-title">
            <Filter size={16} />
            2. Filtrer les données
          </h3>
          <div className="exports-filters">
            <div className="exports-filter-group">
              <label>
                <Building2 size={14} />
                Collectivité (optionnel)
              </label>
              <select value={collectiviteId} onChange={(e) => setCollectiviteId(e.target.value)}>
                <option value="">Toutes les collectivités</option>
                {collectivites.map((c) => (
                  <option key={c.id} value={c.id}>{c.nom} ({c.codeAdministratif})</option>
                ))}
              </select>
            </div>

            {ressource === 'doleances' && (
              <div className="exports-filter-group">
                <label>
                  <FileText size={14} />
                  Statut doléance
                </label>
                <select value={statutDoleance} onChange={(e) => setStatutDoleance(e.target.value)}>
                  {STATUTS_DOLEANCE.map((s) => (
                    <option key={s.value} value={s.value}>{s.label}</option>
                  ))}
                </select>
              </div>
            )}

            {ressource === 'litiges' && (
              <div className="exports-filter-group">
                <label>
                  <MapPin size={14} />
                  Statut litige
                </label>
                <select value={statutLitige} onChange={(e) => setStatutLitige(e.target.value)}>
                  {STATUTS_LITIGE.map((s) => (
                    <option key={s.value} value={s.value}>{s.label}</option>
                  ))}
                </select>
              </div>
            )}
          </div>
        </div>

        {/* Aperçu & Téléchargement */}
        <div className="exports-card exports-preview">
          <h3 className="exports-card-title">
            <Download size={16} />
            3. Télécharger
          </h3>
          <div className="exports-preview-content">
            <div className="exports-preview-info">
              <span
                className="exports-preview-icon"
                style={{ color: ressourceInfo.color, background: ressourceInfo.color + '15' }}
              >
                {ressourceInfo.icon}
              </span>
              <div>
                <span className="exports-preview-name">{ressourceInfo.label}</span>
                <span className="exports-preview-detail">
                  Format CSV « ; » — UTF-8 avec BOM
                  {collectiviteId && ' — Filtre collectivité appliqué'}
                </span>
              </div>
            </div>

            {/* Messages */}
            {success && (
              <div className="exports-message exports-success">
                <CheckCircle size={16} />
                {success}
              </div>
            )}
            {error && (
              <div className="exports-message exports-error">
                <AlertTriangle size={16} />
                {error}
              </div>
            )}

            <button
              className="exports-download-btn"
              onClick={handleExport}
              disabled={loading}
              style={{ '--res-color': ressourceInfo.color } as React.CSSProperties}
            >
              <Download size={18} />
              {loading ? 'Génération…' : `Télécharger ${ressourceInfo.label}`}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
