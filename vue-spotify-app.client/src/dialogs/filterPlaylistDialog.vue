<template>
    <QDialog ref="dialogRef" persistent backdrop-filter="blur(4px)">
        <QCard class="q-dialog-plugin">
            <QCardSection class="row items-center q-pb-none">
                <div class="text-h6">Sort and Filter Playlists</div>
                <QSpace/>
                <QBtn icon="close" flat round dense v-close-popup/>
            </QCardSection>
            <QForm v-on:submit.prevent="onOK">
                <QCardSection>
                    <QInput
                        v-model="filter.query"
                        label="Query"/>
                    <QItem tag="label">
                    <QItemSection>
                        <QItemLabel>Exclude playlists made by other users</QItemLabel>                    
                    </QItemSection>
                    <QItemSection avatar>
                        <QToggle v-model="filter.returnUserPlaylistsOnly"/>
                    </QItemSection>
                </QItem>
        <QSelect v-model="selectedSortType"
          :options="sortTypes"
          label="Sort by"/>
        <QSelect v-model="selectedSortOrder"
          :options="sortOrders"
          label="Sort order"/>
                </QCardSection>
                <QCardActions align="right">
                    <QBtn type="reset" flat label="Clear" color="primary" @click="() => filter = new PlaylistFilter()" />
                    <QBtn type="submit" flat label="OK" color="primary" />
                </QCardActions>
            </QForm>
        </QCard>
    </QDialog>
</template>
<script setup lang="ts">
import PlaylistFilter from '@/classes/playlistFilter';
import { useDialogPluginComponent } from 'quasar';
import { onBeforeMount, ref } from 'vue';

const props = defineProps<{
  currentFilter: PlaylistFilter | null
}>();

const sortTypes = ["Name", "Number of tracks"];
const sortOrders = ["Ascending", "Descending"];

const selectedSortType = ref(sortTypes[props.currentFilter != null ? props.currentFilter.sortType : 0]);
const selectedSortOrder = ref(sortOrders[props.currentFilter != null ? props.currentFilter.sortOrder : 0]);

const filter = ref<PlaylistFilter>(new PlaylistFilter());

onBeforeMount(() => {
    if(props.currentFilter){
        filter.value = props.currentFilter
    }
});

    defineEmits([
        ...useDialogPluginComponent.emits
    ]);

    const {dialogRef, onDialogOK, onDialogCancel} = useDialogPluginComponent();

    function onOK(){
        filter.value.sortType = sortTypes.indexOf(selectedSortType.value);
        filter.value.sortOrder = sortOrders.indexOf(selectedSortOrder.value);
        onDialogOK(filter.value);
    }
</script>