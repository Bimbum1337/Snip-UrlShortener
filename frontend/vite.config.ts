import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5173,
    // Keeps the browser on one origin in dev, so CORS is not in the way.
    proxy: {
      '/api': {
        target: 'http://localhost:5219',
        changeOrigin: true,
      },
    },
  },
})
