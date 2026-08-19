import { MapPin, Shield, Map, BarChart3 } from 'lucide-react'

interface AuthLayoutProps {
  children: React.ReactNode
}

export default function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <div className="auth-split">
      {/* Panneau gauche — branding */}
      <div className="auth-brand">
        <div className="auth-brand-bg">
          <div className="floating-shapes">
            <div className="shape shape-1" />
            <div className="shape shape-2" />
            <div className="shape shape-3" />
          </div>
        </div>

        <div className="auth-brand-content">
          <div className="brand-logo">
            <div className="brand-icon">
              <MapPin size={32} />
            </div>
            <span className="brand-name">Collectivités</span>
          </div>

          <h1 className="brand-headline">
            Gestion intelligente des
            <span className="brand-highlight"> collectivités territoriales</span>
          </h1>

          <p className="brand-subtitle">
            Plateforme de suivi, d'analyse et de pilotage des territoires
          </p>

          <div className="brand-features">
            <div className="brand-feature">
              <div className="feature-icon"><Map size={20} /></div>
              <div>
                <strong>Carte interactive</strong>
                <span>Géolocalisation des collectivités</span>
              </div>
            </div>
            <div className="brand-feature">
              <div className="feature-icon"><BarChart3 size={20} /></div>
              <div>
                <strong>Tableaux de bord</strong>
                <span>Synthèses chiffrées en temps réel</span>
              </div>
            </div>
            <div className="brand-feature">
              <div className="feature-icon"><Shield size={20} /></div>
              <div>
                <strong>Sécurisé</strong>
                <span>Authentification JWT + RBAC</span>
              </div>
            </div>
          </div>
        </div>

        <div className="auth-brand-footer">
          © 2026 Plateforme Collectivités Territoriales
        </div>
      </div>

      {/* Panneau droit — formulaire */}
      <div className="auth-form-panel">
        {children}
      </div>
    </div>
  )
}
