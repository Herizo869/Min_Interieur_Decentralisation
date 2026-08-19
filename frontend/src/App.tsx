import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './contexts/AuthContext'
import Layout from './components/layout/Layout'
import ProtectedRoute from './components/layout/ProtectedRoute'
import LoginPage from './pages/auth/LoginPage'
import ForgotPasswordPage from './pages/auth/ForgotPasswordPage'
import ResetPasswordPage from './pages/auth/ResetPasswordPage'
import DashboardPage from './pages/dashboard/DashboardPage'
import CollectivitesPage from './pages/collectivites/CollectivitesPage'
import ProjetsPage from './pages/projets/ProjetsPage'
import IndicateursPage from './pages/indicateurs/IndicateursPage'
import LitigesPage from './pages/litiges/LitigesPage'
import DoleancesPage from './pages/doleances/DoleancesPage'
import ExportsPage from './pages/exports/ExportsPage'
import UtilisateursPage from './pages/utilisateurs/UtilisateursPage'

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Routes publiques */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />

          {/* Routes protégées */}
          <Route element={<ProtectedRoute />}>
            <Route element={<Layout />}>
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/collectivites" element={<CollectivitesPage />} />
              <Route path="/projets" element={<ProjetsPage />} />
              <Route path="/indicateurs" element={<IndicateursPage />} />
              <Route path="/litiges" element={<LitigesPage />} />
              <Route path="/doleances" element={<DoleancesPage />} />
              <Route path="/exports" element={<ExportsPage />} />
              <Route path="/utilisateurs" element={<UtilisateursPage />} />
            </Route>
          </Route>

          {/* Redirection par défaut */}
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
