import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Eye, EyeOff, Loader2, ArrowRight } from 'lucide-react'
import AuthLayout from '../../components/layout/AuthLayout'
import { login, type LoginRequest } from '../../services/authService'

const loginSchema = z.object({
  identifiant: z.string().min(1, "L'identifiant est requis"),
  motDePasse: z.string().min(1, 'Le mot de passe est requis'),
})

type LoginFormData = z.infer<typeof loginSchema>

export default function LoginPage() {
  const navigate = useNavigate()
  const [showPassword, setShowPassword] = useState(false)
  const [serverError, setServerError] = useState('')
  const [loading, setLoading] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  })

  async function onSubmit(data: LoginFormData) {
    setServerError('')
    setLoading(true)
    try {
      const payload: LoginRequest = {
        identifiant: data.identifiant,
        motDePasse: data.motDePasse,
      }
      const response = await login(payload)

      localStorage.setItem('token', response.token)
      localStorage.setItem('user', JSON.stringify({
        id: response.utilisateurId,
        nom: response.nom,
        identifiant: response.identifiant,
        role: response.role,
      }))

      navigate('/dashboard')
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { data?: { message?: string } } }
        setServerError(axiosErr.response?.data?.message || 'Erreur de connexion')
      } else {
        setServerError('Erreur réseau — impossible de joindre le serveur')
      }
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthLayout>
      <div className="form-panel-inner">
        <div className="form-panel-header">
          <h2>Bienvenue</h2>
          <p>Connectez-vous pour accéder à votre espace</p>
        </div>

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

          <div className="input-group">
            <div className="input-label-row">
              <label htmlFor="motDePasse">Mot de passe</label>
              <Link to="/forgot-password" className="link-sm">Mot de passe oublié ?</Link>
            </div>
            <div className="input-wrapper">
              <input
                id="motDePasse"
                type={showPassword ? 'text' : 'password'}
                placeholder="Entrez votre mot de passe"
                autoComplete="current-password"
                className={errors.motDePasse ? 'input-error' : ''}
                {...register('motDePasse')}
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
            {errors.motDePasse && (
              <span className="field-error">{errors.motDePasse.message}</span>
            )}
          </div>

          <button type="submit" className="btn-modern" disabled={loading}>
            {loading ? (
              <>
                <Loader2 size={18} className="spin" />
                Connexion en cours...
              </>
            ) : (
              <>
                Se connecter
                <ArrowRight size={18} />
              </>
            )}
          </button>
        </form>
      </div>
    </AuthLayout>
  )
}
