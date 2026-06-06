<template>
  <PageLayout>
    <PageHeader
      title="Panel pacjenta"
      :subtitle="authSession?.patientName ? `Witaj, ${authSession.patientName}` : 'Twoje wizyty i umawianie terminu'"
    />

    <AppAlert v-if="success" :message="success" variant="success" @dismiss="success = null" />
    <AppAlert v-if="error" :message="error" variant="error" @dismiss="error = null" />
    <AppSpinner v-if="loading" label="Ladowanie panelu pacjenta..." />

    <template v-else>
      <section class="patient-grid">
        <article class="patient-panel">
          <header class="patient-panel__header">
            <h2>Umow nowa wizyte</h2>
            <span>Status: Do potwierdzenia</span>
          </header>

          <form class="patient-form" @submit.prevent="handleSubmit">
            <label class="patient-field">
              <span>Lekarz</span>
              <select v-model.number="form.doctorId" required>
                <option :value="0" disabled>Wybierz lekarza</option>
                <option v-for="doctor in doctors" :key="doctor.id" :value="doctor.id">
                  {{ getDoctorOptionLabel(doctor, form.appointmentDate) }}
                </option>
              </select>
            </label>

            <label class="patient-field">
              <span>Data i godzina</span>
              <input v-model="form.appointmentDate" type="datetime-local" required />
            </label>

            <p
              v-if="selectedDoctor && form.appointmentDate && selectedDoctorAvailability"
              class="doctor-availability"
              :class="selectedDoctorAvailability.available ? 'doctor-availability--ok' : 'doctor-availability--bad'"
            >
              {{ selectedDoctorAvailability.available ? '✓' : '✕' }}
              {{ selectedDoctorAvailability.message }}
            </p>

            <label class="patient-field">
              <span>Typ wizyty</span>
              <select v-model="form.visitType" required>
                <option value="" disabled>Wybierz typ wizyty</option>
                <option v-for="type in VISIT_TYPES" :key="type" :value="type">
                  {{ type }}
                </option>
              </select>
            </label>

            <label class="patient-field">
              <span>Notatka dla kliniki</span>
              <textarea v-model="form.notes" rows="4" placeholder="Opcjonalnie opisz powod wizyty" />
            </label>

            <button type="submit" class="app-btn app-btn--primary" :disabled="submitting || !canSubmit">
              {{ submitting ? 'Umawianie...' : 'Umow wizyte' }}
            </button>
          </form>
        </article>

        <article class="patient-panel">
          <header class="patient-panel__header">
            <h2>Moje wizyty</h2>
            <span>{{ visiblePatientAppointments.length }} wizyt</span>
          </header>

          <label class="patient-toggle">
            <input v-model="showCancelledAppointments" type="checkbox" />
            <span>Pokaz anulowane</span>
          </label>

          <div v-if="visiblePatientAppointments.length === 0" class="patient-empty">
            Nie masz jeszcze umowionych wizyt.
          </div>

          <ul v-else class="patient-appointments">
            <li
              v-for="appointment in visiblePatientAppointments"
              :key="appointment.id"
              :class="{ 'patient-appointments__item--cancelled': appointment.status === 'Anulowana' }"
            >
              <div>
                <strong>{{ formatAppointmentDate(appointment.appointmentDate) }}</strong>
                <span>{{ appointment.doctorName }} - {{ appointment.visitType }}</span>
              </div>
              <div class="patient-appointments__actions">
                <em>{{ appointment.status }}</em>
                <button
                  v-if="canCancelAppointment(appointment)"
                  type="button"
                  class="patient-cancel-btn"
                  @click="requestCancelAppointment(appointment)"
                >
                  Anuluj
                </button>
              </div>
            </li>
          </ul>
        </article>
      </section>
    </template>

    <div v-if="cancelModalVisible" class="patient-modal-backdrop" @click.self="closeCancelModal">
      <section class="patient-modal" role="dialog" aria-modal="true" aria-labelledby="cancel-title">
        <h2 id="cancel-title">Anulowac wizyte?</h2>
        <p>
          Czy na pewno chcesz anulowac wizyte
          <strong>{{ appointmentToCancel ? formatAppointmentDate(appointmentToCancel.appointmentDate) : '' }}</strong>?
        </p>
        <div class="patient-modal__actions">
          <button type="button" class="app-btn app-btn--secondary" :disabled="cancelling" @click="closeCancelModal">
            Nie
          </button>
          <button type="button" class="app-btn app-btn--danger" :disabled="cancelling" @click="confirmCancelAppointment">
            {{ cancelling ? 'Anulowanie...' : 'Tak, anuluj' }}
          </button>
        </div>
      </section>
    </div>
  </PageLayout>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import PageLayout from '../../components/common/PageLayout.vue'
