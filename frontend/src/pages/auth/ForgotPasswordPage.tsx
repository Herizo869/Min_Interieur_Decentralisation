import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Loader2, ArrowLeft, ArrowRight, CheckCircle, Copy } from 'lucide-react'
import AuthLayout from '../../components/layout/AuthLayout'
import { forgotPassword } from '../../services/authService'

const schema = z.object({
  identifiant: z.string().min(1, "L'identifiant est requis"),
})

type FormData = z.infer<typeof schema>

export default function ForgotPasswordPage() {
  const [success, setSuccess] = useState(false)
  const [serverError, setServerError] = useState('')
  const [loading, setLoading] = useState(false)
  const [generatedToken, setGeneratedToken] = useState('')
  const [copied, setCopied] = useState(false)

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
      if (response.token) {
        setGeneratedToken(response.token)
      }
    } catch {
      setServerError('Une erreur est survenue. Veuillez réessayer.')
    } finally {
      setLoading(false)
    }
  }

  function copyToken() {
    if (generatedToken) {
      navigator.clipboard.writeText(generatedToken)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    }
  }

  return (
    <AuthLayout>
      <div className="form-panel-inner">
        <div className="form-panel-header">
          <h2>Récupération</h2>
          <p>
            {success
              ? 'Votre token de réinitialisation a été généré'
              : 'Saisissez votre identifiant pour recevoir un token'}
          </p>
        </div>

        {success ? (
          <div className="success-state">
            <div className="success-icon-wrap">
              <CheckCircle size={48} />
            </div>
            <h3>Demande envoyée</h3>
            <p className="success-text">
              Si cet identifiant existe, un token de réinitialisation a été généré.
            </p>

            {generatedToken && (
              <div className="token-box">
                <div className="token-label">Token de réinitialisation</div>
                <div className="token-value">
                  <code>{generatedToken}</code>
                  <button onClick={copyToken} className="copy-btn" title="Copier">
                    {copied ? <CheckCircle size={16} /> : <Copy size={16} />}
                  </button>
                </div>
                <small>En production, ce token serait envoyé par email</small>
              </div>
            )}

            <div className="success-actions">
              <Link to="/reset-password" className="btn-modern">
                Réinitialiser le mot de passe
                <ArrowRight size={18} />
              </Link>
              <Link to="/login" className="btn-outline">
                <ArrowLeft size={16} />
                Retour à la connexion
              </Link>
            </div>
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
                    placeholder="Entrez votre identifiant"
                    autoComplete="username"
                    className={errors.identifiant ? 'input-error' : ''}
                    {...register('identifiant')}
                  />
                </div>
                {errors.identifiant && (
                  <span className="field-error">{errors.identifiant.message}</span>
                )}
              </div>

              <button type="submit" className="btn-modern" disabled={loading}>
                {loading ? (
                  <>
                    <Loader2 size={18} className="spin" />
                    Envoi en cours...
                  </>
                ) : (
                  <>
                    Envoyer la demande
                    <ArrowRight size={18} />
                  </>
                )}
              </button>
            </form>

            <Link to="/login" className="btn-outline" style={{ marginTop: 16 }}>
              <ArrowLeft size={16} />
              Retour à la connexion
            </Link>
          </>
        )}
      </div>
    </AuthLayout>
  )
}
