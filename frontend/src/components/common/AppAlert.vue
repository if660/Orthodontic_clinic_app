<template>
  <div
    v-if="message"
    class="alert"
    :class="variant"
    role="alert"
  >
    <span class="alert-text">{{ message }}</span>
    <button
      v-if="dismissible"
      type="button"
      class="alert-close"
      aria-label="Dismiss"
      @click="emit('dismiss')"
    >
      ×
    </button>
  </div>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    message: string | null
    variant?: 'success' | 'error' | 'info'
    dismissible?: boolean
  }>(),
  {
    variant: 'info',
    dismissible: true,
  },
)

const emit = defineEmits<{
  dismiss: []
}>()
</script>

<style scoped>
.alert {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  font-size: 0.875rem;
  margin-bottom: 1rem;
  animation: slide-in 0.25s ease;
}

.alert.success {
  background-color: #ecfdf5;
  color: #065f46;
  border: 1px solid #a7f3d0;
}

.alert.error {
  background-color: #fef2f2;
  color: #991b1b;
  border: 1px solid #fecaca;
}

.alert.info {
  background-color: #eff6ff;
  color: #1e40af;
  border: 1px solid #bfdbfe;
}

.alert-close {
  border: none;
  background: transparent;
  font-size: 1.25rem;
  line-height: 1;
  cursor: pointer;
  color: inherit;
  opacity: 0.7;
  padding: 0 0.25rem;
}

.alert-close:hover {
  opacity: 1;
}

@keyframes slide-in {
  from {
    opacity: 0;
    transform: translateY(-6px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
