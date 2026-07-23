<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import { shortUrlsApi } from '@/api/shortUrls'
import { useLinksStore } from '@/stores/links'
import { useClipboard } from '@/composables/useClipboard'
import { useDebouncedRef } from '@/composables/useDebouncedRef'
import type { ShortUrl } from '@/types'

const links = useLinksStore()
const clipboard = useClipboard()

const destination = ref('')
const alias = ref('')
const title = ref('')
const expiresAt = ref('')
const showOptions = ref(false)
const lastCreated = ref<ShortUrl | null>(null)

const debouncedAlias = useDebouncedRef(alias, 400)
const aliasState = ref<{ checking: boolean; available: boolean | null; reason: string | null }>({
  checking: false,
  available: null,
  reason: null,
})

watch(debouncedAlias, async (value) => {
  const candidate = value.trim()

  if (candidate.length < 3) {
    aliasState.value = { checking: false, available: null, reason: null }
    return
  }

  aliasState.value = { checking: true, available: null, reason: null }

  try {
    const result = await shortUrlsApi.checkAlias(candidate)
    // Drop a response that landed after the user carried on typing.
    if (candidate !== alias.value.trim()) return
    aliasState.value = { checking: false, available: result.isAvailable, reason: result.reason }
  } catch {
    aliasState.value = { checking: false, available: null, reason: null }
  }
})

const aliasError = computed(() =>
  aliasState.value.available === false ? aliasState.value.reason : null,
)

const aliasSuccess = computed(() =>
  aliasState.value.available === true ? 'That alias is available.' : null,
)

const canSubmit = computed(
  () => destination.value.trim().length > 0 && aliasState.value.available !== false,
)

async function submit() {
  if (!canSubmit.value) return

  const created = await links.create({
    destination: destination.value.trim(),
    customAlias: alias.value.trim() || null,
    title: title.value.trim() || null,
    expiresAtUtc: expiresAt.value ? new Date(expiresAt.value).toISOString() : null,
  })

  if (!created) return

  lastCreated.value = created
  destination.value = ''
  alias.value = ''
  title.value = ''
  expiresAt.value = ''
  showOptions.value = false
  aliasState.value = { checking: false, available: null, reason: null }
}
</script>

<template>
  <section class="glass-card relative overflow-hidden p-6 sm:p-8">
    <div
      class="absolute -top-24 -right-16 size-64 rounded-full bg-accent/15 blur-3xl"
      aria-hidden="true"
    />

    <div class="relative">
      <h2 class="text-xl font-semibold tracking-tight text-ink sm:text-2xl">
        Shorten a <span class="brand-gradient-text">link</span>
      </h2>
      <p class="mt-1 text-sm text-muted">Paste a URL, pick an alias if you want one, and go.</p>

      <form class="mt-6 flex flex-col gap-4" @submit.prevent="submit">
        <div class="flex flex-col gap-3 sm:flex-row">
          <div class="flex-1">
            <BaseInput
              id="destination"
              v-model="destination"
              placeholder="https://example.com/a-very-long-address"
              type="url"
            >
              <template #icon>
                <svg viewBox="0 0 24 24" class="size-4" fill="none" stroke="currentColor" stroke-width="2">
                  <path
                    d="M10 13a5 5 0 0 0 7.5.5l3-3a5 5 0 0 0-7-7l-1.5 1.5M14 11a5 5 0 0 0-7.5-.5l-3 3a5 5 0 0 0 7 7l1.5-1.5"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                  />
                </svg>
              </template>
            </BaseInput>
          </div>

          <BaseButton
            type="submit"
            size="lg"
            class="sm:w-40"
            :loading="links.isCreating"
            :disabled="!canSubmit"
          >
            Shorten
          </BaseButton>
        </div>

        <button
          type="button"
          class="flex w-fit items-center gap-1.5 text-xs font-medium text-muted transition-colors hover:text-brand"
          @click="showOptions = !showOptions"
        >
          <svg
            viewBox="0 0 24 24"
            class="size-3.5 transition-transform"
            :class="showOptions ? 'rotate-90' : ''"
            fill="none"
            stroke="currentColor"
            stroke-width="2.5"
          >
            <path d="M9 6l6 6-6 6" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
          {{ showOptions ? 'Hide' : 'Show' }} options
        </button>

        <div v-if="showOptions" class="grid animate-rise gap-4 sm:grid-cols-3">
          <BaseInput
            id="alias"
            v-model="alias"
            label="Custom alias"
            placeholder="my-link"
            prefix="/"
            :error="aliasError"
            :success="aliasSuccess"
            :hint="aliasState.checking ? 'Checking…' : 'Leave blank to generate one.'"
          />

          <BaseInput id="title" v-model="title" label="Title" placeholder="Campaign name" />

          <BaseInput
            id="expires"
            v-model="expiresAt"
            label="Expires"
            type="datetime-local"
            hint="Optional. The link stops working after this."
          />
        </div>
      </form>

      <div
        v-if="lastCreated"
        class="mt-6 flex animate-rise flex-wrap items-center gap-3 rounded-xl border border-brand/25 bg-brand/5 p-4"
      >
        <div class="min-w-0 flex-1">
          <p class="text-[11px] font-medium tracking-wider text-faint uppercase">Your short link</p>
          <a
            :href="lastCreated.shortLink"
            target="_blank"
            rel="noopener noreferrer"
            class="block truncate font-mono text-sm text-brand hover:underline"
          >
            {{ lastCreated.shortLink }}
          </a>
        </div>

        <BaseButton
          variant="secondary"
          size="sm"
          @click="clipboard.copy(lastCreated.shortLink)"
        >
          {{ clipboard.isCopied(lastCreated.shortLink) ? 'Copied!' : 'Copy' }}
        </BaseButton>
      </div>
    </div>
  </section>
</template>
