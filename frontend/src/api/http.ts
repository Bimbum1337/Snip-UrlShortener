const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

export class ApiError extends Error {
  readonly status: number
  readonly title?: string
  readonly fieldErrors?: Record<string, string[]>

  constructor(
    message: string,
    status: number,
    title?: string,
    fieldErrors?: Record<string, string[]>,
  ) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.title = title
    this.fieldErrors = fieldErrors
  }

  get isNotFound() {
    return this.status === 404
  }

  get isConflict() {
    return this.status === 409
  }
}

interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}

// The API returns ProblemDetails on failure. Flatten it so callers can always
// just show error.message.
async function toApiError(response: Response): Promise<ApiError> {
  let problem: ProblemDetails = {}

  try {
    problem = (await response.json()) as ProblemDetails
  } catch {
    // Not JSON, fall back to the status code below.
  }

  const fieldMessages = problem.errors ? Object.values(problem.errors).flat() : []

  const message =
    problem.detail ??
    (fieldMessages.length > 0 ? fieldMessages.join(' ') : undefined) ??
    problem.title ??
    `Request failed with status ${response.status}`

  return new ApiError(message, response.status, problem.title, problem.errors)
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response

  try {
    response = await fetch(`${BASE_URL}${path}`, {
      ...init,
      headers: {
        'Content-Type': 'application/json',
        ...init?.headers,
      },
    })
  } catch {
    throw new ApiError('Could not reach the server. Is the API running?', 0)
  }

  if (!response.ok) {
    throw await toApiError(response)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export const http = {
  get: <T>(path: string) => request<T>(path),

  post: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'POST', body: JSON.stringify(body) }),

  patch: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'PATCH', body: JSON.stringify(body) }),

  delete: (path: string) => request<void>(path, { method: 'DELETE' }),
}

export function toQueryString(params: Record<string, unknown>): string {
  const search = new URLSearchParams()

  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') continue
    search.append(key, String(value))
  }

  const query = search.toString()
  return query ? `?${query}` : ''
}
