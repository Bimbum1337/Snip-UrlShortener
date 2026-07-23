import { ref, watch, type Ref } from 'vue'

export function useDebouncedRef<T>(source: Ref<T>, delayMs = 350): Ref<T> {
  const debounced = ref(source.value) as Ref<T>
  let timer: number | undefined

  watch(source, (value) => {
    window.clearTimeout(timer)
    timer = window.setTimeout(() => {
      debounced.value = value
    }, delayMs)
  })

  return debounced
}
