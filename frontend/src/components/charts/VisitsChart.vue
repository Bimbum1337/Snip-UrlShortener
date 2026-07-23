<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { formatNumber } from '@/utils/format'
import type { DailyVisitCount } from '@/types'

// Width is measured rather than set through a viewBox, so the stroke stays 2px
// at any container size instead of being scaled with everything else.
const props = withDefaults(defineProps<{ points: DailyVisitCount[]; height?: number }>(), {
  height: 200,
})

const container = ref<HTMLElement | null>(null)
const width = ref(720)
const hoverIndex = ref<number | null>(null)

let observer: ResizeObserver | undefined

onMounted(() => {
  if (!container.value) return
  observer = new ResizeObserver(([entry]) => {
    width.value = Math.max(240, entry.contentRect.width)
  })
  observer.observe(container.value)
})

onUnmounted(() => observer?.disconnect())

const padding = { top: 16, right: 8, bottom: 24, left: 8 }

const plot = computed(() => ({
  width: Math.max(1, width.value - padding.left - padding.right),
  height: Math.max(1, props.height - padding.top - padding.bottom),
}))

// Floor of 1 keeps an all-zero series on the baseline instead of filling the panel.
const maxCount = computed(() => Math.max(1, ...props.points.map((p) => p.count)))

const coords = computed(() =>
  props.points.map((point, index) => {
    const step = props.points.length > 1 ? plot.value.width / (props.points.length - 1) : 0
    return {
      ...point,
      x: padding.left + index * step,
      y: padding.top + plot.value.height * (1 - point.count / maxCount.value),
    }
  }),
)

const linePath = computed(() =>
  coords.value.map((c, i) => `${i === 0 ? 'M' : 'L'}${c.x.toFixed(2)},${c.y.toFixed(2)}`).join(' '),
)

const areaPath = computed(() => {
  if (coords.value.length === 0) return ''
  const baseline = padding.top + plot.value.height
  const first = coords.value[0]
  const last = coords.value[coords.value.length - 1]
  return `${linePath.value} L${last.x.toFixed(2)},${baseline} L${first.x.toFixed(2)},${baseline} Z`
})

const gridLines = computed(() =>
  [0, 0.5, 1].map((ratio) => ({
    ratio,
    y: padding.top + plot.value.height * ratio,
    label: formatNumber(Math.round(maxCount.value * (1 - ratio))),
  })),
)

// Only the ends and the middle get a label, otherwise they collide.
const xLabels = computed(() => {
  const count = props.points.length
  if (count === 0) return []
  const indices = count <= 2 ? [0, count - 1] : [0, Math.floor((count - 1) / 2), count - 1]
  return [...new Set(indices)].map((index) => ({
    index,
    x: coords.value[index].x,
    label: shortDate(props.points[index].date),
  }))
})

const hovered = computed(() => (hoverIndex.value === null ? null : coords.value[hoverIndex.value]))

function onPointerMove(event: PointerEvent) {
  const bounds = (event.currentTarget as SVGElement).getBoundingClientRect()
  const relative = event.clientX - bounds.left - padding.left
  const step = props.points.length > 1 ? plot.value.width / (props.points.length - 1) : 1
  const index = Math.round(relative / step)
  hoverIndex.value = Math.min(props.points.length - 1, Math.max(0, index))
}

function shortDate(iso: string) {
  return new Date(`${iso}T00:00:00`).toLocaleDateString(undefined, { day: 'numeric', month: 'short' })
}

function fullDate(iso: string) {
  return new Date(`${iso}T00:00:00`).toLocaleDateString(undefined, {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
  })
}

// Clamped so the tooltip does not hang off either end.
const tooltipStyle = computed(() => {
  if (!hovered.value) return {}
  const clamped = Math.min(Math.max(hovered.value.x, 60), width.value - 60)
  return { left: `${clamped}px`, top: `${Math.max(hovered.value.y - 12, 8)}px` }
})
</script>

<template>
  <div ref="container" class="relative w-full">
    <svg
      :width="width"
      :height="height"
      class="block touch-none"
      role="img"
      :aria-label="`Visits per day over the last ${points.length} days`"
      @pointermove="onPointerMove"
      @pointerleave="hoverIndex = null"
    >
      <defs>
        <linearGradient id="visitsFill" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stop-color="var(--color-brand)" stop-opacity="0.35" />
          <stop offset="100%" stop-color="var(--color-brand)" stop-opacity="0.02" />
        </linearGradient>
      </defs>

      <g>
        <line
          v-for="line in gridLines"
          :key="line.ratio"
          :x1="padding.left"
          :x2="width - padding.right"
          :y1="line.y"
          :y2="line.y"
          stroke="currentColor"
          stroke-width="1"
          stroke-dasharray="3 5"
          class="text-line"
        />
      </g>

      <path :d="areaPath" fill="url(#visitsFill)" />
      <path
        :d="linePath"
        fill="none"
        stroke="var(--color-brand)"
        stroke-width="2"
        stroke-linecap="round"
        stroke-linejoin="round"
      />

      <g v-if="hovered">
        <line
          :x1="hovered.x"
          :x2="hovered.x"
          :y1="padding.top"
          :y2="padding.top + plot.height"
          stroke="var(--color-brand)"
          stroke-width="1"
          stroke-opacity="0.4"
        />
        <!-- Surface-coloured ring so the marker reads against the fill. -->
        <circle
          :cx="hovered.x"
          :cy="hovered.y"
          r="5"
          fill="var(--color-brand)"
          stroke="var(--color-surface)"
          stroke-width="2"
        />
      </g>

      <g class="text-faint" fill="currentColor" font-size="10">
        <text
          v-for="label in xLabels"
          :key="label.index"
          :x="label.x"
          :y="height - 6"
          :text-anchor="label.index === 0 ? 'start' : label.index === points.length - 1 ? 'end' : 'middle'"
        >
          {{ label.label }}
        </text>
      </g>
    </svg>

    <div class="pointer-events-none absolute inset-y-0 right-0 flex flex-col justify-between py-2 text-[10px] text-faint">
      <span>{{ gridLines[0].label }}</span>
      <span>{{ gridLines[2].label }}</span>
    </div>

    <div
      v-if="hovered"
      class="glass-card pointer-events-none absolute z-10 -translate-x-1/2 -translate-y-full px-3 py-2 text-xs whitespace-nowrap"
      :style="tooltipStyle"
    >
      <p class="font-medium text-ink">{{ formatNumber(hovered.count) }} visits</p>
      <p class="text-faint">{{ fullDate(hovered.date) }}</p>
    </div>
  </div>
</template>
