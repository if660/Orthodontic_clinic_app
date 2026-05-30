export interface Doctor {
  id: number
  firstName: string
  lastName: string
  specialization: string
}

export interface DoctorFormPayload {
  firstName: string
  lastName: string
  specialization: string
}

export type DoctorFlashType = 'added' | 'updated' | 'deleted'

export function emptyDoctorForm(): DoctorFormPayload {
  return {
    firstName: '',
    lastName: '',
    specialization: '',
  }
}

export function normalizeDoctor(raw: Partial<Doctor> & { id: number }): Doctor {
  return {
    id: raw.id,
    firstName: raw.firstName ?? '',
    lastName: raw.lastName ?? '',
    specialization: raw.specialization ?? '',
  }
}

export function toDoctorFormPayload(doctor: Doctor): DoctorFormPayload {
  return {
    firstName: doctor.firstName,
    lastName: doctor.lastName,
    specialization: doctor.specialization,
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