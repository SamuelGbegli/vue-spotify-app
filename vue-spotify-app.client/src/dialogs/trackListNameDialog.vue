<template>
    <QDialog class="relative-position" ref="dialogRef" backdrop-filter="blur(4px)" persistent>
            <QCard>
                <QCardSection class="row items-center q-pb-none">
                    <div class="text-h6">{{ listID ? "Edit" : "Add" }} track list</div>
                    <QSpace/>
                    <QBtn icon="close" flat dense round v-close-popup />
                </QCardSection>
                <QCardSection>
                    <QInput label="List name" v-model="listName" :rules="inputRules"/>
                </QCardSection>
                <QCardActions align="right">
                    <QBtn :disable="errorMessage != ''" :label="listID ? 'Edit' : 'Add'" @click="onSubmit"/>
                </QCardActions>
            </QCard>
    </QDialog>
</template>
<script setup lang="ts">
import axios, { AxiosError } from 'axios';
import { Loading, Notify, useDialogPluginComponent } from 'quasar';
import { onBeforeMount, ref } from 'vue';


const props = defineProps<{
    listID: string | null | undefined;
    existingListName: string | null | undefined;
}>();

const listName = ref("");
const errorMessage = ref<string | null>();

defineEmits([
    ...useDialogPluginComponent.emits
]);

const {dialogRef, onDialogOK, onDialogCancel} = useDialogPluginComponent();

onBeforeMount(() => {
    if(props.existingListName)
        listName.value = props.existingListName
});

const inputRules = [
    async (val) => (await validateInput(val) == "" || errorMessage.value)    
];

async function validateInput(val: string) {
    try {
        const response = await axios.post("/api/tracklist/validatelistname", val, {
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json"
      }
    });
        errorMessage.value = response.data;
        console.log(response);
    } catch (error) {
        errorMessage.value = "Could not validate. Try again later.";
        console.log(error as AxiosError);
    }
    
    return errorMessage.value;
}

async function onSubmit() {
    console.log
    Loading.show({
        message: props.listID ? "Updating list..." : "Adding list..."
    })
    try {
        const response = props.listID ? await axios.patch(`/api/tracklist/updatetracklistname/${props.listID}`, listName.value, {
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json"
      }
    })
            : await axios.post(`/api/tracklist/addtracklist`, listName.value, {
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json"
      }
    });

        Notify.create({
            message: props.listID ? "Successfully renamed track list." : "Successfully added track list."
        });
    onDialogOK(listName.value);
    } catch (error) {
        Notify.create({
            message: props.listID ? "Error updating track list." : "Error adding track list",
            color: "red"
        });
    }
    finally{
        Loading.hide();
    }
}

</script>