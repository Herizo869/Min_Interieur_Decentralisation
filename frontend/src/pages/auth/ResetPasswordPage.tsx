import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { MapPin, ArrowLeft, Eye, EyeOff, Loader2, CheckCircle } from 'lucide-react'
import { resetPassword } from '../../services/authService'

const schema = z.object({
  identifiant: z.string().min(1, 'L\'identifiant est requis'),
  token: z.string().min(1, 'Le token est requis'),
  nouveauMotDePasse: z
    .string()
    .min(8, 'Le mot de passe doit contenir au moins 8 caractères'),
  confirmation: z.string().min(1, 'La confirmation est requise'),
}).refine((data) => data.nouveauMotDePasse === data.confirmation, {
  message: 'Les mots de passe ne correspondent pas',
  path: ['confirmation'],
})

type FormData = z.infer<typeof schema>

export default function ResetPasswordPage() {
  const [showPassword, setShowPassword] = useState(false)
  const [success, setSuccess] = useState(false)
  const [serverError, setServerError] = useState('')
  const [loading, setLoading] = useState(false)

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
      await resetPassword({
        identifiant: data.identifiant,
        token: data.token,
        nouveauMotDePasse: data.nouveauMotDePasse,
      })
      setSuccess(true)
    } catch {
      setServerError('Token invalide ou expiré.')
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
          <h1>Mot de passe réinitialisé</h1>
          <p className="auth-subtitle">
            Votre mot de passe a été modifié avec succès. Vous pouvez vous reconnecter.
          </p>
          <Link to="/login" className="btn-primary" style={{ display: 'block', textAlign: 'center', marginTop: 16 }}>
            Se connecter
          </Link>
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
        <h1>Réinitialiser le mot de passe</h1>
        <p className="auth-subtitle">
          Saisissez votre identifiant, le token reçu et votre nouveau mot de passe.
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

          <div className="form-group">
            <label htmlFor="token">Token de réinitialisation</label>
            <input
              id="token"
              type="text"
              placeholder="Token reçu par email"
              {...register('token')}
            />
            {errors.token && (
              <span className="form-error">{errors.token.message}</span>
            )}
          </div>

          <div className="form-group">
            <label htmlFor="nouveauMotDePasse">Nouveau mot de passe</label>
            <div className="password-wrapper">
              <input
                id="nouveauMotDePasse"
                type={showPassword ? 'text' : 'password'}
                placeholder="Minimum 8 caractères"
                autoComplete="new-password"
                {...register('nouveauMotDePasse')}
              />
              <button
                type="button"
                className="password-toggle"
                onClick={() => setShowPassword(!showPassword)}
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {errors.nouveauMotDePasse && (
              <span className="form-error">{errors.nouveauMotDePasse.message}</span>
            )}
          </div>

          <div className="form-group">
            <label htmlFor="confirmation">Confirmer le mot de passe</label>
            <input
              id="confirmation"
              type={showPassword ? 'text' : 'password'}
              placeholder="Confirmez le mot de passe"
              autoComplete="new-password"
              {...register('confirmation')}
            />
            {errors.confirmation && (
              <span className="form-error">{errors.confirmation.message}</span>
            )}
          </div>

          <button type="submit" className="btn-primary" disabled={loading}>
            {loading ? (
              <>
                <Loader2 size={18} className="spin" />
                Réinitialisation...
              </>
            ) : (
              'Réinitialiser le mot de passe'
            )}
          </button>
        </form>

        <div className="auth-links">
          <Link to="/login">
            <ArrowLeft size={16} />
            Retour à la connexion
          </Link>
          <Link to="/forgot-password">
            Demander un nouveau token
          </Link>
        </div>
      </div>
    </div>
  )
}
