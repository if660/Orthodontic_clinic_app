<template>
  <PageLayout narrow>
    <PageHeader title="Edytuj wizytę" subtitle="Zmień dane zaplanowanej wizyty">
      <template #actions>
        <button type="button" class="app-btn app-btn--ghost app-btn--sm" @click="goBack">
          ← Wróć
        </button>
      </template>
    </PageHeader>

    <AppSpinner v-if="loading" label="Ładowanie danych wizyty..." />

    <AppAlert v-if="error" :message="error" variant="error" @dismiss="error = null" />

    <form v-if="!loading" class="appt-form" @submit.prevent="handleSubmit">
      <!-- Date/time header -->
      <div class="appt-form__date-card">
        <div class="appt-form__date-icon">📅</div>
        <div class="appt-form__date-fields">
          <label class="appt-form__date-label">Data i godzina wizyty</label>
          <input
            id="appointmentDate"
            v-model="form.appointmentDate"
            type="datetime-local"
            class="appt-form__date-input"
            required
          />
          <p v-if="readableDateLabel" class="appt-form__date-readable">{{ readableDateLabel }}</p>
        </div>
      </div>

      <!-- Patient + Doctor row -->
      <div class="appt-form__two-col">
        <div class="appt-form__field">
          <label class="appt-form__label" for="patientId">
            <span class="appt-form__field-icon">👤</span> Pacjent
          </label>
          <select
            id="patientId"
            v-model.number="form.patientId"
            class="appt-form__select"
            required
          >
            <option :value="0" disabled>Wybierz pacjenta...</option>
            <option v-for="p in patients" :key="p.id" :value="p.id">
              {{ p.firstName }} {{ p.lastName }}
            </option>
          </select>
          <p v-if="selectedPatient" class="appt-form__field-hint">
            {{ selectedPatient.phone || selectedPatient.email || '' }}
          </p>
        </div>

        <div class="appt-form__field">
          <label class="appt-form__label" for="doctorId">
            <span class="appt-form__field-icon">🩺</span> Lekarz
          </label>
          <select
            id="doctorId"
            v-model.number="form.doctorId"
            class="appt-form__select"
            required
          >
            <option :value="0" disabled>Wybierz lekarza...</option>
            <option v-for="d in doctors" :key="d.id" :value="d.id">
              {{ d.firstName }} {{ d.lastName }}
            </option>
          </select>
          <p v-if="selectedDoctor" class="appt-form__field-hint">
            {{ selectedDoctor.specialization }}
          </p>
        </div>
      </div>

      <!-- Visit type chips -->
      <div class="appt-form__field">
        <label class="appt-form__label">
          <span class="appt-form__field-icon">🔖</span> Typ wizyty
        </label>
        <div class="visit-type-grid">
          <button
            v-for="vt in VISIT_TYPES"
            :key="vt"
            type="button"
            class="visit-type-chip"
            :class="{ 'visit-type-chip--selected': form.visitType === vt }"
            @click="form.visitType = vt"
          >
            <span class="visit-type-chip__icon">{{ visitTypeIcon(vt) }}</span>
            {{ vt }}
          </button>
        </div>
      </div>

      <!-- Status pills -->
      <div class="appt-form__field">
        <label class="appt-form__label">
          <span class="appt-form__field-icon">📋</span> Status
        </label>
        <div class="status-pill-row">
          <button
            v-for="s in APPOINTMENT_STATUSES"
            :key="s"
            type="button"
            class="status-pill"
            :class="[statusPillClass(s), { 'status-pill--selected': form.status === s }]"
            @click="form.status = s"
          >
            {{ s }}
          </button>
        </div>
      </div>

      <!-- Notes -->
      <div class="appt-form__field">
        <label class="appt-form__label" for="notes">
          <span class="appt-form__field-icon">📝</span> Notatki
          <span class="appt-form__optional">(opcjonalnie)</span>
        </label>
        <textarea
          id="notes"
          v-model="form.notes"
          class="appt-form__textarea"
          rows="3"
          placeholder="Dodatkowe informacje, uwagi do wizyty..."
        />
      </div>

      <!-- Actions -->
      <div class="appt-form__actions">
        <button type="button" class="app-btn app-btn--secondary" @click="goBack">
          Anuluj
        </button>
        <button
          type="submit"
          class="app-btn app-btn--primary appt-form__submit"
          :disabled="submitting"
        >
          <span v-if="submitting">Zapisywanie...</span>
          <span v-else>Zapisz zmiany →</span>
        </button>
      </div>
    </form>
  </PageLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import PageLayout from '../../components/common/PageLayout.vue'
