<template>
  <div class="table-container">
    <table class="patients-table">
      <thead>
        <tr>
          <th>First Name</th>
          <th>Last Name</th>
          <th>Phone</th>
          <th>Email</th>
          <th class="actions-header">Actions</th>
        </tr>
      </thead>

      <tbody>
        <tr v-if="patients.length === 0">
          <td colspan="5" class="empty-state">No patients found.</td>
        </tr>

        <tr v-for="patient in patients" :key="patient.id">
          <td>{{ patient.firstName }}</td>
          <td>{{ patient.lastName }}</td>
          <td>{{ patient.phone || '—' }}</td>
          <td>{{ patient.email || '—' }}</td>

          <td class="actions">
            <button type="button" class="details-btn" @click="emit('details', patient)">
              Details
            </button>
            <button type="button" class="edit-btn" @click="emit('edit', patient)">
              Edit
            </button>
            <button type="button" class="delete-btn" @click="emit('delete', patient)">
              Delete
            </button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import type { Patient } from '../../types/patient'

defineProps<{
  patients: Patient[]
}>()

const emit = defineEmits<{
  details: [patient: Patient]
  edit: [patient: Patient]
  delete: [patient: Patient]
}>()
</script>

<style scoped>
.table-container {
  margin-top: 1.5rem;
  padding: 1.5rem;
  background-color: #ffffff;
  border-radius: 12px;
  box-shadow:
    0 1px 3px rgba(0, 0, 0, 0.08),
    0 4px 12px rgba(0, 0, 0, 0.06);
  border: 1px solid #e5e7eb;
  overflow: hidden;
}

.patients-table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
}

.patients-table th {
  background-color: #1e40af;
  color: #ffffff;
  text-align: left;
  padding: 0.875rem 1rem;
  font-size: 0.8125rem;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.patients-table th:first-child {
  border-top-left-radius: 8px;
}

.patients-table th:last-child {
  border-top-right-radius: 8px;
}

.patients-table td {
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
  color: #374151;
  font-size: 0.9375rem;
}

.patients-table tbody tr:last-child td {
  border-bottom: none;
}

.patients-table tbody tr {
  transition: background-color 0.15s ease;
}

.patients-table tbody tr:hover:not(:has(.empty-state)) {
  background-color: #f8fafc;
}

.empty-state {
  text-align: center;
  color: #6b7280;
  padding: 2.5rem 1rem !important;
  font-style: italic;
}

.actions-header {
  text-align: center;
}

.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  justify-content: flex-end;
}

button {
  border: none;
  padding: 0.5rem 0.875rem;
  border-radius: 6px;
  cursor: pointer;
  color: #ffffff;
  font-size: 0.8125rem;
  font-weight: 600;
  transition:
    background-color 0.15s ease,
    transform 0.1s ease,
    box-shadow 0.15s ease;
}

button:hover {
  transform: translateY(-1px);
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);
}

button:active {
  transform: translateY(0);
}

.details-btn {
  background-color: #2563eb;
}

.details-btn:hover {
  background-color: #1d4ed8;
}

.edit-btn {
  background-color: #d97706;
}

.edit-btn:hover {
  background-color: #b45309;
}

.delete-btn {
  background-color: #dc2626;
}

.delete-btn:hover {
  background-color: #b91c1c;
}
</style>
