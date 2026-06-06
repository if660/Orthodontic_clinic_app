<template>
  <select
    :id="id"
    class="status-select"
    :value="modelValue"
    @change="emit('update:modelValue', ($event.target as HTMLSelectElement).value)"
  >
    <option
      v-for="option in selectOptions"
      :key="option.value"
      :value="option.value"
    >
      {{ option.label }}
    </option>
  </select>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue: string
    id?: string
  }>(),
  {
    id: undefined,
  },
)

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const baseOptions = [
  { value: 'Do potwierdzenia', label: 'Do potwierdzenia' },
  { value: 'Zaplanowana', label: 'Zaplanowana' },
  { value: 'Zakonczona', label: 'Odbyta' },
  { value: 'Anulowana', label: 'Anulowana' },
]
const selectOptions = computed(() => {
  if (!props.modelValue || baseOptions.some((option) => option.value === props.modelValue)) {
    return baseOptions
  }

  return [
    { value: props.modelValue, label: props.modelValue },
    ...baseOptions,
  ]
})
</script>

<style scoped>
.status-select {
  width: 100%;
  max-width: 320px;
  border: 1.5px solid var(--color-border-strong);
  border-radius: var(--radius-md);
  background: #fff;
  padding: 0.6rem 0.75rem;
  font: inherit;
  font-size: 0.95rem;
  color: var(--color-text);
  cursor: pointer;
}

.status-select:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px var(--color-primary-soft);
}
</style>

