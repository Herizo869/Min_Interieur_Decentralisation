import { createContext, useContext, useState, useCallback, useEffect, type ReactNode } from 'react'
import { isTokenValid, getStoredUser } from '../utils/auth'

export interface User {
  id: string
  nom: string
  identifiant: string
  role: string
}

interface AuthContextValue {
  user: User | null
  token: string | null
  isAuthenticated: boolean
  isAdmin: boolean
  isAgent: boolean
  login: (token: string, user: User) => void
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    if (isTokenValid()) return getStoredUser()
    return null
  })

  const [token, setToken] = useState<string | null>(() => {
    if (isTokenValid()) return localStorage.getItem('token')
    return null
  })

  // Vérifier périodiquement la validité du token
  useEffect(() => {
    const interval = setInterval(() => {
      if (!isTokenValid()) {
        setUser(null)
        setToken(null)
        localStorage.removeItem('token')
        localStorage.removeItem('user')
      }
    }, 30_000) // Vérifier toutes les 30 secondes

    return () => clearInterval(interval)
  }, [])

  const login = useCallback((newToken: string, newUser: User) => {
    localStorage.setItem('token', newToken)
    localStorage.setItem('user', JSON.stringify(newUser))
    setToken(newToken)
    setUser(newUser)
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    setToken(null)
    setUser(null)
    window.history.replaceState(null, '', '/login')
    window.location.href = '/login'
  }, [])

  const value: AuthContextValue = {
    user,
    token,
    isAuthenticated: !!token && isTokenValid(),
    isAdmin: user?.role === 'Administrateur',
    isAgent: user?.role === 'Agent',
    login,
    logout,
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth doit être utilisé dans un AuthProvider')
  return ctx
}
