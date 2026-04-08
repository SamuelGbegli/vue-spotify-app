<template>
  <QDialog ref="dialogRef" backdrop-filter="blur(4px)">
    <QCard class="q-dialog-plugin">
      <QCardSection class="row items-center q-pb-none">
        <div class="text-h6">Sort and Filter Tracks</div>
        <QSpace/>
        <QBtn icon="close" flat round dense v-close-popup/>
      </QCardSection>
      <QCardSection>
        <QInput
          v-model="filter.query"
          label="Query"/>
          <QCheckbox v-model="filter.searchName" label="Name"/>
          <QCheckbox v-model="filter.searchArtist" label="Artists"/>
          <QCheckbox v-model="filter.searchAlbum" label="Album"/>
      </QCardSection>
      <QCardSection>
        <div class="row">
          <div class="col">
            <QInput
              v-model="filter.dateRangeFrom"
              label="From"
              type="date"
            />
            </div>
          <div class="col">
            <QInput
              v-model="filter.dateRangeTo"
              label="To"
              type="date"
            />
          </div>
        </div>
      </QCardSection>
      <QCardSection>
        <QSelect v-model="selectedSortType"
          :options="sortTypes"
          label="Sort by"/>
        <QSelect v-model="selectedSortOrder"
          :options="sortOrders"
          label="Sort order"/>
      </QCardSection>
        <QCardActions align="right">
          <QBtn flat label="Clear" color="primary" @click="() => filter = new TrackFilter()" />
          <QBtn flat label="OK" color="primary" @click="onOK" />
        </QCardActions>
    </QCard>
  </QDialog>
</template>

<script setup lang="ts">
import TrackFilter from '@/classes/trackFilter';
import { useDialogPluginComponent } from 'quasar';
import { onBeforeMount, ref } from 'vue';

const props = defineProps<{
  currentFilter: TrackFilter | null
}>();

const sortTypes = ["Name", "Artists", "Album", "Track length", "Date added", "Date last played"];
const sortOrders = ["Ascending", "Descending"];

const selectedSortType = ref(sortTypes[props.currentFilter != null ? props.currentFilter.sortType : 0]);
const selectedSortOrder = ref(sortOrders[props.currentFilter != null ? props.currentFilter.sortOrder : 0]);

const filter = ref<TrackFilter>(new TrackFilter());

onBeforeMount(() => {
  if (props.currentFilter != null)
    filter.value = props.currentFilter;
})

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
