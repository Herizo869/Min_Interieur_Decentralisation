import { useState, useEffect, useCallback } from 'react'
import {
  Plus, Pencil, Trash2, X, FolderOpen, Calendar, DollarSign,
  Building2, Filter, Search, AlertTriangle
} from 'lucide-react'
import { useProjets, type ProjetDotation, type ProjetDotationRequest } from '../../hooks/useProjets'
import { useCollectivites, type Collectivite } from '../../hooks/useCollectivites'

const STATUTS = [
  { value: '', label: 'Tous les statuts' },
  { value: 'EnPreparation', label: 'En préparation' },
  { value: 'EnCours', label: 'En cours' },
  { value: 'Termine', label: 'Terminé' },
]

const DEVISES = ['MGA', 'EUR', 'USD']

const STATUT_BADGES: Record<string, { label: string; bg: string; color: string }> = {
  EnPreparation: { label: 'En préparation', bg: '#eff6ff', color: '#2563eb' },
  EnCours: { label: 'En cours', bg: '#f0fdf4', color: '#16a34a' },
  Termine: { label: 'Terminé', bg: '#f8fafc', color: '#64748b' },
}

const EMPTY_FORM: ProjetDotationRequest = {
  intitule: '',
  montant: 0,
  devise: 'MGA',
  statut: 'EnPreparation',
  dateDebut: new Date().toISOString().split('T')[0],
  dateFin: null,
  collectiviteId: '',
}

function formatMontant(n: number, devise: string) {
  return new Intl.NumberFormat('fr-FR', { minimumFractionDigits: 0 }).format(n) + ' ' + devise
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('fr-FR', { day: '2-digit', month: 'short', year: 'numeric' })
}

