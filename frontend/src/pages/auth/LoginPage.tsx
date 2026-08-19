import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { MapPin, Eye, EyeOff, Loader2 } from 'lucide-react'
import { login, type LoginRequest } from '../../services/authService'

const loginSchema = z.object({
  identifiant: z.string().min(1, 'L\'identifiant est requis'),
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

      // Stocker le JWT et les infos utilisateur
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
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-logo">
          <MapPin size={40} />
        </div>
        <h1>Collectivités Territoriales</h1>
        <p className="auth-subtitle">Connectez-vous à votre compte</p>

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
            <label htmlFor="motDePasse">Mot de passe</label>
            <div className="password-wrapper">
              <input
                id="motDePasse"
                type={showPassword ? 'text' : 'password'}
                placeholder="Votre mot de passe"
                autoComplete="current-password"
                {...register('motDePasse')}
              />
              <button
                type="button"
                className="password-toggle"
                onClick={() => setShowPassword(!showPassword)}
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {errors.motDePasse && (
              <span className="form-error">{errors.motDePasse.message}</span>
            )}
          </div>

          <button type="submit" className="btn-primary" disabled={loading}>
            {loading ? (
              <>
                <Loader2 size={18} className="spin" />
                Connexion...
              </>
            ) : (
              'Se connecter'
            )}
          </button>
        </form>

        <div className="auth-links">
          <Link to="/forgot-password">Mot de passe oublié ?</Link>
        </div>
      </div>
    </div>
  )
}
