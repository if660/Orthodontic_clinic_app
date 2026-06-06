<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { getPatients } from '../../services/patientService'
import { getDoctors } from '../../services/doctorService'
import { getAppointments, getAvailableSlots, getVisitTypes } from '../../services/appointmentService'
import type { Patient } from '../../types/patient'
import type { Doctor } from '../../types/doctor'
import type { Appointment } from '../../types/appointment'
import { getPatientAge, getPatientCategoryLabel } from '../../types/patient'
import { toReadableDate } from '../../types/appointment'

type DoctorOperationalData = {
  doctorId: number
  freeSlotsToday: number
  nearestFreeSlot: string | null
}

type CalendarCell = {
  key: string
  isoDate: string | null
  dayNumber: number
  inCurrentMonth: boolean
  isToday: boolean
  visitCount: number
}

const patients = ref<Patient[]>([])
const doctors = ref<Doctor[]>([])
const appointments = ref<Appointment[]>([])
const doctorOperational = ref<DoctorOperationalData[]>([])

const loading = ref(false)
const error = ref('')

const selectedScheduleDoctorId = ref<number | 'all'>('all')
const selectedScheduleDate = ref(new Date().toISOString().slice(0, 10))
const initialCalendarMonth = new Date()
initialCalendarMonth.setDate(1)
initialCalendarMonth.setHours(0, 0, 0, 0)
const currentCalendarMonth = ref(initialCalendarMonth)
const calendarWeekdayLabels = ['Pn', 'Wt', 'Sr', 'Czw', 'Pt', 'Sob', 'Nd']

const toIsoDate = (value: Date) => {
  return `${value.getFullYear()}-${String(value.getMonth() + 1).padStart(2, '0')}-${String(value.getDate()).padStart(2, '0')}`
}

const formatLongDate = (value: Date) => {
  return new Intl.DateTimeFormat('pl-PL', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  }).format(value)
}

const pageDateLabel = computed(() => formatLongDate(new Date()))

const greeting = computed(() => {
  const hour = new Date().getHours()
  if (hour < 12) return 'Dzien dobry'
  if (hour < 18) return 'Dzien dobry'
  return 'Dobry wieczor'
})

const now = computed(() => new Date())
const todayIso = computed(() => new Date().toISOString().slice(0, 10))

const isCancelledAppointment = (appointment: Appointment) => {
  return appointment.status === 'Anulowana' || appointment.status.includes('Odwol')
}

const activeAppointments = computed(() =>
  appointments.value.filter((appointment) => !isCancelledAppointment(appointment)),
)

const todayAppointments = computed(() => {
  const day = todayIso.value
  return activeAppointments.value
    .filter((appointment) => appointment.appointmentDate.slice(0, 10) === day)
    .sort((a, b) => a.appointmentDate.localeCompare(b.appointmentDate))
})

const upcomingAppointments = computed(() => {
  return activeAppointments.value
    .filter((appointment) => new Date(appointment.appointmentDate) >= now.value)
    .sort((a, b) => a.appointmentDate.localeCompare(b.appointmentDate))
})

const nearestAppointment = computed(() => upcomingAppointments.value[0] ?? null)

const freeSlotsToday = computed(() => {
  return doctorOperational.value.reduce((sum, item) => sum + item.freeSlotsToday, 0)
})

const nearestFreeSlot = computed(() => {
  const nearest = doctorOperational.value
    .filter((entry) => entry.nearestFreeSlot)
    .map((entry) => ({
      doctorId: entry.doctorId,
      start: entry.nearestFreeSlot as string,
    }))
    .sort((a, b) => a.start.localeCompare(b.start))[0]

  if (!nearest) return null

  const doctor = doctors.value.find((item) => item.id === nearest.doctorId)
  const time = new Date(nearest.start).toLocaleTimeString('pl-PL', { hour: '2-digit', minute: '2-digit' })
  return {
    time,
    doctorName: doctor ? `${doctor.firstName} ${doctor.lastName}`.trim() : 'Lekarz',
  }
})

const unconfirmedTodayCount = computed(() => {
  return todayAppointments.value.filter((appointment) => appointment.status === 'Zaplanowana').length
})

const confirmedTodayCount = computed(() => {
  return todayAppointments.value.filter((appointment) => appointment.status === 'Potwierdzona').length
})

const patientsMissingContactCount = computed(() => {
  return patients.value.filter((patient) => !patient.phone?.trim() || !patient.email?.trim()).length
})

