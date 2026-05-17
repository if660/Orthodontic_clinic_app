<template>
  <div class="spinner-wrap" :class="{ inline }" role="status" aria-live="polite">
    <span class="spinner" aria-hidden="true" />
    <span v-if="label" class="spinner-label">{{ label }}</span>
  </div>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    label?: string
    inline?: boolean
  }>(),
  {
    label: 'Ładowanie...',
    inline: false,
  },
)
</script>

<style scoped>
.spinner-wrap {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  padding: 2rem 1rem;
  color: #6b7280;
}

.spinner-wrap.inline {
  flex-direction: row;
  padding: 0;
}

.spinner {
  width: 2rem;
  height: 2rem;
  border: 3px solid #e5e7eb;
  border-top-color: #2563eb;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}

.spinner-wrap.inline .spinner {
  width: 1.125rem;
  height: 1.125rem;
  border-width: 2px;
}

.spinner-label {
  font-size: 0.9375rem;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
