import { computed, ref } from 'vue'
import axios from 'axios'

const API_URL = 'http://localhost:5153/api/Auth'
const STORAGE_KEY = 'orthodontic-auth-session'

export type AuthRole = 'Clinic' | 'Patient'

export interface AuthSession {
  token: string
  role: AuthRole
  email: string
  mustChangePassword: boolean
  patientId?: number | null
  patientName?: string | null
}

interface LoginResponse {
  token: string
  role: AuthRole
  email: string
  mustChangePassword?: boolean
  patientId?: number | null
  patientName?: string | null
}

export interface PatientAccountStatus {
  hasAccount: boolean
  email?: string | null
  mustChangePassword: boolean
}

export interface CreatedPatientAccount {
  email: string
  temporaryPassword: string
  mustChangePassword: boolean
}

export const authSession = ref<AuthSession | null>(readStoredSession())
export const isAuthenticated = computed(() => authSession.value !== null)
export const isClinic = computed(() => authSession.value?.role === 'Clinic')
export const isPatient = computed(() => authSession.value?.role === 'Patient')

export const initAuthInterceptor = () => {
  axios.interceptors.request.use((config) => {
    const token = authSession.value?.token
    if (token) {
      config.headers = config.headers ?? {}
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  })
}

export const login = async (email: string, password: string): Promise<AuthSession> => {
  const response = await axios.post<LoginResponse>(`${API_URL}/login`, { email, password })
  const session = normalizeSession(response.data)
  setSession(session)
  return session
}

export const loadCurrentUser = async (): Promise<AuthSession | null> => {
  if (!authSession.value?.token) return null
  const response = await axios.get<LoginResponse>(`${API_URL}/me`)
  const session = normalizeSession(response.data)
  setSession(session)
  return session
}

export const getPatientAccountStatus = async (patientId: number): Promise<PatientAccountStatus> => {
  const response = await axios.get<PatientAccountStatus>(`${API_URL}/patient/${patientId}/account`)
  return response.data
}

export const createPatientAccount = async (
  patientId: number,
  email: string,
): Promise<CreatedPatientAccount> => {
  const response = await axios.post<CreatedPatientAccount>(`${API_URL}/patient/${patientId}/account`, { email })
  return response.data
}

export const changePassword = async (
  currentPassword: string,
  newPassword: string,
  confirmPassword: string,
): Promise<AuthSession> => {
  const response = await axios.post<LoginResponse>(`${API_URL}/change-password`, {
    currentPassword,
    newPassword,
    confirmPassword,
  })
  const session = normalizeSession(response.data)
  setSession(session)
  return session
}

export const logout = () => {
  authSession.value = null
  localStorage.removeItem(STORAGE_KEY)
}

function setSession(session: AuthSession) {
  authSession.value = session
  localStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

function readStoredSession(): AuthSession | null {
  const stored = localStorage.getItem(STORAGE_KEY)
  if (!stored) return null

  try {
    return normalizeSession(JSON.parse(stored))
  } catch {
    localStorage.removeItem(STORAGE_KEY)
    return null
  }
}

function normalizeSession(raw: Partial<AuthSession> & { token?: string; role?: string; email?: string }): AuthSession {
  return {
    token: raw.token ?? '',
    role: raw.role === 'Patient' ? 'Patient' : 'Clinic',
    email: raw.email ?? '',
    mustChangePassword: raw.mustChangePassword ?? false,
    patientId: raw.patientId ?? null,
    patientName: raw.patientName ?? null,
  }
}
