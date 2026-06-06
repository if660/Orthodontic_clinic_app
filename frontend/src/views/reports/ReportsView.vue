<template>
  <PageLayout>
    <PageHeader
      title="Raporty wizyt"
      subtitle="Analiza wizyt wedlug okresu, lekarza i specjalizacji"
    />

    <AppAlert v-if="error" :message="error" variant="error" @dismiss="error = null" />
    <AppSpinner v-if="loading" label="Ladowanie raportow..." />

    <template v-else>
      <section class="report-filters" aria-label="Filtry raportu">
        <label class="report-field">
          <span>Okres</span>
          <select v-model="period" class="report-select">
            <option value="day">Dzien</option>
            <option value="week">Tydzien</option>
            <option value="month">Miesiac</option>
          </select>
        </label>

        <label class="report-field">
          <span>Data</span>
          <input v-model="selectedDate" class="report-input" type="date" />
        </label>

        <label class="report-field">
          <span>Lekarz</span>
          <select v-model="selectedDoctorId" class="report-select">
            <option value="all">Wszyscy lekarze</option>
            <option v-for="doctor in doctors" :key="doctor.id" :value="doctor.id">
              {{ getDoctorLabel(doctor.id) }}
            </option>
          </select>
        </label>

        <label class="report-field">
          <span>Specjalizacja</span>
          <select v-model="selectedSpecialization" class="report-select">
            <option value="all">Wszystkie</option>
            <option v-for="specialization in specializations" :key="specialization" :value="specialization">
              {{ specialization }}
            </option>
          </select>
        </label>
      </section>

      <section class="report-summary" aria-label="Podsumowanie wizyt">
        <article class="report-kpi">
          <span class="report-kpi__label">Laczna liczba wizyt</span>
          <strong>{{ totalCount }}</strong>
          <small>{{ rangeLabel }}</small>
        </article>

        <article v-for="item in statusSummary" :key="item.status" class="report-kpi">
          <span class="report-kpi__label">{{ item.label }}</span>
          <strong>{{ item.count }}</strong>
          <small>{{ item.percent }}% wizyt</small>
        </article>
      </section>

      <section class="report-grid">
        <article class="report-panel">
          <header class="report-panel__header">
            <h2>Wizyty wedlug statusu</h2>
            <span>{{ filteredAppointments.length }} wynikow</span>
          </header>

          <div v-if="statusSummary.length === 0" class="report-empty">
            Brak wizyt dla wybranych filtrow.
          </div>

          <div v-else class="metric-list">
            <div v-for="item in statusSummary" :key="item.status" class="metric-row">
              <div class="metric-row__top">
                <span>{{ item.label }}</span>
                <strong>{{ item.count }}</strong>
              </div>
              <div class="metric-row__bar" aria-hidden="true">
                <span :style="{ width: `${item.percent}%` }" />
              </div>
            </div>
          </div>
        </article>

        <article class="report-panel">
          <header class="report-panel__header">
            <h2>Wizyty wedlug lekarzy</h2>
            <span>{{ appointmentsByDoctor.length }} lekarzy</span>
          </header>

          <div v-if="appointmentsByDoctor.length === 0" class="report-empty">
            Brak lekarzy w wybranym zakresie.
          </div>

          <div v-else class="metric-list">
            <div v-for="item in appointmentsByDoctor" :key="item.doctorId" class="metric-row">
              <div class="metric-row__top">
                <span>{{ item.name }}</span>
                <strong>{{ item.count }}</strong>
              </div>
              <p class="metric-row__meta">{{ item.specialization || 'Brak specjalizacji' }}</p>
              <div class="metric-row__bar metric-row__bar--doctor" aria-hidden="true">
                <span :style="{ width: `${item.percent}%` }" />
              </div>
            </div>
          </div>
        </article>
      </section>

      <section class="report-panel report-panel--wide">
        <header class="report-panel__header">
          <h2>Wizyty w raporcie</h2>
          <span>{{ filteredAppointments.length }} z {{ appointments.length }}</span>
        </header>

        <div v-if="filteredAppointments.length === 0" class="report-empty">
          Brak wizyt spelniajacych wybrane kryteria.
        </div>

        <table v-else class="report-table">
          <thead>
            <tr>
              <th>Data</th>
              <th>Pacjent</th>
              <th>Lekarz</th>
              <th>Typ wizyty</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="appointment in filteredAppointments" :key="appointment.id">
              <td>{{ formatAppointmentDate(appointment.appointmentDate) }}</td>
              <td>{{ appointment.patientName || 'Brak danych' }}</td>
              <td>{{ getDoctorLabel(appointment.doctorId) }}</td>
              <td>{{ appointment.visitType || 'Nie podano' }}</td>
              <td>
                <span class="report-status" :class="getStatusClass(appointment.status)">
                  {{ getStatusLabel(appointment.status) }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </section>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { getAppointments } from '../../services/appointmentService'
import { getDoctors } from '../../services/doctorService'
import PageLayout from '../../components/common/PageLayout.vue'
import PageHeader from '../../components/common/PageHeader.vue'
import AppAlert from '../../components/common/AppAlert.vue'
import AppSpinner from '../../components/common/AppSpinner.vue'
import type { Appointment } from '../../types/appointment'
import { APPOINTMENT_STATUS_OPTIONS, formatAppointmentDate } from '../../types/appointment'
import type { Doctor } from '../../types/doctor'
import { getApiErrorMessage } from '../../utils/apiError'

type ReportPeriod = 'day' | 'week' | 'month'

const appointments = ref<Appointment[]>([])
const doctors = ref<Doctor[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const period = ref<ReportPeriod>('week')
const selectedDate = ref(toDateInputValue(new Date()))
const selectedDoctorId = ref<string | number>('all')
const selectedSpecialization = ref('all')

const doctorMap = computed(() => {
  return new Map(doctors.value.map((doctor) => [doctor.id, doctor]))
})

const specializations = computed(() => {
  return [...new Set(doctors.value.map((doctor) => doctor.specialization).filter(Boolean))].sort((a, b) =>
    a.localeCompare(b, 'pl'),
  )
})

const reportRange = computed(() => getReportRange(selectedDate.value, period.value))

const rangeLabel = computed(() => {
  const formatter = new Intl.DateTimeFormat('pl-PL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })

  if (isSameDay(reportRange.value.start, reportRange.value.end)) {
    return formatter.format(reportRange.value.start)
  }

  return `${formatter.format(reportRange.value.start)} - ${formatter.format(reportRange.value.end)}`
})

const filteredAppointments = computed(() => {
  const { start, end } = reportRange.value

  return appointments.value
    .filter((appointment) => {
      const appointmentDate = new Date(appointment.appointmentDate)
      if (Number.isNaN(appointmentDate.getTime())) return false
      if (appointmentDate < start || appointmentDate > end) return false

      if (selectedDoctorId.value !== 'all' && appointment.doctorId !== Number(selectedDoctorId.value)) {
        return false
      }

      if (selectedSpecialization.value !== 'all') {
        const doctor = doctorMap.value.get(appointment.doctorId)
        return doctor?.specialization === selectedSpecialization.value
      }

      return true
    })
    .sort((a, b) => a.appointmentDate.localeCompare(b.appointmentDate))
})

const totalCount = computed(() => filteredAppointments.value.length)

const statusSummary = computed(() => {
  return APPOINTMENT_STATUS_OPTIONS
    .map((status) => {
      const count = filteredAppointments.value.filter((appointment) => appointment.status === status).length
      return {
        status,
        label: getStatusLabel(status),
        count,
        percent: getPercent(count, totalCount.value),
      }
    })
    .filter((item) => item.count > 0)
})

const appointmentsByDoctor = computed(() => {
  const counts = new Map<number, number>()

  filteredAppointments.value.forEach((appointment) => {
    counts.set(appointment.doctorId, (counts.get(appointment.doctorId) ?? 0) + 1)
  })

  return [...counts.entries()]
    .map(([doctorId, count]) => {
      const doctor = doctorMap.value.get(doctorId)
      return {
        doctorId,
        count,
        name: getDoctorLabel(doctorId),
        specialization: doctor?.specialization ?? '',
        percent: getPercent(count, totalCount.value),
      }
    })
    .sort((a, b) => b.count - a.count || a.name.localeCompare(b.name, 'pl'))
})

watch(selectedSpecialization, () => {
  if (selectedSpecialization.value === 'all' || selectedDoctorId.value === 'all') return
  const selectedDoctor = doctorMap.value.get(Number(selectedDoctorId.value))
  if (selectedDoctor?.specialization !== selectedSpecialization.value) {
    selectedDoctorId.value = 'all'
  }
})

const loadData = async () => {
  loading.value = true
  error.value = null

  try {
    const [loadedAppointments, loadedDoctors] = await Promise.all([getAppointments(), getDoctors()])
    appointments.value = loadedAppointments
    doctors.value = loadedDoctors
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udalo sie zaladowac raportow.')
  } finally {
    loading.value = false
  }
}

function toDateInputValue(date: Date): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function getReportRange(value: string, selectedPeriod: ReportPeriod): { start: Date; end: Date } {
  const base = value ? new Date(`${value}T12:00:00`) : new Date()
  const start = new Date(base)
  const end = new Date(base)

  if (selectedPeriod === 'week') {
    const mondayOffset = (base.getDay() + 6) % 7
    start.setDate(base.getDate() - mondayOffset)
    end.setDate(start.getDate() + 6)
  }

  if (selectedPeriod === 'month') {
    start.setDate(1)
    end.setMonth(start.getMonth() + 1, 0)
  }

  start.setHours(0, 0, 0, 0)
  end.setHours(23, 59, 59, 999)

  return { start, end }
}

function isSameDay(first: Date, second: Date): boolean {
  return first.toDateString() === second.toDateString()
}

function getPercent(count: number, total: number): number {
  if (total === 0 || count === 0) return 0
  return Math.round((count / total) * 100)
}

function getDoctorLabel(doctorId: number): string {
  const doctor = doctorMap.value.get(doctorId)
  if (!doctor) {
    return appointments.value.find((appointment) => appointment.doctorId === doctorId)?.doctorName || 'Nieznany lekarz'
  }
  return `${doctor.firstName} ${doctor.lastName}`.trim()
}

function getStatusLabel(status: string): string {
  const labels: Record<string, string> = {
    'Do potwierdzenia': 'Do potwierdzenia',
    Zaplanowana: 'Zaplanowane',
    Potwierdzona: 'Potwierdzone',
    'Zakończona': 'Zakonczone',
    Anulowana: 'Anulowane',
    'Nieobecność': 'Nieobecnosc',
  }

  return labels[status] ?? status
}

function getStatusClass(status: string) {
  return {
    'report-status--pending': status === 'Do potwierdzenia',
    'report-status--planned': status === 'Zaplanowana',
    'report-status--confirmed': status === 'Potwierdzona',
    'report-status--done': status === 'Zakończona',
    'report-status--cancelled': status === 'Anulowana',
    'report-status--absent': status === 'Nieobecność',
  }
}

onMounted(loadData)
</script>

<style scoped>
.report-filters {
  display: grid;
  grid-template-columns: repeat(4, minmax(160px, 1fr));
  gap: 0.9rem;
  margin-bottom: 1.25rem;
  padding: 1rem;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
}

.report-field {
  display: grid;
  gap: 0.35rem;
  min-width: 0;
}

.report-field span {
  color: var(--color-text-muted);
  font-size: 0.76rem;
  font-weight: 700;
  text-transform: uppercase;
}

.report-select,
.report-input {
  width: 100%;
  min-height: 2.55rem;
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-sm);
  background: #fff;
  color: var(--color-text);
  font: inherit;
  padding: 0.55rem 0.7rem;
}

.report-select:focus,
.report-input:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px var(--color-primary-soft);
}

.report-summary {
  display: grid;
  grid-template-columns: repeat(5, minmax(150px, 1fr));
  gap: 0.9rem;
  margin-bottom: 1.25rem;
}

.report-kpi {
  min-height: 7rem;
  display: grid;
  gap: 0.35rem;
  align-content: center;
  padding: 1rem;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
}

.report-kpi__label {
  color: var(--color-text-muted);
  font-size: 0.82rem;
  font-weight: 700;
}

.report-kpi strong {
  color: var(--color-text);
  font-size: 2rem;
  line-height: 1;
}

.report-kpi small {
  color: var(--color-text-muted);
  font-size: 0.78rem;
}

.report-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1.25rem;
  margin-bottom: 1.25rem;
}

