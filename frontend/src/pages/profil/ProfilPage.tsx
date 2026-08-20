import { useState } from 'react'
import { useAuth } from '../../contexts/AuthContext'
import { useProfil } from '../../hooks/useProfil'

const ROLE_LABELS: Record<string, string> = {
  Administrateur: 'Administrateur',
  Agent: 'Agent',
}

export default function ProfilPage() {
  const { user, login } = useAuth()
  const { modifier } = useProfil()

  const [editing, setEditing] = useState(false)
  const [nom, setNom] = useState(user?.nom || '')
  const [motDePasse, setMotDePasse] = useState('')
  const [confirmation, setConfirmation] = useState('')
  const [loading, setLoading] = useState(false)
  const [success, setSuccess] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSave = async () => {
    if (!nom.trim()) {
      setError('Le nom est obligatoire.')
      return
    }
    if (motDePasse && motDePasse !== confirmation) {
      setError('Les mots de passe ne correspondent pas.')
      return
    }
    if (motDePasse && motDePasse.length < 8) {
      setError('Le mot de passe doit contenir au moins 8 caractères.')
      return
    }

    setLoading(true)
    setError(null)
    setSuccess(false)

    try {
      const payload: { nom: string; motDePasse?: string; confirmationMotDePasse?: string } = { nom }
      if (motDePasse) {
        payload.motDePasse = motDePasse
        payload.confirmationMotDePasse = confirmation
      }
      const result = await modifier(payload)

      // Mettre à jour le contexte d'auth si le nom a changé
      if (user && result.nom !== user.nom) {
        const updatedUser = { ...user, nom: result.nom }
        const token = localStorage.getItem('token')
        if (token) login(token, updatedUser)
      }

      setSuccess(true)
      setEditing(false)
      setMotDePasse('')
      setConfirmation('')
      setTimeout(() => setSuccess(false), 3000)
    } catch (err: unknown) {
      let message = 'Erreur lors de la mise à jour du profil.'
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { data?: { message?: string } } }
        message = axiosErr.response?.data?.message || message
      }
      setError(message)
    } finally {
      setLoading(false)
    }
  }

  const handleCancel = () => {
    setEditing(false)
    setNom(user?.nom || '')
    setMotDePasse('')
    setConfirmation('')
    setError(null)
  }

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Mon Profil</h1>
          <p className="page-subtitle">Consultez et modifiez vos informations</p>
        </div>
      </div>

      <div className="profil-card">
        {/* Avatar + info principale */}
        <div className="profil-header">
          <div className="profil-avatar">
            {user?.nom?.charAt(0).toUpperCase() || '?'}
          </div>
          <div className="profil-header-info">
            <div className="profil-name">{user?.nom}</div>
            <div className="profil-identifiant">@{user?.identifiant}</div>
            <div className="profil-role-badge">{ROLE_LABELS[user?.role || ''] || user?.role}</div>
          </div>
        </div>

        {/* Messages */}
        {success && (
          <div className="profil-success">
            ✅ Profil mis à jour avec succès.
          </div>
        )}
        {error && (
          <div className="profil-error">
            ⚠️ {error}
          </div>
        )}

        {/* Infos */}
        <div className="profil-info-grid">
          <div className="profil-info-item">
            <div className="profil-info-label">Nom complet</div>
            {editing ? (
              <input
                className="form-input"
                value={nom}
                onChange={(e) => setNom(e.target.value)}
                placeholder="Votre nom"
              />
            ) : (
              <div className="profil-info-value">{user?.nom}</div>
            )}
          </div>

          <div className="profil-info-item">
            <div className="profil-info-label">Identifiant</div>
            <div className="profil-info-value profil-info-muted">{user?.identifiant}</div>
          </div>

          <div className="profil-info-item">
            <div className="profil-info-label">Rôle</div>
            <div className="profil-info-value">{ROLE_LABELS[user?.role || ''] || user?.role}</div>
          </div>
        </div>

        {/* Formulaire mot de passe */}
        {editing && (
          <div className="profil-password-section">
            <h3 className="profil-section-title">Changer le mot de passe</h3>
            <p className="profil-section-hint">Laissez vide pour conserver le mot de passe actuel.</p>
            <div className="profil-info-grid">
              <div className="profil-info-item">
                <div className="profil-info-label">Nouveau mot de passe</div>
                <input
                  className="form-input"
                  type="password"
                  value={motDePasse}
                  onChange={(e) => setMotDePasse(e.target.value)}
                  placeholder="Min. 8 caractères"
                  autoComplete="new-password"
                />
              </div>
              <div className="profil-info-item">
                <div className="profil-info-label">Confirmer le mot de passe</div>
                <input
                  className="form-input"
                  type="password"
                  value={confirmation}
                  onChange={(e) => setConfirmation(e.target.value)}
                  placeholder="Retapez le mot de passe"
                  autoComplete="new-password"
                />
              </div>
            </div>
          </div>
        )}

        {/* Actions */}
        <div className="profil-actions">
          {editing ? (
            <>
              <button className="btn btn-secondary" onClick={handleCancel} disabled={loading}>
                Annuler
              </button>
              <button className="btn btn-primary" onClick={handleSave} disabled={loading}>
                {loading ? 'Enregistrement…' : 'Enregistrer'}
              </button>
            </>
          ) : (
            <button className="btn btn-primary" onClick={() => setEditing(true)}>
              Modifier le profil
            </button>
          )}
        </div>
      </div>
    </div>
  )
}
