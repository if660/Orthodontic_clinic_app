import type { Doctor } from '../types/doctor'

const DAY_NAMES = [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
] as const

const DAY_LABELS: Record<string, string> = {
  Monday: 'poniedziałek',
  Tuesday: 'wtorek',
  Wednesday: 'środę',
  Thursday: 'czwartek',
  Friday: 'piątek',
  Saturday: 'sobotę',
  Sunday: 'niedzielę',
}

export interface DoctorAvailabilityResult {
  available: boolean
  message: string
}

function timeToMinutes(time: string): number | null {
  const match = /^(\d{2}):(\d{2})$/.exec(time)
  if (!match) return null
  return Number(match[1]) * 60 + Number(match[2])
}

function getTimeFromDateTimeLocal(value: string): string | null {
  const timePart = value.split('T')[1]
  if (!timePart) return null
  return timePart.slice(0, 5)
}

function getDayNameFromDateTimeLocal(value: string): string | null {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return null
  return DAY_NAMES[date.getDay()] ?? null
}

function formatDaysList(days: string[]): string {
  return days.map((day) => DAY_LABELS[day] ?? day).join(', ')
}

export function getDoctorAvailability(
  doctor: Pick<Doctor, 'availableDays' | 'availabilityStart' | 'availabilityEnd'>,
  appointmentDate: string,
): DoctorAvailabilityResult {
  if (!appointmentDate) {
    return {
      available: true,
      message: 'Wybierz datę i godzinę wizyty, aby sprawdzić dostępność lekarza.',
    }
  }

  const dayName = getDayNameFromDateTimeLocal(appointmentDate)
  if (!dayName) {
    return { available: false, message: 'Nieprawidłowa data wizyty.' }
  }

  const availableDays = doctor.availableDays ?? []
  if (availableDays.length === 0) {
    return {
      available: false,
      message: 'Lekarz nie ma ustawionych dni dyspozycyjności.',
    }
  }

  if (!availableDays.includes(dayName)) {
    return {
      available: false,
      message: `Lekarz nie przyjmuje w ${DAY_LABELS[dayName] ?? dayName}. Dostępne dni: ${formatDaysList(availableDays)}.`,
    }
  }

  const time = getTimeFromDateTimeLocal(appointmentDate)
  if (!time) {
    return { available: false, message: 'Nieprawidłowa godzina wizyty.' }
  }

  const { availabilityStart, availabilityEnd } = doctor
  if (availabilityStart && availabilityEnd) {
    const appointmentMinutes = timeToMinutes(time)
    const startMinutes = timeToMinutes(availabilityStart)
    const endMinutes = timeToMinutes(availabilityEnd)

    if (appointmentMinutes === null || startMinutes === null || endMinutes === null) {
      return {
        available: false,
        message: 'Nie udało się odczytać godzin dyspozycyjności lekarza.',
      }
    }

    if (appointmentMinutes < startMinutes || appointmentMinutes >= endMinutes) {
      return {
        available: false,
        message: `Wybrana godzina (${time}) jest poza godzinami pracy lekarza (${availabilityStart}–${availabilityEnd}).`,
      }
    }
  }

  return {
    available: true,
    message: availabilityStart && availabilityEnd
      ? `Lekarz jest dostępny w ${DAY_LABELS[dayName] ?? dayName} w godzinach ${availabilityStart}–${availabilityEnd}.`
      : `Lekarz jest dostępny w ${DAY_LABELS[dayName] ?? dayName}.`,
  }
}

export function isDoctorAvailable(
  doctor: Pick<Doctor, 'availableDays' | 'availabilityStart' | 'availabilityEnd'>,
  appointmentDate: string,
): boolean {
  return getDoctorAvailability(doctor, appointmentDate).available
}

export function getDoctorOptionLabel(doctor: Doctor, appointmentDate: string): string {
  const base = `${doctor.firstName} ${doctor.lastName}`
  if (!appointmentDate) return base

  const status = getDoctorAvailability(doctor, appointmentDate)
  return `${base} — ${status.available ? 'dostępny' : 'niedostępny'}`
}
