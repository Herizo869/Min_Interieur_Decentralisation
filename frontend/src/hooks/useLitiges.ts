import { useCallback } from 'react'
import api from '../services/api'

export interface Litige {
  id: string
  description: string
  statut: string
  dateCreation: string
  collectiviteAId: string
  collectiviteANom: string
  collectiviteBId: string
  collectiviteBNom: string
  zoneConflit: GeoJSON.Geometry
  geometrie: GeoJSON.Geometry
}

export interface SignalerLitigeRequest {
  description: string
  geometrie: GeoJSON.Geometry
  collectiviteAId: string
  collectiviteBId: string
}

export interface DetectionResultat {
  detectes: number
  litiges: Litige[]
}

export function useLitiges() {
  const lister = useCallback(async (collectiviteId?: string, statut?: string): Promise<Litige[]> => {
    const params = new URLSearchParams()
    if (collectiviteId) params.set('collectiviteId', collectiviteId)
    if (statut) params.set('statut', statut)
    const response = await api.get<Litige[]>(`/api/Litiges?${params.toString()}`)
    return response.data
  }, [])

  const obtenirParId = useCallback(async (id: string): Promise<Litige> => {
    const response = await api.get<Litige>(`/api/Litiges/${id}`)
    return response.data
  }, [])

  const signaler = useCallback(async (data: SignalerLitigeRequest): Promise<Litige> => {
    const response = await api.post<Litige>('/api/Litiges', data)
    return response.data
  }, [])

  const changerStatut = useCallback(async (id: string, statut: string): Promise<Litige> => {
    const response = await api.put<Litige>(`/api/Litiges/${id}/statut`, { statut })
    return response.data
  }, [])

  const detecter = useCallback(async (): Promise<DetectionResultat> => {
    const response = await api.post<DetectionResultat>('/api/Litiges/detecter')
    return response.data
  }, [])

  return { lister, obtenirParId, signaler, changerStatut, detecter }
}
