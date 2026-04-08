<!--
  TODO:
    Add calls to get playlists track is in
-->
<template>
<div class="q-pa-md">
<!--TODO: add skeleton for loading-->
<div class="row q-gutter-md">
  <QImg fit="scale-down" width="300px" v-if="track" :src="track.albumCover"/>
  <QSkeleton v-else width="300px" height="300px" type="rect"/>
  <div v-if="track">
    <h4><span><a :href="track.externalUrl">{{track.name}}</a> ({{ ConvertMilisecondsToMinutesAndSeconds(track.length) }})</span></h4>
    <h5 v-if="track"><span v-for="y in track.artists" :key="y.id" :href="y.externalUrl"><a :href="y.externalUrl">{{ y.name }}</a><span v-if="track.artists.indexOf(y) < track.artists.length - 1">, </span></span></h5>
    <h5 v-if="track"><a :href="track.albumExternalUrl">{{track.albumName}}</a></h5>
    <h5 v-if="track">Added to liked songs: {{ (!!track.dateSaved ? date.formatDate(track.dateSaved, "Do MMM YYYY HH:mm") : "n/a")}}</h5>
  </div>
  <div v-else class="q-gutter-lg q-pa-lg">
    <QSkeleton width="300px" type="rect"/>
    <QSkeleton width="300px" type="rect"/>
    <QSkeleton width="300px" type="rect"/>
    <QSkeleton width="300px" type="rect"/>
  </div>
</div>
<div class="row q-gutter-md">
  <div class="col relative-position">
    <h5>Playlists containing this track</h5>
    <div v-if="playlistStatusCode === 200 || playlistStatusCode == null" class="q-gutter-sm">
    <h6>Total playlists: {{ totalPlaylists }}</h6>
      <QMarkupTable v-if="listOfPlaylists.length > 0">
        <thead>
        <tr>
          <th></th>
          <th>Name</th>
          <th>Date added to playlist</th>
          <th></th>
        </tr>
        </thead>
        <tbody>
        <tr v-for="x in listOfPlaylists" :key="x.playlistID">
          <td>
            <QImg :src="x.image" :alt="`Playlist cover for ${x.playlistName}`" width="50px"/>
          </td>
          <td><a :href="'#'">{{ x.playlistName }}</a></td>
          <td>{{ date.formatDate(x.dateAdded, "Do MMM YYYY HH:mm") }}</td>
          <td>
          <QBtn size="sm" color="secondary" label="view" :to="`/playlists/${x.playlistID}`" />
          </td>
        </tr>
        </tbody>
      </QMarkupTable>
      <QPagination v-if="totalPlaylists > 0" v-model="playlistPage" :max="Math.ceil(totalPlaylists / 20)" :max-pages="6" boundary-numbers direction-links @update:model-value="getTrackPlaylists()"/>
      <div v-else-if="playlistStatusCode === 200">No playlists found containing this track.</div>
    </div>
    <div v-else>An error has occured while fetching playlists containing this track.</div>
    <QInnerLoading :showing="playlistStatusCode === null">
      <div class="row items-center justify-center" style="height: 200px;">
        <q-spinner-dots size="50px" color="green" />
      </div>
    </QInnerLoading>
  </div>
  <div class="col relative-position">
    <h5>Playback records</h5>
    <div v-if="recordsStatusCode === 200 || recordsStatusCode == null" class="q-gutter-sm">
      <h6>Total records: {{ totalRecords }}</h6>
      <QMarkupTable v-if="listOfRecords.length > 0">
        <thead>
        <tr>
          <th>Date played</th>
        </tr>
        </thead>
        <tbody>
        <tr v-for="x in listOfRecords" :key="x">
          <td>{{ date.formatDate(x, "Do MMM YYYY HH:mm") }}</td>
        </tr>
        </tbody>
      </QMarkupTable>
      <QPagination v-if="totalRecords > 0" v-model="recordsPage" :max="Math.ceil(totalRecords / 20)" :max-pages="6" boundary-numbers direction-links @update:model-value="getTrackPlaybackRecords()"/>
      <div v-else-if="recordsStatusCode === 200">No playback records found.</div>
    </div>
    <div v-else>An error has occured while fetching playback records.</div>
    <QInnerLoading :showing="recordsStatusCode === null">
      <div class="row items-center justify-center" style="height: 200px;">
        <q-spinner-dots size="50px" color="green" />
      </div>
    </QInnerLoading>
</div>
</div>
</div>
</template>
<script setup lang="ts">

import TrackViewModel from "@/classes/trackViewModel";
import ConvertMilisecondsToMinutesAndSeconds from "@/helperFunctions/convertMilisecondsToMinutesAndSeconds";
import { useAuthStore } from "@/stores/authStore";
import axios, { Axios, AxiosError } from "axios";
import { onBeforeMount, ref, watch } from "vue";
import {useRoute, useRouter} from "vue-router"
import { date } from "quasar";
import type TrackPlaylistViewModel from "@/classes/trackPlaylistViewModel";

const track = ref<TrackViewModel | null>(null)

const route = useRoute();
const authStore = useAuthStore();

const listOfRecords = ref<string[]>([]);
const recordsPage = ref(1);
const totalRecords = ref(0);
const recordsStatusCode = ref<number | null>(null)

const listOfPlaylists = ref<TrackPlaylistViewModel[]>([]);
const playlistPage = ref(1);
const totalPlaylists = ref(0)
const playlistStatusCode = ref<number | null>(null)

const id = ref(route.params.id);

watch(
  route,
  async () => {
    console.log(route.params.id);
    id.value = route.params.id;
    await getTrackInformation();
    await getTrackPlaybackRecords();
    await getTrackPlaylists();
  }
)

async function getTrackInformation() {
  track.value = null;
  try{
    const response = await axios.get(`/track/gettrack/${id.value}`, {
      headers: {
        authToken: authStore.accessToken
      }
    });
    track.value = new TrackViewModel(response.data);
    console.log(response.data);
  }
  catch (ex) {
    const error = ex as AxiosError;
    console.log(error);
    alert("An error has occured.");
  }
}

async function getTrackPlaybackRecords() {
  recordsStatusCode.value = null;
  try{
    const response = await axios.get(`playbackrecord/getrecordspertrack?trackId=${id.value}&offset=${1}&numberOfRecords=${20}`,
      {
      headers: { "Content-Type": "application/json", "Accept": "application/json"}
    }
    );
    listOfRecords.value = response.data.records;
    totalRecords.value = response.data.totalRecords;
    recordsStatusCode.value = response.status;
    console.log(response)
  }
  catch (ex){
    const error = ex as AxiosError;
    recordsStatusCode.value = error.status;
    console.log(error);
  }
}

  async function getTrackPlaylists() {
    playlistStatusCode.value = null;
    try{
      const response = await axios.get(`playlist/gettrackplaylists?trackId=${id.value}&offset=${(playlistPage.value - 1) * 20}&numberOfPlaylists=${20}`,
        {
          headers: { authToken: authStore.accessToken }
        }
      );
      listOfPlaylists.value = [];
      response.data.playlists.forEach((i) => {
        listOfPlaylists.value.push(i as TrackPlaylistViewModel);
      })
      totalPlaylists.value = response.data.totalPlaylists;
      playlistStatusCode.value = response.status;
      console.log(response)
    }
    catch (ex){
      const error = ex as AxiosError;
      playlistStatusCode.value = error.status;
      console.log(error);
    }
  }

onBeforeMount(async () => {
  await getTrackInformation();
  await getTrackPlaybackRecords();
  await getTrackPlaylists();
});

</script>
