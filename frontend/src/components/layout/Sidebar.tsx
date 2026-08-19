import { NavLink, useNavigate } from 'react-router-dom'
import {
  LayoutDashboard,
  Map,
  FolderKanban,
  BarChart3,
  AlertTriangle,
  MessageSquareWarning,
  Download,
  Users,
  LogOut,
  MapPin,
} from 'lucide-react'
import { logout } from '../../utils/auth'

const navItems = [
  { to: '/dashboard', label: 'Tableau de bord', icon: LayoutDashboard },
  { to: '/collectivites', label: 'Collectivités', icon: Map },
  { to: '/projets', label: 'Projets & Dotations', icon: FolderKanban },
  { to: '/indicateurs', label: 'Indicateurs', icon: BarChart3 },
  { to: '/litiges', label: 'Litiges', icon: AlertTriangle },
  { to: '/doleances', label: 'Doléances', icon: MessageSquareWarning },
  { to: '/exports', label: 'Exports', icon: Download },
  { to: '/utilisateurs', label: 'Utilisateurs', icon: Users },
]

export default function Sidebar() {
  const navigate = useNavigate()

  function handleLogout() {
    // Nettoyer le localStorage
    localStorage.removeItem('token')
    localStorage.removeItem('user')

    // Remplacer l'historique pour bloquer le bouton retour
    window.history.replaceState(null, '', '/login')
    navigate('/login', { replace: true })
  }

  return (
    <aside className="sidebar">
      <div className="sidebar-brand">
        <MapPin size={24} />
        <span>Collectivités</span>
      </div>

      <nav className="sidebar-nav">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `sidebar-link ${isActive ? 'active' : ''}`
            }
          >
            <item.icon size={18} />
            <span>{item.label}</span>
          </NavLink>
        ))}
      </nav>

      <div className="sidebar-footer">
        <button className="sidebar-link" onClick={handleLogout}>
          <LogOut size={18} />
          <span>Déconnexion</span>
        </button>
      </div>
    </aside>
  )
}
