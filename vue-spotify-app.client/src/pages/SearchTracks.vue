<template>
  <h3>Search for items in playlist</h3>
  <p>Use the form below to check if a track exists in a specified playlist.</p>
  <QForm @submit="searchForTrack">
    <QInput v-model="playlistID"
    label="Playlist ID"/>
    <QInput v-model="trackQuery"
    label="Track name"/>
    <QBtn type="submit" label="Submit"/>
  </QForm>
  <br/>
  <p>Found tracks: {{ foundTracks.length }}</p>
  <QMarkupTable v-if="foundTracks.length > 0">
    <thead>
      <tr>
        <th></th>
        <th>ID</th>
        <th>Name</th>
        <th>Artist(s)</th>
        <th>Album</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="x in foundTracks" :key="x.id">
        <td>
          <!--<QImg width="30px" :src="x.albumCover"/>-->
        </td>
        <td>{{ x.id }}</td>
        <td>
          <a :href="x.externalUrl">{{ x.name }}</a>
        </td>
        <td>
          <span v-for="y in x.artists" :key="y.id" :href="y.externalUrl"><a :href="y.externalUrl">{{ y.name }}</a><span v-if="x.artists.indexOf(y) < x.artists.length - 1">, </span></span>
        </td>
        <td>
          <a :href="x.albumExternalUrl">{{ x.albumName }}</a>
        </td>
        <td>
          {{ x.dateSaved}}
        </td>
      </tr>
    </tbody>
  </QMarkupTable>
  </template>
  <script setup lang="ts">
import axios from 'axios';
import { ref } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import TrackViewModel from '@/classes/trackViewModel';

    const authStore = useAuthStore()

  const playlistID = ref("")
  const trackQuery = ref("")
  const foundTracks = ref<TrackViewModel[]>([])

  async function searchForTrack(){
    foundTracks.value = []
    try{
      const response = await axios.get(
      `https://api.spotify.com/v1/playlists/${playlistID.value}`, {
        headers: { Authorization: `Bearer ${authStore.accessToken}` },
      }
    )
    response.data.tracks.items.forEach(x => {
      const trackData = x.track
      if(trackData.name.toLowerCase().startsWith(trackQuery.value.toLowerCase())){
        const artists = []
        trackData.artists.forEach(y => {
          const artist = {
            id: y.id,
            name: y.name,
            uri: y.uri,
            externalURL:y.external_urls.spotify
          }
          console.log(y.external_urls.spotify)
          artists.push(artist)
        })
        console.log(artists)
        const trackViewModel = new TrackViewModel({
          id: trackData.id,
          name: trackData.name,
          uri: trackData.uri,
          externalURL: trackData.external_urls.spotify,
          artists,
          albumName: trackData.album.name,
          albumCover: trackData.album.images[0].url,
          albumURI: trackData.album.uri,
          albumExternalURL: trackData.album.external_urls.spotify,
          length: trackData.duration_ms,
          dateSaved: x.added_at
        })
        foundTracks.value.push(trackViewModel)
      }
    })
    }
    catch(ex){
      console.log(ex)
      alert("Something went wrong with the request.")
    }
  }

  </script>
