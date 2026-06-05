<template>
  <svg
    class="app-icon"
    :class="[`icon-${name}`, size]"
    xmlns="http://www.w3.org/2000/svg"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    stroke-width="2"
    stroke-linecap="round"
    stroke-linejoin="round"
    aria-hidden="true"
  >
    <path v-if="name === 'plus'" d="M12 5v14M5 12h14" />
    <template v-else-if="name === 'search'">
      <circle cx="11" cy="11" r="7" />
      <path d="m20 20-3.5-3.5" />
    </template>
    <template v-else-if="name === 'eye'">
      <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7Z" />
      <circle cx="12" cy="12" r="3" />
    </template>
    <template v-else-if="name === 'pencil'">
      <path d="M12 20h9" />
      <path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4Z" />
    </template>
    <template v-else-if="name === 'trash'">
      <path d="M3 6h18" />
      <path d="M8 6V4h8v2" />
      <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6" />
      <path d="M10 11v6M14 11v6" />
    </template>
    <template v-else-if="name === 'calendar'">
      <path d="M8 2v4M16 2v4M3 4h18a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2Z" />
      <path d="M3 10h18" />
    </template>
  </svg>
</template>

<script setup lang="ts">
export type IconName = 'plus' | 'search' | 'eye' | 'pencil' | 'trash' | 'calendar'

withDefaults(
  defineProps<{
    name: IconName
    size?: 'sm' | 'md'
  }>(),
  {
    size: 'sm',
  },
)
</script>

<style scoped>
.app-icon {
  flex-shrink: 0;
}

.app-icon.sm {
  width: 1rem;
  height: 1rem;
}

.app-icon.md {
  width: 1.25rem;
  height: 1.25rem;
}
</style>
