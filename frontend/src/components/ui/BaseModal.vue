<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'

defineProps<{ title: string; subtitle?: string }>()

const emit = defineEmits<{ close: [] }>()

function onKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape') emit('close')
}

onMounted(() => document.addEventListener('keydown', onKeydown))
onUnmounted(() => document.removeEventListener('keydown', onKeydown))
</script>

<template>
  <Teleport to="body">
    <div class="fixed inset-0 z-50 grid place-items-center p-4">
      <div class="absolute inset-0 animate-fade bg-black/50 backdrop-blur-sm" @click="emit('close')" />

      <div
        role="dialog"
        aria-modal="true"
        class="glass-card relative z-10 w-full max-w-lg animate-rise p-6"
      >
        <header class="mb-5 flex items-start justify-between gap-4">
          <div>
            <h2 class="text-lg font-semibold text-ink">{{ title }}</h2>
            <p v-if="subtitle" class="mt-0.5 text-sm text-muted">{{ subtitle }}</p>
          </div>

          <button
            class="grid size-8 shrink-0 place-items-center rounded-lg text-faint transition-colors hover:bg-ink/5 hover:text-ink"
            aria-label="Close"
            @click="emit('close')"
          >
            <svg viewBox="0 0 24 24" class="size-4" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M6 6l12 12M18 6L6 18" stroke-linecap="round" />
            </svg>
          </button>
        </header>

        <slot />
      </div>
    </div>
  </Teleport>
</template>
