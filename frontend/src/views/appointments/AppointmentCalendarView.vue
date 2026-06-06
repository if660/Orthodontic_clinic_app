<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import PageLayout from '../../components/common/PageLayout.vue'
import PageHeader from '../../components/common/PageHeader.vue'
import AppSpinner from '../../components/common/AppSpinner.vue'
import AppAlert from '../../components/common/AppAlert.vue'
import AppIcon from '../../components/common/AppIcon.vue'
import type { Appointment, AppointmentStatus } from '../../types/appointment'
import {
  APPOINTMENT_STATUS_OPTIONS,
  formatAppointmentDate,
  normalizeAppointment,
} from '../../types/appointment'
import type { Doctor } from '../../types/doctor'
import { getDoctors } from '../../services/doctorService'
import { getAppointments, deleteAppointment, updateAppointmentStatus } from '../../services/appointmentService'
import { getApiErrorMessage } from '../../utils/apiError'

type ViewMode = 'month' | 'week'

interface CalendarCell {
  isoDate: string
  dayNumber: number
  inMonth: boolean
  isToday: boolean
  isWeekend: boolean
}

interface WeekDay {
  isoDate: string
  weekdayLabel: string
  dayNum: number
  isToday: boolean
}

const DOCTOR_PALETTE = ['#6366f1', '#0ea5e9', '#10b981', '#f59e0b', '#ec4899', '#8b5cf6', '#ef4444', '#14b8a6']
const SLOT_HEIGHT = 64
const DAY_START = 8
const DAY_END = 20

const router = useRouter()

const appointments = ref<Appointment[]>([])
const doctors = ref<Doctor[]>([])
const loading = ref(true)
const error = ref('')
const flashMessage = ref('')

const viewMode = ref<ViewMode>('month')
const currentDate = ref(new Date())
const selectedDate = ref(toIsoDate(new Date()))
const filterDoctorId = ref<number | 'all'>('all')

const popupAppt = ref<Appointment | null>(null)
const popupDraftStatus = ref<AppointmentStatus>('Zaplanowana')
const popupUpdatingStatus = ref(false)
const popupDeleteConfirm = ref(false)
const popupDeleting = ref(false)

function toIsoDate(d: Date): string {
  return d.toISOString().slice(0, 10)
}

function formatTime(iso: string): string {
  if (!iso) return ''
  const d = new Date(iso)
  return d.toLocaleTimeString('pl-PL', { hour: '2-digit', minute: '2-digit' })
}

const todayIso = computed(() => toIsoDate(new Date()))

const isCancelledAppointment = (appointment: Appointment) => {
  return appointment.status === 'Anulowana' || appointment.status.includes('Odwol')
}

const activeAppointments = computed(() =>
  appointments.value.filter((appointment) => !isCancelledAppointment(appointment)),
)

const filteredAppointments = computed(() => {
  if (filterDoctorId.value === 'all') return activeAppointments.value
  return activeAppointments.value.filter((a) => a.doctorId === filterDoctorId.value)
})

const doctorColorMap = computed(() => {
  const map: Record<number, string> = {}
  doctors.value.forEach((d, i) => {
    map[d.id] = DOCTOR_PALETTE[i % DOCTOR_PALETTE.length] ?? '#2563eb'
  })
  return map
})

const getDoctorColor = (doctorId: number): string =>
  doctorColorMap.value[doctorId] ?? '#94a3b8'

const getDoctorName = (doctorId: number): string => {
  const d = doctors.value.find((doc) => doc.id === doctorId)
  return d ? `${d.firstName} ${d.lastName}` : ''
}

const appointmentsByDate = computed(() => {
  const map: Record<string, Appointment[]> = {}
  filteredAppointments.value.forEach((a) => {
    const date = a.appointmentDate.slice(0, 10)
    if (!map[date]) map[date] = []
    map[date].push(a)
  })
  return map
})

const getCellAppointments = (isoDate: string): Appointment[] => {
  return (appointmentsByDate.value[isoDate] ?? []).sort((a, b) =>
    a.appointmentDate.localeCompare(b.appointmentDate),
  )
}

// ---- Month view ----
const weekdayLabels = ['Pn', 'Wt', 'Sr', 'Czw', 'Pt', 'Sob', 'Nd']

const monthFirstDay = computed(() => {
  const d = new Date(currentDate.value)
  d.setDate(1)
  d.setHours(0, 0, 0, 0)
  return d
})

