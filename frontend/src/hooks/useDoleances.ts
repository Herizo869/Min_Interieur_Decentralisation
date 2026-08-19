import { useCallback } from 'react'
import api from '../services/api'

export interface Doleance {
  id: string
  description: string
  categorie: string
  statut: string
  auteur: string
  numeroSuivi: string
  dateCreation: string
  collectiviteRattacheeId: string
  collectiviteRattacheeNom: string
  geometrie: GeoJSON.Geometry
}

export interface DeposerDoleanceRequest {
  description: string
  categorie: string
  auteur: string
  point: GeoJSON.Point
}

const CATEGORIES = ['Voirie', 'Eclairage', 'Environnement', 'Assainissement', 'Autre'] as const
type CategorieDoleance = typeof CATEGORIES[number]

export { CATEGORIES }
export type { CategorieDoleance }

export function useDoleances() {
  /** Déposer une doléance (public, pas besoin de token). */
  const deposer = useCallback(async (data: DeposerDoleanceRequest): Promise<Doleance> => {
    const response = await api.post<Doleance>('/api/Doleances', data)
    return response.data
  }, [])

  /** Suivi par numéro de dossier (public). */
  const suivreParNumero = useCallback(async (numeroSuivi: string): Promise<Doleance> => {
    const response = await api.get<Doleance>(`/api/Doleances/suivi/${encodeURIComponent(numeroSuivi)}`)
    return response.data
  }, [])

  /** Lister les doléances (authentifié). */
  const lister = useCallback(async (collectiviteId?: string, statut?: string, categorie?: string): Promise<Doleance[]> => {
    const params = new URLSearchParams()
    if (collectiviteId) params.set('collectiviteId', collectiviteId)
    if (statut) params.set('statut', statut)
    if (categorie) params.set('categorie', categorie)
    const response = await api.get<Doleance[]>(`/api/Doleances?${params.toString()}`)
    return response.data
  }, [])

  /** Fiche d'une doléance (authentifié). */
  const obtenirParId = useCallback(async (id: string): Promise<Doleance> => {
    const response = await api.get<Doleance>(`/api/Doleances/${id}`)
    return response.data
  }, [])

  /** Changer le statut (authentifié). */
  const changerStatut = useCallback(async (id: string, statut: string): Promise<Doleance> => {
    const response = await api.put<Doleance>(`/api/Doleances/${id}/statut`, { statut })
    return response.data
  }, [])

  return { deposer, suivreParNumero, lister, obtenirParId, changerStatut }
}
