import { useCallback } from 'react'
import api from '../services/api'

export interface HistoriqueEntry {
  id: string
  entite: string
  entiteId: string
  action: string
  auteur: string
  date: string
}

export function useHistoriques() {
  const lister = useCallback(async (params?: {
    entite?: string
    entiteId?: string
    dateDebut?: string
    dateFin?: string
    skip?: number
    take?: number
  }): Promise<HistoriqueEntry[]> => {
    const search = new URLSearchParams()
    if (params?.entite) search.set('entite', params.entite)
    if (params?.entiteId) search.set('entiteId', params.entiteId)
    if (params?.dateDebut) search.set('dateDebut', params.dateDebut)
    if (params?.dateFin) search.set('dateFin', params.dateFin)
    if (params?.skip !== undefined) search.set('skip', params.skip.toString())
    if (params?.take !== undefined) search.set('take', params.take.toString())

    const response = await api.get<HistoriqueEntry[]>(`/api/Historiques?${search.toString()}`)
    return response.data
  }, [])

  return { lister }
}
