import { useCallback } from 'react'
import api from '../services/api'

export type ExportResource = 'doleances' | 'litiges' | 'projets' | 'indicateurs'

export function useExports() {
  const telecharger = useCallback(async (
    resource: ExportResource,
    collectiviteId?: string,
    options?: { statutDoleance?: string; statutLitige?: string }
  ): Promise<void> => {
    const params = new URLSearchParams()
    params.set('resource', resource)
    if (collectiviteId) params.set('collectiviteId', collectiviteId)
    if (options?.statutDoleance) params.set('statutDoleance', options.statutDoleance)
    if (options?.statutLitige) params.set('statutLitige', options.statutLitige)

    const response = await api.get(`/api/Exports?${params.toString()}`, {
      responseType: 'blob',
    })

    // Extraire le nom du fichier depuis le Content-Disposition
    const disposition = response.headers['content-disposition']
    let filename = `${resource}_${new Date().toISOString().split('T')[0]}.csv`
    if (disposition) {
      const match = disposition.match(/filename="?([^"]+)"?/)
      if (match) filename = match[1]
    }

    // Télécharger le fichier
    const url = window.URL.createObjectURL(new Blob([response.data], { type: 'text/csv;charset=utf-8' }))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', filename)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
  }, [])

  return { telecharger }
}
