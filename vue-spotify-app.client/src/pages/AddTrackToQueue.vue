<template>
  <div class="q-pa-sm">
    <h4 class="q-ma-sm">Add Track to Queue</h4>
    <p>Please enter the ID of a track in Spotify.</p>
    <QForm @submit.prevent="addToQueue()">
      <QCard flat class="q-pa-md q-mb-md" style="width: 50%;">
        <QCardSection>
          <QInput
            v-model="trackId"
            label="Track ID"
            outlined
            dense
            required
          />
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
          <QBtn type="submit" label="Add to Queue" v-if="!!selectedDevice" color="primary" />
        </QCardActions>
      </QCard>
    </QForm>
    <div class="row q-pa-md">
      <div class="col">

      </div>
      <div class="col">
      </div>
    </div>
  </div>
  </template>

<script setup lang="ts">
  import DeviceInfo from '@/classes/deviceInfo';
  import { useAuthStore } from '@/stores/authStore';
  import axios from 'axios';
  import { onBeforeMount, ref } from 'vue';

  const trackId = ref("");
  const selectedDevice = ref<DeviceInfo | null | undefined>();
  const availableDevices = ref<DeviceInfo[]>([]);

  const authStore = useAuthStore();

  onBeforeMount(async () => {
    await getAvailableDevices();
  });

  async function getAvailableDevices() {
    try {
      const response = await axios.get("/playbackqueue/getdevices");
      availableDevices.value = response.data as DeviceInfo[];
      selectedDevice.value = availableDevices.value.length > 0 ? availableDevices.value[0] : null;
    }
    catch (error) {
      alert("Error fetching available devices.");
      console.error(error);
      return [];
    }
  }

  async function addToQueue() {
    try {
      const response = await axios.post("/playbackqueue/addtoqueue", {
        spotifyTrackId: trackId.value,
        deviceId: selectedDevice.value?.id
      });
      alert("Successfully added track to queue.");
      trackId.value = "";
    }
    catch (error) {
      alert("Error adding track to queue.");
      console.error(error);
    }
  }
</script>
