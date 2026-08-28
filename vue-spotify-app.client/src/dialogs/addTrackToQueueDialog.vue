<template>
  <QDialog class="relative-position" ref="dialogRef" backdrop-filter="blur(4px)" persistent>
      <QCard>
        <div v-if="deviceStatusCode === 200 && availableDevices.length > 0">          
            <QCardSection class="row items-center q-pb-none">
              <div class="text-h6">Adding track to queue</div>
              <QSpace/>
              <QBtn icon="close" flat dense round v-close-popup />
            </QCardSection>
            <QCardSection>
              <TrackPreviewCard :track="props.track" :show-liked-songs="false" />
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
import type TrackViewModel from '@/classes/trackViewModel';
import TrackPreviewCard from '@/components/TrackPreviewCard.vue';
import { useAuthStore } from '@/stores/authStore';
import { biSpotify } from '@quasar/extras/bootstrap-icons';
import axios, { AxiosError } from 'axios';
import { Notify, useDialogPluginComponent } from 'quasar';
import { onBeforeMount, ref } from 'vue';

const props = defineProps<{
  track: TrackViewModel
}>();

  const authStore = useAuthStore();
  // The device's queue that the track will be added to
  const selectedDevice = ref<DeviceInfo | null | undefined>();
  // A list of devices the user can select
  const availableDevices = ref<DeviceInfo[]>([]);
  // The status code of the available devices call
  const deviceStatusCode = ref<number | null>(null);

onBeforeMount( async () => {
  await getAvailableDevices();
});

  async function getAvailableDevices() {
    try{

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

async function onOK(){
  try {
    await axios.post(
      `/api/playbackqueue/addtoqueue`,
      {
        spotifyTrackIds: [props.track.id],
        deviceId: selectedDevice.value?.id
      });
  Notify.create({
          message: `Successfully added track to queue.`,
          color: "green"
        });
  onDialogOK();
  }
  catch (error) {
    Notify.create({
        message: `Error adding track to queue.`,
        color: "red"
      });
    return;
  }
}
</script>
