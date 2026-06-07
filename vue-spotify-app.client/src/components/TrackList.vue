<template>
  <div class="q-pa-md">
    <div v-if="statusCode === 200 || statusCode == null" class="q-gutter-lg">
      <div class="row">
      <div class="text-h5" v-if="numberOftracks != null">Total tracks: {{ numberOftracks }}</div>
      <QSpace />
      <QBtn label="Filter and sort tracks" color="primary" @click="openFilterAndSortDialog()" />
    </div>
      <div v-if="numberOftracks != null" class="row">
        <QSpace />
        <QPagination v-model="pageOffset" :max="Math.ceil(numberOftracks / trackLimit)" :max-pages="6" boundary-numbers direction-links @update:model-value="updateFilter(pageOffset)" />
      </div>
      <QMarkupTable v-if="trackViewModels.length > 0">
        <thead>
          <tr>
            <th></th>
            <th>Name</th>
            <th>Artists</th>
            <th>Album</th>
            <th>Track length</th>
            <th>Date saved</th>
            <th>Last recorded play date</th>
            <th v-if="!!props.playlistId">In Liked Songs?</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="x in trackViewModels" :key="x.id">
            <td>
              <QImg :src="x.albumCover" :alt="`Album cover for ${x.albumName} by ${x.artists.map((y) => y.name).join(', ')}`" width="50px" />
            </td>
            <td>
              <a :href="x.externalUrl">{{ x.name }}</a>
            </td>
            <td>
              <span v-for="y in x.artists" :key="y.id" :href="y.externalUrl"><a :href="y.externalUrl">{{ y.name }}</a><span v-if="x.artists.indexOf(y) < x.artists.length - 1">, </span></span>
            </td>
            <td>
              <a :href="x.albumExternalUrl">{{ x.albumName }}</a>
            </td>

            <td>{{ ConvertMilisecondsToMinutesAndSeconds(x.length) }}</td>
            <td>
              {{ (!!x.dateSaved ? date.formatDate(x.dateSaved, "Do MMM YYYY HH:mm") : "")}}
            </td>
            <td>
              {{ (!!x.dateLastPlayed ? date.formatDate(x.dateLastPlayed, "Do MMM YYYY HH:mm") : "n/a")}}
            </td>
            <td v-if="!!props.playlistId">
              {{ !!x.isInLikedSongs ? "Yes" : "No" }}
            </td>
            <td>
              <QBtnDropdown size="sm" color="primary" label="actions">
                <QList>
                  <QItem clickable v-close-popup :to="`viewtrack/${x.id}`">
                    <QItemSection>View track</QItemSection>
                  </QItem>
                  <QItem clickable v-close-popup @click="openQueueDialog(x.id, x.name)">
                    <QItemSection>Add track to queue</QItemSection>
                  </QItem>
                  <QItem clickable v-close-popup @click="copyTrackIdToClipboard(x.id)">
                    <QItemSection>Copy track ID</QItemSection>
                  </QItem>
                </QList>
              </QBtnDropdown>
            </td>
          </tr>
        </tbody>
      </QMarkupTable>

      <div v-if="numberOftracks != null" class="row">
        <QSpace />
        <QPagination v-model="pageOffset" :max="Math.ceil(numberOftracks / trackLimit)" :max-pages="6" boundary-numbers direction-links @update:model-value="updateFilter(pageOffset)" />
      </div>
    </div>
    <div v-else class="justify-center items-center" style="width: 75vh; height: 75vh">
      <p>Could not connect to server.</p>
      <QBtn label="Reload" @click="getTracks()" />
    </div>
    <QBtn v-if="!playlistId" color="deep-orange" label="Initialise backend database" @click="initialiseBackendDatabase()" />
    <QInnerLoading :showing="statusCode === null">
      <div class="row items-center justify-center" style="height: 200px;">
        <q-spinner-dots size="50px" color="green" />
      </div>
    </QInnerLoading>
  </div>
