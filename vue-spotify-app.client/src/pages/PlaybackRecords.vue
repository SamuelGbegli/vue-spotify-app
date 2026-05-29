<template>
  <div class="col q-pa-md">
    <h4 class="q-ma-sm">Playback records</h4>
    <div class="row items-center">
      <h6 class="q-ma-sm">
        {{
          startDate && endDate
            ? `From ${date.formatDate(startDate, "Do MMM YYYY")} to ${date.formatDate(endDate, "Do MMM YYYY")}`
            : startDate
              ? `From ${date.formatDate(startDate, "Do MMM YYYY")} onwards`
              : endDate
                ? `Up to ${date.formatDate(endDate, "Do MMM YYYY")}`
                : ''
        }}
      </h6>
      <QSpace />
      <QBtn label="Set dates" @click="openFilterDialog()" :disable="statusCode == null || groupStatusCode == null" />
    </div>
    <!--<QBtn label="Update records" @click="updateRecords()" />-->
    <QTabs v-model="selectedTab">
      <QTab name="individual" label="Individual" />
      <QTab name="grouped" label="Grouped" />
    </QTabs>
    <QTabPanels v-model="selectedTab">
      <QTabPanel name="individual">
         <QTable
          style="height: 72vh"
          :rows="playbackRecords"
          :columns="individualColumns"
          row-key="id"
          wrap-cells
          :loading="statusCode === null"
          v-model:pagination="individualPagination"
          hide-pagination
          binary-sort-order
          @request="onIndividualPageChange"
          >
          <template v-slot:body-cell-albumCover="props">
            <q-td :props="props">
              <QImg :src="props.row.albumCover"
              :alt="`Album cover for ${props.row.albumName} by ${props.row.artists.map((x) => x.name).join(', ')}`"
              width="48px" />
            </q-td>
          </template>
          <template v-slot:body-cell-name="props">
            <q-td :props="props">
              <a :href="props.row.trackUrl">{{ props.row.name }}</a>
            </q-td>
          </template>
          <template v-slot:body-cell-artists="props">
            <q-td :props="props">
              <span v-for="y in props.row.artists" :key="y.id" :href="y.externalUrl"><a :href="y.externalUrl">{{ y.name }}</a><span v-if="props.row.artists.indexOf(y) < props.row.artists.length - 1">, </span></span>
            </q-td>
          </template>
          <template v-slot:body-cell-albumName="props">
            <q-td :props="props">
              <a :href="props.row.albumLink">{{ props.row.albumName }}</a>
            </q-td>
          </template>
          <template v-slot:body-cell-datePlayed="props">
            <q-td :props="props">
              {{ date.formatDate(props.row.datePlayed, "Do MMM YYYY HH:mm") }}
            </q-td>
          </template>
          <template v-slot:body-cell-actions="props">
            <q-td :props="props" align="center">
          <QBtn flat dense icon="more_vert">
            <QMenu anchor="bottom left" self="top left">
                    <QList>
                      <QItem clickable v-close-popup :to="`viewtrack/${props.row.spotifyID}`">
                        <QItemSection>View track</QItemSection>
                      </QItem>
                      <QItem clickable v-close-popup @click="openQueueDialog(props.row.spotifyID, props.row.name)">
                        <QItemSection>Add track to queue</QItemSection>
                      </QItem>
                    </QList>
                  </QMenu>
          </QBtn>
            </q-td>
          </template>
          <template v-slot:bottom>
            <QSpace />
            <QPagination v-model="currentPage"
                        :max="totalPages"
                        size="sm"
                        @update:model-value="getRecords()"
                        input />
          </template>
          <template v-slot:loading>
            <QInnerLoading showing size="50px" color="green" />
          </template>
          </QTable>
      </QTabPanel>

      <QTabPanel name="grouped">
        <QTable
          style="height: 72vh"
          :rows="groupedPlaybackRecords"
          :columns="groupColumns"
          row-key="id"
          wrap-cells
          :loading="groupStatusCode === null"
          v-model:pagination="groupPagination"
          hide-pagination
          binary-sort-order
          @request="onIndividualPageChange"
          >
          <template v-slot:body-cell-albumCover="props">
            <q-td :props="props">
              <QImg :src="props.row.albumCover"
              :alt="`Album cover for ${props.row.albumName} by ${props.row.artists.map((x) => x.name).join(', ')}`"
              width="48px" />
            </q-td>
          </template>
          <template v-slot:body-cell-name="props">
            <q-td :props="props">
              <a :href="props.row.externalUrl">{{ props.row.name }}</a>
            </q-td>
          </template>
          <template v-slot:body-cell-artists="props">
            <q-td :props="props">
              <span v-for="y in props.row.artists" :key="y.id" :href="y.externalUrl"><a :href="y.externalUrl">{{ y.name }}</a><span v-if="props.row.artists.indexOf(y) < props.row.artists.length - 1">, </span></span>
            </q-td>
          </template>
          <template v-slot:body-cell-albumName="props">
            <q-td :props="props">
              <a :href="props.row.albumExternalUrl">{{ props.row.albumName }}</a>
            </q-td>
          </template>
          <template v-slot:body-cell-actions="props">
            <q-td :props="props" align="center">
          <QBtn flat dense icon="more_vert">
            <QMenu anchor="bottom left" self="top left">
                    <QList>
                      <QItem clickable v-close-popup :to="`viewtrack/${props.row.spotifyID}`">
                        <QItemSection>View track</QItemSection>
                      </QItem>
                      <QItem clickable v-close-popup @click="openQueueDialog(props.row.spotifyID, props.row.name)">
                        <QItemSection>Add track to queue</QItemSection>
                      </QItem>
                    </QList>
                  </QMenu>
          </QBtn>
            </q-td>
          </template>
          <template v-slot:bottom>
            <QSpace />
            <QPagination v-model="currentGroupPage"
                        :max="totalGroupPages"
                        size="sm"
                        @update:model-value="getGroupedRecords()"
                        input />
          </template>
          <template v-slot:loading>
            <QInnerLoading showing size="50px" color="green" />
          </template>
          </QTable>
      </QTabPanel>
    </QTabPanels>

  </div>
