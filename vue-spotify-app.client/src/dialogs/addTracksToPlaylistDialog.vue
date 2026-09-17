<template>
  <QDialog ref="dialogRef" backdrop-filter="blur(4px)" class="relative-position">
    <QCard>
    <div v-if="playlistStatusCode === 200 && playlists.length > 0">
      <QCardSection>
        <div class="text-h6">Add selected tracks to playlist</div>
      </QCardSection>
      <QCardSection horizontal>
      <div class="col">
        <div>{{ tracks.length }} track(s) will be added to the playlist.</div>
        <QScrollArea style="max-height: 200px;" class="fit q-mt-md">
          <QList>
            <QItem v-for="track in tracks" :key="track.id">
              <QItemSection>
                {{ track.name }} - {{ track.artists.map(a => a.name).join(', ') }}
              </QItemSection>
            </QItem>
          </QList>
        </QScrollArea>
      </div>

      <QCardSection>
        <QSelect
          v-model="selectedPlaylist"
          :options="playlists"
          label="Select playlist"
          option-value="id"
          option-label="name"
        />
      </QCardSection>
      </QCardSection>
        <QCardActions align="right">
          <QBtn flat label="Cancel" color="primary" @click="onDialogCancel" />
          <QBtn flat label="OK" :disable="!selectedPlaylist" color="primary" @click="onOK" />
        </QCardActions>
        <QInnerLoading :showing="playlistStatusCode === null">
      <div class="row items-center justify-center" style="height: 200px;">
        <q-spinner-dots size="50px" color="green" />
      </div>
    </QInnerLoading>
    </div>
    <div v-else-if="playlistStatusCode != null">
      <QCardSection>
        <div class="text-h6">{{ playlistStatusCode === 200 ? 'No playlists were found.' : 'An error has occured.' }}</div>
      </QCardSection>
      <QBtn flat label="Close" color="primary" @click="onDialogCancel" />
    </div>
    </QCard>
  </QDialog>
</template>

<script setup lang="ts">
import AddToPlaylistDTO from '@/classes/addToPlaylistDTO';
import type PlaylistViewModel from '@/classes/playlistViewModel';
import type TrackViewModel from '@/classes/trackViewModel';
import axios from 'axios';
import { Notify, useDialogPluginComponent } from 'quasar';
import { onBeforeMount, ref } from 'vue';

  const props = defineProps<{
    tracks: TrackViewModel[];
  }>();

  const selectedPlaylist = ref<PlaylistViewModel | null>(null);
  const playlists = ref<PlaylistViewModel[]>([]);
  const playlistStatusCode = ref<number | null>(null);

    onBeforeMount(() => {
      console.log("Getting playlists");
      getPlaylists();
    });

    async function getPlaylists(){
      try {
        const response = await axios.get(`playlist/getplaylists?getuserplaylistsonly=true`,{
          headers: { "Content-Type": "application/json", "Accept": "application/json" }
        })
        playlists.value = []
        console.log(response.data);
        response.data.playlists.forEach(element => {
          playlists.value.push(element as PlaylistViewModel);
        });
        playlistStatusCode.value = response.status;
      } catch (error) {
        const ex = error as AxiosError;
        playlistStatusCode.value = ex.status || null;
      }
    }

  defineEmits([
    ...useDialogPluginComponent.emits
  ]);

  const {dialogRef, onDialogOK, onDialogCancel} = useDialogPluginComponent();

  async function onOK(){
    try{
      const addToPlaylistDTO = new AddToPlaylistDTO(selectedPlaylist.value?.id || "", props.tracks.map(t => t.id));
      await axios.post("playlist/addtoplaylist", {
        playlistId: selectedPlaylist.value?.id,
        trackIds: props.tracks.map(t => t.id)
      });

      Notify.create({
        message: 'Successfully added tracks to playlist.',
        color: 'green',
      });

      onDialogOK();
    }
    catch (error) {
      console.error(error);
      Notify.create({
        message: 'An error has occured while adding tracks to playlist.',
        color: 'red',
      });
    }
  }
</script>
