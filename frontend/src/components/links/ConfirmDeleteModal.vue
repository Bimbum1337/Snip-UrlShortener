<script setup lang="ts">
import { ref } from 'vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import { useLinksStore } from '@/stores/links'
import type { ShortUrl } from '@/types'

const props = defineProps<{ link: ShortUrl }>()
const emit = defineEmits<{ close: []; deleted: [] }>()

const links = useLinksStore()
const isDeleting = ref(false)

async function confirm() {
  isDeleting.value = true
  const ok = await links.remove(props.link.code)
  isDeleting.value = false

  if (ok) {
    emit('deleted')
    emit('close')
  }
}
</script>

<template>
  <BaseModal title="Delete this link?" @close="emit('close')">
    <p class="text-sm text-muted">
      <span class="font-mono text-ink">/{{ link.code }}</span> and its
      {{ link.visitCount }} recorded visit{{ link.visitCount === 1 ? '' : 's' }} will be removed.
      Anyone following the link afterwards gets a 404. This cannot be undone.
    </p>

    <div class="mt-6 flex justify-end gap-2">
      <BaseButton variant="ghost" @click="emit('close')">Cancel</BaseButton>
      <BaseButton variant="danger" :loading="isDeleting" @click="confirm">Delete link</BaseButton>
    </div>
  </BaseModal>
</template>
