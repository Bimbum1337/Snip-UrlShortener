<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
import ToastHost from '@/components/ui/ToastHost.vue'
import { useThemeStore } from '@/stores/theme'

const theme = useThemeStore()
</script>

<template>
  <div class="min-h-svh">
    <header class="sticky top-0 z-40 border-b border-line/60 bg-canvas/70 backdrop-blur-xl">
      <div class="mx-auto flex h-16 max-w-6xl items-center justify-between px-4 sm:px-6">
        <RouterLink :to="{ name: 'dashboard' }" class="flex items-center gap-2.5">
          <span class="grid size-9 place-items-center rounded-xl bg-linear-to-br from-brand to-accent text-white">
            <svg viewBox="0 0 24 24" class="size-5" fill="none" stroke="currentColor" stroke-width="2">
              <path
                d="M10 13a5 5 0 0 0 7.5.5l3-3a5 5 0 0 0-7-7l-1.5 1.5M14 11a5 5 0 0 0-7.5-.5l-3 3a5 5 0 0 0 7 7l1.5-1.5"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
            </svg>
          </span>

          <span class="text-lg font-semibold tracking-tight text-ink">Snip</span>
        </RouterLink>

        <button
          class="grid size-9 place-items-center rounded-xl border border-line text-muted transition-colors hover:border-brand/50 hover:text-brand"
          :aria-label="theme.isDark ? 'Switch to light theme' : 'Switch to dark theme'"
          @click="theme.toggle()"
        >
          <svg v-if="theme.isDark" viewBox="0 0 24 24" class="size-4" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="4" />
            <path d="M12 2v2M12 20v2M2 12h2M20 12h2M5 5l1.5 1.5M17.5 17.5L19 19M19 5l-1.5 1.5M6.5 17.5L5 19" stroke-linecap="round" />
          </svg>
          <svg v-else viewBox="0 0 24 24" class="size-4" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M21 13a8.5 8.5 0 0 1-10-10 8.5 8.5 0 1 0 10 10z" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
        </button>
      </div>
    </header>

    <main class="mx-auto max-w-6xl px-4 py-8 sm:px-6 sm:py-10">
      <RouterView v-slot="{ Component }">
        <Transition
          mode="out-in"
          enter-active-class="transition duration-200 ease-out"
          enter-from-class="translate-y-2 opacity-0"
          leave-active-class="transition duration-150 ease-in"
          leave-to-class="opacity-0"
        >
          <component :is="Component" />
        </Transition>
      </RouterView>
    </main>

    <footer class="mx-auto max-w-6xl px-4 pb-10 text-center text-xs text-faint sm:px-6">
      Built on a .NET clean-architecture API · Vue 3 · Tailwind
    </footer>

    <ToastHost />
  </div>
</template>
