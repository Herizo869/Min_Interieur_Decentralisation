import { useCallback, useRef, useState } from 'react'
import { useCollectivites, TYPES_COLLECTIVITE } from '../../hooks/useCollectivites'
import type { ImportReferentielResultat } from '../../hooks/useCollectivites'

export default function ImportReferentielPage() {
  const { importerReferentiel } = useCollectivites()

  const [fichier, setFichier] = useState<File | null>(null)
  const [typeCollectivite, setTypeCollectivite] = useState('')
  const [dragOver, setDragOver] = useState(false)
  const [loading, setLoading] = useState(false)
  const [resultat, setResultat] = useState<ImportReferentielResultat | null>(null)
  const [error, setError] = useState<string | null>(null)
  const inputRef = useRef<HTMLInputElement>(null)

  const handleFile = useCallback((f: File) => {
    const ext = f.name.split('.').pop()?.toLowerCase()
    if (ext !== 'geojson' && ext !== 'json') {
      setError('Format non supporté. Fichier GeoJSON attendu (.geojson ou .json).')
      return
    }
    setFichier(f)
    setError(null)
    setResultat(null)
  }, [])

  const onDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    setDragOver(false)
    const f = e.dataTransfer.files[0]
    if (f) handleFile(f)
  }, [handleFile])

  const onInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0]
    if (f) handleFile(f)
  }, [handleFile])

  const handleImport = async () => {
    if (!fichier) return
    setLoading(true)
    setError(null)
    setResultat(null)
    try {
      const res = await importerReferentiel(fichier, typeCollectivite || undefined)
      setResultat(res)
    } catch (err: unknown) {
      let message = "Erreur lors de l'import du fichier."
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { data?: ImportReferentielResultat | { message?: string } } }
        const data = axiosErr.response?.data
        if (data && 'detailsErreurs' in data) {
          setResultat(data as ImportReferentielResultat)
          message = `${(data as ImportReferentielResultat).erreurs} erreur(s) détectée(s) dans le fichier.`
        } else if (data && 'message' in data) {
          message = (data as { message: string }).message
        }
      }
      setError(message)
    } finally {
      setLoading(false)
    }
  }

  const handleReset = () => {
    setFichier(null)
    setTypeCollectivite('')
    setResultat(null)
    setError(null)
    if (inputRef.current) inputRef.current.value = ''
  }

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Import Référentiel</h1>
          <p className="page-subtitle">Importer des collectivités via un fichier GeoJSON</p>
        </div>
      </div>

      <div className="import-card">
        {/* Zone de dépôt */}
        <div
          className={`import-dropzone ${dragOver ? 'import-dropzone--active' : ''} ${fichier ? 'import-dropzone--has-file' : ''}`}
          onDragOver={(e) => { e.preventDefault(); setDragOver(true) }}
          onDragLeave={() => setDragOver(false)}
          onDrop={onDrop}
          onClick={() => inputRef.current?.click()}
        >
          <input
            ref={inputRef}
            type="file"
            accept=".geojson,.json"
            onChange={onInputChange}
            className="import-hidden-input"
          />
          {fichier ? (
            <div className="import-file-info">
              <div className="import-file-icon">📄</div>
              <div className="import-file-name">{fichier.name}</div>
              <div className="import-file-size">{(fichier.size / 1024 / 1024).toFixed(2)} Mo</div>
            </div>
          ) : (
            <div className="import-placeholder">
              <div className="import-placeholder-icon">📁</div>
              <div className="import-placeholder-text">
                Glissez votre fichier GeoJSON ici
              </div>
              <div className="import-placeholder-hint">
                ou cliquez pour sélectionner un fichier
              </div>
              <div className="import-placeholder-formats">
                Formats acceptés : .geojson, .json (max 100 Mo)
              </div>
            </div>
          )}
        </div>

        {/* Sélecteur de type */}
        <div className="import-type-selector">
          <label className="import-type-label">Type de collectivité importée</label>
          <select
            className="form-select"
            value={typeCollectivite}
            onChange={(e) => setTypeCollectivite(e.target.value)}
          >
            {TYPES_COLLECTIVITE.filter(t => t.value !== '').map(t => (
              <option key={t.value} value={t.value}>{t.label}</option>
            ))}
          </select>
          <span className="import-type-hint">
            Laissez vide pour détecter automatiquement depuis les propriétés GeoJSON.
          </span>
        </div>

        {/* Actions */}
        <div className="import-actions">
          <button
            className="btn btn-secondary"
            onClick={handleReset}
            disabled={loading}
          >
            Réinitialiser
          </button>
          <button
            className="btn btn-primary"
            onClick={handleImport}
            disabled={!fichier || loading}
          >
            {loading ? (
              <>
                <span className="btn-spinner" />
                Import en cours…
              </>
            ) : (
              'Importer le fichier'
            )}
          </button>
        </div>

        {/* Erreur */}
        {error && (
          <div className="import-error">
            <span className="import-error-icon">⚠️</span>
            {error}
          </div>
        )}

        {/* Rapport de résultat */}
        {resultat && (
          <div className="import-report">
            <h3 className="import-report-title">Rapport d'import</h3>

            <div className="import-report-stats">
              <div className="import-stat import-stat--success">
                <div className="import-stat-value">{resultat.importees}</div>
                <div className="import-stat-label">Créées</div>
              </div>
              <div className="import-stat import-stat--info">
                <div className="import-stat-value">{resultat.misesAJour}</div>
                <div className="import-stat-label">Mises à jour</div>
              </div>
              <div className={`import-stat ${resultat.erreurs > 0 ? 'import-stat--danger' : 'import-stat--success'}`}>
                <div className="import-stat-value">{resultat.erreurs}</div>
                <div className="import-stat-label">Erreurs</div>
              </div>
            </div>

            {resultat.detailsErreurs.length > 0 && (
              <div className="import-errors-list">
                <h4 className="import-errors-title">Détail des erreurs</h4>
                <div className="import-errors-scroll">
                  {resultat.detailsErreurs.map((err, i) => (
                    <div key={i} className="import-error-row">
                      <span className="import-error-ligne">Ligne {err.ligne}</span>
                      <span className="import-error-raison">{err.raison}</span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {resultat.erreurs === 0 && (
              <div className="import-success-message">
                ✅ Import terminé avec succès. {resultat.importees} collectivité(s) créée(s), {resultat.misesAJour} mise(s) à jour.
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
