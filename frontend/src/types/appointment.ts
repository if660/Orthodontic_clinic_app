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

export type AppointmentStatus = 'Zaplanowana' | 'Potwierdzona' | 'Zakończona' | 'Anulowana' | 'Nieobecność'

export const APPOINTMENT_STATUS_OPTIONS: AppointmentStatus[] = [
  'Zaplanowana',
  'Potwierdzona',
  'Zakończona',
  'Anulowana',
  'Nieobecność',
]

export const APPOINTMENT_STATUSES = ['Zaplanowana', 'Potwierdzona', 'Zakończona', 'Anulowana', 'Nieobecność'] as const

export const VISIT_TYPES = [
  'Pierwsza konsultacja',
  'Kontrola aparatu',
  'Zakładanie aparatu',
  'Zdjęcie aparatu',
  'Retainer',
  'Inne',
] as const

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
    patientName: raw.patientName ?? '',
    doctorId: raw.doctorId ?? 0,
    doctorName: raw.doctorName ?? '',
    appointmentDate: raw.appointmentDate ?? '',
    status: raw.status ?? 'Zaplanowana',
    visitType: raw.visitType ?? '',
    notes: raw.notes ?? null,
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
  if (!iso) return '—'
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return '—'
  return date.toLocaleString('pl-PL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function toReadableDate(iso?: string | null): string {
  if (!iso) return '—'
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return '—'
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
    added: 'Wizyta została dodana pomyślnie',
    updated: 'Dane wizyty zostały zaktualizowane',
    deleted: 'Wizyta została usunięta pomyślnie',
  }
  return messages[type]
}
