import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { MapPin, ArrowLeft, Loader2, CheckCircle } from 'lucide-react'
import { forgotPassword } from '../../services/authService'

const schema = z.object({
  identifiant: z.string().min(1, 'L\'identifiant est requis'),
})

type FormData = z.infer<typeof schema>

export default function ForgotPasswordPage() {
  const [success, setSuccess] = useState(false)
  const [serverError, setServerError] = useState('')
  const [loading, setLoading] = useState(false)
  const [generatedToken, setGeneratedToken] = useState('')

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  async function onSubmit(data: FormData) {
    setServerError('')
    setLoading(true)
    try {
      const response = await forgotPassword(data)
      setSuccess(true)
      // En dev le token est renvoyé dans la réponse
      if (response.token) {
        setGeneratedToken(response.token)
      }
    } catch {
      setServerError('Une erreur est survenue. Veuillez réessayer.')
    } finally {
      setLoading(false)
    }
  }

  if (success) {
    return (
      <div className="auth-page">
        <div className="auth-card">
          <div className="auth-logo">
            <CheckCircle size={40} className="text-success" />
          </div>
          <h1>Demande envoyée</h1>
          <p className="auth-subtitle">
            Si cet identifiant existe, un token de réinitialisation a été généré.
          </p>

          {generatedToken && (
            <div className="auth-info">
              <strong>Token de réinitialisation :</strong>
              <code>{generatedToken}</code>
              <small>(En production, ce token serait envoyé par email)</small>
            </div>
          )}

          <Link to="/reset-password" className="btn-primary" style={{ display: 'block', textAlign: 'center', marginTop: 16 }}>
            Réinitialiser le mot de passe
          </Link>

          <div className="auth-links">
            <Link to="/login">
              <ArrowLeft size={16} />
              Retour à la connexion
            </Link>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-logo">
          <MapPin size={40} />
        </div>
        <h1>Mot de passe oublié</h1>
        <p className="auth-subtitle">
          Saisissez votre identifiant pour recevoir un token de réinitialisation.
        </p>

        {serverError && (
          <div className="auth-error">{serverError}</div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} className="auth-form">
          <div className="form-group">
            <label htmlFor="identifiant">Identifiant</label>
            <input
              id="identifiant"
              type="text"
              placeholder="Votre identifiant"
              autoComplete="username"
              {...register('identifiant')}
            />
            {errors.identifiant && (
              <span className="form-error">{errors.identifiant.message}</span>
            )}
          </div>

          <button type="submit" className="btn-primary" disabled={loading}>
            {loading ? (
              <>
                <Loader2 size={18} className="spin" />
                Envoi en cours...
              </>
            ) : (
              'Envoyer la demande'
            )}
          </button>
        </form>

        <div className="auth-links">
          <Link to="/login">
            <ArrowLeft size={16} />
            Retour à la connexion
          </Link>
        </div>
      </div>
    </div>
  )
}
