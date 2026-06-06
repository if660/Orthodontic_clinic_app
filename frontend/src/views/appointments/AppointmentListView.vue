<template>
  <PageLayout>
    <PageHeader title="Lista wizyt" subtitle="Zarzadzaj i filtruj wszystkie wizyty">
      <template #actions>
        <RouterLink to="/appointments" class="app-btn app-btn--ghost app-btn--sm">
          <AppIcon name="calendar" />
          Kalendarz
        </RouterLink>
        <RouterLink :to="{ name: 'appointment-add' }" class="app-btn app-btn--primary app-btn--sm">
          + Dodaj wizyte
        </RouterLink>
      </template>
    </PageHeader>

    <AppAlert v-if="successMessage" :message="successMessage" variant="success" @dismiss="successMessage = null" />
    <AppAlert v-if="error" :message="error" variant="error" @dismiss="error = null" />

    <AppSpinner v-if="loading" label="Ladowanie..." />

    <template v-else>
      <div class="app-toolbar">
        <div class="app-search">
          <span class="app-search__icon" aria-hidden="true"><AppIcon name="search" /></span>
          <input
            v-model="searchQuery"
            class="app-search__input"
            type="search"
            placeholder="Szukaj po pacjencie, lekarzu, typie..."
            aria-label="Szukaj wizyt"
          />
        </div>

        <select v-model="filterDoctorId" class="filter-select">
          <option value="all">Wszyscy lekarze</option>
          <option v-for="d in doctors" :key="d.id" :value="d.id">
            {{ d.firstName }} {{ d.lastName }}
          </option>
        </select>

        <select v-model="filterStatus" class="filter-select" aria-label="Filtruj po statusie wizyty">
          <option v-for="s in simpleStatusFilterOptions" :key="s.value" :value="s.value">
            {{ s.label }}
          </option>
        </select>
      </div>

      <div class="app-card app-card--flat">
        <p v-if="filteredAppointments.length === 0" class="empty-text">
          {{ appointments.length === 0 ? 'Brak wizyt w systemie' : 'Brak wizyt pasujacych do wybranych filtrow' }}
        </p>

        <table v-else class="app-table">
          <thead>
            <tr>
              <th>Pacjent</th>
              <th>Lekarz</th>
              <th>Data wizyty</th>
              <th>Typ wizyty</th>
              <th>Status</th>
              <th>Akcje</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="a in filteredAppointments"
              :key="a.id"
              :class="{ 'row--pending': a.status === 'Do potwierdzenia', 'row--cancelled': a.status === 'Anulowana' || a.status.includes('Nieobec') }"
            >
              <td class="td-patient">{{ a.patientName }}</td>
              <td>{{ a.doctorName }}</td>
              <td class="td-date">{{ formatAppointmentDate(a.appointmentDate) }}</td>
              <td>{{ a.visitType }}</td>
              <td>
                <span class="status-badge" :class="statusClass(a.status)">{{ a.status }}</span>
              </td>
              <td class="app-table__actions">
                <RouterLink
                  :to="{ name: 'appointment-details', params: { id: a.id } }"
                  class="app-btn app-btn--ghost app-btn--sm"
                >
                  Szczegoly
                </RouterLink>
                <RouterLink
                  :to="{ name: 'appointment-edit', params: { id: a.id } }"
                  class="app-btn app-btn--ghost app-btn--sm"
                >
                  Edytuj
                </RouterLink>
                <button
                  v-if="a.status === 'Do potwierdzenia' || a.status === 'Zaplanowana'"
                  class="app-btn app-btn--success app-btn--sm"
                  :disabled="updatingStatusId === a.id"
                  @click="confirmAppointment(a)"
                >
                  {{ updatingStatusId === a.id ? 'Potwierdzanie...' : 'Potwierdz' }}
                </button>
                <button class="app-btn app-btn--danger app-btn--sm" @click="onDeleteRequest(a)">
                  Usun
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <p class="results-info">
        Wyswietlono {{ filteredAppointments.length }} z {{ appointments.length }} wizyt
      </p>
    </template>

    <div v-if="deleteModalVisible" class="modal-overlay" @click.self="cancelDelete">
      <div class="modal">
        <h2 class="modal__title">Usun wizyte</h2>
        <p class="modal__body">
          Czy na pewno chcesz usunac wizyte pacjenta
          <strong>{{ appointmentToDelete?.patientName }}</strong>?
        </p>
        <div class="modal__actions">
          <button class="app-btn app-btn--secondary" :disabled="deleting" @click="cancelDelete">
            Anuluj
          </button>
          <button class="app-btn app-btn--danger" :disabled="deleting" @click="confirmDelete">
            {{ deleting ? 'Usuwanie...' : 'Usun' }}
          </button>
        </div>
      </div>
    </div>
  </PageLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { getAppointments, deleteAppointment, updateAppointmentStatus } from '../../services/appointmentService'
