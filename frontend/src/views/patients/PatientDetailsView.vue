<template>
  <PageLayout narrow>
    <PageHeader
      title="Szczegóły pacjenta"
      subtitle="Pełne dane kontaktowe i informacje osobowe"
    >
      <template #actions>
        <button type="button" class="app-btn app-btn--secondary app-btn--sm" @click="goBack">
          ← Lista
        </button>
        <button
          v-if="patient"
          type="button"
          class="app-btn app-btn--warning app-btn--sm app-btn--responsive-text"
          @click="goToEdit"
        >
          <AppIcon name="pencil" />
          <span class="app-btn__text">Edytuj</span>
        </button>
      </template>
    </PageHeader>

    <AppSpinner v-if="loading" label="Ładowanie..." />

    <AppAlert v-else-if="error" :message="error" variant="error" :dismissible="false" />

    <div v-else-if="patient" class="app-card app-details-card">
      <div class="app-details-card__header">
        <h2 class="app-details-card__name">{{ fullName }}</h2>
        <p class="app-details-card__meta">ID pacjenta: {{ patient.id }}</p>
      </div>

      <div class="app-details-grid">
        <div class="app-detail-item">
          <span class="app-detail-item__label">Imię</span>
          <span class="app-detail-item__value">{{ patient.firstName || '—' }}</span>
        </div>
        <div class="app-detail-item">
          <span class="app-detail-item__label">Nazwisko</span>
          <span class="app-detail-item__value">{{ patient.lastName || '—' }}</span>
        </div>
        <div class="app-detail-item">
          <span class="app-detail-item__label">Telefon</span>
          <span class="app-detail-item__value">{{ patient.phone || '—' }}</span>
        </div>
        <div class="app-detail-item">
          <span class="app-detail-item__label">E-mail</span>
          <span class="app-detail-item__value">{{ patient.email || '—' }}</span>
        </div>
        <div class="app-detail-item">
          <span class="app-detail-item__label">Data urodzenia</span>
          <span class="app-detail-item__value">{{ formatBirthDate(patient.birthDate) }}</span>
        </div>
      </div>
    </div>
  </PageLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageLayout from '../../components/common/PageLayout.vue'
import PageHeader from '../../components/common/PageHeader.vue'
import AppAlert from '../../components/common/AppAlert.vue'
import AppSpinner from '../../components/common/AppSpinner.vue'
import AppIcon from '../../components/common/AppIcon.vue'
import { getPatientById } from '../../services/patientService'
import type { Patient } from '../../types/patient'
import { formatBirthDate, getPatientFullName } from '../../types/patient'
import { getApiErrorMessage } from '../../utils/apiError'

const route = useRoute()
const router = useRouter()

const patient = ref<Patient | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

const patientId = Number(route.params.id)

const fullName = computed(() =>
  patient.value ? getPatientFullName(patient.value) || '—' : '—',
)

const goBack = () => {
  router.push({ name: 'patients' })
}

const goToEdit = () => {
  router.push({ name: 'patient-edit', params: { id: patientId } })
}

onMounted(async () => {
  if (Number.isNaN(patientId)) {
    error.value = 'Nieprawidłowy identyfikator pacjenta.'
    loading.value = false
    return
  }

  try {
    patient.value = await getPatientById(patientId)
  } catch (err) {
    console.error('Error loading patient:', err)
    error.value = getApiErrorMessage(err, 'Nie udało się załadować szczegółów pacjenta')
    patient.value = null
  } finally {
    loading.value = false
  }
})
</script>
