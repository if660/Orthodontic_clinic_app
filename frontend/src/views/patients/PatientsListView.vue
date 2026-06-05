<template>
  <PageLayout>
    <PageHeader
      title="Lista pacjentów"
      subtitle="Zarządzaj pacjentami kliniki i ich dokumentacją"
    >
      <template #actions>
        <button type="button" class="app-btn app-btn--primary" @click="goToAdd">
          <AppIcon name="plus" />
          <span>Dodaj pacjenta</span>
        </button>
      </template>
    </PageHeader>

    <AppAlert
      v-if="successMessage"
      :message="successMessage"
      variant="success"
      @dismiss="successMessage = null"
    />

    <AppAlert
      v-if="error"
      :message="error"
      variant="error"
      @dismiss="error = null"
    />

    <AppSpinner v-if="loading" label="Ładowanie..." />

    <template v-else>
      <div class="app-toolbar">
        <div class="app-search">
          <span class="app-search__icon" aria-hidden="true">
            <AppIcon name="search" />
          </span>
          <input
            v-model="searchQuery"
            v-autofocus
            class="app-search__input"
            type="search"
            placeholder="Szukaj pacjenta..."
            aria-label="Szukaj pacjenta"
          />
        </div>

        <div class="app-filter">
          <label class="app-filter__label" for="age-filter">Grupa pacjenta</label>
          <select id="age-filter" v-model="ageFilter" class="app-filter__select">
            <option value="all">Wszyscy</option>
            <option value="minor">Dzieci</option>
            <option value="adult">Dorośli</option>
          </select>
        </div>

        <div class="app-filter">
          <label class="app-filter__label" for="visit-filter">Plan wizyty</label>
          <select id="visit-filter" v-model="visitPlanFilter" class="app-filter__select">
            <option value="all">Wszystko</option>
            <option value="withUpcoming">Ma kolejną wizytę</option>
            <option value="withoutUpcoming">Bez kolejnej wizyty</option>
          </select>
        </div>
      </div>

      <PatientTable
        :patients="filteredPatients"
        :empty-message="emptyTableMessage"
        @details="onDetails"
        @edit="onEdit"
        @appointments="onAppointments"
        @documents="onDocuments"
      />
    </template>

  </PageLayout>
</template>

<script setup lang="ts">
import { ref, computed, defineAsyncComponent, onMounted, watch, watchEffect } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getPatients } from '../../services/patientService'
import { getAppointments } from '../../services/appointmentService'
import PageLayout from '../../components/common/PageLayout.vue'
import PageHeader from '../../components/common/PageHeader.vue'
import AppAlert from '../../components/common/AppAlert.vue'
import AppSpinner from '../../components/common/AppSpinner.vue'
import AppIcon from '../../components/common/AppIcon.vue'
import type { Patient, PatientFlashType } from '../../types/patient'
import type { Appointment } from '../../types/appointment'
import {
  getFlashMessage,
  getPatientFullName,
  isPatientMinorFromBirthDate,
} from '../../types/patient'
import { getApiErrorMessage } from '../../utils/apiError'
import { usePersistedSearch } from '../../composables/usePersistedSearch'

const PatientTable = defineAsyncComponent(() => import('../../components/patients/PatientTable.vue'))

const router = useRouter()
const route = useRoute()

const patients = ref<Patient[]>([])
const appointments = ref<Appointment[]>([])
const { searchQuery } = usePersistedSearch('patients-search-query')
const ageFilter = ref<'all' | 'minor' | 'adult'>('all')
const visitPlanFilter = ref<'all' | 'withUpcoming' | 'withoutUpcoming'>('all')
const loading = ref(true)
const error = ref<string | null>(null)
const successMessage = ref<string | null>(null)

