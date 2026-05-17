<template>
  <form class="patient-form" novalidate @submit.prevent="handleSubmit">
    <div class="form-grid">
      <div class="form-field" :class="{ 'has-error': fieldErrors.firstName }">
        <label for="firstName">
          Imię <span class="required" aria-hidden="true">*</span>
        </label>
        <input
          id="firstName"
          v-model="form.firstName"
          type="text"
          autocomplete="given-name"
          placeholder="np. Anna"
          :disabled="submitting"
          :aria-invalid="!!fieldErrors.firstName"
          :aria-describedby="fieldErrors.firstName ? 'firstName-error' : undefined"
          @blur="validateField('firstName')"
        />
        <p v-if="fieldErrors.firstName" id="firstName-error" class="field-error">
          {{ fieldErrors.firstName }}
        </p>
      </div>

      <div class="form-field" :class="{ 'has-error': fieldErrors.lastName }">
        <label for="lastName">
          Nazwisko <span class="required" aria-hidden="true">*</span>
        </label>
        <input
          id="lastName"
          v-model="form.lastName"
          type="text"
          autocomplete="family-name"
          placeholder="np. Kowalska"
          :disabled="submitting"
          :aria-invalid="!!fieldErrors.lastName"
          :aria-describedby="fieldErrors.lastName ? 'lastName-error' : undefined"
          @blur="validateField('lastName')"
        />
        <p v-if="fieldErrors.lastName" id="lastName-error" class="field-error">
          {{ fieldErrors.lastName }}
        </p>
      </div>

      <div class="form-field" :class="{ 'has-error': fieldErrors.phone }">
        <label for="phone">
          Telefon <span class="required" aria-hidden="true">*</span>
        </label>
        <input
          id="phone"
          v-model="form.phone"
          type="tel"
          autocomplete="tel"
          placeholder="np. 500 600 700"
          :disabled="submitting"
          :aria-invalid="!!fieldErrors.phone"
          :aria-describedby="fieldErrors.phone ? 'phone-error' : undefined"
          @blur="validateField('phone')"
        />
        <p v-if="fieldErrors.phone" id="phone-error" class="field-error">
          {{ fieldErrors.phone }}
        </p>
      </div>

      <div class="form-field" :class="{ 'has-error': fieldErrors.email }">
        <label for="email">E-mail</label>
        <input
          id="email"
          v-model="form.email"
          type="email"
          autocomplete="email"
          placeholder="np. anna@example.com"
          :disabled="submitting"
          :aria-invalid="!!fieldErrors.email"
          :aria-describedby="fieldErrors.email ? 'email-error' : undefined"
          @blur="validateField('email')"
        />
        <p v-if="fieldErrors.email" id="email-error" class="field-error">
          {{ fieldErrors.email }}
        </p>
      </div>

      <div class="form-field form-field--full">
        <label for="birthDate">Data urodzenia</label>
        <input
          id="birthDate"
          v-model="form.birthDate"
          type="date"
          :disabled="submitting"
        />
      </div>
    </div>

    <AppAlert v-if="error" :message="error" variant="error" :dismissible="false" />

    <div class="form-actions">
      <button type="button" class="app-btn app-btn--secondary" :disabled="submitting" @click="emit('cancel')">
        Anuluj
      </button>
      <button type="submit" class="app-btn app-btn--primary" :disabled="submitting">
        <AppSpinner v-if="submitting" label="Zapisywanie..." inline />
        <span v-else>{{ submitLabel }}</span>
      </button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import AppAlert from '../common/AppAlert.vue'
import AppSpinner from '../common/AppSpinner.vue'
import type { PatientFormPayload } from '../../types/patient'
import { emptyPatientForm } from '../../types/patient'
import {
  validatePatientForm,
  hasFieldErrors,
  type FieldErrors,
} from '../../utils/validation'

const props = withDefaults(
  defineProps<{
    modelValue?: PatientFormPayload
    submitLabel?: string
    submitting?: boolean
    error?: string | null
  }>(),
  {
    modelValue: () => emptyPatientForm(),
    submitLabel: 'Zapisz',
    submitting: false,
    error: null,
  },
)

const emit = defineEmits<{
  submit: [payload: PatientFormPayload]
  cancel: []
}>()

const form = reactive<PatientFormPayload>({ ...props.modelValue })
const fieldErrors = ref<FieldErrors>({})
const submitted = ref(false)

watch(
  () => props.modelValue,
  (value) => {
    Object.assign(form, value)
    if (!submitted.value) {
      fieldErrors.value = {}
    }
  },
  { deep: true },
)

const validateField = (field: keyof FieldErrors) => {
  const errors = validatePatientForm(form)
  fieldErrors.value = { ...fieldErrors.value, [field]: errors[field] }
  if (!errors[field]) {
    const next = { ...fieldErrors.value }
    delete next[field]
    fieldErrors.value = next
  }
}

const validateAll = (): boolean => {
  fieldErrors.value = validatePatientForm(form)
  return !hasFieldErrors(fieldErrors.value)
}

const handleSubmit = () => {
  submitted.value = true
  if (!validateAll()) return

  emit('submit', {
    firstName: form.firstName.trim(),
    lastName: form.lastName.trim(),
    phone: form.phone.trim(),
    email: form.email?.trim() || null,
    birthDate: form.birthDate || null,
  })
}
</script>

<style scoped>
.patient-form {
  margin-top: 0;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.5rem;
}

@media (min-width: 640px) {
  .form-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 1.5rem 1.25rem;
  }
}

.form-field--full {
  grid-column: 1 / -1;
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

label {
  font-size: 0.8125rem;
  font-weight: 600;
  color: #334155;
  letter-spacing: 0.02em;
}

.required {
  color: var(--color-danger);
}

input {
  width: 100%;
  padding: 0.75rem 0.875rem;
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-sm);
  font-size: 0.9375rem;
  font-family: inherit;
  color: var(--color-text);
  background: #fafbfc;
  transition:
    border-color 0.2s ease,
    box-shadow 0.2s ease,
    background-color 0.2s ease;
}

input:hover:not(:disabled) {
  border-color: #94a3b8;
  background: #fff;
}

input:focus {
  outline: none;
  border-color: var(--color-primary);
  background: #fff;
  box-shadow: 0 0 0 4px var(--color-primary-soft);
}

input:disabled {
  opacity: 0.65;
  cursor: not-allowed;
  background: #f1f5f9;
}

.has-error input {
  border-color: var(--color-danger);
}

.has-error input:focus {
  box-shadow: 0 0 0 4px rgba(220, 38, 38, 0.12);
}

.field-error {
  margin: 0;
  font-size: 0.8125rem;
  color: var(--color-danger);
}

.form-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-top: 2rem;
  padding-top: 1.5rem;
  border-top: 1px solid var(--color-border);
}

.form-actions .app-btn--primary {
  min-width: 9rem;
}
</style>
