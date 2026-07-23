<script setup lang="ts">
import { ref } from 'vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import { useLinksStore } from '@/stores/links'
import type { ShortUrl } from '@/types'

const props = defineProps<{ link: ShortUrl }>()
const emit = defineEmits<{ close: []; saved: [link: ShortUrl] }>()

const links = useLinksStore()

const destination = ref(props.link.destination)
const title = ref(props.link.title ?? '')
const expiresAt = ref(toLocalInput(props.link.expiresAtUtc))
const isActive = ref(props.link.isActive)
const isSaving = ref(false)

async function save() {
  isSaving.value = true

  const hadExpiry = props.link.expiresAtUtc !== null

  const updated = await links.update(props.link.code, {
    destination: destination.value.trim(),
    title: title.value.trim(),
    expiresAtUtc: expiresAt.value ? new Date(expiresAt.value).toISOString() : null,
    clearExpiry: hadExpiry && !expiresAt.value,
    isActive: isActive.value,
  })

  isSaving.value = false

  if (updated) {
    emit('saved', updated)
    emit('close')
  }
}

// datetime-local wants local time with no zone suffix.
function toLocalInput(iso: string | null): string {
  if (!iso) return ''
  const date = new Date(iso)
  const offsetMs = date.getTimezoneOffset() * 60_000
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16)
}
</script>

<template>
  <BaseModal :title="`Edit /${link.code}`" subtitle="The alias itself cannot be changed." @close="emit('close')">
    <form class="flex flex-col gap-4" @submit.prevent="save">
      <BaseInput id="edit-destination" v-model="destination" label="Destination" type="url" />
      <BaseInput id="edit-title" v-model="title" label="Title" placeholder="Optional label" />
      <BaseInput
        id="edit-expires"
        v-model="expiresAt"
        label="Expires"
        type="datetime-local"
        hint="Clear the field to remove the expiry."
      />

      <label class="flex cursor-pointer items-center gap-3 rounded-xl border border-line p-3">
        <input v-model="isActive" type="checkbox" class="peer sr-only" />
        <span
          class="relative h-6 w-11 rounded-full bg-ink/15 transition-colors peer-checked:bg-brand after:absolute after:top-0.5 after:left-0.5 after:size-5 after:rounded-full after:bg-white after:transition-transform peer-checked:after:translate-x-5"
        />
        <span class="text-sm text-ink">
          {{ isActive ? 'Active - the link redirects' : 'Disabled - visitors get a 403' }}
        </span>
      </label>

      <div class="mt-2 flex justify-end gap-2">
        <BaseButton variant="ghost" @click="emit('close')">Cancel</BaseButton>
        <BaseButton type="submit" :loading="isSaving">Save changes</BaseButton>
      </div>
    </form>
  </BaseModal>
</template>
