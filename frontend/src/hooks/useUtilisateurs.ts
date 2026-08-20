import { useCallback } from 'react'
import api from '../services/api'

export interface Utilisateur {
  id: string
  nom: string
  identifiant: string
  role: string
  actif: boolean
  collectiviteIds: string[]
}

export interface CreerUtilisateurRequest {
  nom: string
  identifiant: string
  motDePasse: string
  role: string
  collectiviteIds: string[]
}

export interface ModifierUtilisateurRequest {
  nom: string
  role: string
  actif: boolean
  motDePasse?: string
  collectiviteIds: string[]
}

export function useUtilisateurs() {
  const lister = useCallback(async (): Promise<Utilisateur[]> => {
    const response = await api.get<Utilisateur[]>('/api/Utilisateurs')
    return response.data
  }, [])

  const obtenirParId = useCallback(async (id: string): Promise<Utilisateur> => {
    const response = await api.get<Utilisateur>(`/api/Utilisateurs/${id}`)
    return response.data
  }, [])

  const creer = useCallback(async (data: CreerUtilisateurRequest): Promise<Utilisateur> => {
    const response = await api.post<Utilisateur>('/api/Utilisateurs', data)
    return response.data
  }, [])

  const modifier = useCallback(async (id: string, data: ModifierUtilisateurRequest): Promise<Utilisateur> => {
    const response = await api.put<Utilisateur>(`/api/Utilisateurs/${id}`, data)
    return response.data
  }, [])

  const desactiver = useCallback(async (id: string): Promise<void> => {
    await api.delete(`/api/Utilisateurs/${id}`)
  }, [])

  return { lister, obtenirParId, creer, modifier, desactiver }
}
