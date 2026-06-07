<template>
  <div class="q-pa-md">
    <div v-if="statusCode === 200 || statusCode == null" class="q-gutter-lg">
      <div class="row">
        <QChip v-if="route.query.query" :label="`'${route.query.query}' ${generateFilterQueryChip()}`" color="primary" text-color="white" />
        <QChip v-if="route.query.from || route.query.to">
          <template v-if="route.query.from && route.query.to">
            {{ `Saved between ${date.formatDate(new Date(route.query.from.toString()), "Do MMM YYYY")} and ${date.formatDate(new Date(route.query.to.toString()), "Do MMM YYYY")}` }}
          </template>
          <template v-else-if="route.query.from">
            {{ `Saved after ${date.formatDate(route.query.from.toString(), "Do MMM YYYY")}` }}
          </template>
          <template v-else-if="route.query.to">
            {{ `Saved before ${date.formatDate(route.query.to.toString(), "Do MMM YYYY")}` }}
          </template>
        </QChip>
        <QChip v-if="route.query.sort != null" :label="`Sorted by ${generateFilterSortChip()}`" color="secondary" text-color="white" />

        <QSpace />
        <QBtn label="Filter and sort tracks" color="primary" @click="openFilterAndSortDialog()" />
      </div>
    </div>
  </div>
  <QTable class="my-table"
          :title='(statusCode === 200 ? `Total tracks: ${ numberOftracks }` : statusCode === null ? "Loading tracks..." : "Failed to load tracks.")'
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
          v-model:pagination="pagination"
          @virtual-scroll="onScroll"
          @request="onRequest">
    <template v-slot:body-cell-albumCover="props">
      <QTd :props="props">
        <QImg :src="props.row.albumCover"
              :alt="`Album cover for ${props.row.albumName} by ${props.row.artists.map((x) => x.name).join(', ')}`"
              width="48px" />
      </QTd>
    </template>
    <template v-slot:body-cell-name="props">
      <QTd :props="props">
        <div>
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
  import { computed, watch, ref, onBeforeMount } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import TrackViewModelBatch from '@/classes/trackViewModelBatch';