const patientsWithoutUpcomingCount = computed(() => {
  const patientIdsWithUpcoming = new Set(upcomingAppointments.value.map((appointment) => appointment.patientId))
  return patients.value.filter((patient) => !patientIdsWithUpcoming.has(patient.id)).length
})

const cancelledWithoutRescheduleCount = computed(() => {
  return upcomingAppointments.value.filter((appointment) => {
    if (appointment.status !== 'Odwolana') return false

    return !upcomingAppointments.value.some((nextAppointment) =>
      nextAppointment.patientId === appointment.patientId
      && nextAppointment.id !== appointment.id
      && new Date(nextAppointment.appointmentDate) > new Date(appointment.appointmentDate)
      && nextAppointment.status !== 'Odwolana')
  }).length
})

const contactNeededCount = computed(() => {
  return unconfirmedTodayCount.value + patientsWithoutUpcomingCount.value + cancelledWithoutRescheduleCount.value
})

const nextAppointments = computed(() => upcomingAppointments.value.slice(0, 6))

const latestPatients = computed(() => patients.value.slice(-3).reverse())

const scheduleAppointments = computed(() => {
  return activeAppointments.value
    .filter((appointment) => {
      const byDate = appointment.appointmentDate.slice(0, 10) === selectedScheduleDate.value
      const byDoctor = selectedScheduleDoctorId.value === 'all' || appointment.doctorId === selectedScheduleDoctorId.value
      return byDate && byDoctor
    })
    .sort((a, b) => a.appointmentDate.localeCompare(b.appointmentDate))
})

const calendarAppointments = computed(() => {
  if (selectedScheduleDoctorId.value === 'all') {
    return activeAppointments.value
  }

  return activeAppointments.value.filter((appointment) => appointment.doctorId === selectedScheduleDoctorId.value)
})

const calendarMonthLabel = computed(() => {
  return new Intl.DateTimeFormat('pl-PL', {
    month: 'long',
    year: 'numeric',
  }).format(currentCalendarMonth.value)
})

const calendarVisitCountByDate = computed(() => {
  const result: Record<string, number> = {}

  calendarAppointments.value.forEach((appointment) => {
    const date = appointment.appointmentDate.slice(0, 10)
    result[date] = (result[date] || 0) + 1
  })

  return result
})

const calendarCells = computed<CalendarCell[]>(() => {
  const year = currentCalendarMonth.value.getFullYear()
  const month = currentCalendarMonth.value.getMonth()

  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)
  const daysInMonth = lastDay.getDate()
  const leadingEmpty = (firstDay.getDay() + 6) % 7

  const cells: CalendarCell[] = []

  for (let i = 0; i < leadingEmpty; i += 1) {
    cells.push({
      key: `empty-${month}-${i}`,
      isoDate: null,
      dayNumber: 0,
      inCurrentMonth: false,
      isToday: false,
      visitCount: 0,
    })
  }

  const today = toIsoDate(new Date())

  for (let day = 1; day <= daysInMonth; day += 1) {
    const date = new Date(year, month, day)
    const isoDate = toIsoDate(date)

    cells.push({
      key: isoDate,
      isoDate,
      dayNumber: day,
      inCurrentMonth: true,
      isToday: isoDate === today,
      visitCount: calendarVisitCountByDate.value[isoDate] || 0,
    })
  }

  const trailingEmpty = (7 - (cells.length % 7)) % 7
  for (let i = 0; i < trailingEmpty; i += 1) {
    cells.push({
      key: `empty-tail-${month}-${i}`,
      isoDate: null,
      dayNumber: 0,
      inCurrentMonth: false,
      isToday: false,
      visitCount: 0,
    })
  }

  return cells
})

const changeCalendarMonth = (offset: number) => {
  const next = new Date(currentCalendarMonth.value)
  next.setMonth(next.getMonth() + offset)
  next.setDate(1)
  next.setHours(0, 0, 0, 0)

  currentCalendarMonth.value = next
  selectedScheduleDate.value = toIsoDate(next)
}

const selectCalendarDate = (isoDate: string | null) => {
  if (!isoDate) return
  selectedScheduleDate.value = isoDate
}

watch(selectedScheduleDate, (value) => {
  if (!value) return

  const date = new Date(`${value}T00:00:00`)
  if (Number.isNaN(date.getTime())) return

  const monthStart = new Date(date)
  monthStart.setDate(1)
  monthStart.setHours(0, 0, 0, 0)
  currentCalendarMonth.value = monthStart
})

