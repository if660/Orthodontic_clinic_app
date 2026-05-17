export interface Patient {
  id: number
  firstName: string
  lastName: string
  phone: string
  email?: string | null
  birthDate?: string | null
}

export interface PatientFormPayload {
  firstName: string
  lastName: string
  phone: string
  email?: string | null
  birthDate?: string | null
}

export function emptyPatientForm(): PatientFormPayload {
  return {
    firstName: '',
    lastName: '',
    phone: '',
    email: '',
    birthDate: '',
  }
}

export function toFormPayload(patient: Patient): PatientFormPayload {
  return {
    firstName: patient.firstName,
    lastName: patient.lastName,
    phone: patient.phone,
    email: patient.email ?? '',
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
  return date.toLocaleDateString()
}
