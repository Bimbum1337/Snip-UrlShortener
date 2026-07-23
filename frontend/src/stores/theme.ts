import { defineStore } from 'pinia'
import { ref, watchEffect } from 'vue'

const STORAGE_KEY = 'snip-theme'

export const useThemeStore = defineStore('theme', () => {
  const isDark = ref(document.documentElement.classList.contains('dark'))

  watchEffect(() => {
    document.documentElement.classList.toggle('dark', isDark.value)
    localStorage.setItem(STORAGE_KEY, isDark.value ? 'dark' : 'light')
  })

  return {
    isDark,
    toggle: () => {
      isDark.value = !isDark.value
    },
  }
})
