import { useCallback } from 'react'
import api from '../services/api'

export interface ProjetDotation {
  id: string
  intitule: string
  montant: number
  devise: string
  statut: string
  dateDebut: string
  dateFin: string | null
  collectiviteId: string
  collectiviteNom: string
}

export interface ProjetDotationRequest {
  intitule: string
  montant: number
  devise: string
  statut: string
  dateDebut: string
  dateFin?: string | null
  collectiviteId: string
}

export function useProjets() {
  const lister = useCallback(async (collectiviteId?: string): Promise<ProjetDotation[]> => {
    const params = new URLSearchParams()
    if (collectiviteId) params.set('collectiviteId', collectiviteId)
    const response = await api.get<ProjetDotation[]>(`/api/ProjetsDotations?${params.toString()}`)
    return response.data
  }, [])

  const obtenirParId = useCallback(async (id: string): Promise<ProjetDotation> => {
    const response = await api.get<ProjetDotation>(`/api/ProjetsDotations/${id}`)
    return response.data
  }, [])

  const creer = useCallback(async (data: ProjetDotationRequest): Promise<ProjetDotation> => {
    const response = await api.post<ProjetDotation>('/api/ProjetsDotations', data)
    return response.data
  }, [])

  const modifier = useCallback(async (id: string, data: ProjetDotationRequest): Promise<ProjetDotation> => {
    const response = await api.put<ProjetDotation>(`/api/ProjetsDotations/${id}`, data)
    return response.data
  }, [])

  const supprimer = useCallback(async (id: string): Promise<void> => {
    await api.delete(`/api/ProjetsDotations/${id}`)
  }, [])

  return { lister, obtenirParId, creer, modifier, supprimer }
}
