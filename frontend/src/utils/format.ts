const numberFormatter = new Intl.NumberFormat()

const relativeFormatter = new Intl.RelativeTimeFormat(undefined, { numeric: 'auto' })

const dateFormatter = new Intl.DateTimeFormat(undefined, {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
})

export const formatNumber = (value: number) => numberFormatter.format(value)

export const formatDate = (iso: string) => dateFormatter.format(new Date(iso))

export function formatRelative(iso: string | null): string {
  if (!iso) return 'never'

  const then = new Date(iso).getTime()
  const diffSeconds = Math.round((then - Date.now()) / 1000)
  const absSeconds = Math.abs(diffSeconds)

  const units: [Intl.RelativeTimeFormatUnit, number][] = [
    ['second', 60],
    ['minute', 60],
    ['hour', 24],
    ['day', 7],
    ['week', 4.35],
  ]

  let value = diffSeconds

  for (const [unit, step] of units) {
    if (Math.abs(value) < step) return relativeFormatter.format(Math.round(value), unit)
    value /= step
  }

  return absSeconds > 60 * 60 * 24 * 30 ? dateFormatter.format(new Date(iso)) : 'a while ago'
}

// Drops the scheme and trailing slash so long destinations fit on one line.
export function prettyUrl(url: string, maxLength = 52): string {
  const trimmed = url.replace(/^https?:\/\//, '').replace(/\/$/, '')
  return trimmed.length <= maxLength ? trimmed : `${trimmed.slice(0, maxLength - 1)}…`
}

export function hostOf(url: string): string {
  try {
    return new URL(url).hostname.replace(/^www\./, '')
  } catch {
    return url
  }
}

// Same string always gives the same hue, so a link keeps its colour in the list.
export function hueFor(seed: string): number {
  let hash = 0
  for (let i = 0; i < seed.length; i++) {
    hash = (hash << 5) - hash + seed.charCodeAt(i)
    hash |= 0
  }
  return Math.abs(hash) % 360
}
