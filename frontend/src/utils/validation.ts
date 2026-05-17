import type { PatientFormPayload } from '../types/patient'

export interface FieldErrors {
  firstName?: string
  lastName?: string
  phone?: string
  email?: string
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

  return errors
}

export function hasFieldErrors(errors: FieldErrors): boolean {
  return Object.keys(errors).length > 0
}
