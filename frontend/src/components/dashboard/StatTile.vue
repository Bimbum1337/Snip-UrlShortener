<script setup lang="ts">
import { formatNumber } from '@/utils/format'

defineProps<{
  label: string
  value: number | string
  caption?: string
  accent?: boolean
}>()
</script>

<template>
  <div class="glass-card relative overflow-hidden p-5">
    <div
      v-if="accent"
      class="absolute -top-12 -right-10 size-32 rounded-full bg-brand/20 blur-2xl"
      aria-hidden="true"
    />

    <p class="text-[11px] font-medium tracking-wider text-faint uppercase">{{ label }}</p>

    <p class="mt-2 text-3xl font-semibold tracking-tight text-ink tabular-nums">
      {{ typeof value === 'number' ? formatNumber(value) : value }}
    </p>

    <p v-if="caption" class="mt-1 truncate text-xs text-muted">{{ caption }}</p>
  </div>
</template>