import { getDoctors } from '../../services/doctorService'
import PageLayout from '../../components/common/PageLayout.vue'
import PageHeader from '../../components/common/PageHeader.vue'
import AppAlert from '../../components/common/AppAlert.vue'
import AppSpinner from '../../components/common/AppSpinner.vue'
import AppIcon from '../../components/common/AppIcon.vue'
import type { Appointment, AppointmentFlashType } from '../../types/appointment'
import { formatAppointmentDate, getAppointmentFlashMessage } from '../../types/appointment'
import type { Doctor } from '../../types/doctor'
import { getApiErrorMessage } from '../../utils/apiError'

const route = useRoute()

const appointments = ref<Appointment[]>([])
const doctors = ref<Doctor[]>([])
const searchQuery = ref('')
const filterStatus = ref<string>('active')
const filterDoctorId = ref<string | number>('all')
const loading = ref(true)
const error = ref<string | null>(null)
const successMessage = ref<string | null>(null)
const appointmentToDelete = ref<Appointment | null>(null)
const deleteModalVisible = ref(false)
const deleting = ref(false)
const updatingStatusId = ref<number | null>(null)

const statusFilterOptions = [
  { value: 'active', label: 'Aktywne' },
  { value: 'all', label: 'Wszystkie' },
  { value: 'Do potwierdzenia', label: 'Do potwierdzenia' },
  { value: 'Zaplanowana', label: 'Zaplanowane' },
  { value: 'Potwierdzona', label: 'Potwierdzone' },
  { value: 'Zakonczona', label: 'Zakonczone' },
  { value: 'Anulowana', label: 'Anulowane' },
  { value: 'Nieobecnosc', label: 'Nieobecnosc' },
]

const simpleStatusFilterOptions = [
  { value: 'active', label: 'Aktywne' },
  { value: 'all', label: 'Wszystkie' },
  { value: 'Do potwierdzenia', label: 'Do potwierdzenia' },
  { value: 'Zaplanowana', label: 'Zaplanowana' },
  { value: 'Zakonczona', label: 'Odbyta' },
  { value: 'Anulowana', label: 'Anulowana' },
]

const getStatusCount = (status: string): number => {
  if (status === 'all') return appointments.value.length
  return appointments.value.filter((a) => a.status === status).length
}

const filteredAppointments = computed(() => {
  let list = appointments.value

  if (filterDoctorId.value !== 'all') {
    list = list.filter((a) => a.doctorId === Number(filterDoctorId.value))
  }

  if (filterStatus.value === 'active') {
    list = list.filter((a) => a.status !== 'Anulowana')
  } else if (filterStatus.value !== 'all') {
    list = list.filter((a) => a.status === filterStatus.value)
  }

  const q = searchQuery.value.trim().toLowerCase()
  if (q) {
    list = list.filter(
      (a) =>
        (a.patientName ?? '').toLowerCase().includes(q) ||
        (a.doctorName ?? '').toLowerCase().includes(q) ||
        a.visitType.toLowerCase().includes(q),
    )
  }

  return list.sort((a, b) => b.appointmentDate.localeCompare(a.appointmentDate))
})

const statusClass = (status: string) => ({
  'status-badge--pending': status === 'Do potwierdzenia',
  'status-badge--planned': status === 'Zaplanowana',
  'status-badge--confirmed': status === 'Potwierdzona',
  'status-badge--done': status.includes('Zako'),
  'status-badge--cancelled': status === 'Anulowana',
  'status-badge--absent': status.includes('Nieobec'),
})

