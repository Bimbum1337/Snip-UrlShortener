# Multi-stage build: a big SDK image compiles the app, then only the compiled
# output is copied into a small runtime image. The SDK stage is thrown away, so
# the final image has no compilers, no source code and no NuGet cache.
#
# Build context is the REPO ROOT (see compose.yaml), so all paths below are
# relative to the folder containing UrlShortener.slnx.

# ---- Stage 1: build -----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy ONLY the project files first, restore, then copy the rest.
# Why: Docker caches each layer. Source code changes on every edit, but the
# csproj files rarely do - so the slow `restore` layer stays cached and is
# skipped on most rebuilds.
COPY Directory.Build.props ./
COPY src/UrlShortener.Domain/UrlShortener.Domain.csproj                 src/UrlShortener.Domain/
COPY src/UrlShortener.Application/UrlShortener.Application.csproj       src/UrlShortener.Application/
COPY src/UrlShortener.Infrastructure/UrlShortener.Infrastructure.csproj src/UrlShortener.Infrastructure/
COPY src/UrlShortener.Api/UrlShortener.Api.csproj                       src/UrlShortener.Api/

RUN dotnet restore src/UrlShortener.Api/UrlShortener.Api.csproj

# Now the source. This layer invalidates on every code change - everything above
# it stays cached.
COPY src/ src/

RUN dotnet publish src/UrlShortener.Api/UrlShortener.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Stage 2: runtime ---------------------------------------------------------
# aspnet (not sdk) image: runtime only, roughly a quarter of the size.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# The base image ships a non-root user called `app`. Serilog's file sink writes
# to ./Logs, so create it and hand it over before dropping privileges.
RUN mkdir -p /app/Logs && chown -R app:app /app

# `--from=build` pulls files out of the previous stage.
COPY --from=build --chown=app:app /app/publish .

USER app

# Documentation only - it does not publish anything. `ports:` in compose does.
EXPOSE 8080

ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]
