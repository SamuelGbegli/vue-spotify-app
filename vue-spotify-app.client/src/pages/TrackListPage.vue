<template>
    <div class="col q-pa-md">
        <!--Title-->
        <div class="row q-mb-md q-gutter-sm">
            <div class="text-h4">Track Lists</div>
            <QSpace/>
            <QBtn label="Filter" color="primary"/>
            <QBtn label="Add new list" color="primary" @click="openListDialog(null)"/>
        </div>
        <QTable
            :columns="tableColumns"
            :rows="trackLists"
            row-key="ID"
            wrap-cells
            v-model:pagination="pagination"
            :loading="statusCode === null"
            hide-pagination>
            <!--Column for track options-->
            <template v-slot:body-cell-dateCreated="props">
                <QTd>
                    {{ date.formatDate(props.row.dateCreated, "Do MMM YYYY HH:mm") }}
                </QTd>
            </template>
            <template v-slot:body-cell-dateLastModified="props">
                <QTd>
                    {{ date.formatDate(props.row.dateModified, "Do MMM YYYY HH:mm") }}
                </QTd>
            </template>
          <template v-slot:body-cell-actions="props">
            <QTd :props="props" align="center">
                <QBtnDropdown
                    color="primary"
                    size="md"
                    label="View"
                    split
                    aria-haspopup="menu"
                    dense
                    :to="`tracklists/${props.row.id}`">
                    <QList role="menu">
                        <QItem clickable v-close-popup @click="openListDialog(props.row)">
                            <QItemSection>
                                <QItemLabel>Edit</QItemLabel>
                            </QItemSection>
                        </QItem>
                        <QItem clickable v-close-popup @click="deleteList(props.row)">
                            <QItemSection>
                                <QItemLabel>Delete</QItemLabel>
                            </QItemSection>
                        </QItem>
                    </QList>
                    </QBtnDropdown>
                </QTd>
            </template>
            <template v-slot:bottom>
                <QSpace />
                <QPagination v-model="pagination.page"
                                :max="Math.ceil(pagination.rowsNumber / pagination.rowsPerPage)"
                                size="sm"
                                @update:model-value="getTrackLists()"
                                input />
            </template>
            <template v-slot:loading>
                <QInnerLoading showing size="50px" color="green" />
            </template>
        </QTable>
    </div>
</template>
<script setup lang="ts">
import TrackListViewModel from '@/classes/TrackListViewModel';
import ConfirmationDialog from '@/dialogs/confirmationDialog.vue';
import TrackListNameDialog from '@/dialogs/trackListNameDialog.vue';
import axios, { AxiosError } from 'axios';
import { date, Dialog, Notify } from 'quasar';
import { onBeforeMount, ref } from 'vue';

const trackLists = ref<TrackListViewModel[]>([]);

const statusCode = ref<number | null>(null);

const tableColumns = [
    {
        name: "name",
        label: "Name",
        field: "name",
        align: "left",
        sortable: true,
        style: "width: 20%"
    },
    {
        name: "dateCreated",
        label: "Date Created",
        field: (row: TrackListViewModel) => row.dateCreated,
        align: "left",
        sortable: true,
        style: "width: 20%"
    },
    {
        name: "dateLastModified",
        label: "Date Last Modified",
        field: (row: TrackListViewModel) => row.dateModified,
        align: "left",
        sortable: true,
        style: "width: 20%"
    },
    {
        name: "actions",
        sortable: false,
    }
];

const pagination = ref({
    page: 1,
    rowsPerPage: 20,
    rowsNumber: 0
});

onBeforeMount(async () => {
    await getTrackLists();
});

async function getTrackLists() {
    statusCode.value = null;
    try {
        const response = await axios.get(`/api/tracklist/gettracklists?offset=${(pagination.value.page - 1)}&numberOfLists=${pagination.value.rowsPerPage}`);
        pagination.value.rowsNumber = response.data.count as number;
        trackLists.value = response.data.trackLists as TrackListViewModel[];
        statusCode.value = response.status;
        console.log(trackLists.value);
    }
    catch (error) {
        const axiosError = error as AxiosError;
        console.log(axiosError);
        statusCode.value = axiosError.status;
    }
}

async function openListDialog(trackList: TrackListViewModel | null){
    console.log(trackList?.id);
    Dialog.create({
        component: TrackListNameDialog,
        componentProps: {
            listID: trackList?.id,
            existingListName: trackList?.name
        }
    }).onOk(async (data) => {
        console.log(data);
        if(!trackList){
            await getTrackLists();
        }
        else {
            trackList.name = data;
            trackList.dateModified = new Date();
        }
    }
    );
}

async function deleteList(trackList: TrackListViewModel) {
    Dialog.create({
        component: ConfirmationDialog,
        componentProps: {
            message: `This will permanently remove ${trackList.name} and all linked tracks. Continue?`
        }
    }).onOk(async () => {
        try {
            await axios.delete(`/api/tracklist/deletetracklist/${trackList.id}`);           
        statusCode.value = null;
            await getTrackLists();
        } catch (error) {
            Notify.create({
                message: "Could not delete list.",
                color: "red"
            });
        }
    })
}

</script>