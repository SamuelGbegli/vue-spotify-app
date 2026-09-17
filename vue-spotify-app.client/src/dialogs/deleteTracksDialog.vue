<template>
    <QDialog class="relative-position" ref="dialogRef" backdrop-filter="blur(4px)" persistent>
      <QCard>
        <QCardSection class="row items-center q-pb-none">
            <div class="text-h6">Remove tracks</div>
          </QCardSection>
        <QCardSection>
          <div class="text">
            Remove the following {{ props.tracks.length > 1 ? "tracks" : "track" }} from "{{ props.listName }}"? This is a permanent action.
          </div>
              <QTable v-if="props.tracks.length > 1"
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
            <TrackPreviewCard v-else :track="props.tracks[0]" :showLikedSongs="false"/>
          </QCardSection>

        <QCardActions align="right">
          <QBtn flat label="No" @click="onDialogCancel"/>
          <QBtn flat label="Yes" color="primary" @click="onOK" />
        </QCardActions>
    </QCard>
    </QDialog>
        
</template>

<script setup lang="ts">

    import TrackViewModel from '@/classes/trackViewModel';
    import { onBeforeMount, ref } from 'vue';
    import axios, { AxiosError } from 'axios';
    import { Notify, Loading, useDialogPluginComponent } from 'quasar';
import ConvertMilisecondsToMinutesAndSeconds from '@/helperFunctions/convertMilisecondsToMinutesAndSeconds';
import TrackPreviewCard from '@/components/TrackPreviewCard.vue';
import TrackListDTO from '@/classes/trackListDTO';

    const props = defineProps<{
        listID: string,
        listName: string,
      tracks: TrackViewModel[]
    }>();
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

  defineEmits([
  ...useDialogPluginComponent.emits
]);

const {dialogRef, onDialogOK, onDialogCancel} = useDialogPluginComponent();

async function onOK(){
  Loading.show({
    message: "Removing tracks..."
  });

  try{
    const trackListDTO = new TrackListDTO();
    trackListDTO.listID = props.listID;
    trackListDTO.trackIDs = props.tracks.map(t => t.id);
    await axios.delete("/api/tracklist/removetracksfromlist", {
                data: trackListDTO
            });
        
        Notify.create({
                message: `Successfully removed ${props.tracks.length != 1 ? `${props.tracks.length} tracks` : "track"} from list.`,
                color: "green"
                });
        onDialogOK();
              }
  catch (error) {
    Notify.create({
        message: `Error removing tracks.`,
        color: "red"
      });

  }
  finally{
    Loading.hide();
  }
}
</script>