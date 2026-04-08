<template>
  <h2>Get tracks played per day</h2>
  <p>Use the form to get the tracks played on an account per day.</p>
  <QForm @submit="getPlayedTracks">
    <div class="row">
      <QInput type="date" required v-model="selectedDate" style="width: 400px;"/>
    <QBtn type="submit" label="Submit"/>
    </div>
  </QForm>
  <QBtn label="Test playback history" @click="testPlaybackHistoryBackend()"/>
  <p>There are {{ availableTracks }} tracks viewable.</p>
  <QMarkupTable v-if="foundTracks.length > 0">
    <thead>
      <tr>
        <th>ID</th>
        <th>Name</th>
        <th>Artist(s)</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="x in foundTracks" :key="x.id">
        <td>{{ x.id }}</td>
        <td>{{ x.name }}</td>
        <td>{{ x.artists.map(y => y.name) }}</td>
      </tr>
    </tbody>
  </QMarkupTable>
</template>
<script setup lang="ts">
import Track from '@/classes/track'
import axios, { AxiosError } from 'axios'
import { ref } from 'vue'
import { useAuthStore } from '@/stores/authStore'

const selectedDate = ref<string>("")
const foundTracks = ref<Track[]>([])
const availableTracks = ref<number>(0);
const authStore = useAuthStore()

async function getPlayedTracks() {
  foundTracks.value = []
  const unixTimestamp = new Date(selectedDate.value).getTime()
  console.log(unixTimestamp)
  const response = await axios.get(
    `https://api.spotify.com/v1/me/player/recently-played?limit=50&after=${unixTimestamp}`,
    {
      headers: { Authorization: `Bearer ${authStore.accessToken}` },
    },
  )
  console.log(response.data)
  availableTracks.value = response.data.total;
  response.data.items.forEach((element) => {
    const track = new Track()
    track.setTrack(element.track)
    console.log(element)
    foundTracks.value.push(track)
  })
}

async function testPlaybackHistoryBackend(){
  try{
    const response = await axios.get(`track/testplaybackhistory`,{
      headers: { authToken: authStore.accessToken },
    })
    console.log(response.status)
  }
  catch(ex){
    console.log((ex as AxiosError));
  }
}
</script>