const weeklyStats = computed(() => {
  const current = new Date()
  const day = current.getDay() || 7
  const weekStart = new Date(current)
  weekStart.setDate(current.getDate() - day + 1)
  weekStart.setHours(0, 0, 0, 0)

  const weekEnd = new Date(weekStart)
  weekEnd.setDate(weekStart.getDate() + 7)

  const weekAppointments = activeAppointments.value.filter((appointment) => {
    const date = new Date(appointment.appointmentDate)
    return date >= weekStart && date < weekEnd
  })

  return {
    total: weekAppointments.length,
    confirmed: weekAppointments.filter((item) => item.status === 'Potwierdzona').length,
    cancelled: weekAppointments.filter((item) => item.status === 'Odwolana').length,
    noShow: weekAppointments.filter((item) => item.status === 'Nieobecnosc').length,
    newPatients: latestPatients.value.length,
  }
})

const maxWeeklyValue = computed(() => {
  return Math.max(
    weeklyStats.value.total,
    weeklyStats.value.confirmed,
    weeklyStats.value.cancelled,
    weeklyStats.value.noShow,
    weeklyStats.value.newPatients,
    1,
  )
})

const doctorTodaySummary = computed(() => {
  return doctors.value.map((doctor) => {
    const visits = todayAppointments.value.filter((appointment) => appointment.doctorId === doctor.id).length
    const operational = doctorOperational.value.find((item) => item.doctorId === doctor.id)

    return {
      ...doctor,
      visits,
      freeSlots: operational?.freeSlotsToday ?? 0,
    }
  })
})

const specializations = computed(() => {
  const result: Record<string, number> = {}

  doctors.value.forEach((doctor) => {
    const specialization = doctor.specialization || 'Brak specjalizacji'
    result[specialization] = (result[specialization] || 0) + 1
  })

  return result
})

const statusClass = (status: string) => {
  const normalized = status.toLowerCase()

  if (normalized === 'potwierdzona' || normalized === 'pacjent przyszedl' || normalized === 'zakonczona') {
    return 'status-chip--success'
  }

  if (normalized === 'odwolana' || normalized === 'nieobecnosc') {
    return 'status-chip--danger'
  }

  if (normalized === 'przelozona') {
    return 'status-chip--warning'
  }

  return 'status-chip--default'
}

const getInitials = (patient: Patient) => {
  const first = patient.firstName?.[0] ?? ''
  const last = patient.lastName?.[0] ?? ''
  return `${first}${last}`.toUpperCase()
}

const getProgressWidth = (value: number) => {
  const percent = Math.round((value / maxWeeklyValue.value) * 100)
  return `${percent}%`
}

const loadDoctorOperationalData = async (doctorData: Doctor[]) => {
  try {
    const visitTypes = await getVisitTypes()
    const selectedVisitType = visitTypes[0]?.name ?? 'Kontrola aparatu'

    const slotSets = await Promise.all(
      doctorData.map(async (doctor) => {
        const slots = await getAvailableSlots(doctor.id, todayIso.value, selectedVisitType)

        return {
          doctorId: doctor.id,
          freeSlotsToday: slots.length,
          nearestFreeSlot: slots[0]?.start ?? null,
        }
      }),
    )

    doctorOperational.value = slotSets
  } catch {
    doctorOperational.value = doctorData.map((doctor) => ({
      doctorId: doctor.id,
      freeSlotsToday: 0,
      nearestFreeSlot: null,
    }))
  }
}

const loadDashboardData = async () => {
  loading.value = true
  error.value = ''

  try {
    const [patientsData, doctorsData, appointmentsData] = await Promise.all([
      getPatients(),
      getDoctors(),
      getAppointments(),
    ])

    patients.value = patientsData
    doctors.value = doctorsData
    appointments.value = appointmentsData
    await loadDoctorOperationalData(doctorsData)
  } catch {
    error.value = 'Nie udalo sie pobrac danych do panelu glownego.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadDashboardData()
})
</script>

