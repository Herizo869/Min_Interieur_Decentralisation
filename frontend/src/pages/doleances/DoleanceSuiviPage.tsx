import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Search, ArrowLeft, MapPin, Clock, CheckCircle, AlertTriangle, Loader2, Hash, FolderOpen
} from 'lucide-react'
import { useDoleances, type Doleance } from '../../hooks/useDoleances'

const STATUT_CONFIG: Record<string, { label: string; color: string; bg: string; icon: typeof Clock }> = {
  Deposee:     { label: 'Déposée',       color: '#d97706', bg: '#fef3c7', icon: Clock },
  EnCours:     { label: 'En cours',      color: '#2563eb', bg: '#eff6ff', icon: Loader2 },
  Traitee:     { label: 'Traitée',       color: '#16a34a', bg: '#f0fdf4', icon: CheckCircle },
  Rejetee:     { label: 'Rejetée',       color: '#dc2626', bg: '#fef2f2', icon: AlertTriangle },
  Archivee:    { label: 'Archivée',      color: '#64748b', bg: '#f8fafc', icon: FolderOpen },
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('fr-FR', {
    day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit',
  })
}

export default function DoleanceSuiviPage() {
  const navigate = useNavigate()
  const { suivreParNumero } = useDoleances()

  const [numero, setNumero] = useState('')
  const [loading, setLoading] = useState(false)
  const [doleance, setDoleance] = useState<Doleance | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [searched, setSearched] = useState(false)

  async function handleSearch(e: React.FormEvent) {
    e.preventDefault()
    if (!numero.trim()) return
    setLoading(true)
    setError(null)
    setDoleance(null)
    setSearched(true)
    try {
      const result = await suivreParNumero(numero.trim())
      setDoleance(result)
    } catch {
      setError('Aucune doléance trouvée avec ce numéro. Vérifiez et réessayez.')
    } finally {
      setLoading(false)
    }
  }

  const statutInfo = doleance ? STATUT_CONFIG[doleance.statut] || STATUT_CONFIG.Deposee : null
  const StatutIcon = statutInfo?.icon || Clock

  return (
    <div className="suivi-public-page">
      <div className="suivi-public-card">
        {/* Header */}
        <div className="suivi-header">
          <button className="suivi-back" onClick={() => navigate('/')}>
            <ArrowLeft size={16} />
          </button>
          <div>
            <h2>
              <Search size={20} />
              Suivi de doléance
            </h2>
            <p>Entrez votre numéro de suivi pour connaître l'état d'avancement</p>
          </div>
        </div>

        {/* Formulaire de recherche */}
        <form className="suivi-search-form" onSubmit={handleSearch}>
          <div className="suivi-search-group">
            <label className="suivi-label" htmlFor="suivi-numero">
              <Hash size={14} />
              Numéro de suivi
            </label>
            <div className="suivi-search-input-wrapper">
              <input
                id="suivi-numero"
                type="text"
                value={numero}
                onChange={(e) => setNumero(e.target.value)}
                placeholder="Ex: DOL-2026-XXXX"
                className="suivi-search-input"
                autoFocus
              />
              <button type="submit" className="suivi-search-btn" disabled={loading || !numero.trim()}>
                {loading ? <Loader2 size={16} className="spin" /> : <Search size={16} />}
                {loading ? 'Recherche…' : 'Rechercher'}
              </button>
            </div>
          </div>
        </form>

        {/* Erreur */}
        {error && searched && (
          <div className="suivi-error">
            <AlertTriangle size={16} />
            <span>{error}</span>
          </div>
        )}

        {/* Résultat */}
        {doleance && statutInfo && (
          <div className="suivi-result">
            {/* Badge statut */}
            <div className="suivi-statut-badge" style={{ background: statutInfo.bg, color: statutInfo.color, border: `1px solid ${statutInfo.color}30` }}>
              <StatutIcon size={16} />
              <span>{statutInfo.label}</span>
            </div>

            {/* Infos principales */}
            <div className="suivi-info-grid">
              <div className="suivi-info-item">
                <span className="suivi-info-label">Numéro</span>
                <code className="suivi-info-value">{doleance.numeroSuivi}</code>
              </div>
              <div className="suivi-info-item">
                <span className="suivi-info-label">Catégorie</span>
                <span className="suivi-info-value">{doleance.categorie}</span>
              </div>
              <div className="suivi-info-item">
                <span className="suivi-info-label">Auteur</span>
                <span className="suivi-info-value">{doleance.auteur}</span>
              </div>
              <div className="suivi-info-item">
                <span className="suivi-info-label">Date de dépôt</span>
                <span className="suivi-info-value">{formatDate(doleance.dateCreation)}</span>
              </div>
              {doleance.collectiviteRattacheeNom && (
                <div className="suivi-info-item suivi-info-full">
                  <span className="suivi-info-label">Collectivité rattachée</span>
                  <span className="suivi-info-value">
                    <MapPin size={14} />
                    {doleance.collectiviteRattacheeNom}
                  </span>
                </div>
              )}
            </div>

            {/* Description */}
            <div className="suivi-description">
              <span className="suivi-info-label">Description</span>
              <p>{doleance.description}</p>
            </div>
          </div>
        )}

        {/* Lien déposer */}
        <div className="suivi-footer">
          <span>Vous n'avez pas de numéro ?</span>
          <button className="suivi-link" onClick={() => navigate('/doleances/deposer')}>
            <MapPin size={14} />
            Déposer une doléance
          </button>
        </div>
      </div>
    </div>
  )
}
