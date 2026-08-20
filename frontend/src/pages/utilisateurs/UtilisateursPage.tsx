import { useState, useEffect, useCallback } from 'react'
import {
  Users, Plus, Search, Filter, Edit2, Trash2, X, Loader2, Shield, UserCheck, UserX
} from 'lucide-react'
import {
  useUtilisateurs,
  type Utilisateur,
  type CreerUtilisateurRequest,
  type ModifierUtilisateurRequest,
} from '../../hooks/useUtilisateurs'
import { useAuth } from '../../contexts/AuthContext'

const ROLES = ['Agent', 'Administrateur'] as const

const ROLE_CONFIG: Record<string, { label: string; color: string; bg: string }> = {
  Agent:          { label: 'Agent',          color: '#2563eb', bg: '#eff6ff' },
  Administrateur: { label: 'Administrateur', color: '#9333ea', bg: '#faf5ff' },
}

type FormData = CreerUtilisateurRequest

const EMPTY_FORM: FormData = {
  nom: '',
  identifiant: '',
  motDePasse: '',
  role: 'Agent',
  collectiviteIds: [],
}

export default function UtilisateursPage() {
  const { lister, creer, modifier, desactiver } = useUtilisateurs()
  const { user: currentUser } = useAuth()

  const [utilisateurs, setUtilisateurs] = useState<Utilisateur[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Filtres
  const [search, setSearch] = useState('')
  const [roleFilter, setRoleFilter] = useState('')

  // Modal
  const [modalOpen, setModalOpen] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState<FormData>(EMPTY_FORM)
  const [formErrors, setFormErrors] = useState<Record<string, string>>({})
  const [saving, setSaving] = useState(false)
  const [saveError, setSaveError] = useState<string | null>(null)

  // Suppression
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null)

  const fetchUtilisateurs = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await lister()
      setUtilisateurs(data)
    } catch {
      setError('Erreur lors du chargement des utilisateurs.')
    } finally {
      setLoading(false)
    }
  }, [lister])

  useEffect(() => { fetchUtilisateurs() }, [fetchUtilisateurs])

  // Filtrage
  const filtered = utilisateurs.filter((u) => {
    if (search) {
      const q = search.toLowerCase()
      if (!u.nom.toLowerCase().includes(q) && !u.identifiant.toLowerCase().includes(q)) return false
    }
    if (roleFilter && u.role !== roleFilter) return false
    return true
  })

  // Modal — ouvrir création
  function openCreate() {
    setEditingId(null)
    setForm(EMPTY_FORM)
    setFormErrors({})
    setSaveError(null)
    setModalOpen(true)
  }

  // Modal — ouvrir modification
  function openEdit(u: Utilisateur) {
    setEditingId(u.id)
    setForm({
      nom: u.nom,
      identifiant: u.identifiant,
      motDePasse: '',
      role: u.role,
      collectiviteIds: u.collectiviteIds || [],
    })
    setFormErrors({})
    setSaveError(null)
    setModalOpen(true)
  }

  function closeModal() {
    setModalOpen(false)
    setEditingId(null)
    setForm(EMPTY_FORM)
    setFormErrors({})
    setSaveError(null)
  }

  // Validation
  function validate(): boolean {
    const errs: Record<string, string> = {}
    if (!form.nom.trim()) errs.nom = 'Le nom est obligatoire.'
    if (!form.identifiant.trim()) errs.identifiant = "L'identifiant est obligatoire."
    if (!editingId && !form.motDePasse) errs.motDePasse = 'Le mot de passe est obligatoire.'
    if (form.motDePasse && form.motDePasse.length < 6) errs.motDePasse = 'Minimum 6 caractères.'
    setFormErrors(errs)
    return Object.keys(errs).length === 0
  }

  // Soumission
  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!validate()) return
    setSaving(true)
    setSaveError(null)
    try {
      if (editingId) {
        const data: ModifierUtilisateurRequest = {
          nom: form.nom.trim(),
          role: form.role,
          actif: true,
          motDePasse: form.motDePasse || undefined,
          collectiviteIds: form.collectiviteIds,
        }
        const updated = await modifier(editingId, data)
        setUtilisateurs((prev) => prev.map((u) => (u.id === editingId ? updated : u)))
      } else {
        const data: CreerUtilisateurRequest = {
          nom: form.nom.trim(),
          identifiant: form.identifiant.trim(),
          motDePasse: form.motDePasse,
          role: form.role,
          collectiviteIds: form.collectiviteIds,
        }
        const created = await creer(data)
        setUtilisateurs((prev) => [...prev, created])
      }
      closeModal()
    } catch (err: unknown) {
      let msg = "Une erreur est survenue."
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { data?: { message?: string } } }
        msg = axiosErr.response?.data?.message || msg
      }
      setSaveError(msg)
    } finally {
      setSaving(false)
    }
  }

  // Désactiver
  async function handleDesactiver(id: string) {
    setDeletingId(id)
    try {
      await desactiver(id)
      setUtilisateurs((prev) => prev.map((u) => (u.id === id ? { ...u, actif: false } : u)))
      setDeleteConfirm(null)
    } catch {
      // silently fail
    } finally {
      setDeletingId(null)
    }
  }

  return (
    <div className="utilisateurs-page">
      {/* Header */}
      <div className="utilisateurs-header">
        <div>
          <h2>
            <Users size={22} />
            Gestion des utilisateurs
          </h2>
          <p>Créer, modifier et gérer les comptes agents et administrateurs</p>
        </div>
        <button className="btn-primary" onClick={openCreate}>
          <Plus size={16} />
          Nouvel utilisateur
        </button>
      </div>

      {/* Filtres */}
      <div className="utilisateurs-filters">
        <div className="utilisateurs-search">
          <Search size={16} />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Rechercher par nom ou identifiant…"
          />
          {search && (
            <button className="filter-clear" onClick={() => setSearch('')}>
              <X size={12} />
            </button>
          )}
        </div>
        <div className="utilisateurs-filter-group">
          <Filter size={14} />
          <select value={roleFilter} onChange={(e) => setRoleFilter(e.target.value)}>
            <option value="">Tous les rôles</option>
            {ROLES.map((r) => (
              <option key={r} value={r}>{ROLE_CONFIG[r]?.label || r}</option>
            ))}
          </select>
        </div>
        <span className="utilisateurs-count">
          {filtered.length} utilisateur{filtered.length !== 1 ? 's' : ''}
        </span>
      </div>

      {/* Erreur */}
      {error && (
        <div className="alert alert-danger">
          {error}
        </div>
      )}

      {/* Loading */}
      {loading && (
        <div className="utilisateurs-loading">
          <Loader2 size={28} className="spin" />
          <span>Chargement…</span>
        </div>
      )}

      {/* Empty */}
      {!loading && !error && filtered.length === 0 && (
        <div className="utilisateurs-empty">
          <Users size={36} />
          <h3>Aucun utilisateur trouvé</h3>
          <p>Créez le premier compte utilisateur.</p>
        </div>
      )}

      {/* Tableau */}
      {!loading && !error && filtered.length > 0 && (
        <div className="utilisateurs-table-wrapper">
          <table className="utilisateurs-table">
            <thead>
              <tr>
                <th>Nom</th>
                <th>Identifiant</th>
                <th>Rôle</th>
                <th>État</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((u) => {
                const roleCfg = ROLE_CONFIG[u.role] || ROLE_CONFIG.Agent
                return (
                  <tr key={u.id} className={!u.actif ? 'row-inactive' : ''}>
                    <td>
                      <div className="utilisateur-nom">
                        <span className="utilisateur-avatar">
                          {u.role === 'Administrateur' ? <Shield size={14} /> : <UserCheck size={14} />}
                        </span>
                        <span className="cell-bold">{u.nom}</span>
                      </div>
                    </td>
                    <td><code className="utilisateur-identifiant">{u.identifiant}</code></td>
                    <td>
                      <span className="utilisateurs-badge" style={{ background: roleCfg.bg, color: roleCfg.color }}>
                        {u.role === 'Administrateur' ? <Shield size={11} /> : <UserCheck size={11} />}
                        {roleCfg.label}
                      </span>
                    </td>
                    <td>
                      <span className={`utilisateurs-actif ${u.actif ? 'actif' : 'inactif'}`}>
                        {u.actif ? 'Actif' : 'Inactif'}
                      </span>
                    </td>
                    <td>
                      <div className="utilisateurs-actions">
                        <button
                          className="action-btn action-edit"
                          title="Modifier"
                          onClick={() => openEdit(u)}
                        >
                          <Edit2 size={14} />
                        </button>
                        {u.id !== currentUser?.id && u.actif && (
                          <button
                            className="action-btn action-delete"
                            title="Désactiver"
                            onClick={() => setDeleteConfirm(u.id)}
                            disabled={deletingId === u.id}
                          >
                            {deletingId === u.id ? <Loader2 size={14} className="spin" /> : <UserX size={14} />}
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}

      {/* ═══════════ Modal création / modification ═══════════ */}
      {modalOpen && (
        <div className="modal-overlay" onClick={(e) => { if (e.target === e.currentTarget) closeModal() }}>
          <div className="modal-content">
            <div className="modal-header">
              <h3>
                {editingId ? <Edit2 size={18} /> : <Plus size={18} />}
                {editingId ? 'Modifier l\'utilisateur' : 'Nouvel utilisateur'}
              </h3>
              <button className="modal-close" onClick={closeModal}>
                <X size={18} />
              </button>
            </div>

            {saveError && (
              <div className="alert alert-danger" style={{ margin: '16px 22px 0' }}>
                {saveError}
              </div>
            )}

            <form className="modal-form" onSubmit={handleSubmit}>
              <div className="form-grid">
                {/* Nom */}
                <div className="form-group">
                  <label htmlFor="util-nom">Nom complet *</label>
                  <input
                    id="util-nom"
                    type="text"
                    value={form.nom}
                    onChange={(e) => setForm((p) => ({ ...p, nom: e.target.value }))}
                    placeholder="Ex: Jean Rakoto"
                    className={formErrors.nom ? 'input-error' : ''}
                  />
                  {formErrors.nom && <span className="field-error">{formErrors.nom}</span>}
                </div>

                {/* Identifiant */}
                <div className="form-group">
                  <label htmlFor="util-identifiant">Identifiant *</label>
                  <input
                    id="util-identifiant"
                    type="text"
                    value={form.identifiant}
                    onChange={(e) => setForm((p) => ({ ...p, identifiant: e.target.value }))}
                    placeholder="Ex: jrakoto"
                    disabled={!!editingId}
                    className={formErrors.identifiant ? 'input-error' : ''}
                  />
                  {formErrors.identifiant && <span className="field-error">{formErrors.identifiant}</span>}
                </div>

                {/* Mot de passe */}
                <div className="form-group">
                  <label htmlFor="util-mdp">
                    Mot de passe {editingId ? '(laisser vide = inchangé)' : '*'}
                  </label>
                  <input
                    id="util-mdp"
                    type="password"
                    value={form.motDePasse}
                    onChange={(e) => setForm((p) => ({ ...p, motDePasse: e.target.value }))}
                    placeholder={editingId ? '••••••••' : 'Minimum 6 caractères'}
                    className={formErrors.motDePasse ? 'input-error' : ''}
                  />
                  {formErrors.motDePasse && <span className="field-error">{formErrors.motDePasse}</span>}
                </div>

                {/* Rôle */}
                <div className="form-group">
                  <label htmlFor="util-role">Rôle *</label>
                  <select
                    id="util-role"
                    value={form.role}
                    onChange={(e) => setForm((p) => ({ ...p, role: e.target.value }))}
                  >
                    {ROLES.map((r) => (
                      <option key={r} value={r}>{ROLE_CONFIG[r]?.label || r}</option>
                    ))}
                  </select>
                </div>
              </div>

              <div className="modal-footer">
                <button type="button" className="btn-outline" onClick={closeModal}>
                  Annuler
                </button>
                <button type="submit" className="btn-primary" disabled={saving}>
                  {saving ? <Loader2 size={14} className="spin" /> : null}
                  {saving ? 'Enregistrement…' : editingId ? 'Enregistrer' : 'Créer'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ═══════════ Confirmation suppression ═══════════ */}
      {deleteConfirm && (
        <div className="modal-overlay" onClick={(e) => { if (e.target === e.currentTarget) setDeleteConfirm(null) }}>
          <div className="modal-content modal-sm">
            <div className="modal-header modal-header-danger">
              <h3>
                <UserX size={18} />
                Désactiver le compte
              </h3>
              <button className="modal-close" onClick={() => setDeleteConfirm(null)}>
                <X size={18} />
              </button>
            </div>
            <div className="modal-body">
              <p>Voulez-vous vraiment désactiver ce compte utilisateur ?</p>
              <p className="text-muted">L'utilisateur ne pourra plus se connecter. Cette action est réversible en modifiant le compte.</p>
            </div>
            <div className="modal-footer" style={{ padding: '0 22px 22px' }}>
              <button className="btn-outline" onClick={() => setDeleteConfirm(null)}>
                Annuler
              </button>
              <button
                className="btn-primary"
                style={{ background: 'linear-gradient(135deg, #dc2626, #b91c1c)' }}
                disabled={deletingId !== null}
                onClick={() => deleteConfirm && handleDesactiver(deleteConfirm)}
              >
                {deletingId ? <Loader2 size={14} className="spin" /> : <UserX size={14} />}
                {deletingId ? 'Désactivation…' : 'Désactiver'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
