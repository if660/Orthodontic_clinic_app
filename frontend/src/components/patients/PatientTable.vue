<template>
  <div class="table-wrapper app-card app-card--flat">
    <div class="table-scroll">
      <table class="patients-table">
        <thead>
          <tr>
            <th>Imię</th>
            <th>Nazwisko</th>
            <th>Status</th>
            <th>Telefon</th>
            <th>E-mail</th>
            <th class="actions-header">Akcje</th>
          </tr>
        </thead>

        <tbody>
          <tr v-if="patients.length === 0">
            <td colspan="6" class="empty-state">{{ emptyMessage }}</td>
          </tr>

          <tr v-for="patient in patients" :key="patient.id">
            <td data-label="Imię">{{ patient.firstName || '—' }}</td>
            <td data-label="Nazwisko">{{ patient.lastName || '—' }}</td>
            <td data-label="Status">
              <div class="patient-meta">
                <span
                  class="age-badge"
                  :class="isPatientMinorFromBirthDate(patient.birthDate) ? 'age-badge--minor' : 'age-badge--adult'"
                >
                  {{ getPatientCategoryLabel(patient) }}
                </span>
                <small class="age-value">{{ getAgeText(patient.birthDate) }}</small>
              </div>
            </td>
            <td data-label="Telefon">{{ patient.phone || '—' }}</td>
            <td data-label="E-mail">{{ patient.email || '—' }}</td>

            <td class="actions" data-label="Akcje">
              <button
                type="button"
                class="action-btn details-btn"
                title="Szczegóły"
                @click="emit('details', patient)"
              >
                <AppIcon name="eye" size="md" />
                <span class="btn-text">Szczegóły</span>
              </button>
              <button
                type="button"
                class="action-btn edit-btn"
                title="Edytuj"
                @click="emit('edit', patient)"
              >
                <AppIcon name="pencil" size="md" />
                <span class="btn-text">Edytuj</span>
              </button>
              <button
                type="button"
                class="action-btn calendar-btn"
                title="Kalendarz"
                @click="emit('appointments', patient)"
              >
                <AppIcon name="calendar" size="md" />
                <span class="btn-text">Kalendarz</span>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Karty na mobile -->
    <div class="mobile-cards">
      <p v-if="patients.length === 0" class="empty-state-mobile">{{ emptyMessage }}</p>

      <article v-for="patient in patients" :key="patient.id" class="patient-card">
        <div class="patient-card__header">
          <h3>{{ patient.firstName }} {{ patient.lastName }}</h3>
        </div>
        <dl class="patient-card__body">
          <div>
            <dt>Status</dt>
            <dd>
              <span
                class="age-badge"
                :class="isPatientMinorFromBirthDate(patient.birthDate) ? 'age-badge--minor' : 'age-badge--adult'"
              >
                {{ getPatientCategoryLabel(patient) }}
              </span>
              <small class="age-value">{{ getAgeText(patient.birthDate) }}</small>
            </dd>
          </div>
          <div>
            <dt>Telefon</dt>
            <dd>{{ patient.phone || '—' }}</dd>
          </div>
          <div>
            <dt>E-mail</dt>
            <dd>{{ patient.email || '—' }}</dd>
          </div>
        </dl>
        <div class="patient-card__actions">
          <button type="button" class="action-btn details-btn" title="Szczegóły" @click="emit('details', patient)">
            <AppIcon name="eye" size="md" />
          </button>
          <button type="button" class="action-btn edit-btn" title="Edytuj" @click="emit('edit', patient)">
            <AppIcon name="pencil" size="md" />
          </button>
          <button type="button" class="action-btn calendar-btn" title="Kalendarz" @click="emit('appointments', patient)">
            <AppIcon name="calendar" size="md" />
          </button>
        </div>
      </article>
    </div>
  </div>
</template>

<script setup lang="ts">
import AppIcon from '../common/AppIcon.vue'
import type { Patient } from '../../types/patient'
import {
  getPatientAge,
  getPatientCategoryLabel,
  isPatientMinorFromBirthDate,
} from '../../types/patient'

withDefaults(
  defineProps<{
    patients: Patient[]
    emptyMessage?: string
  }>(),
  {
    emptyMessage: 'Brak pacjentów',
  },
)

const emit = defineEmits<{
  details: [patient: Patient]
  edit: [patient: Patient]
  appointments: [patient: Patient]
}>()

const getAgeText = (birthDate?: string | null) => {
  const age = getPatientAge(birthDate)
  return age === null ? 'Wiek nieznany' : `${age} lat`
}
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

.mobile-cards {
  display: none;
}

.patients-table {
  width: 100%;
  min-width: 720px;
  border-collapse: separate;
  border-spacing: 0;
}

.patients-table th {
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

.patients-table td {
  padding: 1rem 1.125rem;
  border-bottom: 1px solid #f1f5f9;
  color: #334155;
  font-size: 0.9375rem;
  line-height: 1.45;
  transition: background-color 0.2s ease;
}

.patients-table tbody tr:last-child td {
  border-bottom: none;
}

.patients-table tbody tr:hover:not(:has(.empty-state)) {
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

.patient-meta {
  display: grid;
  gap: 0.2rem;
}

.age-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: fit-content;
  border-radius: 999px;
  padding: 0.12rem 0.55rem;
  font-size: 0.73rem;
  font-weight: 700;
}

.age-badge--minor {
  background: rgba(14, 165, 233, 0.15);
  color: #0369a1;
}

.age-badge--adult {
  background: rgba(34, 197, 94, 0.16);
  color: #166534;
}

.age-value {
  color: var(--color-text-muted);
  font-size: 0.76rem;
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

.action-btn:active {
  transform: translateY(0);
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

@media (max-width: 768px) {
  .table-scroll {
    display: none;
  }

  .mobile-cards {
    display: block;
    padding: 1rem;
  }

  .patient-card {
    background: #fafbfc;
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    padding: 1rem;
    margin-bottom: 0.75rem;
    transition:
      box-shadow 0.2s ease,
      transform 0.2s ease;
  }

  .patient-card:last-child {
    margin-bottom: 0;
  }

  .patient-card:hover {
    box-shadow: var(--shadow-sm);
    transform: translateY(-1px);
  }

  .patient-card__header h3 {
    margin: 0 0 0.75rem;
    font-size: 1rem;
    font-weight: 600;
    color: var(--color-text);
  }

  .patient-card__body {
    margin: 0 0 1rem;
    display: grid;
    gap: 0.5rem;
  }

  .patient-card__body div {
    display: grid;
    grid-template-columns: 80px 1fr;
    gap: 0.5rem;
  }

  .patient-card__body dt {
    font-size: 0.75rem;
    font-weight: 600;
    color: var(--color-text-muted);
    text-transform: uppercase;
  }

  .patient-card__body dd {
    margin: 0;
    font-size: 0.875rem;
    color: var(--color-text);
    word-break: break-word;
  }

  .patient-card__actions {
    display: flex;
    gap: 0.5rem;
    justify-content: flex-end;
  }

  .patient-card__actions .action-btn {
    padding: 0.5rem;
  }

  .empty-state-mobile {
    text-align: center;
    color: var(--color-text-muted);
    padding: 2rem 1rem;
    margin: 0;
  }
}

@media (min-width: 769px) and (max-width: 900px) {
  .btn-text {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border: 0;
  }

  .action-btn {
    padding: 0.5rem;
  }
}
</style>
