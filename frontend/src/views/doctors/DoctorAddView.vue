<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import DoctorForm from '../../components/doctors/DoctorForm.vue'
import { createDoctor } from '../../services/doctorService'
import { emptyDoctorForm } from '../../types/doctor'
import type { DoctorFormPayload } from '../../types/doctor'

const router = useRouter()

const form = ref<DoctorFormPayload>(emptyDoctorForm())
const submitting = ref(false)
const error = ref('')

const handleSubmit = async (payload: DoctorFormPayload) => {
  submitting.value = true
  error.value = ''

  try {
    await createDoctor(payload)
    router.push('/doctors')
  } catch {
    error.value = 'Nie udało się dodać lekarza.'
  } finally {
    submitting.value = false
  }
}

const handleCancel = () => {
  router.push('/doctors')
}
</script>

<template>
  <main class="app-page app-page--narrow">
    <header class="app-page-header">
      <div class="app-page-header__text">
        <h1 class="app-page-header__title">Dodawanie lekarza</h1>
        <p class="app-page-header__subtitle">
          Wprowadź dane nowego lekarza do systemu
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
      <DoctorForm
        :model-value="form"
        :submitting="submitting"
        submit-label="Dodaj lekarza"
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