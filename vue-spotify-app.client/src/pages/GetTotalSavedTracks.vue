<template>
  <h3>Get number of tracks saved</h3>
  <p>Click the button to get the total number of tracks saved in Spotify.</p>
  <button @click="getNumberOfSavedTracks()">Load</button>
  <p v-if="isReadingDone">{{ `${numberOfSavedTracks} tracks are saved.` }}</p>
</template>
<script setup lang="ts">
import { useAuthStore } from '@/stores/authStore'
import axios from 'axios'
import { ref } from 'vue'

const authStore = useAuthStore()

const numberOfSavedTracks = ref(0)
const isReadingDone = ref(false)

async function getNumberOfSavedTracks() {
  numberOfSavedTracks.value = 0

  const response = await axios.get('https://api.spotify.com/v1/me/tracks?offset=0&limit=20', {
    headers: { Authorization: `Bearer ${authStore.accessToken}` },
  })

  numberOfSavedTracks.value = response.data.total

  isReadingDone.value = true
}
</script>
