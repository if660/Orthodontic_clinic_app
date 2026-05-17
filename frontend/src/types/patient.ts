export interface Patient {
  id: number
  firstName: string
  lastName: string
  phone: string
  email: string
  birthDate: string
}

export interface PatientFormPayload {
  firstName: string
  lastName: string
  phone: string
  email?: string | null
  birthDate?: string | null
}

export type PatientFlashType = 'added' | 'updated' | 'deleted'

export function emptyPatientForm(): PatientFormPayload {
  return {
    firstName: '',
    lastName: '',
    phone: '',
    email: '',
    birthDate: '',
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
  }
}

export function toFormPayload(patient: Patient): PatientFormPayload {
  return {
    firstName: patient.firstName,
    lastName: patient.lastName,
    phone: patient.phone,
    email: patient.email || '',
    birthDate: toDateInputValue(patient.birthDate),
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