.report-panel {
  min-width: 0;
  padding: 1.25rem;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
}

.report-panel--wide {
  overflow-x: auto;
}

.report-panel__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1rem;
}

.report-panel__header h2 {
  margin: 0;
  color: var(--color-text);
  font-size: 1rem;
  line-height: 1.3;
}

.report-panel__header span {
  flex-shrink: 0;
  color: var(--color-text-muted);
  font-size: 0.8rem;
  font-weight: 700;
}

.metric-list {
  display: grid;
  gap: 0.9rem;
}

.metric-row {
  display: grid;
  gap: 0.35rem;
}

.metric-row__top {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 0.75rem;
  color: var(--color-text);
  font-size: 0.92rem;
}

.metric-row__meta {
  margin: 0;
  color: var(--color-text-muted);
  font-size: 0.78rem;
}

.metric-row__bar {
  height: 0.45rem;
  overflow: hidden;
  background: #e2e8f0;
  border-radius: 999px;
}

.metric-row__bar span {
  display: block;
  height: 100%;
  min-width: 0.35rem;
  background: var(--color-primary);
  border-radius: inherit;
}

.metric-row__bar--doctor span {
  background: var(--color-success);
}

.report-empty {
  padding: 1rem;
  color: var(--color-text-muted);
  background: #f8fafc;
  border: 1px dashed var(--color-border-strong);
  border-radius: var(--radius-sm);
  text-align: center;
}

