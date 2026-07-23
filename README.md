# UrlShortener

A URL shortener with per-link analytics. .NET 10 Web API on the back, Vue 3 on
the front.

## Structure

```
src/
  UrlShortener.Domain/          entities, value objects, errors
  UrlShortener.Application/     use cases, DTOs, interfaces
  UrlShortener.Infrastructure/  EF Core, repositories, migrations
  UrlShortener.Api/             controllers, filters, startup
frontend/                       Vue 3 + Vite + Pinia + Tailwind
```

Domain has no dependencies. Application defines the interfaces
(`IShortUrlRepository`, `IShortCodeGenerator`, `IDateTimeProvider`, ...) and
Infrastructure implements them, so nothing above the persistence layer knows
that EF Core is being used.

## Running

The API serves the redirects, the frontend is the dashboard. Both need to be up.

```bash
dotnet run --project src/UrlShortener.Api
```

```bash
npm --prefix frontend run dev
```

API on http://localhost:5219, frontend on http://localhost:5173. The Vite dev
server proxies `/api` through to the API. The database is created and migrated
on first run.

For a deployed build set `VITE_API_BASE_URL` instead of relying on the proxy.

## Settings

In `src/UrlShortener.Api/appsettings.json`:

- `ConnectionStrings:DbConnectionString` - SQL Server connection string
- `Database:MigrateOnStartup` - apply migrations at boot
- `Shortener:BaseUrl` - public origin used to build the short links
- `Shortener:CodeLength` - length of a generated code
- `Shortener:AnalyticsWindowDays` - default analytics window
- `Shortener:VisitorHashSalt` - salt for the visitor hash, change it
- `Cors:AllowedOrigins` - origins allowed to call the API

## Endpoints

```
POST   /api/urls                        create (optional alias, title, expiry)
GET    /api/urls                        list: search, isActive, sortBy, desc, paging
GET    /api/urls/{code}                 fetch one
PATCH  /api/urls/{code}                 update destination / title / expiry / active
DELETE /api/urls/{code}                 delete
GET    /api/urls/alias-available/{alias} alias availability
GET    /api/analytics/summary           totals and daily timeline
GET    /api/analytics/{code}?days=      per-link stats and top referrers
GET    /api/preview/{code}              resolve without counting a visit
GET    /{code}                          the redirect
GET    /health
```

Errors come back as ProblemDetails. Internally the use cases return a
`Result`/`Result<T>` holding domain errors, and `BaseApiController` is the only
thing that maps those to status codes.

## Some decisions

- **302 rather than 301.** A permanent redirect gets cached and the visits stop
  being counted.
- **Reserved aliases.** `api`, `health`, `openapi` and so on can't be handed out,
  since the API owns those paths.
- **http/https only** for destinations - otherwise `javascript:` and `data:`
  URLs become redirect targets.
- **Visitor IPs are hashed** with a salt and truncated. Enough to count unique
  visitors, not enough to recover the address.
- **A failed analytics write doesn't break the redirect.** It's logged and the
  visitor carries on.

## Migrations

They live in `src/UrlShortener.Infrastructure/Persistence/Migrations`, and the
Infrastructure project acts as its own startup project:

```bash
dotnet ef migrations add <Name> --project src/UrlShortener.Infrastructure --startup-project src/UrlShortener.Infrastructure --output-dir Persistence/Migrations
```
