<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { getDoctorById } from '../../services/doctorService'
import type { Doctor } from '../../types/doctor'
import { getDoctorFullName } from '../../types/doctor'

const route = useRoute()

const doctor = ref<Doctor | null>(null)
const loading = ref(false)
const error = ref('')

const loadDoctor = async () => {
  const id = Number(route.params.id)

  if (Number.isNaN(id)) {
    error.value = 'Nieprawidłowe ID lekarza.'
    return
  }

  loading.value = true
  error.value = ''

  try {
    doctor.value = await getDoctorById(id)
  } catch {
    error.value = 'Nie udało się pobrać szczegółów lekarza.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadDoctor()
})
</script>

<template>
  <main class="app-page app-page--narrow">
    <header class="app-page-header">
      <div class="app-page-header__text">
        <h1 class="app-page-header__title">Szczegóły lekarza</h1>
        <p class="app-page-header__subtitle">
          Dane lekarza pracującego w klinice ortodontycznej
        </p>
      </div>

      <div class="app-page-header__actions">
        <RouterLink to="/doctors" class="app-btn app-btn--secondary">
          ← Lista
        </RouterLink>

        <RouterLink
          v-if="doctor"
          :to="`/doctors/${doctor.id}/edit`"
          class="app-btn app-btn--warning"
        >
          Edytuj
        </RouterLink>
      </div>
    </header>

    <section class="app-card app-details-card">
      <p v-if="loading">Ładowanie danych lekarza...</p>

      <p v-else-if="error" class="message message--error">
        {{ error }}
      </p>

      <template v-else-if="doctor">
        <div class="app-details-card__header">
          <h2 class="app-details-card__name">
            {{ getDoctorFullName(doctor) }}
          </h2>
          <p class="app-details-card__meta">
            ID lekarza: {{ doctor.id }}
          </p>
        </div>

        <div class="app-details-grid">
          <div class="app-detail-item">
            <span class="app-detail-item__label">Imię</span>
            <span class="app-detail-item__value">{{ doctor.firstName }}</span>
          </div>

          <div class="app-detail-item">
            <span class="app-detail-item__label">Nazwisko</span>
            <span class="app-detail-item__value">{{ doctor.lastName }}</span>
          </div>

          <div class="app-detail-item">
            <span class="app-detail-item__label">Specjalizacja</span>
            <span class="app-detail-item__value">{{ doctor.specialization }}</span>
          </div>
        </div>
      </template>
    </section>
  </main>
</template>

<style scoped>
.message {
  padding: 0.875rem 1rem;
  border-radius: var(--radius-md);
  font-weight: 500;
}

.message--error {
  background: rgba(220, 38, 38, 0.12);
  color: var(--color-danger);
}
</style>