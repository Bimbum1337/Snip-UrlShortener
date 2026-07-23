<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import BaseBadge from '@/components/ui/BaseBadge.vue'
import { formatNumber, formatRelative, hostOf, hueFor, prettyUrl } from '@/utils/format'
import type { ShortUrl } from '@/types'

const props = defineProps<{ link: ShortUrl; copied: boolean }>()

const emit = defineEmits<{
  copy: [link: ShortUrl]
  edit: [link: ShortUrl]
  toggle: [link: ShortUrl]
  remove: [link: ShortUrl]
}>()

const hue = computed(() => hueFor(props.link.code))

const status = computed(() => {
  if (props.link.isExpired) return { tone: 'warning' as const, label: 'Expired' }
  if (!props.link.isActive) return { tone: 'danger' as const, label: 'Disabled' }
  return { tone: 'success' as const, label: 'Active' }
})
</script>

<template>
  <article
    class="group flex flex-col gap-4 border-b border-line px-5 py-4 transition-colors last:border-b-0 hover:bg-ink/[0.02] sm:flex-row sm:items-center"
  >
    <!-- Colour is derived from the code, so a link always looks the same. -->
    <div
      class="grid size-10 shrink-0 place-items-center rounded-xl text-sm font-semibold text-white"
      :style="{
        background: `linear-gradient(135deg, oklch(0.65 0.18 ${hue}), oklch(0.6 0.2 ${(hue + 45) % 360}))`,
      }"
      aria-hidden="true"
    >
      {{ link.code.slice(0, 2).toUpperCase() }}
    </div>

    <div class="min-w-0 flex-1">
      <div class="flex flex-wrap items-center gap-2">
        <RouterLink
          :to="{ name: 'link-detail', params: { code: link.code } }"
          class="truncate font-mono text-sm font-medium text-ink hover:text-brand"
        >
          /{{ link.code }}
        </RouterLink>

        <BaseBadge :tone="status.tone" dot>{{ status.label }}</BaseBadge>
        <BaseBadge v-if="link.isCustomAlias" tone="brand">custom</BaseBadge>
      </div>

      <p v-if="link.title" class="mt-1 truncate text-sm text-ink/80">{{ link.title }}</p>

      <a
        :href="link.destination"
        target="_blank"
        rel="noopener noreferrer"
        class="mt-0.5 block truncate text-xs text-muted hover:text-brand hover:underline"
        :title="link.destination"
      >
        {{ prettyUrl(link.destination) }}
      </a>
    </div>

    <div class="flex shrink-0 items-center gap-6 sm:gap-8">
      <div class="text-right">
        <p class="text-sm font-semibold text-ink tabular-nums">{{ formatNumber(link.visitCount) }}</p>
        <p class="text-[11px] text-faint">visits</p>
      </div>

      <div class="hidden text-right lg:block">
        <p class="text-xs text-muted">{{ formatRelative(link.lastVisitedAtUtc) }}</p>
        <p class="text-[11px] text-faint">last visit</p>
      </div>

      <div class="hidden text-right xl:block">
        <p class="text-xs text-muted">{{ hostOf(link.destination) }}</p>
        <p class="text-[11px] text-faint">host</p>
      </div>

      <!-- Always visible on touch, hover-revealed on pointer devices. -->
      <div class="flex items-center gap-1 opacity-100 transition-opacity sm:opacity-60 sm:group-hover:opacity-100">
        <button
          class="grid size-8 place-items-center rounded-lg text-muted transition-colors hover:bg-brand/10 hover:text-brand"
          :title="copied ? 'Copied!' : 'Copy short link'"
          @click="emit('copy', link)"
        >
          <svg v-if="copied" viewBox="0 0 24 24" class="size-4" fill="none" stroke="currentColor" stroke-width="2.5">
            <path d="M4 12l5 5L20 6" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
          <svg v-else viewBox="0 0 24 24" class="size-4" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="9" y="9" width="11" height="11" rx="2" />
            <path d="M15 5H6a2 2 0 0 0-2 2v9" stroke-linecap="round" />
          </svg>
        </button>

        <button
          class="grid size-8 place-items-center rounded-lg text-muted transition-colors hover:bg-brand/10 hover:text-brand"
          title="Edit"
          @click="emit('edit', link)"
        >
          <svg viewBox="0 0 24 24" class="size-4" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M4 20h4l10-10a2.8 2.8 0 0 0-4-4L4 16v4z" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
        </button>

        <button
          class="grid size-8 place-items-center rounded-lg text-muted transition-colors hover:bg-brand/10 hover:text-brand"
          :title="link.isActive ? 'Disable' : 'Enable'"
          @click="emit('toggle', link)"
        >
          <svg v-if="link.isActive" viewBox="0 0 24 24" class="size-4" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M12 5v7M7.5 7.5a7 7 0 1 0 9 0" stroke-linecap="round" />
          </svg>
          <svg v-else viewBox="0 0 24 24" class="size-4" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M5 12l5 5L19 7" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
        </button>

        <button
          class="grid size-8 place-items-center rounded-lg text-muted transition-colors hover:bg-danger/10 hover:text-danger"
          title="Delete"
          @click="emit('remove', link)"
        >
          <svg viewBox="0 0 24 24" class="size-4" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M5 7h14M10 11v6M14 11v6M6 7l1 13h10l1-13M9 7V4h6v3" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
        </button>
      </div>
    </div>
  </article>
</template>