<template>
  <main class="app-page dashboard-page">
    <header class="hero-card">
      <div>
        <h1 class="hero-title">{{ greeting }} 👋</h1>
        <p class="hero-subtitle">Oto podsumowanie pracy kliniki na dzis.</p>
        <p class="hero-date">{{ pageDateLabel }}</p>
      </div>

      <div class="hero-actions">
        <RouterLink to="/appointments" class="app-btn app-btn--primary">
          + Umow wizyte
        </RouterLink>
        <RouterLink to="/patients/add" class="app-btn app-btn--secondary">
          + Dodaj pacjenta
        </RouterLink>
      </div>
    </header>

    <p class="day-state">
      Dzisiaj: {{ doctors.length }} lekarzy dostepnych · {{ todayAppointments.length }} wizyt
      <span v-if="nearestAppointment"> · najblizsza: {{ toReadableDate(nearestAppointment.appointmentDate) }}</span>
    </p>

    <p v-if="error" class="message message--error">{{ error }}</p>

    <section v-if="loading" class="app-card app-card--flat">
      <p>Ładowanie danych dashboardu...</p>
    </section>

    <template v-else>
      <section class="kpi-grid app-section">
        <article class="kpi-card">
          <div class="kpi-card__header">
            <span class="kpi-icon">🗓</span>
            <span class="kpi-label">Dzisiejsze wizyty</span>
          </div>
          <strong class="kpi-value">{{ todayAppointments.length }}</strong>
          <p class="kpi-text">
            <span v-if="todayAppointments.length > 0">{{ confirmedTodayCount }} potwierdzone · {{ unconfirmedTodayCount }} oczekuje</span>
            <span v-else>Brak zaplanowanych wizyt na dzis</span>
          </p>
        </article>

        <article class="kpi-card">
          <div class="kpi-card__header">
            <span class="kpi-icon">⏰</span>
            <span class="kpi-label">Najblizsza wizyta</span>
          </div>
          <strong class="kpi-value kpi-value--small">
            {{ nearestAppointment ? toReadableDate(nearestAppointment.appointmentDate) : '—' }}
          </strong>
          <p class="kpi-text">
            {{ nearestAppointment ? `${nearestAppointment.patientName} · ${nearestAppointment.doctorName}` : 'Brak nadchodzacych wizyt' }}
          </p>
        </article>

        <RouterLink to="/appointments" class="kpi-card kpi-card--link">
          <div class="kpi-card__header">
            <span class="kpi-icon">🟢</span>
            <span class="kpi-label">Wolne sloty dzis</span>
          </div>
          <strong class="kpi-value">{{ freeSlotsToday }}</strong>
          <p class="kpi-text">
            <span v-if="nearestFreeSlot">Najblizszy: {{ nearestFreeSlot.time }} u {{ nearestFreeSlot.doctorName }}</span>
            <span v-else>Brak wolnych slotow dzisiaj</span>
          </p>
        </RouterLink>

        <RouterLink to="/patients" class="kpi-card kpi-card--link">
          <div class="kpi-card__header">
            <span class="kpi-icon">⚠️</span>
            <span class="kpi-label">Do kontaktu</span>
          </div>
          <strong class="kpi-value">{{ contactNeededCount }}</strong>
          <p class="kpi-text">
            {{ contactNeededCount > 0 ? 'Niepotwierdzone wizyty i zalegle sprawy' : 'Wszystko gotowe' }}
          </p>
        </RouterLink>
      </section>

      <section class="main-grid app-section">
        <article class="app-card schedule-card">
          <div class="section-header">
            <h2 class="section-title">Dzisiejszy harmonogram</h2>

            <div class="section-controls">
              <select v-model="selectedScheduleDoctorId" class="section-select">
                <option value="all">Wszyscy lekarze</option>
                <option v-for="doctor in doctors" :key="doctor.id" :value="doctor.id">
                  {{ doctor.firstName }} {{ doctor.lastName }}
                </option>
              </select>

              <input v-model="selectedScheduleDate" type="date" class="section-date" />
            </div>
          </div>

          <div class="dashboard-calendar">
            <div class="dashboard-calendar__toolbar">
              <button type="button" class="calendar-nav-btn" @click="changeCalendarMonth(-1)">
                ←
              </button>
              <strong class="dashboard-calendar__month">{{ calendarMonthLabel }}</strong>
              <button type="button" class="calendar-nav-btn" @click="changeCalendarMonth(1)">
                →
              </button>
            </div>

            <div class="dashboard-calendar__weekdays">
              <span v-for="day in calendarWeekdayLabels" :key="day">{{ day }}</span>
            </div>

            <div class="dashboard-calendar__grid">
              <button
                v-for="cell in calendarCells"
                :key="cell.key"
                type="button"
                class="calendar-cell"
                :class="{
                  'calendar-cell--empty': !cell.inCurrentMonth,
                  'calendar-cell--selected': cell.isoDate === selectedScheduleDate,
                  'calendar-cell--today': cell.isToday,
                  'calendar-cell--has-visits': cell.visitCount > 0,
                }"
                :disabled="!cell.inCurrentMonth"
                @click="selectCalendarDate(cell.isoDate)"
              >
                <span v-if="cell.inCurrentMonth" class="calendar-cell__day">{{ cell.dayNumber }}</span>
                <span v-if="cell.visitCount > 0" class="calendar-cell__count">{{ cell.visitCount }}</span>
              </button>
            </div>
          </div>

          <ul v-if="scheduleAppointments.length > 0" class="timeline-list">
            <li v-for="appointment in scheduleAppointments" :key="appointment.id" class="timeline-item">
              <strong class="timeline-time">
                {{ new Date(appointment.appointmentDate).toLocaleTimeString('pl-PL', { hour: '2-digit', minute: '2-digit' }) }}
              </strong>
              <div class="timeline-main">
                <p class="timeline-patient">{{ appointment.patientName }}</p>
                <p class="timeline-meta">{{ appointment.visitType }} · {{ appointment.doctorName }}</p>
              </div>
              <span class="status-chip" :class="statusClass(appointment.status)">{{ appointment.status }}</span>
            </li>
          </ul>

          <div v-else class="empty-state">
            <p class="empty-state__title">Brak wizyt dla wybranego dnia</p>
            <p class="empty-state__text">Mozesz umowic pierwsza wizyte lub przejsc do kalendarza.</p>
            <div class="empty-state__actions">
              <RouterLink to="/appointments" class="app-btn app-btn--primary">+ Umow wizyte</RouterLink>
              <RouterLink to="/appointments" class="app-btn app-btn--secondary">Otworz kalendarz</RouterLink>
            </div>
          </div>
        </article>

        <article class="app-card tasks-card">
          <h2 class="section-title">Zadania recepcji</h2>

          <ul v-if="contactNeededCount > 0 || patientsMissingContactCount > 0" class="task-list">
            <li>
              <RouterLink to="/appointments">{{ unconfirmedTodayCount }} wizyt wymaga potwierdzenia</RouterLink>
            </li>
            <li>
              <RouterLink to="/patients">{{ patientsWithoutUpcomingCount }} pacjentow bez kolejnej wizyty</RouterLink>
            </li>
            <li>
              <RouterLink to="/patients">{{ patientsMissingContactCount }} pacjentow bez pelnych danych kontaktowych</RouterLink>
            </li>
            <li>
              <RouterLink to="/appointments">{{ cancelledWithoutRescheduleCount }} odwolanych wizyt bez nowego terminu</RouterLink>
            </li>
          </ul>

          <div v-else class="empty-state empty-state--compact">
            <p class="empty-state__title">✅ Brak pilnych zadan</p>
            <p class="empty-state__text">Wszystkie najwazniejsze sprawy sa uporzadkowane.</p>
            <RouterLink to="/patients" class="app-btn app-btn--secondary">Sprawdz pacjentow</RouterLink>
          </div>
        </article>
      </section>

      <section class="two-col-grid app-section">
        <article class="app-card">
          <h2 class="section-title">Najblizsze wizyty</h2>

          <p v-if="nextAppointments.length === 0" class="empty-text">Brak nadchodzacych wizyt.</p>

          <ul v-else class="appointment-list">
            <li v-for="appointment in nextAppointments" :key="appointment.id">
              <strong>{{ toReadableDate(appointment.appointmentDate) }}</strong>
              <span>{{ appointment.patientName }}</span>
              <small>{{ appointment.visitType }} · {{ appointment.doctorName }}</small>
              <small><span class="status-chip" :class="statusClass(appointment.status)">{{ appointment.status }}</span></small>
            </li>
          </ul>
        </article>

        <article class="app-card">
          <h2 class="section-title">Lekarze dzisiaj</h2>

          <ul class="doctor-list">
            <li v-for="doctor in doctorTodaySummary" :key="doctor.id">
              <strong>{{ doctor.firstName }} {{ doctor.lastName }}</strong>
              <small>{{ doctor.availabilityStart || '08:00' }}-{{ doctor.availabilityEnd || '16:00' }} · {{ doctor.visits }} wizyt · {{ doctor.freeSlots }} wolnych slotow</small>
            </li>
          </ul>
        </article>
      </section>

      <section class="three-col-grid app-section">
        <article class="app-card">
          <h2 class="section-title">Ostatnio dodani pacjenci</h2>

          <ul v-if="latestPatients.length > 0" class="patient-rich-list">
            <li v-for="patient in latestPatients" :key="patient.id">
              <div class="patient-avatar">{{ getInitials(patient) }}</div>
              <div class="patient-main">
                <strong>{{ patient.firstName }} {{ patient.lastName }}</strong>
                <small>{{ getPatientCategoryLabel(patient) }} · {{ getPatientAge(patient.birthDate) ?? '—' }} lat</small>
                <small>{{ patient.phone || patient.email || 'Brak danych kontaktowych' }}</small>
              </div>
              <RouterLink :to="`/patients/${patient.id}`" class="mini-link">Zobacz</RouterLink>
            </li>
          </ul>
          <p v-else class="empty-text">Brak pacjentow do wyswietlenia.</p>
        </article>

        <article class="app-card">
          <h2 class="section-title">Statystyki tygodnia</h2>

          <ul class="weekly-list">
            <li>
              <div><span>Wizyty</span><strong>{{ weeklyStats.total }}</strong></div>
              <div class="progress-track"><span class="progress-fill" :style="{ width: getProgressWidth(weeklyStats.total) }" /></div>
            </li>
            <li>
              <div><span>Potwierdzone</span><strong>{{ weeklyStats.confirmed }}</strong></div>
              <div class="progress-track"><span class="progress-fill progress-fill--success" :style="{ width: getProgressWidth(weeklyStats.confirmed) }" /></div>
            </li>
            <li>
              <div><span>Odwolane</span><strong>{{ weeklyStats.cancelled }}</strong></div>
              <div class="progress-track"><span class="progress-fill progress-fill--danger" :style="{ width: getProgressWidth(weeklyStats.cancelled) }" /></div>
            </li>
            <li>
              <div><span>Nieobecnosci</span><strong>{{ weeklyStats.noShow }}</strong></div>
              <div class="progress-track"><span class="progress-fill progress-fill--warning" :style="{ width: getProgressWidth(weeklyStats.noShow) }" /></div>
            </li>
          </ul>
        </article>

        <article class="app-card">
          <h2 class="section-title">Szybkie akcje</h2>

          <div class="quick-grid">
            <RouterLink to="/patients/add" class="quick-tile">
              <span class="quick-tile__icon">+</span>
              <span>Dodaj pacjenta</span>
            </RouterLink>
            <RouterLink to="/appointments" class="quick-tile quick-tile--primary">
              <span class="quick-tile__icon">🗓</span>
              <span>Umow wizyte</span>
            </RouterLink>
            <RouterLink to="/doctors/add" class="quick-tile">
              <span class="quick-tile__icon">👨</span>
              <span>Dodaj lekarza</span>
            </RouterLink>
            <RouterLink to="/appointments" class="quick-tile">
              <span class="quick-tile__icon">📅</span>
              <span>Otworz kalendarz</span>
            </RouterLink>
          </div>
        </article>
      </section>

      <section class="two-col-grid app-section">
        <article class="app-card app-card--flat">
          <h2 class="section-title">Klinika</h2>
          <div class="stats-pills">
            <span>Pacjenci: {{ patients.length }}</span>
            <span>Lekarze: {{ doctors.length }}</span>
            <span>Specjalizacje: {{ Object.keys(specializations).length }}</span>
          </div>
        </article>
      </section>
    </template>
  </main>
