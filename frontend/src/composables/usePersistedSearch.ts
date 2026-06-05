import { ref, watch } from 'vue'

export function usePersistedSearch(key: string) {
  const searchQuery = ref(sessionStorage.getItem(key) ?? '')

  watch(searchQuery, (value) => {
    sessionStorage.setItem(key, value)
  })

  return { searchQuery }
}
