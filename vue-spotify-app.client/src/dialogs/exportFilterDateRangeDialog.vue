<template>
  <QDialog ref="dialogRef" persistent backdrop-filter="blur(4px)">
    <QCard class="q-dialog-plugin">
      <QCardSection>
        <div class="text-h6">Select date range to export</div>
      </QCardSection>

      <QCardSection>
        <q-input
          v-model="startDate"
          label="From"
          type="date"
        />
        <q-input
          v-model="endDate"
        label="To"
          type="date"
        />
      </QCardSection>

      <QCardActions align="right">
        <QBtn flat label="Cancel" color="primary" @click="onDialogCancel" />
        <QBtn flat label="Apply" color="primary" @click="onOK" />
      </QCardActions>
    <QInnerLoading :showing="loading"/>
    </QCard>
  </QDialog>
</template>

<script setup lang="ts">

import axios from 'axios';
import { date, Notify, useDialogPluginComponent } from 'quasar';
import { onMounted, ref } from 'vue';

const props = defineProps<{
  startDateProp: string | null;
  endDateProp: string | null;
}>();

const startDate = ref<string | null>(null);
const endDate = ref<string | null>(null);
const loading = ref(false);

onMounted(() => {
  startDate.value = props.startDateProp;
  endDate.value = props.endDateProp;
});

defineEmits([
  ...useDialogPluginComponent.emits
]);

const {dialogRef, onDialogOK, onDialogCancel} = useDialogPluginComponent();


async function onOK(): Promise<void>{
  loading.value = true;
  try{
    // Sets up the search parameters for the API call
    const searchParams = new URLSearchParams();
    // Adds the start date to the search parameters if it is set
    if (startDate.value !== null) searchParams.append("startDate", startDate.value);
    // Adds the end date to the search parameters if it is set
    if (endDate.value !== null) searchParams.append("endDate", endDate.value);
    const response = await axios.get<Blob>(
      `/api/playbackrecord/exporttop100${startDate.value || endDate.value ? "?" +  searchParams.toString() : ""}`,
      {
        responseType: 'blob'
      }
    )

    const url = window.URL.createObjectURL(response.data);
    const link = document.createElement("a");

    

    link.href = url;
    link.download = `spotify-playack-analysis ${date.formatDate(new Date(), "YYYY-MM-DD mm-ss-SSS")}`
    document.body.appendChild(link);

    try{
      link.click();
    }
    finally{
      link.remove();
      setTimeout(() => {
        window.URL.revokeObjectURL(url)
      }, 1000);
    }
    onDialogOK({startDate: startDate.value, endDate: endDate.value});
  }
  catch(ex){
    loading.value = false;
    Notify.create({
      message: "Could not export data. Please try again."
    })
  }
  
}
  </script>
