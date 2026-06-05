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
    <AppAlert v-if="successMessage" :message="successMessage" variant="success" @dismiss="successMessage = null" />

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

        <div class="app-detail-item app-detail-item--full">
          <span class="app-detail-item__label">Opiekun prawny</span>
          <span class="app-detail-item__value">{{ patient.guardianFullName || 'Nie dotyczy' }}</span>
        </div>

        <div class="app-detail-item">
          <span class="app-detail-item__label">Telefon opiekuna</span>
          <span class="app-detail-item__value">{{ patient.guardianPhone || '—' }}</span>
        </div>

        <div class="app-detail-item">
          <span class="app-detail-item__label">E-mail opiekuna</span>
          <span class="app-detail-item__value">{{ patient.guardianEmail || '—' }}</span>
        </div>
      </div>

      <section class="app-details-section">
        <h3 class="app-details-section__title">Historia i plan wizyt</h3>

        <p v-if="nextAppointment" class="next-appointment">
          Najblizsza wizyta: <strong>{{ toReadableDate(nextAppointment.appointmentDate) }}</strong>
          • {{ nextAppointment.visitType }} • {{ nextAppointment.doctorName }}
        </p>

        <p v-if="patientAppointments.length === 0" class="app-detail-item__value">
          Brak wizyt przypisanych do pacjenta.
        </p>

        <ul v-else class="appointment-history">
          <li v-for="appointment in patientAppointments" :key="appointment.id" class="appointment-history__item">
            <div class="appointment-history__main">
              <strong>{{ toReadableDate(appointment.appointmentDate) }}</strong>
              <p>{{ appointment.doctorName }} • {{ appointment.visitType }}</p>
            </div>

            <div class="appointment-history__status-area">
              <span class="status-chip" :class="statusClass(appointment.status)">{{ appointment.status }}</span>

              <div class="status-editor">
                <select
                  class="status-editor__select"
                  :value="getDraftStatus(appointment.id, appointment.status)"
                  :disabled="updatingAppointmentId === appointment.id"
                  @change="onDraftStatusChange(appointment.id, ($event.target as HTMLSelectElement).value)"
                >
                  <option v-for="status in statusOptions" :key="status" :value="status">
                    {{ status }}
                  </option>
                </select>

                <button
                  type="button"
                  class="app-btn app-btn--secondary app-btn--sm"
                  :disabled="updatingAppointmentId === appointment.id || getDraftStatus(appointment.id, appointment.status) === appointment.status"
                  @click="saveAppointmentStatus(appointment.id, appointment.status)"
                >
                  {{ updatingAppointmentId === appointment.id ? 'Zapisywanie...' : 'Zapisz status' }}
                </button>
              </div>
            </div>
          </li>
        </ul>
      </section>

      <section class="app-details-section">
        <h3 class="app-details-section__title">Dokumenty pacjenta</h3>

        <AppAlert
          v-if="docError"
          :message="docError"
          variant="error"
          @dismiss="docError = null"
        />
        <AppAlert
          v-if="docSuccess"
          :message="docSuccess"
          variant="success"
          @dismiss="docSuccess = null"
        />

        <div class="doc-upload-bar">
          <label class="doc-upload-label">
            <span class="app-btn app-btn--secondary app-btn--sm">
              {{ uploading ? 'Przesyłanie...' : '📎 Dodaj dokument' }}
            </span>
            <input
              ref="fileInputRef"
              type="file"
              class="doc-file-input"
              accept=".pdf,.jpg,.jpeg,.png,.gif,.doc,.docx"
              :disabled="uploading"
              @change="onFileSelected"
            />
          </label>
          <span class="doc-upload-hint">PDF, JPG, PNG, DOC — maks. 10 MB</span>
        </div>

        <div v-if="docDescriptionPending" class="doc-desc-row">
          <input
            v-model="pendingDescription"
            type="text"
            class="doc-desc-input"
            placeholder="Opis dokumentu (opcjonalnie)"
          />
          <button class="app-btn app-btn--primary app-btn--sm" :disabled="uploading" @click="confirmUpload">
            {{ uploading ? 'Przesyłanie...' : 'Prześlij' }}
          </button>
          <button class="app-btn app-btn--ghost app-btn--sm" @click="cancelUpload">
            Anuluj
          </button>
        </div>

        <p v-if="docsLoading" class="app-detail-item__value">Ładowanie dokumentów...</p>

        <p v-else-if="documents.length === 0" class="app-detail-item__value">
          Brak dokumentów. Kliknij „Dodaj dokument", aby przesłać plik.
        </p>

        <ul v-else class="doc-list">
          <li v-for="doc in documents" :key="doc.id" class="doc-item">
            <span class="doc-item__icon">{{ fileTypeIcon(doc.contentType) }}</span>
            <div class="doc-item__info">
              <strong class="doc-item__name">{{ doc.fileName }}</strong>
              <span class="doc-item__meta">
                {{ formatFileSize(doc.fileSize) }} •
                {{ new Date(doc.uploadedAt).toLocaleDateString('pl-PL') }}
                <span v-if="doc.description"> • {{ doc.description }}</span>
              </span>
            </div>
            <div class="doc-item__actions">
              <button
                class="app-btn app-btn--secondary app-btn--sm"
                @click="downloadDoc(doc)"
              >
                ⬇ Pobierz
              </button>
              <button
                class="app-btn app-btn--danger app-btn--sm"
                :disabled="deletingDocId === doc.id"
                @click="deleteDoc(doc.id)"
              >
                {{ deletingDocId === doc.id ? '...' : 'Usuń' }}
              </button>
            </div>
          </li>
        </ul>
      </section>

      <section class="app-details-section app-details-section--danger">
        <h3 class="app-details-section__title">Strefa zagrożenia</h3>
        <p class="app-detail-item__value">Operacje na koncie pacjenta, które nie mogą być cofnięte.</p>
        <button
          type="button"
          class="app-btn app-btn--danger app-btn--sm"
          @click="showDeleteConfirm"
        >
          <AppIcon name="trash" />
          <span>Usuń pacjenta</span>
        </button>
      </section>
    </div>

    <div v-if="deleteConfirmVisible" class="modal-backdrop">
      <div class="modal">
        <div class="modal__header">
          <h3>Usuń pacjenta</h3>
        </div>
        <div class="modal__body">
          <p>Jesteś pewny, że chcesz usunąć pacjenta <strong>{{ fullName }}</strong>?</p>
          <p class="modal__warning">Tej operacji nie można cofnąć. Wszystkie dane pacjenta zostaną trwale usunięte.</p>
        </div>
        <div class="modal__footer">
          <button type="button" class="app-btn app-btn--secondary" @click="cancelDelete" :disabled="isDeleting">
            Anuluj
          </button>
          <button type="button" class="app-btn app-btn--danger" @click="confirmDelete" :disabled="isDeleting">
            {{ isDeleting ? 'Usuwanie...' : 'Usuń pacjenta' }}
          </button>
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
import { getPatientById, deletePatient } from '../../services/patientService'
import { getAppointments, updateAppointmentStatus } from '../../services/appointmentService'
import { getPatientDocuments, uploadDocument, downloadDocument, deleteDocument } from '../../services/documentService'
import type { Patient } from '../../types/patient'
import { formatBirthDate, getPatientFullName, getFlashMessage } from '../../types/patient'
import type { Appointment, AppointmentStatus } from '../../types/appointment'
import { APPOINTMENT_STATUS_OPTIONS, toReadableDate } from '../../types/appointment'
import type { PatientDocument } from '../../types/document'
import { formatFileSize, fileTypeIcon } from '../../types/document'
import { getApiErrorMessage } from '../../utils/apiError'

