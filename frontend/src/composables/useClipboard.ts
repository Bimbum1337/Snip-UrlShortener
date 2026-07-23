import { ref } from 'vue'

export function useClipboard(resetAfterMs = 1800) {
  const copied = ref<string | null>(null)

  async function copy(text: string) {
    try {
      await navigator.clipboard.writeText(text)
    } catch {
      // The Clipboard API needs a secure context, so fall back to the old trick.
      const textarea = document.createElement('textarea')
      textarea.value = text
      textarea.style.position = 'fixed'
      textarea.style.opacity = '0'
      document.body.appendChild(textarea)
      textarea.select()
      document.execCommand('copy')
      document.body.removeChild(textarea)
    }

    copied.value = text
    window.setTimeout(() => {
      if (copied.value === text) copied.value = null
    }, resetAfterMs)
  }

  return {
    copied,
    copy,
    isCopied: (text: string) => copied.value === text,
  }
}
