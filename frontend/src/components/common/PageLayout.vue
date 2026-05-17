<template>
  <div class="app-page" :class="{ 'app-page--narrow': narrow }">
    <slot />
  </div>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    narrow?: boolean
  }>(),
  {
    narrow: false,
  },
)
</script>
