import { http, toQueryString } from './http'
import type {
  AliasAvailability,
  CreateShortUrlRequest,
  ListQuery,
  PagedResult,
  ShortUrl,
  UpdateShortUrlRequest,
} from '@/types'

export const shortUrlsApi = {
  list: (query: ListQuery = {}) =>
    http.get<PagedResult<ShortUrl>>(`/api/urls${toQueryString({ ...query })}`),

  getByCode: (code: string) => http.get<ShortUrl>(`/api/urls/${encodeURIComponent(code)}`),

  create: (request: CreateShortUrlRequest) => http.post<ShortUrl>('/api/urls', request),

  update: (code: string, request: UpdateShortUrlRequest) =>
    http.patch<ShortUrl>(`/api/urls/${encodeURIComponent(code)}`, request),

  remove: (code: string) => http.delete(`/api/urls/${encodeURIComponent(code)}`),

  checkAlias: (alias: string) =>
    http.get<AliasAvailability>(`/api/urls/alias-available/${encodeURIComponent(alias)}`),
}
