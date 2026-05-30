<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import AppIcon from '../../components/common/AppIcon.vue'
import DoctorDeleteModal from '../../components/doctors/DoctorDeleteModal.vue'
import DoctorTable from '../../components/doctors/DoctorTable.vue'
import { deleteDoctor, getDoctors } from '../../services/doctorService'
import type { Doctor } from '../../types/doctor'
import { getDoctorFullName } from '../../types/doctor'

const router = useRouter()

const doctors = ref<Doctor[]>([])
const loading = ref(false)
const deleting = ref(false)
const error = ref('')
const successMessage = ref('')
const searchQuery = ref('')
const selectedDoctor = ref<Doctor | null>(null)
const isDeleteModalVisible = ref(false)

const filteredDoctors = computed(() => {
  const query = searchQuery.value.toLowerCase().trim()

  if (!query) return doctors.value

  return doctors.value.filter((doctor) => {
    return (
      doctor.firstName.toLowerCase().includes(query) ||
      doctor.lastName.toLowerCase().includes(query) ||
      doctor.specialization.toLowerCase().includes(query)
    )
  })
})

const loadDoctors = async () => {
  loading.value = true
  error.value = ''

  try {
    doctors.value = await getDoctors()
  } catch {
    error.value = 'Nie udało się pobrać listy lekarzy.'
  } finally {
    loading.value = false
  }
}

const goToDetails = (doctor: Doctor) => {
  router.push(`/doctors/${doctor.id}`)
}

const goToEdit = (doctor: Doctor) => {
  router.push(`/doctors/${doctor.id}/edit`)
}

const openDeleteModal = (doctor: Doctor) => {
  selectedDoctor.value = doctor
  isDeleteModalVisible.value = true
}

const closeDeleteModal = () => {
  if (deleting.value) return

  selectedDoctor.value = null
  isDeleteModalVisible.value = false
}

const confirmDelete = async () => {
  if (!selectedDoctor.value) return

  deleting.value = true
  error.value = ''
  successMessage.value = ''

  try {
    await deleteDoctor(selectedDoctor.value.id)

    doctors.value = doctors.value.filter((item) => item.id !== selectedDoctor.value?.id)
    successMessage.value = 'Lekarz został usunięty pomyślnie.'

    selectedDoctor.value = null
    isDeleteModalVisible.value = false
  } catch {
    error.value = 'Nie udało się usunąć lekarza. Sprawdź, czy lekarz nie ma przypisanych wizyt.'
  } finally {
    deleting.value = false
  }
}

onMounted(() => {
  loadDoctors()
})
</script>

<template>
  <main class="app-page">
    <header class="app-page-header">
      <div class="app-page-header__text">
        <h1 class="app-page-header__title">Lekarze</h1>
        <p class="app-page-header__subtitle">
          Lista lekarzy pracujących w klinice ortodontycznej.
        </p>
      </div>

      <div class="app-page-header__actions">
        <RouterLink to="/doctors/new" class="app-btn app-btn--primary">
          <AppIcon name="plus" />
          <span>Dodaj lekarza</span>
        </RouterLink>
      </div>
    </header>

    <section class="app-toolbar">
      <div class="app-search">
        <span class="app-search__icon" aria-hidden="true">
          <AppIcon name="search" />
        </span>

        <input
          v-model="searchQuery"
          class="app-search__input"
          type="search"
          placeholder="Szukaj lekarza..."
          aria-label="Szukaj lekarza"
        />
      </div>
    </section>

    <p v-if="successMessage" class="message message--success">
      {{ successMessage }}
    </p>

    <p v-if="error" class="message message--error">
      {{ error }}
    </p>

    <section v-if="loading" class="app-card app-card--flat">
      <p>Ładowanie lekarzy...</p>
    </section>

    <DoctorTable
      v-else
      :doctors="filteredDoctors"
      empty-message="Brak lekarzy do wyświetlenia"
      @details="goToDetails"
      @edit="goToEdit"
      @delete="openDeleteModal"
    />

    <DoctorDeleteModal
      :visible="isDeleteModalVisible"
      :doctor-name="selectedDoctor ? getDoctorFullName(selectedDoctor) : ''"
      :deleting="deleting"
      @confirm="confirmDelete"
      @cancel="closeDeleteModal"
    />
  </main>
</template>

<style scoped>
.message {
  margin-bottom: 1rem;
  padding: 0.875rem 1rem;
  border-radius: var(--radius-md);
  font-weight: 500;
}

.message--success {
  background: rgba(5, 150, 105, 0.12);
  color: var(--color-success);
}

.message--error {
  background: rgba(220, 38, 38, 0.12);
  color: var(--color-danger);
}
</style>