<template>
  <div class="page">
    <header class="page-header">
      <h1>Edit Patient</h1>
      <button type="button" class="btn-back" @click="goBack">← Back to list</button>
    </header>

    <p v-if="loading" class="status-message">Loading patient…</p>
    <p v-else-if="loadError" class="status-message error">{{ loadError }}</p>

    <div v-else class="card">
      <PatientForm
        :model-value="formData"
        submit-label="Save Changes"
        :submitting="submitting"
        :error="submitError"
        @submit="handleSubmit"
        @cancel="goBack"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PatientForm from '../../components/patients/PatientForm.vue'
import { getPatientById, updatePatient } from '../../services/patientService'
import type { PatientFormPayload } from '../../types/patient'
import { emptyPatientForm, toFormPayload } from '../../types/patient'

const route = useRoute()
const router = useRouter()

const formData = ref<PatientFormPayload>(emptyPatientForm())
const loading = ref(true)
const loadError = ref<string | null>(null)
const submitting = ref(false)
const submitError = ref<string | null>(null)

const patientId = Number(route.params.id)

const goBack = () => {
  router.push({ name: 'patients' })
}

const loadPatient = async () => {
  loading.value = true
  loadError.value = null

  try {
    const response = await getPatientById(patientId)
    formData.value = toFormPayload(response.data)
  } catch (err) {
    console.error('Error loading patient:', err)
    loadError.value = 'Failed to load patient data.'
  } finally {
    loading.value = false
  }
}

const handleSubmit = async (payload: PatientFormPayload) => {
  submitting.value = true
  submitError.value = null

  try {
    await updatePatient(patientId, payload)
    router.push({ name: 'patients' })
  } catch (err) {
    console.error('Error updating patient:', err)
    submitError.value = 'Failed to save changes. Please try again.'
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  if (Number.isNaN(patientId)) {
    loadError.value = 'Invalid patient ID.'
    loading.value = false
    return
  }
  loadPatient()
})
</script>

<style scoped>
.page {
  max-width: 800px;
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

.btn-back {
  padding: 0.5rem 1rem;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  background: #ffffff;
  color: #374151;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
}

.btn-back:hover {
  background-color: #f9fafb;
}

.card {
  margin-top: 1.5rem;
  padding: 1.5rem;
  background: #ffffff;
  border-radius: 12px;
  border: 1px solid #e5e7eb;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

.status-message {
  margin-top: 1.5rem;
  color: #6b7280;
}

.status-message.error {
  color: #dc2626;
}
</style>
