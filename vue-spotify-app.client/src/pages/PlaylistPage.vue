<template>
    <div>
         <h3>Playlists</h3>
    <h5 v-if="numberOfPlaylists != null">Total playlists: {{ numberOfPlaylists }}</h5>

    <div v-if="statusCode === 200 || statusCode == null">
      <div v-if="numberOfPlaylists != null" class="row">
        <QSpace />
        <QPagination v-model="pageOffset" :max="Math.ceil(numberOfPlaylists / playlistLimit)" :max-pages="6" boundary-numbers direction-links @update:model-value="() => router.push(`playlists?page=${pageOffset}`)" />
      </div>
      <QMarkupTable v-if="playlists.length > 0">
        <thead>
          <tr>
            <th></th>
            <th>Name</th>
            <th>Creator</th>
            <th>Number of tracks</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="x in playlists" :key="x.id">
            <td>
              <QImg :src="x.imageLink" :alt="`Playlist image for ${x.name} by ${x.ownerName}`" width="50px"/>
            </td>
            <td>
              <a :href="x.externalURL">{{ x.name }}</a>
            </td>
            <td>
              <a :href="x.ownerLink">{{ x.ownerName }}</a>
            </td>
            <td>
              {{ x.numberOfTracks }}
            </td>
            <td>
              <QBtn size="sm" color="secondary" label="view" :to="`playlists/${x.id}`" />
            </td>
          </tr>
        </tbody>
      </QMarkupTable>

    </div>
    <div v-else class="justify-center items-center" style="width: 75vh; height: 75vh">
      <p>Could not connect to server.</p>
      <QBtn label="Reload" @click="getPlaylists()" />
    </div>
    <QBtn color="deep-orange" label="Initialise backend database" @click="initialiseBackendDatabase()" />
    <QInnerLoading :showing="statusCode === null">
      <div class="row items-center justify-center" style="height: 200px;">
        <q-spinner-dots size="50px" color="green" />
      </div>
    </QInnerLoading>
    </div>
</template>
<script setup lang="ts">
import PlaylistViewModel from '@/classes/playlistViewModel';
import { useAuthStore } from '@/stores/authStore';
import axios, { AxiosError } from 'axios';
import { Loading } from 'quasar';
import { onBeforeMount, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

    const authStore = useAuthStore()
  const route = useRoute()
  const router = useRouter()

  const numberOfPlaylists = ref<number | null>(null)
  const playlists = ref<PlaylistViewModel[]>([]);

  const pageOffset = ref<number>(1)

  const playlistLimit = ref<number>(20)

  const statusCode = ref<number | null>(null)

    onBeforeMount(async () => {
      await getPlaylists();
    })

    watch(
      () => route.query.page,
      async () => {
        await getPlaylists();
      }
    )

    async function getPlaylists() {
      pageOffset.value = route.query.page? parseInt(route.query.page.toString()) : 1;

      statusCode.value = null;

      try {
        const response = await axios.get(`/api/playlist/getplaylists?offset=${(pageOffset.value - 1) * playlistLimit.value}&numberOfPlaylists=${playlistLimit.value}`,{
          headers: { "authToken": authStore.accessToken, "Content-Type": "application/json", "Accept": "application/json" }
        })
        playlists.value = []
        numberOfPlaylists.value = response.data.totalPlaylists;
        response.data.playlists.forEach(element => {
          playlists.value.push(element as PlaylistViewModel);
        });
        statusCode.value = response.status;
        console.log(numberOfPlaylists.value);
      } catch (error) {
        const ex = error as AxiosError;
        statusCode.value = ex.status || null;
      }
    }

    async function initialiseBackendDatabase() {

    Loading.show({
      message: 'Initialising backend database...',
    })
    try {
      const response = await axios.get("/api/playlist/initialisetracks",
        {
          headers: { "authToken": authStore.accessToken, "Content-Type": "application/json", "Accept": "application/json" }
        })
      console.log(response.data)
      alert(`Successfully initialised backend database. Call was completed in ${response.data} s.`)
    }
    catch (error) {
      const ex = error as AxiosError
      console.log(ex.status)
      alert("Error calling server.")
    }


    finally {
      Loading.hide()
    }
  }
</script>
