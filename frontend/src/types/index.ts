// Mirrors the DTOs returned by the API.

export interface ShortUrl {
  id: number
  code: string
  shortLink: string
  destination: string
  title: string | null
  isCustomAlias: boolean
  isActive: boolean
  isExpired: boolean
  visitCount: number
  createdAtUtc: string
  expiresAtUtc: string | null
  lastVisitedAtUtc: string | null
}

export interface CreateShortUrlRequest {
  destination: string
  customAlias?: string | null
  title?: string | null
  expiresAtUtc?: string | null
}

export interface UpdateShortUrlRequest {
  destination?: string | null
  title?: string | null
  expiresAtUtc?: string | null
  isActive?: boolean | null
  clearExpiry?: boolean
}

export interface AliasAvailability {
  alias: string
  isAvailable: boolean
  reason: string | null
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

export type ShortUrlSortBy = 'CreatedAt' | 'Visits' | 'LastVisitedAt' | 'Code'

export interface ListQuery {
  page?: number
  pageSize?: number
  search?: string | null
  isActive?: boolean | null
  sortBy?: ShortUrlSortBy
  desc?: boolean
}

export interface DailyVisitCount {
  date: string
  count: number
}

export interface ReferrerCount {
  referrer: string
  count: number
}

export interface ShortUrlStats {
  code: string
  shortLink: string
  destination: string
  totalVisits: number
  uniqueVisitors: number
  lastVisitedAtUtc: string | null
  timeline: DailyVisitCount[]
  topReferrers: ReferrerCount[]
}

export interface DashboardSummary {
  totalLinks: number
  activeLinks: number
  totalVisits: number
  visitsLast7Days: number
  topCode: string | null
  topShortLink: string | null
  topCodeVisits: number
  timeline: DailyVisitCount[]
}
