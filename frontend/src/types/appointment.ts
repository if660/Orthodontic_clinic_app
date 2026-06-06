export interface Appointment {
  id: number
  patientId: number
  patientName?: string
  doctorId: number
  doctorName?: string
  appointmentDate: string
  status: string
  visitType: string
  notes?: string | null
}

export interface AppointmentFormPayload {
  patientId: number
  doctorId: number
  appointmentDate: string
  status: string
  visitType: string
  notes?: string | null
}

export type AppointmentFlashType = 'added' | 'updated' | 'deleted'

export type AppointmentStatus = string

export const APPOINTMENT_STATUS_OPTIONS: AppointmentStatus[] = [
  'Do potwierdzenia',
  'Zaplanowana',
  'Potwierdzona',
  'Zakonczona',
  'Anulowana',
  'Nieobecnosc',
]

export const APPOINTMENT_STATUSES = ['Do potwierdzenia', 'Zaplanowana', 'Potwierdzona', 'Zakończona', 'Anulowana', 'Nieobecność'] as const

export const VISIT_TYPES = [
  'Pierwsza konsultacja',
  'Kontrola aparatu',
  'Zakladanie aparatu',
  'Zdjecie aparatu',
  'Retainer',
  'Inne',
] as const

const fixMisencodedPolish = (value?: string | null): string => {
  if (!value) return ''
  return value
    .replaceAll('ZakĹ‚adanie aparatu', 'Zakladanie aparatu')
    .replaceAll('ZdjÄ™cie aparatu', 'Zdjecie aparatu')
    .replaceAll('ZakoĹ„czona', 'Zakonczona')
    .replaceAll('NieobecnoĹ›Ä‡', 'Nieobecnosc')
    .replaceAll('pomyĹ›lnie', 'pomyslnie')
    .replaceAll('zostaĹ‚a', 'zostala')
    .replaceAll('usuniÄ™ta', 'usunieta')
}

export function emptyAppointmentForm(): AppointmentFormPayload {
  return {
    patientId: 0,
    doctorId: 0,
    appointmentDate: '',
    status: 'Zaplanowana',
    visitType: '',
    notes: '',
  }
}

export function normalizeAppointment(raw: Partial<Appointment> & { id: number }): Appointment {
  return {
    id: raw.id,
    patientId: raw.patientId ?? 0,
    patientName: fixMisencodedPolish(raw.patientName),
    doctorId: raw.doctorId ?? 0,
    doctorName: fixMisencodedPolish(raw.doctorName),
    appointmentDate: raw.appointmentDate ?? '',
    status: fixMisencodedPolish(raw.status) || 'Zaplanowana',
    visitType: fixMisencodedPolish(raw.visitType),
    notes: raw.notes ? fixMisencodedPolish(raw.notes) : null,
  }
}

export function toAppointmentFormPayload(a: Appointment): AppointmentFormPayload {
  return {
    patientId: a.patientId,
    doctorId: a.doctorId,
    appointmentDate: toDateTimeInputValue(a.appointmentDate),
    status: a.status,
    visitType: a.visitType,
    notes: a.notes ?? '',
  }
}

export function toDateTimeInputValue(iso?: string | null): string {
  if (!iso) return ''
  return iso.slice(0, 16)
}

export function formatAppointmentDate(iso?: string | null): string {
  if (!iso) return '-'
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return '-'
  return date.toLocaleString('pl-PL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function toReadableDate(iso?: string | null): string {
  if (!iso) return '-'
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return '-'
  return date.toLocaleString('pl-PL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function getAppointmentFlashMessage(type: AppointmentFlashType): string {
  const messages: Record<AppointmentFlashType, string> = {
    added: 'Wizyta zostala dodana pomyslnie',
    updated: 'Dane wizyty zostaly zaktualizowane',
    deleted: 'Wizyta zostala usunieta pomyslnie',
  }
  return messages[type]
}