const route = useRoute()
const router = useRouter()

const patient = ref<Patient | null>(null)
const appointments = ref<Appointment[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const successMessage = ref<string | null>(null)
const updatingAppointmentId = ref<number | null>(null)
const draftStatuses = ref<Record<number, string>>({})
const statusOptions = APPOINTMENT_STATUS_OPTIONS
const deleteConfirmVisible = ref(false)
const isDeleting = ref(false)

const patientId = Number(route.params.id)

// Documents
const documents = ref<PatientDocument[]>([])
const docsLoading = ref(false)
const docError = ref<string | null>(null)
const docSuccess = ref<string | null>(null)
const uploading = ref(false)
const deletingDocId = ref<number | null>(null)
const fileInputRef = ref<HTMLInputElement | null>(null)
const pendingFile = ref<File | null>(null)
const pendingDescription = ref('')
const docDescriptionPending = ref(false)

const loadDocuments = async () => {
  docsLoading.value = true
  try {
    documents.value = await getPatientDocuments(patientId)
  } catch {
    // non-critical, ignore
  } finally {
    docsLoading.value = false
  }
}

const onFileSelected = (event: Event) => {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  pendingFile.value = file
  pendingDescription.value = ''
  docDescriptionPending.value = true
}

const cancelUpload = () => {
  pendingFile.value = null
  docDescriptionPending.value = false
  if (fileInputRef.value) fileInputRef.value.value = ''
}

const confirmUpload = async () => {
  if (!pendingFile.value) return
  uploading.value = true
  docError.value = null
  try {
    const doc = await uploadDocument(patientId, pendingFile.value, pendingDescription.value || undefined)
    documents.value.unshift(doc)
    docSuccess.value = `Plik „${doc.fileName}" został przesłany pomyślnie.`
    cancelUpload()
  } catch (err) {
    docError.value = getApiErrorMessage(err, 'Nie udało się przesłać pliku.')
  } finally {
    uploading.value = false
  }
}

const downloadDoc = async (doc: PatientDocument) => {
  try {
    await downloadDocument(doc.id, doc.fileName)
  } catch {
    docError.value = 'Nie udało się pobrać pliku.'
  }
}

const deleteDoc = async (id: number) => {
  deletingDocId.value = id
  docError.value = null
  try {
    await deleteDocument(id)
    documents.value = documents.value.filter((d) => d.id !== id)
    docSuccess.value = 'Dokument został usunięty.'
  } catch (err) {
    docError.value = getApiErrorMessage(err, 'Nie udało się usunąć dokumentu.')
  } finally {
    deletingDocId.value = null
  }
}

const fullName = computed(() =>
  patient.value ? getPatientFullName(patient.value) || '—' : '—',
)

const patientAppointments = computed(() => {
  return appointments.value
    .filter((appointment) => appointment.patientId === patientId)
    .sort((a, b) => a.appointmentDate.localeCompare(b.appointmentDate))
})

const nextAppointment = computed(() => {
  const now = new Date()
  return patientAppointments.value.find((appointment) => new Date(appointment.appointmentDate) >= now) ?? null
})

const statusClass = (status: string) => {
  const normalized = status.toLowerCase()

  if (normalized === 'potwierdzona' || normalized === 'pacjent przyszedl' || normalized === 'zakonczona') {
    return 'status-chip--success'
  }

  if (normalized === 'odwolana' || normalized === 'nieobecnosc') {
    return 'status-chip--danger'
  }

  if (normalized === 'przelozona') {
    return 'status-chip--warning'
  }

  return 'status-chip--default'
}

const getDraftStatus = (appointmentId: number, currentStatus: string) => {
  return draftStatuses.value[appointmentId] ?? currentStatus
}

const onDraftStatusChange = (appointmentId: number, value: string) => {
  draftStatuses.value = {
    ...draftStatuses.value,
    [appointmentId]: value,
  }
}

const saveAppointmentStatus = async (appointmentId: number, currentStatus: string) => {
  const nextStatus = getDraftStatus(appointmentId, currentStatus)
  if (nextStatus === currentStatus) return

  updatingAppointmentId.value = appointmentId
  successMessage.value = null
  error.value = null

  try {
    await updateAppointmentStatus(appointmentId, nextStatus as AppointmentStatus)

    appointments.value = appointments.value.map((appointment) => (
      appointment.id === appointmentId
        ? { ...appointment, status: nextStatus }
        : appointment
    ))

    successMessage.value = 'Status wizyty został zaktualizowany i zapisany w historii pacjenta.'
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udało się zaktualizować statusu wizyty')
  } finally {
    updatingAppointmentId.value = null
  }
}

const goBack = () => {
  router.push({ name: 'patients' })
}

const goToEdit = () => {
  router.push({ name: 'patient-edit', params: { id: patientId } })
}

const showDeleteConfirm = () => {
  deleteConfirmVisible.value = true
}

const cancelDelete = () => {
  deleteConfirmVisible.value = false
}

const confirmDelete = async () => {
  if (!patient.value) return

  isDeleting.value = true
  error.value = null

  try {
    await deletePatient(patientId)
    successMessage.value = null
    await router.push({ name: 'patients', query: { flash: 'deleted' } })
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Nie udało się usunąć pacjenta. Spróbuj ponownie.')
    deleteConfirmVisible.value = false
  } finally {
    isDeleting.value = false
  }
}

onMounted(async () => {
  if (Number.isNaN(patientId)) {
    error.value = 'Nieprawidłowy identyfikator pacjenta.'
    loading.value = false
    return
  }

  try {
    const [patientData, appointmentData] = await Promise.all([
      getPatientById(patientId),
      getAppointments(),
    ])

    patient.value = patientData
    appointments.value = appointmentData
    draftStatuses.value = Object.fromEntries(appointmentData.map((appointment) => [appointment.id, appointment.status]))
    await loadDocuments()
  } catch (err) {
    console.error('Error loading patient:', err)
    error.value = getApiErrorMessage(err, 'Nie udało się załadować szczegółów pacjenta')
    patient.value = null
    appointments.value = []
    draftStatuses.value = {}
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.app-details-section {
  margin-top: 1.5rem;
  border-top: 1px solid var(--color-border);
  padding-top: 1rem;
}

.app-details-section__title {
  margin: 0 0 0.75rem;
  font-size: 1rem;
}

.next-appointment {
  margin: 0 0 0.75rem;
  color: var(--color-primary);
}

.appointment-history {
  list-style: none;
  padding: 0;
  margin: 0;
  display: grid;
  gap: 0.5rem;
}

.appointment-history__item {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: 0.75rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.75rem;
  background: #f8fafc;
}

.appointment-history__main {
  display: grid;
}

.appointment-history__status-area {
  display: grid;
  justify-items: end;
  gap: 0.45rem;
}

.appointment-history__item p {
  margin: 0.2rem 0 0;
  color: var(--color-text-muted);
}

.status-chip {
  border-radius: 999px;
  padding: 0.2rem 0.6rem;
  border: 1px solid transparent;
  font-size: 0.8rem;
  white-space: nowrap;
  font-weight: 700;
}

.status-chip--default {
  background: #e2e8f0;
  color: #1e293b;
  border-color: #cbd5e1;
}

.status-chip--success {
  background: #dcfce7;
  color: #166534;
  border-color: #86efac;
}

.status-chip--warning {
  background: #fef3c7;
  color: #92400e;
  border-color: #fcd34d;
}

.status-chip--danger {
  background: #fee2e2;
  color: #991b1b;
  border-color: #fca5a5;
}

.status-editor {
  display: flex;
  align-items: center;
  gap: 0.45rem;
}

.status-editor__select {
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-sm);
  background: #fff;
  padding: 0.35rem 0.45rem;
  font: inherit;
  font-size: 0.85rem;
}

.app-details-section--danger {
  background: #fef2f2;
  border-left: 4px solid #dc2626;
  padding: 1rem;
  border-radius: var(--radius-md);
}

.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: white;
  border-radius: var(--radius-lg);
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
  max-width: 400px;
  width: 90%;
  overflow: hidden;
}

.modal__header {
  padding: 1.5rem;
  border-bottom: 1px solid var(--color-border);
}

.modal__header h3 {
  margin: 0;
  font-size: 1.1rem;
}

.modal__body {
  padding: 1.5rem;
}

.modal__body p {
  margin: 0 0 0.75rem;
}

.modal__warning {
  background: #fef2f2;
  border-left: 3px solid #dc2626;
  padding: 0.75rem;
  margin: 1rem 0 0;
  font-size: 0.9rem;
  border-radius: var(--radius-sm);
}

.modal__footer {
  padding: 1rem 1.5rem;
  border-top: 1px solid var(--color-border);
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
}

@media (max-width: 760px) {
  .appointment-history__item {
    flex-direction: column;
    align-items: stretch;
  }

  .appointment-history__status-area {
    justify-items: start;
  }

  .status-editor {
    width: 100%;
  }

  .status-editor__select {
    flex: 1;
  }
}

/* ---- Documents section ---- */
.doc-upload-bar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
  flex-wrap: wrap;
}

.doc-upload-label {
  cursor: pointer;
}

.doc-file-input {
  display: none;
}

.doc-upload-hint {
  font-size: 0.78rem;
  color: var(--color-text-muted);
}

.doc-desc-row {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  margin-bottom: 0.75rem;
  flex-wrap: wrap;
}

.doc-desc-input {
  flex: 1;
  min-width: 180px;
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-sm);
  padding: 0.4rem 0.6rem;
  font: inherit;
  font-size: 0.85rem;
}

.doc-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.doc-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.65rem 0.85rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: #f8fafc;
}

.doc-item__icon {
  font-size: 1.6rem;
  flex-shrink: 0;
}

.doc-item__info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.doc-item__name {
  font-size: 0.88rem;
  color: var(--color-text);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.doc-item__meta {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.doc-item__actions {
  display: flex;
  gap: 0.4rem;
  flex-shrink: 0;
}
</style>
