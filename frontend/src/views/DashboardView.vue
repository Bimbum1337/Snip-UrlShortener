<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import ShortenForm from '@/components/links/ShortenForm.vue'
import LinkList from '@/components/links/LinkList.vue'
import StatTile from '@/components/dashboard/StatTile.vue'
import VisitsChart from '@/components/charts/VisitsChart.vue'
import { analyticsApi } from '@/api/analytics'
import { useLinksStore } from '@/stores/links'
import type { DashboardSummary } from '@/types'

const links = useLinksStore()
const summary = ref<DashboardSummary | null>(null)

async function loadSummary() {
  try {
    summary.value = await analyticsApi.summary()
  } catch {
    summary.value = null
  }
}

watch(() => links.page.totalCount, loadSummary)

onMounted(async () => {
  await Promise.all([links.load(), loadSummary()])
})
</script>

<template>
  <div class="flex flex-col gap-6">
    <ShortenForm />

    <section v-if="summary" class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <StatTile label="Total links" :value="summary.totalLinks" :caption="`${summary.activeLinks} active`" accent />
      <StatTile label="Total visits" :value="summary.totalVisits" caption="all time" />
      <StatTile label="Last 7 days" :value="summary.visitsLast7Days" caption="visits" />
      <StatTile
        label="Top link"
        :value="summary.topCode ? `/${summary.topCode}` : '—'"
        :caption="summary.topCode ? `${summary.topCodeVisits} visits` : 'no traffic yet'"
      />
    </section>

    <section v-if="summary && summary.timeline.length > 0" class="glass-card p-5 sm:p-6">
      <header class="mb-4">
        <h2 class="text-base font-semibold text-ink">Visits per day</h2>
        <p class="text-xs text-muted">Across every link, over the last {{ summary.timeline.length }} days.</p>
      </header>

      <VisitsChart :points="summary.timeline" :height="220" />
    </section>

    <LinkList />
  </div>
</template>
