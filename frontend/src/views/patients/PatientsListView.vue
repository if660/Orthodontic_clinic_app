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
            class="app-search__input"
            type="search"
            placeholder="Szukaj pacjenta..."
            aria-label="Szukaj pacjenta"
          />
        </div>
      </div>

      <PatientTable
        :patients="filteredPatients"
        :empty-message="emptyTableMessage"
        @details="onDetails"
        @edit="onEdit"
        @delete="onDeleteRequest"
      />
    </template>

    <ConfirmDeleteModal
      :visible="deleteModalVisible"
      :patient-name="patientToDeleteName"
      :deleting="deleting"
      @confirm="confirmDelete"
      @cancel="cancelDelete"
    />
  </PageLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getPatients, deletePatient } from '../../services/patientService'
import PatientTable from '../../components/patients/PatientTable.vue'
import ConfirmDeleteModal from '../../components/patients/ConfirmDeleteModal.vue'
import PageLayout from '../../components/common/PageLayout.vue'
import PageHeader from '../../components/common/PageHeader.vue'
import AppAlert from '../../components/common/AppAlert.vue'
import AppSpinner from '../../components/common/AppSpinner.vue'
import AppIcon from '../../components/common/AppIcon.vue'
import type { Patient, PatientFlashType } from '../../types/patient'
import { getFlashMessage, getPatientFullName } from '../../types/patient'
import { getApiErrorMessage } from '../../utils/apiError'

const router = useRouter()
const route = useRoute()

const patients = ref<Patient[]>([])
const searchQuery = ref('')
const loading = ref(true)
const error = ref<string | null>(null)
const successMessage = ref<string | null>(null)

const patientToDelete = ref<Patient | null>(null)
const deleteModalVisible = ref(false)
const deleting = ref(false)

const filteredPatients = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()
  if (!query) return patients.value

  return patients.value.filter(
    (patient) =>
      patient.firstName.toLowerCase().includes(query) ||
      patient.lastName.toLowerCase().includes(query),
  )
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
  if (!patientToDelete.value) return ''
  return getPatientFullName(patientToDelete.value) || 'tego pacjenta'
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
    patients.value = await getPatients()
  } catch (err) {
    console.error('Error loading patients:', err)
    error.value = getApiErrorMessage(err, 'Nie udało się załadować listy pacjentów')
    patients.value = []
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

const onDeleteRequest = (patient: Patient) => {
  patientToDelete.value = patient
  deleteModalVisible.value = true
}

const cancelDelete = () => {
  deleteModalVisible.value = false
  patientToDelete.value = null
}

const confirmDelete = async () => {
  if (!patientToDelete.value) return

  deleting.value = true
  error.value = null

  try {
    await deletePatient(patientToDelete.value.id)
    cancelDelete()
    successMessage.value = getFlashMessage('deleted')
    await loadPatients()
  } catch (err) {
    console.error('Error deleting patient:', err)
    error.value = getApiErrorMessage(err, 'Nie udało się usunąć pacjenta. Spróbuj ponownie.')
    cancelDelete()
  } finally {
    deleting.value = false
  }
}

onMounted(async () => {
  applyFlashFromQuery()
  await loadPatients()
})
</script>
