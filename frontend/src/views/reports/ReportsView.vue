<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AppAlert from '../../components/common/AppAlert.vue'
import AppSpinner from '../../components/common/AppSpinner.vue'
import PageHeader from '../../components/common/PageHeader.vue'
import PageLayout from '../../components/common/PageLayout.vue'
import { getAppointments } from '../../services/appointmentService'
import { getDoctors } from '../../services/doctorService'
import type { Appointment } from '../../types/appointment'
import { APPOINTMENT_STATUS_OPTIONS } from '../../types/appointment'
import type { Doctor } from '../../types/doctor'
import { getDoctorFullName } from '../../types/doctor'

type ReportPeriod = 'day' | 'week' | 'month'

const appointments = ref<Appointment[]>([])
const doctors = ref<Doctor[]>([])
const isLoading = ref(true)
const errorMessage = ref('')
const selectedPeriod = ref<ReportPeriod>('month')
const selectedDoctorId = ref('all')
const selectedDate = ref(formatDateKey(new Date()))

const periodOptions: { value: ReportPeriod; label: string }[] = [
  { value: 'day', label: 'Dzień' },
  { value: 'week', label: 'Tydzień' },
  { value: 'month', label: 'Miesiąc' },
]

const doctorOptions = computed(() =>
  doctors.value.map((doctor) => ({
    id: String(doctor.id),
    name: getDoctorFullName(doctor) || `Lekarz #${doctor.id}`,
  })),
)

const filteredAppointments = computed(() => {
  const dateRange = getDateRange(selectedDate.value, selectedPeriod.value)
  const doctorId = selectedDoctorId.value === 'all' ? null : Number(selectedDoctorId.value)

  return appointments.value.filter((appointment) => {
    const appointmentDate = new Date(appointment.appointmentDate)

    if (Number.isNaN(appointmentDate.getTime())) {
      return false
    }

    const matchesDate =
      appointmentDate >= dateRange.start && appointmentDate <= dateRange.end
    const matchesDoctor = doctorId === null || appointment.doctorId === doctorId

    return matchesDate && matchesDoctor
  })
})

const totalAppointments = computed(() => filteredAppointments.value.length)

const statusSummary = computed(() =>
  APPOINTMENT_STATUS_OPTIONS.map((status) => {
    const count = filteredAppointments.value.filter(
      (appointment) => appointment.status === status,
    ).length

    return {
      status,
      count,
      percent: totalAppointments.value ? Math.round((count / totalAppointments.value) * 100) : 0,
    }
  }),
)

const appointmentsByDoctor = computed(() => {
  const grouped = new Map<number, { doctorId: number; name: string; count: number }>()

  filteredAppointments.value.forEach((appointment) => {
    const current = grouped.get(appointment.doctorId)

    if (current) {
      current.count += 1
      return
    }

    grouped.set(appointment.doctorId, {
      doctorId: appointment.doctorId,
      name: getAppointmentDoctorName(appointment),
      count: 1,
    })
  })

  return Array.from(grouped.values()).sort((a, b) => b.count - a.count)
})

const maxDoctorCount = computed(() =>
  appointmentsByDoctor.value.reduce((max, item) => Math.max(max, item.count), 0),
)

const dateRangeLabel = computed(() => {
  const range = getDateRange(selectedDate.value, selectedPeriod.value)
  const formatter = new Intl.DateTimeFormat('pl-PL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })

  if (selectedPeriod.value === 'day') {
    return formatter.format(range.start)
  }

  return `${formatter.format(range.start)} - ${formatter.format(range.end)}`
})

onMounted(async () => {
  try {
    const [appointmentsData, doctorsData] = await Promise.all([
      getAppointments(),
      getDoctors(),
    ])

    appointments.value = appointmentsData
    doctors.value = doctorsData
  } catch (error) {
    console.error(error)
    errorMessage.value = 'Nie udało się pobrać danych do raportu wizyt.'
  } finally {
    isLoading.value = false
  }
})

function formatDateKey(date: Date): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')

  return `${year}-${month}-${day}`
}

function getAppointmentDoctorName(appointment: Appointment): string {
  const doctor = doctors.value.find((item) => item.id === appointment.doctorId)

  return appointment.doctorName || (doctor ? getDoctorFullName(doctor) : '') || `Lekarz #${appointment.doctorId}`
}

function getDateRange(dateKey: string, period: ReportPeriod): { start: Date; end: Date } {
  const baseDate = new Date(`${dateKey}T00:00:00`)
  const safeDate = Number.isNaN(baseDate.getTime()) ? new Date() : baseDate
  const start = new Date(safeDate)
  const end = new Date(safeDate)

  if (period === 'day') {
    start.setHours(0, 0, 0, 0)
    end.setHours(23, 59, 59, 999)
    return { start, end }
  }

  if (period === 'week') {
    const day = start.getDay()
    const mondayOffset = day === 0 ? -6 : 1 - day

    start.setDate(start.getDate() + mondayOffset)
    start.setHours(0, 0, 0, 0)
    end.setTime(start.getTime())
    end.setDate(start.getDate() + 6)
    end.setHours(23, 59, 59, 999)
    return { start, end }
  }

  start.setDate(1)
  start.setHours(0, 0, 0, 0)
  end.setMonth(start.getMonth() + 1, 0)
  end.setHours(23, 59, 59, 999)

  return { start, end }
}
</script>

