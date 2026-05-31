<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { getPatients } from '../../services/patientService'
import { getDoctors } from '../../services/doctorService'
import type { Patient } from '../../types/patient'
import type { Doctor } from '../../types/doctor'

const patients = ref<Patient[]>([])
const doctors = ref<Doctor[]>([])
const loading = ref(false)
const error = ref('')

const totalPatients = computed(() => patients.value.length)
const totalDoctors = computed(() => doctors.value.length)

const specializations = computed(() => {
  const result: Record<string, number> = {}

  doctors.value.forEach((doctor) => {
    const specialization = doctor.specialization || 'Brak specjalizacji'
    result[specialization] = (result[specialization] || 0) + 1
  })

  return result
})

const totalSpecializations = computed(() => Object.keys(specializations.value).length)

const latestPatients = computed(() => {
  return patients.value.slice(-3).reverse()
})

const loadDashboardData = async () => {
  loading.value = true
  error.value = ''

  try {
    const [patientsData, doctorsData] = await Promise.all([
      getPatients(),
      getDoctors(),
    ])

    patients.value = patientsData
    doctors.value = doctorsData
  } catch {
    error.value = 'Nie udało się pobrać danych do strony głównej.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadDashboardData()
})
</script>

<template>
  <main class="app-page">
    <header class="app-page-header">
      <div class="app-page-header__text">
        <h1 class="app-page-header__title">Panel główny</h1>
        <p class="app-page-header__subtitle">
          Szybki podgląd najważniejszych informacji w systemie kliniki ortodontycznej.
        </p>
      </div>
    </header>

    <p v-if="error" class="message message--error">
      {{ error }}
    </p>

    <section v-if="loading" class="app-card app-card--flat">
      <p>Ładowanie danych...</p>
    </section>

    <template v-else>
      <section class="dashboard-grid">
        <RouterLink to="/patients" class="dashboard-card dashboard-card--link">
          <span class="dashboard-card__label">Pacjenci</span>
          <strong class="dashboard-card__value">{{ totalPatients }}</strong>
          <p class="dashboard-card__text">Liczba pacjentów zapisanych w systemie.</p>
        </RouterLink>

        <RouterLink to="/doctors" class="dashboard-card dashboard-card--link">
          <span class="dashboard-card__label">Lekarze</span>
          <strong class="dashboard-card__value">{{ totalDoctors }}</strong>
          <p class="dashboard-card__text">Liczba lekarzy dostępnych w klinice.</p>
        </RouterLink>

        <article class="dashboard-card">
          <span class="dashboard-card__label">Specjalizacje</span>
          <strong class="dashboard-card__value">{{ totalSpecializations }}</strong>
          <p class="dashboard-card__text">Liczba różnych specjalizacji lekarzy.</p>
        </article>
      </section>

      <section class="dashboard-sections">
        <article class="app-card app-card--flat">
          <h2 class="section-title">Lekarze według specjalizacji</h2>

          <p v-if="Object.keys(specializations).length === 0" class="empty-text">
            Brak danych o specjalizacjach.
          </p>

          <div v-else class="stats-list">
            <div
              v-for="(count, specialization) in specializations"
              :key="specialization"
              class="stats-row"
            >
              <span>{{ specialization }}</span>
              <strong>{{ count }}</strong>
            </div>
          </div>
        </article>

        <article class="app-card app-card--flat">
          <h2 class="section-title">Ostatnio dodani pacjenci</h2>

          <p v-if="latestPatients.length === 0" class="empty-text">
            Brak pacjentów do wyświetlenia.
          </p>

          <ul v-else class="patient-list">
            <li v-for="patient in latestPatients" :key="patient.id">
              <span>{{ patient.firstName }} {{ patient.lastName }}</span>
              <small>{{ patient.email || patient.phone || 'Brak danych kontaktowych' }}</small>
            </li>
          </ul>
        </article>
      </section>
    </template>
  </main>
</template>

<style scoped>
.dashboard-grid {
  display: grid;
  gap: 1rem;
  margin-bottom: var(--section-gap);
}

@media (min-width: 760px) {
  .dashboard-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

.dashboard-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-md);
  padding: 1.5rem;
  transition:
    transform var(--transition-base),
    box-shadow var(--transition-base),
    border-color var(--transition-base);
}

.dashboard-card--link {
  display: block;
  text-decoration: none;
  color: inherit;
  cursor: pointer;
}

.dashboard-card--link:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-lg);
  border-color: var(--color-primary);
}

.dashboard-card--link:hover .dashboard-card__label {
  color: var(--color-primary);
}

.dashboard-card__label {
  display: block;
  font-size: 0.8rem;
  font-weight: 700;
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.06em;
}

.dashboard-card__value {
  display: block;
  margin-top: 0.5rem;
  font-size: 2.25rem;
  color: var(--color-text);
}

.dashboard-card__text {
  margin: 0.5rem 0 0;
  color: var(--color-text-muted);
  line-height: 1.5;
}

.dashboard-sections {
  display: grid;
  gap: 1rem;
}

@media (min-width: 1000px) {
  .dashboard-sections {
    grid-template-columns: 1fr 1fr;
  }
}

.section-title {
  margin: 0 0 1rem;
  font-size: 1.125rem;
  font-weight: 700;
  color: var(--color-text);
}

.stats-list {
  display: grid;
  gap: 0.75rem;
}

.stats-row {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.875rem 1rem;
  background: #f8fafc;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
}

.stats-row span {
  color: var(--color-text);
}

.stats-row strong {
  color: var(--color-primary);
}

.patient-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: grid;
  gap: 0.75rem;
}

.patient-list li {
  display: grid;
  gap: 0.25rem;
  padding: 0.875rem 1rem;
  background: #f8fafc;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
}

.patient-list span {
  font-weight: 600;
  color: var(--color-text);
}

.patient-list small,
.empty-text {
  color: var(--color-text-muted);
}

.message {
  margin-bottom: 1rem;
  padding: 0.875rem 1rem;
  border-radius: var(--radius-md);
  font-weight: 500;
}

.message--error {
  background: rgba(220, 38, 38, 0.12);
  color: var(--color-danger);
}
</style>