import PageHeader from '../../components/common/PageHeader.vue'
import AppAlert from '../../components/common/AppAlert.vue'
import AppSpinner from '../../components/common/AppSpinner.vue'
import { getAppointmentById, updateAppointment } from '../../services/appointmentService'
import { getDoctors } from '../../services/doctorService'
import { getPatients } from '../../services/patientService'
import {
  toAppointmentFormPayload,
  APPOINTMENT_STATUSES,
  VISIT_TYPES,
} from '../../types/appointment'
import type { AppointmentFormPayload } from '../../types/appointment'
import type { Doctor } from '../../types/doctor'
import type { Patient } from '../../types/patient'
import { getApiErrorMessage } from '../../utils/apiError'

const props = defineProps<{ id: string }>()
const router = useRouter()
const loading = ref(true)
const submitting = ref(false)
const error = ref<string | null>(null)
const form = ref<AppointmentFormPayload>({
  patientId: 0,
  doctorId: 0,
  appointmentDate: '',
  status: 'Zaplanowana',
  visitType: '',
  notes: '',
})
const doctors = ref<Doctor[]>([])
const patients = ref<Patient[]>([])

const selectedDoctor = computed(() =>
  doctors.value.find((d) => d.id === form.value.doctorId) ?? null,
)

const selectedPatient = computed(() =>
  patients.value.find((p) => p.id === form.value.patientId) ?? null,
)

const readableDateLabel = computed(() => {
  if (!form.value.appointmentDate) return ''
  const d = new Date(form.value.appointmentDate)
  if (Number.isNaN(d.getTime())) return ''
  return d.toLocaleDateString('pl-PL', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  })
})

const visitTypeIcon = (vt: string): string => {
  const icons: Record<string, string> = {
    'Pierwsza konsultacja': '🆕',
    'Kontrola aparatu': '🔧',
    'Zakładanie aparatu': '🦷',
    'Zdjęcie aparatu': '✅',
    'Retainer': '📐',
    'Inne': '📌',
  }
  return icons[vt] ?? '📌'
}

const statusPillClass = (s: string) => {
  switch (s) {
    case 'Potwierdzona': return 'status-pill--confirmed'
    case 'Zakończona': return 'status-pill--done'
    case 'Anulowana': return 'status-pill--cancelled'
    case 'Nieobecność': return 'status-pill--absent'
    default: return 'status-pill--planned'
  }
}

const goBack = () => router.go(-1)

onMounted(async () => {
  try {
    const [appointment, d, p] = await Promise.all([
      getAppointmentById(Number(props.id)),
      getDoctors(),
      getPatients(),
    ])
    form.value = toAppointmentFormPayload(appointment)
    doctors.value = d
    patients.value = p
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udało się załadować danych wizyty.')
  } finally {
    loading.value = false
  }
})

