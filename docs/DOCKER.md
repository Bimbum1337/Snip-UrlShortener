# Docker Compose + Seq — Full Walkthrough

This document explains **every file and every line** added to the UrlShortener project
to run it under Docker Compose with Seq structured logging.

It is written to be read top to bottom while you are learning Docker. Nothing is
assumed.

> 📘 **Companion document:** [`DOCKER-REFERENCE.md`](DOCKER-REFERENCE.md) is the
> A→Z reference — what files a Docker project needs in general, how they link
> together, and what every single Dockerfile instruction and compose key means.
> **If you're wondering "do I have to memorize all these commands?", the answer is
> in [Part 0](DOCKER-REFERENCE.md#part-0--do-i-have-to-memorize-all-this) — start
> there.**
>
> - **This file** = what I did to *your* project, and why.
> - **DOCKER-REFERENCE.md** = how Docker works in general, for *any* project.

---

## Table of contents

- [Start here — Docker in 5 minutes](#start-here--docker-in-5-minutes)
- [0. What changed — the file manifest](#0-what-changed--the-file-manifest)
- [1. Docker vocabulary you need first](#1-docker-vocabulary-you-need-first)
- [2. The target architecture](#2-the-target-architecture)
- [Phase 1 — SQL Server in a container](#phase-1--sql-server-in-a-container)
- [Phase 2 — The API in a container](#phase-2--the-api-in-a-container)
- [Phase 3 — The Vue frontend in a container](#phase-3--the-vue-frontend-in-a-container)
- [Phase 4 — Seq structured logging](#phase-4--seq-structured-logging)
- [5. Full file listings, line by line](#5-full-file-listings-line-by-line)
- [6. Command cheat sheet](#6-command-cheat-sheet)
- [7. Troubleshooting](#7-troubleshooting)
- [8. "Why can't I see Docker Compose in Visual Studio?"](#8-why-cant-i-see-docker-compose-in-visual-studio)

---

## Start here — Docker in 5 minutes

If you've never used Docker, read this section first. Everything after it will
make sense.

### The problem Docker solves

Right now, to run this project you need on your machine: .NET 10, SQL Server
LocalDB, Node.js 22, and the right versions of each. If a teammate has SQL Server
2019 instead of 2022, or Node 18 instead of 22, things break in ways that take
hours to diagnose. "Works on my machine" is the entire problem.

Docker's answer: **package the app together with everything it needs to run** into
a single unit, so it behaves identically on every machine.

### The three words that matter

Think of baking a cake.

| Docker word | Cake analogy | What it really is |
|---|---|---|
| **Dockerfile** | The recipe | A text file of instructions: "start with .NET, copy my code, compile it" |
| **Image** | The cake, boxed and frozen | The finished, read-only result of running the recipe |
| **Container** | The cake, out of the box, on a plate, being eaten | A *running* instance of the image |

The important consequences:

- One image can produce **many** containers, the same way one recipe can produce
  many cakes.
- An image is **read-only and frozen**. To change the app, you rebuild the image.
- A container is **disposable**. Delete it, and anything written inside it is
  gone — which is exactly why databases need a **volume** (a box outside the
  container where data actually lives).

### What "in a container" actually means

A container is *not* a virtual machine. There's no second Windows, no second
kernel, no 4 GB of RAM reserved. It's a normal process on your machine that Linux
(or Docker Desktop's Linux VM, on Windows) has been told to lie to. The lies are:

- **"You have your own filesystem."** The container sees only what's in its image.
  It cannot see `C:\Users\Compumarts\`.
- **"You have your own network card and your own `localhost`."** ← This one causes
  90% of beginner confusion. Inside the API container, `localhost` means *the API
  container*, not your Windows machine and not the database container.
- **"You are the only process running."** It sees its own process list.

Hold onto the `localhost` point. It's why the connection string says
`Server=sqlserver` and the Seq URL says `http://seq:80` instead of `localhost`.

### What Docker Compose adds

`docker` alone runs one container at a time, with very long commands:

```
docker run -d --name sqlserver -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=... -p 1433:1433 -v mssql-data:/var/opt/mssql mcr.microsoft.com/mssql/server:2022-latest
```

This app needs **four** containers (database, API, frontend, Seq) that must find
each other. Typing four of those commands in the right order, every time, is
miserable.

**Docker Compose** lets you write all four down in one YAML file (`compose.yaml`)
and start them with one command:

```bash
docker compose up
```

It also does something you'd otherwise have to configure by hand: it puts all four
containers on a **shared private network** and gives each one a hostname equal to
its name in the YAML file. That's why the API can reach the database by typing
`sqlserver`.

### The two numbers in `"5219:8080"`

Port mappings look confusing until you read them in the right direction. The rule
is always **`HOST:CONTAINER`** — outside first, inside second.

```
"5219:8080"
  |     |
  |     +-- port INSIDE the container (where the API is actually listening)
  +-------- port on YOUR Windows machine (what you type in the browser)
```

So `http://localhost:5219` in your browser reaches the API listening on 8080
inside its container.

**But — and this is the part that trips everyone up — containers talking to each
other do NOT use these mappings.** They're on their own private network, where
they see each other's *real* ports directly. That's why:

| Who's talking | Address used | Why |
|---|---|---|
| Your browser → API | `localhost:5219` | Goes through the host port mapping |
| Frontend container → API container | `api:8080` | Private network, container port |
| API container → database | `sqlserver,1433` | Private network, container port |
| SSMS on Windows → database | `localhost,1433` | Goes through the host port mapping |

If you remember nothing else from this document, remember this table.

### What you'll actually type

```bash
docker compose up
```

Starts everything. Leave the terminal open; logs stream into it. `Ctrl+C` stops.

```bash
docker compose up -d
```

Same, but `-d` = detached: it runs in the background and gives you your terminal
back.

```bash
docker compose down
```

Stops and deletes the containers. **Your data survives** (it's in volumes).

```bash
docker compose ps
```

Shows what's currently running.

You're ready for the rest of the document now.

---

## 0. What changed — the file manifest

### New files

| File | Purpose | Phase |
|---|---|---|
| `compose.yaml` | Declares all four services (sqlserver, seq, api, frontend) | 1–4 |
| `.env` | Real values for the `${VARS}` in `compose.yaml`. **Gitignored.** | 1 |
| `.env.example` | Committed template of `.env` so teammates know what to fill in | 1 |
| `.dockerignore` | Excludes junk from the **repo-root** build context (API image) | 2 |
| `src/UrlShortener.Api/Dockerfile` | Multi-stage build of the ASP.NET Core API image | 2 |
| `frontend/Dockerfile` | Multi-stage build of the Vue app → nginx image | 3 |
| `frontend/nginx.conf` | nginx site config: serve SPA, proxy `/api` to the API | 3 |
| `frontend/.dockerignore` | Excludes junk from the **frontend** build context | 3 |
| `docs/DOCKER.md` | This document | — |

### Modified files

| File | Change |
|---|---|
| `src/UrlShortener.Api/UrlShortener.Api.csproj` | Added `Serilog.Sinks.Seq` 9.1.0 |
| `src/UrlShortener.Api/appsettings.json` | Added a third Serilog sink: `Seq` → `http://localhost:5341` |
| `.gitignore` | No net change — it already ignored `.env`; a duplicate entry I briefly added was removed |

### Files deliberately NOT changed

- `Program.cs` — untouched. The Seq sink is added through configuration, not code.
- `frontend/vite.config.ts` — untouched. The dev proxy still works for `npm run dev`.
- Any C# source file — the app has no idea it is in a container.

That last point is the whole design goal: **Docker is configuration, not code.**

---

## 1. Docker vocabulary you need first

Read this once; the rest of the document uses these words precisely.

**Image** — a read-only, layered filesystem snapshot plus a default command. Think
of it as a class. `mcr.microsoft.com/mssql/server:2022-latest` is an image.

**Container** — a running instance of an image. Think of it as an object. It gets
its own filesystem (a thin writable layer on top of the image), its own process
tree, and its own network interface. When you delete it, the writable layer dies
with it.

**Layer** — each instruction in a `Dockerfile` (`COPY`, `RUN`, …) produces one
layer. Docker caches layers. If a layer's inputs are unchanged, Docker reuses the
cached result and skips the work. This is why the order of instructions matters
so much — see Phase 2.

**Build context** — the folder you hand to `docker build`. Docker tars it up and
sends it to the Docker daemon *before* the build starts. A `Dockerfile` can only
`COPY` files that are inside its context. `.dockerignore` trims the context.

**Volume** — storage that lives outside a container's lifecycle. Delete and
recreate the container; the volume's data is still there. Without a volume, your
database is erased every time you rebuild.

**Compose service** — one entry under `services:` in `compose.yaml`. Compose turns
each service into one (or more) container(s) and puts them all on a shared private
network.

**Compose network** — Compose automatically creates a bridge network for the
project and runs an embedded DNS server on it. Inside that network, the hostname
`sqlserver` resolves to the sqlserver container's IP. This is the single most
important thing to understand about multi-container apps, and it is covered in
detail in Phase 2.

**`HOST:CONTAINER` port mapping** — `"5219:8080"` means "traffic arriving at port
5219 on my Windows machine is forwarded to port 8080 inside the container."
Container-to-container traffic does **not** use these mappings at all.

---

## 2. The target architecture

```
Your browser
     |
     |  http://localhost:5173                    http://localhost:5219/abc123
     v                                                    |
+----------------------+                                  |
| frontend (nginx)     |                                  |
|  :8080 in container  |                                  |
|  - serves dist/      |                                  |
|  - proxies /api/ ----+----------+                       |
+----------------------+          |                       |
                                  v                       v
                        +-------------------------------------+
                        | api (ASP.NET Core)                  |
                        |  :8080 in container                 |
                        +----+---------------------------+----+
                             |                           |
                             v                           v
                  +--------------------+      +--------------------+
                  | sqlserver          |      | seq                |
                  |  :1433             |      |  :80 (UI+ingest)   |
                  |  volume mssql-data |      |  volume seq-data   |
                  +--------------------+      +--------------------+

Published to your machine:
  5173 -> frontend:8080      (the dashboard)
  5219 -> api:8080           (API + short-link redirects)
  1433 -> sqlserver:1433     (so SSMS / Azure Data Studio can connect)
  5341 -> seq:80             (the Seq log UI)
```

Why the API stays on its own published port instead of hiding behind nginx: the
short-link redirect route is `GET /{code}` at the **root** of the API
(`RedirectController.cs:13`). If nginx also owned the root, `/abc123` would be
ambiguous — is it a short code or a Vue route? Keeping the API on `:5219` and
setting `Shortener__BaseUrl` to `http://localhost:5219` avoids that collision
entirely and matches how the app already behaved before Docker.

---

## Phase 1 — SQL Server in a container

**Goal:** replace LocalDB with a real SQL Server, without touching the app yet.
The API still runs on your machine with `dotnet run`.

**Concepts:** `image`, `environment`, `ports`, `volumes`, `healthcheck`, `.env`.

### 1.1 The `.env` file

Compose automatically reads a file named `.env` sitting next to `compose.yaml`
and substitutes those values wherever `${NAME}` appears. It is **not** the same
thing as a container's environment variables — it is a variable file for the
compose file itself.

`.env` (gitignored — it holds a password):

```dotenv
MSSQL_SA_PASSWORD=Your_Str0ng!Passw0rd
VISITOR_HASH_SALT=local-dev-salt-change-me
API_PORT=5219
FRONTEND_PORT=5173
SQLSERVER_PORT=1433
SEQ_PORT=5341
```

`.env.example` is the committed copy with the same keys and placeholder values.
The convention is: commit the shape, never the secret. `.gitignore` already had
`.env` and `!.env.example`, so this works out of the box.

Line by line:

- `MSSQL_SA_PASSWORD` — SQL Server refuses to start unless the `sa` password is
  8+ characters with three of: uppercase, lowercase, digit, symbol. If the
  container exits seconds after starting, this is almost always why.
- `VISITOR_HASH_SALT` — feeds `Shortener:VisitorHashSalt`, used by
  `SaltedVisitorHasher` to hash visitor IPs. It was hardcoded to
  `change-me-in-production` in `appsettings.json`; now it comes from the
  environment.
- `API_PORT` / `FRONTEND_PORT` / `SQLSERVER_PORT` / `SEQ_PORT` — the **host-side**
  ports. If port 1433 is already taken by a locally installed SQL Server, you
  change one number here and nothing else.

### 1.2 The `sqlserver` service

```yaml
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-latest
  container_name: urlshortener-sqlserver
  environment:
    ACCEPT_EULA: "Y"
    MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD}
    MSSQL_PID: Developer
  ports:
    - "${SQLSERVER_PORT}:1433"
  volumes:
    - mssql-data:/var/opt/mssql
  healthcheck:
    test:
      - CMD-SHELL
      - /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" -b
    interval: 10s
    timeout: 5s
    retries: 12
    start_period: 30s
  restart: unless-stopped
```

**`image:`** — pull a prebuilt image from Microsoft's registry. Contrast with
`build:` in Phase 2, which builds one from our source. `2022-latest` is a moving
tag; pin to a digest if you ever want true reproducibility.

**`container_name:`** — without this, Compose invents a name like
`urlshortener-sqlserver-1`. Fixing it means `docker logs urlshortener-sqlserver`
always works. (Side effect: you cannot scale this service to multiple replicas.
Irrelevant for a database.)

**`environment:`** — these become environment variables *inside* the container.
The SQL Server image's entrypoint script reads them:
- `ACCEPT_EULA: "Y"` — mandatory. Note the quotes: unquoted `Y` is fine, but
  unquoted `Yes`/`No`/`On`/`Off` are parsed as booleans by YAML, which is a classic
  footgun. Quoting is the safe habit.
- `MSSQL_PID: Developer` — selects the free Developer edition (full Enterprise
  feature set, non-production licence).

**`ports:`** — `"1433:1433"` after substitution. **This line exists only for
tools running on your Windows machine** — SSMS, Azure Data Studio, and in Phase 1
the API running under `dotnet run`. The API *container* in Phase 2 does not use
it. If you deleted this line, Phases 2–4 would still work perfectly.

**`volumes:`** — `mssql-data:/var/opt/mssql`. The left side has no `/`, so it is a
**named volume**, managed by Docker (as opposed to a bind mount like
`./data:/var/opt/mssql`, which maps a host folder). `/var/opt/mssql` is where the
SQL Server image keeps `.mdf`/`.ldf` files. Without this line,
`docker compose down` destroys your entire database. Every named volume must also
be declared in the top-level `volumes:` block at the bottom of the file — that's
what the trailing

```yaml
volumes:
  mssql-data:
  seq-data:
```

is for. The empty value means "default driver, default options."

**`healthcheck:`** — Compose runs `test` inside the container on a schedule and
tracks the container's health state (`starting` → `healthy` / `unhealthy`).
- `CMD-SHELL` means "run this string through `/bin/sh -c`", which is what lets
  `$$MSSQL_SA_PASSWORD` expand.
- `$$` is an **escaped dollar sign**. Compose would otherwise try to substitute
  `${MSSQL_SA_PASSWORD}` from `.env` at parse time; `$$` makes Compose emit a
  literal `$` so the shell *inside the container* does the expansion. Either
  would work here, but the `$$` form keeps the password out of
  `docker compose config` output.
- `/opt/mssql-tools18/bin/sqlcmd` — the path in the 2022 image. Older images used
  `mssql-tools` (no `18`). If your healthcheck never goes healthy, check this path
  first.
- `-C` trusts the server certificate (tools18 enables encryption by default and
  the container uses a self-signed cert). `-b` makes sqlcmd exit non-zero on
  error, which is what turns a failed query into a failed healthcheck.
- `start_period: 30s` — failures during the first 30 seconds don't count against
  `retries`. SQL Server takes 15–25 seconds to accept connections on a cold start.
- `interval` × `retries` = 120 s of grace after the start period.

**`restart: unless-stopped`** — Docker restarts the container if it crashes or if
Docker Desktop restarts, but not if *you* stopped it deliberately.

### 1.3 Running just Phase 1

```bash
docker compose up -d sqlserver
```

`up` creates the network, volume and container. `-d` detaches (runs in the
background). Naming `sqlserver` starts only that service.

Watch it become healthy:

```bash
docker compose ps
```

The `STATUS` column moves from `Up 5s (health: starting)` to `Up 40s (healthy)`.

Then point the app at it. Edit the connection string in
`src/UrlShortener.Api/appsettings.json` (or better, use user-secrets) to:

```
Server=localhost,1433;Database=UrlShortenerDb;User Id=sa;Password=Your_Str0ng!Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=True;Encrypt=false
```

and run `dotnet run --project src/UrlShortener.Api`. `DatabaseInitializer`
(`src/UrlShortener.Infrastructure/Persistence/DatabaseInitializer.cs:12`) applies
migrations on startup because `Database:MigrateOnStartup` is `true`, so the schema
is created for you.

Once that works, you have proven the container is real, reachable, and
persistent. Move to Phase 2.

---

## Phase 2 — The API in a container

**Goal:** stop running `dotnet run` on your machine.

**Concepts:** multi-stage builds, layer caching, build context, container DNS,
.NET configuration overrides, `depends_on` + health conditions.

### 2.1 `src/UrlShortener.Api/Dockerfile`

The whole file, then the explanation:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/UrlShortener.Domain/UrlShortener.Domain.csproj                 src/UrlShortener.Domain/
COPY src/UrlShortener.Application/UrlShortener.Application.csproj       src/UrlShortener.Application/
COPY src/UrlShortener.Infrastructure/UrlShortener.Infrastructure.csproj src/UrlShortener.Infrastructure/
COPY src/UrlShortener.Api/UrlShortener.Api.csproj                       src/UrlShortener.Api/

RUN dotnet restore src/UrlShortener.Api/UrlShortener.Api.csproj

COPY src/ src/

RUN dotnet publish src/UrlShortener.Api/UrlShortener.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN mkdir -p /app/Logs && chown -R app:app /app

COPY --from=build --chown=app:app /app/publish .

USER app

EXPOSE 8080

ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]
```

**`FROM … sdk:10.0 AS build`** — starts a stage from the .NET 10 SDK image (~800 MB;
contains compilers, MSBuild, NuGet). `AS build` names the stage so a later stage
can copy out of it. This is a **multi-stage build**: the SDK stage is used and then
discarded.

**`WORKDIR /src`** — sets the working directory for every following instruction,
and creates it if missing. Equivalent to `cd`, but persistent.

**The four `COPY … .csproj` lines, then `RUN dotnet restore`, then `COPY src/`** —
this ordering is the single most important optimisation in the file, and it looks
redundant until you understand layer caching.

Docker caches per instruction. A cached layer is reused if its inputs are
byte-identical. If you had written the obvious version:

```dockerfile
COPY . .
RUN dotnet restore
RUN dotnet publish ...
```

then editing one line in `ShortUrlsController.cs` changes the `COPY . .` layer,
which invalidates every layer after it — so **NuGet restore re-downloads every
package on every single build.** That's 30–60 seconds per edit.

By copying only the `.csproj` files (which change rarely) and restoring *before*
copying source (which changes constantly), the expensive restore layer stays
cached. Typical rebuild after a code edit drops to a few seconds.

`Directory.Build.props` is copied too because MSBuild reads it for
`<TargetFramework>net10.0</TargetFramework>` — without it, restore fails with
"TargetFramework not set."

Each `COPY src/X/X.csproj src/X/` has a **trailing slash** on the destination.
That tells Docker "destination is a directory." Omit it and Docker writes a file
literally named `src/UrlShortener.Domain`, and restore fails confusingly.

Note the paths are relative to the **repo root**, not to the Dockerfile's folder.
That is because `compose.yaml` sets `context: .`. A Dockerfile can never `COPY`
something outside its context — that is exactly why the context is the root and
not `src/UrlShortener.Api/`.

**`RUN dotnet publish … --no-restore`** — `--no-restore` skips restore because the
previous layer already did it; without the flag, the caching work above is wasted.
`-c Release` builds optimised, `-o /app/publish` puts the output somewhere
predictable for the next stage.

**`FROM … aspnet:10.0 AS final`** — a second stage from the **runtime-only** image
(~220 MB). No compiler, no SDK, no NuGet cache, no source code. Smaller image,
faster pulls, smaller attack surface.

**`RUN mkdir -p /app/Logs && chown -R app:app /app`** — the aspnet base image ships
a non-root user named `app`. Serilog's File sink writes to `Logs/log-.txt`
(relative to the working directory), so the folder must exist and be writable by
that user. Combining both commands with `&&` in one `RUN` produces one layer
instead of two — the standard Dockerfile idiom.

**`COPY --from=build --chown=app:app /app/publish .`** — `--from=build` reaches
into the earlier stage and copies out only the published output. This is the line
that makes multi-stage worth it. `--chown` sets ownership during the copy so you
don't need a second `chown` layer.

**`USER app`** — every instruction after this, and the container's main process,
runs as a non-root user. If the API is ever compromised, the attacker is not root
inside the container.

**`EXPOSE 8080`** — pure documentation/metadata. It publishes nothing. The
`ports:` key in `compose.yaml` is what actually opens a host port. Its real value
is telling the next developer which port to map.

**`ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]`** — the exec form (JSON array).
Use this form, not `ENTRYPOINT dotnet UrlShortener.Api.dll`: the shell form wraps
the process in `/bin/sh -c`, which swallows `SIGTERM`, which means
`docker compose stop` waits the full 10-second timeout and then kills your app
instead of letting it shut down gracefully. With the exec form, `dotnet` is PID 1
and receives the signal directly, so `IHostApplicationLifetime` fires,
`Log.CloseAndFlushAsync()` runs, and buffered logs get written.

### 2.2 `.dockerignore` (repo root)

```
**/bin/
**/obj/
**/.vs/
**/*.user
**/*.suo

**/node_modules/
frontend/dist/

**/Logs/
**/*.log

.git/
.gitignore
.env
.dockerignore
compose.yaml
docs/
README.md
```

Before a build starts, the Docker CLI packs the entire context and ships it to the
daemon. Your repo currently has `bin/`, `obj/`, `frontend/node_modules/` and
`frontend/dist/` on disk — hundreds of megabytes that would be transferred on
every build and then ignored.

Worse than slow: **stale `obj/` folders from a Windows host build can poison the
Linux container build.** `obj/project.assets.json` contains absolute Windows paths
(`C:\Users\...`) that mean nothing inside the container, and NuGet may skip restore
because it thinks the project is already restored. Excluding `bin/` and `obj/` is
not an optimisation; it is a correctness fix.

`**/` matches at any depth. `.env` is excluded so a secret never ends up baked
into an image layer. `.git/` is excluded because build history is not needed to
compile.

### 2.3 The `api` service

```yaml
api:
  build:
    context: .
    dockerfile: src/UrlShortener.Api/Dockerfile
  container_name: urlshortener-api
  environment:
    ASPNETCORE_ENVIRONMENT: Development
    ASPNETCORE_HTTP_PORTS: "8080"
    ConnectionStrings__DbConnectionString: >-
      Server=sqlserver,1433;Database=UrlShortenerDb;User Id=sa;
      Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=true;
      MultipleActiveResultSets=True;Encrypt=false
    Shortener__BaseUrl: http://localhost:${API_PORT}
    Shortener__VisitorHashSalt: ${VISITOR_HASH_SALT}
    Cors__AllowedOrigins__0: http://localhost:${FRONTEND_PORT}
    Serilog__WriteTo__2__Args__serverUrl: http://seq:80
  ports:
    - "${API_PORT}:8080"
  depends_on:
    sqlserver:
      condition: service_healthy
    seq:
      condition: service_started
  restart: unless-stopped
```

**`build:` instead of `image:`** — Compose builds this image from source.
- `context: .` — the build context is the repo root (relative to `compose.yaml`).
- `dockerfile: src/UrlShortener.Api/Dockerfile` — path to the Dockerfile
  **relative to the context**.

**`ASPNETCORE_ENVIRONMENT: Development`** — this drives
`app.Environment.IsDevelopment()` in `Program.cs`, which enables
`app.MapOpenApi()` and EF Core's `EnableSensitiveDataLogging`. For a real
deployment you'd set `Production`.

**Where does the `8080` come from?** Kestrel binds it because the `aspnet` base
image already sets `ASPNETCORE_HTTP_PORTS=8080`. Port 80 would need root, and we
run as `app`; 8080 is the .NET container convention. Confirm it yourself:

```bash
docker image inspect urlshortener-api --format "{{range .Config.Env}}{{println .}}{{end}}"
```

An earlier version of this compose file set `ASPNETCORE_HTTP_PORTS: "8080"`
explicitly. It was removed — it only restated the default and added a number to a
block that already has several.

`launchSettings.json` is irrelevant here — it is a local dev-tooling file that is
not even copied into the image.

### Aside: only ONE of these numbers is a port mapping

This block contains 5219, 8080, 1433, 5173 and 80, which reads like far more
plumbing than it is. Only the `ports:` line maps anything; every other number is
the address *of something else*:

| Line | Number | What it is |
|---|---|---|
| `ports: "5219:8080"` | 5219→8080 | ✅ **The mapping.** A door through the network wall. |
| `Server=sqlserver,1433` | 1433 | Where to *find the database*. |
| `serverUrl: http://seq:80` | 80 | Where to *find Seq*. |
| `Shortener__BaseUrl: …:5219` | 5219 | A string printed into short links for users to click. |
| `Cors__AllowedOrigins__0: …:5173` | 5173 | Which origin to *trust*. A security rule, not a port. |

Like a phone: `ports:` is **your** number; the rest are entries in your contacts.

**`ConnectionStrings__DbConnectionString`** — this is the key .NET+Docker idiom.
The .NET configuration system maps a **double underscore** to the `:` separator:

```
ConnectionStrings__DbConnectionString   ==   ConnectionStrings:DbConnectionString
Shortener__BaseUrl                      ==   Shortener:BaseUrl
Cors__AllowedOrigins__0                 ==   Cors:AllowedOrigins[0]
Serilog__WriteTo__2__Args__serverUrl    ==   Serilog:WriteTo[2]:Args:serverUrl
```

Environment variables sit **above** `appsettings.json` in the default
configuration precedence order, so these override the file without editing it.
That is why not one line of C# had to change. `AddInfrastructure` still calls
`configuration.GetConnectionString("DbConnectionString")` and simply receives the
container value.

Array indices are just numeric segments: `__0`, `__1`, `__2`.

**`Server=sqlserver,1433`** — read this carefully, it is the concept most people
get wrong:

- `sqlserver` is the **Compose service name**, not a hostname you configured
  anywhere. Compose creates a private network for the project and runs a DNS
  resolver on it that maps every service name to that service's container IP.
- `1433` is the **container's** port, not `${SQLSERVER_PORT}`. Published ports
  (`ports:`) are a host↔container mechanism. Container↔container traffic goes
  straight over the private network and ignores them entirely.
- Using `localhost` here would be wrong: inside the api container, `localhost` is
  *the api container itself*. Each container has its own network namespace and its
  own loopback interface.

`TrustServerCertificate=true` is required because the container's TLS certificate
is self-signed. `Encrypt=false` and `MultipleActiveResultSets=True` are carried
over from the original LocalDB string.

**`>-`** is YAML's *folded block scalar with chomping*: it joins the following
indented lines into one line separated by single spaces, and strips the trailing
newline. It exists purely so a long connection string is readable. The result is
`Server=sqlserver,1433;Database=UrlShortenerDb;User Id=sa; Password=...` — note
the space after `;`, which SQL connection strings tolerate.

**`Shortener__BaseUrl: http://localhost:${API_PORT}`** — this value is *not* used
for internal networking. `ShortLinkBuilder` uses it to construct the short URL
that gets returned to the browser and copied to the user's clipboard. It must
therefore be an address **your browser** can reach: `http://localhost:5219`.

**`Cors__AllowedOrigins__0`** — overrides the first entry of the CORS array read
in `ApiServiceCollectionExtensions.cs:26`. Note it overrides element 0 only;
`http://127.0.0.1:5173` from `appsettings.json` remains at index 1.

**`ports: "${API_PORT}:8080"`** → `5219:8080`. Chosen to match the pre-Docker port
so your bookmarks, the vite dev proxy, and any existing short links all keep
working.

**`depends_on:` with conditions** — the long form, not the short list form.
- `sqlserver: condition: service_healthy` — Compose blocks the api container from
  starting until sqlserver's healthcheck passes. This matters because
  `DatabaseInitializer.InitialiseAsync()` runs `Database.MigrateAsync()` during
  startup; against a not-yet-listening SQL Server it would throw and
  `Program.cs`'s catch would log Fatal and exit. (EF's `EnableRetryOnFailure` with
  `MaxRetryCount: 3` gives some cover, but a healthcheck is deterministic.)
- `seq: condition: service_started` — Seq only needs to exist; the Serilog sink
  buffers and retries on its own, so waiting for health would be pointless
  ceremony.

The plain-list form (`depends_on: [sqlserver]`) only waits for the container to be
*created*, not for the software inside it to be *ready* — a distinction that
causes a lot of flaky startups.

### 2.4 Running Phase 2

```bash
docker compose up --build api
```

`--build` forces a rebuild of the image. `api` names one service, and
`depends_on` pulls in `sqlserver` and `seq` automatically. Without `-d` you see
the interleaved logs of all three, which is exactly what you want the first time.

Check it:

```bash
curl http://localhost:5219/health
```

---

## Phase 3 — The Vue frontend in a container

**Goal:** serve the built SPA from nginx instead of the vite dev server.

**Concepts:** build-time vs run-time, static hosting, reverse proxying, SPA
fallback.

The key insight: **a Vue app has no server.** `npm run build` turns it into plain
HTML, CSS and JS. Nothing executes on the server side. So the container's job is
just to hand those files to the browser — and nginx is very good at that. Node is
needed only during the build, and multi-stage lets us throw it away afterwards.

### 3.1 `frontend/Dockerfile`

```dockerfile
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:alpine AS final

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
```

**`node:22-alpine`** — Alpine Linux base, ~50 MB versus ~350 MB for the Debian
variant. Fine here because the build only needs Node itself.

**`COPY package.json package-lock.json ./` then `RUN npm ci`, then `COPY . .`** —
the exact same layer-caching trick as the API's `.csproj` files. `npm ci`
re-downloading ~200 packages on every source edit is the thing being avoided.

**`npm ci` rather than `npm install`** — `ci` installs precisely what
`package-lock.json` specifies, errors if the lockfile is out of sync with
`package.json`, and deletes `node_modules` first. Reproducible and faster. `npm
install` may silently update the lockfile, which is not what you want in a build.

**`RUN npm run build`** — runs `vue-tsc -b && vite build` (from
`frontend/package.json:8`), producing `/app/dist`. Note that TypeScript errors will
now **fail your Docker build**, which is a feature.

**`FROM nginx:alpine AS final`** — a fresh stage. Everything from the Node stage
is gone unless explicitly copied.

**`COPY nginx.conf /etc/nginx/conf.d/default.conf`** — the stock nginx image
includes an `include /etc/nginx/conf.d/*.conf;` inside its `http` block, and ships
a `default.conf` serving its welcome page on port 80. Overwriting that file
replaces the default site. This is why `nginx.conf` here contains a bare `server
{ … }` block and no `http {}` or `events {}` — those come from the image's main
`/etc/nginx/nginx.conf`.

**`COPY --from=build /app/dist /usr/share/nginx/html`** — the compiled SPA into
nginx's document root.

**`CMD ["nginx", "-g", "daemon off;"]`** — `daemon off` is mandatory in
containers. By default nginx forks into the background and the foreground process
exits — and when PID 1 exits, the container stops. `-g` injects that directive
from the command line.

(`CMD` vs `ENTRYPOINT`: `CMD` is a default that `docker run <image> <cmd>` can
replace; `ENTRYPOINT` is fixed. `CMD` is the right choice for a server you might
want to override for debugging.)

### 3.2 `frontend/nginx.conf`

```nginx
server {
    listen 8080;
    server_name _;

    root /usr/share/nginx/html;
    index index.html;

    gzip on;
    gzip_types text/css application/javascript application/json image/svg+xml;
    gzip_min_length 1024;

    location /api/ {
        proxy_pass         http://api:8080;
        proxy_http_version 1.1;
        proxy_set_header   Host              $host;
        proxy_set_header   X-Real-IP         $remote_addr;
        proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }

    location /assets/ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

**`listen 8080`** — not 80. Consistent with the API and with running unprivileged.

**`server_name _`** — `_` is the conventional "match anything" placeholder. There
is only one site in this container, so no virtual-host matching is needed.

**`root` / `index`** — where the files are, and what to serve for a directory
request.

**`gzip`** — `gzip_types` deliberately omits `text/html` because nginx always
gzips that when `gzip on`. `gzip_min_length 1024` avoids the overhead of
compressing tiny responses that would often get *larger*.

**`location /api/ { proxy_pass http://api:8080; }`** — this is what replaces the
vite dev proxy from `frontend/vite.config.ts:15`.

Follow the chain: `frontend/src/api/http.ts:1` sets
`const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''`. We never set
`VITE_API_BASE_URL`, so `BASE_URL` is `''` and every call is a **same-origin
relative** request like `GET /api/urls`. The browser sends it to
`localhost:5173` → nginx → matches `location /api/` → forwarded to `api:8080`
over the Compose network.

Consequence worth noticing: **the browser never makes a cross-origin request, so
CORS never comes into play** for the dashboard. The `Cors__AllowedOrigins__0`
setting is belt-and-braces for anyone hitting `:5219` directly.

Note that `proxy_pass http://api:8080;` has **no trailing path**. That means
nginx forwards the URI unchanged: `/api/urls` arrives at the API as `/api/urls`,
which is what `[Route("api/urls")]` expects. Had it been
`proxy_pass http://api:8080/;` (trailing slash), nginx would strip the matched
`/api/` prefix and the API would see `/urls` → 404. This trailing-slash rule is
one of nginx's sharpest edges.

`proxy_http_version 1.1` — the default is 1.0, which lacks keep-alive and breaks
chunked responses and WebSockets.

The `X-Forwarded-*` headers preserve the real client's information, which would
otherwise be replaced by the nginx container's internal IP. Relevant here because
`RedirectController.cs:20` reads
`HttpContext.Connection.RemoteIpAddress` for visitor hashing — though note that to
*actually* use those headers, the API would need
`app.UseForwardedHeaders(...)`, which is **not** currently configured. Redirects
go straight to `:5219` and bypass nginx anyway, so analytics are unaffected; the
headers are set now so the plumbing is ready if you later put the API behind the
proxy too.

**`location /assets/ { expires 1y; … immutable; }`** — vite emits hashed
filenames (`index-9NCiek3r.js`). The content behind a given name can never change,
so caching for a year is safe and `immutable` tells the browser not to even send
a revalidation request. `index.html` itself is *not* in `/assets/` and stays
uncached, so a new deploy is picked up immediately.

**`location / { try_files $uri $uri/ /index.html; }`** — the SPA fallback, and the
one line people always forget. vue-router owns client-side paths. If you load
`/links/abc123` directly, nginx looks for a file at that path, doesn't find one,
and would return 404 — even though the app handles that route fine. `try_files`
says: try the literal file, then the directory, then fall back to `index.html` so
the Vue app boots and the router takes over.

**`location` matching order**, since it is not top-to-bottom: nginx picks the
longest matching prefix, so `/api/foo` matches `/api/` (5 chars) over `/` (1
char). Order in the file does not matter for prefix locations.

### 3.3 `frontend/.dockerignore`

```
node_modules/
dist/
.env
.env.local
*.log
Dockerfile
.dockerignore
```

A separate file because this build uses a different context (`./frontend`) and
`.dockerignore` is always read from the root of the context.

Excluding `node_modules/` is not just about speed: your host's `node_modules` was
installed on Windows and can contain platform-specific native binaries (esbuild,
for example, ships a per-platform executable). Copying those into a Linux
container produces a genuinely baffling `Cannot execute binary file` error. `npm
ci` inside the container installs the correct Linux builds.

Excluding `dist/` guarantees the image contains a fresh build, not whatever was
last built on your machine.

### 3.4 The `frontend` service

```yaml
frontend:
  build:
    context: ./frontend
    dockerfile: Dockerfile
  container_name: urlshortener-frontend
  ports:
    - "${FRONTEND_PORT}:8080"
  depends_on:
    - api
  restart: unless-stopped
```

`context: ./frontend` — a narrower context than the API's, because the frontend
build needs nothing from the rest of the repo.

`depends_on: [api]` — the short list form is fine here. nginx starts happily
without the API being up; it only needs `api` to resolve in DNS by the time a
request is proxied, and Compose creates the DNS entry when the container is
created.

### 3.5 Running Phase 3

```bash
docker compose up --build frontend
```

Open <http://localhost:5173>.

---

## Phase 4 — Seq structured logging

**Goal:** stop reading `Logs/log-20260723.txt` with your eyes. Query logs instead.

**Concepts:** structured logging, a sidecar observability service, sink
configuration through environment overrides.

### 4.1 Why Seq

The app already uses Serilog, and Serilog already logs **structured** events. In
`Program.cs:38`, the request-logging template is:

```
"HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms"
```

The text file sink flattens that into a string and the structure is lost. Seq
stores the event with its properties intact — `RequestMethod`, `RequestPath`,
`StatusCode`, `Elapsed`, plus the enrichers configured in `appsettings.json`
(`MachineName`, `EnvironmentName`, `Application`) and the custom `RemoteIP` added
by `EnrichDiagnosticContext` at `Program.cs:48`.

That means you can write real queries in the Seq UI:

```
StatusCode >= 500
Elapsed > 500
RequestPath like '/api/urls%' and StatusCode = 409
@Level = 'Error' and Application = 'UrlShortener'
```

None of which is possible against a text file.

### 4.2 NuGet package

`src/UrlShortener.Api/UrlShortener.Api.csproj`, one line added:

```xml
<PackageReference Include="Serilog.Sinks.Seq" Version="9.1.0" />
```

That's the entire code change for Phase 4. It sits alongside the existing
`Serilog.Sinks.Console` and `Serilog.Sinks.File`.

Worth knowing *why* no C# was needed. `Program.cs:26` already does:

```csharp
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));
```

`ReadFrom.Configuration` scans the app's dependency context for assemblies named
`Serilog.Sinks.*` and reflects over them to resolve sink names. Adding the package
reference is literally enough for `"Name": "Seq"` in JSON to start working.

### 4.3 `appsettings.json`

Added as the third element of the existing `Serilog:WriteTo` array:

```json
{
  "Name": "Seq",
  "Args": {
    "serverUrl": "http://localhost:5341"
  }
}
```

- `"Name": "Seq"` maps by convention to the `WriteTo.Seq(...)` extension method in
  the package just added.
- `"Args"` are that method's parameters, matched by name. `serverUrl` is the only
  required one.
- The default `http://localhost:5341` is correct for the case where **Seq runs in
  Docker but the API runs on your machine** — a genuinely useful hybrid while
  you're still debugging in Visual Studio.

Its position matters: it is index **2** in the array (Console = 0, File = 1),
which is what the environment override in `compose.yaml` targets.

Note the sink is fire-and-forget: if Seq is unreachable, the Seq sink retries in
the background and drops events after a while, but it **never** throws into your
request pipeline. Running the API with no Seq at all is safe.

### 4.4 The `seq` service

```yaml
seq:
  image: datalust/seq:latest
  container_name: urlshortener-seq
  environment:
    ACCEPT_EULA: "Y"
    SEQ_FIRSTRUN_NOAUTHENTICATION: "true"
  ports:
    - "${SEQ_PORT}:80"
  volumes:
    - seq-data:/data
  restart: unless-stopped
```

**`ACCEPT_EULA: "Y"`** — required, same as SQL Server. Seq is free for single-user
local development.

**`SEQ_FIRSTRUN_NOAUTHENTICATION: "true"`** — required on Seq 2026.1+, and the
cause of a restart loop if you leave it out. Newer Seq refuses to start until you
make an explicit decision about authentication:

```
[FTL] Error running the server application
System.InvalidOperationException: No default admin password was supplied;
set `firstRun.adminPassword` or `SEQ_FIRSTRUN_ADMINPASSWORD`, or opt out of
authentication using `firstRun.noAuthentication`/`SEQ_FIRSTRUN_NOAUTHENTICATION`.
```

Seq then exits with code 1, `restart: unless-stopped` starts it again, it crashes
again, forever. In Docker Desktop this looks like a container that never settles.

This is a local dev log viewer with nothing sensitive in it, so opting out of
login is the right call. To require a password instead, swap the line for:

```yaml
SEQ_FIRSTRUN_ADMINPASSWORD: ${SEQ_ADMIN_PASSWORD}
```

⚠️ **If Seq already crash-looped once, changing the setting is not enough.** The
failed run left a half-initialised metastore in the volume. Delete it:

```bash
docker compose stop seq && docker compose rm -f seq && docker volume rm urlshortener_seq-data && docker compose up -d seq
```

Look for `[INF] Ingestion enabled` in `docker compose logs seq` to confirm success.

**`ports: "5341:80"`** — the important subtlety: the Seq container serves **both
the web UI and the log-ingestion API on port 80**. There is no separate ingestion
port to map. 5341 is simply the port everyone conventionally associates with Seq,
so the UI lands at <http://localhost:5341> and that also happens to be the
ingestion endpoint for anything running on your host.

**`volumes: seq-data:/data`** — Seq stores its ingested events, saved queries,
dashboards and signals under `/data`. Without the volume, every
`docker compose down` throws all of that away.

No healthcheck: nothing waits on Seq, so there is nothing to gate.

### 4.5 The one line that wires it together

In the `api` service's `environment`:

```yaml
Serilog__WriteTo__2__Args__serverUrl: http://seq:80
```

Decoded through the .NET double-underscore rule:

```
Serilog:WriteTo[2]:Args:serverUrl  =  http://seq:80
```

This overrides **only the URL**, leaving `"Name": "Seq"` and everything else in
`appsettings.json` intact.

It has to be overridden because `http://localhost:5341` from `appsettings.json`
would be wrong inside the container in *both* halves:
- `localhost` inside the api container means the api container itself, not your
  Windows host.
- `5341` is a **host** port. On the Compose network you address the container port
  directly, which is `80`.

Hence `http://seq:80` — service name, container port. Exactly the same reasoning
as `Server=sqlserver,1433` in Phase 2. If you internalise one idea from this whole
document, make it this one.

### 4.6 Running Phase 4

```bash
docker compose up --build
```

No service name → everything starts. Then:

1. Open <http://localhost:5341>.
2. Open <http://localhost:5173> and create a short link.
3. Watch the events appear in Seq in real time.
4. Click any event to expand its structured properties.
5. Try the query `StatusCode >= 400` in the filter bar.

---

## 5. Full file listings, line by line

For quick reference, here is where each concept lives:

| Concept | File | Why it's there |
|---|---|---|
| `${VAR}` substitution | `.env` → `compose.yaml` | Keeps the password out of the committed compose file |
| Named volume | `compose.yaml` `volumes:` | Data survives `down`/`up` |
| Healthcheck | `compose.yaml` sqlserver | So the API doesn't race a cold-starting database |
| `depends_on: service_healthy` | `compose.yaml` api | Deterministic startup ordering |
| Layer-cache ordering | both Dockerfiles | Fast rebuilds; copy manifests → restore → copy source |
| Multi-stage build | both Dockerfiles | Ship runtime only; no SDK, no source, no Node |
| Non-root `USER app` | API Dockerfile | Reduced blast radius |
| Exec-form `ENTRYPOINT` | API Dockerfile | Graceful `SIGTERM` shutdown |
| `daemon off` | frontend Dockerfile | Keep PID 1 in the foreground |
| Service-name DNS | `Server=sqlserver,1433`, `http://seq:80`, `proxy_pass http://api:8080` | Container-to-container addressing |
| `__` config override | `compose.yaml` api `environment:` | Configure .NET without editing appsettings or code |
| SPA fallback | `nginx.conf` `try_files` | Deep links don't 404 |
| Build-context hygiene | both `.dockerignore` files | Speed **and** avoiding host-artifact contamination |

---

## 6. Command cheat sheet

```bash
docker compose up -d sqlserver
```

```bash
docker compose up --build
```

```bash
docker compose ps
```

```bash
docker compose logs -f api
```

```bash
docker compose down
```

```bash
docker compose down -v
```

```bash
docker compose exec api sh
```

```bash
docker compose build --no-cache api
```

Notes on the ones that bite:

- `down` stops and removes containers and the network, but **keeps volumes**.
- `down -v` also deletes the volumes — your database and all Seq history. This is
  the "start completely clean" button; there is no undo.
- `up` alone does **not** rebuild after a code change. Use `up --build`.
- `build --no-cache` ignores every cached layer. Reach for it only when you
  suspect the cache itself is the problem; it's slow by design.
- `exec … sh` opens a shell in a *running* container — the fastest way to check
  "is the file actually where I think it is?"

Connecting SSMS / Azure Data Studio to the containerised database:

```
Server:   localhost,1433
Login:    sa
Password: (whatever is in .env)
Trust server certificate: yes
```

---

## 7. Troubleshooting

**`sqlserver` exits a few seconds after starting.** Almost always the password.
Run `docker compose logs sqlserver` — it says so explicitly. Fix
`MSSQL_SA_PASSWORD` in `.env` (8+ chars, upper + lower + digit + symbol), then
`docker compose down -v && docker compose up -d sqlserver` — the `-v` is needed
because the first boot persisted the rejected password state into the volume.

**Healthcheck never goes healthy.** Check the sqlcmd path inside the image:

```bash
docker compose exec sqlserver ls /opt/mssql-tools18/bin
```

If that directory doesn't exist, the image version changed the tools path; adjust
the `test:` line.

**API logs `A network-related or instance-specific error`.** The host in the
connection string is wrong. It must be `sqlserver`, not `localhost` and not
`(localdb)\MSSQLLocalDB`. Confirm what the container actually received:

```bash
docker compose exec api printenv ConnectionStrings__DbConnectionString
```

**`Bind for 0.0.0.0:1433 failed: port is already allocated`.** You have a real SQL
Server installed on Windows. Change `SQLSERVER_PORT` in `.env` to e.g. `14330`.
Nothing else needs to change — the API talks to port 1433 over the internal
network regardless.

**Frontend loads but every API call fails.** Check nginx is reaching the API:

```bash
docker compose exec frontend wget -qO- http://api:8080/health
```

If that fails, the API container isn't up. If it succeeds but the browser still
errors, open DevTools → Network and confirm requests are going to
`localhost:5173/api/...` and not `localhost:5219/api/...`.

**Nothing appears in Seq.** Verify the override landed:

```bash
docker compose exec api printenv Serilog__WriteTo__2__Args__serverUrl
```

Expected: `http://seq:80`. If it prints nothing, the index is wrong — count the
entries in `Serilog:WriteTo` in `appsettings.json`; Seq must be at index 2.

**The `seq` container restarts forever.** Missing
`SEQ_FIRSTRUN_NOAUTHENTICATION` — see [4.4](#44-the-seq-service). Note that fixing
the setting alone isn't enough once it has crashed; you must also delete the
`seq-data` volume.

**Frontend build fails with `npm ci` and `Missing: … from lock file`.**

```
npm error `npm ci` can only install packages when your package.json and
package-lock.json are in sync.
npm error Missing: @emnapi/core@1.11.2 from lock file
```

`npm ci` is deliberately strict — it installs the lockfile exactly and refuses if
it doesn't match `package.json`. The lockfile here was generated on Windows by
npm 11, and npm 10 (shipped in `node:22`) resolves optional dependency trees
differently, so the two disagreed.

Two fixes were needed together:

1. **`frontend/Dockerfile` now uses `node:24-alpine`** (npm 11) instead of
   `node:22-alpine` (npm 10), matching the npm on the host.
2. **The lockfile was regenerated inside a Linux container**, so it contains the
   platform-specific optional dependencies that a Windows `npm install` omits:

```bash
docker run --rm -v "${PWD}/frontend:/app" -w /app node:24-alpine npm install --package-lock-only
```

That command is worth remembering. Any time a lockfile works on Windows but fails
in a Linux build, regenerating it *inside the build image* is the reliable fix —
it removes the host from the equation entirely.

**Code changes don't show up.** `docker compose up` reuses the existing image. Use
`docker compose up --build`.

**Build is slow every single time.** Confirm `.dockerignore` is being applied —
the first line of build output reports the context size. If it says hundreds of
megabytes, `bin/`, `obj/` or `node_modules/` are leaking in.

---

## 8. "Why can't I see Docker Compose in Visual Studio?"

Short answer: **nothing is broken.** Visual Studio's Solution Explorer does not
show every file in the folder — it shows only what the *solution file* lists. Your
solution is `UrlShortener.slnx`, and before this change it listed exactly four
things:

```xml
<Solution>
  <Folder Name="/src/">
    <Project Path="src/UrlShortener.Domain/UrlShortener.Domain.csproj" />
    <Project Path="src/UrlShortener.Application/UrlShortener.Application.csproj" />
    <Project Path="src/UrlShortener.Infrastructure/UrlShortener.Infrastructure.csproj" />
    <Project Path="src/UrlShortener.Api/UrlShortener.Api.csproj" />
  </Folder>
</Solution>
```

Four `.csproj` files, and nothing else. `compose.yaml` sits at the repo root, which
belongs to no project, so Solution Explorer simply has nowhere to draw it. Same
reason you can't see `README.md` or `.gitignore` there.

(VS Code behaves the opposite way — it shows the raw folder, so everything appears.
This is purely a Visual Studio thing.)

### Three ways to fix it, from simplest to most powerful

#### Option A — Click "Show All Files" (zero changes, 2 seconds)

At the top of Solution Explorer there's a small toolbar icon, **Show All Files**
(two overlapping pages). Click it and Solution Explorer switches to showing
everything on disk, greyed out for files not in the solution.

Downside: it's a per-project toggle and it clutters the tree. Fine for a quick
look, annoying permanently.

#### Option B — List the files in the solution (what I did)

`.slnx` (the new XML solution format you're using) supports a `<File>` element for
loose files. I added a solution folder called `docker`:

```xml
<Folder Name="/docker/">
  <File Path="compose.yaml" />
  <File Path=".dockerignore" />
  <File Path=".env.example" />
  <File Path="src/UrlShortener.Api/Dockerfile" />
  <File Path="frontend/Dockerfile" />
  <File Path="frontend/nginx.conf" />
  <File Path="docs/DOCKER.md" />
</Folder>
```

Line by line:

- `<Folder Name="/docker/">` — a **solution folder**. It's virtual: it groups files
  in Solution Explorer and does *not* create a `docker` directory on disk. The
  leading and trailing `/` are required by the `.slnx` format. This is the same
  mechanism that produces the existing `/src/` folder in your tree.
- Each `<File Path="…" />` — a path **relative to the `.slnx` file**, i.e. relative
  to the repo root. Forward slashes, even on Windows.
- `.env` is deliberately absent — it holds a password and is gitignored. Only the
  committed `.env.example` is listed.

Close and reopen the solution (VS caches `.slnx` on load). You'll now see a
`docker` folder alongside `src`, and double-clicking `compose.yaml` opens it with
YAML syntax highlighting.

**Important:** this is cosmetic. It makes the files *visible and editable*; it does
**not** teach Visual Studio to run them. `F5` still starts only the API project.
For that, you need Option C.

#### Option C — Container Orchestrator Support (F5 launches Compose)

Visual Studio *can* build and debug through Docker Compose, but only when the
solution contains a special project type: a **Docker Compose project**, file
extension `.dcproj`. Without one, VS has no idea Compose is involved.

You add one through the UI:

1. Right-click the **`UrlShortener.Api`** project in Solution Explorer.
2. **Add → Container Orchestrator Support…**
3. Choose **Docker Compose**, then **Linux** as the target OS.

What Visual Studio generates:

| Generated file | What it is |
|---|---|
| `docker-compose.dcproj` | The project that makes VS aware of Compose; becomes the startup project |
| `docker-compose.yml` | Its own compose file, listing the API service |
| `docker-compose.override.yml` | Debug-time overrides (ports, `ASPNETCORE_ENVIRONMENT`) |
| `.dockerignore` | It will want to create one — you already have it |
| `Dockerfile` | It will want to create one in the API project — **you already have one** |

Then `F5` builds the images, starts the Compose stack, and attaches the debugger
to the API *inside its container* — real breakpoints in a containerised app, which
is genuinely useful.

**Why I didn't do this for you**, and you should decide deliberately:

1. **It overwrites your `Dockerfile`.** VS generates its own, structured for its
   debugging workflow (it has extra `base`/`debug` stages and expects to be
   invoked with the solution root as context). It would replace the hand-written,
   commented one this document explains.
2. **It expects `docker-compose.yml`, not `compose.yaml`.** You'd end up with two
   compose files and have to reconcile them, or point the `.dcproj` at yours via
   its `<DockerComposeBaseFilePath>` property.
3. **It's a big jump for someone still learning Docker.** VS hides what it's doing
   — volume-mounting the debugger, rewriting the entrypoint, injecting extra
   layers. When it misbehaves, you're debugging Visual Studio's Docker integration
   rather than learning Docker. Running `docker compose up` in a terminal and
   reading the output teaches you far more right now.

My recommendation: **stay on Option B while you're learning.** Use a terminal for
Compose, use Visual Studio for C#. Once the four phases feel routine and you
specifically want breakpoints inside the container, come back and add Option C
deliberately — ideally on a branch so you can compare what it generates against
the Dockerfile you already understand.

### Related: the Containers window

Independently of all the above, Visual Studio has a **Containers** tool window:

> **View → Other Windows → Containers**

It lists running containers, their logs, environment variables, ports and files —
and it works whether or not you started them from Visual Studio. So once you run
`docker compose up` in a terminal, all four containers appear there and you can
read the API's logs without leaving the IDE. This is probably the single most
useful piece of Docker tooling in VS for you right now, and it required no setup.

### Requirement checklist

If none of the above shows anything, verify:

- **Docker Desktop is installed and running.** The whale icon in the system tray
  must be steady, not animating. Compose is bundled with it — you already have
  Docker 29.4.0 and Compose v5.1.1.
- **The "Container development tools" workload** is installed in Visual Studio
  (Visual Studio Installer → Modify → Individual components). Without it, the
  Containers window and the "Add → Container Orchestrator Support" menu item are
  simply absent.

---

## What to try next (not implemented)

Natural follow-ups once the four phases are comfortable:

- A `compose.override.yaml` for hot-reload development (bind-mount the source and
  run `dotnet watch`).
- Health checks on `api` and `frontend`, not just `sqlserver`.
- `secrets:` instead of environment variables for the SA password.
- `ASPNETCORE_ENVIRONMENT=Production` plus a separate `compose.prod.yaml`.
- OpenTelemetry alongside Seq for traces and metrics.
