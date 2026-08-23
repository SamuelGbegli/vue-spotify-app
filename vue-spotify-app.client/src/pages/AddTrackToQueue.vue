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
  <hr/>

  <div>
    <div class="text-h4">Add tracks to queue</div>
    <p>Enter a list of track IDs to add to the queue. Place each ID on a new line.</p>
    <QForm @submit.prevent="validateIDs()">
      <QCard flat class="q-pa-md q-mb-md" style="width: 50%;">
        <QCardSection>
          <QInput
            v-model="trackIds"
            label="Track IDs"
            outlined
            dense
            required
            type="textarea"
          />
        </QCardSection>
        <QCardActions align="right">
          <QBtn type="submit" label="Add to Queue" :loading="loading" color="primary" />
        </QCardActions>
      </QCard>
    </QForm>
  </div>
  </template>

<script setup lang="ts">
  import DeviceInfo from '@/classes/deviceInfo';
import type TrackViewModel from '@/classes/trackViewModel';
import AddTracksToQueueDialog from '@/dialogs/addTracksToQueueDialog.vue';
  import { useAuthStore } from '@/stores/authStore';
  import axios from 'axios';
import { Dialog } from 'quasar';
  import { onBeforeMount, ref } from 'vue';

  const trackId = ref("");
  const trackIds = ref("");
  const selectedDevice = ref<DeviceInfo | null | undefined>();
  const availableDevices = ref<DeviceInfo[]>([]);

  const loading = ref(false);

  onBeforeMount(async () => {
    await getAvailableDevices();
  });

  async function getAvailableDevices() {
    try {
      const response = await axios.get("/api/playbackqueue/getdevices");
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
      const response = await axios.post("/api/playbackqueue/addtoqueue", {
        spotifyTrackIds: [trackId.value],
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

  async function validateIDs(){
    loading.value = true;
    const ids = trackIds.value.split("\n").map(id => id.trim());
    console.log(ids);
    try{
      const response = await axios.post("/api/track/validatetracks", 
      ids
      );
      console.log(response.data);
      const foundTracks = response.data as TrackViewModel[];
      if(foundTracks.length < 0){
        alert("No valid tracks found.");
        return;
      }
      else{
        Dialog.create({
          component: AddTracksToQueueDialog,
            componentProps: {
              tracks: foundTracks
            }
          });
      }
    }
    catch (error) {
      alert("Error validating track IDs.");
      console.error(error);
    }
    finally{
      loading.value = false;
    }
  }
</script>
