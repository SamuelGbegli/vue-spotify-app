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
          <a :href="x.externalURL">{{ x.name }}</a>
        </td>
        <td>
          <span v-for="y in x.artists" :key="y.id" :href="y.externalURL"><a :href="y.externalURL">{{ y.name }}</a><span v-if="x.artists.indexOf(y) < x.artists.length - 1">, </span></span>
        </td>
        <td>
          <a :href="x.albumExternalURL">{{ x.albumName }}</a>
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

  const playlistID = ref("");
  const trackQuery = ref("");
  const foundTracks = ref<TrackViewModel[]>([]);

  async function searchForTrack(){
    foundTracks.value = []

  }

  </script>
