<script setup lang="ts">
import { reactive, watch } from 'vue'
import type { DoctorFormPayload } from '../../types/doctor'

const dayOptions = [
  { label: 'Poniedziałek', value: 'Monday' },
  { label: 'Wtorek', value: 'Tuesday' },
  { label: 'Środa', value: 'Wednesday' },
  { label: 'Czwartek', value: 'Thursday' },
  { label: 'Piątek', value: 'Friday' },
  { label: 'Sobota', value: 'Saturday' },
]

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
  licenseNumber: '',
  availableDays: [],
  availabilityStart: '',
  availabilityEnd: '',
  profileNote: '',
})

const errors = reactive({
  firstName: '',
  lastName: '',
  specialization: '',
  licenseNumber: '',
  availableDays: '',
  availabilityWindow: '',
})

watch(
  () => props.modelValue,
  (value) => {
    form.firstName = value.firstName
    form.lastName = value.lastName
    form.specialization = value.specialization
    form.licenseNumber = value.licenseNumber
    form.availableDays = [...value.availableDays]
    form.availabilityStart = value.availabilityStart ?? ''
    form.availabilityEnd = value.availabilityEnd ?? ''
    form.profileNote = value.profileNote ?? ''
  },
  { immediate: true, deep: true }
)

const validateForm = () => {
  errors.firstName = form.firstName.trim() ? '' : 'Imię jest wymagane.'
  errors.lastName = form.lastName.trim() ? '' : 'Nazwisko jest wymagane.'
  errors.specialization = form.specialization.trim() ? '' : 'Specjalizacja jest wymagana.'
  errors.licenseNumber = form.licenseNumber.trim() ? '' : 'Numer licencji jest wymagany.'
  errors.availableDays = form.availableDays.length > 0 ? '' : 'Wybierz co najmniej jeden dzień dyspozycyjności.'
  errors.availabilityWindow = ''

  if (form.availabilityStart && form.availabilityEnd && form.availabilityStart >= form.availabilityEnd) {
    errors.availabilityWindow = 'Godzina końcowa musi być późniejsza niż początkowa.'
  }

  if ((form.availabilityStart && !form.availabilityEnd) || (!form.availabilityStart && form.availabilityEnd)) {
    errors.availabilityWindow = 'Podaj obie godziny dyspozycyjności lub zostaw oba pola puste.'
  }

  return !errors.firstName && !errors.lastName && !errors.specialization && !errors.licenseNumber && !errors.availableDays && !errors.availabilityWindow
}

const handleSubmit = () => {
  if (!validateForm()) return

  emit('submit', {
    firstName: form.firstName.trim(),
    lastName: form.lastName.trim(),
    specialization: form.specialization.trim(),
    licenseNumber: form.licenseNumber.trim(),
    availableDays: [...form.availableDays],
    availabilityStart: form.availabilityStart || null,
    availabilityEnd: form.availabilityEnd || null,
    profileNote: form.profileNote?.trim() || null,
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

      <label class="form-field">
        <span>Numer licencji <span class="required">*</span></span>
        <input v-model="form.licenseNumber" type="text" placeholder="np. ORTO-PL-1234" />
        <small v-if="errors.licenseNumber">{{ errors.licenseNumber }}</small>
      </label>

      <fieldset class="form-field form-field--full">
        <legend>Dni dyspozycyjności <span class="required">*</span></legend>
        <div class="availability-days">
          <label v-for="day in dayOptions" :key="day.value" class="day-chip">
            <input v-model="form.availableDays" type="checkbox" :value="day.value" />
            <span>{{ day.label }}</span>
          </label>
        </div>
        <small v-if="errors.availableDays">{{ errors.availableDays }}</small>
      </fieldset>

      <div class="form-field">
        <span>Godzina od</span>
        <input v-model="form.availabilityStart" type="time" />
      </div>

      <div class="form-field">
        <span>Godzina do</span>
        <input v-model="form.availabilityEnd" type="time" />
      </div>

      <p v-if="errors.availabilityWindow" class="availability-error">
        {{ errors.availabilityWindow }}
      </p>

      <label class="form-field form-field--full">
        <span>Notatka profilu</span>
        <textarea v-model="form.profileNote" rows="4" placeholder="Informacje o lekarzu, stylu pracy, specjalnych preferencjach." />
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

.form-field textarea {
  width: 100%;
  padding: 0.8rem 1rem;
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-md);
  font-size: 0.95rem;
  font-family: inherit;
  color: var(--color-text);
  background: var(--color-surface);
  resize: vertical;
}

.form-field textarea:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 4px var(--color-primary-soft);
}

fieldset.form-field {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: 0.75rem 1rem 1rem;
}

fieldset legend {
  padding: 0 0.35rem;
  font-size: 0.9rem;
  font-weight: 700;
}

.availability-days {
  display: flex;
  flex-wrap: wrap;
  gap: 0.6rem;
  margin-top: 0.35rem;
}

.day-chip {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  border: 1px solid var(--color-border);
  border-radius: 999px;
  padding: 0.35rem 0.65rem;
  font-weight: 600;
  color: var(--color-text-muted);
}

.day-chip input {
  width: auto;
  height: auto;
}

.availability-error {
  margin: 0;
  color: var(--color-danger);
  font-size: 0.85rem;
  grid-column: 1 / -1;
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