import PageHeader from '../../components/common/PageHeader.vue'
import AppAlert from '../../components/common/AppAlert.vue'
import AppSpinner from '../../components/common/AppSpinner.vue'
import { authSession } from '../../services/authService'
import { createAppointment, getAppointments, updateAppointmentStatus } from '../../services/appointmentService'
import { getDoctors } from '../../services/doctorService'
import type { Appointment, AppointmentFormPayload } from '../../types/appointment'
import { formatAppointmentDate, VISIT_TYPES } from '../../types/appointment'
import type { Doctor } from '../../types/doctor'
import { getApiErrorMessage } from '../../utils/apiError'
import {
  getDoctorAvailability,
  getDoctorOptionLabel,
  isDoctorAvailable,
} from '../../utils/doctorAvailability'

const appointments = ref<Appointment[]>([])
const doctors = ref<Doctor[]>([])
const loading = ref(true)
const submitting = ref(false)
const cancelling = ref(false)
const showCancelledAppointments = ref(false)
const error = ref<string | null>(null)
const success = ref<string | null>(null)
const appointmentToCancel = ref<Appointment | null>(null)
const cancelModalVisible = ref(false)

const form = ref<AppointmentFormPayload>({
  patientId: authSession.value?.patientId ?? 0,
  doctorId: 0,
  appointmentDate: '',
  status: 'Do potwierdzenia',
  visitType: '',
  notes: '',
})

const patientAppointments = computed(() => {
  const patientId = authSession.value?.patientId
  if (!patientId) return []

  return appointments.value
    .filter((appointment) => appointment.patientId === patientId)
    .sort((a, b) => a.appointmentDate.localeCompare(b.appointmentDate))
})

const visiblePatientAppointments = computed(() => {
  if (showCancelledAppointments.value) return patientAppointments.value
  return patientAppointments.value.filter((appointment) => appointment.status !== 'Anulowana')
})

const selectedDoctor = computed(() =>
  doctors.value.find((doctor) => doctor.id === form.value.doctorId) ?? null,
)

const selectedDoctorAvailability = computed(() => {
  if (!selectedDoctor.value || !form.value.appointmentDate) return null
  return getDoctorAvailability(selectedDoctor.value, form.value.appointmentDate)
})

const canSubmit = computed(() => {
  return Boolean(
    form.value.patientId &&
      form.value.doctorId &&
      form.value.appointmentDate &&
      form.value.visitType &&
      selectedDoctor.value &&
      isDoctorAvailable(selectedDoctor.value, form.value.appointmentDate),
  )
})

const loadData = async () => {
  loading.value = true
  error.value = null

  try {
    const [loadedAppointments, loadedDoctors] = await Promise.all([getAppointments(), getDoctors()])
    appointments.value = loadedAppointments
    doctors.value = loadedDoctors
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udalo sie zaladowac panelu pacjenta.')
  } finally {
    loading.value = false
  }
}

const handleSubmit = async () => {
  if (!canSubmit.value) {
    if (selectedDoctor.value && form.value.appointmentDate && !isDoctorAvailable(selectedDoctor.value, form.value.appointmentDate)) {
      error.value = selectedDoctorAvailability.value?.message ?? 'Wybrany lekarz nie jest dostępny w podanym terminie.'
    }
    return
  }

  submitting.value = true
  error.value = null
  success.value = null

  try {
    await createAppointment({
      ...form.value,
      patientId: authSession.value?.patientId ?? form.value.patientId,
      status: 'Do potwierdzenia',
    })
    success.value = 'Wizyta zostala umowiona i czeka na potwierdzenie kliniki.'
    form.value = {
      patientId: authSession.value?.patientId ?? 0,
      doctorId: 0,
      appointmentDate: '',
      status: 'Do potwierdzenia',
      visitType: '',
      notes: '',
    }
    await loadData()
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udalo sie umowic wizyty.')
  } finally {
    submitting.value = false
  }
}

const canCancelAppointment = (appointment: Appointment): boolean => {
  return appointment.status !== 'Anulowana' && !appointment.status.includes('Zako')
}

const requestCancelAppointment = (appointment: Appointment) => {
  appointmentToCancel.value = appointment
  cancelModalVisible.value = true
}

