<template>
  <QDialog
    class="relative-position"
    ref="dialogRef"
    backdrop-filter="blur(4px)"
    persistent>
    <QCard>
      <div v-if="deviceStatusCode === 200 && availableDevices.length > 0">
        <QCardSection class="row items-center q-pb-none">
          <div>
            <div class="text-h6">Add random tracks to queue</div>
          </div>
          <QSpace />
          <QBtn icon="close" flat dense round v-close-popup />
        </QCardSection>
        <QForm>
          <QCardSection>
            <QTable :columns="tableColumns"
                    :rows="tracksToAdd"
                    row-key="id"
                    wrap-cells
                    no-data-label="No tracks are loaded."
                    flat
                    style="height: 400px;"
                    >
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
            </QTable>
          </QCardSection>

          <QCardSection class="q-gutter-sm">
            <div class="row q-gutter-md items-center justify-center">
                <div class="col">
                    <QInput
                        v-model.number="numberOfTracks"
                        type="number"
                        label="Number of tracks"
                        min="1"
                        max="20"
                        />                        
                </div>
                <div class="col"">
                    <QSpace/>
                    <QBtn
                        type="button"
                        icon="shuffle"
                        class="full-width"
                        label="Get random tracks"
                        :loading="shuffleLoading"
                        @click="getRandomTracks"
                        />
                </div>
            </div>
            <QSelect v-model="selectedDevice"
                     :options="availableDevices"
                     option-label="name"
                     option-value="id"
                     label="Selected device"
                     outlined
                     dense
                     class="q-mt-md" />
            <QList v-if="props.listID">
                <QItem tag="label">
                    <QItemSection>
                        <QItemLabel>Remove selected tracks from list</QItemLabel>                    
                    </QItemSection>
                    <QItemSection avatar>
                        <QToggle v-model="removeSelectedTracks"/>
                    </QItemSection>
                </QItem>
            </QList>
          </QCardSection>
        </QForm>

        <QCardActions align="right">
          <QBtn flat label="OK" :disable="tracksToAdd.length < 1 || shuffleLoading" color="primary" :loading="processingResponse" @click="onOK" />
        </QCardActions>
      </div>
      <div v-else-if="deviceStatusCode != null">
        <div>{{ deviceStatusCode === 200 ? 'No available devices were found.' : 'An error has occured.' }}</div>
        <QBtn flat label="Retry" color="primary" @click="getAvailableDevices" />
      </div>
      <QInnerLoading :showing="deviceStatusCode === null || processingResponse">
        <div class="row items-center justify-center" style="height: 200px;">
          <q-spinner-dots size="50px" color="green" />
        </div>
      </QInnerLoading>
    </QCard>
  </QDialog>
</template>
<script setup lang="ts">
  import type DeviceInfo from '@/classes/deviceInfo';
import TrackListDTO from '@/classes/trackListDTO';
  import TrackViewModel from '@/classes/trackViewModel';
  import ConvertMilisecondsToMinutesAndSeconds from '@/helperFunctions/convertMilisecondsToMinutesAndSeconds';
  import axios, { AxiosError } from 'axios';
  import { Notify, useDialogPluginComponent } from 'quasar';
  import { onBeforeMount, ref } from 'vue';


  const props = defineProps<{
    listID: string | null | undefined,
    playlistID: string | null | undefined
  }>();

  const numberOfTracks = ref<number>(1);

  const tracksToAdd = ref<TrackViewModel[]>([]);

  const selectedDevice = ref<DeviceInfo | null | undefined>();
  // A list of devices the user can select
  const availableDevices = ref<DeviceInfo[]>([]);
  // The status code of the available devices call
  const deviceStatusCode = ref<number | null>(null);

    const shuffleLoading = ref(false);

    const removeSelectedTracks = ref(false);

    const processingResponse = ref(false);

  const tableColumns = [
    {
      name: "order",
      label: "#",
      field: (row: TrackViewModel) => tracksToAdd.value.indexOf(row) + 1,
      align: "left",
      sortable: false
    },
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
  ]

  onBeforeMount(async () => {
    await getAvailableDevices();
  });

  async function getAvailableDevices() {
    try {
      const response = await axios.get("/api/playbackqueue/getdevices");
      availableDevices.value = response.data as DeviceInfo[];
      selectedDevice.value = availableDevices.value.length > 0 ? availableDevices.value[0] : null;
      deviceStatusCode.value = response.status;
    }
    catch (error) {
      deviceStatusCode.value = (error as AxiosError).response?.status || null;
      console.error(error);
      return [];
    }
  }

  async function getRandomTracks() {
    try {
        shuffleLoading.value = true;
        const urlSearchParams = new URLSearchParams();
        if(props.playlistID) urlSearchParams.append("playlistID", props.playlistID);
        if(props.listID) urlSearchParams.append("listID", props.listID);
        urlSearchParams.append("count", numberOfTracks.value.toLocaleString());
        console.log(urlSearchParams.toString());
        const response = await axios.get(`/api/track/getrandomtracks?${urlSearchParams.toString()}`);
        tracksToAdd.value = response.data as TrackViewModel[];
    } catch (error) {
        console.log(error as AxiosError);
    }
    finally {
        shuffleLoading.value = false;
    }
  }

  defineEmits([
    ...useDialogPluginComponent.emits
  ]);

  const { dialogRef, onDialogOK, onDialogCancel } = useDialogPluginComponent();

  async function onOK() {
    try {
        processingResponse.value = true;
        const trackIDs =  tracksToAdd.value.map(track => track.id);
        await axios.post( `/api/playbackqueue/addtoqueue`,
            {
                spotifyTrackIds: trackIDs,
                deviceId: selectedDevice.value?.id
            });
        if(props.listID && removeSelectedTracks.value)
        {
            const listDTO = new TrackListDTO();
            listDTO.listID = props.listID;    
            listDTO.trackIDs = trackIDs; 
            await axios.delete("/api/tracklist/removetracksfromlist", {
                data: listDTO
            });
        }
        Notify.create({
                message: `Successfully added ${tracksToAdd.value.length} ${tracksToAdd.value.length != 1 ? "tracks" : "track"} to queue.`,
                color: "green"
                });
        onDialogOK({
            numberOfTracks: tracksToAdd.value.length,
            removeSelectedTracks: removeSelectedTracks.value
        });
    } catch (error) {
            Notify.create({
                message: `Error adding tracks to queue.`,
                color: "red"
            });
        }
    finally {
        processingResponse.value = false;
    }
  }

</script>
