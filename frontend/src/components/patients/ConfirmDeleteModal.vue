<template>
  <div v-if="visible" class="modal-overlay" @click.self="emit('cancel')">
    <div class="modal" role="dialog" aria-modal="true" aria-labelledby="delete-title">
      <h2 id="delete-title">Delete patient</h2>
      <p>
        Are you sure you want to delete
        <strong>{{ patientName }}</strong
        >? This action cannot be undone.
      </p>

      <div class="modal-actions">
        <button type="button" class="btn-cancel" :disabled="deleting" @click="emit('cancel')">
          Cancel
        </button>
        <button type="button" class="btn-delete" :disabled="deleting" @click="emit('confirm')">
          {{ deleting ? 'Deleting…' : 'Delete' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  visible: boolean
  patientName: string
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
  background-color: rgba(17, 24, 39, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.modal {
  background-color: #ffffff;
  border-radius: 12px;
  padding: 1.5rem;
  max-width: 420px;
  width: 100%;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.15);
}

h2 {
  margin: 0 0 0.75rem;
  font-size: 1.25rem;
  color: #111827;
}

p {
  margin: 0;
  color: #4b5563;
  line-height: 1.5;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-top: 1.5rem;
}

button {
  padding: 0.625rem 1.25rem;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  border: none;
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-cancel {
  background-color: #f3f4f6;
  color: #374151;
}

.btn-cancel:hover:not(:disabled) {
  background-color: #e5e7eb;
}

.btn-delete {
  background-color: #dc2626;
  color: #ffffff;
}

.btn-delete:hover:not(:disabled) {
  background-color: #b91c1c;
}
</style>
