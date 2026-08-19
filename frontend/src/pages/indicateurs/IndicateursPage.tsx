import { useState, useEffect, useCallback } from 'react'
import {
  Plus, Pencil, Trash2, X, BarChart3, Calendar, Building2,
  Filter, Search, AlertTriangle, Hash
} from 'lucide-react'
import { useIndicateurs, type Indicateur, type IndicateurRequest } from '../../hooks/useIndicateurs'
import { useCollectivites, type Collectivite } from '../../hooks/useCollectivites'

const TYPES_INDICATEUR = [
  'Population', 'Budget', 'Taux', 'Superficie', 'Densité', 'Autre'
]

const EMPTY_FORM: IndicateurRequest = {
  type: '',
  valeur: 0,
  unite: '',
  source: '',
  dateReleve: new Date().toISOString().split('T')[0],
  collectiviteId: '',
}

function formatValeur(n: number, unite: string) {
  const formatted = new Intl.NumberFormat('fr-FR', { maximumFractionDigits: 2 }).format(n)
  return unite ? `${formatted} ${unite}` : formatted
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('fr-FR', { day: '2-digit', month: 'short', year: 'numeric' })
}

export default function IndicateursPage() {
  const { lister, creer, modifier, supprimer } = useIndicateurs()
  const { rechercher } = useCollectivites()

  const [indicateurs, setIndicateurs] = useState<Indicateur[]>([])
  const [collectivites, setCollectivites] = useState<Collectivite[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Filtres
  const [filtreType, setFiltreType] = useState('')
  const [filtreCollectivite, setFiltreCollectivite] = useState('')
  const [recherche, setRecherche] = useState('')

  // Modal
  const [modalOpen, setModalOpen] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState<IndicateurRequest>(EMPTY_FORM)
  const [formErrors, setFormErrors] = useState<Record<string, string>>({})
  const [saving, setSaving] = useState(false)

  // Suppression
  const [deleteTarget, setDeleteTarget] = useState<Indicateur | null>(null)
  const [deleting, setDeleting] = useState(false)

  // Types découverts depuis les données
  const [typesDisponibles, setTypesDisponibles] = useState<string[]>(TYPES_INDICATEUR)

  const charger = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const [ind, c] = await Promise.all([lister(), rechercher()])
      setIndicateurs(ind)
      setCollectivites(c)
      // Enrichir les types depuis les données existantes
      const typesFromData = [...new Set(ind.map((i) => i.type))].filter(Boolean)
      setTypesDisponibles([...new Set([...TYPES_INDICATEUR, ...typesFromData])])
    } catch {
      setError('Erreur lors du chargement des indicateurs.')
    } finally {
      setLoading(false)
    }
  }, [lister, rechercher])

  useEffect(() => { charger() }, [charger])

  // Filtrage côté client
  const indicateursFiltres = indicateurs.filter((i) => {
    if (filtreType && i.type !== filtreType) return false
    if (filtreCollectivite && i.collectiviteId !== filtreCollectivite) return false
    if (recherche.trim()) {
      const term = recherche.trim().toLowerCase()
      if (
        !i.type.toLowerCase().includes(term) &&
        !i.source.toLowerCase().includes(term) &&
        !i.collectiviteNom.toLowerCase().includes(term)
      ) return false
    }
    return true
  })

  // Validation
  function validate(): boolean {
    const errs: Record<string, string> = {}
    if (!form.type.trim()) errs.type = 'Le type est obligatoire.'
    if (!form.unite.trim()) errs.unite = 'L\'unité est obligatoire.'
    if (!form.source.trim()) errs.source = 'La source est obligatoire.'
    if (!form.collectiviteId) errs.collectiviteId = 'Sélectionnez une collectivité.'
    if (!form.dateReleve) errs.dateReleve = 'La date de relevé est obligatoire.'
    setFormErrors(errs)
    return Object.keys(errs).length === 0
  }

  function ouvrirCreation() {
    setEditingId(null)
    setForm({ ...EMPTY_FORM, dateReleve: new Date().toISOString().split('T')[0] })
    setFormErrors({})
    setModalOpen(true)
  }

  function ouvrirEdition(i: Indicateur) {
    setEditingId(i.id)
    setForm({
      type: i.type,
      valeur: i.valeur,
      unite: i.unite,
      source: i.source,
      dateReleve: i.dateReleve.split('T')[0],
      collectiviteId: i.collectiviteId,
    })
    setFormErrors({})
    setModalOpen(true)
  }

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
    <div className="page indicateurs-page">
      {/* En-tête */}
      <div className="projets-header">
        <div>
          <h2>Indicateurs</h2>
          <p>Données chiffrées et statistiques des collectivités</p>
        </div>
        <button className="btn-primary" onClick={ouvrirCreation}>
          <Plus size={16} /> Nouvel indicateur
        </button>
      </div>

      {/* Filtres */}
      <div className="projets-filters">
        <div className="projets-search">
          <Search size={16} />
          <input
            type="text"
            placeholder="Rechercher par type, source ou collectivité…"
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
          <select value={filtreType} onChange={(e) => setFiltreType(e.target.value)}>
            <option value="">Tous les types</option>
            {typesDisponibles.map((t) => (
              <option key={t} value={t}>{t}</option>
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
          {indicateursFiltres.length} indicateur{indicateursFiltres.length !== 1 ? 's' : ''}
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
      ) : indicateursFiltres.length === 0 ? (
        <div className="projets-empty">
          <BarChart3 size={40} />
          <h3>Aucun indicateur trouvé</h3>
          <p>
            {recherche || filtreType || filtreCollectivite
              ? 'Modifiez vos filtres pour voir des résultats.'
              : 'Ajoutez votre premier indicateur chiffré.'}
          </p>
        </div>
      ) : (
        <div className="projets-table-wrapper">
          <table className="projets-table">
            <thead>
              <tr>
                <th>Type</th>
                <th>Valeur</th>
                <th>Source</th>
                <th>Collectivité</th>
                <th>Date de relevé</th>
                <th style={{ width: 100 }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {indicateursFiltres.map((i) => (
                <tr key={i.id}>
                  <td>
                    <span className="ind-type-badge">
                      <Hash size={12} />
                      {i.type}
                    </span>
                  </td>
                  <td className="cell-montant">{formatValeur(i.valeur, i.unite)}</td>
                  <td>
                    <span className="cell-source">{i.source}</span>
                  </td>
                  <td>
                    <span className="cell-collectivite">
                      <Building2 size={13} />
                      {i.collectiviteNom}
                    </span>
                  </td>
                  <td>
                    <span className="cell-date">
                      <Calendar size={12} />
                      {formatDate(i.dateReleve)}
                    </span>
                  </td>
                  <td>
                    <div className="projets-actions">
                      <button
                        className="action-btn action-edit"
                        title="Modifier"
                        onClick={() => ouvrirEdition(i)}
                      >
                        <Pencil size={14} />
                      </button>
                      <button
                        className="action-btn action-delete"
                        title="Supprimer"
                        onClick={() => setDeleteTarget(i)}
                      >
                        <Trash2 size={14} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
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
                <BarChart3 size={18} />
                {editingId ? 'Modifier l\'indicateur' : 'Nouvel indicateur'}
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
                <div className="form-group">
                  <label>Type *</label>
                  <input
                    type="text"
                    list="types-list"
                    value={form.type}
                    onChange={(e) => setForm({ ...form, type: e.target.value })}
                    placeholder="Ex: Population, Budget"
                    className={formErrors.type ? 'input-error' : ''}
                  />
                  <datalist id="types-list">
                    {typesDisponibles.map((t) => (
                      <option key={t} value={t} />
                    ))}
                  </datalist>
                  {formErrors.type && <span className="field-error">{formErrors.type}</span>}
                </div>

                <div className="form-group">
                  <label>Collectivité *</label>
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
                  <label>Valeur *</label>
                  <input
                    type="number"
                    step="0.01"
                    value={form.valeur}
                    onChange={(e) => setForm({ ...form, valeur: parseFloat(e.target.value) || 0 })}
                  />
                </div>

                <div className="form-group">
                  <label>Unité *</label>
                  <input
                    type="text"
                    value={form.unite}
                    onChange={(e) => setForm({ ...form, unite: e.target.value })}
                    placeholder="Ex: habitants, €, %"
                    className={formErrors.unite ? 'input-error' : ''}
                  />
                  {formErrors.unite && <span className="field-error">{formErrors.unite}</span>}
                </div>

                <div className="form-group">
                  <label>Source *</label>
                  <input
                    type="text"
                    value={form.source}
                    onChange={(e) => setForm({ ...form, source: e.target.value })}
                    placeholder="Ex: INSTAT, préfecture"
                    className={formErrors.source ? 'input-error' : ''}
                  />
                  {formErrors.source && <span className="field-error">{formErrors.source}</span>}
                </div>

                <div className="form-group">
                  <label>Date de relevé *</label>
                  <input
                    type="date"
                    value={form.dateReleve}
                    onChange={(e) => setForm({ ...form, dateReleve: e.target.value })}
                    className={formErrors.dateReleve ? 'input-error' : ''}
                  />
                  {formErrors.dateReleve && <span className="field-error">{formErrors.dateReleve}</span>}
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
                Voulez-vous vraiment supprimer l'indicateur <strong>« {deleteTarget.type} »</strong> ?
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