const loadData = async () => {
  loading.value = true
  error.value = null
  try {
    const [appts, docs] = await Promise.all([getAppointments(), getDoctors()])
    appointments.value = appts
    doctors.value = docs
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udalo sie zaladowac danych')
  } finally {
    loading.value = false
  }
}

const applyFlashFromQuery = () => {
  const flash = route.query.flash as AppointmentFlashType | undefined
  if (!flash) return
  successMessage.value = getAppointmentFlashMessage(flash)
}

const onDeleteRequest = (a: Appointment) => {
  appointmentToDelete.value = a
  deleteModalVisible.value = true
}

const cancelDelete = () => {
  deleteModalVisible.value = false
  appointmentToDelete.value = null
}

const confirmDelete = async () => {
  if (!appointmentToDelete.value) return
  deleting.value = true
  error.value = null
  try {
    await deleteAppointment(appointmentToDelete.value.id)
    cancelDelete()
    successMessage.value = getAppointmentFlashMessage('deleted')
    await loadData()
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udalo sie usunac wizyty.')
    cancelDelete()
  } finally {
    deleting.value = false
  }
}

const confirmAppointment = async (a: Appointment) => {
  updatingStatusId.value = a.id
  error.value = null
  try {
    await updateAppointmentStatus(a.id, 'Potwierdzona')
    successMessage.value = 'Wizyta zostala potwierdzona'
    await loadData()
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udalo sie potwierdzic wizyty.')
  } finally {
    updatingStatusId.value = null
  }
}

onMounted(async () => {
  applyFlashFromQuery()
  await loadData()
})
</script>

<style scoped>
.filter-select {
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-sm);
  background: #fff;
  padding: 0.45rem 0.65rem;
  font: inherit;
  font-size: 0.85rem;
  color: var(--color-text);
}

.status-filter-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
  margin-bottom: 0.75rem;
}

.status-filter-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.35rem 0.75rem;
  border-radius: 999px;
  border: 1px solid var(--color-border-strong);
  background: #fff;
  font: inherit;
  font-size: 0.82rem;
  font-weight: 600;
  cursor: pointer;
  color: var(--color-text-muted);
  transition: background 0.12s, color 0.12s, border-color 0.12s;
}

.status-filter-btn:hover {
  border-color: var(--color-primary);
  color: var(--color-primary);
}

.status-filter-btn--active {
  background: var(--color-primary);
  color: #fff;
  border-color: var(--color-primary);
}

.status-filter-btn__count {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 1.3rem;
  height: 1.3rem;
  border-radius: 999px;
  font-size: 0.72rem;
  font-weight: 700;
  background: rgba(0, 0, 0, 0.1);
  padding: 0 0.2rem;
}

.status-filter-btn--active .status-filter-btn__count {
  background: rgba(255, 255, 255, 0.25);
}

.status-badge {
  display: inline-block;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  font-size: 0.78rem;
  font-weight: 600;
}

.status-badge--pending { background: #fee2e2; color: #b91c1c; }
.status-badge--planned { background: #dbeafe; color: #1d4ed8; }
.status-badge--confirmed { background: #d1fae5; color: #065f46; }
.status-badge--done { background: #e2e8f0; color: #475569; }
.status-badge--cancelled { background: #fee2e2; color: #991b1b; }
.status-badge--absent { background: #fef3c7; color: #92400e; }

.row--cancelled td {
  opacity: 0.6;
}

.row--pending td {
  background: #fff1f2;
}

.row--pending:hover td {
  background: #ffe4e6;
}

.td-date {
  white-space: nowrap;
}

.td-patient {
  font-weight: 600;
}

.results-info {
  text-align: right;
  font-size: 0.8rem;
  color: var(--color-text-muted);
  margin-top: 0.5rem;
}

.empty-text {
  color: var(--color-text-muted);
  padding: 1rem 0;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}

.modal {
  background: #fff;
  border-radius: var(--radius-lg);
  padding: 2rem;
  max-width: 420px;
  width: 100%;
  box-shadow: var(--shadow-lg);
}

.modal__title { margin: 0 0 1rem; font-size: 1.2rem; }
.modal__body { margin: 0 0 1.5rem; color: var(--color-text-muted); }
.modal__actions { display: flex; gap: 0.75rem; justify-content: flex-end; }
</style>
