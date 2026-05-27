<template>
  <div class="q-pa-md">
    <div v-if="statusCode === 200 || statusCode == null" class="q-gutter-lg">
      <div class="row">
      <div class="text-h5" v-if="numberOftracks != null">Total tracks: {{ numberOftracks }}</div>
      <QSpace />
      <QBtn label="Filter and sort tracks" color="primary" @click="openFilterAndSortDialog()" />
    </div>
</div>
</div>
  <QTable class="my-table"
          style="height: 70vh"
          :rows="trackViewModels"
          :columns="columns"
          row-key="id"
          wrap-cells
          :loading="statusCode === null"
          virtual-scroll
          :virtual-scroll-item-size="50"
          :virtual-scroll-sticky-size-start="50"
          :rows-per-page-options="[0]"
          @virtual-scroll="onScroll">
    <template v-slot:body-cell-albumCover="props">
      <QTd :props="props">
        <QImg :src="props.row.albumCover"
              :alt="`Album cover for ${props.row.albumName} by ${props.row.artists.map((x) => x.name).join(', ')}`"
              width="48px" />
      </QTd>
    </template>
    <template v-slot:body-cell-name="props">
      <QTd :props="props">
        <div class="text-left wrap-text">
          <a :href="props.row.externalUrl">{{ props.row.name }}</a>
        </div>
      </QTd>
    </template>
    <template v-slot:body-cell-artists="props">
      <QTd :props="props">
        <div class="text-left">
          <span v-for="x in props.row.artists" :key="x.id" :href="x.externalUrl"><a :href="x.externalUrl">{{ x.name }}</a><span v-if="props.row.artists.indexOf(x) < props.row.artists.length - 1">, </span></span>
        </div>
      </QTd>
    </template>
    <template v-slot:body-cell-albumName="props">
      <QTd :props="props">
        <div class="text-left">
          <a :href="props.row.albumExternalUrl">{{ props.row.albumName }}</a>
        </div>
      </QTd>
    </template>
    <template v-slot:body-cell-length="props">
      <QTd :props="props">
        <div class="text-left">
          {{ ConvertMilisecondsToMinutesAndSeconds(props.row.length) }}
        </div>
      </QTd>
    </template>
    <template v-slot:body-cell-dateSaved="props">
      <QTd :props="props">
        <div class="text-left">
          {{ date.formatDate(props.row.dateSaved, "Do MMM YYYY HH:mm") }}
        </div>
      </QTd>
    </template>

    <template v-slot:body-cell-lastPlayed="props">
      <QTd :props="props">
        <div class="text-left">
          {{!!props.row.dateLastPlayed ? date.formatDate(props.row.dateLastPlayed, "Do MMM YYYY HH:mm") : "n/a" }}
        </div>
      </QTd>
    </template>
    <template v-slot:body-cell-inLikedSongs="props">
      <QTd :props="props">
        <div class="text-left">
          {{props.row.isInLikedSongs ? "Yes" : "No" }}
        </div>
      </QTd>
    </template>
    <template v-slot:body-cell-actions="props">
      <QTd :props="props">
        <div class="text-left">
          <QBtn flat dense icon="more_vert">
            <QMenu anchor="bottom left" self="top left">
            <QList style="min-width: 150px">
            <QItem clickable v-close-popup :to="`/viewtrack/${props.row.id}`">
                <QItemSection>
                  <QItemLabel>View track</QItemLabel>
                </QItemSection>
              </QItem>
              <QItem clickable v-close-popup @click="copyTrackIdToClipboard(props.row.id)">
                <QItemSection>
                  <QItemLabel>Copy track ID</QItemLabel>
                </QItemSection>
              </QItem>
              <QItem clickable v-close-popup @click="openQueueDialog(props.row.id, props.row.name)">
                <QItemSection>
                  <QItemLabel>Add to queue</QItemLabel>
                </QItemSection>
              </QItem>
            </QList>
            </QMenu>
          </QBtn>
        </div>
      </QTd>
    </template>
  </QTable>
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
  import { computed, onBeforeMount, ref } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import TrackViewModelBatch from '@/classes/trackViewModelBatch';


  const authStore = useAuthStore()
  const route = useRoute()
  const router = useRouter()

  // Stores the ID of the playlist whose tracks are to be fetched. If null,
  // tracks from the Liked Songs library are fetched instead.
  const props = defineProps<{
    playlistId?: string | null
  }>()

  // The total tracks stored in the playlist or Liked Songs library.
  const numberOftracks = ref<number | null>(null)

  // The current page of tracks the user is on.
  const pageOffset = ref<number>(1)

  // The maximum number of tracks to be fetched in a single request.
  const trackLimit = ref<number>(50)

  const trackQuery = ref<string>("")

  // Stores batches of tracks fetched from the server.
  const trackBatches = ref<TrackViewModelBatch[]>([])

  const maximumNumberOfBatches = ref(3);

  const trackViewModels = computed(() => {
    const allTracks = trackBatches.value.map(x => x.trackViewModels).flat();
    return allTracks;
  });

  const statusCode = ref<number | null>(null);

  const filter = ref<TrackFilter>(new TrackFilter());

    const total = ref<number>(0);

  // Represents the columns to be displayed in the table.
  const baseColumns = [
    // Shows the album cover of the track.
    {
      name: "albumCover",
      label: "",
      field: "albumCover",
      align: "left",
      sortable: false,
      style: "width: auto"
    },
    // Shows the track's name.
    {
      name: "name",
      label: "Name",
      field: "name",
      align: "left",
      sortable: true,
      style: "width: 20%"
    },
    // Shows the artists credited for the track.
    {
      name: "artists",
      label: "Artist",
      field: "artists",
      align: "left",
      style: "width: 200px",
      sortable: true
    },
    // Shows the name of the album the track comes from.
    {
      name: "albumName",
      label: "Album",
      field: "albumName",
      align: "left",
      style: "width: 200px",
      sortable: true
    },
    // Shows the track's length in minutes and seconds.
    {
      name: "length",
      label: "Length",
      field: "length",
      align: "left",
      style: "width: 100px",
      sortable: true
    },
    // Shows the date the track was added to the playlist or Liked Songs library.
    {
      name: "dateSaved",
      label: "Date saved",
      field: "dateSaved",
      align: "left",
      style: "width: 200px",
      sortable: true
    },
    // Shows the date the track was last recorded as played, if applicable.
    {
      name: "lastPlayed",
      label: "Last play date",
      field: "dateLastPlayed",
      align: "left",
      style: "width: 200px",
      sortable: true
    },
    // Shows whether the track is in the user's Liked Songs library.
    // Only displayed when viewing a playlist.
    {
      name: "inLikedSongs",
      label: "In Liked Songs?",
      field: "isInLikedSongs",
      align: "left",
      style: "width: 150px",
      sortable: true
    },
    // Shows a list of actions that can be performed with the track.
    {
      name: "actions",
      label: "Actions",
      field: "",
      align: "left",
      style: "width: 5%",
      sortable: false
    }
  ];

  // Generates the columns visible based on whether a playlist ID is provided.
  const columns = computed(() => {
    if(props.playlistId == null){
      return baseColumns.filter(x => x.name !== "inLikedSongs");
    }
    return baseColumns;
  });

  // Stores pagination values for the table.
  const pagination = ref({
    sortBy: "name",
    page: 1,
    rowsPerPage: 0,
    rowsNumber: 806
  });

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

  async function getTracks(addToTop:boolean = false, reset = false) {

    console.log(route.query, pageOffset.value);
    //pageOffset.value = route.query.page ? parseInt(route.query.page.toString()) : 1
    trackQuery.value = route.query.trackQuery ? route.query.trackQuery.toString() : ""

    statusCode.value = null;

    if(reset){
        pageOffset.value = 1;
        trackBatches.value = [];
    }

    try {
      console.log("page " + pagination.value.page);
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

      const response = await axios.get(`track/gettracks?${query.toString()}`);

      numberOftracks.value = response.data.totalTracks
      const batch = new TrackViewModelBatch();
      batch.batchIndex = response.data.tracks.batchIndex;
      response.data.tracks.trackViewModels.forEach((x: any) => {
        batch.trackViewModels.push(new TrackViewModel(x));
      });
      if(addToTop){
        if(trackBatches.value.length >= 3){
          trackBatches.value.pop();
        }
        trackBatches.value.unshift(batch);
      }
      else{
        if(trackBatches.value.length >= 3){
          trackBatches.value.shift();
        }
        trackBatches.value.push(batch);
      }
      console.log(batch);

      statusCode.value = response.status;
    } catch (error) {
      const ex = error as AxiosError
      statusCode.value = ex.status || null
    }
  }

  async function onScroll({from, to, index, ref}){
    const nearTop = from <= 5;
    const nearBottom = to >= trackViewModels.value.length - 5;
    console.log(pageOffset.value);
    if(trackViewModels.value.length > trackLimit.value){
      if(nearTop && Math.min(...trackBatches.value.map(x => x.batchIndex)) > 1  && statusCode.value != null){
      console.log("Top of page reached", trackBatches.value);
        pageOffset.value = trackBatches.value.length > 0 ? Math.min(...trackBatches.value.map(x => x.batchIndex)) - 1 : 1;
        console.log(pageOffset.value);
        await getTracks(true);
        if(trackBatches.value[0]?.batchIndex != 1) ref.scrollTo(index + trackBatches.value[0].trackViewModels.length, 0);
    }
    if (nearBottom && Math.max(...trackBatches.value.map(x => x.batchIndex)) < Math.ceil(pagination.value.rowsNumber / trackLimit.value) && statusCode.value != null){
      console.log("End of page reached", trackBatches.value);
        pageOffset.value = trackBatches.value.length > 0 ? Math.max(...trackBatches.value.map(x => x.batchIndex)) + 1 : 1;
        console.log(pageOffset.value);
        await getTracks();
        if(trackBatches.value[0]?.batchIndex != 1) ref.scrollTo(index - trackBatches.value[0].trackViewModels.length, 0);
    }
    }
  }


  function copyTrackIdToClipboard(trackId: string) {
    navigator.clipboard.writeText(trackId).then(() => {
      Notify.create({
          message: `Successfully copied track ID to clipboard.`,
          color: "green"
        });
    }).catch((err) => {
      Notify.create({
          message: `Failed to copy track ID.`,
          color: "red"
        });
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
    await getTracks(false, true);
  }

  function openQueueDialog(trackId: string, name: string) {
    Dialog.create({
      component: AddTrackToQueueDialog,
      componentProps: {
        trackId: trackId,
        name: name
      }
    }).onOk(async (data) => {

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
<style lang="css">
.my-table {

  border-collapse: separate ;
  overflow: auto;

  thead tr th {
  position: sticky;
  top: 0;
  z-index: 4;
  background: white;
}
td:nth-child(1),
  th:nth-child(1)
  {
    position: sticky;
    left: 0;
    background-color: white;
    z-index: 3;
    width: 80px;
  }
  th:nth-child(2),
  td:nth-child(2) {
    position: sticky;
    left: 80px;
    z-index: 3;
    background-color: white;
}

thead th:nth-child(1),
thead th:nth-child(2) {
  z-index: 5;
}
}
</style>
