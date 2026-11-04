<template>
  <div class="q-pa-md" v-if="albumStatusCode !== 404">
    <div class="row q-gutter-md q-mb-md items-center">
    <div>
      <QImg v-if="albumStatusCode === 200" :src="album?.imageURL" :alt="album?.name" fit="scale-down" width="250px" />
      <QSkeleton v-else-if="albumStatusCode === null" width="250px" height="250px" square/>
    </div>
    <div class="col" v-if="albumStatusCode === 200">
      <div class="text-h4">{{ album?.name }}<QBtn :href="album?.externalURL" title="View on Spotify" icon="link" flat round size="md" /></div>
      <div class="text-subtitle1">{{ album?.albumType }} by {{ album?.artists.map((x) => x.name).join(", ") }}</div>
      <div class="text-subtitle1">Release: {{ album?.releaseDate }}</div>
      <div class="text-subtitle1">Number of tracks: {{ album?.numberOfTracks }}</div>
      <a :href="album?.externalURL" target="_blank">View on Spotify</a>
    </div>
    <div class="col" v-else-if="albumStatusCode === null">
      <QSkeleton type="text" class="text-h4" width="20%" />
      <QSkeleton type="text" class="text-subtitle1" width="20%" />
      <QSkeleton type="text" class="text-subtitle1" width="10%" />
      <QSkeleton type="text" class="text-subtitle1" width="10%" />
      <QSkeleton type="text" width="10%" />
      </div>
  </div>
  <QTable v-if="albumStatusCode === 200"
          :title="`Tracks: ${album?.numberOfTracks}`"
          style="max-height: 85vh"
          :rows="tracks"
          :columns="columns"
          row-key="id"
          :loading="tracksStatusCode === null"
          wrap-cells
          v-model:pagination="pagination">
          <template v-slot:body-cell-index="props">
            <q-td :props="props">
              <span>{{ props.row.index }}</span>
            </q-td>
          </template>
    <template v-slot:body-cell-name="props">
      <QTd :props="props">
        <div>
          <a :href="props.row.externalURL">{{ props.row.name }}</a>
        </div>
      </QTd>
    </template>
    <template v-slot:body-cell-artists="props">
      <QTd :props="props">
        <div class="text-left">
          <span v-for="x in props.row.artists" :key="x.id" :href="x.externalURL"><a :href="x.externalURL">{{ x.name }}</a><span v-if="props.row.artists.indexOf(x) < props.row.artists.length - 1">, </span></span>
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
                <QItem clickable v-close-popup @click="openQueueDialog(props.row)">
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
    <template v-slot:bottom>
      <QSpace />
      <QPagination v-model="pagination.page"
                  @update:model-value="getTracks()"
                   :max="Math.ceil(pagination.rowsNumber/pagination.rowsPerPage)"
                   size="sm"
                   input />
    </template>
    <template v-slot:loading>
      <QInnerLoading showing size="50px" color="green" />
    </template>
  </QTable>
  <QSkeleton v-else-if="albumStatusCode === null" width="100%" height="600px" square />
  </div>
  <div v-else-if="albumStatusCode === 404" class="q-pa-md">
    <div class="text-h4">Album not found</div>
    <div class="text-subtitle1">The album you are looking for does not exist or has been removed.</div>
  </div>
</template>
<script setup lang="ts">

    import { onBeforeMount, ref } from "vue";
    import AlbumViewModel from "../classes/albumViewModel";
    import TrackViewModel from "../classes/trackViewModel";
import { useRoute } from "vue-router";
import axios, { AxiosError } from "axios";
import { date, Dialog, Notify } from "quasar";
import ConvertMilisecondsToMinutesAndSeconds from "@/helperFunctions/convertMilisecondsToMinutesAndSeconds";
  import AddTrackToQueueDialog from '@/dialogs/addTrackToQueueDialog.vue';
import { useTitle } from "@vueuse/core";

    const route = useRoute();
    const title = useTitle();

    const album = ref<AlbumViewModel | null>(null);
    const tracks = ref<TrackViewModel[]>([]);

    const albumStatusCode = ref<number | null>(null);
    const tracksStatusCode = ref<number | null>(null);

      // Represents the columns to be displayed in the table.
  const columns = [
    {
      name: "index",
      label: "#",
      field: "index",
      align: "left",
      sortable: false,
      style: "width: 50px"
    },
    // Shows the track's name.
    {
      name: "name",
      label: "Name",
      field: "name",
      align: "left",
      sortable: false,
      style: "width: 20%"
    },
    // Shows the artists credited for the track.
    {
      name: "artists",
      label: "Artist",
      field: "artists",
      align: "left",
      style: "width: 200px",
      sortable: false
    },
    // Shows the track's length in minutes and seconds.
    {
      name: "length",
      label: "Length",
      field: "length",
      align: "left",
      style: "width: 100px",
      sortable: false
    },
    // Shows the date the track was last recorded as played, if applicable.
    {
      name: "lastPlayed",
      label: "Last play date",
      field: "dateLastPlayed",
      align: "left",
      style: "width: 200px",
      sortable: false
    },
    // Shows whether the track is in the user's Liked Songs library.
    // Only displayed when viewing a playlist.
    {
      name: "inLikedSongs",
      label: "In Liked Songs?",
      field: "isInLikedSongs",
      align: "left",
      style: "width: 150px",
      sortable: false
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

    // Stores pagination values for the table.
  const pagination = ref({
    page: 1,
    rowsPerPage: 50,
    rowsNumber: 0,
    descending: false
  });

  onBeforeMount(async () => {
    const albumId = route.params.id as string;
    try{
        const response = await axios.get(`/api/album/getalbumwithtracks/${albumId}`);
        album.value = response.data.album as AlbumViewModel;
        tracks.value = response.data.tracks as TrackViewModel[];
        pagination.value.rowsNumber = album.value.numberOfTracks;
        title.value = `${album.value.name} - ${album.value.albumType} by ${album.value.artists.map((x) => x.name).join(", ")}`;
        albumStatusCode.value = response.status;
        tracksStatusCode.value = response.status;
    }
    catch (error) {
        console.error("Error fetching album data:", error);
        albumStatusCode.value = (error as AxiosError).response?.status || 500;
    }
  });

  async function getTracks(){
    try{
      tracksStatusCode.value = null;
      console.log(tracksStatusCode.value);
      const albumId = route.params.id as string;
      const response = await axios.get(`/api/album/gettracks?albumId=${albumId}&offset=${(pagination.value.page - 1) * pagination.value.rowsPerPage}`);
      tracks.value = response.data as TrackViewModel[];
      tracksStatusCode.value = response.status;
    }
    catch (error) {
      console.error("Error fetching tracks:", error);
      tracksStatusCode.value = (error as AxiosError).response?.status || 500;
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

  function openQueueDialog(track: TrackViewModel) {
    Dialog.create({
      component: AddTrackToQueueDialog,
      componentProps: {
        tracks: [track]
      }
    });
  }

</script>
