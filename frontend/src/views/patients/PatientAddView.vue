<template>
  <div class="page">
    <header class="page-header">
      <h1>Add Patient</h1>
      <button type="button" class="btn-back" @click="goBack">← Back to list</button>
    </header>

    <div class="card">
      <PatientForm
        submit-label="Add Patient"
        :submitting="submitting"
        :error="error"
        @submit="handleSubmit"
        @cancel="goBack"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import PatientForm from '../../components/patients/PatientForm.vue'
import { createPatient } from '../../services/patientService'
import type { PatientFormPayload } from '../../types/patient'

const router = useRouter()
const submitting = ref(false)
const error = ref<string | null>(null)

const goBack = () => {
  router.push({ name: 'patients' })
}

const handleSubmit = async (payload: PatientFormPayload) => {
  submitting.value = true
  error.value = null

  try {
    await createPatient(payload)
    router.push({ name: 'patients' })
  } catch (err) {
    console.error('Error creating patient:', err)
    error.value = 'Failed to add patient. Please try again.'
  } finally {
    submitting.value = false
  }
}
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
</style>
