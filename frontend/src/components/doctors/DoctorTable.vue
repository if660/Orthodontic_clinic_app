<template>
  <div class="table-wrapper app-card app-card--flat">
    <div class="table-scroll">
      <table class="doctors-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Imię i nazwisko</th>
            <th>Specjalizacja</th>
            <th class="actions-header">Akcje</th>
          </tr>
        </thead>

        <tbody>
          <tr v-if="doctors.length === 0">
            <td colspan="4" class="empty-state">{{ emptyMessage }}</td>
          </tr>

          <tr v-for="doctor in doctors" :key="doctor.id">
            <td>{{ doctor.id }}</td>
            <td>{{ getDoctorFullName(doctor) }}</td>
            <td>{{ doctor.specialization || '—' }}</td>

            <td class="actions">
              <button
                type="button"
                class="action-btn details-btn"
                title="Szczegóły"
                @click="emit('details', doctor)"
              >
                <AppIcon name="eye" size="md" />
                <span class="btn-text">Szczegóły</span>
              </button>

              <button
                type="button"
                class="action-btn edit-btn"
                title="Edytuj"
                @click="emit('edit', doctor)"
              >
                <AppIcon name="pencil" size="md" />
                <span class="btn-text">Edytuj</span>
              </button>

              <button
                type="button"
                class="action-btn delete-btn"
                title="Usuń"
                @click="emit('delete', doctor)"
              >
                <AppIcon name="trash" size="md" />
                <span class="btn-text">Usuń</span>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import AppIcon from '../common/AppIcon.vue'
import type { Doctor } from '../../types/doctor'
import { getDoctorFullName } from '../../types/doctor'

withDefaults(
  defineProps<{
    doctors: Doctor[]
    emptyMessage?: string
  }>(),
  {
    emptyMessage: 'Brak lekarzy',
  },
)

const emit = defineEmits<{
  details: [doctor: Doctor]
  edit: [doctor: Doctor]
  delete: [doctor: Doctor]
}>()
</script>

<style scoped>
.table-wrapper {
  padding: 0;
  overflow: hidden;
}

.table-scroll {
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
}

.doctors-table {
  width: 100%;
  min-width: 720px;
  border-collapse: separate;
  border-spacing: 0;
}

.doctors-table th {
  background: linear-gradient(180deg, #f8fafc 0%, #f1f5f9 100%);
  color: #475569;
  text-align: left;
  padding: 0.875rem 1.125rem;
  font-size: 0.75rem;
  font-weight: 600;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  border-bottom: 1px solid var(--color-border);
  white-space: nowrap;
}

.doctors-table td {
  padding: 1rem 1.125rem;
  border-bottom: 1px solid #f1f5f9;
  color: #334155;
  font-size: 0.9375rem;
  line-height: 1.45;
}

.doctors-table tbody tr:last-child td {
  border-bottom: none;
}

.doctors-table tbody tr:hover:not(:has(.empty-state)) {
  background-color: #f8fafc;
}

.empty-state {
  text-align: center;
  color: var(--color-text-muted);
  padding: 3rem 1rem !important;
  font-size: 0.9375rem;
}

.actions-header {
  text-align: right;
}

.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  justify-content: flex-end;
}

.action-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  border: none;
  padding: 0.5rem 0.875rem;
  border-radius: var(--radius-sm);
  cursor: pointer;
  color: #fff;
  font-size: 0.8125rem;
  font-weight: 600;
  font-family: inherit;
  box-shadow: 0 1px 3px rgba(15, 23, 42, 0.12);
  transition:
    background-color 0.2s ease,
    transform 0.2s ease,
    box-shadow 0.2s ease;
}

.action-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(15, 23, 42, 0.18);
}

.details-btn {
  background-color: var(--color-details);
}

.details-btn:hover {
  background-color: #1d4ed8;
}

.edit-btn {
  background-color: var(--color-edit);
}

.edit-btn:hover {
  background-color: #b45309;
}

.delete-btn {
  background-color: var(--color-delete);
}

.delete-btn:hover {
  background-color: #b91c1c;
}
</style>