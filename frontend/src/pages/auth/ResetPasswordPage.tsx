import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Eye, EyeOff, Loader2, ArrowLeft, ArrowRight, CheckCircle, ShieldCheck } from 'lucide-react'
import AuthLayout from '../../components/layout/AuthLayout'
import { resetPassword } from '../../services/authService'

const schema = z.object({
  identifiant: z.string().min(1, "L'identifiant est requis"),
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

  return (
    <AuthLayout>
      <div className="form-panel-inner">
        <div className="form-panel-header">
          <h2>Nouveau mot de passe</h2>
          <p>
            {success
              ? 'Votre mot de passe a été modifié'
              : 'Saisissez le token reçu et votre nouveau mot de passe'}
          </p>
        </div>

        {success ? (
          <div className="success-state">
            <div className="success-icon-wrap success">
              <ShieldCheck size={48} />
            </div>
            <h3>Mot de passe réinitialisé</h3>
            <p className="success-text">
              Votre mot de passe a été modifié avec succès. Vous pouvez maintenant vous reconnecter.
            </p>
            <Link to="/login" className="btn-modern" style={{ marginTop: 20 }}>
              Se connecter
              <ArrowRight size={18} />
            </Link>
          </div>
        ) : (
          <>
            {serverError && (
              <div className="alert alert-danger">{serverError}</div>
            )}

            <form onSubmit={handleSubmit(onSubmit)} className="modern-form">
              <div className="input-group">
                <label htmlFor="identifiant">Identifiant</label>
                <div className="input-wrapper">
                  <input
                    id="identifiant"
                    type="text"
                    placeholder="Votre identifiant"
                    autoComplete="username"
                    className={errors.identifiant ? 'input-error' : ''}
                    {...register('identifiant')}
                  />
                </div>
                {errors.identifiant && (
                  <span className="field-error">{errors.identifiant.message}</span>
                )}
              </div>

              <div className="input-group">
                <label htmlFor="token">Token de réinitialisation</label>
                <div className="input-wrapper">
                  <input
                    id="token"
                    type="text"
                    placeholder="Collez le token reçu"
                    className={errors.token ? 'input-error' : ''}
                    {...register('token')}
                  />
                </div>
                {errors.token && (
                  <span className="field-error">{errors.token.message}</span>
                )}
              </div>

              <div className="input-group">
                <label htmlFor="nouveauMotDePasse">Nouveau mot de passe</label>
                <div className="input-wrapper">
                  <input
                    id="nouveauMotDePasse"
                    type={showPassword ? 'text' : 'password'}
                    placeholder="Minimum 8 caractères"
                    autoComplete="new-password"
                    className={errors.nouveauMotDePasse ? 'input-error' : ''}
                    {...register('nouveauMotDePasse')}
                  />
                  <button
                    type="button"
                    className="input-icon-btn"
                    onClick={() => setShowPassword(!showPassword)}
                    tabIndex={-1}
                  >
                    {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>
                {errors.nouveauMotDePasse && (
                  <span className="field-error">{errors.nouveauMotDePasse.message}</span>
                )}
              </div>

              <div className="input-group">
                <label htmlFor="confirmation">Confirmer le mot de passe</label>
                <div className="input-wrapper">
                  <input
                    id="confirmation"
                    type={showPassword ? 'text' : 'password'}
                    placeholder="Confirmez le mot de passe"
                    autoComplete="new-password"
                    className={errors.confirmation ? 'input-error' : ''}
                    {...register('confirmation')}
                  />
                </div>
                {errors.confirmation && (
                  <span className="field-error">{errors.confirmation.message}</span>
                )}
              </div>

              <button type="submit" className="btn-modern" disabled={loading}>
                {loading ? (
                  <>
                    <Loader2 size={18} className="spin" />
                    Réinitialisation...
                  </>
                ) : (
                  <>
                    Réinitialiser le mot de passe
                    <ArrowRight size={18} />
                  </>
                )}
              </button>
            </form>

            <div className="form-footer-links">
              <Link to="/login" className="btn-outline">
                <ArrowLeft size={16} />
                Retour à la connexion
              </Link>
              <Link to="/forgot-password" className="link-sm">
                Demander un nouveau token
              </Link>
            </div>
          </>
        )}
      </div>
    </AuthLayout>
  )
}