const filteredPatients = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()
  const now = new Date()

  const patientIdsWithUpcoming = new Set(
    appointments.value
      .filter((appointment) => new Date(appointment.appointmentDate) >= now)
      .map((appointment) => appointment.patientId),
  )

  if (!query && ageFilter.value === 'all' && visitPlanFilter.value === 'all') return patients.value

  return patients.value
    .filter((patient) => {
      const matchesSearch =
        patient.firstName.toLowerCase().includes(query) ||
        patient.lastName.toLowerCase().includes(query) ||
        patient.phone.toLowerCase().includes(query) ||
        patient.email.toLowerCase().includes(query) ||
        patient.guardianFullName.toLowerCase().includes(query)

      const isMinor = isPatientMinorFromBirthDate(patient.birthDate)
      const matchesAgeFilter =
        ageFilter.value === 'all' ||
        (ageFilter.value === 'minor' && isMinor) ||
        (ageFilter.value === 'adult' && !isMinor)

      const hasUpcoming = patientIdsWithUpcoming.has(patient.id)
      const matchesVisitPlanFilter =
        visitPlanFilter.value === 'all' ||
        (visitPlanFilter.value === 'withUpcoming' && hasUpcoming) ||
        (visitPlanFilter.value === 'withoutUpcoming' && !hasUpcoming)

      return matchesSearch && matchesAgeFilter && matchesVisitPlanFilter
    })
    .sort((a, b) => {
      const lastNameCompare = a.lastName.localeCompare(b.lastName)
      return lastNameCompare !== 0
        ? lastNameCompare
        : a.firstName.localeCompare(b.firstName)
    })
})

const emptyTableMessage = computed(() => {
  if (patients.value.length === 0) {
    return 'Brak pacjentów'
  }
  if (filteredPatients.value.length === 0) {
    return 'Nie znaleziono pacjentów pasujących do wyszukiwania'
  }
  return 'Brak pacjentów'
})

const patientToDeleteName = computed(() => {
  return ''
})

const applyFlashFromQuery = () => {
  const flash = route.query.flash as PatientFlashType | undefined
  if (!flash) return

  successMessage.value = getFlashMessage(flash)
  router.replace({ query: {} })
}

const loadPatients = async () => {
  loading.value = true
  error.value = null

  try {
    const [patientsData, appointmentsData] = await Promise.all([
      getPatients(),
      getAppointments(),
    ])

    patients.value = patientsData
    appointments.value = appointmentsData
  } catch (err) {
    console.error('Error loading patients:', err)
    error.value = getApiErrorMessage(err, 'Nie udało się załadować listy pacjentów')
    patients.value = []
    appointments.value = []
  } finally {
    loading.value = false
  }
}

const goToAdd = () => {
  router.push({ name: 'patient-add' })
}

const onDetails = (patient: Patient) => {
  router.push({ name: 'patient-details', params: { id: patient.id } })
}

const onEdit = (patient: Patient) => {
  router.push({ name: 'patient-edit', params: { id: patient.id } })
}

const onAppointments = (patient: Patient) => {
  router.push({ name: 'appointments', query: { patientId: patient.id } })
}

const onDocuments = (patient: Patient) => {
  router.push({ name: 'patient-details', params: { id: patient.id }, hash: '#documents' })
}

onMounted(async () => {
  applyFlashFromQuery()
  await loadPatients()
})

watch(searchQuery, () => {
  successMessage.value = null
})

watch(ageFilter, () => {
  successMessage.value = null
})

watch(visitPlanFilter, () => {
  successMessage.value = null
})

watchEffect(() => {
  document.title = `Klinika ortodontyczna - Pacjenci (${filteredPatients.value.length})`
})
</script>

<style scoped>
.app-filter {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
}

.app-filter__label {
  font-size: 0.85rem;
  color: var(--color-text-muted);
  white-space: nowrap;
}

.app-filter__select {
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-md);
  padding: 0.5rem 0.7rem;
  background: #fff;
  font: inherit;
}

@media (max-width: 760px) {
  .app-filter {
    width: 100%;
    justify-content: space-between;
  }
}
</style>
