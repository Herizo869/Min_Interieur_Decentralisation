import { useCallback } from 'react'
import api from '../services/api'

export interface Indicateur {
  id: string
  type: string
  valeur: number
  unite: string
  source: string
  dateReleve: string
  collectiviteId: string
  collectiviteNom: string
}

export interface IndicateurRequest {
  type: string
  valeur: number
  unite: string
  source: string
  dateReleve: string
  collectiviteId: string
}

export function useIndicateurs() {
  const lister = useCallback(async (collectiviteId?: string, type?: string): Promise<Indicateur[]> => {
    const params = new URLSearchParams()
    if (collectiviteId) params.set('collectiviteId', collectiviteId)
    if (type) params.set('type', type)
    const response = await api.get<Indicateur[]>(`/api/Indicateurs?${params.toString()}`)
    return response.data
  }, [])

  const obtenirParId = useCallback(async (id: string): Promise<Indicateur> => {
    const response = await api.get<Indicateur>(`/api/Indicateurs/${id}`)
    return response.data
  }, [])

  const creer = useCallback(async (data: IndicateurRequest): Promise<Indicateur> => {
    const response = await api.post<Indicateur>('/api/Indicateurs', data)
    return response.data
  }, [])

  const modifier = useCallback(async (id: string, data: IndicateurRequest): Promise<Indicateur> => {
    const response = await api.put<Indicateur>(`/api/Indicateurs/${id}`, data)
    return response.data
  }, [])

  const supprimer = useCallback(async (id: string): Promise<void> => {
    await api.delete(`/api/Indicateurs/${id}`)
  }, [])

  return { lister, obtenirParId, creer, modifier, supprimer }
}
