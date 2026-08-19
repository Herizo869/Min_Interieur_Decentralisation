import { useCallback, useState } from 'react'
import api from '../services/api'

export interface Collectivite {
  id: string
  nom: string
  codeAdministratif: string
  population: number | null
  type: string
}

/** GeoJSON Feature avec propriétés collectivité. */
export interface CollectiviteFeature {
  type: 'Feature'
  geometry: GeoJSON.Geometry
  properties: {
    id: string
    nom: string
    codeAdministratif: string
    population: number
    type: string
  }
}

export interface CollectiviteFeatureCollection {
  type: 'FeatureCollection'
  features: CollectiviteFeature[]
}

/** Types de collectivité disponibles pour le filtre. */
export type TypeCollectivite = 'commune' | 'departement' | 'region' | 'epci'

export const TYPES_COLLECTIVITE: { value: TypeCollectivite | ''; label: string }[] = [
  { value: '', label: 'Toutes' },
  { value: 'commune', label: 'Communes' },
  { value: 'departement', label: 'Départements' },
  { value: 'region', label: 'Régions' },
  { value: 'epci', label: 'EPCI' },
]

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

/** Hook dédié à la carte : charge les données GeoJSON avec cache et filtre par type. */
export function useCollectivitesGeoJson() {
  const [geoData, setGeoData] = useState<CollectiviteFeatureCollection | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const chargerGeoJson = useCallback(async (type?: TypeCollectivite) => {
    setLoading(true)
    setError(null)
    try {
      const params = new URLSearchParams()
      if (type) params.set('type', type)
      const url = `/api/collectivites/geojson${params.toString() ? `?${params}` : ''}`
      const response = await api.get<CollectiviteFeatureCollection>(url)
      setGeoData(response.data)
    } catch (err: unknown) {
      let message = 'Erreur lors du chargement des données cartographiques'
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { data?: { message?: string } } }
        message = axiosErr.response?.data?.message || message
      }
      setError(message)
    } finally {
      setLoading(false)
    }
  }, [])

  return { geoData, loading, error, chargerGeoJson }
}
