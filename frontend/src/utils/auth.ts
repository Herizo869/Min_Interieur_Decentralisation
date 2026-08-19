/**
 * Déconnexion sécurisée : supprime le token, l'utilisateur et
 * efface l'historique du navigateur pour empêcher le bouton retour.
 */
export function logout() {
  localStorage.removeItem('token')
  localStorage.removeItem('user')

  // Remplacer l'historique entier pour bloquer le bouton retour
  window.history.replaceState(null, '', '/login')
  window.location.href = '/login'
}

/**
 * Vérifie si le JWT stocké est encore valide (non expiré).
 */
export function isTokenValid(): boolean {
  const token = localStorage.getItem('token')
  if (!token) return false

  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    const now = Math.floor(Date.now() / 1000)
    return payload.exp && payload.exp > now
  } catch {
    return false
  }
}

/**
 * Récupère les informations utilisateur depuis le localStorage.
 */
export function getStoredUser() {
  const raw = localStorage.getItem('user')
  if (!raw) return null
  try {
    return JSON.parse(raw) as {
      id: string
      nom: string
      identifiant: string
      role: string
    }
  } catch {
    return null
  }
}
