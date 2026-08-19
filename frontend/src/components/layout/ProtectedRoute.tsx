import { Navigate, Outlet } from 'react-router-dom'
import { isTokenValid } from '../../utils/auth'

export default function ProtectedRoute() {
  if (!isTokenValid()) {
    // Supprimer le token expiré/invalide
    localStorage.removeItem('token')
    localStorage.removeItem('user')

    return <Navigate to="/login" replace />
  }

  return <Outlet />
}