const handleSubmit = async () => {
  submitting.value = true
  error.value = null
  try {
    await updateAppointment(Number(props.id), form.value)
    router.push({ name: 'appointments' })
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udało się zaktualizować wizyty.')
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.appt-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.appt-form__date-card {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
  border: 1px solid #bfdbfe;
  border-radius: var(--radius-lg);
  padding: 1.25rem 1.5rem;
}

.appt-form__date-icon {
  font-size: 2rem;
  line-height: 1;
  flex-shrink: 0;
  margin-top: 0.1rem;
}

.appt-form__date-fields {
  flex: 1;
  min-width: 0;
}

.appt-form__date-label {
  display: block;
  font-size: 0.78rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: #1d4ed8;
  margin-bottom: 0.4rem;
}

.appt-form__date-input {
  border: 1.5px solid #93c5fd;
  border-radius: var(--radius-md);
  background: rgba(255, 255, 255, 0.8);
  padding: 0.55rem 0.75rem;
  font: inherit;
  font-size: 1rem;
  font-weight: 600;
  color: #1e3a8a;
  width: 100%;
  max-width: 300px;
  transition: border-color 0.15s;
}

.appt-form__date-input:focus {
  outline: none;
  border-color: #3b82f6;
  background: #fff;
}

.appt-form__date-readable {
  margin: 0.4rem 0 0;
  font-size: 0.85rem;
  color: #2563eb;
  font-weight: 600;
  text-transform: capitalize;
}

.appt-form__two-col {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

@media (max-width: 600px) {
  .appt-form__two-col {
    grid-template-columns: 1fr;
  }
}

.appt-form__field {
  background: #fff;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: 1rem 1.25rem;
}

.appt-form__label {
  display: flex;
  align-items: center;
  gap: 0.45rem;
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
  margin-bottom: 0.65rem;
}

.appt-form__field-icon {
  font-size: 1rem;
}

.appt-form__optional {
  font-weight: 400;
  text-transform: none;
  font-size: 0.75rem;
  letter-spacing: 0;
}

.appt-form__select {
  width: 100%;
  border: 1.5px solid var(--color-border-strong);
  border-radius: var(--radius-md);
  background: #fff;
  padding: 0.6rem 0.75rem;
  font: inherit;
  font-size: 0.95rem;
  color: var(--color-text);
  cursor: pointer;
  transition: border-color 0.15s;
  appearance: auto;
}

.appt-form__select:focus {
  outline: none;
  border-color: var(--color-primary);
}

.appt-form__field-hint {
  margin: 0.4rem 0 0;
  font-size: 0.78rem;
  color: var(--color-text-muted);
}

.visit-type-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 0.5rem;
}

.visit-type-chip {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.6rem 0.85rem;
  border: 2px solid var(--color-border);
  border-radius: var(--radius-md);
  background: #f8fafc;
  font: inherit;
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--color-text-muted);
  cursor: pointer;
  transition: border-color 0.15s, background 0.15s, color 0.15s, transform 0.1s;
  text-align: left;
}

.visit-type-chip:hover {
  border-color: var(--color-primary);
  color: var(--color-primary);
  background: var(--color-primary-soft);
  transform: translateY(-1px);
}

.visit-type-chip--selected {
  border-color: var(--color-primary);
  background: #eff6ff;
  color: #1d4ed8;
}

.visit-type-chip__icon {
  font-size: 1.1rem;
  flex-shrink: 0;
}

.status-pill-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.status-pill {
  padding: 0.45rem 1rem;
  border-radius: 999px;
  border: 2px solid transparent;
  font: inherit;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  opacity: 0.55;
  transition: opacity 0.15s, border-color 0.15s, transform 0.1s;
}

.status-pill:hover {
  opacity: 0.8;
  transform: translateY(-1px);
}

.status-pill--selected {
  opacity: 1 !important;
  border-color: currentColor;
}

.status-pill--planned { background: #dbeafe; color: #1d4ed8; }
.status-pill--confirmed { background: #d1fae5; color: #065f46; }
.status-pill--done { background: #e2e8f0; color: #475569; }
.status-pill--cancelled { background: #fee2e2; color: #991b1b; }
.status-pill--absent { background: #fef3c7; color: #92400e; }

.appt-form__textarea {
  width: 100%;
  border: 1.5px solid var(--color-border-strong);
  border-radius: var(--radius-md);
  background: #fff;
  padding: 0.65rem 0.75rem;
  font: inherit;
  font-size: 0.92rem;
  color: var(--color-text);
  resize: vertical;
  min-height: 90px;
  transition: border-color 0.15s;
  box-sizing: border-box;
}

.appt-form__textarea:focus {
  outline: none;
  border-color: var(--color-primary);
}

.appt-form__actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding-top: 0.5rem;
}

.appt-form__submit {
  min-width: 160px;
}
</style>
