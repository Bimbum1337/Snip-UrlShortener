import { defineStore } from 'pinia'
import { ref } from 'vue'

export type ToastKind = 'success' | 'error' | 'info'

export interface Toast {
  id: number
  kind: ToastKind
  message: string
}

let nextId = 1

export const useToastStore = defineStore('toasts', () => {
  const toasts = ref<Toast[]>([])

  function push(message: string, kind: ToastKind = 'info', timeoutMs = 4000) {
    const id = nextId++
    toasts.value.push({ id, kind, message })
    window.setTimeout(() => dismiss(id), timeoutMs)
  }

  function dismiss(id: number) {
    toasts.value = toasts.value.filter((t) => t.id !== id)
  }

  return {
    toasts,
    dismiss,
    success: (message: string) => push(message, 'success'),
    error: (message: string) => push(message, 'error', 6000),
    info: (message: string) => push(message, 'info'),
  }
})