<template>
  <PageLayout>
    <PageHeader
      eyebrow="Raporty wizyt"
      title="Analiza harmonogramu wizyt"
      description="Podsumowanie wizyt według wybranego okresu, statusu i lekarza."
    />

    <section class="reports-toolbar app-card">
      <label class="reports-field">
        <span>Okres</span>
        <select v-model="selectedPeriod" class="app-input">
          <option v-for="option in periodOptions" :key="option.value" :value="option.value">
            {{ option.label }}
          </option>
        </select>
      </label>

      <label class="reports-field">
        <span>Data</span>
        <input v-model="selectedDate" class="app-input" type="date" />
      </label>

      <label class="reports-field">
        <span>Lekarz</span>
        <select v-model="selectedDoctorId" class="app-input">
          <option value="all">Wszyscy lekarze</option>
          <option v-for="doctor in doctorOptions" :key="doctor.id" :value="doctor.id">
            {{ doctor.name }}
          </option>
        </select>
      </label>
    </section>

    <AppAlert v-if="errorMessage" type="error" :message="errorMessage" />

    <div v-if="isLoading" class="reports-loading app-card">
      <AppSpinner label="Ładowanie raportu wizyt..." />
    </div>

    <template v-else>
      <section class="reports-summary">
        <article class="report-card report-card--total">
          <span class="report-card__label">Łączna liczba wizyt</span>
          <strong>{{ totalAppointments }}</strong>
          <small>{{ dateRangeLabel }}</small>
        </article>

        <article
          v-for="item in statusSummary"
          :key="item.status"
          class="report-card"
        >
          <span class="report-card__label">{{ item.status }}</span>
          <strong>{{ item.count }}</strong>
          <div class="report-card__bar" aria-hidden="true">
            <span :style="{ width: `${item.percent}%` }" />
          </div>
        </article>
      </section>

      <section class="reports-grid">
        <article class="app-card reports-panel">
          <div class="reports-panel__header">
            <div>
              <h2>Wizyty według lekarzy</h2>
              <p>Porównanie liczby wizyt w wybranym okresie.</p>
            </div>
          </div>

          <div v-if="appointmentsByDoctor.length" class="doctor-report-list">
            <div
              v-for="doctor in appointmentsByDoctor"
              :key="doctor.doctorId"
              class="doctor-report-row"
            >
              <div class="doctor-report-row__meta">
                <strong>{{ doctor.name }}</strong>
                <span>{{ doctor.count }} wizyt</span>
              </div>
              <div class="doctor-report-row__bar" aria-hidden="true">
                <span
                  :style="{
                    width: `${maxDoctorCount ? (doctor.count / maxDoctorCount) * 100 : 0}%`,
                  }"
                />
              </div>
            </div>
          </div>

          <p v-else class="reports-empty">
            Brak wizyt spełniających wybrane kryteria.
          </p>
        </article>

        <article class="app-card reports-panel">
          <div class="reports-panel__header">
            <div>
              <h2>Struktura statusów</h2>
              <p>Ile wizyt jest zaplanowanych, zakończonych albo anulowanych.</p>
            </div>
          </div>

          <div class="status-table">
            <div v-for="item in statusSummary" :key="item.status" class="status-table__row">
              <span>{{ item.status }}</span>
              <strong>{{ item.count }}</strong>
              <small>{{ item.percent }}%</small>
            </div>
          </div>
        </article>
      </section>
    </template>
  </PageLayout>
</template>

<style scoped>
.reports-toolbar {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.reports-field {
  display: grid;
  gap: 0.4rem;
  color: var(--color-text-muted);
  font-size: 0.85rem;
  font-weight: 700;
}

.reports-field .app-input {
  min-height: 2.9rem;
}

.reports-loading {
  display: flex;
  justify-content: center;
  padding: 3rem;
}

.reports-summary {
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.report-card {
  min-height: 8.5rem;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 0.65rem;
  padding: 1rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: #fff;
  box-shadow: var(--shadow-sm);
}

.report-card--total {
  background: #0f172a;
  color: #fff;
}

.report-card__label {
  color: inherit;
  font-size: 0.85rem;
  font-weight: 750;
  opacity: 0.75;
}

.report-card strong {
  font-size: 2rem;
  line-height: 1;
}

.report-card small {
  color: inherit;
  opacity: 0.72;
}

.report-card__bar,
.doctor-report-row__bar {
  height: 0.45rem;
  overflow: hidden;
  border-radius: 999px;
  background: #e2e8f0;
}

.report-card__bar span,
.doctor-report-row__bar span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: var(--color-primary);
}

.reports-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(18rem, 0.65fr);
  gap: 1rem;
}

.reports-panel {
  display: grid;
  gap: 1rem;
}

.reports-panel__header h2 {
  margin: 0;
  font-size: 1.1rem;
}

.reports-panel__header p {
  margin: 0.35rem 0 0;
  color: var(--color-text-muted);
}

.doctor-report-list {
  display: grid;
  gap: 0.9rem;
}

.doctor-report-row {
  display: grid;
  gap: 0.45rem;
}

.doctor-report-row__meta,
.status-table__row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.doctor-report-row__meta span,
.status-table__row small {
  color: var(--color-text-muted);
  font-weight: 700;
}

.status-table {
  display: grid;
  gap: 0.65rem;
}

.status-table__row {
  min-height: 2.75rem;
  padding: 0.65rem 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  background: #f8fafc;
}

.status-table__row strong {
  min-width: 2rem;
  text-align: right;
}

.reports-empty {
  margin: 0;
  padding: 1.5rem;
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-md);
  color: var(--color-text-muted);
  text-align: center;
}

@media (max-width: 1100px) {
  .reports-summary {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }

  .reports-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 760px) {
  .reports-toolbar,
  .reports-summary {
    grid-template-columns: 1fr;
  }
}
</style>
