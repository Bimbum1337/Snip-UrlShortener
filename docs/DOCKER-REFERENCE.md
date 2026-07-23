# Docker Files — Complete Reference, A→Z

The companion to [`DOCKER.md`](DOCKER.md).

`DOCKER.md` explains **what I did to this specific project, and why**.
This document answers the general questions:

1. **What files does a Dockerised project need?** (Part I)
2. **How do those files find each other?** (Part II)
3. **What does every single instruction/keyword mean?** (Parts III–VII)
4. **What do I create for a brand-new project?** (Part VIII)

Parts III–VII are reference material — skim them once, then come back and look
things up. Parts I, II and VIII are meant to be read straight through.

**👉 If you're short on time, read [Part 0](#part-0--do-i-have-to-memorize-all-this)
first. It's the only part you truly need on day one.**

---

## Table of contents

**[Part 0 — Do I have to memorize all this?](#part-0--do-i-have-to-memorize-all-this)** ← start here
- [0.1 The honest answer: no](#01-the-honest-answer-no)
- [0.2 The 5 commands you actually use](#02-the-5-commands-you-actually-use)
- [0.3 Yes — one command up, one command down](#03-yes--one-command-up-one-command-down)
- [0.4 What a normal working day looks like](#04-what-a-normal-working-day-looks-like)
- [0.5 The 3 ideas behind everything else](#05-the-3-ideas-behind-everything-else)

**Part I — The files**
- [I.1 The complete file map](#i1-the-complete-file-map)
- [I.2 Which files are required vs optional](#i2-which-files-are-required-vs-optional)
- [I.3 Where each file must live (and why)](#i3-where-each-file-must-live-and-why)
- [I.4 Naming rules and accepted alternatives](#i4-naming-rules-and-accepted-alternatives)

**Part II — How they link together**
- [II.1 The wiring diagram](#ii1-the-wiring-diagram)
- [II.2 The seven links, one by one](#ii2-the-seven-links-one-by-one)
- [II.3 Build context — the rule that governs everything](#ii3-build-context--the-rule-that-governs-everything)
- [II.4 Following one HTTP request through every file](#ii4-following-one-http-request-through-every-file)

**Part III — Dockerfile instruction reference**
- [III.1 Every instruction, A→Z](#iii1-every-instruction-az)
- [III.2 The three that confuse everyone](#iii2-the-three-that-confuse-everyone)
- [III.3 The shell vs exec form](#iii3-the-shell-vs-exec-form)
- [III.4 Multi-stage builds in depth](#iii4-multi-stage-builds-in-depth)
- [III.5 Layer caching — the complete rules](#iii5-layer-caching--the-complete-rules)

**Part IV — compose.yaml key reference**
- [IV.1 Top-level keys](#iv1-top-level-keys)
- [IV.2 Every service key, A→Z](#iv2-every-service-key-az)
- [IV.3 The `build` block in full](#iv3-the-build-block-in-full)
- [IV.4 `volumes` — the three kinds](#iv4-volumes--the-three-kinds)
- [IV.5 `depends_on` and startup ordering](#iv5-depends_on-and-startup-ordering)
- [IV.6 `healthcheck` in full](#iv6-healthcheck-in-full)
- [IV.7 YAML syntax you'll trip over](#iv7-yaml-syntax-youll-trip-over)

**Part V — .dockerignore reference**
- [V.1 Pattern syntax](#v1-pattern-syntax)
- [V.2 What to always exclude, by stack](#v2-what-to-always-exclude-by-stack)

**Part VI — .env reference**
- [VI.1 Rules and gotchas](#vi1-rules-and-gotchas)
- [VI.2 The four different "env" things](#vi2-the-four-different-env-things)

**Part VII — nginx.conf reference**
- [VII.1 Structure](#vii1-structure)
- [VII.2 Every directive used here](#vii2-every-directive-used-here)
- [VII.3 location matching rules](#vii3-location-matching-rules)

**Part VIII — Starting a new project**
- [VIII.1 The decision tree](#viii1-the-decision-tree)
- [VIII.2 Step-by-step for any project](#viii2-step-by-step-for-any-project)
- [VIII.3 Copy-paste recipes by stack](#viii3-copy-paste-recipes-by-stack)
- [VIII.4 Copy-paste recipes for common services](#viii4-copy-paste-recipes-for-common-services)

**Part IX — CLI command reference**
- [IX.1 docker compose, A→Z](#ix1-docker-compose-az)
- [IX.2 docker, the ones you need](#ix2-docker-the-ones-you-need)
- [IX.3 Debugging workflows](#ix3-debugging-workflows)

---
---

# PART 0 — DO I HAVE TO MEMORIZE ALL THIS?

## 0.1 The honest answer: no

**You need 5 commands. That's it.**

This document has about 40 commands in it. You will use 5 of them every day, 3 of
them occasionally, and the other 32 maybe twice a year — and when you do, you'll
look them up, exactly like you look up a rarely-used LINQ method or a regex
feature.

Think about how you already work with `git`. Git has **150+** commands. Do you know
them all? No. You know maybe six:

```
git status
git add
git commit
git push
git pull
git checkout
```

And that's enough to be productive for years. Docker Compose is the same, except
the "six" is actually five, and they're simpler.

**Nobody memorizes Docker.** Professionals with ten years of experience look up
`.dockerignore` pattern syntax. They Google "docker compose depends_on healthy"
every time. That's normal. This document exists so you have somewhere better than
Google to look.

### What you should actually memorize

Not commands — **concepts**. Three of them, listed in [0.5](#05-the-3-ideas-behind-everything-else).
If you understand those three, you can figure out any command from the docs. If you
memorize commands without the concepts, you'll be stuck the first time something
goes wrong.

---

## 0.2 The 5 commands you actually use

Here they are, with two examples each.

---

### 1️⃣ `docker compose up` — start everything

```bash
docker compose up
```

Starts **all four** of your containers (database, API, frontend, Seq) with one
command. Your terminal fills with logs from all of them, colour-coded per service.
Press `Ctrl+C` to stop.

**Example A — first time today, you just want to see it run:**

```bash
docker compose up
```

You watch the logs, see `Now listening on: http://[::]:8080`, and open your
browser. When you're done, `Ctrl+C`.

**Example B — you changed some C# code and want to see the change:**

```bash
docker compose up --build
```

`--build` rebuilds your images first. **This is the flag you'll forget most often.**
Without it, Docker happily reuses the image it built an hour ago and your new code
simply isn't there — which is confusing, because there's no error, the app just
behaves like the old version.

> 🧠 **Simple rule:** changed code → `--build`. Changed only `compose.yaml` or
> `.env` → plain `up` is enough.

---

### 2️⃣ `docker compose down` — stop everything

```bash
docker compose down
```

Stops and deletes all four containers and the network between them. One command,
everything gone, terminal back.

**Your data is safe.** The database lives in a *volume*, which `down` does not
touch. Run `up` again tomorrow and every short link you created is still there.

**Example A — end of your work day:**

```bash
docker compose down
```

Frees up your RAM. Tomorrow, `docker compose up -d` and you're exactly where you
left off.

**Example B — you want to wipe the database and start clean:**

```bash
docker compose down -v
```

The `-v` also deletes the volumes. Your database, your short links, your Seq log
history — **all gone, permanently, no undo.**

> ⚠️ **`-v` stands for volumes.** This is the one dangerous flag in this whole
> document. `down` = safe, everyday. `down -v` = "delete my data on purpose."

---

### 3️⃣ `docker compose ps` — what's running right now?

```bash
docker compose ps
```

A table of your containers and their status. This is your "is everything OK?" check.

**Example A — you ran `up -d` and want to confirm it worked:**

```
NAME                      STATUS                    PORTS
urlshortener-api          Up 2 minutes              0.0.0.0:5219->8080/tcp
urlshortener-frontend     Up 2 minutes              0.0.0.0:5173->8080/tcp
urlshortener-seq          Up 2 minutes              0.0.0.0:5341->80/tcp
urlshortener-sqlserver    Up 2 minutes (healthy)    0.0.0.0:1433->1433/tcp
```

All four `Up`, and sqlserver says `(healthy)` — its healthcheck is passing.
Everything is fine.

**Example B — something's wrong and you're checking why:**

```
NAME                      STATUS
urlshortener-sqlserver    Up 30 seconds (health: starting)
urlshortener-api          Created
```

The API says `Created`, not `Up` — it hasn't started yet. Why? Because sqlserver
is still `health: starting`, and `depends_on` is holding the API back until the
database is genuinely ready. **Nothing is broken — just wait 20 seconds.**

> 💡 Reading the `STATUS` column is a skill worth 10 minutes of your time.
> `Up` = running. `(healthy)` = its healthcheck passes. `Exited (1)` = it crashed.
> `Created` = waiting on a dependency.

---

### 4️⃣ `docker compose logs` — what is my app saying?

```bash
docker compose logs -f api
```

This is where your `Console.WriteLine` and your Serilog Console output actually go.
When you ran the app in Visual Studio, that text appeared in a console window.
Inside Docker, the container has no window — this command is that window.

**Example A — watching your API live while you click around the UI:**

```bash
docker compose logs -f api
```

`-f` = **follow**. New lines appear as they happen, like `tail -f`. `Ctrl+C` stops
watching (it does **not** stop the container).

**Example B — the API crashed and you want to know why:**

```bash
docker compose logs --tail=50 api
```

Shows the last 50 lines. The crash reason is nearly always in there — an exception,
a connection failure, a missing config value.

> 💡 Leave out the service name (`docker compose logs -f`) to watch **all four**
> services interleaved. Useful once, overwhelming after that. Usually you want one.

---

### 5️⃣ `docker compose config` — did I write my file correctly?

```bash
docker compose config
```

Prints your `compose.yaml` **fully resolved** — every `${VARIABLE}` replaced with
its real value, every default filled in. It doesn't start anything; it just shows
you what Docker actually understood.

**Example A — checking your `.env` is being read:**

You wrote `ports: - "${API_PORT}:8080"`. Run `config` and look:

```yaml
ports:
  - "5219:8080"      # ✅ .env is working
```

versus:

```yaml
ports:
  - ":8080"          # ❌ API_PORT is empty — .env missing or in the wrong folder
```

**Example B — you have a YAML typo:**

```bash
docker compose config
```
```
services.api.ports must be a list
```

Found it in one second, without waiting for a build.

> 💡 **When anything is behaving strangely, run this first.** It's the single most
> useful debugging command in Compose, and beginners almost never know it exists.

---

### That's it. Those 5.

```bash
docker compose up --build     # start (and rebuild)
docker compose down           # stop
docker compose ps             # what's running?
docker compose logs -f api    # what is it saying?
docker compose config         # did I write my file right?
```

Write these on a sticky note. In two weeks you won't need the note.

### The 3 "occasional" ones, for later

```bash
docker compose exec api sh
```
Open a shell **inside** a running container, like remoting into a tiny Linux box.
Use it to answer "is the file really where I think it is?"

```bash
docker compose restart api
```
Restart one container without rebuilding. Fast. Note: it does **not** pick up
changes to `compose.yaml` — use `up -d` for that.

```bash
docker compose build --no-cache api
```
Rebuild ignoring all cached layers. Slow (minutes). Only when you suspect the
cache itself is wrong, which is rare.

---

## 0.3 Yes — one command up, one command down

You asked exactly the right question, and the answer is **yes, that's precisely the
point of Compose.**

### Without Compose

You'd type four separate commands, in the right order, every single time:

```
docker network create urlshortener-net

docker run -d --name sqlserver --network urlshortener-net -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=... -p 1433:1433 -v mssql-data:/var/opt/mssql mcr.microsoft.com/mssql/server:2022-latest

docker run -d --name seq --network urlshortener-net -e ACCEPT_EULA=Y -p 5341:80 -v seq-data:/data datalust/seq

docker build -t urlshortener-api -f src/UrlShortener.Api/Dockerfile .
docker run -d --name api --network urlshortener-net -e ConnectionStrings__DbConnectionString="Server=sqlserver,1433;..." -e ASPNETCORE_HTTP_PORTS=8080 -p 5219:8080 urlshortener-api

docker build -t urlshortener-frontend ./frontend
docker run -d --name frontend --network urlshortener-net -p 5173:8080 urlshortener-frontend
```

And to shut down, four more `docker stop` + four `docker rm` + `docker network rm`.

Every day. Without typos. In the right order (database before API, or the API
crashes).

### With Compose

```bash
docker compose up
```
```bash
docker compose down
```

**That's the whole trade.** `compose.yaml` is a written-down version of all those
flags, and the two commands replace all that typing.

### The mental model: a light switch for a room

```
       Without Compose                    With Compose
       ──────────────────                 ─────────────
   💡 flip 4 separate switches        💡 flip 1 switch for the room
      in the right order                 (wiring is in compose.yaml)

   turn on the lamp                    docker compose up
   turn on the ceiling light
   turn on the fan
   turn on the heater

   ...then 4 more to turn off          docker compose down
```

`compose.yaml` is the **wiring diagram**. You write it once. After that, one switch.

### So does it really start as one "package"?

Yes — Compose calls it a **project**, and it's a real thing, not just a
convenience. Compose groups everything under the project name (which defaults to
your folder name, `urlshortener`):

| Thing | Named |
|---|---|
| Containers | `urlshortener-api`, `urlshortener-sqlserver`, … |
| Network | `urlshortener_default` |
| Volumes | `urlshortener_mssql-data`, `urlshortener_seq-data` |

Because they're all tagged with that project name, Compose knows what belongs
together. That's how `down` knows which four containers to stop — and how it avoids
touching some *other* project's containers running on the same machine.

**You can prove this.** Run `docker compose ps` and you see four containers. Run
plain `docker ps` and you might see six, because two belong to a different project.
Compose only ever manages its own.

### One important limit

You must run these commands **from the folder that contains `compose.yaml`**
(or any subfolder — Compose searches upward).

```bash
cd C:/Users/Compumarts/Desktop/RoadMap/Phase1/phase1_SomeProjects/UrlShortener
docker compose up
```

Run `docker compose up` from your Desktop and you'll get
`no configuration file provided`. That's not a broken install — Compose just can't
find a compose file there. Every project is its own package, in its own folder.

---

## 0.4 What a normal working day looks like

Concretely, here's the rhythm.

### ☀️ Morning — start work

```bash
docker compose up -d
```

`-d` = **detached**: it runs in the background and gives your terminal back. Takes
about 30 seconds (SQL Server is the slow one).

```bash
docker compose ps
```

Confirm all four say `Up`. Now open:
- <http://localhost:5173> — the dashboard
- <http://localhost:5341> — Seq, for logs

### 💻 During the day — you change some C# code

```bash
docker compose up -d --build api
```

Rebuilds and restarts **only the API**. The database and Seq keep running
untouched — which means you don't lose your data or wait for SQL Server to boot
again.

Takes ~5 seconds thanks to layer caching (the whole reason the Dockerfile copies
`.csproj` files before the source).

### 🐛 Something's wrong

```bash
docker compose logs -f api
```

Read the error. Fix the code. Rebuild.

If the logs don't explain it:

```bash
docker compose config
```

Check your configuration is what you think it is.

### 🌙 Evening — stop work

```bash
docker compose down
```

Everything stops. **Your data stays.** Tomorrow morning, `docker compose up -d` and
your short links are all still there.

### 🧹 Occasionally — you want a truly clean slate

```bash
docker compose down -v
docker compose up --build
```

Empty database, fresh migrations, everything rebuilt from scratch. Do this when
you've made a mess, or when you want to prove the setup works from zero.

### That's the whole workflow

Five commands, four scenarios. Everything else in this document is for the day
something unusual happens — and on that day, you'll search this file with `Ctrl+F`
rather than recalling it from memory. **That's the intended use.**

---

## 0.5 The 3 ideas behind everything else

Forget commands. These three ideas explain 90% of Docker problems you'll hit, and
they're what actually deserve memorizing.

---

### Idea 1 — A container is a sealed box with its own `localhost`

The most important sentence in this document:

> **Inside a container, `localhost` means *that container*, not your computer and
> not the other containers.**

Picture four separate apartments in a building. Each has its own front door and its
own internal phone. If someone in Apartment A picks up the internal phone and dials
"home", they reach **their own apartment** — not Apartment B, and not the street
outside.

```
┌─────────────────── Your Windows PC ───────────────────┐
│                                                        │
│   ┌──────────┐  ┌──────────┐  ┌──────────┐            │
│   │   api    │  │ frontend │  │sqlserver │            │
│   │          │  │          │  │          │            │
│   │localhost │  │localhost │  │localhost │            │
│   │  = me    │  │  = me    │  │  = me    │            │
│   └──────────┘  └──────────┘  └──────────┘            │
│                                                        │
│   Your PC's localhost is a FOURTH, separate thing.     │
└────────────────────────────────────────────────────────┘
```

**So how do containers talk to each other?** By the **service name** you wrote in
`compose.yaml`:

```yaml
services:
  sqlserver:      # ← writing this name here...
    image: ...
  api:
    environment:
      # ...creates a hostname you can use here
      ConnectionStrings__…: Server=sqlserver,1433;...
```

Compose runs a tiny DNS server on its private network. Every service name becomes a
working hostname automatically. You configured no IP addresses, no hosts file —
naming the service **is** the configuration.

**Example A — ✅ correct:**
```yaml
Server=sqlserver,1433        # service name → works
```

**Example B — ❌ wrong, and the error is confusing:**
```yaml
Server=localhost,1433        # "localhost" inside the api container = the api container
```
The API tries to find SQL Server *inside itself*, fails, and you get:
> `A network-related or instance-specific error occurred while establishing a
> connection to SQL Server`

which reads like a network problem but is actually a naming mistake.

---

### Idea 2 — Two ports: one for your browser, one for containers

Every port mapping has two numbers, and **using the wrong one** is mistake #2.

```
ports:
  - "5219:8080"
     │     │
     │     └── INSIDE the container. Other containers use this.
     └──────── On YOUR PC. Your browser uses this.
```

Read it as **`OUTSIDE:INSIDE`**. Left = your world, right = container world.

The rule:

| Who is asking? | Which number? | Example |
|---|---|---|
| Your **browser** | Left (host) | `http://localhost:5219/health` |
| **SSMS** on Windows | Left (host) | `localhost,1433` |
| Another **container** | Right (container) | `http://api:8080` |
| The API → database | Right (container) | `Server=sqlserver,1433` |

**Example A — ✅ correct, in `nginx.conf`:**
```nginx
proxy_pass http://api:8080;      # container→container: service name + INSIDE port
```

**Example B — ❌ wrong:**
```nginx
proxy_pass http://api:5219;      # 5219 only exists on your PC, not inside the network
```
nginx returns `502 Bad Gateway`. Nothing is listening on 5219 inside the container.

> 🧠 **Why does it work this way?** Because containers are on their own private
> network where they see each other's *real* ports directly. The `5219:8080`
> mapping is a door punched through the wall of that network so **you** can get in
> from outside. Containers already inside don't need the door.

---

### Idea 3 — Containers are disposable; volumes are not

When you delete a container, **everything written inside it disappears.** That's
by design, not a bug — it's what makes containers reliable and repeatable.

```
┌──────────────────────────────────┐
│  sqlserver container             │
│                                  │
│  /tmp/whatever      ← DELETED    │
│  /app/Logs/*.txt    ← DELETED    │
│  /var/opt/mssql ────┼──────────► │ mssql-data volume  ← SURVIVES ✅
│                     │            │ (lives outside the container)
└─────────────────────┼────────────┘
                      │
              this path is mounted
              to a volume, so writes
              go OUTSIDE the container
```

The line in `compose.yaml` that creates this:
```yaml
volumes:
  - mssql-data:/var/opt/mssql
```

Meaning: *"anything SQL Server writes to `/var/opt/mssql`, store it in the volume
named `mssql-data` instead of inside the container."*

**Example A — ✅ your database survives:**
```bash
docker compose down     # containers deleted
docker compose up -d    # brand new containers...
# ...and all your short links are still there, because the DATA was in a volume
```

**Example B — ❌ this project's Serilog files do NOT survive:**

The API writes log files to `/app/Logs` inside the container, and there's no volume
on that path. So:
```bash
docker compose down     # log files gone forever
```

That's acceptable *here* — because those same log events also go to the Console
(readable via `docker compose logs`) and to Seq (which **does** have a volume). But
it's a real illustration of the rule: **no volume = no persistence.**

> 🧠 **The practical takeaway:** every time you add a database, cache, or anything
> that stores files, ask *"where does this write its data?"* and mount a volume
> there. The path differs per image — there's a lookup table in
> [VIII.2 Step 7](#step-7--add-volumes-for-anything-that-must-persist).

---

### Those three ideas, in one line each

1. **`localhost` inside a container means that container.** Use service names.
2. **Left port = your PC, right port = inside.** Containers use the right one.
3. **Containers are throwaway.** Only volumes survive.

Almost every Docker problem a beginner hits is one of these three, wearing a
disguise. When something breaks, ask which of the three it is before anything else.

---
---

# PART I — THE FILES

## I.1 The complete file map

Here is every Docker-related file in this project and exactly what it does.

```
UrlShortener/
│
├── compose.yaml                    ← THE ORCHESTRATOR. Declares all 4 services.
├── .env                            ← Secret values for compose.yaml. GITIGNORED.
├── .env.example                    ← Committed template of .env.
├── .dockerignore                   ← Excludes files from the ROOT build context.
│
├── src/UrlShortener.Api/
│   └── Dockerfile                  ← Recipe for the API image.
│
├── frontend/
│   ├── Dockerfile                  ← Recipe for the frontend image.
│   ├── nginx.conf                  ← Web-server config, COPYd into that image.
│   └── .dockerignore               ← Excludes files from the FRONTEND context.
│
└── docs/
    ├── DOCKER.md                   ← The phase-by-phase walkthrough.
    └── DOCKER-REFERENCE.md         ← This file.
```

**That's six functional files.** Two docs, four config. It looks like a lot in
Solution Explorer because they're scattered across three directories, but the
mental model is small:

| Count | Kind | Rule |
|---|---|---|
| **1** | `compose.yaml` | One per *project*. Lists every service. |
| **2** | `Dockerfile` | One per *image you build*. We build 2 (api, frontend). |
| **2** | `.dockerignore` | One per *build context*. We have 2 contexts (root, frontend). |
| **1** | `.env` (+ its example) | One per project. |
| **n** | Config files copied into images | `nginx.conf` here. Zero for the API. |

### The one-sentence job of each

| File | One sentence |
|---|---|
| `compose.yaml` | "Here are my 4 services, how to get each one, and how they connect." |
| `Dockerfile` | "Here's how to turn my source code into a runnable image." |
| `.dockerignore` | "Don't send these files to the Docker daemon when building." |
| `.env` | "Here are the values to substitute into `${...}` in compose.yaml." |
| `nginx.conf` | "Here's how nginx should serve files and proxy requests." |

---

## I.2 Which files are required vs optional

| File | Required? | What happens without it |
|---|---|---|
| `compose.yaml` | **Required** for multi-container. | You'd run long `docker run` commands by hand, one per container. |
| `Dockerfile` | **Required only if you build your own image.** | Services that use `image:` (sqlserver, seq) need no Dockerfile at all — that's why there are only 2 Dockerfiles for 4 services. |
| `.dockerignore` | Technically optional. **Practically mandatory.** | Builds are slow and — worse — host artefacts like `obj/` leak in and break the build. See [V.2](#v2-what-to-always-exclude-by-stack). |
| `.env` | Optional. | You'd hardcode passwords into `compose.yaml` and commit them. Don't. |
| `.env.example` | Optional but strong convention. | New teammates clone the repo, get no `.env`, and see cryptic substitution errors. |
| `nginx.conf` | Only for this frontend image. | nginx serves its default welcome page instead of your app. |

**Key realisation for beginners:** `sqlserver` and `seq` have **no Dockerfile**.
They use `image:` to pull a ready-made image someone else built. You only write a
Dockerfile for code *you* wrote.

---

## I.3 Where each file must live (and why)

This is not arbitrary — each location is forced by a rule.

### `compose.yaml` → repo root

**Why:** Compose resolves every relative path in the file (`context: .`,
`context: ./frontend`, `dockerfile: src/...`) **relative to the compose file's own
directory**. Putting it at the root means `.` = the repo, which is what you want.

It also means `docker compose` works from the root without `-f`. Compose searches
the current directory and then walks *up* parent directories looking for a compose
file — so it works from subdirectories too.

### `.env` → next to `compose.yaml`

**Why:** Compose looks for `.env` in the **project directory**, which by default is
the directory containing the compose file. Put it anywhere else and it's silently
ignored — no error, your `${VARS}` just resolve to empty strings.

### `.dockerignore` → the root of each **build context**

**Not** next to the Dockerfile. This is the #1 misplacement.

| Build context | `.dockerignore` must be at |
|---|---|
| `context: .` (repo root) | `./.dockerignore` |
| `context: ./frontend` | `./frontend/.dockerignore` |

Our API's Dockerfile lives at `src/UrlShortener.Api/Dockerfile` but its context is
the repo root — so a `.dockerignore` placed next to that Dockerfile would do
**nothing**. It goes at the root.

> Docker 23+ also supports `<dockerfile-name>.dockerignore` next to the Dockerfile,
> which takes precedence. Ignore that; it confuses more than it helps.

### `Dockerfile` → anywhere inside its context

Convention: next to the thing it builds.
- API's Dockerfile → `src/UrlShortener.Api/` (with the project it builds)
- Frontend's → `frontend/`

The *location* is convention. The *context* is what matters, and it's set in
`compose.yaml`, not by where the Dockerfile sits.

### `nginx.conf` → inside the frontend context

**Why:** the Dockerfile does `COPY nginx.conf /etc/nginx/conf.d/default.conf`, and
`COPY` can only reach files inside the context. Since the frontend's context is
`./frontend`, `nginx.conf` must be in `frontend/`.

---

## I.4 Naming rules and accepted alternatives

### compose file

Docker Compose searches, in this priority order:

1. `compose.yaml`  ← **modern, preferred**
2. `compose.yml`
3. `docker-compose.yaml`
4. `docker-compose.yml`  ← legacy, still extremely common

All four work identically. I used `compose.yaml` because it's the name the Compose
Specification standardised on.

You'll see `docker-compose.yml` everywhere in tutorials and older projects — that's
the pre-2020 name, kept for compatibility.

**Override files** are picked up automatically if present:
- `compose.override.yaml` — merged on top of `compose.yaml` automatically.
- Any other name needs `-f`: `docker compose -f compose.yaml -f compose.prod.yaml up`

Later `-f` files override earlier ones. This is how you keep dev and prod configs
apart without duplicating everything.

### Dockerfile

- Default name: `Dockerfile` (capital D, no extension).
- Any other name requires pointing at it: `dockerfile: Dockerfile.dev` in compose,
  or `docker build -f Dockerfile.dev`.
- Common convention for variants: `Dockerfile.dev`, `Dockerfile.prod`,
  `Dockerfile.test`.

Note our compose has `dockerfile: src/UrlShortener.Api/Dockerfile` — that path is
relative to the **context**, not to compose.yaml. In this case both happen to be
the repo root, so it looks the same. If the context were `./src`, the line would
read `dockerfile: UrlShortener.Api/Dockerfile`.

### The others

- `.dockerignore` — exact name, no alternatives worth using.
- `.env` — exact name. Override with `--env-file other.env`.
- `nginx.conf` — arbitrary, it's just a file we copy. Could be `site.conf`.

---
---

# PART II — HOW THEY LINK TOGETHER

This is the part that's genuinely hard to see from the files alone. Nothing
"auto-discovers" anything — every connection is an explicit line of text pointing
at another file.

## II.1 The wiring diagram

```
                        ┌───────────────┐
                        │     .env      │
                        │ MSSQL_SA_PW=… │
                        │ API_PORT=5219 │
                        └───────┬───────┘
                                │ ① Compose substitutes ${VARS}
                                ▼
    ┌───────────────────────────────────────────────────────────┐
    │                      compose.yaml                          │
    │                                                            │
    │  sqlserver:  image: mcr.../mssql/server  ──────────► ⑦ Docker Hub / MCR
    │  seq:        image: datalust/seq         ──────────► ⑦
    │                                                            │
    │  api:        build:                                        │
    │                context: .            ──② which folder──┐   │
    │                dockerfile: src/…/Dockerfile ──③────┐   │   │
    │              environment:                          │   │   │
    │                ConnectionStrings__…  ──⑥──┐        │   │   │
    │                                            │        │   │   │
    │  frontend:   build:                        │        │   │   │
    │                context: ./frontend  ──②──┐ │        │   │   │
    │                dockerfile: Dockerfile ─③┐│ │        │   │   │
    └────────────────────────────────────────┼┼─┼────────┼───┼───┘
                                             ││ │        │   │
                    ┌────────────────────────┘│ │        │   │
                    │       ┌──────────────────┘ │        │   │
                    ▼       ▼                    │        ▼   ▼
      ┌──────────────────────────┐               │  ┌────────────────────────┐
      │ frontend/Dockerfile      │               │  │ src/…/Api/Dockerfile   │
      │                          │               │  │                        │
      │ COPY package.json ./ ──④─┼──┐            │  │ COPY *.csproj ──④──┐   │
      │ COPY nginx.conf … ───④───┼─┐│            │  │ COPY src/ src/ ─④─┐│   │
      │ COPY --from=build … ──⑤  │ ││            │  │ COPY --from=build ⑤│   │
      └──────────────────────────┘ ││            │  └────────────────────┼───┘
                                   ││            │                       │
                   ┌───────────────┘│            │                       │
                   ▼                ▼            │                       ▼
      ┌────────────────────┐  ┌──────────────┐   │        ┌──────────────────────┐
      │ frontend/nginx.conf│  │  frontend/   │   │        │  repo root files     │
      │                    │  │  (context)   │   │        │  (context)           │
      │ proxy_pass         │  │              │   │        │                      │
      │   http://api:8080 ─┼──┼──────────────┼───┼───┐    │  filtered by          │
      └────────────────────┘  │ filtered by  │   │   │    │  .dockerignore ⑧     │
                              │ frontend/    │   │   │    └──────────────────────┘
                              │ .dockerignore│   │   │
                              └──────────────┘   │   │
                                                 │   │  ⑨ service-name DNS
                                                 ▼   ▼
                                  ┌──────────────────────────────┐
                                  │  appsettings.json            │
                                  │  (inside the API image)      │
                                  │  overridden by ⑥             │
                                  └──────────────────────────────┘
```

Nine links. Each is one line of text in one file. Let's walk them.

---

## II.2 The seven links, one by one

### Link ① — `.env` → `compose.yaml`

**The line in `.env`:**
```dotenv
API_PORT=5219
```

**The line in `compose.yaml`:**
```yaml
ports:
  - "${API_PORT}:8080"
```

**How it works:** before Compose does *anything*, it reads `.env` and performs
plain text substitution on the compose file. By the time Compose parses YAML, the
line literally reads `"5219:8080"`.

**Prove it to yourself:**
```bash
docker compose config
```
This prints the fully-resolved compose file with every `${VAR}` replaced. It is the
single best debugging tool for compose problems — it shows you exactly what Compose
thinks you wrote.

**Gotcha:** `.env` only affects the **compose file**. It does NOT automatically
become environment variables inside your containers. For that you need
`environment:` or `env_file:`. See [VI.2](#vi2-the-four-different-env-things).

---

### Link ② — `compose.yaml` → the build context folder

**The line:**
```yaml
build:
  context: .
```

**How it works:** this tells Docker "the folder `.` (relative to compose.yaml, so
the repo root) is the set of files available to this build."

Before the build starts, the Docker CLI **packs that entire folder into a tar
archive and uploads it to the Docker daemon**. This is why context size matters —
and why `.dockerignore` exists.

You'll see this in the build output:
```
=> [internal] load build context
=> => transferring context: 2.34MB
```

If that number is 400MB, your `.dockerignore` isn't working.

**The critical consequence:** a Dockerfile can **never** `COPY` a file outside its
context. `COPY ../something` is an error. If your build needs a file, it must be
inside the context folder — full stop.

---

### Link ③ — `compose.yaml` → the Dockerfile

**The line:**
```yaml
build:
  context: .
  dockerfile: src/UrlShortener.Api/Dockerfile
```

**How it works:** the path is relative to the **context** (link ②), not to
compose.yaml. Here they're both the repo root so it's indistinguishable, but the
rule matters:

```yaml
# If context were ./src, this same Dockerfile would be:
build:
  context: ./src
  dockerfile: UrlShortener.Api/Dockerfile
```

If `dockerfile:` is omitted, Docker looks for a file named `Dockerfile` at the root
of the context. The frontend uses this default (`dockerfile: Dockerfile` is
redundant there — I wrote it out for clarity).

---

### Link ④ — Dockerfile `COPY` → files in the context

**The lines:**
```dockerfile
COPY Directory.Build.props ./
COPY src/UrlShortener.Api/UrlShortener.Api.csproj src/UrlShortener.Api/
COPY src/ src/
```

**How it works:** the **source** path is relative to the context root. The
**destination** is relative to the current `WORKDIR` inside the image.

So `COPY src/ src/` means:
> take `<repo-root>/src/` from the tarball, put it at `/src/src/` inside the image
> (because `WORKDIR /src` was set earlier).

**Why the API's context is the repo root, not the API folder:** the Dockerfile
needs `Directory.Build.props` (at the repo root, defines `net10.0`) and all four
`.csproj` files. If the context were `src/UrlShortener.Api/`, none of those would
be reachable, and there is no way to escape the context.

**This is the single most common Dockerfile error for beginners:**
```
ERROR: failed to compute cache key: "/Directory.Build.props" not found
```
It always means: the file isn't in the context, or `.dockerignore` excluded it.

---

### Link ⑤ — Dockerfile stage → Dockerfile stage

**The lines:**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
...
RUN dotnet publish ... -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
COPY --from=build /app/publish .
```

**How it works:** `AS build` names the first stage. `--from=build` in a later stage
copies files *out of* that stage's filesystem instead of out of the build context.

This is a link **within one file**, and it's what makes the final image small — the
SDK stage (with compilers, NuGet cache, source code) is discarded entirely. Only
`/app/publish` survives.

You can also copy from an external image: `COPY --from=nginx:alpine /etc/nginx/... .`

---

### Link ⑥ — `compose.yaml` environment → the app's configuration

**The line in `compose.yaml`:**
```yaml
environment:
  ConnectionStrings__DbConnectionString: Server=sqlserver,1433;...
```

**The line in `appsettings.json` (inside the image):**
```json
"ConnectionStrings": { "DbConnectionString": "Server=(localdb)\\MSSQLLocalDB;..." }
```

**The line in C# (unchanged):**
```csharp
configuration.GetConnectionString("DbConnectionString")
```

**How it works:** .NET's configuration system layers sources in a defined order.
Environment variables sit **above** `appsettings.json`, so they win. The `__`
(double underscore) maps to the `:` section separator because `:` isn't legal in
env var names on Linux.

```
ConnectionStrings__DbConnectionString  →  ConnectionStrings:DbConnectionString
Serilog__WriteTo__2__Args__serverUrl   →  Serilog:WriteTo[2]:Args:serverUrl
Cors__AllowedOrigins__0                →  Cors:AllowedOrigins[0]
```

**Why this design is good:** the image is built once and configured at run time.
The same image can run in dev, staging and prod with different connection strings —
you never rebuild to change config, and no secret is baked into a layer.

**Equivalents in other stacks** (this concept exists everywhere):

| Stack | How env vars reach config |
|---|---|
| .NET | Built in. `__` → `:`. Automatic. |
| Node | `process.env.DATABASE_URL`, usually via `dotenv` |
| Python/Django | `os.environ["DATABASE_URL"]`, often via `django-environ` |
| Spring Boot | `SPRING_DATASOURCE_URL` → `spring.datasource.url`; `_` → `.` |
| Rails | `ENV["DATABASE_URL"]` |

---

### Link ⑦ — `image:` → a registry

**The line:**
```yaml
image: mcr.microsoft.com/mssql/server:2022-latest
```

**How it works:** Docker parses this into three parts:

```
mcr.microsoft.com  /  mssql/server  :  2022-latest
└── registry ──────┘  └─ repo ────┘   └─ tag ───┘
```

- **Registry** — omitted means Docker Hub (`docker.io`). `mcr.microsoft.com` is
  Microsoft's own registry. Others: `ghcr.io` (GitHub), `quay.io`.
- **Repository** — the image name. On Docker Hub, single-word names like `nginx`
  are *official images* (curated); `datalust/seq` is a user/org image.
- **Tag** — a version label. `latest` is **not** "the newest" — it's just the
  default tag name, and it moves. For anything that matters, pin a real version.

Docker pulls the image on first use and caches it locally forever. `docker images`
lists what you have; `docker compose pull` refreshes them.

---

### Link ⑧ — `.dockerignore` → the context

**The lines:**
```
**/bin/
**/obj/
**/node_modules/
```

**How it works:** applied while packing the context tarball (link ②). Excluded
files never reach the daemon, so `COPY` cannot see them even if a `COPY` line
would otherwise match.

**This is filtering, not deletion.** Your local `bin/` folders are untouched.

---

### Link ⑨ — container → container, by service name

**The lines:**
```yaml
# compose.yaml — the service is NAMED sqlserver
services:
  sqlserver:
    ...
  api:
    environment:
      ConnectionStrings__…: Server=sqlserver,1433;...   # ← used as a hostname
```
```nginx
# frontend/nginx.conf
proxy_pass http://api:8080;                              # ← service name again
```

**How it works:** Compose creates a private bridge network for the project and runs
an embedded DNS server on it at `127.0.0.11`. Every service name is registered as a
hostname resolving to that container's IP.

You wrote no hostnames, no IPs, no `/etc/hosts` entries. Naming the service
`sqlserver` in YAML **is** what creates the DNS name `sqlserver`.

**Prove it:**
```bash
docker compose exec api getent hosts sqlserver
```

**The two rules that follow, and they are the whole ballgame:**

1. **Use the service name, never `localhost`.** Inside the api container,
   `localhost` is the api container itself.
2. **Use the CONTAINER port, never the published one.** `sqlserver,1433` not
   `sqlserver,${SQLSERVER_PORT}`. Published ports are a host↔container thing;
   containers on the same network bypass them entirely.

---

## II.3 Build context — the rule that governs everything

If you internalise one mechanical fact, make it this one.

```
context: .                              ← you choose a folder here
   │
   ▼
[ Docker tars up that folder ]          ← minus .dockerignore patterns
   │
   ▼
[ Uploads it to the Docker daemon ]     ← "transferring context: 2.34MB"
   │
   ▼
COPY <src> <dst>                        ← <src> can ONLY be inside that tarball
```

**Everything a build can see, it sees through the context.** Not your filesystem.
Not the parent folder. Not absolute paths on your host. Just the tarball.

### Consequences that surprise people

| You want to... | Result |
|---|---|
| `COPY ../shared/lib.dll .` | **Error.** Cannot escape the context. Widen the context instead. |
| `COPY /etc/hosts .` | **Error.** That's a host path, not a context path. |
| Build with `context: ./frontend` but need a root file | **Impossible.** Change to `context: .` and use `COPY frontend/...`. |
| Reference a file excluded by `.dockerignore` | **Error**, "not found". Confusing, because the file *is* on disk. |

### Choosing your context: the rule

> **Set the context to the smallest folder that contains every file the build
> needs.**

| Project shape | Context |
|---|---|
| Single app, self-contained | The app folder |
| Multi-project solution with shared props/packages (this one) | The repo root |
| Monorepo with shared packages | The repo root |
| Frontend with no shared deps (this one) | The frontend folder |

That's exactly why this project has two different contexts: the API needs
`Directory.Build.props` and sibling `.csproj` files, the frontend needs nothing
outside `frontend/`.

---

## II.4 Following one HTTP request through every file

Concrete end-to-end trace. You click "Shorten" in the dashboard.

```
1. Browser: POST http://localhost:5173/api/urls
   │
   │  WHY that URL: frontend/src/api/http.ts:1
   │    const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''
   │  We never set VITE_API_BASE_URL → BASE_URL is '' → the call is
   │  same-origin relative: /api/urls
   ▼
2. Windows port 5173
   │
   │  WHY it's open: compose.yaml → frontend → ports: "${FRONTEND_PORT}:8080"
   │  ${FRONTEND_PORT} came from .env → 5173
   ▼
3. frontend container, nginx listening on 8080
   │
   │  WHY 8080: frontend/nginx.conf → listen 8080;
   │  WHY nginx is running: frontend/Dockerfile → CMD ["nginx","-g","daemon off;"]
   ▼
4. nginx matches location /api/
   │
   │  WHY: nginx.conf → location /api/ { proxy_pass http://api:8080; }
   │  It beats location / because prefix matching picks the LONGEST match.
   ▼
5. DNS lookup of "api" on the Compose network
   │
   │  WHY it resolves: compose.yaml has a service literally named `api`.
   │  Compose's embedded DNS registers every service name.
   ▼
6. api container, Kestrel listening on 8080
   │
   │  WHY 8080: compose.yaml → api → environment → ASPNETCORE_HTTP_PORTS: "8080"
   ▼
7. ASP.NET routes to ShortUrlsController.Create
   │
   │  WHY: [Route("api/urls")] + [HttpPost]
   │  NOTE the path is still /api/urls — proxy_pass had NO trailing slash,
   │  so nginx forwarded the URI unchanged. A trailing slash would have
   │  stripped /api/ and produced a 404.
   ▼
8. EF Core opens a SQL connection to "sqlserver"
   │
   │  WHY: compose.yaml → api → ConnectionStrings__DbConnectionString
   │       → Server=sqlserver,1433
   │  This OVERRODE appsettings.json's LocalDB string via the __ rule.
   ▼
9. sqlserver container, SQL Server on 1433
   │
   │  Data written to /var/opt/mssql, which is the named volume mssql-data,
   │  so it survives `docker compose down`.
   ▼
10. Serilog writes the request log to three sinks
   │
   │  Console  → visible in `docker compose logs api`
   │  File     → /app/Logs/log-*.txt inside the container (NOT persisted!)
   │  Seq      → http://seq:80
   │            appsettings.json says localhost:5341;
   │            compose.yaml overrode it with
   │            Serilog__WriteTo__2__Args__serverUrl: http://seq:80
   ▼
11. Response travels back: api → nginx → browser
```

Every arrow in that trace is a line you can point at in a file. Nothing is magic.

---
---

# PART III — DOCKERFILE INSTRUCTION REFERENCE

## III.1 Every instruction, A→Z

Alphabetical. Instructions marked **★** are the ones you'll use constantly;
the rest are worth recognising.

---

### `ADD`

```dockerfile
ADD https://example.com/file.tar.gz /tmp/
ADD archive.tar.gz /app/          # auto-extracts!
```

Like `COPY`, plus two extra behaviours: it can fetch URLs, and it
**auto-extracts local tar archives**.

**Use `COPY` instead**, almost always. `ADD`'s auto-extraction is surprising — if
you `ADD data.tar.gz /app/` expecting a file, you get an unpacked directory. The
official guidance is: `COPY` unless you specifically need extraction.

Not used in this project.

---

### `ARG`

```dockerfile
ARG NODE_VERSION=22
FROM node:${NODE_VERSION}-alpine

ARG BUILD_CONFIG=Release
RUN dotnet publish -c $BUILD_CONFIG
```

A **build-time** variable. Set with `--build-arg` or compose's `build.args`.

**`ARG` vs `ENV` — the critical difference:**

| | `ARG` | `ENV` |
|---|---|---|
| Available during build | Yes | Yes |
| Available at run time | **No** | Yes |
| Visible in `docker inspect` | No | **Yes** |
| Survives into the final image | No | Yes |

**Never put a secret in `ENV`** — it's baked into the image and readable by anyone
who pulls it. `ARG` is better but still appears in build history. For real secrets,
use BuildKit's `RUN --mount=type=secret`.

`ARG` declared before the first `FROM` is special: it's only usable in `FROM`
lines. To use it inside a stage, re-declare it after `FROM`.

Not used in this project — nothing varies at build time.

---

### ★ `CMD`

```dockerfile
CMD ["nginx", "-g", "daemon off;"]      # exec form  ← use this
CMD nginx -g "daemon off;"              # shell form
CMD ["--verbose"]                       # args-only form (with ENTRYPOINT)
```

The **default command** the container runs. Key word: *default* — it's overridden
by anything you append to `docker run <image> <here>`.

**Only the last `CMD` in a Dockerfile takes effect.** Earlier ones are silently
discarded.

**Used here:** `frontend/Dockerfile`. `daemon off;` is mandatory — nginx normally
forks into the background, the foreground process exits, and since that process is
PID 1, the container stops immediately. See [III.2](#iii2-the-three-that-confuse-everyone).

---

### ★ `COPY`

```dockerfile
COPY src/ dest/
COPY --from=build /app/publish .
COPY --chown=app:app files/ /app/
COPY package.json package-lock.json ./
```

Copies from the build context (or another stage) into the image.

**Rules that bite:**

1. **Source is relative to the context root.** Not to the Dockerfile.
2. **Source cannot escape the context.** No `../`.
3. **Destination ending in `/` means "directory".** Without it, Docker may create
   a *file* with that name. This is why the API Dockerfile has
   `COPY src/X/X.csproj src/X/` — that trailing slash is load-bearing.
4. **`COPY dir/ dest/` copies the *contents*, not the directory itself.**
   `COPY src/ src/` puts the contents of `src/` into `/src` (given
   `WORKDIR /src`... actually into `<workdir>/src`).
5. **Wildcards are shell-globs, not regex,** and they do not create directories.
   `COPY **/*.csproj ./` famously flattens everything into one folder — which is
   why the API Dockerfile lists all four `.csproj` paths explicitly instead.

**Flags:**
- `--from=<stage|image>` — copy out of another stage. The heart of multi-stage.
- `--chown=user:group` — set ownership during the copy. Saves a `RUN chown` layer.
- `--chmod=755` — set permissions (BuildKit).

**Used here:** both Dockerfiles, extensively.

---

### `ENTRYPOINT` ★

```dockerfile
ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]     # exec form ← use this
ENTRYPOINT dotnet UrlShortener.Api.dll            # shell form ← avoid
```

The command that **always** runs. Unlike `CMD`, arguments appended to `docker run`
are passed *to* it rather than replacing it.

**`ENTRYPOINT` vs `CMD` decision:**

| Situation | Use |
|---|---|
| The image *is* one program (our API) | `ENTRYPOINT` |
| The image is a base you'd want to override for debugging | `CMD` |
| Fixed program with default args | `ENTRYPOINT ["prog"]` + `CMD ["--default-arg"]` |

**Why the exec form matters here** — this is a real bug, not style:

Shell form becomes `/bin/sh -c "dotnet UrlShortener.Api.dll"`. `sh` is PID 1;
`dotnet` is its child. When you run `docker compose stop`, Docker sends `SIGTERM`
to PID 1. `sh` does not forward signals to children. So:

- `dotnet` never receives `SIGTERM`
- ASP.NET's graceful shutdown never runs
- `Log.CloseAndFlushAsync()` in `Program.cs`'s `finally` never runs
- Buffered Seq events are **lost**
- Docker waits 10 seconds, then `SIGKILL`s everything

With the exec form, `dotnet` **is** PID 1 and gets the signal directly. Graceful
shutdown works. Logs flush.

**Used here:** `src/UrlShortener.Api/Dockerfile`.

---

### ★ `ENV`

```dockerfile
ENV ASPNETCORE_URLS=http://+:8080
ENV NODE_ENV=production PATH="/app/bin:$PATH"
```

Sets an environment variable **inside the image**, persisting to run time.

**We deliberately don't use `ENV` here.** All configuration comes from
`compose.yaml`'s `environment:` instead. Why: `ENV` bakes the value into the image,
so changing it requires a rebuild, and the same image can't serve dev and prod.
Compose's `environment:` is applied at container start — same image, different
config.

**Rule of thumb:** `ENV` for things intrinsic to the image (`NODE_ENV=production`,
`PATH`). `environment:` in compose for anything deployment-specific.

---

### `EXPOSE`

```dockerfile
EXPOSE 8080
```

**This does nothing functional.** It publishes no ports and opens no firewall.

It is metadata: documentation for humans, plus it enables `docker run -P`
(capital P) to auto-publish to random host ports.

The thing that actually opens a host port is `ports:` in `compose.yaml`.

**Used here:** both Dockerfiles, purely as documentation.

---

### `FROM` ★

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
FROM scratch
FROM node:22-alpine
```

Starts a build stage from a base image. Must be the first instruction (after
optional `ARG`s and comments).

- `AS <name>` names the stage for `COPY --from=<name>`.
- Multiple `FROM`s = multi-stage build.
- `FROM scratch` = literally empty; used for static Go/Rust binaries.

**Choosing a tag — this matters more than beginners expect:**

| Tag style | Size | When |
|---|---|---|
| `:10.0` | Medium (Debian) | **Default choice.** Broad compatibility. |
| `:10.0-alpine` | Small (~5MB base) | Size matters. Uses musl libc — some native deps break. |
| `:10.0-slim` | Small-ish | Debian, minimal packages. Good middle ground. |
| `:latest` | ??? | **Avoid.** Moves without warning; breaks builds silently. |

We use `sdk:10.0` / `aspnet:10.0` (Debian) for .NET and `node:22-alpine` /
`nginx:alpine` for the frontend — Alpine is safe for those because nothing needs
glibc-specific native modules.

---

### `HEALTHCHECK`

```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
```

Defines how Docker tests whether the container is *working*, not merely *running*.

We define our healthcheck in `compose.yaml` instead (on `sqlserver`). Either
location works; compose is more flexible because you can change it without
rebuilding. Dockerfile is better if you're publishing an image for others.

---

### `LABEL`

```dockerfile
LABEL org.opencontainers.image.source="https://github.com/you/repo"
LABEL maintainer="you@example.com"
```

Arbitrary key/value metadata, readable via `docker inspect`. Useful in CI for
traceability. Not used here.

---

### `ONBUILD`

```dockerfile
ONBUILD COPY . /app
```

Registers an instruction that fires when *someone else* uses this image as their
`FROM`. Confusing and rarely justified. Avoid.

---

### ★ `RUN`

```dockerfile
RUN dotnet restore src/UrlShortener.Api/UrlShortener.Api.csproj
RUN mkdir -p /app/Logs && chown -R app:app /app
RUN npm ci
```

Executes a command **during the build**, and commits the resulting filesystem as a
new layer.

**The layer rule:** each `RUN` = one layer, permanently. This has a famous
consequence:

```dockerfile
RUN wget https://example.com/big-500mb-file.zip
RUN unzip big-500mb-file.zip
RUN rm big-500mb-file.zip          # ← the 500MB is STILL in the image!
```

Layers are stacked, not merged. Deleting a file in a later layer just marks it
hidden; the bytes remain in the earlier layer. The fix is to do it all in one
`RUN`:

```dockerfile
RUN wget ... && unzip ... && rm ...
```

This is why you see long `&&`-chained `RUN` commands everywhere. Our
`RUN mkdir -p /app/Logs && chown -R app:app /app` follows the same idiom (for
layer count, not size).

**`RUN` vs `CMD`:** `RUN` happens at **build** time and its result is baked in.
`CMD` happens at **run** time. Beginners frequently write `RUN npm start` and
wonder why the build hangs forever — it does, because `RUN` waits for the command
to finish and a server never finishes.

---

### `SHELL`

```dockerfile
SHELL ["/bin/bash", "-c"]
```

Changes the shell used by shell-form `RUN`/`CMD`/`ENTRYPOINT`. Mostly relevant on
Windows containers (switching to PowerShell). Not used here.

---

### `STOPSIGNAL`

```dockerfile
STOPSIGNAL SIGQUIT
```

Changes the signal Docker sends on `docker stop`. Default `SIGTERM`, which is what
.NET and nginx both handle correctly. Not used here.

---

### ★ `USER`

```dockerfile
USER app
USER 1000:1000
```

Switches the user for all subsequent instructions **and** for the container's main
process.

**Why it matters:** by default containers run as **root**. Root inside a container
isn't root on your host (thanks to namespaces), but combined with a container
escape or a mounted volume it's a real risk. Running as non-root is the single
cheapest security improvement available.

**The ordering trap:** everything after `USER` runs as that user, so anything
needing root (`apt-get install`, `chown`, `mkdir` in a root-owned dir) must come
**before** it. That's exactly why our API Dockerfile reads:

```dockerfile
RUN mkdir -p /app/Logs && chown -R app:app /app   # as root
COPY --from=build --chown=app:app /app/publish .  # as root, but sets ownership
USER app                                          # drop privileges last
```

The .NET `aspnet` images ship a ready-made `app` user (UID 64198). Node images ship
a `node` user. Alpine images generally don't — you'd `RUN adduser -D myuser`.

---

### `VOLUME`

```dockerfile
VOLUME /var/lib/data
```

Declares a path as a mount point, and creates an **anonymous** volume if nothing
is mounted there.

**Avoid this in your own Dockerfiles.** Anonymous volumes accumulate invisibly
(`docker volume ls` fills with hash-named entries) and they make the path
un-writable-into by later build steps. Declare volumes in `compose.yaml` instead,
where they get names and can be managed.

Not used here.

---

### ★ `WORKDIR`

```dockerfile
WORKDIR /src
WORKDIR /app
```

Sets the working directory for all subsequent `RUN`, `CMD`, `ENTRYPOINT`, `COPY`
and `ADD`. Creates the directory if it doesn't exist.

**Always use this instead of `RUN cd /app`.** `RUN cd /app` does nothing lasting —
each `RUN` is a fresh shell, so the `cd` is forgotten immediately.

`WORKDIR` also becomes the container's starting directory at run time, which is
why `dotnet UrlShortener.Api.dll` works without a path in our `ENTRYPOINT`, and why
Serilog's relative `Logs/log-.txt` resolves to `/app/Logs/`.

---

## III.2 The three that confuse everyone

### 1. `RUN` vs `CMD` vs `ENTRYPOINT`

```
BUILD TIME                          RUN TIME
─────────────────────────           ─────────────────────────
RUN <cmd>                           ENTRYPOINT + CMD
  runs now, during build              runs when the container starts
  result is baked into a layer        not baked into anything
  runs many times (one per RUN)       runs once, as PID 1
```

```dockerfile
RUN npm ci             # ✅ install packages into the image — build time
RUN npm start          # ❌ build hangs forever, server never exits
CMD ["npm", "start"]   # ✅ start the server when a container runs
```

### 2. `EXPOSE` vs `ports:`

```dockerfile
EXPOSE 8080            # 📄 documentation. Opens nothing.
```
```yaml
ports:
  - "5219:8080"        # 🔌 actually opens port 5219 on your machine
```

You can publish a port that was never `EXPOSE`d, and `EXPOSE` a port you never
publish. They're independent. `EXPOSE` exists so the next developer knows what to
map.

### 3. `daemon off;` and PID 1

**The rule: the container lives exactly as long as its PID 1 process.**

Servers traditionally "daemonise" — fork a background process and exit the
foreground one. That's correct on a VM and fatal in a container:

```
nginx starts → forks a background worker → foreground process exits
                                            ↓
                                    PID 1 has exited
                                            ↓
                                  container STOPS immediately
```

Hence `nginx -g "daemon off;"`. Equivalents you'll meet:

| Program | Foreground flag |
|---|---|
| nginx | `-g "daemon off;"` |
| Apache | `-DFOREGROUND` |
| PostgreSQL | (default is foreground) |
| Node/.NET/Python | (default is foreground — nothing needed) |

---

## III.3 The shell vs exec form

Every one of `RUN`, `CMD`, `ENTRYPOINT` accepts two syntaxes.

```dockerfile
CMD npm start                    # shell form  → /bin/sh -c "npm start"
CMD ["npm", "start"]             # exec form   → npm start, directly
```

| | Shell form | Exec form |
|---|---|---|
| Runs via `/bin/sh -c` | Yes | No |
| Shell features (`&&`, `|`, `$VAR`, `>`) | **Yes** | No |
| Receives signals correctly | **No** | **Yes** |
| Requires a shell in the image | Yes | No (works on `scratch`) |
| PID 1 is | `sh` | your program |

**The rule:**

- **`RUN`** → shell form is fine and usually preferable (you want `&&` and `$VAR`).
- **`CMD` / `ENTRYPOINT`** → **always exec form.** Signal handling, as explained
  above under `ENTRYPOINT`.

**Exec-form gotcha:** it is JSON, so **double quotes only**. `CMD ['npm','start']`
with single quotes fails with a parse error.

**If you need both** (shell features *and* correct signals), be explicit:

```dockerfile
ENTRYPOINT ["/bin/sh", "-c", "exec dotnet MyApp.dll --port $PORT"]
```

`exec` replaces the shell with your program, so your program becomes PID 1.

---

## III.4 Multi-stage builds in depth

### The problem

```dockerfile
# SINGLE STAGE — don't do this
FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/UrlShortener.Api.dll"]
```

This works. It also ships, to production:

- The entire .NET SDK (compilers, MSBuild, analysers) — ~800 MB
- Your complete source code, including anything sensitive in it
- The NuGet package cache
- Intermediate build artefacts

Final image: **~900 MB**, with a compiler in it.

### The solution

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build      # ← stage 0: has tools
...
RUN dotnet publish -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final   # ← stage 1: fresh start
COPY --from=build /app/publish .                     # ← reach back, take output
```

Final image: **~230 MB**. No compiler, no source, no NuGet cache.

### Typical savings

| Stack | Single-stage | Multi-stage |
|---|---|---|
| .NET | ~900 MB | ~230 MB |
| Node + React | ~1.2 GB | ~45 MB (nginx:alpine + dist) |
| Go | ~900 MB | ~15 MB (scratch + binary) |
| Java | ~700 MB | ~200 MB (JRE, not JDK) |

The frontend is the dramatic one: `node_modules` alone is hundreds of megabytes,
and **none of it is needed at run time** because `npm run build` produced plain
static files.

### Rules

1. Each `FROM` starts a new, empty filesystem. Nothing carries over automatically.
2. `AS <name>` names a stage; `COPY --from=<name>` reaches into it.
3. You can have as many stages as you like.
4. Stages not needed for the final image **are skipped entirely** by BuildKit.
5. `--target=<stage>` builds only up to a named stage — great for a test stage:

```bash
docker build --target build -t myapp:build-stage .
```

### A three-stage pattern you'll want eventually

```dockerfile
FROM node:22-alpine AS deps
WORKDIR /app
COPY package*.json ./
RUN npm ci

FROM deps AS test                    # ← inherits node_modules from deps
COPY . .
RUN npm run lint && npm test

FROM deps AS build
COPY . .
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
```

`docker build --target test .` runs the tests; a normal build skips that stage.

---

## III.5 Layer caching — the complete rules

This is *the* thing that separates a 3-second rebuild from a 3-minute one.

### How caching decides

For each instruction, Docker computes a cache key and checks for a match:

| Instruction | Cache key |
|---|---|
| `RUN` | The **command text only**. Docker does *not* inspect what it does. |
| `COPY` / `ADD` | The command text **plus a checksum of the copied files**. |
| `FROM` | The image digest. |
| Everything else | The command text. |

### The invalidation cascade — the rule that dictates ordering

> **When one layer's cache misses, every layer after it also misses. Always.**

Even if a later instruction's own inputs are unchanged, its *starting filesystem*
differs, so its result can't be reused.

This makes ordering everything:

```
┌──────────────────────────────────────┐
│  RARELY changes                      │  ← put at TOP
│  (base image, system packages,       │
│   dependency manifests, restore)     │
├──────────────────────────────────────┤
│  OFTEN changes                       │  ← put at BOTTOM
│  (your source code, build)           │
└──────────────────────────────────────┘
```

### Applied: the API Dockerfile

```dockerfile
COPY Directory.Build.props ./                    # changes ~never
COPY src/*/*.csproj ...                          # changes when deps change
RUN dotnet restore                               # SLOW (30-60s) — cached ✅
COPY src/ src/                                   # changes on EVERY edit
RUN dotnet publish --no-restore                  # rebuilds only your code
```

Edit a controller → `COPY src/ src/` misses → only `publish` re-runs. Restore
stays cached. **~5 second rebuild.**

The naive version:

```dockerfile
COPY . .                     # changes on every edit
RUN dotnet restore           # ❌ cache always missed
RUN dotnet publish           # ❌ cache always missed
```

Edit a controller → NuGet re-downloads every package. **~60 second rebuild.**

Same for the frontend: `COPY package*.json` → `npm ci` → `COPY . .` → `npm run
build`.

### Cache busters to watch for

| Pattern | Why it breaks caching | Fix |
|---|---|---|
| `COPY . .` early on | Any file change invalidates it | Copy manifests first |
| `RUN apt-get update` alone | Cached forever → stale package lists | `apt-get update && apt-get install -y x` in ONE `RUN` |
| No `.dockerignore` | Log files, `bin/` changing → checksum changes | Write one |
| Timestamps/version in `ENV` | Changes every build | Use `ARG`, place late |

### Inspecting the cache

Build output tells you:
```
=> CACHED [build 4/7] RUN dotnet restore ...        ← reused ✅
=> [build 5/7] COPY src/ src/                       ← rebuilt
```

Force a full rebuild (rarely needed):
```bash
docker compose build --no-cache api
```

---
---

# PART IV — COMPOSE.YAML KEY REFERENCE

## IV.1 Top-level keys

```yaml
name: urlshortener        # project name; defaults to the folder name
services: {}              # ← the only required key
volumes: {}               # named volume declarations
networks: {}              # custom networks (rarely needed)
secrets: {}               # file-based secrets
configs: {}               # file-based configs
include: []               # pull in other compose files
```

| Key | Used here? | Notes |
|---|---|---|
| `services` | ✅ | 4 services. The whole point. |
| `volumes` | ✅ | Declares `mssql-data` and `seq-data`. **Every named volume used in a service must be declared here** or Compose errors. |
| `networks` | ❌ | Compose auto-creates a `default` network and joins all services. You only need this for network segmentation. |
| `name` | ❌ | Defaults to the folder name (`urlshortener`), which is already what we want. It prefixes volumes and networks. |
| `secrets` | ❌ | The proper way to pass a password; mounts it as a file at `/run/secrets/<name>` instead of an env var. Worth graduating to. |
| `version` | ❌ | **Obsolete.** If you see `version: "3.8"` at the top of a tutorial, it's pre-2020. Modern Compose warns if you include it. Delete it. |

---

## IV.2 Every service key, A→Z

Marked **★** = used in this project.

---

### `build` ★

Build an image from a Dockerfile instead of pulling one. See [IV.3](#iv3-the-build-block-in-full).

Mutually exclusive with `image:` in practice — though you *can* use both, in which
case `image:` names the image that gets built.

---

### `cap_add` / `cap_drop`

```yaml
cap_drop: [ALL]
cap_add: [NET_BIND_SERVICE]
```

Linux capabilities. `cap_drop: [ALL]` then adding back only what's needed is good
hardening. Not used here.

---

### `command`

```yaml
command: ["dotnet", "MyApp.dll", "--verbose"]
command: npm run dev
```

Overrides the image's `CMD` at run time. Handy for running the same image
differently:

```yaml
worker:
  image: myapp
  command: ["dotnet", "MyApp.dll", "worker"]
```

Not used here — our images' defaults are correct.

---

### `container_name` ★

```yaml
container_name: urlshortener-api
```

A fixed container name. Without it you get `<project>-<service>-<index>`, e.g.
`urlshortener-api-1`.

**Trade-off:** a fixed name means the service **cannot be scaled** (two containers
can't share a name). Fine for databases and single-instance apps; remove it if you
ever want `docker compose up --scale api=3`.

---

### `depends_on` ★

Startup ordering. See [IV.5](#iv5-depends_on-and-startup-ordering).

---

### `deploy`

```yaml
deploy:
  replicas: 3
  resources:
    limits: { cpus: "0.5", memory: 512M }
```

Mostly a Docker Swarm feature, but `resources.limits` **does** work with plain
Compose and is useful to stop SQL Server eating all your RAM. Not used here.

---

### `entrypoint`

Overrides the image's `ENTRYPOINT`. Same idea as `command`. A common debugging
trick:

```yaml
entrypoint: ["sleep", "infinity"]   # keep a broken container alive to poke at
```

---

### `env_file`

```yaml
env_file:
  - ./api.env
```

Loads variables from a file **into the container's environment**. Different from
the root `.env` file, which substitutes into compose.yaml itself. See
[VI.2](#vi2-the-four-different-env-things) — this distinction trips up everyone.

Not used here; we set variables explicitly so the compose file is self-documenting.

---

### `environment` ★

```yaml
environment:
  ASPNETCORE_ENVIRONMENT: Development       # map syntax  ← clearer
  - ASPNETCORE_ENVIRONMENT=Development      # list syntax ← also valid
```

Environment variables inside the container. Both syntaxes work; pick one and stay
consistent. I used map syntax throughout.

**Pass-through trick:** a bare name with no value inherits from your shell:
```yaml
environment:
  - HOME              # takes $HOME from wherever you ran `docker compose up`
```

---

### `expose`

```yaml
expose: ["8080"]
```

Documentation, like the Dockerfile's `EXPOSE`. Publishes nothing. Rarely worth
writing.

---

### `extra_hosts`

```yaml
extra_hosts:
  - "host.docker.internal:host-gateway"
```

Adds `/etc/hosts` entries. The line above is how a container reaches a service
running on **your host machine** — useful when you're running the API in Visual
Studio but the database in Docker. On Docker Desktop (Windows/Mac),
`host.docker.internal` already exists; on Linux you need this line.

---

### `healthcheck` ★

See [IV.6](#iv6-healthcheck-in-full).

---

### `image` ★

```yaml
image: mcr.microsoft.com/mssql/server:2022-latest
```

Pull a prebuilt image. See [link ⑦](#link--image--a-registry) for the
registry/repo/tag anatomy.

---

### `labels`

Metadata key/values on the container. Used by tools like Traefik for routing
config. Not used here.

---

### `logging`

```yaml
logging:
  driver: json-file
  options:
    max-size: "10m"
    max-file: "3"
```

Controls log rotation. **Worth adding in real projects** — the default `json-file`
driver has *no size limit*, and a chatty container can fill your disk. Not used
here, but a reasonable next step.

---

### `networks`

```yaml
networks: [backend]
```

Which networks to join. Omitted → the auto-created `default` network, which is what
all four of our services use. You'd use this to isolate, e.g., the database from
anything but the API.

---

### `ports` ★

```yaml
ports:
  - "5219:8080"           # HOST:CONTAINER
  - "127.0.0.1:5219:8080" # bind to localhost only — more secure
  - "8080"                # random host port → container 8080
  - "5219:8080/udp"       # protocol
```

**Always quote them.** Unquoted, YAML parses `56:56` as a base-60 sexagesimal
number. This is a real, documented YAML footgun.

`127.0.0.1:5219:8080` is worth knowing: without the IP prefix, the port is bound on
**all** interfaces, meaning anyone on your network (café Wi-Fi, office LAN) can
reach your dev database. Binding to `127.0.0.1` restricts it to your machine.

---

### `profiles`

```yaml
seq:
  profiles: [observability]
```

Services with a profile only start when it's activated:
```bash
docker compose --profile observability up
```

Useful for optional tooling. I left Seq unprofiled so `docker compose up` gives
you the full stack — but putting Seq behind a profile would be a defensible choice.

---

### `restart` ★

```yaml
restart: "no"             # default
restart: always           # restart even if you stopped it manually
restart: on-failure       # only on non-zero exit
restart: unless-stopped   # restart unless YOU stopped it  ← used here
```

`unless-stopped` is the right default for dev: containers come back after a Docker
Desktop restart, but `docker compose stop` stays stopped.

Note `"no"` **must be quoted** — unquoted `no` is YAML boolean `false`.

---

### `secrets`

```yaml
secrets:
  - db_password
```
```yaml
secrets:
  db_password:
    file: ./db_password.txt
```

Mounts the secret as a file at `/run/secrets/db_password`. Better than an env var
because env vars leak into `docker inspect`, crash dumps and child processes.

.NET can read it via a small config addition, or you can use the
`SA_PASSWORD_FILE` convention some images support. A good next step for this
project.

---

### `security_opt`

```yaml
security_opt:
  - no-new-privileges:true
```

Prevents privilege escalation via setuid binaries. Cheap hardening.

---

### `stop_grace_period`

```yaml
stop_grace_period: 30s
```

How long Docker waits after `SIGTERM` before `SIGKILL`. Default 10s. Increase it
if your app needs longer to drain in-flight requests.

---

### `tmpfs`

```yaml
tmpfs: /tmp
```

Mount a RAM-backed filesystem. Fast, and wiped on stop.

---

### `user`

```yaml
user: "1000:1000"
```

Overrides the image's `USER` at run time. Our API sets `USER app` in the Dockerfile
instead, which is the better place for it.

---

### `volumes` ★

See [IV.4](#iv4-volumes--the-three-kinds).

---

### `working_dir`

Overrides the image's `WORKDIR`. Rarely needed.

---

## IV.3 The `build` block in full

```yaml
build:
  context: .                                   # ★ required
  dockerfile: src/UrlShortener.Api/Dockerfile  # ★ default: Dockerfile
  args:                                        # ARG values
    NODE_VERSION: "22"
  target: build                                # stop at a named stage
  cache_from:                                  # reuse cache from a registry image
    - myapp:latest
  labels:
    com.example.version: "1.0"
  network: host                                # network during build
  no_cache: true                               # ignore cache
  pull: true                                   # always re-pull base images
```

Shorthand when everything is default:
```yaml
build: ./frontend       # equivalent to: build: { context: ./frontend }
```

**`args`** connects to the Dockerfile's `ARG`:
```yaml
build:
  args:
    BUILD_CONFIG: Debug
```
```dockerfile
ARG BUILD_CONFIG=Release
RUN dotnet publish -c $BUILD_CONFIG
```

**`target`** is the multi-stage hook. A dev compose override could do:
```yaml
build:
  target: build          # stop at the SDK stage, which has dotnet watch
```

We use only `context` and `dockerfile` — no build-time variation is needed.

---

## IV.4 `volumes` — the three kinds

This is the most under-explained part of Compose, so here it is in full.

### Kind 1 — Named volume ★ (what we use)

```yaml
services:
  sqlserver:
    volumes:
      - mssql-data:/var/opt/mssql      # NAME : PATH-IN-CONTAINER

volumes:
  mssql-data:                          # must be declared here too
```

- Docker manages the storage (on Windows, inside the Docker Desktop VM).
- Survives `docker compose down`. Deleted only by `docker compose down -v` or
  `docker volume rm`.
- **Best performance on Windows/Mac**, because it lives inside the Linux VM rather
  than crossing the host filesystem boundary.
- Portable: no host paths in your compose file.

**Use for:** databases, uploaded files, log stores — anything the container writes
that must survive.

### Kind 2 — Bind mount

```yaml
volumes:
  - ./src:/app/src                     # HOST-PATH : CONTAINER-PATH
  - .:/app
```

- Maps a **real folder on your machine** into the container.
- Identified by the leading `.` or `/` — that's the only syntactic difference from
  a named volume.
- Changes on either side are instantly visible on the other.
- **Slower on Windows/Mac** (filesystem translation layer).

**Use for:** live-reload development. This is how you'd add `dotnet watch`:

```yaml
# compose.override.yaml
services:
  api:
    build:
      target: build
    volumes:
      - ./src:/src/src                 # your code, live
    command: ["dotnet", "watch", "run", "--project", "src/UrlShortener.Api"]
```

**Use for also:** injecting a config file without rebuilding:
```yaml
- ./frontend/nginx.conf:/etc/nginx/conf.d/default.conf:ro
```
`:ro` = read-only. Good habit for config.

### Kind 3 — Anonymous volume

```yaml
volumes:
  - /app/node_modules                  # no name, no host path
```

Docker creates a hash-named volume. The main legitimate use is **shadowing**: when
you bind-mount `.:/app` for live reload, the host's (Windows) `node_modules` would
overwrite the container's (Linux) one. Adding an anonymous volume at
`/app/node_modules` protects the container's copy.

Otherwise avoid — they accumulate as unnamed junk in `docker volume ls`.

### Quick identification

```yaml
- mssql-data:/var/opt/mssql     # no . or / at the start  → NAMED
- ./src:/app/src                # starts with ./          → BIND
- /c/data:/app/data             # starts with /           → BIND
- /app/node_modules             # single path only        → ANONYMOUS
```

### Modifiers

```yaml
- ./config:/etc/app:ro          # read-only
- data:/var/lib/data:rw         # read-write (default)
```

### Managing them

```bash
docker volume ls
```

```bash
docker volume inspect urlshortener_mssql-data
```

```bash
docker compose down -v
```

That last one deletes every volume in the project — your database and all Seq
history. There is no undo.

### ⚠️ A gap in this project

The API writes Serilog file logs to `/app/Logs` **inside the container**, with no
volume. Those logs vanish on every `docker compose down`. That's *acceptable* here
because Console → `docker compose logs` and Seq → a persistent volume both capture
the same events. But if you wanted the files, you'd add:

```yaml
api:
  volumes:
    - api-logs:/app/Logs

volumes:
  api-logs:
```

---

## IV.5 `depends_on` and startup ordering

### The short form — and why it's usually not enough

```yaml
depends_on:
  - sqlserver
```

This waits for the sqlserver **container to be created and started**. It does
**not** wait for SQL Server the *program* to finish booting and accept
connections — which takes 15–25 more seconds.

Result: the API starts, `DatabaseInitializer.InitialiseAsync()` calls
`MigrateAsync()`, the connection is refused, `Program.cs`'s catch logs Fatal, and
the container exits. Classic flaky startup.

### The long form ★ (what we use)

```yaml
depends_on:
  sqlserver:
    condition: service_healthy
  seq:
    condition: service_started
```

| Condition | Waits until |
|---|---|
| `service_started` | The container is running. (Same as short form.) |
| `service_healthy` | The container's **healthcheck passes**. |
| `service_completed_successfully` | The container ran and exited 0. For migration jobs. |

`service_healthy` requires the *dependency* to define a `healthcheck` — which is
exactly why `sqlserver` has one and `seq` doesn't (nothing waits on Seq's
readiness; the Serilog sink retries on its own).

### `restart: true`

```yaml
depends_on:
  sqlserver:
    condition: service_healthy
    restart: true      # restart this service if sqlserver is restarted
```

### What `depends_on` does NOT do

- It does **not** work across separate compose files.
- It does **not** retry if the dependency dies later. It's startup-only.
- It does **not** replace application-level resilience. `EnableRetryOnFailure` in
  `DependencyInjection.cs` still matters for transient failures *during* operation.

### The `service_completed_successfully` pattern

For a project that keeps migrations separate from app startup:

```yaml
migrator:
  build: .
  command: ["dotnet", "ef", "database", "update"]
  depends_on:
    db: { condition: service_healthy }

api:
  build: .
  depends_on:
    migrator: { condition: service_completed_successfully }
```

Worth knowing about — this project runs migrations in `Program.cs` instead
(`MigrateOnStartup: true`), which is simpler but couples startup to schema changes.

---

## IV.6 `healthcheck` in full

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
  interval: 10s          # how often to run it
  timeout: 5s            # fail the attempt if it takes longer
  retries: 12            # consecutive failures before "unhealthy"
  start_period: 30s      # grace period; failures here don't count
  start_interval: 2s     # (newer) faster polling during start_period
  disable: false         # true to disable a healthcheck from the image
```

### `test` — three forms

```yaml
test: ["CMD", "curl", "-f", "http://localhost/health"]
```
Runs the command **directly** — no shell. Fastest, no shell required in the image.

```yaml
test: ["CMD-SHELL", "curl -f http://localhost/health || exit 1"]
```
Runs via `/bin/sh -c`. **Needed for** `||`, `&&`, pipes, redirects, and `$VAR`
expansion. This is what our sqlserver check uses, because it must expand
`$$MSSQL_SA_PASSWORD`.

```yaml
test: ["NONE"]
```
Disables a healthcheck inherited from the image.

### Success is exit code 0

Anything non-zero is a failure. Hence the common `|| exit 1` and flags like
`curl -f` (fail on HTTP errors) or `sqlcmd -b` (exit non-zero on SQL errors) —
without those, the command succeeds even when the request failed.

### Our sqlserver healthcheck, decoded

```yaml
test:
  - CMD-SHELL
  - /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" -b
```

| Piece | Meaning |
|---|---|
| `CMD-SHELL` | Needed so `$$MSSQL_SA_PASSWORD` expands |
| `/opt/mssql-tools18/bin/sqlcmd` | Path in the 2022 image. Older images: `mssql-tools` |
| `-S localhost` | Here `localhost` **is correct** — we're already *inside* the sqlserver container |
| `-U sa -P "$$..."` | Credentials |
| `$$` | Escaped `$`. Compose emits a literal `$`; the container's shell expands it. Keeps the password out of `docker compose config` output. |
| `-C` | Trust the self-signed certificate (tools18 encrypts by default) |
| `-Q "SELECT 1"` | Cheapest possible "are you alive" query |
| `-b` | **Exit non-zero on error.** Without this the check always passes. |

`start_period: 30s` covers SQL Server's cold start; `interval 10s × retries 12`
gives another 120 s of grace after that.

### Checking status

```bash
docker compose ps
```
The `STATUS` column shows `Up 40s (healthy)` / `(health: starting)` / `(unhealthy)`.

```bash
docker inspect --format "{{json .State.Health}}" urlshortener-sqlserver
```
Shows the last five probe results including their output — the fastest way to
diagnose a check that never goes healthy.

### Writing one for your own app

```yaml
# HTTP endpoint (needs curl or wget in the image)
test: ["CMD", "curl", "-f", "http://localhost:8080/health"]

# Alpine images have wget, not curl
test: ["CMD", "wget", "-q", "--spider", "http://localhost:8080/health"]

# Postgres
test: ["CMD-SHELL", "pg_isready -U postgres"]

# Redis
test: ["CMD", "redis-cli", "ping"]

# .NET without curl installed — use the runtime itself
test: ["CMD", "dotnet", "--info"]
```

Note: this project's API would be a good `healthcheck` candidate — it already has
`app.MapGet("/health", ...)` in `Program.cs`. The `aspnet` image doesn't ship curl,
so you'd either install it or add a tiny health-check tool.

---

## IV.7 YAML syntax you'll trip over

YAML is whitespace-sensitive and has some genuinely surprising type coercion.

### Indentation

**Spaces only. Never tabs.** A tab is a parse error, and it's invisible.
Two spaces per level is the convention.

### The Norway problem

```yaml
ACCEPT_EULA: Y        # string "Y" — fine
ACCEPT_EULA: Yes      # ← BOOLEAN true, not the string "Yes"
country: NO           # ← BOOLEAN false, not "NO" (Norway)
restart: no           # ← BOOLEAN false, not the string "no"
```

YAML 1.1 treats `y`, `yes`, `on`, `true` (and their negatives) as booleans. Docker
wants strings.

**Fix: quote anything that could be misread.**
```yaml
ACCEPT_EULA: "Y"
restart: "no"
```

### The sexagesimal port problem

```yaml
ports:
  - 5219:8080     # parsed as a STRING here, but...
  - 56:56         # ← parsed as base-60! Becomes 3416.
```

**Fix: always quote ports.**
```yaml
ports:
  - "5219:8080"
```

### Version-number truncation

```yaml
NODE_VERSION: 22.0      # → the float 22.0, might render as "22"
NODE_VERSION: "22.0"    # → the string "22.0" ✅
```

### Multi-line strings

```yaml
# >- folded, strip trailing newline  ← what we use for the connection string
key: >-
  line one
  line two
# → "line one line two"

# > folded, keep one trailing newline
# | literal, keep all newlines
key: |
  line one
  line two
# → "line one\nline two"

# |- literal, strip trailing newline
```

Our connection string uses `>-` so a long string stays readable while resolving to
one line. Note the folded form joins with a **space**, so
`Server=sqlserver,1433;Database=X;` + newline + `Password=...` becomes
`...Database=X; Password=...` — with a space after the semicolon. SQL connection
strings tolerate that.

### Lists vs maps

```yaml
# Map (key: value)
environment:
  KEY: value

# List (- item)
environment:
  - KEY=value
```

Both valid for `environment`. Some keys accept only one form — `ports` and
`volumes` are lists only.

### Comments and anchors

```yaml
# a comment

x-common: &common          # anchor (the x- prefix means "extension, ignore me")
  restart: unless-stopped
  networks: [default]

services:
  api:
    <<: *common            # merge the anchor in
    image: myapp
```

Anchors reduce duplication in large compose files. Not used here — four services
isn't enough repetition to justify the indirection.

### Validate before you run

```bash
docker compose config
```

Resolves variables, merges override files, applies defaults, and prints the final
result. If it errors, your compose file is wrong. If it succeeds, what it prints is
exactly what Compose will act on. **Run this whenever something is behaving
strangely.**

---
---

# PART V — .DOCKERIGNORE REFERENCE

## V.1 Pattern syntax

Similar to `.gitignore`, but **not identical** — the differences matter.

```
# comment

bin/                 directory named bin at the CONTEXT ROOT only
**/bin/              directory named bin at ANY depth      ← usually what you want
*.log                any .log file at the root
**/*.log             any .log file at any depth
temp?                temp1, tempA — ? is exactly one char
!important.log       negate: re-include this even if excluded above
docs/**              everything under docs/
```

### The difference from .gitignore that catches people

In `.gitignore`, `bin/` matches at any depth. In `.dockerignore`, it matches **only
at the root of the context**. You need `**/bin/` for any depth.

That's why our file reads:
```
**/bin/
**/obj/
```
and not `bin/` — the `bin` folders live at `src/UrlShortener.Api/bin/`, not the root.

### Negation

```
**/*.json
!appsettings.json
```

Order matters: later rules win. Beware — negation can't re-include a file whose
*parent directory* was excluded.

### Checking it works

The build output reports context size:
```
=> => transferring context: 2.34MB      ← good
=> => transferring context: 412.7MB     ← .dockerignore isn't working
```

---

## V.2 What to always exclude, by stack

### Universal

```
.git/
.gitignore
*.md
.env
.env.*
.vscode/
.idea/
**/*.log
```

### .NET

```
**/bin/
**/obj/
**/.vs/
**/*.user
**/*.suo
artifacts/
TestResults/
```

**`bin/` and `obj/` are not optional to exclude.** This is a correctness issue, not
performance:

`obj/project.assets.json` records absolute paths from the machine that restored it
— `C:\Users\Compumarts\.nuget\packages\...`. Copy that into a Linux container and
NuGet either fails on the invalid paths, or worse, believes restore is already done
and skips it, producing baffling "package not found" errors at compile time.

### Node

```
node_modules/
dist/
build/
.next/
coverage/
npm-debug.log*
```

**`node_modules/` is also a correctness issue.** Many npm packages ship
**platform-specific native binaries** — esbuild (which Vite uses), sharp, bcrypt,
node-sass. Your Windows `node_modules` contains Windows `.exe`s. Copy those into a
Linux container and you get:

```
Error: Cannot find module '@esbuild/linux-x64'
```
or
```
sh: ./esbuild: cannot execute binary file: Exec format error
```

`npm ci` inside the container downloads the correct Linux builds. This is why
`frontend/.dockerignore` excludes `node_modules/` and the Dockerfile runs `npm ci`.

### Python

```
__pycache__/
*.pyc
.venv/
venv/
.pytest_cache/
*.egg-info/
```

### Java

```
target/
build/
.gradle/
*.class
```

### Go

```
vendor/
*.exe
```

---
---

# PART VI — .ENV REFERENCE

## VI.1 Rules and gotchas

```dotenv
# comments start with #
KEY=value
KEY_WITH_SPACES=hello world          # quotes NOT required, and NOT stripped as you'd expect
QUOTED="hello world"                 # in Compose's .env, the quotes ARE stripped
EMPTY=
```

### The rules

1. **Location: next to `compose.yaml`.** Anywhere else and it's silently ignored —
   no error, your `${VARS}` just become empty strings.
2. **No `export`.** This isn't a shell script. `export KEY=value` will confuse it.
3. **No spaces around `=`.** `KEY = value` gives you a key named `KEY ` with a
   value ` value`.
4. **No variable interpolation between lines** in older versions. Don't rely on
   `B=${A}/sub`.
5. **`#` starts a comment** — including mid-line in some parsers. Avoid `#` inside
   unquoted values. Our password `Your_Str0ng!Passw0rd` deliberately avoids `#`.
6. **Precedence:** a real shell environment variable **beats** `.env`. If your
   terminal has `API_PORT=9999` exported, that wins.

### Substitution syntax in compose.yaml

```yaml
${VAR}              # substitute; empty string if unset
${VAR:-default}     # use "default" if VAR is unset OR empty
${VAR-default}      # use "default" only if VAR is UNSET
${VAR:?error msg}   # FAIL with this message if unset or empty  ← good for secrets
${VAR:+alt}         # use "alt" if VAR IS set
$$                  # a literal $ (escape)
```

`${VAR:?}` is worth adopting for anything mandatory:

```yaml
MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD:?set MSSQL_SA_PASSWORD in .env}
```

Now a missing `.env` fails immediately with a clear message, instead of SQL Server
starting with an empty password and dying mysteriously.

### Always gitignore `.env`, always commit `.env.example`

```gitignore
.env
.env.*
!.env.example
```

Already present in this project's `.gitignore`.

The convention exists because a repo needs to communicate **what** configuration is
needed without leaking **what the values are**. A new teammate copies
`.env.example` → `.env` and fills in real values.

---

## VI.2 The four different "env" things

This naming collision confuses everyone. There are four distinct concepts.

### 1. The root `.env` file → substitutes into compose.yaml

```dotenv
API_PORT=5219
```
```yaml
ports:
  - "${API_PORT}:8080"
```

**Affects the compose file itself.** Resolved before Compose even parses YAML.

⚠️ **These do NOT automatically appear inside your containers.** This is the #1
misunderstanding. `API_PORT` is not visible to your .NET app unless you also pass
it through `environment:`.

### 2. `environment:` → variables inside the container ★

```yaml
environment:
  ASPNETCORE_ENVIRONMENT: Development
```

**This** is what your app reads. Verify with:
```bash
docker compose exec api printenv
```

### 3. `env_file:` → bulk-load variables into the container

```yaml
env_file:
  - ./api.env
```

Same effect as `environment:`, but from a file. Useful when there are many
variables. Note this file is a *different file* from the root `.env` and serves a
*different purpose*, despite the identical format.

### 4. `ENV` in the Dockerfile → baked into the image

```dockerfile
ENV NODE_ENV=production
```

Set at build time, permanent in the image, applies to every container from it.

### Precedence, highest wins

```
1. environment: in compose.yaml          ← highest
2. env_file: in compose.yaml
3. ENV in the Dockerfile
4. (nothing)                             ← lowest
```

### Summary table

| | Affects | Set at | Changing it needs |
|---|---|---|---|
| Root `.env` | The compose file's `${VARS}` | Parse time | Nothing, just re-`up` |
| `environment:` | Container env | Container start | Re-`up` |
| `env_file:` | Container env | Container start | Re-`up` |
| `ENV` | Image + container env | **Build** time | **Rebuild** |

**Which to use:** `environment:` for anything deployment-specific, `ENV` only for
things intrinsic to the image. This project uses `environment:` exclusively, which
is why the same API image would run unchanged in production with a different
connection string.

---
---

# PART VII — NGINX.CONF REFERENCE

## VII.1 Structure

A full nginx config has this shape:

```nginx
events { ... }              # connection handling
http {                      # all HTTP config
    server { ... }          # one virtual host
    server { ... }          # another
}
```

**Our file has only the `server` block.** That's because it's copied to
`/etc/nginx/conf.d/default.conf`, and the image's main `/etc/nginx/nginx.conf`
already contains:

```nginx
events { worker_connections 1024; }
http {
    include /etc/nginx/conf.d/*.conf;    # ← our file gets pulled in HERE
}
```

So we're writing a fragment that lands inside `http { }`. Writing `http { }`
ourselves would be a syntax error (nested `http` blocks aren't allowed).

**Every directive ends with `;`.** Missing semicolons are the most common nginx
error, and the message points at the *next* line, which is confusing.

---

## VII.2 Every directive used here

### `listen 8080;`

The port nginx accepts connections on, inside the container. Not 80, for
consistency with the API and because it doesn't require root.

Variants: `listen 443 ssl;`, `listen [::]:8080;` (IPv6).

### `server_name _;`

Which `Host:` headers this block handles. `_` is a conventional catch-all
placeholder (it's not special syntax — it's just an invalid hostname that can never
match a real one, so it acts as the default). With one server block, it's
effectively "match everything".

### `root /usr/share/nginx/html;`

Where files live. A request for `/assets/index.js` maps to
`/usr/share/nginx/html/assets/index.js`.

### `index index.html;`

What to serve for a directory request (`/` → `/index.html`).

### `gzip on;` / `gzip_types` / `gzip_min_length`

```nginx
gzip on;
gzip_types text/css application/javascript application/json image/svg+xml;
gzip_min_length 1024;
```

- `gzip on` enables compression. Typically cuts JS/CSS transfer size by 70%.
- `gzip_types` — which MIME types to compress. **`text/html` is always compressed
  when gzip is on and cannot be listed** (nginx errors on duplicates), which is why
  it's absent.
- `gzip_min_length 1024` — don't compress responses under 1 KB. Compression has
  per-response overhead, and tiny payloads often get *bigger*.

### `location /api/ { ... }`

Matches request paths starting with `/api/`. See [VII.3](#vii3-location-matching-rules).

### `proxy_pass http://api:8080;`

Forwards the request to another server.

- `api` — the Compose service name, resolved by Compose's DNS.
- `8080` — the container port.

**The trailing-slash rule — nginx's sharpest edge:**

```nginx
location /api/ { proxy_pass http://api:8080;  }   # NO trailing slash
# request /api/urls  →  backend receives /api/urls    ✅ what we want

location /api/ { proxy_pass http://api:8080/; }   # WITH trailing slash
# request /api/urls  →  backend receives /urls       ❌ 404
```

**Rule:** a URI on `proxy_pass` (even just `/`) **replaces** the matched `location`
prefix. No URI means the path passes through untouched.

Our API's routes are `[Route("api/urls")]` — they *include* the `/api` prefix — so
we must not strip it. Hence no trailing slash.

### `proxy_http_version 1.1;`

nginx defaults to HTTP/1.0 for upstream connections, which has no keep-alive and
breaks chunked transfer encoding and WebSockets. Effectively always set this.

### `proxy_set_header ...`

```nginx
proxy_set_header Host              $host;
proxy_set_header X-Real-IP         $remote_addr;
proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
proxy_set_header X-Forwarded-Proto $scheme;
```

Without these, the backend sees the *proxy* as the client, and loses the original
host and scheme.

| Header | Variable | Carries |
|---|---|---|
| `Host` | `$host` | The hostname the browser requested |
| `X-Real-IP` | `$remote_addr` | The immediate client's IP |
| `X-Forwarded-For` | `$proxy_add_x_forwarded_for` | Appends to any existing chain |
| `X-Forwarded-Proto` | `$scheme` | `http` or `https` |

**⚠️ Note for this project:** setting these headers is only half the job. For
ASP.NET Core to *use* them, you must call `app.UseForwardedHeaders(...)` — which
this project does **not** currently do. So `RemoteIpAddress` in
`RedirectController.cs` would see nginx's internal IP, not the visitor's, for any
request routed through nginx.

In practice this is fine, because short-link redirects go **directly** to
`localhost:5219` and never pass through nginx — so visitor analytics are correct.
The headers are configured now so the plumbing is ready if you later put the API
behind the proxy.

### `expires 1y;` / `add_header Cache-Control "public, immutable";`

```nginx
location /assets/ {
    expires 1y;
    add_header Cache-Control "public, immutable";
}
```

Vite emits content-hashed filenames (`index-9NCiek3r.js`). The content behind a
given name can **never** change — a new build produces a new hash. So caching for a
year is safe, and `immutable` tells the browser not to even send a revalidation
request.

Critically, `index.html` is **not** under `/assets/`, so it stays uncached. That
means a deploy is picked up immediately: the browser refetches `index.html`, sees
new hashed filenames, and fetches those.

### `try_files $uri $uri/ /index.html;`

**The SPA fallback — the line everyone forgets.**

```nginx
location / {
    try_files $uri $uri/ /index.html;
}
```

Tries each in order, uses the first that exists:
1. `$uri` — the literal file
2. `$uri/` — as a directory
3. `/index.html` — the fallback

**Why it's needed:** vue-router owns client-side URLs. Navigate inside the app to
`/links/abc123` and it works — no server involved. But **refresh the page**, and
the browser asks nginx for `/links/abc123`. There's no such file, so without
`try_files` you get a 404 on a route the app handles perfectly.

With it, nginx returns `index.html`, the Vue app boots, and vue-router reads the
URL and renders the right view.

**Every SPA needs this.** React, Vue, Angular, Svelte — identical requirement.

---

## VII.3 location matching rules

nginx does **not** evaluate `location` blocks top to bottom. The order is:

```
1. location = /exact           exact match      → wins immediately, stops
2. location ^~ /prefix         prefix, no-regex → wins over regex if longest
3. location ~ /regex           regex, case-sensitive    ┐ first match wins,
   location ~* /regex          regex, case-insensitive  ┘ IN FILE ORDER
4. location /prefix            prefix           → LONGEST match wins
```

For our file, only rule 4 applies:

```nginx
location /api/    { ... }   # 5 characters
location /assets/ { ... }   # 8 characters
location /        { ... }   # 1 character
```

| Request | Matches | Why |
|---|---|---|
| `/api/urls` | `/api/` | Longest matching prefix (5 > 1) |
| `/assets/index.js` | `/assets/` | Longest (8 > 1) |
| `/links/abc123` | `/` | Only match |
| `/favicon.svg` | `/` | Only match |

**Because prefix matching picks the longest, file order does not matter** for these
three blocks. You could shuffle them freely. (Order *does* matter for regex
locations, which we don't use.)

### Other directives worth knowing

```nginx
return 301 https://$host$request_uri;    # redirect
rewrite ^/old/(.*)$ /new/$1 permanent;   # rewrite the URI
error_page 404 /index.html;              # alternative SPA fallback (worse — wrong status code)
client_max_body_size 10M;                # allow larger uploads (default 1M!)
access_log off;                          # quieter logs
alias /some/other/path/;                 # like root, but REPLACES the matched prefix
```

`client_max_body_size` is the one that will bite you eventually — the default 1 MB
silently rejects larger uploads with a 413.

---
---

# PART VIII — STARTING A NEW PROJECT

## VIII.1 The decision tree

```
START
  │
  ├─ Do I need more than one container?
  │     (app + database counts as two)
  │     │
  │     ├─ NO  → You can skip compose.yaml and just use `docker build`
  │     │        + `docker run`. But write compose.yaml anyway — the moment
  │     │        you add a database you'll want it, and it documents your
  │     │        run flags.
  │     │
  │     └─ YES → ✅ compose.yaml    [1 file]
  │
  ├─ For each service, did I write the code?
  │     │
  │     ├─ NO (Postgres, Redis, nginx, Seq...)
  │     │      → use `image:`. NO Dockerfile needed. ✅
  │     │
  │     └─ YES (my API, my frontend)
  │            → use `build:` → ✅ Dockerfile        [1 per image]
  │
  ├─ For each build context, is there junk in the folder?
  │     (node_modules, bin/, obj/, .git — the answer is always yes)
  │     → ✅ .dockerignore at the root of each context   [1 per context]
  │
  ├─ Do I have passwords / values that differ per machine?
  │     → ✅ .env + .env.example     [2 files]
  │
  ├─ Does any service need a config file baked in?
  │     (nginx.conf, redis.conf, my.cnf)
  │     → ✅ that config file, inside the relevant context
  │
  └─ Does any service store data that must survive a restart?
        (database, uploads, log store)
        → ✅ a named volume in compose.yaml
```

### Applied to this project

| Question | Answer |
|---|---|
| Multiple containers? | Yes, 4 → `compose.yaml` |
| Did I write sqlserver? | No → `image:`, no Dockerfile |
| Did I write seq? | No → `image:`, no Dockerfile |
| Did I write the API? | Yes → `src/UrlShortener.Api/Dockerfile` |
| Did I write the frontend? | Yes → `frontend/Dockerfile` |
| How many build contexts? | 2 (root, frontend) → 2 `.dockerignore` files |
| Secrets? | Yes, SA password → `.env` + `.env.example` |
| Config files to bake in? | nginx → `frontend/nginx.conf` |
| Data to persist? | SQL Server + Seq → 2 named volumes |

**Total: 6 files.** Exactly what's in the repo.

---

## VIII.2 Step-by-step for any project

### Step 1 — Inventory your services

Write the list before writing any YAML.

```
1. api        — my code        → needs a Dockerfile
2. frontend   — my code        → needs a Dockerfile
3. database   — off the shelf  → image only
4. cache      — off the shelf  → image only
```

### Step 2 — Skeleton compose.yaml

```yaml
services:
  db:
    image: postgres:16
  api:
    build: .
  frontend:
    build: ./frontend
```

Nothing else yet. Get the shape right first.

### Step 3 — Write a Dockerfile per service you build

Universal template:

```dockerfile
# 1. Base image with build tools
FROM <sdk-image> AS build
WORKDIR /src

# 2. Dependency manifests FIRST (layer caching)
COPY <manifest-files> ./
RUN <install-dependencies>

# 3. Source code
COPY . .
RUN <build-command>

# 4. Fresh, small runtime image
FROM <runtime-image> AS final
WORKDIR /app

# 5. Copy ONLY the build output
COPY --from=build /src/<output> .

# 6. Security + docs + start
USER <non-root>
EXPOSE <port>
ENTRYPOINT ["<program>", "<args>"]
```

Fill in the six blanks per stack — see [VIII.3](#viii3-copy-paste-recipes-by-stack).

### Step 4 — Write .dockerignore per context

Start from [V.2](#v2-what-to-always-exclude-by-stack). Then verify:

```bash
docker compose build api
```

Check `transferring context:` in the output. Under ~10 MB for most projects.

### Step 5 — Make your app configurable by environment variable

**This is the step people skip, and it's the one that matters most.**

Find every hardcoded value — connection strings, URLs, ports, API keys — and make
it readable from the environment.

```csharp
// .NET: already automatic. appsettings.json values are overridable via
// ConnectionStrings__Name, Section__Key, Section__Array__0
```
```js
// Node
const dbUrl = process.env.DATABASE_URL ?? 'postgres://localhost/dev'
```
```python
# Python
db_url = os.environ.get("DATABASE_URL", "postgres://localhost/dev")
```

If your app can't be configured without editing a file, you'll have to rebuild the
image for every environment — which defeats a large part of the point.

### Step 6 — Fill in compose.yaml properly

Add, per service: `environment`, `ports`, `volumes`, `depends_on`, `restart`.

**Replace every `localhost` with a service name.** This is where new Docker users
lose the most time. Go through your config and check every host:

```
localhost:5432   →   db:5432
localhost:6379   →   cache:6379
localhost:5341   →   seq:80
```

### Step 7 — Add volumes for anything that must persist

```yaml
services:
  db:
    volumes:
      - db-data:/var/lib/postgresql/data

volumes:
  db-data:
```

**Look up the right container path for your image** — it's in the image's docs on
Docker Hub, and it differs per database:

| Image | Data path |
|---|---|
| `postgres` | `/var/lib/postgresql/data` |
| `mysql` / `mariadb` | `/var/lib/mysql` |
| `mongo` | `/data/db` |
| `mcr.microsoft.com/mssql/server` | `/var/opt/mssql` |
| `redis` | `/data` |
| `datalust/seq` | `/data` |

### Step 8 — Add a healthcheck to your database, and `depends_on` to your app

```yaml
db:
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U postgres"]
    interval: 10s
    retries: 5
    start_period: 30s

api:
  depends_on:
    db: { condition: service_healthy }
```

Skip this and you'll get intermittent startup failures that only happen on cold
starts — and are therefore maddening to reproduce.

### Step 9 — Extract secrets to .env

```yaml
POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:?set POSTGRES_PASSWORD in .env}
```

Create `.env` and `.env.example`. Add `.env` to `.gitignore`.

### Step 10 — Verify, then run

```bash
docker compose config
```
Resolves everything and prints the final file. Fix errors here before running.

```bash
docker compose up --build
```
Leave it in the foreground the first time and read the logs.

```bash
docker compose ps
```
Everything should be `Up`, and anything with a healthcheck should be `(healthy)`.

---

## VIII.3 Copy-paste recipes by stack

### ASP.NET Core (multi-project solution — what this repo uses)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY src/MyApp.Domain/MyApp.Domain.csproj         src/MyApp.Domain/
COPY src/MyApp.Api/MyApp.Api.csproj               src/MyApp.Api/
RUN dotnet restore src/MyApp.Api/MyApp.Api.csproj
COPY src/ src/
RUN dotnet publish src/MyApp.Api/MyApp.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build --chown=app:app /app/publish .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyApp.Api.dll"]
```

Single-project version is simpler:
```dockerfile
COPY MyApp.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore
```

### Node/Express API

```dockerfile
FROM node:22-alpine AS deps
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --omit=dev

FROM node:22-alpine AS final
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
USER node
EXPOSE 3000
CMD ["node", "server.js"]
```

`--omit=dev` skips devDependencies. `node` is a built-in non-root user.

### React / Vue / Angular SPA

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

Output folder differs: Vite → `dist`, CRA → `build`, Angular → `dist/<project-name>`,
Next.js static export → `out`.

### Python / FastAPI

```dockerfile
FROM python:3.12-slim AS final
WORKDIR /app
ENV PYTHONUNBUFFERED=1 PYTHONDONTWRITEBYTECODE=1
COPY requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt
COPY . .
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
EXPOSE 8000
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
```

`PYTHONUNBUFFERED=1` is important — without it, `print()` output is buffered and
your logs appear late or not at all.

**`--host 0.0.0.0` is critical.** Binding to `127.0.0.1` inside a container means
only that container can reach it — port mappings won't work. Same applies to any
framework's dev server.

### Go

```dockerfile
FROM golang:1.23-alpine AS build
WORKDIR /src
COPY go.mod go.sum ./
RUN go mod download
COPY . .
RUN CGO_ENABLED=0 go build -o /app/server ./cmd/server

FROM scratch AS final
COPY --from=build /app/server /server
EXPOSE 8080
ENTRYPOINT ["/server"]
```

`FROM scratch` = empty image. Final size ~15 MB. `CGO_ENABLED=0` produces a static
binary with no libc dependency, which is what makes `scratch` possible.

### Java / Spring Boot

```dockerfile
FROM maven:3.9-eclipse-temurin-21 AS build
WORKDIR /src
COPY pom.xml ./
RUN mvn dependency:go-offline
COPY src/ src/
RUN mvn package -DskipTests

FROM eclipse-temurin:21-jre-alpine AS final
WORKDIR /app
COPY --from=build /src/target/*.jar app.jar
EXPOSE 8080
ENTRYPOINT ["java", "-jar", "app.jar"]
```

Note `jre` not `jdk` in the final stage — runtime only.

---

## VIII.4 Copy-paste recipes for common services

Drop these into `services:`. Each includes the volume path and healthcheck.

### PostgreSQL

```yaml
postgres:
  image: postgres:16-alpine
  environment:
    POSTGRES_DB: myapp
    POSTGRES_USER: myapp
    POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:?required}
  ports: ["5432:5432"]
  volumes: [postgres-data:/var/lib/postgresql/data]
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U myapp -d myapp"]
    interval: 10s
    timeout: 5s
    retries: 5
```
Connection string: `Host=postgres;Port=5432;Database=myapp;Username=myapp;Password=...`

### MySQL / MariaDB

```yaml
mysql:
  image: mysql:8
  environment:
    MYSQL_DATABASE: myapp
    MYSQL_USER: myapp
    MYSQL_PASSWORD: ${MYSQL_PASSWORD:?required}
    MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD:?required}
  ports: ["3306:3306"]
  volumes: [mysql-data:/var/lib/mysql]
  healthcheck:
    test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
    interval: 10s
    retries: 5
```

### SQL Server (what we use)

```yaml
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-latest
  environment:
    ACCEPT_EULA: "Y"
    MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD:?required}
    MSSQL_PID: Developer
  ports: ["1433:1433"]
  volumes: [mssql-data:/var/opt/mssql]
  healthcheck:
    test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \"$$MSSQL_SA_PASSWORD\" -C -Q 'SELECT 1' -b"]
    interval: 10s
    retries: 12
    start_period: 30s
```

### Redis

```yaml
redis:
  image: redis:7-alpine
  command: ["redis-server", "--appendonly", "yes"]
  ports: ["6379:6379"]
  volumes: [redis-data:/data]
  healthcheck:
    test: ["CMD", "redis-cli", "ping"]
    interval: 10s
    retries: 5
```
`--appendonly yes` enables persistence; without it Redis is memory-only.

### MongoDB

```yaml
mongo:
  image: mongo:7
  environment:
    MONGO_INITDB_ROOT_USERNAME: root
    MONGO_INITDB_ROOT_PASSWORD: ${MONGO_PASSWORD:?required}
  ports: ["27017:27017"]
  volumes: [mongo-data:/data/db]
  healthcheck:
    test: ["CMD", "mongosh", "--eval", "db.adminCommand('ping')"]
    interval: 10s
    retries: 5
```

### Seq (what we use)

```yaml
seq:
  image: datalust/seq:latest
  environment:
    ACCEPT_EULA: "Y"
  ports: ["5341:80"]
  volumes: [seq-data:/data]
```
Serves UI **and** ingestion on port 80. From another container: `http://seq:80`.

### RabbitMQ

```yaml
rabbitmq:
  image: rabbitmq:3-management-alpine
  environment:
    RABBITMQ_DEFAULT_USER: guest
    RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD:?required}
  ports:
    - "5672:5672"      # AMQP
    - "15672:15672"    # management UI
  volumes: [rabbitmq-data:/var/lib/rabbitmq]
  healthcheck:
    test: ["CMD", "rabbitmq-diagnostics", "check_port_connectivity"]
    interval: 15s
    retries: 5
```

### Mailhog (catch outgoing email in dev)

```yaml
mailhog:
  image: mailhog/mailhog
  ports:
    - "1025:1025"      # SMTP — point your app here
    - "8025:8025"      # web UI — read the caught mail here
```
No volume: it's ephemeral by design.

### Adminer (web DB client)

```yaml
adminer:
  image: adminer
  ports: ["8081:8080"]
  depends_on: [postgres]
```
Browse to `localhost:8081`, connect with server = `postgres` (the service name).

---
---

# PART IX — CLI COMMAND REFERENCE

## IX.1 docker compose, A→Z

### `build`

```bash
docker compose build
```
Build all services that have a `build:` block.

```bash
docker compose build api
```
Build one service.

```bash
docker compose build --no-cache api
```
Ignore all cached layers. Slow — use only when you suspect cache corruption.

```bash
docker compose build --pull
```
Re-pull base images before building. Use when a base image tag has been updated.

---

### `config`

```bash
docker compose config
```

**The most useful debugging command in Compose.** Resolves all `${VARS}`, merges
override files, applies defaults, and prints the final effective configuration.

```bash
docker compose config --quiet
```
Validate only — prints nothing, exits non-zero on error. Good for CI.

```bash
docker compose config --services
```
List service names.

---

### `cp`

```bash
docker compose cp api:/app/Logs/log-20260723.txt ./
```
Copy files out of (or into) a container. Handy for grabbing the Serilog files
before they're lost on `down`.

---

### `down`

```bash
docker compose down
```
Stop and **remove** containers and the network. **Volumes survive.**

```bash
docker compose down -v
```
Also delete named volumes. **This destroys your database and all Seq history. No
undo.**

```bash
docker compose down --rmi all
```
Also delete the built images.

---

### `exec`

```bash
docker compose exec api sh
```
Open a shell in a **running** container. The fastest way to answer "is the file
actually where I think it is?"

```bash
docker compose exec api printenv
```
Dump all environment variables — verify your `environment:` block landed.

```bash
docker compose exec api ls -la /app
```

```bash
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT name FROM sys.databases"
```

Note: `sh` not `bash` for Alpine images — Alpine has no bash by default.

---

### `images`

```bash
docker compose images
```
List the images used by this project, with sizes.

---

### `kill`

```bash
docker compose kill
```
`SIGKILL` immediately, no graceful shutdown. Use `stop` unless something is hung.

---

### `logs`

```bash
docker compose logs
```
All logs from all services.

```bash
docker compose logs -f api
```
Follow one service live. **This is where your Serilog Console sink output goes.**

```bash
docker compose logs --tail=100 api
```
Last 100 lines.

```bash
docker compose logs -t
```
With timestamps.

---

### `ps`

```bash
docker compose ps
```
What's running, with status and ports. The `STATUS` column shows health:
`Up 40s (healthy)`.

```bash
docker compose ps -a
```
Include stopped containers — needed to see one that crashed.

---

### `pull`

```bash
docker compose pull
```
Re-download images declared with `image:`. Doesn't touch built images.

---

### `restart`

```bash
docker compose restart api
```
Restart without recreating. **Does not pick up compose.yaml changes** — use `up -d`
for that.

---

### `run`

```bash
docker compose run --rm api dotnet ef database update
```
Run a **one-off** command in a new container. `--rm` removes it afterwards. Starts
dependencies but doesn't map ports (add `--service-ports` if you need them).

Different from `exec`, which uses an already-running container.

---

### `start` / `stop`

```bash
docker compose stop
docker compose start
```
Stop/start **without removing**. Faster than `down`/`up`, and preserves the
container's writable layer.

---

### `top`

```bash
docker compose top
```
Processes running in each container.

---

### `up` ★

```bash
docker compose up
```
Create and start everything. Foreground: logs stream to your terminal, `Ctrl+C`
stops.

```bash
docker compose up -d
```
Detached — background, terminal returned.

```bash
docker compose up --build
```
**Rebuild images first.** Plain `up` reuses existing images, so *your code changes
won't appear* without this. Most common beginner confusion.

```bash
docker compose up api
```
One service plus its `depends_on` chain.

```bash
docker compose up --force-recreate
```
Recreate containers even if nothing changed.

```bash
docker compose up --scale api=3
```
Run 3 copies. **Requires removing `container_name:`** and the port mapping (or
using a range).

```bash
docker compose up -d --wait
```
Wait until all healthchecks pass before returning. Excellent in CI.

---

## IX.2 docker, the ones you need

```bash
docker ps
```
All running containers, including ones not from this project.

```bash
docker ps -a
```
Including stopped/crashed ones.

```bash
docker images
```
Local images and their sizes.

```bash
docker volume ls
```
All volumes. Yours will be prefixed with the project name:
`urlshortener_mssql-data`.

```bash
docker volume inspect urlshortener_mssql-data
```

```bash
docker network ls
```
Networks. Compose created `urlshortener_default`.

```bash
docker network inspect urlshortener_default
```
Shows every container on the network and its IP — useful when DNS isn't resolving.

```bash
docker inspect urlshortener-api
```
Everything about a container: env vars, mounts, network, health, entrypoint.

```bash
docker stats
```
Live CPU/memory/network per container. Use it to catch SQL Server eating your RAM.

```bash
docker system df
```
How much disk Docker is using, broken down by images/containers/volumes/cache.

```bash
docker system prune
```
Delete stopped containers, unused networks, dangling images, and build cache.
**Safe** — doesn't touch volumes.

```bash
docker system prune -a --volumes
```
Delete **everything** not currently in use, including volumes. Frees a lot of
space; also deletes data. Read the confirmation prompt.

```bash
docker history urlshortener-api
```
Every layer in an image with its size. The tool for answering "why is my image
900 MB?"

---

## IX.3 Debugging workflows

### "My code change didn't appear"

```bash
docker compose up -d --build api
```
`up` alone reuses the existing image. You need `--build`.

### "Is my environment variable actually set?"

```bash
docker compose exec api printenv | sort
```
Or check what Compose *thinks* it's setting, before it runs:
```bash
docker compose config
```

### "Can container A reach container B?"

```bash
docker compose exec api getent hosts sqlserver
```
Resolves the name → confirms Compose DNS works.

```bash
docker compose exec frontend wget -qO- http://api:8080/health
```
Actually makes the request → confirms the service is listening.

If DNS resolves but the connection is refused, the target isn't listening on that
port — often because it bound to `127.0.0.1` instead of `0.0.0.0`.

### "The container starts then immediately exits"

```bash
docker compose logs api
```
Read the last lines — the crash reason is almost always there.

```bash
docker compose ps -a
```
Check the exit code. `Exited (0)` = the process finished normally (your PID 1
daemonised — see [III.2](#iii2-the-three-that-confuse-everyone)). Non-zero = crash.

Keep a broken container alive to poke around:
```bash
docker compose run --rm --entrypoint sh api
```

### "The healthcheck never goes healthy"

```bash
docker inspect --format "{{json .State.Health}}" urlshortener-sqlserver
```
Shows the last five probes **with their output**. Then run the check by hand:
```bash
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1"
```

### "Port is already allocated"

```bash
netstat -ano | findstr :1433
```
(PowerShell/Windows.) Find what's using it, then either stop it or change the port
in `.env`.

### "Build is slow / context is huge"

Look for `transferring context:` in the build output. If it's hundreds of MB, your
`.dockerignore` isn't matching. Remember: `bin/` matches only at the context root —
you need `**/bin/`.

### "I want to start completely fresh"

```bash
docker compose down -v --rmi local
```
Removes containers, volumes and locally-built images. **Deletes your database.**

---
---

# Appendix — The 12 mistakes that cost the most time

1. **Using `localhost` between containers.** Use the service name. Inside a
   container, `localhost` is that container.

2. **Using the published port between containers.** `sqlserver:1433`, not
   `sqlserver:5219`. Published ports are host↔container only.

3. **Forgetting `--build`.** `docker compose up` reuses the old image. Your code
   change isn't there.

4. **Binding to `127.0.0.1` inside the container.** Your app must listen on
   `0.0.0.0` or port mappings do nothing. (`ASPNETCORE_HTTP_PORTS` handles this for
   .NET; `--host 0.0.0.0` for uvicorn/vite.)

5. **`COPY . .` before installing dependencies.** Destroys layer caching; every
   build re-downloads everything.

6. **No `.dockerignore`.** Slow builds, and `obj/`/`node_modules/` from your host
   corrupt the container build in confusing ways.

7. **Missing SPA fallback in nginx.** The app works until someone refreshes on a
   deep link, then 404.

8. **Trailing slash on `proxy_pass`.** Silently strips your path prefix → 404s that
   look like routing bugs.

9. **Shell-form `ENTRYPOINT`.** `SIGTERM` never reaches your app; shutdown isn't
   graceful; buffered logs are lost.

10. **Forgetting `daemon off;`.** nginx forks, PID 1 exits, container stops
    instantly, and the logs show nothing wrong.

11. **No volume on the database.** `docker compose down` silently wipes everything.

12. **Unquoted YAML.** `restart: no` → `false`. `ports: 56:56` → `3416`.
    `ACCEPT_EULA: Yes` → `true`. Quote strings.

---

# Where to go next

Concrete improvements for this project, roughly in order of value:

1. **`compose.override.yaml` for hot reload** — bind-mount `./src` and run
   `dotnet watch`, so C# edits don't need a rebuild. [IV.4](#iv4-volumes--the-three-kinds)
   sketches it.
2. **Healthchecks on `api` and `frontend`**, so `docker compose up --wait` becomes
   meaningful and so you can see at a glance when the API is genuinely ready.
3. **`logging:` limits** on every service, so a chatty container can't fill your
   disk.
4. **`secrets:` instead of `environment:`** for `MSSQL_SA_PASSWORD` — env vars leak
   into `docker inspect` and crash dumps.
5. **A volume for `/app/Logs`**, or drop the Serilog File sink entirely now that
   Seq exists.
6. **A production compose file** — `ASPNETCORE_ENVIRONMENT=Production`, no
   published database port, pinned image digests.
7. **`app.UseForwardedHeaders(...)`** in `Program.cs` if you ever route the API
   through nginx, so client IPs stay correct for analytics.