</template>

<style scoped>
.dashboard-page {
  max-width: 1280px;
}

.hero-card {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: flex-start;
  flex-wrap: wrap;
  padding: 1.35rem 1.4rem;
  border: 1px solid #dbeafe;
  border-radius: var(--radius-lg);
  background: linear-gradient(135deg, #eff6ff 0%, #ffffff 100%);
}

.hero-title {
  margin: 0;
  font-size: clamp(1.5rem, 3vw, 2rem);
  letter-spacing: -0.02em;
}

.hero-subtitle {
  margin: 0.35rem 0 0;
  color: var(--color-text-muted);
}

.hero-date {
  margin: 0.3rem 0 0;
  color: var(--color-text);
  font-weight: 600;
}

.hero-actions {
  display: flex;
  gap: 0.6rem;
  flex-wrap: wrap;
}

.day-state {
  margin: 0.85rem 0 0;
  padding: 0.65rem 0.85rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border);
  background: #f8fbff;
  color: var(--color-text-muted);
  font-size: 0.92rem;
}

.kpi-grid {
  display: grid;
  grid-template-columns: repeat(1, minmax(0, 1fr));
  gap: 0.85rem;
}

@media (min-width: 760px) {
  .kpi-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (min-width: 1120px) {
  .kpi-grid {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }
}

.kpi-card {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: #fff;
  padding: 1rem;
  display: grid;
  gap: 0.45rem;
  box-shadow: var(--shadow-sm);
}

.kpi-card--link {
  text-decoration: none;
  color: inherit;
}

.kpi-card--link:hover {
  border-color: var(--color-primary);
}

.kpi-card__header {
  display: flex;
  align-items: center;
  gap: 0.4rem;
}

.kpi-icon {
  width: 1.6rem;
  height: 1.6rem;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 0.9rem;
  background: #eff6ff;
}

.kpi-label {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--color-text-muted);
}

