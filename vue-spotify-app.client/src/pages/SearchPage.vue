<template>
  <div class="q-gutter-md q-pa-md">
    <div class="text-h4">Search</div>

  <div>
    <QForm @submit.prevent="submitForm" style="max-width: 600px">
      <QCard class="q-pa-md">
        <QCardSection>
          <QInput v-model="searchDto.query" label="Search query" dense/>
        </QCardSection>
        <QSlideTransition>
          <div v-if="showAdvancedSearch">
            <QCardSection>
            <QSelect
              multiple
              :options="itemTypes"
              option-value="value"
              option-label="label"
              use-chips
              v-model="selectedItemTypes"
              label="Item type"
              dense
              options-dense
              >
            </QSelect>

              <QInput v-model="searchDto.track" label="Track name" dense/>
              <QInput v-model="searchDto.artist" label="Artist name" dense />
              <QInput v-model="searchDto.album" label="Album name" dense/>
              <QInput v-model="searchDto.year" label="Year range" dense/>
              <QInput v-model="searchDto.genre" label="Genre" dense/>
              <QInput v-model="searchDto.ISRC" label="ISRC" dense/>
              <QInput v-model="searchDto.UPC" label="UPC" dense/>
              <div class="row">
                <QCheckbox v-model="searchDto.newAlbums" label="New albums only"/>
                <QCheckbox v-model="searchDto.hipsterAlbums" label="Hipster albums only"/>
              </div>
            </QCardSection>
          </div>
        </QSlideTransition>
        <QCardActions>
          <QBtn flat label="Advanced search" :icon-right="showAdvancedSearch ? 'keyboard_arrow_up' : 'keyboard_arrow_down'" @click="showAdvancedSearch = !showAdvancedSearch" />
          <QSpace />
          <QBtn flat label="Clear" @click="searchDto = new SearchDTO()" :disable="statusCode === null" />
          <QBtn type="submit" label="Search" color="primary" :loading="statusCode === null" />
        </QCardActions>
      </QCard>
    </QForm>
  </div>
  <div>
    <QTabs v-model="selectedTab" inline-labels>
      <QTab v-for="itemType in itemTypes" :key="itemType.value" :name="itemType.value" :label="itemType.label">
        <QBadge floating color="primary" />
      </QTab>
    </QTabs>
    <QTabPanels v-model="selectedTab">
      <QTabPanel name="track">
        <!-- Track search results are displayed in the table below -->
        <QTable
          :columns="columns"
          :rows="tracks"
          row-key="id"
          wrap-cells
          :loading="statusCode === null"
          v-model:pagination="pagination"
          @request="onPageChange"
          >

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
        <template v-slot:body-cell-album="props">
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
        <template v-slot:bottom>
                <QSpace />
                  <QPagination v-model="pagination.page"
                            :max="Math.ceil(pagination.rowsNumber / pagination.rowsPerPage)"
                            size="sm"
                            @update:model-value="getTracks()"
                            input />
                </template>
              <template v-slot:loading>
            <QInnerLoading showing size="50px" color="green" />
          </template>
        </QTable>
      </QTabPanel>
      <QTabPanel name="album">
        <p>Album search results will be displayed here.</p>
      </QTabPanel>
    </QTabPanels>
  </div>
  </div>
</template>
<script setup lang="ts">
import SearchDTO from '@/classes/searchDTO';
import TrackViewModel from '@/classes/trackViewModel';
import AddTracksToPlaylistDialog from '@/dialogs/addTracksToPlaylistDialog.vue';
import AddTrackToQueueDialog from '@/dialogs/addTrackToQueueDialog.vue';
import ConvertMilisecondsToMinutesAndSeconds from '@/helperFunctions/convertMilisecondsToMinutesAndSeconds';
import axios, { AxiosError } from 'axios';
import { Dialog, Notify } from 'quasar';
import { computed, ref } from 'vue';


  const searchDto = ref<SearchDTO>(new SearchDTO());
  const tracks = ref<TrackViewModel[]>([]);

  const showAdvancedSearch = ref(false);

  const statusCode = ref<number | null>(0);

  const columns = [
    { name: 'name', label: 'Track Name', field: "name", align: 'left' },
    { name: 'artists', label: 'Artist', field: "artists", align: 'left' },
    { name: 'album', label: 'Album', field: "album", align: 'left' },
    { name: "length", label: "Length", field: "length", align: "left", style: "width: 100px" },
    { name: "actions", label: "Actions", align: "left", style: "width: 5%", sortable: false
    }
  ];

const itemTypes = [
  { label: 'Tracks', value: 'track' },
  { label: 'Albums', value: 'album' },
  { label: 'Artists', value: 'artist' },
  { label: 'Playlists', value: 'playlist' },
  { label: 'Audiobooks', value: 'audiobook' },
  { label: 'Shows', value: 'show' },
  { label: 'Episodes', value: 'episode' }
];

const selectedItemTypes = ref<{label: string, value: string}[]>([]);

  const selectedTab = ref("track");

  const pagination = ref({
    page: 1,
    rowsPerPage: 10,
    rowsNumber: 0
  });

  async function submitForm() {
    searchDto.value.offset = 0;
    searchDto.value.itemTypes = selectedItemTypes.value.map(x => x.value);

    statusCode.value = null;
    try{
      const response = await axios.post("/api/search", searchDto.value, {
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json"
      }
    });

    console.log(response.data);
    statusCode.value = response.status;
    }
    catch(error){
      console.log(error as AxiosError);
      statusCode.value = (error as AxiosError).response?.status || 500;
      //alert("An error occurred while searching. Please try again.");
    }
  }

  async function getTracks(){
    try {
      statusCode.value = null;
      const response = await axios.post("track/searchtracks", searchDto.value, {
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json"
        }
      });
      tracks.value = [];
      response.data.tracks.forEach(element => {
        const viewModel = new TrackViewModel(element);
       tracks.value.push(viewModel);
      });
      pagination.value.rowsNumber = response.data.total;
      statusCode.value = response.status;
    } catch (error) {
      statusCode.value = (error as AxiosError).response?.status;
      console.error("Error during search:", error);
      alert("An error occurred while searching. Please try again.");
    }
  }

  async function onPageChange(props){
    const {page, rowsPerPage} = props.pagination;
    searchDto.value.offset = (page - 1) * 10;
    await submit();
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

</script>
