<template>
  <QDialog class="relative-position" ref="dialogRef" backdrop-filter="blur(4px)">
      <QCard>
        <div v-if="deviceStatusCode === 200 && availableDevices.length > 0">
          <QCardSection>
          <div class="text-h6">Adding {{ name }} to queue</div>

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
          <QBtn flat label="Cancel" color="primary" @click="onDialogCancel" />
          <QBtn flat label="OK" color="primary" @click="onOK" />
        </QCardActions>
        </div>
    <div v-else-if="deviceStatusCode != null">
      <div>{{ deviceStatusCode === 200 ? 'No available devices were found.' : 'An error has occured.' }}</div>
      <QBtn flat label="Close" color="primary" @click="onDialogCancel" />
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
import { useAuthStore } from '@/stores/authStore';
import { biSpotify } from '@quasar/extras/bootstrap-icons';
import axios, { AxiosError } from 'axios';
import { Notify, useDialogPluginComponent } from 'quasar';
import { onBeforeMount, ref } from 'vue';

const props = defineProps<{
  trackId: string;
  name: string;
}>();

  const authStore = useAuthStore();
  const selectedDevice = ref<DeviceInfo | null | undefined>();
  const availableDevices = ref<DeviceInfo[]>([]);
  const deviceStatusCode = ref<number | null>(null);

onBeforeMount( async () => {
  await getAvailableDevices();
});

  async function getAvailableDevices() {
    try{

    const response = await axios.get("playbackqueue/getdevices");
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

function onOK(){
  try{
    axios.post(
      `playbackqueue/addtoqueue`,
      {
        spotifyTrackId: props.trackId,
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