const closeCancelModal = () => {
  if (cancelling.value) return
  appointmentToCancel.value = null
  cancelModalVisible.value = false
}

const confirmCancelAppointment = async () => {
  if (!appointmentToCancel.value) return

  cancelling.value = true
  error.value = null
  success.value = null

  try {
    await updateAppointmentStatus(appointmentToCancel.value.id, 'Anulowana')
    success.value = 'Wizyta zostala anulowana.'
    appointmentToCancel.value = null
    cancelModalVisible.value = false
    await loadData()
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udalo sie anulowac wizyty.')
  } finally {
    cancelling.value = false
  }
}

onMounted(loadData)
</script>

<style scoped>
.patient-grid {
  display: grid;
  grid-template-columns: minmax(320px, 0.9fr) minmax(0, 1.1fr);
  gap: 1.25rem;
}

.patient-panel {
  min-width: 0;
  padding: 1.25rem;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
}

.patient-panel__header {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1rem;
}

.patient-panel__header h2 {
  margin: 0;
  font-size: 1.1rem;
}

.patient-panel__header span {
  color: var(--color-text-muted);
  font-size: 0.8rem;
  font-weight: 700;
}

.patient-form {
  display: grid;
  gap: 0.85rem;
}

.patient-field {
  display: grid;
  gap: 0.35rem;
}

.patient-field span {
  color: var(--color-text-muted);
  font-size: 0.76rem;
  font-weight: 800;
  text-transform: uppercase;
}

.patient-field input,
.patient-field select,
.patient-field textarea {
  width: 100%;
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-sm);
  color: var(--color-text);
  font: inherit;
  padding: 0.65rem 0.75rem;
}

.patient-field input:focus,
.patient-field select:focus,
.patient-field textarea:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px var(--color-primary-soft);
}

.doctor-availability {
  margin: -0.25rem 0 0;
  padding: 0.65rem 0.75rem;
  border-radius: var(--radius-sm);
  font-size: 0.82rem;
  font-weight: 600;
  line-height: 1.45;
}

.doctor-availability--ok {
  background: #ecfdf5;
  border: 1px solid #a7f3d0;
  color: #065f46;
}

.doctor-availability--bad {
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #991b1b;
}

.patient-appointments {
  display: grid;
  gap: 0.75rem;
  margin: 0;
  padding: 0;
  list-style: none;
}

.patient-toggle {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  width: fit-content;
  margin: -0.25rem 0 0.85rem;
  color: var(--color-text-muted);
  cursor: pointer;
  font-size: 0.84rem;
  font-weight: 700;
}

.patient-toggle input {
  width: 1rem;
  height: 1rem;
  accent-color: var(--color-primary);
}

.patient-appointments li {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.85rem;
  background: #f8fafc;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
}

.patient-appointments__item--cancelled {
  opacity: 0.68;
}

.patient-appointments div {
  display: grid;
  gap: 0.25rem;
}

.patient-appointments span {
  color: var(--color-text-muted);
  font-size: 0.86rem;
}

.patient-appointments em {
  flex-shrink: 0;
  color: var(--color-primary);
  font-size: 0.8rem;
  font-style: normal;
  font-weight: 800;
}

.patient-appointments__actions {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.45rem;
}

.patient-cancel-btn {
  width: 6.75rem;
  min-height: 2rem;
  border: 1px solid #fecaca;
  border-radius: var(--radius-sm);
  background: #fff;
  color: #b91c1c;
  cursor: pointer;
  font: inherit;
  font-size: 0.78rem;
  font-weight: 800;
  padding: 0.35rem 0.6rem;
  text-align: center;
}

.patient-cancel-btn:hover {
  background: #fef2f2;
}

.patient-modal-backdrop {
  position: fixed;
  inset: 0;
  z-index: 500;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background: rgba(15, 23, 42, 0.46);
}

.patient-modal {
  width: min(100%, 420px);
  padding: 1.35rem;
  background: #fff;
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
}

.patient-modal h2 {
  margin: 0 0 0.65rem;
  font-size: 1.15rem;
}

.patient-modal p {
  margin: 0;
  color: var(--color-text-muted);
  line-height: 1.5;
}

.patient-modal__actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.65rem;
  margin-top: 1.25rem;
}

.patient-empty {
  padding: 1rem;
  color: var(--color-text-muted);
  background: #f8fafc;
  border: 1px dashed var(--color-border-strong);
  border-radius: var(--radius-sm);
  text-align: center;
}

@media (max-width: 900px) {
  .patient-grid {
    grid-template-columns: 1fr;
  }
}
</style>
