<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

<template>
  <div class="grid place-items-center py-24 text-center">
    <p class="brand-gradient-text text-7xl font-bold tracking-tight">404</p>
    <h1 class="mt-4 text-xl font-semibold text-ink">That page does not exist</h1>
    <p class="mt-2 max-w-sm text-sm text-muted">
      Short links are served by the API, not by this app - if you were following one, it may have
      expired or been deleted.
    </p>

    <RouterLink
      :to="{ name: 'dashboard' }"
      class="mt-6 rounded-xl bg-linear-to-r from-brand to-accent px-5 py-2.5 text-sm font-medium text-white transition-all hover:brightness-110"
    >
      Back to the dashboard
    </RouterLink>
  </div>
</template>