</template>
<script setup lang="ts">
  import PlaybackRecordViewModel from '@/classes/playbackRecordViewModel';
  import TrackViewModel from '@/classes/trackViewModel';
  import AddTrackToQueueDialog from '@/dialogs/addTrackToQueueDialog.vue';
  import PlaybackRecordFilterDialog from '@/dialogs/playbackRecordFilterDialog.vue';
  import { useAuthStore } from '@/stores/authStore';
  import axios, { AxiosError } from 'axios';
  import { Dialog, Loading, date } from 'quasar';
  import { onBeforeMount, ref } from 'vue';


  const authStore = useAuthStore();

  const startDate = ref<string | null>(null);
  const endDate = ref<string | null>(null);

  // Stores records fetched from the database
  const playbackRecords = ref<PlaybackRecordViewModel[]>([]);

  //
  const groupedPlaybackRecords = ref<TrackViewModel[]>([]);

  // Stores the current page of records
  const currentPage = ref(1);

  // Stores the number of pages the use can browse
  const totalPages = ref(0);

  // Stores the status code returned from the API call
  const statusCode = ref<number | null>(null)

  const currentGroupPage = ref(1);
  const totalGroupPages = ref(0)
  const groupStatusCode = ref<number | null>(null);

  // Sets the tab visible in the page
  const selectedTab = ref("individual");

    const individualColumns = [
    {
      name: "albumCover",
      label: "",
      field: "albumCover",
      align: "left",
      sortable: false,
      style: "width: auto"
    },
    {
      name: "name",
      label: "Name",
      field: "name",
      align: "left",
      sortable: false,
      style: "width: 25%"
    },
    {
      name: "artists",
      label: "Artist(s)",
      field: "artists",
      align: "left",
      sortable: false,
      style: "width: 25%"
    },
    {
      name: "albumName",
      label: "Album",
      field: "albumName",
      align: "left",
      sortable: false,
      style: "width: 25%"
    },
    {
      name: "datePlayed",
      label: "Date recorded",
      field: "datePlayed",
      align: "left",
      sortable: true,
      style: "width: auto"
    },
    {
      name: "actions",
      label: "",
      field: "actions",
      align: "center",
      sortable: false,
      style: "width: auto"
    }
  ];

    const groupColumns = [
    {
      name: "albumCover",
      label: "",
      field: "albumCover",
      align: "left",
      sortable: false,
      style: "width: auto"
    },
    {
      name: "name",
      label: "Name",
      field: "name",
      align: "left",
      sortable: false,
      style: "width: 25%"
    },
    {
      name: "artists",
      label: "Artist(s)",
      field: "artists",
      align: "left",
      sortable: false,
      style: "width: 25%"
    },
    {
      name: "albumName",
      label: "Album",
      field: "albumName",
      align: "left",
      sortable: false,
      style: "width: 25%"
    },
    {
      name: "totalRecordedPlays",
      label: "Total recorded plays",
      field: "numberOfFoundRecords",
      align: "left",
      sortable: true,
      style: "width: auto"
    },
    {
      name: "actions",
      label: "",
      field: "actions",
      align: "center",
      sortable: false,
      style: "width: auto"
    }
  ];

  const individualPagination = ref({
    sortBy: "datePlayed",
    descending: true,
    page: 1,
    rowsPerPage: 50,
    rowsNumber: 100
  });


  const groupPagination = ref({
    descending: true,
    page: 1,
    rowsPerPage: 50,
    rowsNumber: 100
  });

  onBeforeMount(async () => {
    await getRecords();
    await getGroupedRecords();
  })

  async function updateRecords() {
    try {
      Loading.show({
        message: "Updating records..."
      });
      await axios.get("playbackrecord/updaterecords", {
        headers: {
          authToken: authStore.accessToken
        }
      });
      alert("Successfully updated database.");
    }
    catch (ex) {
      const error = ex as AxiosError;
      console.log(error)
      alert("An error has occured.");
    }
    finally {
      Loading.hide();
    }
  }

  async function getRecords() {
    statusCode.value = null;

    const searchParams = new URLSearchParams();
    searchParams.append("offset", ((currentPage.value)).toString());
    searchParams.append("numberofrecords", "50");
    if (startDate.value !== null) searchParams.append("startDate", startDate.value);
    if (endDate.value !== null) searchParams.append("endDate", endDate.value);

    try {
      const response = await axios.get(
        `playbackrecord/getrecords?${searchParams}`,
      );
      console.log(response.data);
      playbackRecords.value = [];
      response.data.records.forEach(element => {
        const viewModel = new PlaybackRecordViewModel();
        viewModel.initialiseData(element);
        playbackRecords.value.push(viewModel);
      });
      individualPagination.value.rowsNumber = response.data.totalRecords;
      individualPagination.value.page = currentPage.value;
      totalPages.value = Math.ceil(response.data.totalRecords / 50);
      statusCode.value = response.status;
    }
    catch (ex) {
      const error = ex as AxiosError;
      statusCode.value = error.status;
      console.log(error);
    }
  }

  async function onIndividualPageChange(props) {
    console.log(props);

    const { page, rowsPerPage } = props.pagination;
    console.log(page, rowsPerPage);
    currentPage.value = page;
    await getRecords();
  }

  async function getGroupedRecords() {
    groupStatusCode.value = null;

    const searchParams = new URLSearchParams();
    searchParams.append("offset", ((currentGroupPage.value)).toString());
    searchParams.append("numberofrecords", "50");
    if (startDate.value !== null) searchParams.append("startDate", startDate.value);
    if (endDate.value !== null) searchParams.append("endDate", endDate.value);


    try {
      const response = await axios.get(`playbackrecord/getTrackFoundRecords?${searchParams}`, {
        headers: {
          authToken: authStore.accessToken
        }
      });
      groupedPlaybackRecords.value = [];
      response.data.records.forEach(element => {
        const viewModel = new TrackViewModel(element);
        groupedPlaybackRecords.value.push(viewModel);
      });

      groupPagination.value.rowsNumber = response.data.totalRecords;
      groupPagination.value.page = currentPage.value;
      totalGroupPages.value = (Math.ceil(response.data.totalRecords / 50));
      groupStatusCode.value = response.status;

    } catch (ex) {
      const error = ex as AxiosError;
      groupStatusCode.value = error.status;
      console.log(error);
    }
  }

  function openFilterDialog() {
    Dialog.create({
      component: PlaybackRecordFilterDialog,
      componentProps: {
        startDateProp: startDate.value,
        endDateProp: endDate.value
      }
    }).onOk(async (data) => {
      startDate.value = data.startDate;
      endDate.value = data.endDate;
      currentPage.value = 1;
      currentGroupPage.value = 1;
      await getRecords();
      await getGroupedRecords();
    });
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

</script>
