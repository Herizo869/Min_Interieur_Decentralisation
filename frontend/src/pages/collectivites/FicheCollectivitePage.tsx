import { useEffect, useState } from 'react'
import { useParams, Link, useNavigate } from 'react-router-dom'
import {
  ArrowLeft,
  MapPin,
  Users,
  BarChart3,
  FolderKanban,
  AlertTriangle,
  Calendar,
  ExternalLink,
} from 'lucide-react'
import api from '../../services/api'

/* ── Types ──────────────────────────────────────────── */

interface ProjetItem {
  intitule: string
  montant: number
  devise: string
  statut: string
}

interface IndicateurItem {
  type: string
  valeur: number
  unite: string
  dateReleve: string
}

interface LitigeItem {
  id: string
  description: string
  statut: string
  dateCreation: string
  collectiviteANom: string
  collectiviteBNom: string
}

interface CollectiviteDetail {
  id: string
  nom: string
  codeAdministratif: string
  population: number
  type: string
  projets: ProjetItem[]
  indicateurs: IndicateurItem[]
}

/* ── Constantes ─────────────────────────────────────── */

const TYPE_COLORS: Record<string, string> = {
  Commune: '#3b82f6',
  Departement: '#f59e0b',
  Region: '#10b981',
  Epci: '#8b5cf6',
}

const STATUT_PROJET_COLORS: Record<string, string> = {
  EnPreparation: '#f59e0b',
  EnCours: '#3b82f6',
  Termine: '#10b981',
}

const STATUT_LITIGE_COLORS: Record<string, string> = {
  Signale: '#f59e0b',
  EnCours: '#3b82f6',
  Resolu: '#10b981',
  Rejete: '#94a3b8',
}

/* ── Composant ──────────────────────────────────────── */