</template>
<script setup lang="ts">
  import TrackFilter from '@/classes/trackFilter';
  import TrackViewModel from '@/classes/trackViewModel';
  import AddTrackToQueueDialog from '@/dialogs/addTrackToQueueDialog.vue';
  import FilterTrackDialog from '@/dialogs/filterTrackDialog.vue';
  import ConvertMilisecondsToMinutesAndSeconds from '@/helperFunctions/convertMilisecondsToMinutesAndSeconds';
  import { useAuthStore } from '@/stores/authStore';
  import axios, { AxiosError } from 'axios';
  import { date, Dialog, Loading, Notify } from 'quasar';
  import { onBeforeMount, ref } from 'vue';
  import { useRoute, useRouter } from 'vue-router';


  const authStore = useAuthStore()
  const route = useRoute()
  const router = useRouter()

  const props = defineProps<{
    playlistId?: string | null
  }>()

  const numberOftracks = ref<number | null>(null)

  const pageOffset = ref<number>(1)

  const trackLimit = ref<number>(50)

  const trackQuery = ref<string>("")

  const trackViewModels = ref<TrackViewModel[]>([])

  const statusCode = ref<number | null>(null);

  const filter = ref<TrackFilter>(new TrackFilter());

  const columns = [
    {
      name: "albumCover",
      label: "",
      field: "albumCover",
      sortable: false,
      style: "width: 50px"
    },
    {
      name: "name",
      label: "Name",
      field: "name",
      sortable: true
    },
    {
      name: "artists",
      label: "Artist",
      field: "artists",
      sortable: true
    },
    {
      name: "albumName",
      label: "Album",
      field: "albumName",
      sortable: true
    },
    {
      name: "dateSaved",
      label: "Date saved",
      field: "dateSaved",
      sortable: true
    },
    {
      name: "lastPlayed",
      label: "Last play date",
      field: "dateLastPlayed",
      sortable: true
    },
    {
      name: "inLikedSongs",
      label: "In Liked Songs?",
      field: "isInLikedSongs",
      sortable: true
    },
    {
      name: "actions",
      label: "",
      field: "",
      sortable: true
    }
  ]

  onBeforeMount(async () => {
    if (route.query.page) pageOffset.value = parseInt(route.query.page.toString());
    if (route.query.query) filter.value.query = route.query.query.toString();
    if (route.query.searchName) filter.value.searchName = route.query.searchName.toString() === "true";
    if (route.query.searchArtist) filter.value.searchArtist = route.query.searchArtist.toString() === "true";
    if (route.query.searchAlbum) filter.value.searchAlbum = route.query.searchAlbum.toString() === "true";
    if (route.query.dateRangeFrom) filter.value.dateRangeFrom = route.query.dateRangeFrom.toString();
    if (route.query.dateRangeTo) filter.value.dateRangeTo = route.query.dateRangeTo.toString();
    if (route.query.sortType) filter.value.sortType = parseInt(route.query.sortType.toString());
    if (route.query.sortOrder) filter.value.sortOrder = parseInt(route.query.sortOrder.toString());
    await getTracks();
  })

  async function getTracks() {

    console.log(route.query, pageOffset.value);
    //pageOffset.value = route.query.page ? parseInt(route.query.page.toString()) : 1
    trackQuery.value = route.query.trackQuery ? route.query.trackQuery.toString() : ""

    statusCode.value = null;

    try {
      const query = new URLSearchParams();
      if (!!props.playlistId) query.append("playlistId", props.playlistId.toString());
      query.append("offset", ((pageOffset.value - 1) * trackLimit.value).toString());
      query.append("numberOfTracks", trackLimit.value.toString());
      if (filter.value.query !== null && filter.value.query.match(/^ *$/) == null) query.append("query", filter.value.query);
      query.append("searchName", filter.value.searchName.toString());
      query.append("searchArtist", filter.value.searchArtist.toString());
      query.append("searchAlbum", filter.value.searchAlbum.toString());
      if (filter.value.dateRangeFrom != null) query.append("dateRangeFrom", filter.value.dateRangeFrom.toString());
      if (filter.value.dateRangeTo != null) query.append("dateRangeTo", filter.value.dateRangeTo.toString());
      query.append("sortType", filter.value.sortType.toString());
      query.append("sortOrder", filter.value.sortOrder.toString());

      const response = await axios.get(`/api/track/gettracks?${query.toString()}`)
      console.log(response.data)
      trackViewModels.value = []
      numberOftracks.value = response.data.totalTracks
      response.data.tracks.forEach((i) => {
        const trackViewModel: TrackViewModel = new TrackViewModel(i)
        trackViewModels.value.push((trackViewModel))
      })
      statusCode.value = response.status;
    } catch (error) {
      const ex = error as AxiosError
      statusCode.value = ex.status || null
    }
  }

  function copyTrackIdToClipboard(trackId: string) {
    navigator.clipboard.writeText(trackId).then(() => {
      alert("Track ID copied to clipboard.")
    }).catch((err) => {
      alert("Failed to copy track ID to clipboard.")
    })
  }

  async function updateFilter(offset: number = 1) {
    const query = new URLSearchParams();
    query.append("page", offset.toString());
    if (filter.value.query !== null && filter.value.query.match(/^ *$/) == null) query.append("query", filter.value.query)
    query.append("searchName", filter.value.searchName.toString())
    query.append("searchArtist", filter.value.searchArtist.toString())
    query.append("searchAlbum", filter.value.searchAlbum.toString())
    if (filter.value.dateRangeFrom != null) query.append("from", filter.value.dateRangeFrom.toString())
    if (filter.value.dateRangeTo != null) query.append("to", filter.value.dateRangeTo.toString())
    query.append("sort", filter.value.sortType.toString());
    query.append("order", filter.value.sortOrder.toString());
    router.push(!!props.playlistId ? `/playlists/${props.playlistId}?${query.toString()}` : `/?${query.toString()}`);
    await getTracks();
  }

  async function initialiseBackendDatabase() {

    Loading.show({
      message: 'Initialising backend database...',
    })
    try {
      const response = await axios.get("/api/track/initialisetracks",
        {
          headers: { "authToken": authStore.accessToken, "Content-Type": "application/json", "Accept": "application/json" }
        })
      console.log(response.data)
      alert(`Successfully initialised backend database. Call was completed in ${response.data} ms.`)
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

  function openQueueDialog(trackId: string, name: string) {
    Dialog.create({
      component: AddTrackToQueueDialog,
      componentProps: {
        trackId: trackId,
        name: name
      }
    }).onOk(async (data) => {
      Notify.create("Successfully added track to queue.");
    });
  }

  function openFilterAndSortDialog() {
    Dialog.create({
      component: FilterTrackDialog,
      componentProps: {
        currentFilter: filter.value
      }
    }).onOk(async (data) => {
      console.log(data);
      filter.value = data;
      pageOffset.value = 1;
      await updateFilter();
    });
  }
</script>
