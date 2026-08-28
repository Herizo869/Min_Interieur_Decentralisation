import { useDashboard } from '../../hooks/useDashboard'
import {
  PieChart, Pie, Cell, BarChart, Bar,
  XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts'
import {
  Building2, FolderKanban, CircleDollarSign,
  AlertTriangle, MessageSquareWarning, Users,
  BarChart3, UserCheck,
} from 'lucide-react'
import type { LucideIcon } from 'lucide-react'

const COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4', '#f97316', '#ec4899']

const STATUT_COLORS: Record<string, string> = {
  EnPreparation: '#f59e0b',
  EnCours: '#3b82f6',
  Termine: '#10b981',
  Signale: '#f59e0b',
  EnInstruction: '#3b82f6',
  Arbitre: '#8b5cf6',
  Clos: '#10b981',
  Nouveau: '#f59e0b',
  EnCoursTraitement: '#3b82f6',
  Resolu: '#10b981',
}

function formatMontant(v: number) {
  if (v >= 1_000_000) return `${(v / 1_000_000).toFixed(1)} M`
  if (v >= 1_000) return `${(v / 1_000).toFixed(0)} k`
  return v.toFixed(0)
}

function toPieData(record: Record<string, number>) {
  return Object.entries(record).map(([name, value]) => ({ name, value }))
}

function toBarData(record: Record<string, number>) {
  return Object.entries(record).map(([name, value]) => ({ name, value }))
}

function CustomTooltip({ active, payload }: { active?: boolean; payload?: Array<{ name: string; value: number }> }) {
  if (!active || !payload?.length) return null
  return (
    <div className="dash-tooltip">
      <div className="dash-tooltip-name">{payload[0].name}</div>
      <div className="dash-tooltip-value">{payload[0].value}</div>
    </div>
  )
}

interface KpiItem {
  label: string
  value: number | string
  icon: LucideIcon
  color: string
}

export default function DashboardPage() {
  const { stats, loading, error } = useDashboard()

  if (loading) return <div className="page-container"><div className="loading-state">Chargement du tableau de bord...</div></div>
  if (error || !stats) return <div className="page-container"><div className="error-state">{error || 'Aucune donnee disponible.'}</div></div>

  const { collectivites, projets, indicateurs, litiges, doléances, utilisateurs } = stats

  const kpis: KpiItem[] = [
    { label: 'Collectivites', value: collectivites.total, icon: Building2, color: '#3b82f6' },
    { label: 'Projets', value: projets.total, icon: FolderKanban, color: '#10b981' },
    { label: 'Montant total', value: `${formatMontant(projets.montantTotal)}`, icon: CircleDollarSign, color: '#f59e0b' },
    { label: 'Litiges ouverts', value: litiges.ouverts, icon: AlertTriangle, color: '#ef4444' },
    { label: 'Doleances en attente', value: doléances.enAttente, icon: MessageSquareWarning, color: '#f97316' },
    { label: 'Utilisateurs actifs', value: utilisateurs.actifs, icon: Users, color: '#8b5cf6' },
  ]

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Tableau de bord</h1>
          <p className="page-subtitle">Vue d'ensemble de tous les modules</p>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="dash-kpi-grid">
        {kpis.map((kpi) => (
          <div key={kpi.label} className="dash-kpi-card" style={{ borderTopColor: kpi.color }}>
            <div className="dash-kpi-icon" style={{ color: kpi.color }}>
              <kpi.icon size={24} />
            </div>
            <div className="dash-kpi-content">
              <div className="dash-kpi-value" style={{ color: kpi.color }}>
                {kpi.value}
              </div>
              <div className="dash-kpi-label">{kpi.label}</div>
            </div>
          </div>
        ))}
      </div>

      {/* Charts Row 1 */}
      <div className="dash-charts-row">
        <div className="dash-chart-card">
          <h3 className="dash-chart-title">Collectivites par type</h3>
          <ResponsiveContainer width="100%" height={260}>
            <PieChart>
              <Pie data={toPieData(collectivites.parType)} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={90} label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}>
                {toPieData(collectivites.parType).map((_, i) => (
                  <Cell key={i} fill={COLORS[i % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip content={<CustomTooltip />} />
            </PieChart>
          </ResponsiveContainer>
        </div>

        <div className="dash-chart-card">
          <h3 className="dash-chart-title">Projets par statut</h3>
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={toBarData(projets.parStatut)}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
              <XAxis dataKey="name" tick={{ fontSize: 12 }} />
              <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
              <Tooltip content={<CustomTooltip />} />
              <Bar dataKey="value" radius={[6, 6, 0, 0]}>
                {toBarData(projets.parStatut).map((entry) => (
                  <Cell key={entry.name} fill={STATUT_COLORS[entry.name] || '#6b7280'} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="dash-chart-card">
          <h3 className="dash-chart-title">Litiges par statut</h3>
          <ResponsiveContainer width="100%" height={260}>
            <PieChart>
              <Pie data={toPieData(litiges.parStatut)} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={90} label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}>
                {toPieData(litiges.parStatut).map((_, i) => (
                  <Cell key={i} fill={Object.values(STATUT_COLORS)[i % Object.values(STATUT_COLORS).length]} />
                ))}
              </Pie>
              <Tooltip content={<CustomTooltip />} />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Charts Row 2 */}
      <div className="dash-charts-row">
        <div className="dash-chart-card">
          <h3 className="dash-chart-title">Doleances par categorie</h3>
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={toBarData(doléances.parCategorie)}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
              <XAxis dataKey="name" tick={{ fontSize: 12 }} />
              <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
              <Tooltip content={<CustomTooltip />} />
              <Bar dataKey="value" radius={[6, 6, 0, 0]}>
                {toBarData(doléances.parCategorie).map((_, i) => (
                  <Cell key={i} fill={COLORS[i % COLORS.length]} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="dash-chart-card">
          <h3 className="dash-chart-title">Doleances par statut</h3>
          <ResponsiveContainer width="100%" height={260}>
            <PieChart>
              <Pie data={toPieData(doléances.parStatut)} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={90} label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}>
                {toPieData(doléances.parStatut).map((_, i) => (
                  <Cell key={i} fill={Object.values(STATUT_COLORS)[i % Object.values(STATUT_COLORS).length]} />
                ))}
              </Pie>
              <Tooltip content={<CustomTooltip />} />
            </PieChart>
          </ResponsiveContainer>
        </div>

        <div className="dash-chart-card">
          <h3 className="dash-chart-title">Utilisateurs par role</h3>
          <ResponsiveContainer width="100%" height={260}>
            <PieChart>
              <Pie data={toPieData(utilisateurs.parRole)} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={90} label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}>
                {toPieData(utilisateurs.parRole).map((_, i) => (
                  <Cell key={i} fill={COLORS[i % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip content={<CustomTooltip />} />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Summary Row */}
      <div className="dash-summary-row">
        <div className="dash-summary-card">
          <div className="dash-summary-icon"><BarChart3 size={22} color="#3b82f6" /></div>
          <div>
            <div className="dash-summary-value">{indicateurs.total}</div>
            <div className="dash-summary-label">Indicateurs ({indicateurs.collectivitesCouvertes} collectivites couvertes)</div>
          </div>
        </div>
        <div className="dash-summary-card">
          <div className="dash-summary-icon"><UserCheck size={22} color="#8b5cf6" /></div>
          <div>
            <div className="dash-summary-value">{utilisateurs.total}</div>
            <div className="dash-summary-label">Utilisateurs ({utilisateurs.actifs} actifs)</div>
          </div>
        </div>
        <div className="dash-summary-card">
          <div className="dash-summary-icon"><CircleDollarSign size={22} color="#f59e0b" /></div>
          <div>
            <div className="dash-summary-value">{formatMontant(projets.montantMoyen)}</div>
            <div className="dash-summary-label">Montant moyen par projet</div>
          </div>
        </div>
      </div>
    </div>
  )
}
