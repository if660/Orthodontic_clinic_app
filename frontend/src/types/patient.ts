export interface Patient {
  id: number
  firstName: string
  lastName: string
  phone: string
  email: string
  birthDate: string
  guardianFullName: string
  guardianPhone: string
  guardianEmail: string
}

export interface PatientFormPayload {
  firstName: string
  lastName: string
  phone: string
  email?: string | null
  birthDate?: string | null
  guardianFullName?: string | null
  guardianPhone?: string | null
  guardianEmail?: string | null
}

export type PatientFlashType = 'added' | 'updated' | 'deleted'

export function emptyPatientForm(): PatientFormPayload {
  return {
    firstName: '',
    lastName: '',
    phone: '',
    email: '',
    birthDate: '',
    guardianFullName: '',
    guardianPhone: '',
    guardianEmail: '',
  }
}

export function normalizePatient(raw: Partial<Patient> & { id: number }): Patient {
  return {
    id: raw.id,
    firstName: raw.firstName ?? '',
    lastName: raw.lastName ?? '',
    phone: raw.phone ?? '',
    email: raw.email ?? '',
    birthDate: raw.birthDate ?? '',
    guardianFullName: raw.guardianFullName ?? '',
    guardianPhone: raw.guardianPhone ?? '',
    guardianEmail: raw.guardianEmail ?? '',
  }
}

export function toFormPayload(patient: Patient): PatientFormPayload {
  return {
    firstName: patient.firstName,
    lastName: patient.lastName,
    phone: patient.phone,
    email: patient.email || '',
    birthDate: toDateInputValue(patient.birthDate),
    guardianFullName: patient.guardianFullName || '',
    guardianPhone: patient.guardianPhone || '',
    guardianEmail: patient.guardianEmail || '',
  }
}

export function toDateInputValue(isoDate?: string | null): string {
  if (!isoDate) return ''
  return isoDate.slice(0, 10)
}

export function formatBirthDate(isoDate?: string | null): string {
  if (!isoDate) return '—'
  const date = new Date(isoDate)
  if (Number.isNaN(date.getTime())) return '—'
  return date.toLocaleDateString('pl-PL')
}

export function getFlashMessage(type: PatientFlashType): string {
  const messages: Record<PatientFlashType, string> = {
    added: 'Pacjent został dodany pomyślnie',
    updated: 'Dane pacjenta zostały zaktualizowane',
    deleted: 'Pacjent został usunięty pomyślnie',
  }
  return messages[type]
}

export function getPatientFullName(patient: Pick<Patient, 'firstName' | 'lastName'>): string {
  return `${patient.firstName} ${patient.lastName}`.trim()
}

export function getPatientAge(birthDate?: string | null): number | null {
  if (!birthDate) return null

  const date = new Date(birthDate)
  if (Number.isNaN(date.getTime())) return null

  const today = new Date()
  let age = today.getFullYear() - date.getFullYear()
  const monthDifference = today.getMonth() - date.getMonth()

  if (monthDifference < 0 || (monthDifference === 0 && today.getDate() < date.getDate())) {
    age -= 1
  }

  return age
}

export function isPatientMinorFromBirthDate(birthDate?: string | null): boolean {
  const age = getPatientAge(birthDate)
  if (age === null) return false
  return age < 18
}

export function getPatientCategoryLabel(patient: Pick<Patient, 'birthDate'>): string {
  return isPatientMinorFromBirthDate(patient.birthDate) ? 'Dziecko' : 'Dorosły'
}

