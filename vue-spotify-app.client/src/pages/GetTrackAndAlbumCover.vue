<template>
  <img src="https://i.scdn.co/image/ab67616d00001e02ff9ca10b55ce82ae553c8228" />
  <h3>Get track</h3>
  <p>Use the form below to get information of a specific track.</p>
  <form v-on:submit.prevent="getTrackInformation()">
    <input required v-model="inputtedId" />
    <button type="submit">Search</button>
  </form>
  <br />
  <div v-if="track != null" style="display: flex">
    <div style="height: 150px; width: 150px">
      <img style="height: 150px; width: 150px" :src="track.album.albumCover ? track.album.albumCover.link : ''" />
    </div>
    <div>
      <h4>{{ track.name }}</h4>
      <p>{{ track.artists.map(x => x.name) }}</p>
      <p>{{ track.album.name }}</p>
    </div>
  </div>
  <button @click="testPostRequest" :disabled="!track">Test backend POST request</button>
  <button @click="testGetRequest" :disabled="!inputtedId || inputtedId === ''">Test backend GET request</button>
</template>
<script setup lang="ts">
import Track from '@/classes/track'
import { useAuthStore } from '@/stores/authStore'
import axios, { AxiosError } from 'axios'
import { ref } from 'vue'

const inputtedId = ref('')
const track = ref<Track>()
const statusCode = ref<number>()

const authStore = useAuthStore()

async function getTrackInformation() {
  try {
    const response = await axios.get(`https://api.spotify.com/v1/tracks/${inputtedId.value}`, {
      headers: { Authorization: `Bearer ${authStore.accessToken}` },
    })

    const newTrack = new Track();
    newTrack.setTrack(response.data)
    console.log(newTrack.name);

    console.log(response.data.id)

    track.value = newTrack;

    statusCode.value = response.status
  } catch (error) {
    const ex = error as AxiosError
    statusCode.value = ex.status
  }
  console.log(statusCode.value)
}

  async function testPostRequest() {
    try {
      const response = await axios.post("track/testpost", track.value)
      console.log(response.data)
      alert(`Successfully made post request with track ${track.value?.name}.`)
    }
    catch (ex) {
      alert("Failed to make post request.")
    }
  }

  async function testGetRequest() {
    try {
      const response = await axios.get(`track/makecalltoapi?authtoken=${authStore.accessToken}&trackId=${inputtedId.value}`)
      console.log(response.data)
      alert("Successfully made get request.")
    }
    catch (ex) {
      alert("Failed to make get request.")
    }
  }
</script>
