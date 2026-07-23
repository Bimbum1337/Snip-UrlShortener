import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { shortUrlsApi } from '@/api/shortUrls'
import { ApiError } from '@/api/http'
import { useToastStore } from './toasts'
import type {
  CreateShortUrlRequest,
  ListQuery,
  PagedResult,
  ShortUrl,
  ShortUrlSortBy,
  UpdateShortUrlRequest,
} from '@/types'

const EMPTY_PAGE: PagedResult<ShortUrl> = {
  items: [],
  page: 1,
  pageSize: 10,
  totalCount: 0,
  totalPages: 0,
  hasPrevious: false,
  hasNext: false,
}

export const useLinksStore = defineStore('links', () => {
  const toasts = useToastStore()

  const page = ref<PagedResult<ShortUrl>>(EMPTY_PAGE)
  const isLoading = ref(false)
  const isCreating = ref(false)
  const loadError = ref<string | null>(null)

  const query = ref<Required<Pick<ListQuery, 'page' | 'pageSize' | 'sortBy' | 'desc'>> & ListQuery>({
    page: 1,
    pageSize: 10,
    search: '',
    isActive: null,
    sortBy: 'CreatedAt',
    desc: true,
  })

  const items = computed(() => page.value.items)
  const isEmpty = computed(() => !isLoading.value && items.value.length === 0)
  const hasFilters = computed(() => Boolean(query.value.search) || query.value.isActive !== null)

  async function load() {
    isLoading.value = true
    loadError.value = null

    try {
      page.value = await shortUrlsApi.list(query.value)
    } catch (error) {
      loadError.value = messageOf(error)
      page.value = EMPTY_PAGE
    } finally {
      isLoading.value = false
    }
  }

  // Reset to page 1 on any filter change, otherwise you can end up stranded on
  // page 7 of a 2-page result.
  async function applyFilters(patch: Partial<ListQuery>) {
    query.value = { ...query.value, ...patch, page: patch.page ?? 1 }
    await load()
  }

  async function setSort(sortBy: ShortUrlSortBy) {
    const desc = query.value.sortBy === sortBy ? !query.value.desc : true
    await applyFilters({ sortBy, desc })
  }

  async function goToPage(target: number) {
    query.value = { ...query.value, page: target }
    await load()
  }

  async function create(request: CreateShortUrlRequest): Promise<ShortUrl | null> {
    isCreating.value = true

    try {
      const created = await shortUrlsApi.create(request)
      toasts.success('Short link created.')
      await load()
      return created
    } catch (error) {
      toasts.error(messageOf(error))
      return null
    } finally {
      isCreating.value = false
    }
  }

  async function update(code: string, request: UpdateShortUrlRequest): Promise<ShortUrl | null> {
    try {
      const updated = await shortUrlsApi.update(code, request)
      replaceInPage(updated)
      toasts.success('Link updated.')
      return updated
    } catch (error) {
      toasts.error(messageOf(error))
      return null
    }
  }

  async function toggleActive(link: ShortUrl) {
    return update(link.code, { isActive: !link.isActive })
  }

  async function remove(code: string): Promise<boolean> {
    try {
      await shortUrlsApi.remove(code)
      toasts.success('Link deleted.')
      await load()
      return true
    } catch (error) {
      toasts.error(messageOf(error))
      return false
    }
  }

  function replaceInPage(updated: ShortUrl) {
    page.value = {
      ...page.value,
      items: page.value.items.map((item) => (item.id === updated.id ? updated : item)),
    }
  }

  return {
    page,
    items,
    query,
    isLoading,
    isCreating,
    isEmpty,
    hasFilters,
    loadError,
    load,
    applyFilters,
    setSort,
    goToPage,
    create,
    update,
    toggleActive,
    remove,
  }
})

function messageOf(error: unknown): string {
  return error instanceof ApiError ? error.message : 'Something went wrong. Please try again.'
}
