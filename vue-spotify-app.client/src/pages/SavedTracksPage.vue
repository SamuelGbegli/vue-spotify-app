<template>
  <div class="q-pa-md">

    <div class="text-h4">Saved Tracks</div>
  <QTable style="height: 85vh"
          :rows="trackViewModels"
          :columns="tableColumns"
          row-key="id"
          wrap-cells
          :loading="statusCode === null"
          v-model:pagination="pagination"
          hide-pagination>
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
    <template v-slot:body-cell-albumName="props">
      <QTd :props="props">
        <div class="text-left">
          <a :href="props.row.albumExternalURL">{{ props.row.albumName }}</a>
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
    <!--Shows pagination field for user to navigate across table pages-->
    <template v-slot:bottom>
        <QBtn flat label="Get 10 Saved Tracks" color="primary" @click="selectSavedTracks" />
      <QSpace />
      <QPagination v-model="pagination.page"
                   :max="Math.ceil(pagination.rowsNumber / pagination.rowsPerPage)"
                   size="sm"
                   @update:model-value="getTracks()"
                   input />
    </template>
    <!--Shows loading spinner when table is loading-->
    <template v-slot:loading>
      <QInnerLoading showing size="50px" color="green" />
    </template>
  </QTable>
  </div>
  
</template>
<script setup lang="ts">

  import { ref, onBeforeMount } from 'vue';
  import ConvertMilisecondsToMinutesAndSeconds from '@/helperFunctions/convertMilisecondsToMinutesAndSeconds';
  import axios, { AxiosError } from 'axios';
  import { date } from 'quasar';
  import type TrackViewModel from '@/classes/trackViewModel';

  const statusCode = ref<number | null>(null);
  const pagination = ref({
    page: 1,
    rowsPerPage: 100,
    rowsNumber: 0,
    //sortBy: 'name',
    descending: false
  });

  const trackViewModels = ref<TrackViewModel[]>([]);

  // Represents the columns to be displayed in the table.
  const tableColumns = [
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

  onBeforeMount(async () => {
    await getTracks();
  });

  async function getTracks() {
    statusCode.value = null;
    try {
      const response = await axios.get(`/api/savedtrack/getsavedtracks?offset=${(pagination.value.page - 1)}&count=${pagination.value.rowsPerPage}`);
      console.log(response.data);
      trackViewModels.value = response.data.tracks as TrackViewModel[];
      pagination.value.rowsNumber = response.data.totalTracks;
      statusCode.value = response.status;
      console.log(pagination.value);
    } catch (error) {
      statusCode.value = (error as AxiosError).response?.status;
      console.error(error);
      alert("Error fetching saved tracks. Please check the console for details.");
    }
  }
  async function selectSavedTracks() {
    try {
      const response = await axios.post("/api/savedtrack/getrandomsavedtracks");
      console.log(response.data);
    } catch (error) {
      console.error(error);
      alert("Error fetching saved tracks. Please check the console for details.");
    }
  }
</script>
