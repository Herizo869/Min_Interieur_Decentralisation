import { useCallback, useEffect, useState } from 'react'
import api from '../services/api'

export interface CollectiviteStats {
  total: number
  parType: Record<string, number>
}

export interface ProjetStats {
  total: number
  parStatut: Record<string, number>
  montantTotal: number
  montantMoyen: number
}

export interface IndicateurStats {
  total: number
  parType: Record<string, number>
  collectivitesCouvertes: number
}

export interface LitigeStats {
  total: number
  parStatut: Record<string, number>
  ouverts: number
}

export interface DoleanceStats {
  total: number
  parStatut: Record<string, number>
  parCategorie: Record<string, number>
  enAttente: number
}

export interface UtilisateurStats {
  total: number
  parRole: Record<string, number>
  actifs: number
}

export interface TableauDeBord {
  collectivites: CollectiviteStats
  projets: ProjetStats
  indicateurs: IndicateurStats
  litiges: LitigeStats
  doléances: DoleanceStats
  utilisateurs: UtilisateurStats
}

export function useDashboard() {
  const [stats, setStats] = useState<TableauDeBord | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const charger = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const response = await api.get<TableauDeBord>('/api/Dashboard')
      setStats(response.data)
    } catch {
      setError('Erreur lors du chargement des statistiques.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { charger() }, [charger])

  return { stats, loading, error, charger }
}
