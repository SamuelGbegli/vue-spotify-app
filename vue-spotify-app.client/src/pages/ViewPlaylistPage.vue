<template>
  <div class="q-pa-md">
    <!-- Shows playlist information -->
    <div class="row q-gutter-md">
      <QImg fit="scale-down" width="300px" v-if="playlistViewModel?.imageLink" :src="playlistViewModel.imageLink" />
      <div v-else-if="playlistViewModel" style="width: 300px; height: 300px" />
      <QSkeleton v-else width="300px" height="300px" type="rect" />
      <div v-if="playlistViewModel">
        <h4><a :href="playlistViewModel.externalURL">{{playlistViewModel.name}}</a></h4>
        <h5><a :href="playlistViewModel.ownerLink">{{playlistViewModel.ownerName}}</a></h5>
        <h5>Number of tracks: {{ playlistViewModel.numberOfTracks }}</h5>
        <p>{{ playlistViewModel.description }}</p>
      </div>
      <!-- Shows skeleton if playlist is loading and error message if playlist could not be loaded -->
      <div v-else-if="playlistStatusCode == null" class="q-gutter-lg q-pa-lg">
        <QSkeleton width="300px" type="rect" />
        <QSkeleton width="300px" type="rect" />
        <QSkeleton width="300px" type="rect" />
        <QSkeleton width="300px" type="rect" />
      </div>
    </div>
    <div>
      <!--<TrackList :playlistId="route.params.id?.toString()" />-->
      <TrackListNew :playlistId="route.params.id?.toString()"/>
    </div>
  </div>

</template>
<script setup lang="ts">
  import PlaylistViewModel from '@/classes/playlistViewModel';
  import TrackViewModel from '@/classes/trackViewModel';
  import TrackList from '@/components/TrackList.vue';
  import TrackListNew from '@/components/TrackListNew.vue';
  import { useAuthStore } from '@/stores/authStore';
  import axios, { AxiosError } from 'axios';
  import { onBeforeMount, ref, watch } from 'vue';
  import { useRoute, useRouter } from 'vue-router';


  // Stores the view model for the playlist
  const playlistViewModel = ref<PlaylistViewModel | null>(null);
  // Stores the status code when fetching a playlist
  const playlistStatusCode = ref<number | null>(null);

  const numberOftracks = ref<number | null>(null)

  const pageOffset = ref<number>(1)

  const trackLimit = ref<number>(50)

  const trackQuery = ref<string>("")

  const trackViewModels = ref<TrackViewModel[]>([])

  const trackStatusCode = ref<number | null>(null)

  const route = useRoute();
  const router = useRouter();

  const authStore = useAuthStore();

  onBeforeMount(async () => {
    await getPlaylist();
    await getTracks();
  });

  async function getPlaylist() {
    try {
      console.log(route.params.id)
      const response = await axios.get(`playlist/getplaylist/${route.params.id}`,
        {
          headers: { authToken: authStore.accessToken }
        }
      );
      playlistViewModel.value = response.data as PlaylistViewModel;
      playlistStatusCode.value = response.status;

    } catch (error) {
      playlistStatusCode.value = (error as AxiosError).status;
    }
  }

  async function getTracks() {

    pageOffset.value = route.query.page ? parseInt(route.query.page.toString()) : 1
    trackQuery.value = route.query.trackQuery ? route.query.trackQuery.toString() : ""

    trackStatusCode.value = null;

    try {
      const response = await axios.get(`playlist/getplaylisttracks/${route.params.id}?offset=${(pageOffset.value - 1) * trackLimit.value}&numberOfTracks=${trackLimit.value}&trackQuery=${trackQuery.value}`,
        {
          headers: { "authToken": authStore.accessToken, "Content-Type": "application/json", "Accept": "application/json" }
        })
      console.log(response.data)
      trackViewModels.value = []
      numberOftracks.value = response.data.totalTracks
      response.data.forEach((i) => {
        const trackViewModel: TrackViewModel = new TrackViewModel(i)
        trackViewModels.value.push((trackViewModel))
      })
      trackStatusCode.value = response.status;
    } catch (error) {
      const ex = error as AxiosError
      trackStatusCode.value = ex.status || null
    }
  }


</script>
