<template>
  <form class="patient-form" @submit.prevent="handleSubmit">
    <div class="form-grid">
      <div class="form-field">
        <label for="firstName">First Name</label>
        <input
          id="firstName"
          v-model="form.firstName"
          type="text"
          required
          autocomplete="given-name"
        />
      </div>

      <div class="form-field">
        <label for="lastName">Last Name</label>
        <input
          id="lastName"
          v-model="form.lastName"
          type="text"
          required
          autocomplete="family-name"
        />
      </div>

      <div class="form-field">
        <label for="phone">Phone</label>
        <input
          id="phone"
          v-model="form.phone"
          type="tel"
          required
          autocomplete="tel"
        />
      </div>

      <div class="form-field">
        <label for="email">Email</label>
        <input
          id="email"
          v-model="form.email"
          type="email"
          autocomplete="email"
        />
      </div>

      <div class="form-field">
        <label for="birthDate">Birth Date</label>
        <input id="birthDate" v-model="form.birthDate" type="date" />
      </div>
    </div>

    <p v-if="error" class="form-error">{{ error }}</p>

    <div class="form-actions">
      <button type="button" class="btn-cancel" :disabled="submitting" @click="emit('cancel')">
        Cancel
      </button>
      <button type="submit" class="btn-submit" :disabled="submitting">
        {{ submitting ? 'Saving…' : submitLabel }}
      </button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { reactive, watch } from 'vue'
import type { PatientFormPayload } from '../../types/patient'
import { emptyPatientForm } from '../../types/patient'

const props = withDefaults(
  defineProps<{
    modelValue?: PatientFormPayload
    submitLabel?: string
    submitting?: boolean
    error?: string | null
  }>(),
  {
    modelValue: () => emptyPatientForm(),
    submitLabel: 'Save',
    submitting: false,
    error: null,
  },
)

const emit = defineEmits<{
  submit: [payload: PatientFormPayload]
  cancel: []
}>()

const form = reactive<PatientFormPayload>({ ...props.modelValue })

watch(
  () => props.modelValue,
  (value) => {
    Object.assign(form, value)
  },
  { deep: true },
)

const handleSubmit = () => {
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
  margin-top: 1.5rem;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 1.25rem;
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

label {
  font-size: 0.8125rem;
  font-weight: 600;
  color: #374151;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

input {
  padding: 0.625rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 0.9375rem;
  color: #111827;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

input:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.15);
}

.form-error {
  margin: 1rem 0 0;
  color: #dc2626;
  font-size: 0.875rem;
}

.form-actions {
  display: flex;
  gap: 0.75rem;
  margin-top: 1.5rem;
  padding-top: 1.5rem;
  border-top: 1px solid #e5e7eb;
}

button {
  padding: 0.625rem 1.25rem;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  border: none;
  transition: background-color 0.15s ease, opacity 0.15s ease;
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-cancel {
  background-color: #f3f4f6;
  color: #374151;
}

.btn-cancel:hover:not(:disabled) {
  background-color: #e5e7eb;
}

.btn-submit {
  background-color: #2563eb;
  color: #ffffff;
}

.btn-submit:hover:not(:disabled) {
  background-color: #1d4ed8;
}
</style>
