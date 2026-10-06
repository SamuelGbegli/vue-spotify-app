<template>
    <QDialog ref="dialogRef" backdrop-filter="blur(4px)" class="relative-position">
        <QCard>
            <QCardSection class="row items-center">
                <div class="text-h6">Add tracks to list</div>
                <QSpace/>
                <QBtn icon="close" flat round dense v-close-popup/>
            </QCardSection>
            <QForm @submit.prevent="onOK">
                <QCardSection>
                    <div class="text">Enter the IDs of any tracks to add to the list. Separate IDs with a new line.</div>
                    <QInput
                        v-model="trackIDs"
                        label="Track IDs"
                        outlined
                        dense
                        required
                        type="textarea"
                        />
                </QCardSection>
                <QCardActions align="right">
                    <QBtn type="submit" label="Add" color="primary"/>
                </QCardActions>
            </QForm>
        <QInnerLoading :showing="loading"></QInnerLoading>
        </QCard>
    </QDialog>
</template>

<script setup lang="ts">
import TrackListDTO from '@/classes/trackListDTO';
import axios from 'axios';
import { Notify, useDialogPluginComponent } from 'quasar';
import { ref } from 'vue';


    const props = defineProps<{
        listID: string
    }>();

    const trackIDs = ref("");
    const loading = ref(false);

    defineEmits([
        ...useDialogPluginComponent.emits
    ]);

    const {dialogRef, onDialogOK, onDialogCancel} = useDialogPluginComponent();

    async function onOK() {
        loading.value = true;

        const dto = new TrackListDTO();
        dto.listID = props.listID;
        dto.trackIDs = trackIDs.value.split("\n").map(id => id.trim());

        try {
            const response = await axios.post("/api/tracklist/addtrackstolist", dto);
            console.log(response);
            if(response.data.addedTracks === 0)
                Notify.create({
                    message: "No tracks were added. Check the IDs are valid and try again.",
                    color: "amber"
                });
            else {
                if (response.data.addedTracks === dto.trackIDs.length)
                    Notify.create({
                    message: "Successully added all tracks to list.",
                    color: "green"
                });
                else Notify.create({
                    message: `Added ${response.data.addedTracks} track(s) out of ${dto.trackIDs.length}. Check the list to see what wasn't added and ensure their IDs are valid.`,
                    color: "green"
                });

                onDialogOK();
            }
        }
        catch(ex) {
            console.error(ex);
            Notify.create({
                message: "An error has occured. Please try again later.",
                color: "red"
            });
        }
        finally{
            loading.value = false;
        }
    }

</script>