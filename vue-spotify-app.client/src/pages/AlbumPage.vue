<template>
  <div class="text-h4">{{ album?.name }}</div>
  <QTable class="my-table"
          style="height: 85vh"
          :rows="tracks"
          :columns="columns"
          row-key="id"
          wrap-cells
          v-model:pagination="pagination">
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
                   :max="Math.ceil(pagination.rowsNumber/pagination.rowsPerPage)"
                   size="sm"
                   input />
    </template>
  </QTable>
</template>
<script setup lang="ts">

    import { onBeforeMount, ref } from "vue";
    import AlbumViewModel from "../classes/albumViewModel";
    import TrackViewModel from "../classes/trackViewModel";
import { useRoute } from "vue-router";
import axios from "axios";
import { date, Dialog, Notify } from "quasar";
import ConvertMilisecondsToMinutesAndSeconds from "@/helperFunctions/convertMilisecondsToMinutesAndSeconds";
  import AddTrackToQueueDialog from '@/dialogs/addTrackToQueueDialog.vue';
import { useTitle } from "@vueuse/core";

    const route = useRoute();
    const title = useTitle();

    const album = ref<AlbumViewModel | null>(null);
    const tracks = ref<TrackViewModel[]>([]);

      // Represents the columns to be displayed in the table.
  const columns = [
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
    // Shows the track's length in minutes and seconds.
    {
      name: "length",
      label: "Length",
      field: "length",
      align: "left",
      style: "width: 100px",
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
        title.value = `${album.value.name} - ${album.value.artists.map((x) => x.name).join(", ")}`;
    }
    catch (error) {
        console.error("Error fetching album data:", error);
    }
  });

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
