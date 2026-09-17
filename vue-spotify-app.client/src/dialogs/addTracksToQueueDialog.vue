<template>
    <QDialog class="relative-position" ref="dialogRef" backdrop-filter="blur(4px)" persistent>
      <QCard>
        <div v-if="deviceStatusCode === 200 && availableDevices.length > 0">          
            <QCardSection class="row items-center q-pb-none">
              <div class="text-h6">Adding {{ tracks.length }} {{ tracks.length != 1 ? "tracks" : "track" }} to queue</div>
              <QSpace/>
              <QBtn icon="close" flat dense round v-close-popup />
            </QCardSection>
            <QCardSection>
          <QTable
            :columns="tableColumns"
            :rows="tracks"
            row-key="id"
            wrap-cells
            flat>
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
        <QCardSection>
          <QSelect
            v-model="selectedDevice"
            :options="availableDevices"
            option-label="name"
            option-value="id"
            label="Selected device"
            outlined
            dense
            class="q-mt-md"
          />
        </QCardSection>

        <QCardActions align="right">
          <QBtn flat label="OK" color="primary" @click="onOK" />
        </QCardActions>
        </div>
    <div v-else-if="deviceStatusCode != null">
      <div>{{ deviceStatusCode === 200 ? 'No available devices were found.' : 'An error has occured.' }}</div>
      <QBtn flat label="Retry" color="primary" @click="getAvailableDevices" />
    </div>
      <QInnerLoading :showing="deviceStatusCode === null">
      <div class="row items-center justify-center" style="height: 200px;">
        <q-spinner-dots size="50px" color="green" />
      </div>
    </QInnerLoading>
    </QCard>
    </QDialog>
        
</template>

<script setup lang="ts">

    import DeviceInfo from '@/classes/deviceInfo';
    import TrackViewModel from '@/classes/trackViewModel';
    import { onBeforeMount, ref } from 'vue';
    import axios, { AxiosError } from 'axios';
    import { Notify, Loading, useDialogPluginComponent } from 'quasar';
import ConvertMilisecondsToMinutesAndSeconds from '@/helperFunctions/convertMilisecondsToMinutesAndSeconds';

    const props = defineProps<{
      tracks: TrackViewModel[];
    }>();

    // The device's queue that the track will be added to
    const selectedDevice = ref<DeviceInfo | null | undefined>();
    // A list of devices the user can select
    const availableDevices = ref<DeviceInfo[]>([]);
    // The status code of the available devices call
    const deviceStatusCode = ref<number | null>(null);

    const tableColumns = [
        {
            name: "order",
            label: "#",
            field: (row: TrackViewModel) => props.tracks.indexOf(row) + 1,
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

    onBeforeMount( async () => {
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

  defineEmits([
  ...useDialogPluginComponent.emits
]);

const {dialogRef, onDialogOK, onDialogCancel} = useDialogPluginComponent();

// TODO: Add onOK function
async function onOK(){
  Loading.show({
    message: "Adding tracks to queue..."
  });
  try{
    await axios.post(
      `/api/playbackqueue/addtoqueue`,
      {
        spotifyTrackIds: props.tracks.map(track => track.id),
        deviceId: selectedDevice.value?.id
      });
  Notify.create({
          message: `Successfully added ${props.tracks.length} ${props.tracks.length != 1 ? "tracks" : "track"} to queue.`,
          color: "green"
        });
  onDialogOK();
  }
  catch (error) {
    Notify.create({
        message: `Error adding tracks to queue.`,
        color: "red"
      });
    return;
  }
  finally{
    Loading.hide();
  }
}
</script>