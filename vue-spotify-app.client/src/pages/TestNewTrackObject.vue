<template>
  <h3>Test track object function</h3>
  <button @click="testFunction()">Run function</button>
</template>
<script setup lang="ts">
import { useAuthStore } from '@/stores/authStore';
import Track from "../classes/track"
import axios from 'axios';

const authStore = useAuthStore();

async function testFunction(){
  const response = await axios.get('https://api.spotify.com/v1/me/tracks?offset=0&limit=10', {
    headers: {"authToken": authStore.accessToken}
  })
  response.data.items.forEach(x => {
    const track = new Track();
    track.setTrack(x.track)
    console.log(track.name);
  })
}

</script>
