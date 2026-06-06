<template>
  <PageLayout narrow>
    <PageHeader title="Szczegóły wizyty" subtitle="Podgląd danych wizyty">
      <template #actions>
        <button type="button" class="app-btn app-btn--secondary" @click="goBack">
          ← Wróć do listy
        </button>
        <button type="button" class="app-btn app-btn--primary" @click="goToEdit">
          Edytuj
        </button>
      </template>
    </PageHeader>

    <AppSpinner v-if="loading" label="Ładowanie..." />

    <AppAlert v-else-if="error" :message="error" variant="error" />

    <div v-else-if="appointment" class="app-card app-card--flat">
      <dl class="detail-list">
        <div class="detail-row">
          <dt>Pacjent</dt>
          <dd>{{ appointment.patientName }}</dd>
        </div>
        <div class="detail-row">
          <dt>Lekarz</dt>
          <dd>{{ appointment.doctorName }}</dd>
        </div>
        <div class="detail-row">
          <dt>Data wizyty</dt>
          <dd>{{ formatAppointmentDate(appointment.appointmentDate) }}</dd>
        </div>
        <div class="detail-row">
          <dt>Typ wizyty</dt>
          <dd>{{ appointment.visitType }}</dd>
        </div>
        <div class="detail-row">
          <dt>Status</dt>
          <dd>
            <span class="status-badge" :class="statusClass(appointment.status)">
              {{ appointment.status }}
            </span>
          </dd>
        </div>
        <div class="detail-row">
          <dt>Notatki</dt>
          <dd>{{ appointment.notes || '—' }}</dd>
        </div>
      </dl>
    </div>
  </PageLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import PageLayout from '../../components/common/PageLayout.vue'
import PageHeader from '../../components/common/PageHeader.vue'
import AppAlert from '../../components/common/AppAlert.vue'
import AppSpinner from '../../components/common/AppSpinner.vue'
import { getAppointmentById } from '../../services/appointmentService'
import type { Appointment } from '../../types/appointment'
import { formatAppointmentDate } from '../../types/appointment'
import { getApiErrorMessage } from '../../utils/apiError'

const props = defineProps<{ id: string }>()
const router = useRouter()
const loading = ref(true)
const error = ref<string | null>(null)
const appointment = ref<Appointment | null>(null)

const goBack = () => router.push({ name: 'appointments' })
const goToEdit = () => router.push({ name: 'appointment-edit', params: { id: props.id } })

const statusClass = (status: string) => ({
  'status-badge--pending': status === 'Do potwierdzenia',
  'status-badge--planned': status === 'Zaplanowana',
  'status-badge--done': status === 'Zakończona',
  'status-badge--cancelled': status === 'Anulowana',
})

onMounted(async () => {
  try {
    appointment.value = await getAppointmentById(Number(props.id))
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udało się załadować danych wizyty.')
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.detail-list { display: grid; gap: 0.75rem; }
.detail-row { display: grid; grid-template-columns: 160px 1fr; gap: 0.5rem; padding: 0.75rem 0; border-bottom: 1px solid var(--color-border); }
.detail-row:last-child { border-bottom: none; }
.detail-row dt { font-weight: 600; color: var(--color-text-muted); }

.status-badge { display: inline-block; padding: 0.2rem 0.6rem; border-radius: 999px; font-size: 0.78rem; font-weight: 600; }
.status-badge--pending { background: #fee2e2; color: #b91c1c; }
.status-badge--planned { background: #dbeafe; color: #1d4ed8; }
.status-badge--done { background: #dcfce7; color: #15803d; }
.status-badge--cancelled { background: #fee2e2; color: #b91c1c; }
</style>
