export interface Doctor {
  id: number
  firstName: string
  lastName: string
  specialization: string
  licenseNumber: string
  availableDays: string[]
  availabilityStart: string
  availabilityEnd: string
  profileNote: string
}

export interface DoctorFormPayload {
  firstName: string
  lastName: string
  specialization: string
  licenseNumber: string
  availableDays: string[]
  availabilityStart?: string | null
  availabilityEnd?: string | null
  profileNote?: string | null
}

export type DoctorFlashType = 'added' | 'updated' | 'deleted'

export function emptyDoctorForm(): DoctorFormPayload {
  return {
    firstName: '',
    lastName: '',
    specialization: '',
    licenseNumber: '',
    availableDays: [],
    availabilityStart: '',
    availabilityEnd: '',
    profileNote: '',
  }
}

function toDaysArray(value?: string | string[] | null): string[] {
  if (!value) return []
  if (Array.isArray(value)) return value.filter(Boolean)
  return value.split(',').map((item) => item.trim()).filter(Boolean)
}

export function normalizeDoctor(raw: Partial<Doctor> & { id: number }): Doctor {
  return {
    id: raw.id,
    firstName: raw.firstName ?? '',
    lastName: raw.lastName ?? '',
    specialization: raw.specialization ?? '',
    licenseNumber: raw.licenseNumber ?? '',
    availableDays: toDaysArray(raw.availableDays),
    availabilityStart: raw.availabilityStart ?? '',
    availabilityEnd: raw.availabilityEnd ?? '',
    profileNote: raw.profileNote ?? '',
  }
}

export function toDoctorFormPayload(doctor: Doctor): DoctorFormPayload {
  return {
    firstName: doctor.firstName,
    lastName: doctor.lastName,
    specialization: doctor.specialization,
    licenseNumber: doctor.licenseNumber,
    availableDays: [...doctor.availableDays],
    availabilityStart: doctor.availabilityStart,
    availabilityEnd: doctor.availabilityEnd,
    profileNote: doctor.profileNote,
  }
}

export function getDoctorFullName(doctor: Pick<Doctor, 'firstName' | 'lastName'>): string {
  return `${doctor.firstName} ${doctor.lastName}`.trim()
}

export function getDoctorFlashMessage(type: DoctorFlashType): string {
  const messages: Record<DoctorFlashType, string> = {
    added: 'Lekarz został dodany pomyślnie',
    updated: 'Dane lekarza zostały zaktualizowane',
    deleted: 'Lekarz został usunięty pomyślnie',
  }

  return messages[type]
}