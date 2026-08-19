import { useLocation } from 'react-router-dom'
import { Bell, User } from 'lucide-react'
import { useAuth } from '../../contexts/AuthContext'

const pageTitles: Record<string, string> = {
  '/dashboard': 'Tableau de bord',
  '/collectivites': 'Collectivités',
  '/projets': 'Projets & Dotations',
  '/indicateurs': 'Indicateurs',
  '/litiges': 'Litiges de limites',
  '/doleances': 'Doléances citoyennes',
  '/exports': 'Exports CSV',
  '/utilisateurs': 'Gestion des utilisateurs',
}

export default function Header() {
  const location = useLocation()
  const { user } = useAuth()
  const title = pageTitles[location.pathname] || 'Collectivités territoriales'

  return (
    <header className="app-header">
      <h1 className="header-title">{title}</h1>
      <div className="header-actions">
        <button className="header-icon-btn" title="Notifications">
          <Bell size={20} />
        </button>
        <div className="header-user">
          <User size={18} />
          <span>{user?.nom || 'Agent'}</span>
          {user?.role && (
            <span className="user-role">{user.role}</span>
          )}
        </div>
      </div>
    </header>
  )
}
