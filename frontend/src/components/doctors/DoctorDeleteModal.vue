<template>
  <Teleport to="body">
    <Transition name="modal-fade">
      <div v-if="visible" class="modal-overlay" @click.self="emit('cancel')">
        <div class="modal" role="dialog" aria-modal="true" aria-labelledby="delete-doctor-title">
          <h2 id="delete-doctor-title">Usuń lekarza</h2>

          <p>
            Czy na pewno chcesz usunąć lekarza
            <strong>{{ doctorName }}</strong
            >? Tej operacji nie można cofnąć.
          </p>

          <div class="modal-actions">
            <button
              type="button"
              class="app-btn app-btn--secondary"
              :disabled="deleting"
              @click="emit('cancel')"
            >
              Anuluj
            </button>

            <button
              type="button"
              class="app-btn btn-delete"
              :disabled="deleting"
              @click="emit('confirm')"
            >
              <AppSpinner v-if="deleting" label="Usuwanie..." inline />

              <template v-else>
                <AppIcon name="trash" />
                Usuń
              </template>
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import AppIcon from '../common/AppIcon.vue'
import AppSpinner from '../common/AppSpinner.vue'

defineProps<{
  visible: boolean
  doctorName: string
  deleting?: boolean
}>()

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background-color: rgba(15, 23, 42, 0.55);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.modal {
  background-color: #ffffff;
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  max-width: 440px;
  width: 100%;
  box-shadow: var(--shadow-lg);
  border: 1px solid var(--color-border);
}

h2 {
  margin: 0 0 0.75rem;
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--color-text);
}

p {
  margin: 0;
  color: var(--color-text-muted);
  line-height: 1.55;
  font-size: 0.9375rem;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-top: 1.5rem;
  flex-wrap: wrap;
}

.btn-delete {
  background-color: var(--color-delete) !important;
  color: #fff !important;
  box-shadow: 0 2px 8px rgba(220, 38, 38, 0.3);
}

.btn-delete:hover:not(:disabled) {
  background-color: #b91c1c !important;
}
</style>