.kpi-value {
  font-size: clamp(1.85rem, 3vw, 2.3rem);
  line-height: 1;
}

.kpi-value--small {
  font-size: 1.15rem;
  line-height: 1.35;
}

.kpi-text {
  margin: 0;
  font-size: 0.83rem;
  color: var(--color-text-muted);
}

.main-grid {
  display: grid;
  gap: 1rem;
}

@media (min-width: 1080px) {
  .main-grid {
    grid-template-columns: 2fr 1fr;
  }
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.6rem;
  margin-bottom: 0.8rem;
  flex-wrap: wrap;
}

.section-title {
  margin: 0;
  font-size: 1.05rem;
}

.section-controls {
  display: flex;
  align-items: center;
  gap: 0.45rem;
  flex-wrap: wrap;
}

.section-select,
.section-date {
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-sm);
  padding: 0.45rem 0.55rem;
  font: inherit;
  background: #fff;
}

.dashboard-calendar {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: #fff;
  padding: 0.7rem;
  margin-bottom: 0.8rem;
}

.dashboard-calendar__toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  margin-bottom: 0.55rem;
}

.dashboard-calendar__month {
  font-size: 0.92rem;
  text-transform: capitalize;
}

.calendar-nav-btn {
  border: 1px solid var(--color-border-strong);
  background: #fff;
  border-radius: var(--radius-sm);
  width: 2rem;
  height: 2rem;
  font-weight: 700;
  cursor: pointer;
}

