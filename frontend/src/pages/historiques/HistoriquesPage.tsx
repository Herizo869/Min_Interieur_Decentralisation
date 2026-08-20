import { useCallback, useEffect, useState } from 'react'
import { useHistoriques, HistoriqueEntry } from '../../hooks/useHistoriques'

const ENTITES = ['', 'Litige', 'Doleance']

const ENTITE_ICONS: Record<string, string> = {
  Litige: '⚠️',
  Doleance: '📢',
}

const ACTION_COLORS: Record<string, string> = {
  création: 'var(--success)',
  'changement de statut': 'var(--primary)',
  'création (détection automatique)': 'var(--warning)',
}

function getActionColor(action: string): string {
  for (const [key, color] of Object.entries(ACTION_COLORS)) {
    if (action.includes(key)) return color
  }
  return 'var(--text-light)'
}

function formatDate(iso: string): string {
  const d = new Date(iso)
  return d.toLocaleDateString('fr-FR', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

function formatTime(iso: string): string {
  const d = new Date(iso)
  return d.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
}

export default function HistoriquesPage() {
  const { lister } = useHistoriques()

  const [entries, setEntries] = useState<HistoriqueEntry[]>([])
  const [loading, setLoading] = useState(true)
  const [filtreEntite, setFiltreEntite] = useState('')
  const [dateDebut, setDateDebut] = useState('')
  const [dateFin, setDateFin] = useState('')

  const charger = useCallback(async () => {
    setLoading(true)
    try {
      const data = await lister({
        entite: filtreEntite || undefined,
        dateDebut: dateDebut || undefined,
        dateFin: dateFin || undefined,
        take: 200,
      })
      setEntries(data)
    } catch {
      // silently fail
    } finally {
      setLoading(false)
    }
  }, [lister, filtreEntite, dateDebut, dateFin])

  useEffect(() => { charger() }, [charger])

  // Grouper par jour
  const parJour = entries.reduce<Record<string, HistoriqueEntry[]>>((acc, e) => {
    const jour = new Date(e.date).toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })
    if (!acc[jour]) acc[jour] = []
    acc[jour].push(e)
    return acc
  }, {})

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Historique / Audit</h1>
          <p className="page-subtitle">Traçabilité des actions sur les litiges et doléances</p>
        </div>
      </div>

      {/* Filtres */}
      <div className="histo-filters">
        <div className="histo-filter-group">
          <label className="histo-filter-label">Entité</label>
          <select
            className="form-select"
            value={filtreEntite}
            onChange={(e) => setFiltreEntite(e.target.value)}
          >
            <option value="">Toutes</option>
            {ENTITES.filter(Boolean).map(e => (
              <option key={e} value={e}>{e}</option>
            ))}
          </select>
        </div>
        <div className="histo-filter-group">
          <label className="histo-filter-label">Date début</label>
          <input
            type="date"
            className="form-input"
            value={dateDebut}
            onChange={(e) => setDateDebut(e.target.value)}
          />
        </div>
        <div className="histo-filter-group">
          <label className="histo-filter-label">Date fin</label>
          <input
            type="date"
            className="form-input"
            value={dateFin}
            onChange={(e) => setDateFin(e.target.value)}
          />
        </div>
        <div className="histo-filter-count">
          {entries.length} événement{entries.length !== 1 ? 's' : ''}
        </div>
      </div>

      {/* Contenu */}
      {loading ? (
        <div className="loading-state">Chargement de l'historique…</div>
      ) : entries.length === 0 ? (
        <div className="empty-state">
          <div className="empty-icon">📋</div>
          <div>Aucun événement d'audit trouvé.</div>
        </div>
      ) : (
        <div className="histo-timeline">
          {Object.entries(parJour).map(([jour, jourEntries]) => (
            <div key={jour} className="histo-jour-group">
              <div className="histo-jour-header">
                <span className="histo-jour-dot" />
                <span className="histo-jour-label">{jour}</span>
                <span className="histo-jour-count">{jourEntries.length}</span>
              </div>
              <div className="histo-entries">
                {jourEntries.map((entry) => (
                  <div key={entry.id} className="histo-entry">
                    <div className="histo-entry-time">
                      {formatTime(entry.date)}
                    </div>
                    <div className="histo-entry-line">
                      <div
                        className="histo-entry-dot"
                        style={{ background: getActionColor(entry.action) }}
                      />
                    </div>
                    <div className="histo-entry-content">
                      <div className="histo-entry-main">
                        <span className="histo-entry-icon">
                          {ENTITE_ICONS[entry.entite] || '📝'}
                        </span>
                        <span className="histo-entry-action">{entry.action}</span>
                        <span className="histo-entry-entity">{entry.entite}</span>
                      </div>
                      <div className="histo-entry-meta">
                        par <span className="histo-entry-author">{entry.auteur}</span>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
