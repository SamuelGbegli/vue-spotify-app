<template>
    <QDialog class="relative-position" ref="dialogRef" backdrop-filter="blur(4px)" persistent>
      <QCard>
        <QCardSection>
            <div class="text">{{ message }}</div>
        </QCardSection>
        <QCardActions align="right">
            <QBtn label="no" @click="onDialogCancel"/>
            <QBtn label="yes" @click="onDialogOK"/>
        </QCardActions>
        </QCard>
    </QDialog>
</template>
<script setup lang="ts">
import { useDialogPluginComponent } from 'quasar';


    const props = defineProps<{
        message: string
    }>();

defineEmits([
  ...useDialogPluginComponent.emits
]);

const {dialogRef, onDialogOK, onDialogCancel} = useDialogPluginComponent();

    
</script>