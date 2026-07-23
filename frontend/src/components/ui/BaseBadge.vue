<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(
  defineProps<{ tone?: 'neutral' | 'success' | 'warning' | 'danger' | 'brand'; dot?: boolean }>(),
  { tone: 'neutral' },
)

const tones = {
  neutral: 'bg-ink/5 text-muted border-line',
  success: 'bg-success/10 text-success border-success/25',
  warning: 'bg-warning/10 text-warning border-warning/25',
  danger: 'bg-danger/10 text-danger border-danger/25',
  brand: 'bg-brand/10 text-brand border-brand/25',
} as const

const classes = computed(() => tones[props.tone])
</script>

<template>
  <span
    class="inline-flex items-center gap-1.5 rounded-full border px-2.5 py-1 text-[11px] font-medium"
    :class="classes"
  >
    <span v-if="dot" class="size-1.5 rounded-full bg-current" />
    <slot />
  </span>
</template>
