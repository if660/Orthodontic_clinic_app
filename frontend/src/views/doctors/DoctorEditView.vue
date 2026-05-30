<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import DoctorForm from '../../components/doctors/DoctorForm.vue'
import { getDoctorById, updateDoctor } from '../../services/doctorService'
import { emptyDoctorForm, toDoctorFormPayload } from '../../types/doctor'
import type { DoctorFormPayload } from '../../types/doctor'

const route = useRoute()
const router = useRouter()

const form = ref<DoctorFormPayload>(emptyDoctorForm())
const loading = ref(false)
const submitting = ref(false)
const error = ref('')

const doctorId = Number(route.params.id)

const loadDoctor = async () => {
  if (Number.isNaN(doctorId)) {
    error.value = 'Nieprawidłowe ID lekarza.'
    return
  }

  loading.value = true
  error.value = ''

  try {
    const doctor = await getDoctorById(doctorId)
    form.value = toDoctorFormPayload(doctor)
  } catch {
    error.value = 'Nie udało się pobrać danych lekarza.'
  } finally {
    loading.value = false
  }
}

const handleSubmit = async (payload: DoctorFormPayload) => {
  submitting.value = true
  error.value = ''

  try {
    await updateDoctor(doctorId, payload)
    router.push(`/doctors/${doctorId}`)
  } catch {
    error.value = 'Nie udało się zapisać zmian.'
  } finally {
    submitting.value = false
  }
}

const handleCancel = () => {
  router.push('/doctors')
}

onMounted(() => {
  loadDoctor()
})
</script>

<template>
  <main class="app-page app-page--narrow">
    <header class="app-page-header">
      <div class="app-page-header__text">
        <h1 class="app-page-header__title">Edycja lekarza</h1>
        <p class="app-page-header__subtitle">
          Zaktualizuj dane lekarza w systemie
        </p>
      </div>

      <div class="app-page-header__actions">
        <RouterLink to="/doctors" class="app-btn app-btn--secondary">
          ← Wróć do listy
        </RouterLink>
      </div>
    </header>

    <p v-if="error" class="message message--error">
      {{ error }}
    </p>

    <section class="app-card">
      <p v-if="loading">Ładowanie danych lekarza...</p>

      <DoctorForm
        v-else
        :model-value="form"
        :submitting="submitting"
        submit-label="Zapisz zmiany"
        @submit="handleSubmit"
        @cancel="handleCancel"
      />
    </section>
  </main>
</template>

<style scoped>
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