const calendarCells = computed<CalendarCell[]>(() => {
  const year = monthFirstDay.value.getFullYear()
  const month = monthFirstDay.value.getMonth()
  const firstDay = new Date(year, month, 1)
  const daysInMonth = new Date(year, month + 1, 0).getDate()
  const leadingEmpty = (firstDay.getDay() + 6) % 7
  const today = todayIso.value
  const cells: CalendarCell[] = []

  for (let i = leadingEmpty - 1; i >= 0; i--) {
    const d = new Date(year, month, -i)
    const dayOfWeek = d.getDay()
    cells.push({
      isoDate: toIsoDate(d),
      dayNumber: d.getDate(),
      inMonth: false,
      isToday: false,
      isWeekend: dayOfWeek === 0 || dayOfWeek === 6,
    })
  }

  for (let day = 1; day <= daysInMonth; day++) {
    const isoDate = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`
    const dayOfWeek = new Date(year, month, day).getDay()
    cells.push({
      isoDate,
      dayNumber: day,
      inMonth: true,
      isToday: isoDate === today,
      isWeekend: dayOfWeek === 0 || dayOfWeek === 6,
    })
  }

  const trailing = (42 - cells.length) % 7 === 0 ? 0 : 7 - (cells.length % 7)
  for (let i = 1; i <= trailing; i++) {
    const d = new Date(year, month + 1, i)
    const dayOfWeek = d.getDay()
    cells.push({
      isoDate: toIsoDate(d),
      dayNumber: i,
      inMonth: false,
      isToday: false,
      isWeekend: dayOfWeek === 0 || dayOfWeek === 6,
    })
  }

  return cells
})

// ---- Week view ----
const weekDays = computed<WeekDay[]>(() => {
  const d = new Date(currentDate.value)
  const day = d.getDay() || 7
  d.setDate(d.getDate() - day + 1)
  d.setHours(0, 0, 0, 0)
  const today = todayIso.value
  const result: WeekDay[] = []
  for (let i = 0; i < 7; i++) {
    const date = new Date(d)
    date.setDate(d.getDate() + i)
    result.push({
      isoDate: toIsoDate(date),
      weekdayLabel: date.toLocaleDateString('pl-PL', { weekday: 'short' }),
      dayNum: date.getDate(),
      isToday: toIsoDate(date) === today,
    })
  }
  return result
})

const timeSlotHours = computed(() => {
  const hours: number[] = []
  for (let h = DAY_START; h < DAY_END; h++) hours.push(h)
  return hours
})

const getWeekDayAppointments = (isoDate: string): Appointment[] => {
  return getCellAppointments(isoDate).filter((a) => {
    const h = new Date(a.appointmentDate).getHours()
    return h >= DAY_START && h < DAY_END
  })
}

const getWeekApptStyle = (appt: Appointment) => {
  const d = new Date(appt.appointmentDate)
  const top = (d.getHours() - DAY_START + d.getMinutes() / 60) * SLOT_HEIGHT
  return {
    top: `${top}px`,
    height: `${SLOT_HEIGHT - 4}px`,
    borderLeftColor: getDoctorColor(appt.doctorId),
  }
}

// ---- Navigation ----
const headerLabel = computed(() => {
  if (viewMode.value === 'month') {
    return new Intl.DateTimeFormat('pl-PL', { month: 'long', year: 'numeric' }).format(currentDate.value)
  }
  const first = weekDays.value[0]
  const last = weekDays.value[6]
  if (!first || !last) return ''
  const d1 = new Date(`${first.isoDate}T00:00:00`)
  const d2 = new Date(`${last.isoDate}T00:00:00`)
  const fmt1 = d1.toLocaleDateString('pl-PL', { day: 'numeric', month: 'short' })
  const fmt2 = d2.toLocaleDateString('pl-PL', { day: 'numeric', month: 'short', year: 'numeric' })
  return `${fmt1} - ${fmt2}`
})

const navigate = (direction: number) => {
  const d = new Date(currentDate.value)
  if (viewMode.value === 'month') {
    d.setMonth(d.getMonth() + direction)
  } else {
    d.setDate(d.getDate() + direction * 7)
  }
  currentDate.value = d
}

const goToday = () => {
  currentDate.value = new Date()
  selectedDate.value = toIsoDate(new Date())
}

const selectDay = (isoDate: string) => {
  selectedDate.value = isoDate
  if (viewMode.value === 'week') return
  const clicked = new Date(`${isoDate}T00:00:00`)
  const cell = calendarCells.value.find((c) => c.isoDate === isoDate)
  if (cell && !cell.inMonth) {
    currentDate.value = clicked
  }
}

const quickAddForDate = (isoDate: string) => {
  router.push({ name: 'appointment-add', query: { date: isoDate } })
}

const quickAddForDateTime = (isoDate: string, hour: number) => {
  const time = `${String(hour).padStart(2, '0')}:00`
  router.push({ name: 'appointment-add', query: { date: isoDate, time } })
}

// ---- Side panel ----
const selectedDayAppointments = computed(() =>
  getCellAppointments(selectedDate.value),
)

const selectedDayLabel = computed(() => {
  if (!selectedDate.value) return ''
  const d = new Date(`${selectedDate.value}T00:00:00`)
  return d.toLocaleDateString('pl-PL', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })
})

// ---- Appointment popup ----
const openAppointmentPopup = (appt: Appointment) => {
  popupAppt.value = appt
  popupDraftStatus.value = appt.status as AppointmentStatus
  popupDeleteConfirm.value = false
}

const closePopup = () => {
  popupAppt.value = null
  popupDeleteConfirm.value = false
}

const savePopupStatus = async () => {
  if (!popupAppt.value) return
  popupUpdatingStatus.value = true
  try {
    await updateAppointmentStatus(popupAppt.value.id, popupDraftStatus.value)
    appointments.value = appointments.value.map((a) =>
      a.id === popupAppt.value!.id ? { ...a, status: popupDraftStatus.value } : a,
    )
    popupAppt.value = { ...popupAppt.value, status: popupDraftStatus.value }
    flashMessage.value = 'Status wizyty zaktualizowany.'
    setTimeout(() => { flashMessage.value = '' }, 3000)
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udalo sie zmienic statusu.')
  } finally {
    popupUpdatingStatus.value = false
  }
}

const confirmDeleteAppt = async () => {
  if (!popupAppt.value) return
  popupDeleting.value = true
  try {
    await deleteAppointment(popupAppt.value.id)
    appointments.value = appointments.value.filter((a) => a.id !== popupAppt.value!.id)
    closePopup()
    flashMessage.value = 'Wizyta zostala usunieta.'
    setTimeout(() => { flashMessage.value = '' }, 3000)
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udalo sie usunac wizyty.')
  } finally {
    popupDeleting.value = false
  }
}

// ---- Status classes ----
const statusPillClass = (status: string) => {
  if (status === 'Do potwierdzenia') return 'cal-pill--pending'
  if (status === 'Potwierdzona') return 'cal-pill--confirmed'
  if (status === 'Anulowana') return 'cal-pill--cancelled'
  if (status.includes('Zako')) return 'cal-pill--done'
  if (status.includes('Nieobec')) return 'cal-pill--absent'
  return 'cal-pill--planned'
}

const statusChipClass = (status: string) => {
  if (status === 'Do potwierdzenia') return 'chip--danger'
  if (status === 'Potwierdzona') return 'chip--info'
  if (status === 'Anulowana') return 'chip--danger'
  if (status.includes('Zako')) return 'chip--success'
  if (status.includes('Nieobec')) return 'chip--warning'
  return 'chip--default'
}

watch(viewMode, () => {
  selectedDate.value = toIsoDate(new Date())
  currentDate.value = new Date()
})

onMounted(async () => {
  try {
    const [appts, docs] = await Promise.all([getAppointments(), getDoctors()])
    appointments.value = appts
    doctors.value = docs
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udalo sie zaladowac danych kalendarza.')
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <PageLayout>
    <PageHeader title="Kalendarz wizyt" subtitle="Planowanie i zarzadzanie wizytami kliniki">
      <template #actions>
        <RouterLink to="/appointments/list" class="app-btn app-btn--ghost app-btn--sm">
          <AppIcon name="list" />
          Lista wizyt
        </RouterLink>
        <RouterLink
          :to="{ name: 'appointment-add', query: { date: selectedDate } }"
          class="app-btn app-btn--primary app-btn--sm"
        >
          + Umow wizyte
        </RouterLink>
      </template>
    </PageHeader>

    <AppAlert v-if="flashMessage" :message="flashMessage" variant="success" @dismiss="flashMessage = ''" />
    <AppAlert v-if="error" :message="error" variant="error" @dismiss="error = ''" />

    <AppSpinner v-if="loading" label="Ladowanie kalendarza..." />

    <template v-else>
      <div class="cal-toolbar">
        <div class="cal-nav">
          <button class="app-btn app-btn--ghost app-btn--sm cal-nav-btn" @click="navigate(-1)">
            <-
          </button>
          <button class="app-btn app-btn--ghost app-btn--sm" @click="goToday">Dzis</button>
          <h2 class="cal-nav__label">{{ headerLabel }}</h2>
          <button class="app-btn app-btn--ghost app-btn--sm cal-nav-btn" @click="navigate(1)">
            ->
          </button>
        </div>

        <div class="cal-controls">
          <div class="view-toggle">
            <button
              class="view-toggle__btn"
              :class="{ 'view-toggle__btn--active': viewMode === 'month' }"
              @click="viewMode = 'month'"
            >
              Miesiac
            </button>
            <button
              class="view-toggle__btn"
              :class="{ 'view-toggle__btn--active': viewMode === 'week' }"
              @click="viewMode = 'week'"
            >
              Tydzien
            </button>
          </div>

          <select v-model="filterDoctorId" class="cal-filter-select">
            <option value="all">Wszyscy lekarze</option>
            <option v-for="d in doctors" :key="d.id" :value="d.id">
              {{ d.firstName }} {{ d.lastName }}
            </option>
          </select>
        </div>
      </div>

      <div class="cal-shell">
        <div class="cal-main">
          <!-- MONTH VIEW -->
          <div v-if="viewMode === 'month'" class="cal-month">
            <div class="cal-month__header">
              <div v-for="label in weekdayLabels" :key="label" class="cal-wday">{{ label }}</div>
            </div>

            <div class="cal-month__grid">
              <div
                v-for="cell in calendarCells"
                :key="cell.isoDate"
                class="cal-cell"
                :class="{
                  'cal-cell--today': cell.isToday,
                  'cal-cell--other': !cell.inMonth,
                  'cal-cell--selected': cell.isoDate === selectedDate,
                  'cal-cell--weekend': cell.isWeekend && !cell.isToday,
                }"
                @click="selectDay(cell.isoDate)"
              >
                <div class="cal-cell__top">
                  <span class="cal-cell__num">{{ cell.dayNumber }}</span>
                  <button
                    v-if="cell.inMonth"
                    class="cal-cell__add"
                    title="Umow wizyte na ten dzien"
                    @click.stop="quickAddForDate(cell.isoDate)"
                  >
                    +
                  </button>
                </div>

                <div class="cal-cell__events">
                  <div
                    v-for="appt in getCellAppointments(cell.isoDate).slice(0, 3)"
                    :key="appt.id"
                    class="cal-pill"
                    :class="statusPillClass(appt.status)"
                    :title="`${formatTime(appt.appointmentDate)} - ${appt.patientName} (${appt.visitType})`"
                    @click.stop="openAppointmentPopup(appt)"
                  >
                    <span class="cal-pill__dot" :style="{ background: getDoctorColor(appt.doctorId) }" />
                    <span class="cal-pill__text">{{ formatTime(appt.appointmentDate) }} {{ appt.patientName }}</span>
                  </div>

                  <button
                    v-if="getCellAppointments(cell.isoDate).length > 3"
                    class="cal-pill cal-pill--more"
                    @click.stop="selectDay(cell.isoDate)"
                  >
                    +{{ getCellAppointments(cell.isoDate).length - 3 }} wiecej
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- WEEK VIEW -->
          <div v-else-if="viewMode === 'week'" class="cal-week">
            <div class="cal-week__header">
              <div class="cal-week__time-col" />
              <div
                v-for="day in weekDays"
                :key="day.isoDate"
                class="cal-week__day-header"
                :class="{
                  'cal-week__day-header--today': day.isToday,
                  'cal-week__day-header--selected': day.isoDate === selectedDate,
                }"
                @click="selectDay(day.isoDate)"
              >
                <span class="cal-week__wday">{{ day.weekdayLabel }}</span>
                <span class="cal-week__date-num" :class="{ 'cal-week__date-num--today': day.isToday }">{{ day.dayNum }}</span>
              </div>
            </div>

            <div class="cal-week__body-wrapper">
              <div class="cal-week__body">
                <div class="cal-week__time-col">
                  <div v-for="hour in timeSlotHours" :key="hour" class="cal-week__hour-label">
                    {{ String(hour).padStart(2, '0') }}:00
                  </div>
                </div>

                <div
                  v-for="day in weekDays"
                  :key="day.isoDate"
                  class="cal-week__day-col"
                  :class="{ 'cal-week__day-col--today': day.isToday }"
                >
                  <div
                    v-for="hour in timeSlotHours"
                    :key="hour"
                    class="cal-week__slot"
                    @click="quickAddForDateTime(day.isoDate, hour)"
                  />

                  <div
                    v-for="appt in getWeekDayAppointments(day.isoDate)"
                    :key="appt.id"
                    class="cal-week__appt"
                    :class="statusPillClass(appt.status)"
                    :style="getWeekApptStyle(appt)"
                    @click.stop="openAppointmentPopup(appt)"
                  >
                    <strong class="cal-week__appt-time">{{ formatTime(appt.appointmentDate) }}</strong>
                    <span class="cal-week__appt-patient">{{ appt.patientName }}</span>
                    <span class="cal-week__appt-doctor">{{ appt.doctorName }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Doctor legend -->
          <div v-if="doctors.length > 0" class="cal-legend">
            <div v-for="doc in doctors" :key="doc.id" class="cal-legend__item">
              <span class="cal-legend__dot" :style="{ background: getDoctorColor(doc.id) }" />
              <span>{{ doc.firstName }} {{ doc.lastName }}</span>
            </div>
          </div>
        </div>

        <!-- Side panel -->
        <aside class="cal-side-panel">
          <div class="side-panel__header">
            <h3 class="side-panel__title">{{ selectedDayLabel }}</h3>
            <RouterLink
              :to="{ name: 'appointment-add', query: { date: selectedDate } }"
              class="app-btn app-btn--primary app-btn--sm"
            >
              + Umow
            </RouterLink>
          </div>

          <div v-if="selectedDayAppointments.length === 0" class="side-panel__empty">
            <span class="side-panel__empty-icon">[]</span>
            <p>Brak wizyt w tym dniu</p>
            <p class="side-panel__empty-hint">Kliknij "+" na dniu kalendarza lub uzyj przycisku powyzej.</p>
          </div>

          <ul v-else class="side-panel__list">
            <li
              v-for="appt in selectedDayAppointments"
              :key="appt.id"
              class="side-appt"
              :class="{ 'side-appt--pending': appt.status === 'Do potwierdzenia', 'side-appt--cancelled': appt.status === 'Anulowana' || appt.status.includes('Nieobec') }"
              @click="openAppointmentPopup(appt)"
            >
              <div class="side-appt__time-col">
                <span class="side-appt__time">{{ formatTime(appt.appointmentDate) }}</span>
                <span class="side-appt__dot" :style="{ background: getDoctorColor(appt.doctorId) }" />
              </div>
              <div class="side-appt__body">
                <strong class="side-appt__patient">{{ appt.patientName }}</strong>
                <span class="side-appt__sub">{{ appt.doctorName }}</span>
                <span class="side-appt__sub side-appt__type">{{ appt.visitType }}</span>
              </div>
              <span class="status-chip" :class="statusChipClass(appt.status)">{{ appt.status }}</span>
            </li>
          </ul>

          <div v-if="selectedDayAppointments.length > 0" class="side-panel__summary">
            {{ selectedDayAppointments.length }}
            {{ selectedDayAppointments.length === 1 ? 'wizyta' : selectedDayAppointments.length < 5 ? 'wizyty' : 'wizyt' }}
            tego dnia
          </div>
        </aside>
      </div>
    </template>

    <!-- Appointment detail popup -->
    <Teleport to="body">
      <div v-if="popupAppt" class="modal-backdrop" @click.self="closePopup">
        <div class="modal appt-modal">
          <button class="modal-close" @click="closePopup">x</button>

          <div v-if="!popupDeleteConfirm">
            <div class="appt-modal__header">
              <div class="appt-modal__doc-dot" :style="{ background: getDoctorColor(popupAppt.doctorId) }" />
              <div>
                <h3 class="appt-modal__patient">{{ popupAppt.patientName }}</h3>
                <p class="appt-modal__meta">{{ popupAppt.doctorName }}</p>
                <p class="appt-modal__meta">{{ formatAppointmentDate(popupAppt.appointmentDate) }}</p>
                <p class="appt-modal__type">{{ popupAppt.visitType }}</p>
              </div>
            </div>

            <div class="appt-modal__body">
              <div class="appt-modal__status-row">
                <label class="app-form__label">Status wizyty</label>
                <div class="appt-modal__status-ctrl">
                  <select v-model="popupDraftStatus" class="app-form__input app-form__input--sm">
                    <option v-for="s in APPOINTMENT_STATUS_OPTIONS" :key="s" :value="s">{{ s }}</option>
                  </select>
                  <button
                    class="app-btn app-btn--secondary app-btn--sm"
                    :disabled="popupDraftStatus === popupAppt.status || popupUpdatingStatus"
                    @click="savePopupStatus"
                  >
                    {{ popupUpdatingStatus ? 'Zapisuje...' : 'Zapisz status' }}
                  </button>
                </div>
              </div>

              <div v-if="popupAppt.notes" class="appt-modal__notes">
                <strong>Notatki:</strong>
                <p>{{ popupAppt.notes }}</p>
              </div>
            </div>

            <div class="appt-modal__footer">
              <button
                class="app-btn app-btn--danger app-btn--sm"
                @click="popupDeleteConfirm = true"
              >
                Usun wizyte
              </button>
              <RouterLink
                :to="{ name: 'appointment-edit', params: { id: popupAppt.id } }"
                class="app-btn app-btn--warning app-btn--sm"
                @click="closePopup"
              >
                Edytuj pelne dane
              </RouterLink>
            </div>
          </div>

          <div v-else class="appt-modal__confirm-delete">
            <p class="appt-modal__confirm-text">
              Na pewno chcesz usunac wizyte pacjenta <strong>{{ popupAppt.patientName }}</strong>?
            </p>
            <p class="appt-modal__confirm-warn">Tej operacji nie mozna cofnac.</p>
            <div class="appt-modal__footer">
              <button class="app-btn app-btn--secondary app-btn--sm" @click="popupDeleteConfirm = false">
                Anuluj
              </button>
              <button
                class="app-btn app-btn--danger app-btn--sm"
                :disabled="popupDeleting"
                @click="confirmDeleteAppt"
              >
                {{ popupDeleting ? 'Usuwanie...' : 'Usun wizyte' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>
  </PageLayout>
</template>

<style scoped>
/* ---- Toolbar ---- */
.cal-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.cal-nav {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.cal-nav__label {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 700;
  min-width: 16rem;
  text-align: center;
}

.cal-nav-btn {
  font-size: 1.1rem;
  min-width: 2rem;
}

.cal-controls {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.view-toggle {
  display: flex;
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-sm);
  overflow: hidden;
}

.view-toggle__btn {
  padding: 0.4rem 0.85rem;
  font: inherit;
  font-size: 0.85rem;
  font-weight: 600;
  background: transparent;
  color: var(--color-text-muted);
  border: none;
  cursor: pointer;
  transition: background 0.15s, color 0.15s;
}

.view-toggle__btn--active {
  background: var(--color-primary);
  color: #fff;
}

.cal-filter-select {
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-sm);
  background: #fff;
  padding: 0.4rem 0.65rem;
  font: inherit;
  font-size: 0.85rem;
  color: var(--color-text);
  cursor: pointer;
}

/* ---- Shell layout ---- */
.cal-shell {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 1.25rem;
  align-items: start;
}

@media (max-width: 900px) {
  .cal-shell {
    grid-template-columns: 1fr;
  }
}

.cal-main {
  min-width: 0;
}

/* ---- Month view ---- */
.cal-month {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  overflow: hidden;
  background: #fff;
}

.cal-month__header {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  background: var(--color-primary);
}

.cal-wday {
  padding: 0.6rem 0.5rem;
  text-align: center;
  font-size: 0.78rem;
  font-weight: 700;
  color: rgba(255, 255, 255, 0.9);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.cal-month__grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
}

.cal-cell {
  min-height: 110px;
  border-right: 1px solid var(--color-border);
  border-bottom: 1px solid var(--color-border);
  padding: 0.35rem;
  cursor: pointer;
  transition: background 0.12s;
  position: relative;
}

.cal-cell:nth-child(7n) {
  border-right: none;
}

.cal-cell:hover {
  background: var(--color-primary-soft);
}

.cal-cell--other {
  background: #f8fafc;
}

.cal-cell--other .cal-cell__num {
  color: #cbd5e1;
}

.cal-cell--today .cal-cell__num {
  background: var(--color-primary);
  color: #fff;
}

.cal-cell--selected {
  background: #eff6ff;
  box-shadow: inset 0 0 0 2px var(--color-primary);
}

.cal-cell--weekend:not(.cal-cell--today):not(.cal-cell--other) {
  background: #fafbff;
}

.cal-cell__top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.3rem;
}

.cal-cell__num {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 1.7rem;
  height: 1.7rem;
  border-radius: 50%;
  font-size: 0.82rem;
  font-weight: 700;
  color: var(--color-text);
}

.cal-cell__add {
  opacity: 0;
  width: 1.4rem;
  height: 1.4rem;
  border-radius: 50%;
  border: none;
  background: var(--color-primary);
  color: #fff;
  font-size: 1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.15s, transform 0.15s;
  line-height: 1;
}

.cal-cell:hover .cal-cell__add {
  opacity: 1;
}

.cal-cell__add:hover {
  transform: scale(1.15);
}

.cal-cell__events {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

/* ---- Calendar pills ---- */
.cal-pill {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 2px 5px;
  border-radius: 4px;
  font-size: 0.72rem;
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  border: none;
  text-align: left;
  width: 100%;
  transition: opacity 0.12s;
}

.cal-pill:hover {
  opacity: 0.8;
}

.cal-pill__dot {
  flex-shrink: 0;
  width: 7px;
  height: 7px;
  border-radius: 50%;
}

.cal-pill__text {
  overflow: hidden;
  text-overflow: ellipsis;
}

.cal-pill--pending { background: #fee2e2; color: #b91c1c; border: 1px solid #fecaca; }
.cal-pill--planned { background: #dbeafe; color: #1d4ed8; }
.cal-pill--confirmed { background: #d1fae5; color: #065f46; }
.cal-pill--done { background: #e2e8f0; color: #475569; }
.cal-pill--cancelled { background: #fee2e2; color: #991b1b; text-decoration: line-through; }
.cal-pill--absent { background: #fef3c7; color: #92400e; }
.cal-pill--more {
  background: transparent;
  color: var(--color-primary);
  font-weight: 700;
  font-size: 0.7rem;
}

/* ---- Week view ---- */
.cal-week {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  overflow: hidden;
  background: #fff;
}

.cal-week__header {
  display: grid;
  grid-template-columns: 4rem repeat(7, 1fr);
  border-bottom: 2px solid var(--color-border);
  background: #f8fafc;
}

.cal-week__time-col {
  border-right: 1px solid var(--color-border);
}

.cal-week__day-header {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 0.6rem 0.25rem;
  cursor: pointer;
  border-right: 1px solid var(--color-border);
  transition: background 0.12s;
}

.cal-week__day-header:last-child {
  border-right: none;
}

.cal-week__day-header:hover {
  background: var(--color-primary-soft);
}

.cal-week__day-header--today {
  background: #eff6ff;
}

.cal-week__day-header--selected {
  background: #dbeafe;
}

.cal-week__wday {
  font-size: 0.72rem;
  font-weight: 600;
  color: var(--color-text-muted);
  text-transform: uppercase;
}

.cal-week__date-num {
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--color-text);
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
}

.cal-week__date-num--today {
  background: var(--color-primary);
  color: #fff;
}

.cal-week__body-wrapper {
  overflow-y: auto;
  max-height: 640px;
}

.cal-week__body {
  display: grid;
  grid-template-columns: 4rem repeat(7, 1fr);
  position: relative;
}

.cal-week__time-col {
  display: flex;
  flex-direction: column;
  border-right: 1px solid var(--color-border);
}

.cal-week__hour-label {
  height: 64px;
  display: flex;
  align-items: flex-start;
  padding: 2px 6px 0;
  font-size: 0.72rem;
  color: var(--color-text-muted);
  font-weight: 600;
  border-bottom: 1px solid var(--color-border);
  box-sizing: border-box;
}

.cal-week__day-col {
  border-right: 1px solid var(--color-border);
  position: relative;
}

.cal-week__day-col:last-child {
  border-right: none;
}

.cal-week__day-col--today {
  background: #fefffe;
}

.cal-week__slot {
  height: 64px;
  border-bottom: 1px solid var(--color-border);
  cursor: pointer;
  transition: background 0.1s;
}

.cal-week__slot:hover {
  background: #eff6ff;
}

.cal-week__appt {
  position: absolute;
  left: 2px;
  right: 2px;
  border-radius: 4px;
  padding: 3px 6px;
  font-size: 0.72rem;
  cursor: pointer;
  overflow: hidden;
  z-index: 2;
  border-left: 3px solid;
  display: flex;
  flex-direction: column;
  transition: opacity 0.12s;
}

.cal-week__appt:hover {
  opacity: 0.85;
  z-index: 3;
}

.cal-week__appt.cal-pill--pending { background: #fee2e2; color: #b91c1c; border-left-color: #ef4444; }
.cal-week__appt.cal-pill--planned { background: #dbeafe; color: #1e40af; border-left-color: #3b82f6; }
.cal-week__appt.cal-pill--confirmed { background: #d1fae5; color: #065f46; border-left-color: #10b981; }
.cal-week__appt.cal-pill--done { background: #e2e8f0; color: #475569; border-left-color: #94a3b8; }
.cal-week__appt.cal-pill--cancelled { background: #fee2e2; color: #991b1b; border-left-color: #ef4444; }
.cal-week__appt.cal-pill--absent { background: #fef3c7; color: #92400e; border-left-color: #f59e0b; }

.cal-week__appt-time { font-weight: 700; line-height: 1.2; }
.cal-week__appt-patient { line-height: 1.3; }
.cal-week__appt-doctor { font-size: 0.68rem; opacity: 0.75; }

/* ---- Legend ---- */
.cal-legend {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  padding: 0.75rem;
  background: #f8fafc;
  border: 1px solid var(--color-border);
  border-top: none;
  border-radius: 0 0 var(--radius-lg) var(--radius-lg);
}

.cal-legend__item {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--color-text-muted);
}

.cal-legend__dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  flex-shrink: 0;
}

/* ---- Side panel ---- */
.cal-side-panel {
  background: #fff;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.side-panel__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  padding: 0.85rem 1rem;
  border-bottom: 1px solid var(--color-border);
  background: #f8fafc;
  flex-wrap: wrap;
}

.side-panel__title {
  margin: 0;
  font-size: 0.88rem;
  font-weight: 700;
  color: var(--color-text);
  text-transform: capitalize;
  line-height: 1.3;
}

.side-panel__empty {
  padding: 2rem 1.25rem;
  text-align: center;
  color: var(--color-text-muted);
}

.side-panel__empty-icon {
  font-size: 2rem;
  display: block;
  margin-bottom: 0.5rem;
}

.side-panel__empty p {
  margin: 0.25rem 0;
  font-weight: 600;
}

.side-panel__empty-hint {
  font-size: 0.8rem;
  font-weight: 400 !important;
}

.side-panel__list {
  list-style: none;
  margin: 0;
  padding: 0.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  overflow-y: auto;
  max-height: 520px;
}

.side-appt {
  display: flex;
  align-items: flex-start;
  gap: 0.6rem;
  padding: 0.6rem 0.75rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border);
  cursor: pointer;
  background: #fdfdff;
  transition: background 0.12s, border-color 0.12s;
}

.side-appt:hover {
  background: var(--color-primary-soft);
  border-color: var(--color-primary);
}

.side-appt--cancelled {
  opacity: 0.65;
}

.side-appt--pending {
  background: #fff1f2;
  border-color: #fecaca;
}

.side-appt--pending:hover {
  background: #ffe4e6;
  border-color: #fca5a5;
}

.side-appt--cancelled .side-appt__patient {
  text-decoration: line-through;
}

.side-appt__time-col {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  min-width: 38px;
}

.side-appt__time {
  font-size: 0.78rem;
  font-weight: 800;
  color: var(--color-text);
  white-space: nowrap;
}

.side-appt__dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.side-appt__body {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 1px;
}

.side-appt__patient {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--color-text);
  display: block;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.side-appt__sub {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.side-appt__type {
  font-style: italic;
}

.side-panel__summary {
  padding: 0.6rem 1rem;
  border-top: 1px solid var(--color-border);
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--color-text-muted);
  background: #f8fafc;
}

/* ---- Status chips in side panel ---- */
.status-chip {
  flex-shrink: 0;
  display: inline-flex;
  align-items: center;
  padding: 0.2rem 0.55rem;
  border-radius: 999px;
  font-size: 0.7rem;
  font-weight: 700;
  white-space: nowrap;
}

.chip--default { background: #e2e8f0; color: #475569; }
.chip--info { background: #dbeafe; color: #1d4ed8; }
.chip--success { background: #d1fae5; color: #065f46; }
.chip--warning { background: #fef3c7; color: #92400e; }
.chip--danger { background: #fee2e2; color: #991b1b; }

/* ---- Appointment modal ---- */
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 500;
  padding: 1rem;
}

.modal {
  background: #fff;
  border-radius: var(--radius-lg);
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.2);
  max-width: 440px;
  width: 100%;
  position: relative;
  overflow: hidden;
}

.modal-close {
  position: absolute;
  top: 0.75rem;
  right: 0.75rem;
  background: none;
  border: none;
  font-size: 1.4rem;
  line-height: 1;
  cursor: pointer;
  color: var(--color-text-muted);
  padding: 0.15rem 0.35rem;
  border-radius: var(--radius-sm);
}

.modal-close:hover {
  background: var(--color-border);
  color: var(--color-text);
}

.appt-modal {
  padding: 0;
}

.appt-modal__header {
  display: flex;
  gap: 0.85rem;
  align-items: flex-start;
  padding: 1.25rem 1.5rem 1rem;
  border-bottom: 1px solid var(--color-border);
  background: #f8fafc;
}

.appt-modal__doc-dot {
  width: 14px;
  height: 14px;
  border-radius: 50%;
  margin-top: 4px;
  flex-shrink: 0;
}

.appt-modal__patient {
  margin: 0 0 0.2rem;
  font-size: 1.05rem;
  font-weight: 800;
  padding-right: 2rem;
}

.appt-modal__meta {
  margin: 0;
  font-size: 0.85rem;
  color: var(--color-text-muted);
}

.appt-modal__type {
  margin: 0.35rem 0 0;
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--color-primary);
}

.appt-modal__body {
  padding: 1rem 1.5rem;
}

.appt-modal__status-row {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.appt-modal__status-ctrl {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  flex-wrap: wrap;
}

.app-form__input--sm {
  padding: 0.35rem 0.5rem;
  font-size: 0.85rem;
  flex: 1;
  min-width: 0;
}

.appt-modal__notes {
  margin-top: 1rem;
  padding: 0.75rem;
  background: #f8fafc;
  border-radius: var(--radius-md);
  font-size: 0.88rem;
}

.appt-modal__notes strong {
  display: block;
  margin-bottom: 0.25rem;
}

.appt-modal__notes p {
  margin: 0;
  color: var(--color-text-muted);
}

.appt-modal__footer {
  display: flex;
  gap: 0.5rem;
  padding: 0.85rem 1.5rem;
  border-top: 1px solid var(--color-border);
  background: #f8fafc;
  justify-content: flex-end;
}

.appt-modal__confirm-delete {
  padding: 1.5rem;
}

.appt-modal__confirm-text {
  margin: 0 0 0.75rem;
  font-size: 0.95rem;
}

.appt-modal__confirm-warn {
  margin: 0 0 1.25rem;
  font-size: 0.85rem;
  color: #dc2626;
}
</style>
