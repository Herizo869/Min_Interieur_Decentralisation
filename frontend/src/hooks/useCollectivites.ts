import { useCallback } from 'react'
import api from '../services/api'

export interface Collectivite {
  id: string
  nom: string
  codeAdministratif: string
  population: number | null
  type: string
}

export function useCollectivites() {
  const rechercher = useCallback(async (recherche?: string, type?: string): Promise<Collectivite[]> => {
    const params = new URLSearchParams()
    if (recherche) params.set('recherche', recherche)
    if (type) params.set('type', type)

    const response = await api.get<Collectivite[]>(`/api/collectivites?${params.toString()}`)
    return response.data
  }, [])

  const obtenirParId = useCallback(async (id: string) => {
    const response = await api.get(`/api/collectivites/${id}`)
    return response.data
  }, [])

  return { rechercher, obtenirParId }
}
