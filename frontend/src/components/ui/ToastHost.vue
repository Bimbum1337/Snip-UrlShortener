<script setup lang="ts">
import { useToastStore } from '@/stores/toasts'

const toasts = useToastStore()

const tones = {
  success: 'border-success/30 text-success',
  error: 'border-danger/30 text-danger',
  info: 'border-brand/30 text-brand',
} as const
</script>

<template>
  <Teleport to="body">
    <div class="pointer-events-none fixed inset-x-0 bottom-6 z-60 flex flex-col items-center gap-2 px-4">
      <TransitionGroup
        enter-active-class="transition duration-300 ease-out"
        enter-from-class="translate-y-3 opacity-0"
        leave-active-class="transition duration-200 ease-in absolute"
        leave-to-class="translate-y-2 opacity-0"
        move-class="transition duration-200"
      >
        <div
          v-for="toast in toasts.toasts"
          :key="toast.id"
          class="glass-card pointer-events-auto flex max-w-md items-center gap-3 border px-4 py-3 text-sm"
          :class="tones[toast.kind]"
        >
          <span class="size-1.5 shrink-0 rounded-full bg-current" />
          <span class="text-ink">{{ toast.message }}</span>

          <button
            class="ml-2 text-faint transition-colors hover:text-ink"
            aria-label="Dismiss"
            @click="toasts.dismiss(toast.id)"
          >
            <svg viewBox="0 0 24 24" class="size-3.5" fill="none" stroke="currentColor" stroke-width="2.5">
              <path d="M6 6l12 12M18 6L6 18" stroke-linecap="round" />
            </svg>
          </button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>
