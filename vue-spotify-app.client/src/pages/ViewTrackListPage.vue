<template>
  <div class="q-pa-md">
    <!-- Shows track list information -->
      <div class="text-h4">
        <span v-if="trackListViewModel">{{trackListViewModel?.name }}</span>
        <span v-else-if="statusCode == null">Loading...</span>
        <span v-else>List not found</span>
      </div>
    <div v-if="statusCode === 200" class="q-ma-sm  q-gutter-sm">
      <TrackListNew :listId="route.params.id?.toString()"/>
  </div>
  </div>

</template>
<script setup lang="ts">
  import TrackListNew from '@/components/TrackListNew.vue';
  import { useAuthStore } from '@/stores/authStore';
  import axios, { AxiosError } from 'axios';
  import { onBeforeMount, ref, watch } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useTitle } from '@vueuse/core';
import TrackListViewModel from '@/classes/TrackListViewModel';


  // Stores the view model for the track list
  const trackListViewModel = ref<TrackListViewModel | null>(null);
  // Stores the status code when fetching a track list
  const statusCode = ref<number | null>(null);

  const route = useRoute();
  const router = useRouter();
  const title = useTitle('Track List');

  const authStore = useAuthStore();

  onBeforeMount(async () => {
    await getTrackList();
  });

  watch(() => route.params.id, async () => {
    await getTrackList();
  });

  async function getTrackList() {
    try {
      console.log(route.params.id)
      const response = await axios.get(`/api/tracklist/gettracklist/${route.params.id}`,
      );
      trackListViewModel.value = response.data as TrackListViewModel;
      title.value = trackListViewModel.value.name;
      statusCode.value = response.status;

    } catch (error) {
      title.value = 'List not found';
      statusCode.value = (error as AxiosError).status;
    }
  }

</script>
