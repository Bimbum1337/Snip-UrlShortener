<script setup lang="ts">
import { computed } from 'vue'
import { formatNumber } from '@/utils/format'
import type { ReferrerCount } from '@/types'

const props = defineProps<{ items: ReferrerCount[] }>()

const max = computed(() => Math.max(1, ...props.items.map((i) => i.count)))

// Minimum 2% so a single visit still draws something visible.
const rows = computed(() =>
  props.items.map((item) => ({
    ...item,
    percent: Math.max(2, Math.round((item.count / max.value) * 100)),
  })),
)
</script>

<template>
  <ul class="flex flex-col gap-3">
    <li v-for="row in rows" :key="row.referrer" class="group">
      <div class="mb-1.5 flex items-baseline justify-between gap-3 text-sm">
        <span class="truncate text-ink">{{ row.referrer }}</span>
        <span class="shrink-0 font-mono text-xs text-muted">{{ formatNumber(row.count) }}</span>
      </div>

      <div class="h-2 overflow-hidden rounded-full bg-ink/5">
        <div
          class="h-full rounded-full bg-linear-to-r from-brand to-accent transition-all duration-500"
          :style="{ width: `${row.percent}%` }"
        />
      </div>
    </li>
  </ul>
</template>
