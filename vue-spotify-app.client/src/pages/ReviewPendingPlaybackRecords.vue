<template>
    <div>
        <h4>Review pending playback records</h4>
        <div class="row">
          <QSpace/>
          <QBtn label="Select all" @click="() => pendingRecords.forEach(x => x.isSelected = true)"/>
          <QBtn label="Invert selection" @click="() => pendingRecords.forEach(x => x.isSelected = !x.isSelected)"/>
          <QBtn label="Clear selection" @click="() => pendingRecords.forEach(x => x.isSelected = false)"/>
          <QBtnDropdown label="Push records" v-if="pendingRecords.length > 0">
            <QList>
              <QItem clickable v-close-popup @click="confirmSelection('all')">
                <QItemSection>
                  <QItemLabel>Push all visible records</QItemLabel>
                </QItemSection>
              </QItem>
              <QItem clickable v-close-popup @click="confirmSelection('selected')" v-if="pendingRecords.filter(x => x.isSelected).length > 0">
                <QItemSection>
                  <QItemLabel>Push all selected records</QItemLabel>
                </QItemSection>
              </QItem>
              <QItem clickable v-close-popup @click="confirmSelection('matched')" v-if="pendingRecords.filter(x => x.recordedTrackName.toLowerCase() == x.foundTrackName.toLowerCase() ||
          x.recordedTrackName.toLowerCase().startsWith(x.foundTrackName.toLowerCase())).length > 0">
                <QItemSection>
                  <QItemLabel>Push all records with matching names</QItemLabel>
                </QItemSection>
              </QItem>
            </QList>
          </QBtnDropdown>
        </div>
        <div class="row q-pa-xs">
          <h5 class="q-pa-xs">Total records: {{ totalRecords }}</h5>
          <QSpace/>
          <QPagination class="q-pa-xs" v-model="currentPage" :max="Math.ceil(totalRecords / 50)" input @update:model-value="getRecords()" />
        </div>
        <h5 class="q-pa-xs" v-if="pendingRecords && pendingRecords.length > 0">Selected: {{ pendingRecords.filter(x => x.isSelected).length }}</h5>
        <QMarkupTable v-if="pendingRecords.length > 0">
          <thead>
            <tr>
              <th></th>
              <th>Date added</th>
              <th>Recorded track name</th>
              <th>Recorded Spotify ID</th>
              <th></th>
              <th>Found track name</th>
              <th>Artists</th>
              <th>Album</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="value in pendingRecords" :key="value.id" :class="getRowBackground(value.recordedTrackName, value.foundTrackName)">
              <td>
                <QCheckbox v-model="value.isSelected"/>
              </td>
              <td>{{ value.dateAdded }}</td>
              <td>{{ value.recordedTrackName }}</td>
              <td>{{ value.recordedSpotifyID }}</td>
              <td>
                <QImg :src="value.foundAlbumCover" width="40px"/>
              </td>
              <td>{{ value.foundTrackName }}</td>
              <td>{{ value.foundArtists.toString() }}</td>
              <td>{{ value.foundAlbum }}</td>
              <td>
                <QCheckbox v-model="value.isSelected"/>
              </td>
            </tr>
          </tbody>
        </QMarkupTable>
        <QInnerLoading :showing="statusCode === null">
      <div class="row items-center justify-center" style="height: 200px;">
        <q-spinner-dots size="50px" color="green"/>
      </div>
    </QInnerLoading>
    </div>
</template>
<script setup lang="ts">
import PendingPlaybackRecordViewModel from '@/classes/pendingPlaybackRecordViewModel';
import { useAuthStore } from '@/stores/authStore';
import axios, { AxiosError } from 'axios';
import { Loading, Notify } from 'quasar';
import { onBeforeMount, ref } from 'vue';

const authStore = useAuthStore();

// Visible records
const pendingRecords = ref<PendingPlaybackRecordViewModel[]>([]);

// Total number of pending records
const totalRecords = ref(0)

// Page of records currently visible
const currentPage = ref(1)

const statusCode = ref<number | null>(null)

onBeforeMount(async () => {
  await getRecords();
})


async function getRecords() {
  statusCode.value = null;
  try{
    const response = await axios.get(
      `playbackrecord/getpendingrecords?page=${currentPage.value}`, {
        headers: { authToken: authStore.accessToken}
      }
    );
    console.log(response.data);
  pendingRecords.value = [];
    totalRecords.value = response.data.totalRecords;
    response.data.records.forEach(element => {
      const recordViewModel = new PendingPlaybackRecordViewModel();
      recordViewModel.initialise(element);
      pendingRecords.value.push(recordViewModel);
    });
    statusCode.value = response.status;
  }
  catch(error){
    const ex = error as AxiosError;
    console.log(ex);
    statusCode.value = ex.status || null;
  }
}

  function getRowBackground(inputtedName: string, fetchedName: string) {
    if (inputtedName.toLowerCase() == fetchedName.toLowerCase() || inputtedName.toLowerCase().startsWith(fetchedName.toLowerCase())) return "bg-green";
    if (fetchedName.toLowerCase().startsWith(inputtedName.toLowerCase())) return "bg-amber";
    return "";
  }

  async function confirmSelection(values: string){
    let response = false;
    switch (values){
      case "all":
        response = confirm("Push all visible records to database?");
        if (response == true) {
          await pushValues(pendingRecords.value.map(x => x.id));
        }
          break;
      case "selected":
        response = confirm("Push all selected records to database?");
        if (response == true) {
          await pushValues(pendingRecords.value.filter(x => x.isSelected).map(x => x.id));
        }
          break;
      case "matched":
        response = confirm("Push all records with matching track titles to database?");
        if (response == true) {
          const values = pendingRecords.value.filter(x => x.recordedTrackName.toLowerCase() == x.foundTrackName.toLowerCase() ||
          x.recordedTrackName.toLowerCase().startsWith(x.foundTrackName.toLowerCase()))
          await pushValues(values.map(x => x.id));
        }
        break;
    }
  }

  async function pushValues(ids: string[]){
    Loading.show({
      message: "Please wait..."
    });
    try {
      await axios.post("playbackrecord/pushpendingrecords", ids);
      alert(`Successfully pushed ${ids.length} records.`)
      if(currentPage.value > Math.ceil((totalRecords.value - ids.length) / 50))
        currentPage.value -= 1;
      await getRecords();
    } catch (error) {
      alert("An error has occured.");
      console.log(error as AxiosError);
    }
    Loading.hide();
  }

  function showNumberSelected(){

  }

</script>
