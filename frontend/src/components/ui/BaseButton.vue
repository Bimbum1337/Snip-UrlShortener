<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(
  defineProps<{
    variant?: 'primary' | 'secondary' | 'ghost' | 'danger'
    size?: 'sm' | 'md' | 'lg'
    loading?: boolean
    disabled?: boolean
    type?: 'button' | 'submit'
    block?: boolean
  }>(),
  { variant: 'primary', size: 'md', type: 'button' },
)

const variants = {
  primary:
    'text-white bg-linear-to-r from-brand to-accent shadow-[0_10px_30px_-12px] shadow-brand hover:brightness-110 active:brightness-95',
  secondary: 'bg-elevated text-ink border border-line hover:border-brand/60 hover:text-brand',
  ghost: 'text-muted hover:text-ink hover:bg-ink/5',
  danger: 'bg-danger/10 text-danger border border-danger/30 hover:bg-danger/20',
} as const

const sizes = {
  sm: 'h-8 px-3 text-xs gap-1.5 rounded-lg',
  md: 'h-10 px-4 text-sm gap-2 rounded-xl',
  lg: 'h-12 px-6 text-base gap-2 rounded-xl',
} as const

const classes = computed(() => [
  'inline-flex items-center justify-center font-medium transition-all duration-150',
  'disabled:opacity-50 disabled:pointer-events-none select-none whitespace-nowrap',
  variants[props.variant],
  sizes[props.size],
  props.block ? 'w-full' : '',
])
</script>

<template>
  <button :type="type" :class="classes" :disabled="disabled || loading">
    <svg v-if="loading" class="size-4 animate-spin" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="3" class="opacity-25" />
      <path d="M22 12a10 10 0 0 1-10 10" stroke="currentColor" stroke-width="3" stroke-linecap="round" />
    </svg>
    <slot />
  </button>
</template>