.calendar-nav-btn:hover {
  border-color: var(--color-primary);
}

.dashboard-calendar__weekdays {
  display: grid;
  grid-template-columns: repeat(7, minmax(0, 1fr));
  gap: 0.35rem;
  margin-bottom: 0.35rem;
}

.dashboard-calendar__weekdays span {
  font-size: 0.76rem;
  color: var(--color-text-muted);
  text-align: center;
  font-weight: 700;
}

.dashboard-calendar__grid {
  display: grid;
  grid-template-columns: repeat(7, minmax(0, 1fr));
  gap: 0.35rem;
}

.calendar-cell {
  border: 1px solid var(--color-border);
  background: #fff;
  border-radius: var(--radius-sm);
  min-height: 2.45rem;
  padding: 0.22rem;
  display: grid;
  justify-items: end;
  align-content: space-between;
  cursor: pointer;
}

.calendar-cell--empty {
  border-style: dashed;
  border-color: #e2e8f0;
  background: #f8fafc;
  cursor: default;
}

.calendar-cell--selected {
  border-color: var(--color-primary);
  background: #eff6ff;
}

.calendar-cell--today {
  box-shadow: inset 0 0 0 1px #93c5fd;
}

.calendar-cell--has-visits .calendar-cell__day {
  color: #1e3a8a;
  font-weight: 800;
}

.calendar-cell__day {
  font-size: 0.78rem;
}

.calendar-cell__count {
  border-radius: 999px;
  padding: 0.05rem 0.35rem;
  font-size: 0.68rem;
  font-weight: 700;
  background: #dbeafe;
  color: #1d4ed8;
}

.timeline-list,
.task-list,
.appointment-list,
.doctor-list,
.weekly-list,
.patient-rich-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: grid;
  gap: 0.65rem;
}

.timeline-item {
  display: grid;
  grid-template-columns: auto 1fr auto;
  align-items: center;
  gap: 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: 0.7rem;
  background: #f8fafc;
}

