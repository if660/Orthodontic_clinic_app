<template>
  <PageLayout narrow>
    <PageHeader
      title="Dodaj pacjenta"
      subtitle="Wprowadź dane nowego pacjenta kliniki"
    >
      <template #actions>
        <button type="button" class="app-btn app-btn--secondary" @click="goBack">
          ← Wróć do listy
        </button>
      </template>
    </PageHeader>

    <div class="app-card app-card--flat">
      <PatientForm
        submit-label="Dodaj pacjenta"
        :submitting="submitting"
        :error="error"
        @submit="handleSubmit"
        @cancel="goBack"
      />
    </div>
  </PageLayout>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import PatientForm from '../../components/patients/PatientForm.vue'
import PageLayout from '../../components/common/PageLayout.vue'
import PageHeader from '../../components/common/PageHeader.vue'
import { createPatient } from '../../services/patientService'
import type { PatientFormPayload } from '../../types/patient'
import { getApiErrorMessage } from '../../utils/apiError'

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
    router.push({ name: 'patients', query: { flash: 'added' } })
  } catch (err) {
    console.error('Error creating patient:', err)
    error.value = getApiErrorMessage(err, 'Nie udało się dodać pacjenta. Spróbuj ponownie.')
  } finally {
    submitting.value = false
  }
}
</script>
