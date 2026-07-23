<script setup lang="ts">
withDefaults(
  defineProps<{
    label?: string
    hint?: string
    error?: string | null
    success?: string | null
    placeholder?: string
    type?: string
    prefix?: string
    id?: string
    disabled?: boolean
  }>(),
  { type: 'text' },
)

const model = defineModel<string>({ default: '' })
</script>

<template>
  <div class="flex flex-col gap-1.5">
    <label v-if="label" :for="id" class="text-xs font-medium tracking-wide text-muted uppercase">
      {{ label }}
    </label>

    <div
      class="group flex items-center rounded-xl border bg-surface/60 transition-colors focus-within:border-brand"
      :class="error ? 'border-danger/60' : 'border-line'"
    >
      <span v-if="prefix" class="pl-3 font-mono text-sm text-faint select-none">{{ prefix }}</span>

      <span v-if="$slots.icon" class="grid place-items-center pl-3 text-faint">
        <slot name="icon" />
      </span>

      <input
        :id="id"
        v-model="model"
        :type="type"
        :placeholder="placeholder"
        :disabled="disabled"
        class="w-full bg-transparent px-3 py-2.5 text-sm text-ink outline-none placeholder:text-faint disabled:opacity-60"
      />

      <span v-if="$slots.suffix" class="pr-2"><slot name="suffix" /></span>
    </div>

    <p v-if="error" class="text-xs text-danger">{{ error }}</p>
    <p v-else-if="success" class="text-xs text-success">{{ success }}</p>
    <p v-else-if="hint" class="text-xs text-faint">{{ hint }}</p>
  </div>
</template>
