import api from './api'

export interface LoginRequest {
  identifiant: string
  motDePasse: string
}

export interface LoginResponse {
  token: string
  expiration: string
  utilisateurId: string
  nom: string
  identifiant: string
  role: string
}

export interface ForgotPasswordRequest {
  identifiant: string
}

export interface ForgotPasswordResponse {
  message: string
  token?: string // renvoyé en dev uniquement
}

export interface ResetPasswordRequest {
  identifiant: string
  token: string
  nouveauMotDePasse: string
}

export async function login(data: LoginRequest): Promise<LoginResponse> {
  const response = await api.post<LoginResponse>('/api/auth/login', data)
  return response.data
}

export async function forgotPassword(data: ForgotPasswordRequest): Promise<ForgotPasswordResponse> {
  const response = await api.post<ForgotPasswordResponse>('/api/auth/forgot-password', data)
  return response.data
}

export async function resetPassword(data: ResetPasswordRequest): Promise<{ message: string }> {
  const response = await api.post<{ message: string }>('/api/auth/reset-password', data)
  return response.data
}
