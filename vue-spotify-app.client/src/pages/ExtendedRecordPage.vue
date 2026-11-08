<template>
    <div class="q-pa-md q-gutter-md">
        <div class="text-h4">Extended Records</div>
        <QTable
        :title="statusCode === 200 ? `Total records: ${pagination.rowsNumber}` : statusCode === null ? 'Loading...' : 'Failed to load records.'"
          style="height: 85vh"
          :rows="extendedRecords"
          :columns="columns"
          :table-row-class-fn="setRowClass"
          row-key="id"
          wrap-cells
          :loading="statusCode === null"
          v-model:pagination="pagination"
          hide-pagination
          >
          <!--Column for album cover-->
          <template v-slot:body-cell-albumCover="props">
            <q-td :props="props">
              <QImg :src="props.row.track.albumCover"
              :alt="`Album cover for ${props.row.track.albumName} by ${props.row.track.artists.map((x) => x.name).join(', ')}`"
              width="48px" />
            </q-td>
          </template>
          <!--Column for track name-->
          <template v-slot:body-cell-name="props">
            <q-td :props="props">
              <a :href="props.row.track.externalURL">{{ props.row.track.name }}</a>
            </q-td>
          </template>
          <!--Column for track artists-->
          <template v-slot:body-cell-artists="props">
            <q-td :props="props">
              <span v-for="y in props.row.track.artists" :key="y.id" :href="y.externalURL"><a :href="y.externalURL">{{ y.name }}</a><span v-if="props.row.track.artists.indexOf(y) < props.row.track.artists.length - 1">, </span></span>
            </q-td>
          </template>
          <!--Column for album name-->
          <template v-slot:body-cell-albumName="props">
            <q-td :props="props">
              <a :href="props.row.track.albumExternalURL">{{ props.row.track.albumName }}</a>
            </q-td>
          </template>
          <!--Column for the amount of time the track was played for-->
          <template v-slot:body-cell-amountPlayed="props">
            <q-td :props="props">
              {{ ConvertMilisecondsToMinutesAndSeconds(props.row.amountPlayed) }}
            </q-td>
          </template>
          <!--Column for the percentage of the track that has been played-->
          <template v-slot:body-cell-playbackPercentage="props">
            <q-td :props="props">
              {{ `${RoundAndTruncatePercentage(props.row.playbackPercentage, 2)}%` }}
            </q-td>
          </template>
          <!--Column for when track was recorded as played-->
          <template v-slot:body-cell-timestamp="props">
            <q-td :props="props">
              {{ date.formatDate(props.row.timestamp, "Do MMM YYYY HH:mm") }}
            </q-td>
          </template>
          <!--Column for track options-->
          <template v-slot:body-cell-actions="props">
            <q-td :props="props" align="center">
          <!--Button to open menu-->
          <QBtn flat dense icon="more_vert">
            <QMenu anchor="bottom left" self="top left">
                    <QList>
                      <!--Item to view the track in a new page-->
                      <QItem clickable v-close-popup :to="`viewtrack/${props.row.track.id}`">
                        <QItemSection>View track</QItemSection>
                      </QItem>
                      <!--Item to open dialog to add track to queue-->
                      <QItem clickable v-close-popup @click="openQueueDialog(props.row.track)">
                        <QItemSection>Add track to queue</QItemSection>
                      </QItem>
                    </QList>
                  </QMenu>
          </QBtn>
            </q-td>
          </template>
          <!--Shows pagination field for user to navigate across table pages-->
          <template v-slot:bottom>
            <QSpace />
            <QPagination v-model="pagination.page"
                        :max="pagination.rowsNumber > 0 ? Math.ceil(pagination.rowsNumber/pagination.rowsPerPage) : 1"
                        size="sm"
                        @update:model-value="getRecords()"
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
import type ExtendedRecordViewModel from '@/classes/extendedRecordViewModel';
import type TrackViewModel from '@/classes/trackViewModel';
import AddTrackToQueueDialog from '@/dialogs/addTrackToQueueDialog.vue';
import ConvertMilisecondsToMinutesAndSeconds from '@/helperFunctions/convertMilisecondsToMinutesAndSeconds';
import RoundAndTruncatePercentage from '@/helperFunctions/roundAndTruncatePercentage';
import axios from 'axios';
import { date, Dialog } from 'quasar';
import { onBeforeMount, ref } from 'vue';


const extendedRecords = ref<ExtendedRecordViewModel[]>([]);
const statusCode = ref<number | null>(null);

// Represents the columns to be displayed in the table.
  const columns = [
    // Shows the album cover of the track.
    {
      name: "albumCover",
      label: "",
      field: "track.albumCover",
      align: "left",
      sortable: false,
      style: "width: 50px"
    },
    // Shows the track's name.
    {
      name: "name",
      label: "Name",
      field: "track.name",
      align: "left",
      sortable: true,
      style: "width: 20%"
    },
    // Shows the artists credited for the track.
    {
      name: "artists",
      label: "Artist",
      field: "track.artists",
      align: "left",
      style: "width: 200px",
      sortable: true
    },
    // Shows the name of the album the track comes from.
    {
      name: "albumName",
      label: "Album",
      field: "track.albumName",
      align: "left",
      style: "width: 200px",
      sortable: true
    },
    // Shows the cause of a track playing.
    {
      name: "reasonStart",
      label: "Start cause",
      field: "reasonStart",
      align: "left",
      style: "width: 200px",
      sortable: true
    },
    // Shows the cause of a track stopping.
    {
      name: "reasonEnd",
      label: "End cause",
      field: "reasonEnd",
      align: "left",
      style: "width: 200px",
      sortable: true
    },
    // Shows how much of the track was recorded as played.
    {
      name: "amountPlayed",
      label: "Amount Played",
      field: "amountPlayed",
      align: "left",
      style: "width: 100px",
      sortable: true
    },
    // Shows the percentage of the track that was recorded as played.
    {
      name: "playbackPercentage",
      label: "Playback Percentage",
      field: "playbackPercentage",
      align: "left",
      style: "width: 150px",
      sortable: true
    },
    // Shows the timestamp of when the track was played.
    {
      name: "timestamp",
      label: "Timestamp",
      field: "timestamp",
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
    rowsNumber: 0
  });

  onBeforeMount(async () => {
    await getRecords();
  });

  async function getRecords(){
    statusCode.value = null;
    try{
      const response = await axios.get(`/api/extendedrecord/getrecords?offset=${(pagination.value.page - 1) * pagination.value.rowsPerPage}&numberOfRecords=${pagination.value.rowsPerPage}`);

      extendedRecords.value = response.data.records as ExtendedRecordViewModel[];
      pagination.value.rowsNumber = response.data.totalCount;
      statusCode.value = response.status;
    }
    catch(ex){
      if (axios.isAxiosError(ex) && ex.response) {
        statusCode.value = ex.response.status;
      }
      else{
        statusCode.value = 500;
      }
    }
  }

    function openQueueDialog(track: TrackViewModel) {
    Dialog.create({
      component: AddTrackToQueueDialog,
      componentProps: {
        tracks: [track]
      }
    });
  }

  function setRowClass(row){
    if(row.playbackPercentage >= 125){
      return "bg-indigo-4 text-black"
    }
    if(row.playbackPercentage >= 65){
      return "bg-green-4 text-black"
    }
    else if(row.playbackPercentage >= 30){
      return "bg-yellow-4 text-black"
    }
    else{
      return "bg-red-4 text-black"
    }
  }
</script>
