import type { PatientFormPayload } from '../types/patient'

export interface FieldErrors {
  firstName?: string
  lastName?: string
  phone?: string
  email?: string
  guardianFullName?: string
  guardianPhone?: string
  guardianEmail?: string
}

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export function isValidEmail(email: string): boolean {
  return EMAIL_REGEX.test(email.trim())
}

export function validatePatientForm(form: PatientFormPayload): FieldErrors {
  const errors: FieldErrors = {}

  if (!form.firstName.trim()) {
    errors.firstName = 'Imię jest wymagane'
  }

  if (!form.lastName.trim()) {
    errors.lastName = 'Nazwisko jest wymagane'
  }

  if (!form.phone.trim()) {
    errors.phone = 'Numer telefonu jest wymagany'
  }

  const email = form.email?.trim() ?? ''
  if (email && !isValidEmail(email)) {
    errors.email = 'Nieprawidłowy format adresu e-mail'
  }

  const isMinor = isPatientMinor(form.birthDate)

  if (isMinor && !(form.guardianFullName?.trim() ?? '')) {
    errors.guardianFullName = 'Dla osoby niepełnoletniej wymagany jest opiekun prawny'
  }

  if (isMinor && !(form.guardianPhone?.trim() ?? '')) {
    errors.guardianPhone = 'Numer telefonu opiekuna jest wymagany'
  }

  const guardianEmail = form.guardianEmail?.trim() ?? ''
  if (guardianEmail && !isValidEmail(guardianEmail)) {
    errors.guardianEmail = 'Nieprawidłowy format e-mail opiekuna'
  }

  return errors
}

function isPatientMinor(birthDate?: string | null): boolean {
  if (!birthDate) return false

  const date = new Date(birthDate)
  if (Number.isNaN(date.getTime())) return false

  const today = new Date()
  let age = today.getFullYear() - date.getFullYear()
  const monthDifference = today.getMonth() - date.getMonth()

  if (monthDifference < 0 || (monthDifference === 0 && today.getDate() < date.getDate())) {
    age -= 1
  }

  return age < 18
}

export function hasFieldErrors(errors: FieldErrors): boolean {
  return Object.keys(errors).length > 0
}
