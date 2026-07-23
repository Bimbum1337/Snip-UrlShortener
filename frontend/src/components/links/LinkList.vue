<script setup lang="ts">
import { ref, watch } from 'vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import LinkRow from './LinkRow.vue'
import EditLinkModal from './EditLinkModal.vue'
import ConfirmDeleteModal from './ConfirmDeleteModal.vue'
import { useLinksStore } from '@/stores/links'
import { useClipboard } from '@/composables/useClipboard'
import { useDebouncedRef } from '@/composables/useDebouncedRef'
import type { ShortUrl, ShortUrlSortBy } from '@/types'

const links = useLinksStore()
const clipboard = useClipboard()

const search = ref('')
const debouncedSearch = useDebouncedRef(search, 300)

watch(debouncedSearch, (value) => links.applyFilters({ search: value || null }))

const editing = ref<ShortUrl | null>(null)
const deleting = ref<ShortUrl | null>(null)

const sortOptions: { value: ShortUrlSortBy; label: string }[] = [
  { value: 'CreatedAt', label: 'Newest' },
  { value: 'Visits', label: 'Most visited' },
  { value: 'LastVisitedAt', label: 'Recently visited' },
  { value: 'Code', label: 'Alias' },
]

const statusOptions: { value: boolean | null; label: string }[] = [
  { value: null, label: 'All' },
  { value: true, label: 'Active' },
  { value: false, label: 'Disabled' },
]
</script>

<template>
  <section class="glass-card overflow-hidden">
    <header class="flex flex-col gap-4 border-b border-line p-5 lg:flex-row lg:items-center lg:justify-between">
      <div>
        <h2 class="text-base font-semibold text-ink">Your links</h2>
        <p class="text-xs text-muted">
          {{ links.page.totalCount }} total{{ links.hasFilters ? ' matching your filters' : '' }}
        </p>
      </div>

      <div class="flex flex-col gap-3 sm:flex-row sm:items-center">
        <div class="sm:w-56">
          <BaseInput id="search" v-model="search" placeholder="Search links…">
            <template #icon>
              <svg viewBox="0 0 24 24" class="size-4" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="11" cy="11" r="7" />
                <path d="M20 20l-3.5-3.5" stroke-linecap="round" />
              </svg>
            </template>
          </BaseInput>
        </div>

        <div class="flex items-center gap-1 rounded-xl border border-line p-1">
          <button
            v-for="option in statusOptions"
            :key="String(option.value)"
            class="rounded-lg px-2.5 py-1.5 text-xs font-medium transition-colors"
            :class="
              links.query.isActive === option.value
                ? 'bg-brand/15 text-brand'
                : 'text-muted hover:text-ink'
            "
            @click="links.applyFilters({ isActive: option.value })"
          >
            {{ option.label }}
          </button>
        </div>

        <select
          :value="links.query.sortBy"
          class="h-10 rounded-xl border border-line bg-surface/60 px-3 text-xs text-ink outline-none"
          @change="links.setSort(($event.target as HTMLSelectElement).value as ShortUrlSortBy)"
        >
          <option v-for="option in sortOptions" :key="option.value" :value="option.value">
            {{ option.label }}
          </option>
        </select>
      </div>
    </header>

    <div v-if="links.isLoading" class="flex flex-col">
      <div v-for="n in 3" :key="n" class="flex items-center gap-4 border-b border-line px-5 py-5 last:border-b-0">
        <div class="size-10 shrink-0 animate-pulse rounded-xl bg-ink/5" />
        <div class="flex-1 space-y-2">
          <div class="h-3 w-28 animate-pulse rounded bg-ink/5" />
          <div class="h-3 w-56 animate-pulse rounded bg-ink/5" />
        </div>
      </div>
    </div>

    <div v-else-if="links.loadError" class="p-10 text-center">
      <p class="text-sm text-danger">{{ links.loadError }}</p>
      <BaseButton variant="secondary" size="sm" class="mt-4" @click="links.load()">Try again</BaseButton>
    </div>

    <div v-else-if="links.isEmpty" class="p-12 text-center">
      <div class="mx-auto grid size-12 place-items-center rounded-2xl bg-brand/10 text-brand">
        <svg viewBox="0 0 24 24" class="size-6" fill="none" stroke="currentColor" stroke-width="1.8">
          <path
            d="M10 13a5 5 0 0 0 7.5.5l3-3a5 5 0 0 0-7-7l-1.5 1.5M14 11a5 5 0 0 0-7.5-.5l-3 3a5 5 0 0 0 7 7l1.5-1.5"
            stroke-linecap="round"
            stroke-linejoin="round"
          />
        </svg>
      </div>

      <p class="mt-4 text-sm font-medium text-ink">
        {{ links.hasFilters ? 'No links match those filters.' : 'No links yet.' }}
      </p>
      <p class="mt-1 text-xs text-muted">
        {{ links.hasFilters ? 'Try a broader search.' : 'Shorten your first URL using the form above.' }}
      </p>
    </div>

    <div v-else>
      <LinkRow
        v-for="link in links.items"
        :key="link.id"
        :link="link"
        :copied="clipboard.isCopied(link.shortLink)"
        @copy="clipboard.copy($event.shortLink)"
        @edit="editing = $event"
        @toggle="links.toggleActive($event)"
        @remove="deleting = $event"
      />
    </div>

    <footer
      v-if="links.page.totalPages > 1"
      class="flex items-center justify-between border-t border-line px-5 py-3"
    >
      <p class="text-xs text-muted">Page {{ links.page.page }} of {{ links.page.totalPages }}</p>

      <div class="flex gap-2">
        <BaseButton
          variant="secondary"
          size="sm"
          :disabled="!links.page.hasPrevious"
          @click="links.goToPage(links.page.page - 1)"
        >
          Previous
        </BaseButton>
        <BaseButton
          variant="secondary"
          size="sm"
          :disabled="!links.page.hasNext"
          @click="links.goToPage(links.page.page + 1)"
        >
          Next
        </BaseButton>
      </div>
    </footer>
  </section>

  <EditLinkModal v-if="editing" :link="editing" @close="editing = null" />
  <ConfirmDeleteModal v-if="deleting" :link="deleting" @close="deleting = null" />
</template>