export default function FicheCollectivitePage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [data, setData] = useState<CollectiviteDetail | null>(null)
  const [litiges, setLitiges] = useState<LitigeItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!id) return

    async function load() {
      setLoading(true)
      setError(null)
      try {
        const [colRes, litRes] = await Promise.all([
          api.get<CollectiviteDetail>(`/api/collectivites/${id}`),
          api.get<LitigeItem[]>(`/api/litiges?collectiviteId=${id}`),
        ])
        setData(colRes.data)
        setLitiges(litRes.data)
      } catch (err: unknown) {
        let message = 'Erreur lors du chargement'
        if (err && typeof err === 'object' && 'response' in err) {
          const axiosErr = err as { response?: { status?: number; data?: { message?: string } } }
          if (axiosErr.response?.status === 404) message = 'Collectivité introuvable'
          else message = axiosErr.response?.data?.message || message
        }
        setError(message)
      } finally {
        setLoading(false)
      }
    }

    load()
  }, [id])

  if (loading) {
    return (
      <div className="page fiche-loading">
        <div className="fiche-spinner" />
        <span>Chargement de la fiche…</span>
      </div>
    )
  }

  if (error || !data) {
    return (
      <div className="page fiche-error">
        <AlertTriangle size={40} />
        <p>{error || 'Collectivité introuvable'}</p>
        <button className="btn-outline" onClick={() => navigate('/collectivites')}>
          <ArrowLeft size={16} />
          Retour à la carte
        </button>
      </div>
    )
  }

  const typeColor = TYPE_COLORS[data.type] || '#94a3b8'

  return (
    <div className="page fiche-page">
      {/* ── En-tête ── */}
      <div className="fiche-header">
        <Link to="/collectivites" className="fiche-back">
          <ArrowLeft size={18} />
          Retour à la carte
        </Link>

        <div className="fiche-title-row">
          <div className="fiche-title-badge" style={{ background: typeColor }}>
            {data.type}
          </div>
          <h2>{data.nom}</h2>
        </div>

        <div className="fiche-meta">
          <span className="fiche-meta-item">
            <MapPin size={14} />
            {data.codeAdministratif}
          </span>
          <span className="fiche-meta-item">
            <Users size={14} />
            {data.population.toLocaleString('fr-FR')} habitants
          </span>
        </div>
      </div>

      {/* ── Stats rapides ── */}
      <div className="fiche-stats">
        <div className="fiche-stat-card">
          <BarChart3 size={20} style={{ color: '#3b82f6' }} />
          <div>
            <span className="stat-number">{data.indicateurs.length}</span>
            <span className="stat-label">Indicateur{data.indicateurs.length > 1 ? 's' : ''}</span>
          </div>
        </div>
        <div className="fiche-stat-card">
          <FolderKanban size={20} style={{ color: '#10b981' }} />
          <div>
            <span className="stat-number">{data.projets.length}</span>
            <span className="stat-label">Projet{data.projets.length > 1 ? 's' : ''}</span>
          </div>
        </div>
        <div className="fiche-stat-card">
          <AlertTriangle size={20} style={{ color: '#f59e0b' }} />
          <div>
            <span className="stat-number">{litiges.length}</span>
            <span className="stat-label">Litige{litiges.length > 1 ? 's' : ''}</span>
          </div>
        </div>
      </div>

      {/* ── Sections ── */}
      <div className="fiche-grid">
        {/* Indicateurs */}
        <div className="fiche-section">
          <div className="fiche-section-header">
            <BarChart3 size={18} />
            <h3>Indicateurs</h3>
          </div>
          {data.indicateurs.length === 0 ? (
            <div className="fiche-empty">Aucun indicateur renseigné</div>
          ) : (
            <div className="fiche-table-wrapper">
              <table className="fiche-table">
                <thead>
                  <tr>
                    <th>Type</th>
                    <th>Valeur</th>
                    <th>Unité</th>
                    <th>Date</th>
                  </tr>
                </thead>
                <tbody>
                  {data.indicateurs.map((ind, i) => (
                    <tr key={i}>
                      <td className="cell-bold">{ind.type}</td>
                      <td>{ind.valeur.toLocaleString('fr-FR')}</td>
                      <td>{ind.unite}</td>
                      <td className="cell-muted">
                        <Calendar size={12} />
                        {new Date(ind.dateReleve).toLocaleDateString('fr-FR')}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        {/* Projets & Dotations */}
        <div className="fiche-section">
          <div className="fiche-section-header">
            <FolderKanban size={18} />
            <h3>Projets & Dotations</h3>
          </div>
          {data.projets.length === 0 ? (
            <div className="fiche-empty">Aucun projet ou dotation</div>
          ) : (
            <div className="fiche-table-wrapper">
              <table className="fiche-table">
                <thead>
                  <tr>
                    <th>Intitulé</th>
                    <th>Montant</th>
                    <th>Statut</th>
                  </tr>
                </thead>
                <tbody>
                  {data.projets.map((p, i) => (
                    <tr key={i}>
                      <td className="cell-bold">{p.intitule}</td>
                      <td>
                        {p.montant.toLocaleString('fr-FR')} {p.devise}
                      </td>
                      <td>
                        <span
                          className="fiche-badge"
                          style={{
                            background: (STATUT_PROJET_COLORS[p.statut] || '#94a3b8') + '18',
                            color: STATUT_PROJET_COLORS[p.statut] || '#94a3b8',
                            border: `1px solid ${(STATUT_PROJET_COLORS[p.statut] || '#94a3b8')}30`,
                          }}
                        >
                          {p.statut}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        {/* Litiges */}
        <div className="fiche-section fiche-section-full">
          <div className="fiche-section-header">
            <AlertTriangle size={18} />
            <h3>Litiges de limites</h3>
          </div>
          {litiges.length === 0 ? (
            <div className="fiche-empty">Aucun litige associé</div>
          ) : (
            <div className="fiche-litiges">
              {litiges.map((lit) => (
                <div key={lit.id} className="fiche-litige-card">
                  <div className="litige-card-header">
                    <span
                      className="fiche-badge"
                      style={{
                        background: (STATUT_LITIGE_COLORS[lit.statut] || '#94a3b8') + '18',
                        color: STATUT_LITIGE_COLORS[lit.statut] || '#94a3b8',
                        border: `1px solid ${(STATUT_LITIGE_COLORS[lit.statut] || '#94a3b8')}30`,
                      }}
                    >
                      {lit.statut}
                    </span>
                    <span className="litige-date">
                      <Calendar size={12} />
                      {new Date(lit.dateCreation).toLocaleDateString('fr-FR')}
                    </span>
                  </div>
                  <p className="litige-desc">{lit.description}</p>
                  <div className="litige-parties">
                    <span>{lit.collectiviteANom}</span>
                    <span className="litige-vs">↔</span>
                    <span>{lit.collectiviteBNom}</span>
                  </div>
                  <Link to={`/litiges`} className="litige-link">
                    Voir le litige
                    <ExternalLink size={12} />
                  </Link>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
