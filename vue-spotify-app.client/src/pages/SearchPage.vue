<template>
  <div class="q-gutter-md q-pa-md">
    <div class="text-h4">Search</div>
    <div class="row q-gutter-xs">
      <div class="col-5">
         <QForm class="q-gutter-md" @submit.prevent="submit">
    <QCard class="q-pa-md" style="width: 50%">
      <QCardSection>
        <QInput v-model="searchDto.track" label="Track name" dense/>
        <QInput v-model="searchDto.artist" label="Artist name" dense />
        <QInput v-model="searchDto.album" label="Album name" dense/>
        <QInput v-model="searchDto.year" label="Year range" dense/>
        <QInput v-model="searchDto.genre" label="Genre" dense/>
        <QInput v-model="searchDto.ISRC" label="ISRC" dense/>
      </QCardSection>
      <QCardActions align="right">
        <QBtn type="submit" label="Search" color="primary" :loading="statusCode === null" />
      </QCardActions>
    </QCard>
  </QForm>
  <div>
    <QTable
      :columns="searchColumns"
      :rows="tracks"
      row-key="id"
      wrap-cells
      :loading="statusCode === null"
      v-model:pagination="pagination"
      @request="onPageChange"
      >
      <template v-slot:body-cell-selected="props">
        <QTd :props="props">
          <QCheckbox v-model="props.row.selected" @update:model-value="(value) => {
            if(value) {
              selectedTracks.set(props.row.id, props.row);
            } else {
              selectedTracks.delete(props.row.id);
            }
          }" />
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
  </div>
    </div>
    <!--<div class="col-5">
      <div class="row">
        <div class="text-h5">Selected tracks</div>
        <QSpace />
        <QBtnDropdown label="Actions" color="primary" :disable="selectedTracks.size === 0">
          <QList style="min-width: 150px">
            <QItem clickable v-close-popup @click="openQueueDialog(Array.from(selectedTracks.values())[0].id, Array.from(selectedTracks.values())[0].name)">
              <QItemSection>
                <QItemLabel>Add first selected track to queue</QItemLabel>
              </QItemSection>
            </QItem>
            <QItem clickable v-close-popup @click="openAddToPlaylistDialog()">
              <QItemSection>
                <QItemLabel>Add to playlist</QItemLabel>
              </QItemSection>
            </QItem>
            <QItem clickable v-close-popup @click="() => {
              selectedTracks.clear();
              tracks.forEach(x => x.selected = false);
            }">
              <QItemSection>
                <QItemLabel>Clear selection</QItemLabel>
              </QItemSection>
            </QItem>
          </QList>
        </QBtnDropdown>
      </div>
      <QTable
        :columns="selectedColumns"
        :rows="Array.from(selectedTracks.values())"
        row-key="id"
        wrap-cells
        no-results-label="No tracks selected"
        no-data-label="No tracks selected"
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
          <template v-slot:body-cell-remove="props">
            <QTd :props="props">
              <div class="text-left">
                <QBtn flat dense icon="close" color="red" @click="() =>{
                  selectedTracks.delete(props.row.id);
                  if(tracks.find(x => x.id === props.row.id)) {
                    tracks.find(x => x.id === props.row.id)!.selected = false;
                  }
                }" />
              </div>
            </QTd>
          </template>
        </QTable>
      </div>-->
    </div>
  </div>
</template>
<script setup lang="ts">
import SearchDTO from '@/classes/searchDTO';
import TrackViewModel from '@/classes/trackViewModel';
import AddTracksToPlaylist from '@/dialogs/addTracksToPlaylist.vue';
import AddTracksToPlaylistDialog from '@/dialogs/addTracksToPlaylistDialog.vue';
import AddTrackToQueueDialog from '@/dialogs/addTrackToQueueDialog.vue';
import ConvertMilisecondsToMinutesAndSeconds from '@/helperFunctions/convertMilisecondsToMinutesAndSeconds';
import axios, { AxiosError } from 'axios';
import { Dialog, Notify } from 'quasar';
import { computed, ref } from 'vue';


  const searchDto = ref<SearchDTO>(new SearchDTO());
  const tracks = ref<TrackViewModel[]>([]);

  const selectedTracks = ref<Map<string, TrackViewModel>>(new Map<string, TrackViewModel>());

  const statusCode = ref<number | null>(0);

  const columns = [
    { name: 'selected', label: 'Selected', field: "selected", align: 'centred', style: "width: auto" },
    { name: 'remove', label: 'Remove', align: 'centred', style: "width: auto" },
    { name: 'name', label: 'Track Name', field: "name", align: 'left' },
    { name: 'artists', label: 'Artist', field: "artists", align: 'left' },
    { name: 'album', label: 'Album', field: "album", align: 'left' },
    { name: "length", label: "Length", field: "length", align: "left", style: "width: 100px" },
    { name: "actions", label: "Actions", align: "left", style: "width: 5%", sortable: false
    }
  ];

  const searchColumns = computed(() => {
    return columns.filter(x => x.name !== "remove");
  });

  const selectedColumns = computed(() => {
    return columns.filter(x => x.name !== "selected");
  });

  const pagination = ref({
    page: 1,
    rowsPerPage: 10,
    rowsNumber: 0
  });

  async function submit() {
    searchDto.value.offset = 0;
    await getTracks();
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
        viewModel.selected = selectedTracks.value.has(viewModel.id);
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

  function openAddToPlaylistDialog() {
    Dialog.create({
      component: AddTracksToPlaylistDialog,
      componentProps: {
        tracks: Array.from(selectedTracks.value.values())
      }
    }).onOk(async (data) => {

    });
  }
</script>
