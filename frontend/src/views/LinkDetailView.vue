<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import BaseButton from '@/components/ui/BaseButton.vue'
import StatTile from '@/components/dashboard/StatTile.vue'
import VisitsChart from '@/components/charts/VisitsChart.vue'
import ReferrerBars from '@/components/charts/ReferrerBars.vue'
import { analyticsApi } from '@/api/analytics'
import { ApiError } from '@/api/http'
import { useClipboard } from '@/composables/useClipboard'
import { formatRelative, prettyUrl } from '@/utils/format'
import type { ShortUrlStats } from '@/types'

const props = defineProps<{ code: string }>()

const clipboard = useClipboard()

const stats = ref<ShortUrlStats | null>(null)
const isLoading = ref(true)
const error = ref<string | null>(null)
const windowDays = ref(30)

const ranges = [
  { days: 7, label: '7d' },
  { days: 30, label: '30d' },
  { days: 90, label: '90d' },
]

const hasReferrers = computed(() => (stats.value?.topReferrers.length ?? 0) > 0)

async function load() {
  isLoading.value = true
  error.value = null

  try {
    stats.value = await analyticsApi.forCode(props.code, windowDays.value)
  } catch (err) {
    error.value = err instanceof ApiError ? err.message : 'Could not load this link.'
    stats.value = null
  } finally {
    isLoading.value = false
  }
}

function openInNewTab() {
  if (stats.value) window.open(stats.value.shortLink, '_blank', 'noopener')
}

watch(windowDays, load)
onMounted(load)
</script>

<template>
  <div class="flex flex-col gap-6">
    <RouterLink
      :to="{ name: 'dashboard' }"
      class="flex w-fit items-center gap-1.5 text-sm text-muted transition-colors hover:text-brand"
    >
      <svg viewBox="0 0 24 24" class="size-4" fill="none" stroke="currentColor" stroke-width="2">
        <path d="M15 6l-6 6 6 6" stroke-linecap="round" stroke-linejoin="round" />
      </svg>
      Back to dashboard
    </RouterLink>

    <div v-if="isLoading" class="glass-card h-40 animate-pulse" />

    <div v-else-if="error" class="glass-card p-10 text-center">
      <p class="text-sm text-danger">{{ error }}</p>
      <RouterLink :to="{ name: 'dashboard' }" class="mt-4 inline-block text-sm text-brand hover:underline">
        Return to the dashboard
      </RouterLink>
    </div>

    <template v-else-if="stats">
      <header class="glass-card flex flex-col gap-4 p-6 sm:flex-row sm:items-center sm:justify-between">
        <div class="min-w-0">
          <h1 class="font-mono text-2xl font-semibold tracking-tight text-ink">/{{ stats.code }}</h1>
          <a
            :href="stats.destination"
            target="_blank"
            rel="noopener noreferrer"
            class="mt-1 block truncate text-sm text-muted hover:text-brand hover:underline"
          >
            {{ prettyUrl(stats.destination, 70) }}
          </a>
        </div>

        <div class="flex shrink-0 gap-2">
          <BaseButton variant="secondary" size="sm" @click="clipboard.copy(stats.shortLink)">
            {{ clipboard.isCopied(stats.shortLink) ? 'Copied!' : 'Copy link' }}
          </BaseButton>
          <BaseButton size="sm" @click="openInNewTab">Open</BaseButton>
        </div>
      </header>

      <section class="grid gap-4 sm:grid-cols-3">
        <StatTile label="Total visits" :value="stats.totalVisits" accent />
        <StatTile label="Unique visitors" :value="stats.uniqueVisitors" caption="by hashed IP" />
        <StatTile label="Last visit" :value="formatRelative(stats.lastVisitedAtUtc)" />
      </section>

      <section class="glass-card p-5 sm:p-6">
        <header class="mb-4 flex flex-wrap items-center justify-between gap-3">
          <div>
            <h2 class="text-base font-semibold text-ink">Visits per day</h2>
            <p class="text-xs text-muted">Traffic to this link over time.</p>
          </div>

          <div class="flex items-center gap-1 rounded-xl border border-line p-1">
            <button
              v-for="range in ranges"
              :key="range.days"
              class="rounded-lg px-3 py-1.5 text-xs font-medium transition-colors"
              :class="windowDays === range.days ? 'bg-brand/15 text-brand' : 'text-muted hover:text-ink'"
              @click="windowDays = range.days"
            >
              {{ range.label }}
            </button>
          </div>
        </header>

        <VisitsChart :points="stats.timeline" :height="240" />
      </section>

      <section class="glass-card p-5 sm:p-6">
        <header class="mb-4">
          <h2 class="text-base font-semibold text-ink">Top referrers</h2>
          <p class="text-xs text-muted">Where the traffic came from.</p>
        </header>

        <ReferrerBars v-if="hasReferrers" :items="stats.topReferrers" />
        <p v-else class="py-6 text-center text-sm text-faint">No visits recorded yet.</p>
      </section>
    </template>
  </div>
</template>