.timeline-time {
  min-width: 3.4rem;
}

.timeline-patient {
  margin: 0;
  font-weight: 700;
}

.timeline-meta {
  margin: 0.15rem 0 0;
  color: var(--color-text-muted);
  font-size: 0.85rem;
}

.task-list li a {
  display: block;
  padding: 0.7rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  text-decoration: none;
  color: var(--color-text);
  background: #fff;
}

.task-list li a:hover {
  border-color: var(--color-primary);
}

.empty-state {
  border: 1px dashed var(--color-border-strong);
  border-radius: var(--radius-md);
  background: #f8fbff;
  padding: 1rem;
}

.empty-state--compact {
  padding: 0.85rem;
}

.empty-state__title {
  margin: 0;
  font-weight: 700;
}

.empty-state__text {
  margin: 0.35rem 0 0;
  color: var(--color-text-muted);
}

.empty-state__actions {
  margin-top: 0.75rem;
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.status-chip {
  display: inline-flex;
  border-radius: 999px;
  padding: 0.1rem 0.55rem;
  font-size: 0.74rem;
  font-weight: 700;
  border: 1px solid transparent;
}

.status-chip--default {
  background: #e2e8f0;
  color: #1e293b;
  border-color: #cbd5e1;
}

.status-chip--success {
  background: #dcfce7;
  color: #166534;
  border-color: #86efac;
}

.status-chip--warning {
  background: #fef3c7;
  color: #92400e;
  border-color: #fcd34d;
}

.status-chip--danger {
  background: #fee2e2;
  color: #991b1b;
  border-color: #fca5a5;
}

.appointment-list li,
.doctor-list li {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: 0.7rem;
  background: #f8fafc;
  display: grid;
  gap: 0.2rem;
}

.appointment-list span,
.appointment-list small,
.doctor-list small {
  color: var(--color-text-muted);
}

.two-col-grid,
.three-col-grid {
  display: grid;
  gap: 1rem;
}

@media (min-width: 1020px) {
  .two-col-grid {
    grid-template-columns: 1fr 1fr;
  }
}

@media (min-width: 1200px) {
  .three-col-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

.patient-rich-list li {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: #f8fafc;
  padding: 0.65rem;
  display: grid;
  grid-template-columns: auto 1fr auto;
  align-items: center;
  gap: 0.65rem;
}

.patient-avatar {
  width: 2rem;
  height: 2rem;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 0.73rem;
  font-weight: 800;
  color: var(--color-primary);
  background: #dbeafe;
}

.patient-main {
  display: grid;
  gap: 0.1rem;
}

.patient-main small {
  color: var(--color-text-muted);
}

.mini-link {
  color: var(--color-primary);
  font-size: 0.82rem;
  text-decoration: none;
  font-weight: 700;
}

.weekly-list li {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: 0.65rem;
  display: grid;
  gap: 0.35rem;
  background: #fff;
}

.weekly-list li > div:first-child {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.progress-track {
  height: 0.45rem;
  border-radius: 999px;
  background: #e2e8f0;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  display: block;
  background: var(--color-primary);
}

.progress-fill--success {
  background: #16a34a;
}

.progress-fill--warning {
  background: #f59e0b;
}

.progress-fill--danger {
  background: #dc2626;
}

.quick-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.6rem;
}

.quick-tile {
  text-decoration: none;
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-md);
  padding: 0.65rem;
  color: var(--color-text);
  display: grid;
  gap: 0.3rem;
  justify-items: start;
  background: #fff;
  font-weight: 700;
}

.quick-tile:hover {
  border-color: var(--color-primary);
}

.quick-tile--primary {
  background: #eff6ff;
  border-color: #bfdbfe;
}

.quick-tile__icon {
  font-size: 1.05rem;
}

.stats-pills {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.stats-pills span {
  border-radius: 999px;
  padding: 0.35rem 0.65rem;
  background: #eff6ff;
  color: #1e3a8a;
  font-size: 0.84rem;
  font-weight: 700;
}

.empty-text {
  color: var(--color-text-muted);
}

.message {
  margin-top: 0.85rem;
  margin-bottom: 0;
  padding: 0.875rem 1rem;
  border-radius: var(--radius-md);
  font-weight: 500;
}

.message--error {
  background: rgba(220, 38, 38, 0.12);
  color: var(--color-danger);
}
</style>
