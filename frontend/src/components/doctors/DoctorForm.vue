<script setup lang="ts">
import { reactive, watch } from 'vue'
import type { DoctorFormPayload } from '../../types/doctor'

const props = defineProps<{
  modelValue: DoctorFormPayload
  submitting?: boolean
  submitLabel: string
}>()

const emit = defineEmits<{
  submit: [value: DoctorFormPayload]
  cancel: []
}>()

const form = reactive<DoctorFormPayload>({
  firstName: '',
  lastName: '',
  specialization: '',
})

const errors = reactive({
  firstName: '',
  lastName: '',
  specialization: '',
})

watch(
  () => props.modelValue,
  (value) => {
    form.firstName = value.firstName
    form.lastName = value.lastName
    form.specialization = value.specialization
  },
  { immediate: true, deep: true }
)

const validateForm = () => {
  errors.firstName = form.firstName.trim() ? '' : 'Imię jest wymagane.'
  errors.lastName = form.lastName.trim() ? '' : 'Nazwisko jest wymagane.'
  errors.specialization = form.specialization.trim() ? '' : 'Specjalizacja jest wymagana.'

  return !errors.firstName && !errors.lastName && !errors.specialization
}

const handleSubmit = () => {
  if (!validateForm()) return

  emit('submit', {
    firstName: form.firstName.trim(),
    lastName: form.lastName.trim(),
    specialization: form.specialization.trim(),
  })
}
</script>

<template>
  <form class="doctor-form" @submit.prevent="handleSubmit">
    <div class="form-grid">
      <label class="form-field">
        <span>Imię <span class="required">*</span></span>
        <input v-model="form.firstName" type="text" />
        <small v-if="errors.firstName">{{ errors.firstName }}</small>
      </label>

      <label class="form-field">
        <span>Nazwisko <span class="required">*</span></span>
        <input v-model="form.lastName" type="text" />
        <small v-if="errors.lastName">{{ errors.lastName }}</small>
      </label>

      <label class="form-field form-field--full">
        <span>Specjalizacja <span class="required">*</span></span>
        <input v-model="form.specialization" type="text" />
        <small v-if="errors.specialization">{{ errors.specialization }}</small>
      </label>
    </div>

    <div class="form-actions">
      <button class="app-btn app-btn--secondary" type="button" @click="emit('cancel')">
        Anuluj
      </button>

      <button class="app-btn app-btn--primary" type="submit" :disabled="submitting">
        {{ submitLabel }}
      </button>
    </div>
  </form>
</template>

<style scoped>
.doctor-form {
  display: grid;
  gap: 1.5rem;
}

.form-grid {
  display: grid;
  gap: 1.25rem;
}

@media (min-width: 720px) {
  .form-grid {
    grid-template-columns: 1fr 1fr;
  }

  .form-field--full {
    grid-column: 1 / -1;
  }
}

.form-field {
  display: grid;
  gap: 0.5rem;
  font-weight: 600;
  color: var(--color-text);
}

.form-field span {
  font-size: 0.9rem;
}

.required {
  color: var(--color-danger);
}

.form-field input {
  width: 100%;
  height: 3.25rem;
  padding: 0 1rem;
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-md);
  font-size: 1rem;
  font-family: inherit;
  color: var(--color-text);
  background: var(--color-surface);
}

.form-field input:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 4px var(--color-primary-soft);
}

.form-field small {
  color: var(--color-danger);
  font-size: 0.8rem;
}

.form-actions {
  display: flex;
  gap: 0.75rem;
  padding-top: 1.25rem;
  border-top: 1px solid var(--color-border);
}

</style>