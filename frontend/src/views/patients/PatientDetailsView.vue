<template>
  <div class="page">
    <header class="page-header">
      <h1>Patient Details</h1>
      <div class="header-actions">
        <button type="button" class="btn-back" @click="goBack">← Back to list</button>
        <button
          v-if="patient"
          type="button"
          class="btn-edit"
          @click="goToEdit"
        >
          Edit
        </button>
      </div>
    </header>

    <p v-if="loading" class="status-message">Loading patient…</p>
    <p v-else-if="error" class="status-message error">{{ error }}</p>

    <dl v-else-if="patient" class="details-card">
      <div class="detail-row">
        <dt>First Name</dt>
        <dd>{{ patient.firstName }}</dd>
      </div>
      <div class="detail-row">
        <dt>Last Name</dt>
        <dd>{{ patient.lastName }}</dd>
      </div>
      <div class="detail-row">
        <dt>Phone</dt>
        <dd>{{ patient.phone || '—' }}</dd>
      </div>
      <div class="detail-row">
        <dt>Email</dt>
        <dd>{{ patient.email || '—' }}</dd>
      </div>
      <div class="detail-row">
        <dt>Birth Date</dt>
        <dd>{{ formatBirthDate(patient.birthDate) }}</dd>
      </div>
    </dl>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getPatientById } from '../../services/patientService'
import type { Patient } from '../../types/patient'
import { formatBirthDate } from '../../types/patient'

const route = useRoute()
const router = useRouter()

const patient = ref<Patient | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

const patientId = Number(route.params.id)

const goBack = () => {
  router.push({ name: 'patients' })
}

const goToEdit = () => {
  router.push({ name: 'patient-edit', params: { id: patientId } })
}

onMounted(async () => {
  if (Number.isNaN(patientId)) {
    error.value = 'Invalid patient ID.'
    loading.value = false
    return
  }

  try {
    const response = await getPatientById(patientId)
    patient.value = response.data
  } catch (err) {
    console.error('Error loading patient:', err)
    error.value = 'Failed to load patient details.'
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.page {
  max-width: 640px;
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

.header-actions {
  display: flex;
  gap: 0.5rem;
}

.btn-back,
.btn-edit {
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
}

.btn-back {
  border: 1px solid #d1d5db;
  background: #ffffff;
  color: #374151;
}

.btn-back:hover {
  background-color: #f9fafb;
}

.btn-edit {
  border: none;
  background-color: #d97706;
  color: #ffffff;
}

.btn-edit:hover {
  background-color: #b45309;
}

.details-card {
  margin: 1.5rem 0 0;
  padding: 1.5rem;
  background: #ffffff;
  border-radius: 12px;
  border: 1px solid #e5e7eb;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

.detail-row {
  display: grid;
  grid-template-columns: 140px 1fr;
  gap: 0.5rem 1rem;
  padding: 0.75rem 0;
  border-bottom: 1px solid #f3f4f6;
}

.detail-row:last-child {
  border-bottom: none;
}

dt {
  font-size: 0.8125rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: #6b7280;
}

dd {
  margin: 0;
  font-size: 0.9375rem;
  color: #111827;
}

.status-message {
  margin-top: 1.5rem;
  color: #6b7280;
}

.status-message.error {
  color: #dc2626;
}
</style>
