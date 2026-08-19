import { useState, useCallback } from 'react'
import { useAuth } from '../contexts/AuthContext'

interface UseApiOptions<T> {
  onSuccess?: (data: T) => void
  onError?: (error: string) => void
}

interface UseApiResult<T> {
  data: T | null
  loading: boolean
  error: string | null
  execute: (fn: () => Promise<T>) => Promise<T | null>
}

export function useApi<T = unknown>(options?: UseApiOptions<T>): UseApiResult<T> {
  const [data, setData] = useState<T | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const { logout } = useAuth()

  const execute = useCallback(async (fn: () => Promise<T>): Promise<T | null> => {
    setLoading(true)
    setError(null)
    try {
      const result = await fn()
      setData(result)
      options?.onSuccess?.(result)
      return result
    } catch (err: unknown) {
      let message = 'Une erreur est survenue'

      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { status?: number; data?: { message?: string } } }
        message = axiosErr.response?.data?.message || message

        // Token expiré ou invalide → déconnexion
        if (axiosErr.response?.status === 401) {
          logout()
          return null
        }
      }

      setError(message)
      options?.onError?.(message)
      return null
    } finally {
      setLoading(false)
    }
  }, [logout, options])

  return { data, loading, error, execute }
}

// Hook simple pour récupérer les infos de l'utilisateur connecté via l'API
export function useCurrentUser() {
  return useApi<{ utilisateurId: string; identifiant: string; nom: string; role: string }>()
}
