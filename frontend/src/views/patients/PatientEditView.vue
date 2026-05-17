<template>
  <PageLayout narrow>
    <PageHeader
      title="Edycja pacjenta"
      subtitle="Zaktualizuj dane pacjenta w systemie"
    >
      <template #actions>
        <button type="button" class="app-btn app-btn--secondary" @click="goBack">
          ← Wróć do listy
        </button>
      </template>
    </PageHeader>

    <AppSpinner v-if="loading" label="Ładowanie..." />

    <AppAlert v-else-if="loadError" :message="loadError" variant="error" :dismissible="false" />

    <div v-else class="app-card app-card--flat">
      <PatientForm
        :model-value="formData"
        submit-label="Zapisz zmiany"
        :submitting="submitting"
        :error="submitError"
        @submit="handleSubmit"
        @cancel="goBack"
      />
    </div>
  </PageLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PatientForm from '../../components/patients/PatientForm.vue'
import PageLayout from '../../components/common/PageLayout.vue'
import PageHeader from '../../components/common/PageHeader.vue'
import AppAlert from '../../components/common/AppAlert.vue'
import AppSpinner from '../../components/common/AppSpinner.vue'
import { getPatientById, updatePatient } from '../../services/patientService'
import type { PatientFormPayload } from '../../types/patient'
import { emptyPatientForm, toFormPayload } from '../../types/patient'
import { getApiErrorMessage } from '../../utils/apiError'

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
    const patient = await getPatientById(patientId)
    formData.value = toFormPayload(patient)
  } catch (err) {
    console.error('Error loading patient:', err)
    loadError.value = getApiErrorMessage(err, 'Nie udało się załadować danych pacjenta')
  } finally {
    loading.value = false
  }
}

const handleSubmit = async (payload: PatientFormPayload) => {
  submitting.value = true
  submitError.value = null

  try {
    await updatePatient(patientId, payload)
    router.push({ name: 'patients', query: { flash: 'updated' } })
  } catch (err) {
    console.error('Error updating patient:', err)
    submitError.value = getApiErrorMessage(err, 'Nie udało się zapisać zmian. Spróbuj ponownie.')
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  if (Number.isNaN(patientId)) {
    loadError.value = 'Nieprawidłowy identyfikator pacjenta.'
    loading.value = false
    return
  }
  loadPatient()
})
</script>
