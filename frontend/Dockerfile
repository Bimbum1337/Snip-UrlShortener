# Same multi-stage idea as the API: Node compiles the Vue app into plain
# HTML/CSS/JS, then nginx serves those static files. Node is not in the final
# image - there is no Node process running in production.

# ---- Stage 1: build -----------------------------------------------------------
# node:24 ships npm 11, matching the npm that generated package-lock.json on the
# host. This matters: npm 10 and npm 11 resolve optional dependency trees
# differently, and `npm ci` aborts if the lockfile does not match exactly
# ("Missing: @emnapi/core@... from lock file").
FROM node:24-alpine AS build
WORKDIR /app

# Lockfile first, for the same layer-caching reason as the API's csproj files.
COPY package.json package-lock.json ./

# `npm ci` (not `install`) installs exactly what the lockfile says - reproducible
# and faster in CI.
RUN npm ci

COPY . .

RUN npm run build

# ---- Stage 2: serve -----------------------------------------------------------
FROM nginx:alpine AS final

# Our config replaces the default site definition.
COPY nginx.conf /etc/nginx/conf.d/default.conf

# vite build output lands in /app/dist.
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 8080

CMD ["nginx", "-g", "daemon off;"]
