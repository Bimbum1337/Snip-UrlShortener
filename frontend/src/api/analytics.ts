import { http, toQueryString } from './http'
import type { DashboardSummary, ShortUrlStats } from '@/types'

export const analyticsApi = {
  summary: () => http.get<DashboardSummary>('/api/analytics/summary'),

  forCode: (code: string, days?: number) =>
    http.get<ShortUrlStats>(`/api/analytics/${encodeURIComponent(code)}${toQueryString({ days })}`),
}
