<template>
  <div class="col q-pa-md">
  <!--Title-->
    <div class="text-h4">Playback records</div>
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
      <!--Chip to show the date range if either start or end date is set-->
      <QChip v-if="startDate || endDate" >
        {{
          startDate && endDate
            ? `Between ${date.formatDate(startDate, "Do MMM YYYY")} and ${date.formatDate(endDate, "Do MMM YYYY")}`
            : startDate
              ? `From ${date.formatDate(startDate, "Do MMM YYYY")}`
              : endDate
                ? `Up to ${date.formatDate(endDate, "Do MMM YYYY")}`
                : ''
         }}
      </QChip>
      <QSpace />
      <!--Button to open filter dialog to set start and end date ranges-->
      <QBtn label="Set dates" @click="openFilterDialog()" :disable="statusCode == null || groupStatusCode == null" />
    </div>
    <!--<QBtn label="Update records" @click="updateRecords()" />-->
    <!--Tabs to switch between individual and grouped records-->
    <QTabs v-model="selectedTab">
      <QTab name="individual" label="Individual" />
      <QTab name="grouped" label="Grouped" />
    </QTabs>
    <QTabPanels v-model="selectedTab">
    <!--Tab section for individual playback records-->
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
          >
          <!--Column for album cover-->
          <template v-slot:body-cell-albumCover="props">
            <q-td :props="props">
              <QImg :src="props.row.albumCover"
              :alt="`Album cover for ${props.row.albumName} by ${props.row.artists.map((x) => x.name).join(', ')}`"
              width="48px" />
            </q-td>
          </template>
          <!--Column for track name-->
          <template v-slot:body-cell-name="props">
            <q-td :props="props">
              <a :href="props.row.trackUrl">{{ props.row.name }}</a>
            </q-td>
          </template>
          <!--Column for track artists-->
          <template v-slot:body-cell-artists="props">
            <q-td :props="props">
              <span v-for="y in props.row.artists" :key="y.id" :href="y.externalURL"><a :href="y.externalURL">{{ y.name }}</a><span v-if="props.row.artists.indexOf(y) < props.row.artists.length - 1">, </span></span>
            </q-td>
          </template>
          <!--Column for album name-->
          <template v-slot:body-cell-albumName="props">
            <q-td :props="props">
              <a :href="props.row.albumLink">{{ props.row.albumName }}</a>
            </q-td>
          </template>
          <!--Column for when track was recorded as played-->
          <template v-slot:body-cell-datePlayed="props">
            <q-td :props="props">
              {{ date.formatDate(props.row.datePlayed, "Do MMM YYYY HH:mm") }}
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
                      <QItem clickable v-close-popup :to="`viewtrack/${props.row.spotifyID}`">
                        <QItemSection>View track</QItemSection>
                      </QItem>
                      <!--Item to open dialog to add track to queue-->
                      <QItem clickable v-close-popup @click="openQueueDialog(props.row.spotifyID, props.row.name)">
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
            <QPagination v-model="currentPage"
                        :max="totalPages"
                        size="sm"
                        @update:model-value="getRecords()"
                        input />
          </template>
          <!--Shows loading spinner when table is loading-->
          <template v-slot:loading>
            <QInnerLoading showing size="50px" color="green" />
          </template>
          </QTable>
      </QTabPanel>

    <!--Tab section for grouped playback records-->
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
          >
          <!--Column for album cover-->
          <template v-slot:body-cell-albumCover="props">
            <q-td :props="props">
              <QImg :src="props.row.albumCover"
              :alt="`Album cover for ${props.row.albumName} by ${props.row.artists.map((x) => x.name).join(', ')}`"
              width="48px" />
            </q-td>
          </template>
          <!--Column for track name-->
          <template v-slot:body-cell-name="props">
            <q-td :props="props">
              <a :href="props.row.externalURL">{{ props.row.name }}</a>
            </q-td>
          </template>
          <!--Column for track artists-->
          <template v-slot:body-cell-artists="props">
            <q-td :props="props">
              <span v-for="y in props.row.artists" :key="y.id" :href="y.externalURL"><a :href="y.externalURL">{{ y.name }}</a><span v-if="props.row.artists.indexOf(y) < props.row.artists.length - 1">, </span></span>
            </q-td>
          </template>
          <!--Column for album name-->
          <template v-slot:body-cell-albumName="props">
            <q-td :props="props">
              <a :href="props.row.albumExternalURL">{{ props.row.albumName }}</a>
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
                      <QItem clickable v-close-popup :to="`viewtrack/${props.row.id}`">
                        <QItemSection>View track</QItemSection>
                      </QItem>
                      <!--Item to open dialog to add track to queue-->
                      <QItem clickable v-close-popup @click="openQueueDialog(props.row.id, props.row.name)">
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
            <QPagination v-model="currentGroupPage"
                        :max="totalGroupPages"
                        size="sm"
                        @update:model-value="getGroupedRecords()"
                        input />
          </template>
          <!--Shows loading spinner when table is loading-->
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
  import { onBeforeMount, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';


  const authStore = useAuthStore();

  const route  = useRoute();
  const router = useRouter();

  // The earliest date of playback records that will be returned. If null, goes back to the earliest known record.
  const startDate = ref<string | null>(null);
  // The latest date of playback records that will be returned. If null, goes up to the latest known record.
  const endDate = ref<string | null>(null);

  // Stores records fetched from the database
  const playbackRecords = ref<PlaybackRecordViewModel[]>([]);

  // Stores each unique track with a playback record, plus the number of times a play was recorded.
  const groupedPlaybackRecords = ref<TrackViewModel[]>([]);

  // Stores the current page of individual records
  const currentPage = ref(1);

  // Stores the number of pages the use can browse for individual records
  const totalPages = ref(0);

  // Stores the status code for individual records returned from the API call
  const statusCode = ref<number | null>(null)

  // Stores the current page of grouped records
  const currentGroupPage = ref(1);

  // Stores the number of pages the user can browse for grouped records
  const totalGroupPages = ref(0)

  // Stores the status code for grouped records returned from the API call
  const groupStatusCode = ref<number | null>(null);

  // Sets the tab visible in the page
  const selectedTab = ref("individual");

  // Table columns for individual playback records
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

  // Table columns for grouped playback records
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

  // Pagination values for individual playback records
  const individualPagination = ref({
    sortBy: "datePlayed",
    descending: true,
    page: 1,
    rowsPerPage: 50,
    rowsNumber: 100
  });

  // Pagination values for grouped playback records
  const groupPagination = ref({
    descending: true,
    page: 1,
    rowsPerPage: 50,
    rowsNumber: 100
  });

  onBeforeMount(async () => {
    onRouteUpdate();
    await getRecords();
    await getGroupedRecords();
  })

  watch(route, async () => {
    onRouteUpdate();
  });

  // Sets the start and end date values, plus page values, when the route is updated
  function onRouteUpdate() {
    // Sets the start date if present in the URL query
    if(route.query.startDate) startDate.value = route.query.startDate as string;
    // Sets the end date if present in the URL query
    if(route.query.endDate) endDate.value = route.query.endDate as string;
    // Sets the individual page value if present in the URL query
    if(route.query.page) currentPage.value = parseInt(route.query.page as string);
    // Sets the group page value if present in the URL query
    if(route.query.groupPage) currentGroupPage.value = parseInt(route.query.groupPage as string);
    updateAddressBar();
  }

  // Changes the address bar to reflect the current start and end date values, plus page values
  function updateAddressBar(){
    // Sets up the URL query parameters
    const query = new URLSearchParams();
    // Adds start date to query if it is set
    if(startDate.value) query.append("startDate", startDate.value.toString());
    // Adds end date to query if it is set
    if(endDate.value) query.append("endDate", endDate.value.toString());
    // Adds individual page to query
    query.append("page", currentPage.value.toString());
    // Adds group page to query
    query.append("groupPage", currentGroupPage.value.toString());
    // Pushes the new query to the router, updating the address bar
    router.push(`playbackrecords?${query.toString()}`);
  }

  async function updateRecords() {
    try {
      Loading.show({
        message: "Updating records..."
      });
      await axios.get("/api/playbackrecord/updaterecords", {
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

  // Function to get individual playback records from the API
  async function getRecords() {

    // Updates address bar to reflect the current start and end date values, plus page values
    updateAddressBar();

    // Sets status code to null to show loading spinner in table
    statusCode.value = null;

    // Sets up the search parameters for the API call
    const searchParams = new URLSearchParams();
    // Adds the current page to the search parameters
    searchParams.append("offset", ((currentPage.value)).toString());
    // Adds the number of records to return to the search parameters
    searchParams.append("numberofrecords", "50");
    // Adds the start date to the search parameters if it is set
    if (startDate.value !== null) searchParams.append("startDate", startDate.value);
    // Adds the end date to the search parameters if it is set
    if (endDate.value !== null) searchParams.append("endDate", endDate.value);

    try {
      // Makes call to the API
      const response = await axios.get(
        `/api/playbackrecord/getrecords?${searchParams}`,
      );

      // Clears the playback records array
      playbackRecords.value = [];

      // Creates a view model for each data element returned from the API, which are pushed to the array
      response.data.records.forEach(element => {
        const viewModel = new PlaybackRecordViewModel();
        viewModel.initialiseData(element);
        playbackRecords.value.push(viewModel);
      });

      // Sets the total number of records and current page for the table
      individualPagination.value.rowsNumber = response.data.totalRecords;
      individualPagination.value.page = currentPage.value;

      // Sets the maximum page number for the table
      totalPages.value = Math.ceil(response.data.totalRecords / 50);
      // Sets the status code to the response status to hide the loading spinner
      statusCode.value = response.status;
    }
    catch (ex) {
      // Section if something goes wrong

      const error = ex as AxiosError;
      // Sets error status code to hide loading spinner
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

  // Function to get tracks and the number of times they were recorded as played
  async function getGroupedRecords() {

    // Function to get individual playback records from the API
    updateAddressBar();

    // Sets status code to null to show loading spinner in table
    groupStatusCode.value = null;

    // Sets up the search parameters for the API call
    const searchParams = new URLSearchParams();
    // Adds the current page to the search parameters
    searchParams.append("offset", ((currentGroupPage.value)).toString());
    // Adds the number of records to return to the search parameters
    searchParams.append("numberofrecords", "50");
    // Adds the start date to the search parameters if it is set
    if (startDate.value !== null) searchParams.append("startDate", startDate.value);
    // Adds the end date to the search parameters if it is set
    if (endDate.value !== null) searchParams.append("endDate", endDate.value);

    try {
      // Makes call to the API
      const response = await axios.get(`/api/playbackrecord/getTrackFoundRecords?${searchParams}`, {
        headers: {
          authToken: authStore.accessToken
        }
      });

      // Clears the grouped playback records array
      groupedPlaybackRecords.value = [];
      // Creates a view model for each data element returned from the API, which are pushed to the array
      response.data.records.forEach(element => {
        const viewModel = new TrackViewModel(element);
        groupedPlaybackRecords.value.push(viewModel);
      });

      // Sets the total number of records and current page for the table
      groupPagination.value.rowsNumber = response.data.totalRecords;
      groupPagination.value.page = currentPage.value;

      // Sets the maximum page number for the table
      totalGroupPages.value = (Math.ceil(response.data.totalRecords / 50));
      // Sets the status code to the response status to hide the loading spinner
      groupStatusCode.value = response.status;

    } catch (ex) {
      // Section if something goes wrong
      const error = ex as AxiosError;

      // Sets error status code to hide loading spinner
      groupStatusCode.value = error.status;
      console.log(error);
    }
  }

  // Function to open a dialog to set the start and end date ranges for playback records
  function openFilterDialog() {
    // Creates dialog with the PlaybackRecordFilterDialog component, with the current start and end date values passed down as properties
    Dialog.create({
      component: PlaybackRecordFilterDialog,
      componentProps: {
        startDateProp: startDate.value,
        endDateProp: endDate.value
      }
    }).onOk(async (data) => {
      // Sets start and end date values to those returned from the dialog
      startDate.value = data.startDate;
      endDate.value = data.endDate;

      // Resets the current page values to 1
      currentPage.value = 1;
      currentGroupPage.value = 1;

      // Sets status codes to null to show loading spinners in the tables
      statusCode.value = null;
      groupStatusCode.value = null;

      // Gets records from the API with the new date ranges
      await getRecords();
      await getGroupedRecords();
    });
  }

  // Function to open a dialog to add a track to the queue
  function openQueueDialog(trackId: string, name: string) {
    // Creates dialog with the AddTrackToQueueDialog component, with the track ID and name passed down as properties
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
