import { useCallback } from 'react'
import api from '../services/api'

interface ModifierProfilPayload {
  nom: string
  motDePasse?: string
  confirmationMotDePasse?: string
}

interface ModifierProfilResponse {
  nom: string
  identifiant: string
  role: string
}

export function useProfil() {
  const modifier = useCallback(async (payload: ModifierProfilPayload): Promise<ModifierProfilResponse> => {
    const response = await api.put<ModifierProfilResponse>('/api/Auth/me', payload)
    return response.data
  }, [])

  return { modifier }
}