.report-table {
  width: 100%;
  min-width: 760px;
  border-collapse: collapse;
}

.report-table th,
.report-table td {
  padding: 0.85rem 0.75rem;
  border-bottom: 1px solid var(--color-border);
  text-align: left;
  vertical-align: middle;
}

.report-table th {
  color: var(--color-text-muted);
  font-size: 0.76rem;
  font-weight: 800;
  text-transform: uppercase;
}

.report-table td {
  color: var(--color-text);
  font-size: 0.9rem;
}

.report-status {
  display: inline-flex;
  align-items: center;
  min-height: 1.7rem;
  padding: 0.25rem 0.6rem;
  border-radius: 999px;
  background: #f1f5f9;
  color: var(--color-text-muted);
  font-size: 0.78rem;
  font-weight: 800;
}

.report-status--planned {
  background: #eff6ff;
  color: #1d4ed8;
}

.report-status--pending {
  background: #fee2e2;
  color: #b91c1c;
}

.report-status--confirmed {
  background: #ecfeff;
  color: #0e7490;
}

.report-status--done {
  background: #ecfdf5;
  color: #047857;
}

.report-status--cancelled,
.report-status--absent {
  background: #fef2f2;
  color: #b91c1c;
}

@media (max-width: 1100px) {
  .report-filters,
  .report-summary {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 760px) {
  .report-grid,
  .report-filters,
  .report-summary {
    grid-template-columns: 1fr;
  }

  .report-panel__header {
    flex-direction: column;
    gap: 0.35rem;
  }
}
</style>