export default function ProjetsPage() {
  const { lister, creer, modifier, supprimer } = useProjets()
  const { rechercher } = useCollectivites()

  const [projets, setProjets] = useState<ProjetDotation[]>([])
  const [collectivites, setCollectivites] = useState<Collectivite[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Filtres
  const [filtreStatut, setFiltreStatut] = useState('')
  const [filtreCollectivite, setFiltreCollectivite] = useState('')
  const [recherche, setRecherche] = useState('')

  // Modal
  const [modalOpen, setModalOpen] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState<ProjetDotationRequest>(EMPTY_FORM)
  const [formErrors, setFormErrors] = useState<Record<string, string>>({})
  const [saving, setSaving] = useState(false)

  // Suppression
  const [deleteTarget, setDeleteTarget] = useState<ProjetDotation | null>(null)
  const [deleting, setDeleting] = useState(false)

  // Charger les données
  const charger = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const [p, c] = await Promise.all([lister(), rechercher()])
      setProjets(p)
      setCollectivites(c)
    } catch {
      setError('Erreur lors du chargement des projets.')
    } finally {
      setLoading(false)
    }
  }, [lister, rechercher])

  useEffect(() => { charger() }, [charger])

  // Filtrage côté client
  const projetsFiltres = projets.filter((p) => {
    if (filtreStatut && p.statut !== filtreStatut) return false
    if (filtreCollectivite && p.collectiviteId !== filtreCollectivite) return false
    if (recherche.trim()) {
      const term = recherche.trim().toLowerCase()
      if (
        !p.intitule.toLowerCase().includes(term) &&
        !p.collectiviteNom.toLowerCase().includes(term)
      ) return false
    }
    return true
  })

  // Validation
  function validate(): boolean {
    const errs: Record<string, string> = {}
    if (!form.intitule.trim()) errs.intitule = 'L\'intitulé est obligatoire.'
    if (form.montant < 0) errs.montant = 'Le montant ne peut pas être négatif.'
    if (!form.devise) errs.devise = 'La devise est obligatoire.'
    if (!form.collectiviteId) errs.collectiviteId = 'Sélectionnez une collectivité.'
    if (!form.dateDebut) errs.dateDebut = 'La date de début est obligatoire.'
    if (form.dateFin && form.dateFin < form.dateDebut) errs.dateFin = 'La date de fin doit être postérieure.'
    setFormErrors(errs)
    return Object.keys(errs).length === 0
  }

  // Ouvrir modal création
  function ouvrirCreation() {
    setEditingId(null)
    setForm({ ...EMPTY_FORM, dateDebut: new Date().toISOString().split('T')[0] })
    setFormErrors({})
    setModalOpen(true)
  }

  // Ouvrir modal édition
  function ouvrirEdition(p: ProjetDotation) {
    setEditingId(p.id)
    setForm({
      intitule: p.intitule,
      montant: p.montant,
      devise: p.devise,
      statut: p.statut,
      dateDebut: p.dateDebut.split('T')[0],
      dateFin: p.dateFin ? p.dateFin.split('T')[0] : null,
      collectiviteId: p.collectiviteId,
    })
    setFormErrors({})
    setModalOpen(true)
  }

  // Soumettre formulaire
  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!validate()) return
    setSaving(true)
    try {
      if (editingId) {
        await modifier(editingId, form)
      } else {
        await creer(form)
      }
      setModalOpen(false)
      await charger()
    } catch (err: unknown) {
      let msg = 'Erreur lors de la sauvegarde.'
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { data?: { message?: string } } }
        msg = axiosErr.response?.data?.message || msg
      }
      setFormErrors({ submit: msg })
    } finally {
      setSaving(false)
    }
  }

  // Supprimer
  async function handleDelete() {
    if (!deleteTarget) return
    setDeleting(true)
    try {
      await supprimer(deleteTarget.id)
      setDeleteTarget(null)
      await charger()
    } catch {
      setError('Erreur lors de la suppression.')
    } finally {
      setDeleting(false)
    }
  }

  return (
    <div className="page projets-page">
      {/* En-tête */}
      <div className="projets-header">
        <div>
          <h2>Projets & Dotations</h2>
          <p>Gestion des projets financés et dotations attribuées aux collectivités</p>
        </div>
        <button className="btn-primary" onClick={ouvrirCreation}>
          <Plus size={16} /> Nouveau projet
        </button>
      </div>

      {/* Filtres */}
      <div className="projets-filters">
        <div className="projets-search">
          <Search size={16} />
          <input
            type="text"
            placeholder="Rechercher par intitulé ou collectivité…"
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
          {projetsFiltres.length} projet{projetsFiltres.length !== 1 ? 's' : ''}
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
      ) : projetsFiltres.length === 0 ? (
        <div className="projets-empty">
          <FolderOpen size={40} />
          <h3>Aucun projet trouvé</h3>
          <p>
            {recherche || filtreStatut || filtreCollectivite
              ? 'Modifiez vos filtres pour voir des résultats.'
              : 'Créez votre premier projet ou dotation.'}
          </p>
        </div>
      ) : (
        <div className="projets-table-wrapper">
          <table className="projets-table">
            <thead>
              <tr>
                <th>Intitulé</th>
                <th>Collectivité</th>
                <th>Montant</th>
                <th>Statut</th>
                <th>Début</th>
                <th>Fin</th>
                <th style={{ width: 100 }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {projetsFiltres.map((p) => {
                const badge = STATUT_BADGES[p.statut] || STATUT_BADGES.EnPreparation
                return (
                  <tr key={p.id}>
                    <td className="cell-bold">{p.intitule}</td>
                    <td>
                      <span className="cell-collectivite">
                        <Building2 size={13} />
                        {p.collectiviteNom}
                      </span>
                    </td>
                    <td className="cell-montant">{formatMontant(p.montant, p.devise)}</td>
                    <td>
                      <span
                        className="projets-badge"
                        style={{ background: badge.bg, color: badge.color }}
                      >
                        {badge.label}
                      </span>
                    </td>
                    <td>
                      <span className="cell-date">
                        <Calendar size={12} />
                        {formatDate(p.dateDebut)}
                      </span>
                    </td>
                    <td>
                      {p.dateFin ? (
                        <span className="cell-date">
                          <Calendar size={12} />
                          {formatDate(p.dateFin)}
                        </span>
                      ) : (
                        <span className="cell-muted">—</span>
                      )}
                    </td>
                    <td>
                      <div className="projets-actions">
                        <button
                          className="action-btn action-edit"
                          title="Modifier"
                          onClick={() => ouvrirEdition(p)}
                        >
                          <Pencil size={14} />
                        </button>
                        <button
                          className="action-btn action-delete"
                          title="Supprimer"
                          onClick={() => setDeleteTarget(p)}
                        >
                          <Trash2 size={14} />
                        </button>
                      </div>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}

      {/* Modal formulaire */}
      {modalOpen && (
        <div className="modal-overlay" onClick={() => setModalOpen(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>
                <FolderOpen size={18} />
                {editingId ? 'Modifier le projet' : 'Nouveau projet / dotation'}
              </h3>
              <button className="modal-close" onClick={() => setModalOpen(false)}>
                <X size={18} />
              </button>
            </div>

            <form className="modal-form" onSubmit={handleSubmit}>
              {formErrors.submit && (
                <div className="alert alert-danger">{formErrors.submit}</div>
              )}

              <div className="form-grid">
                <div className="form-group full-width">
                  <label>Intitulé *</label>
                  <input
                    type="text"
                    value={form.intitule}
                    onChange={(e) => setForm({ ...form, intitule: e.target.value })}
                    placeholder="Ex: Construction école primaire"
                    className={formErrors.intitule ? 'input-error' : ''}
                  />
                  {formErrors.intitule && <span className="field-error">{formErrors.intitule}</span>}
                </div>

                <div className="form-group full-width">
                  <label>Collectivité bénéficiaire *</label>
                  <select
                    value={form.collectiviteId}
                    onChange={(e) => setForm({ ...form, collectiviteId: e.target.value })}
                    className={formErrors.collectiviteId ? 'input-error' : ''}
                  >
                    <option value="">— Sélectionner —</option>
                    {collectivites.map((c) => (
                      <option key={c.id} value={c.id}>{c.nom} ({c.codeAdministratif})</option>
                    ))}
                  </select>
                  {formErrors.collectiviteId && <span className="field-error">{formErrors.collectiviteId}</span>}
                </div>

                <div className="form-group">
                  <label>Montant *</label>
                  <div className="input-with-icon">
                    <DollarSign size={14} />
                    <input
                      type="number"
                      min="0"
                      step="0.01"
                      value={form.montant}
                      onChange={(e) => setForm({ ...form, montant: parseFloat(e.target.value) || 0 })}
                      className={formErrors.montant ? 'input-error' : ''}
                    />
                  </div>
                  {formErrors.montant && <span className="field-error">{formErrors.montant}</span>}
                </div>

                <div className="form-group">
                  <label>Devise *</label>
                  <select
                    value={form.devise}
                    onChange={(e) => setForm({ ...form, devise: e.target.value })}
                  >
                    {DEVISES.map((d) => (
                      <option key={d} value={d}>{d}</option>
                    ))}
                  </select>
                </div>

                <div className="form-group">
                  <label>Statut *</label>
                  <select
                    value={form.statut}
                    onChange={(e) => setForm({ ...form, statut: e.target.value })}
                  >
                    {STATUTS.filter((s) => s.value).map((s) => (
                      <option key={s.value} value={s.value}>{s.label}</option>
                    ))}
                  </select>
                </div>

                <div className="form-group">
                  <label>Date de début *</label>
                  <input
                    type="date"
                    value={form.dateDebut}
                    onChange={(e) => setForm({ ...form, dateDebut: e.target.value })}
                    className={formErrors.dateDebut ? 'input-error' : ''}
                  />
                  {formErrors.dateDebut && <span className="field-error">{formErrors.dateDebut}</span>}
                </div>

                <div className="form-group">
                  <label>Date de fin</label>
                  <input
                    type="date"
                    value={form.dateFin || ''}
                    onChange={(e) => setForm({ ...form, dateFin: e.target.value || null })}
                    className={formErrors.dateFin ? 'input-error' : ''}
                  />
                  {formErrors.dateFin && <span className="field-error">{formErrors.dateFin}</span>}
                </div>
              </div>

              <div className="modal-actions">
                <button type="button" className="btn-cancel" onClick={() => setModalOpen(false)}>
                  Annuler
                </button>
                <button type="submit" className="btn-primary" disabled={saving}>
                  {saving ? 'Enregistrement…' : editingId ? 'Mettre à jour' : 'Créer'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal suppression */}
      {deleteTarget && (
        <div className="modal-overlay" onClick={() => setDeleteTarget(null)}>
          <div className="modal-content modal-sm" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header modal-header-danger">
              <h3>
                <AlertTriangle size={18} />
                Confirmer la suppression
              </h3>
              <button className="modal-close" onClick={() => setDeleteTarget(null)}>
                <X size={18} />
              </button>
            </div>
            <div className="modal-body">
              <p>
                Voulez-vous vraiment supprimer le projet <strong>« {deleteTarget.intitule} »</strong> ?
              </p>
              <p className="text-muted">Cette action est irréversible.</p>
            </div>
            <div className="modal-actions">
              <button className="btn-cancel" onClick={() => setDeleteTarget(null)}>
                Annuler
              </button>
              <button className="btn-danger" onClick={handleDelete} disabled={deleting}>
                {deleting ? 'Suppression…' : 'Supprimer'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
