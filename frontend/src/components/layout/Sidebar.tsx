import { NavLink } from 'react-router-dom'
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
  Upload,
  History,
  UserCircle,
} from 'lucide-react'
import { useAuth } from '../../contexts/AuthContext'

const navItems = [
  { to: '/dashboard', label: 'Tableau de bord', icon: LayoutDashboard },
  { to: '/collectivites', label: 'Collectivités', icon: Map },
  { to: '/collectivites/import', label: 'Import Référentiel', icon: Upload },
  { to: '/projets', label: 'Projets & Dotations', icon: FolderKanban },
  { to: '/indicateurs', label: 'Indicateurs', icon: BarChart3 },
  { to: '/litiges', label: 'Litiges', icon: AlertTriangle },
  { to: '/doleances', label: 'Doléances', icon: MessageSquareWarning },
  { to: '/historiques', label: 'Historique', icon: History },
  { to: '/exports', label: 'Exports', icon: Download },
  { to: '/utilisateurs', label: 'Utilisateurs', icon: Users },
  { to: '/profil', label: 'Mon Profil', icon: UserCircle },
]

export default function Sidebar() {
  const { logout } = useAuth()

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
        <button className="sidebar-link" onClick={logout}>
          <LogOut size={18} />
          <span>Déconnexion</span>
        </button>
      </div>
    </aside>
  )
}
