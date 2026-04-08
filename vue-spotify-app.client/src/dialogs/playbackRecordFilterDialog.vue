<template>
  <QDialog ref="dialogRef" backdrop-filter="blur(4px)">
    <QCard class="q-dialog-plugin">
      <QCardSection>
        <div class="text-h6">Filter Playback Records</div>
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
    </QCard>
  </QDialog>
</template>

<script setup lang="ts">

import { useDialogPluginComponent } from 'quasar';
import { onMounted, ref } from 'vue';

const props = defineProps<{
  startDateProp: string | null;
  endDateProp: string | null;
}>();

const startDate = ref<string | null>(null);
const endDate = ref<string | null>(null);

onMounted(() => {
  startDate.value = props.startDateProp;
  endDate.value = props.endDateProp;
});

defineEmits([
  ...useDialogPluginComponent.emits
]);

const {dialogRef, onDialogOK, onDialogCancel} = useDialogPluginComponent();


function onOK(){
  onDialogOK({startDate: startDate.value, endDate: endDate.value});
}
  </script>
