import { useState, useEffect, useRef, useCallback } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { Bell, User, Search, X, MapPin, Sun, Moon } from 'lucide-react'
import { useAuth } from '../../contexts/AuthContext'
import { useTheme } from '../../contexts/ThemeContext'
import { useCollectivites, type Collectivite } from '../../hooks/useCollectivites'

const pageTitles: Record<string, string> = {
  '/dashboard': 'Tableau de bord',
  '/collectivites': 'Collectivités',
  '/projets': 'Projets & Dotations',
  '/indicateurs': 'Indicateurs',
  '/litiges': 'Litiges de limites',
  '/doleances': 'Doléances citoyennes',
  '/exports': 'Exports CSV',
  '/utilisateurs': 'Gestion des utilisateurs',
}

const TYPE_COLORS: Record<string, string> = {
  Commune: '#3b82f6',
  Departement: '#f59e0b',
  Region: '#10b981',
  Epci: '#8b5cf6',
}

export default function Header() {
  const location = useLocation()
  const navigate = useNavigate()
  const { user } = useAuth()
  const { isDark, toggleTheme } = useTheme()
  const { rechercher } = useCollectivites()
  const title = pageTitles[location.pathname] || 'Collectivités territoriales'

  const [query, setQuery] = useState('')
  const [results, setResults] = useState<Collectivite[]>([])
  const [open, setOpen] = useState(false)
  const [loading, setLoading] = useState(false)
  const wrapperRef = useRef<HTMLDivElement>(null)
  const debounceRef = useRef<ReturnType<typeof setTimeout>>(undefined)

  // Recherche débouncée (300ms)
  useEffect(() => {
    if (!query.trim()) {
      setResults([])
      setOpen(false)
      return
    }

    clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(async () => {
      setLoading(true)
      try {
        const data = await rechercher(query.trim())
        setResults(data)
        setOpen(true)
      } catch {
        setResults([])
      } finally {
        setLoading(false)
      }
    }, 300)

    return () => clearTimeout(debounceRef.current)
  }, [query, rechercher])

  // Fermer le dropdown au clic extérieur
  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (wrapperRef.current && !wrapperRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [])

  const handleSelect = useCallback((id: string) => {
    setQuery('')
    setOpen(false)
    navigate(`/collectivites/${id}`)
  }, [navigate])

  const clearSearch = useCallback(() => {
    setQuery('')
    setResults([])
    setOpen(false)
  }, [])

  return (
    <header className="app-header">
      <h1 className="header-title">{title}</h1>
      <div className="header-actions">
        {/* ── Barre de recherche ── */}
        <div className="header-search" ref={wrapperRef}>
          <Search size={16} className="header-search-icon" />
          <input
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onFocus={() => results.length > 0 && setOpen(true)}
            placeholder="Rechercher une collectivité…"
            className="header-search-input"
          />
          {query && (
            <button className="header-search-clear" onClick={clearSearch}>
              <X size={14} />
            </button>
          )}

          {/* ── Dropdown résultats ── */}
          {open && (
            <div className="header-search-dropdown">
              {loading && (
                <div className="header-search-loading">
                  <div className="header-search-spinner" />
                  Recherche…
                </div>
              )}

              {!loading && results.length === 0 && (
                <div className="header-search-empty">
                  Aucune collectivité trouvée
                </div>
              )}

              {!loading && results.map((r) => (
                <button
                  key={r.id}
                  className="header-search-result"
                  onClick={() => handleSelect(r.id)}
                >
                  <MapPin size={14} style={{ color: TYPE_COLORS[r.type] || '#94a3b8', flexShrink: 0 }} />
                  <div className="header-search-result-info">
                    <span className="header-search-result-name">{r.nom}</span>
                    <span className="header-search-result-meta">
                      {r.codeAdministratif}
                      {r.population != null && ` · ${r.population.toLocaleString('fr-FR')} hab.`}
                    </span>
                  </div>
                  <span
                    className="header-search-result-type"
                    style={{ background: (TYPE_COLORS[r.type] || '#94a3b8') + '18', color: TYPE_COLORS[r.type] || '#94a3b8' }}
                  >
                    {r.type}
                  </span>
                </button>
              ))}
            </div>
          )}
        </div>

        <button className="theme-toggle" onClick={toggleTheme} title={isDark ? 'Mode clair' : 'Mode sombre'}>
          {isDark ? <Sun size={20} /> : <Moon size={20} />}
        </button>
        <button className="header-icon-btn" title="Notifications">
          <Bell size={20} />
        </button>
        <div className="header-user">
          <User size={18} />
          <span>{user?.nom || 'Agent'}</span>
          {user?.role && (
            <span className="user-role">{user.role}</span>
          )}
        </div>
      </div>
    </header>
  )
}
