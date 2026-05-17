<template>
  <div class="patients-page">
    <header class="page-header">
      <h1>Patients List</h1>
      <button type="button" class="btn-add" @click="goToAdd">+ Add Patient</button>
    </header>

    <p v-if="loading" class="status-message">Loading patients…</p>
    <p v-else-if="error" class="status-message error">{{ error }}</p>

    <PatientTable
      v-else
      :patients="patients"
      @details="onDetails"
      @edit="onEdit"
      @delete="onDeleteRequest"
    />

    <ConfirmDeleteModal
      :visible="deleteModalVisible"
      :patient-name="patientToDeleteName"
      :deleting="deleting"
      @confirm="confirmDelete"
      @cancel="cancelDelete"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getPatients, deletePatient } from '../../services/patientService'
import PatientTable from '../../components/patients/PatientTable.vue'
import ConfirmDeleteModal from '../../components/patients/ConfirmDeleteModal.vue'
import type { Patient } from '../../types/patient'

const router = useRouter()

const patients = ref<Patient[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

const patientToDelete = ref<Patient | null>(null)
const deleteModalVisible = ref(false)
const deleting = ref(false)

const patientToDeleteName = computed(() => {
  if (!patientToDelete.value) return ''
  return `${patientToDelete.value.firstName} ${patientToDelete.value.lastName}`
})

const loadPatients = async () => {
  loading.value = true
  error.value = null

  try {
    const response = await getPatients()
    patients.value = response.data
  } catch (err) {
    console.error('Error loading patients:', err)
    error.value = 'Failed to load patients. Please try again later.'
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

  try {
    await deletePatient(patientToDelete.value.id)
    cancelDelete()
    await loadPatients()
  } catch (err) {
    console.error('Error deleting patient:', err)
    error.value = 'Failed to delete patient. Please try again.'
    cancelDelete()
  } finally {
    deleting.value = false
  }
}

onMounted(() => {
  loadPatients()
})
</script>

<style scoped>
.patients-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem 1.5rem;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}

h1 {
  margin: 0;
  font-size: 1.75rem;
  font-weight: 700;
  color: #111827;
}

.btn-add {
  padding: 0.625rem 1.25rem;
  border: none;
  border-radius: 8px;
  background-color: #2563eb;
  color: #ffffff;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.btn-add:hover {
  background-color: #1d4ed8;
}

.status-message {
  margin-top: 1.5rem;
  color: #6b7280;
}

.status-message.error {
  color: #dc2626;
}
</style>