import SortOrder from '@/enumClasses/sortOrder';
import SortType from '@/enumClasses/sortType';


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

  // The maximum number of batches to be stored at once.
  const maximumNumberOfBatches = ref(3);

  // The index of the current visible item in the list. Updates when the user scrolls up or down.
  const currentIndex = ref(0);

  // Computed function to flatten the batches of tracks into an array that can be read by the QTable element.
  const trackViewModels = computed(() => {
    const allTracks = trackBatches.value.map(x => x.trackViewModels).flat();
    return allTracks;
  });

  // Stores the status code of the request to get the list of items from the backend.
  const statusCode = ref<number | null>(null);

  // Stores a filter that is applied when fetching tracks.
  const filter = ref<TrackFilter>(new TrackFilter());

  // Stores the total tracks found in the backend that matches the filter.
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
      align: "left",
      style: "width: 5%",
      sortable: false
    }
  ];

  // Generates the columns visible based on whether a playlist ID is provided.
  const columns = computed(() => {
    if (props.playlistId == null) {
      return baseColumns.filter(x => x.name !== "inLikedSongs");
    }
    return baseColumns;
  });

  // Stores pagination values for the table.
  const pagination = ref({
    sortBy: "name",
    page: 1,
    rowsPerPage: 0,
    rowsNumber: 0,
    descending: false
  });

  onBeforeMount(async () => {
    //console.log("Section called");
    onRouteUpdate();
  })

  watch(route, async () => {
    console.log("Section called");
    onRouteUpdate();
  })

  // Handles changes to the filter if the route changes. Called when the page is
  // loaded or the user resubmits the page's URL.
  async function onRouteUpdate() {
    console.log(typeof (route.query.sort));
    if (route.query.page) pageOffset.value = parseInt(route.query.page.toString());
    if (route.query.query) filter.value.query = route.query.query.toString();
    if (route.query.searchName) filter.value.searchName = route.query.searchName.toString() === "true";
    if (route.query.searchArtist) filter.value.searchArtist = route.query.searchArtist.toString() === "true";
    if (route.query.searchAlbum) filter.value.searchAlbum = route.query.searchAlbum.toString() === "true";
    if (route.query.from) filter.value.dateRangeFrom = route.query.from.toString();
    if (route.query.to) filter.value.dateRangeTo = route.query.to.toString();
    if (route.query.sort) filter.value.sortType = parseInt(route.query.sort.toString());
    if (route.query.order) filter.value.sortOrder = parseInt(route.query.order.toString());
    await getTracks(false, true);
  }

  async function getTracks(addToTop: boolean = false, reset = false, batchIndexes: number[] | null = null) {

    console.log(route.query, pageOffset.value);
    //pageOffset.value = route.query.page ? parseInt(route.query.page.toString()) : 1
    trackQuery.value = route.query.trackQuery ? route.query.trackQuery.toString() : ""

    statusCode.value = null;

    if (reset) {
      pageOffset.value = 1;
      trackBatches.value = [];
    }

    try {
      console.log("page " + pagination.value.page);
      const query = new URLSearchParams();
      if (!!props.playlistId) query.append("playlistId", props.playlistId.toString());
      if (batchIndexes != null) {
        batchIndexes.forEach(x =>{
          query.append("offset", x.toString());
        })
      }
      else query.append("offset", ((pageOffset.value - 1) * trackLimit.value).toString());
      query.append("numberOfTracks", trackLimit.value.toString());
      if (filter.value.query !== null && filter.value.query.match(/^ *$/) == null) query.append("query", filter.value.query);
      query.append("searchName", filter.value.searchName.toString());
      query.append("searchArtist", filter.value.searchArtist.toString());
      query.append("searchAlbum", filter.value.searchAlbum.toString());
      if (filter.value.dateRangeFrom != null) query.append("dateRangeFrom", filter.value.dateRangeFrom.toString());
      if (filter.value.dateRangeTo != null) query.append("dateRangeTo", filter.value.dateRangeTo.toString());
      query.append("sortType", filter.value.sortType.toString());
      query.append("sortOrder", filter.value.sortOrder.toString());

      const response = await axios.get(`/api/track/gettracks?${query.toString()}`);

      numberOftracks.value = response.data.totalTracks
      pagination.value.rowsNumber = response.data.totalTracks;
      console.log(response.data);
     if(batchIndexes != null){
      const updatedBatches: TrackViewModelBatch[] = [];
      response.data.tracks.forEach(x =>{
        const batch = new TrackViewModelBatch();
        batch.batchIndex = x.batchIndex;
        x.trackViewModels.forEach((y: any) => {
          batch.trackViewModels.push(new TrackViewModel(y));
        });
        updatedBatches.push(batch);
      });
      trackBatches.value = updatedBatches;
      console.log(trackBatches.value);
     }
     else if (!trackBatches.value.map(x => x.batchIndex).includes(response.data.tracks[0].batchIndex)){
       const batch = new TrackViewModelBatch();
      batch.batchIndex = response.data.tracks[0].batchIndex;
      response.data.tracks[0].trackViewModels.forEach((x: any) => {
        batch.trackViewModels.push(new TrackViewModel(x));
      });
      if (addToTop) {
        if (trackBatches.value.length >= maximumNumberOfBatches.value) {
          trackBatches.value.pop();
        }
        trackBatches.value.unshift(batch);
      }
      else {
        if (trackBatches.value.length >= maximumNumberOfBatches.value) {
          trackBatches.value.shift();
        }
        trackBatches.value.push(batch);
      }
      console.log(batch);
     }

      statusCode.value = response.status;
    } catch (error) {
      const ex = error as AxiosError;
      statusCode.value = ex.status || null;
    }
  }

  async function onScroll({ from, to, index, ref }) {
    currentIndex.value = index;
    const nearTop = from <= 5;
    const nearBottom = to >= trackViewModels.value.length - 5;
    console.log(currentIndex.value);
    if (pagination.value.rowsNumber > trackLimit.value) {
      if (nearTop && Math.min(...trackBatches.value.map(x => x.batchIndex)) > 1 && statusCode.value != null) {
        console.log("Top of page reached", trackBatches.value);
        pageOffset.value = trackBatches.value.length > 0 ? Math.min(...trackBatches.value.map(x => x.batchIndex)) - 1 : 1;
        console.log(pageOffset.value);
        await getTracks(true);
        if (trackBatches.value[trackBatches.value.length - 1]?.batchIndex != Math.ceil(pagination.value.rowsNumber / trackLimit.value)) ref.scrollTo(currentIndex.value + trackBatches.value[trackBatches.value.length - 1].trackViewModels.length, 0);
      }
      if (nearBottom && Math.max(...trackBatches.value.map(x => x.batchIndex)) < Math.ceil(pagination.value.rowsNumber / trackLimit.value) && statusCode.value != null) {
        console.log("End of page reached", trackBatches.value);
        pageOffset.value = trackBatches.value.length > 0 ? Math.max(...trackBatches.value.map(x => x.batchIndex)) + 1 : 1;
        console.log(pageOffset.value);
        await getTracks();
        if (trackBatches.value[0]?.batchIndex != 1) ref.scrollTo(currentIndex.value - trackBatches.value[0].trackViewModels.length, 0);
      }
    }
  }

  async function onRequest(props){
    const {
      sortBy,
      page,
      rowsPerPage,
      descending
    } = props.pagination;

    switch (sortBy) {
      case "name":
        filter.value.sortType = SortType.Name;
        break;
      case "artists":
        filter.value.sortType = SortType.Artist;
        break;
      case "albumName":
        filter.value.sortType = SortType.Album;
        break;
      case "length":
        filter.value.sortType = SortType.TrackLength;
        break;
      case "dateSaved":
        filter.value.sortType = SortType.DateAdded;
        break;
      case "lastPlayed":
        filter.value.sortType = SortType.DateLastPlayed;
        break;
    }
    filter.value.sortOrder = descending ? SortOrder.Descending : SortOrder.Ascending;

    await getTracks(false, false, trackBatches.value.map(x => x.batchIndex));

    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
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

  function generateFilterQueryChip(){
    if(filter.value.searchName == filter.value.searchArtist && filter.value.searchArtist == filter.value.searchAlbum){
      return "(all)";
    }

    let text = "";

    if(filter.value.searchName) text += " (name)";
    if(filter.value.searchArtist) text += " (artist)";
    if(filter.value.searchAlbum) text += " (album)";

    return text;
  }

  function generateFilterSortChip(){
    let text = "";
    switch(parseInt(route.query.sort.toString())){
      case SortType.Name:
        text = "Name";
        break;
      case SortType.Artist:
        text = "Artist";
        break;
      case SortType.Album:
        text = "Album";
        break;
      case SortType.TrackLength:
        text = "Track length";
        break;
      case SortType.DateAdded:
        text = "Date added";
        break;
      case SortType.DateLastPlayed:
        text = "Date last played";
        break;
    }
    text += parseInt(route.query.order.toString()) == SortOrder.Ascending ? " (ascending)" : " (descending)";

    return text;
  }

</script>
<style lang="css">
  .my-table {
    border-collapse: separate;
    overflow: auto;

    thead tr th {
      position: sticky;
      top: 0;
      z-index: 4;
      background: white;
    }

    td:nth-child(1),
    th:nth-child(1) {
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
