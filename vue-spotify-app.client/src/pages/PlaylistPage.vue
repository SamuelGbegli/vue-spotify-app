<template>
  <div class="q-pa-md">
    <div class="text-h3">Playlists</div>
    <QTable :title="(statusCode === 200 ? `Total playlists: ${pagination.rowsNumber}` : statusCode === null ? 'Loading...' : 'Failed to load playlists.')"
            :columns="columns"
            :rows="playlists"
            row-key="id"
            wrap-cells
            :loading="statusCode === null"
            v-model:pagination="pagination"
            style="height: 85vh;">
      <template v-slot:top>
        <div class="col q-pa-sm q-gutter-sm">
          <div class="row">
            <div class="q-table__title" v-if="playlists.length > 0">Total playlists: {{ pagination.rowsNumber }}</div>
            <QSpace />
            <QBtn label="Filter and sort playlists" color="primary" @click="openFilterAndSortDialog" />
          </div>
          <div class="row items-center">
            <!--Section for filter chips-->
          </div>
        </div>
      </template>
      <template v-slot:body-cell-image="props">
        <QTd :props="props">
          <QImg :src="props.row.imageLink"
                :alt="`Image cover for Spotify playlist ${props.row.name} by ${props.row.creator}`"
                width="48px" />
        </QTd>
      </template>
      <template v-slot:body-cell-creator="props">
        <QTd :props="props">
          <div class="row q-gutter-md items-center">
            <span>{{ props.row.ownerName }}</span>
            <QBtn title="View owner on Spotify" icon="link" flat round size="sm" />
          </div>
        </QTd>
      </template>
      <template v-slot:body-cell-actions="props">
        <QTd :props="props">
          <QBtnDropdown v-if="props.row.isUserMadePlaylist"
                        split
                        size="sm" color="secondary" label="view" :to="`/playlists/${props.row.id}`">
            <QList role="menu">
              <QItem v-close-popup clickable :to="props.row.externalURL">
                <QItemSection>
                  <QItemLabel>View in Spotify</QItemLabel>
                </QItemSection>
              </QItem>
            </QList>
          </QBtnDropdown>
          <QBtn v-else size="sm" color="red" label="View in Spotify" :to="props.row.externalURL" />
        </QTd>
      </template>
      <template v-slot:bottom>
        <QSpace />
        <QPagination v-model="pagination.page"
                     :max="Math.ceil(pagination.rowsNumber/pagination.rowsPerPage)"
                     size="sm"
                     @update:model-value="getPlaylists()"
                     input />
      </template>
      <!--Shows loading spinner when table is loading-->
      <template v-slot:loading>
        <QInnerLoading showing size="50px" color="green" />
      </template>
    </QTable>
  </div>
</template>
<script setup lang="ts">import PlaylistFilter from '@/classes/playlistFilter';
import PlaylistViewModel from '@/classes/playlistViewModel';
import FilterPlaylistDialog from '@/dialogs/filterPlaylistDialog.vue';
import SortOrder from '@/enumClasses/sortOrder';
import { useAuthStore } from '@/stores/authStore';
import { matQueryBuilder } from '@quasar/extras/material-icons';
import axios, { AxiosError } from 'axios';
import { Dialog, Loading } from 'quasar';
import { onBeforeMount, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

    const authStore = useAuthStore();
  const route = useRoute();
  const router = useRouter();

  const numberOfPlaylists = ref<number | null>(null)
  const playlists = ref<PlaylistViewModel[]>([]);

  const filter = ref<PlaylistFilter>(new PlaylistFilter());

  const pageOffset = ref<number>(1)

  const statusCode = ref<number | null>(null)

  // Represents the columns to be displayed in the table.
  const columns = [
    // Shows the playlist's image, if applicable.
    {
      name: "image",
      field: "imageLink",
      align: "left",
      sortable: false,
      style: "width: 50px"
    },
    {
      name: "name",
      label: "Name",
      field: "name",
      align: "left",
      sortable: true
    },
    {
      name: "creator",
      label: "Creator",
      field: "ownerName",
      align: "left",
      sortable: true
    },
    {
      name: "numberOfTracks",
      label: "No. of tracks",
      field: "numberOfTracks",
      align: "left",
      sortable: true
    },
    {
      name: "actions",
      align: "left",
      sortable: false
    },
  ];

  const pagination = ref({
    sortBy: "name",
    page: 1,
    rowsPerPage: 20,
    rowsNumber: 0
  });

    onBeforeMount(async () => {
      await onRouteUpdate();
    })

    watch(route, async () => {
        await onRouteUpdate();
      }
    )

      async function onRouteUpdate() {
        if (route.query.page) pagination.value.page = parseInt(route.query.page.toString());
        if (route.query.query) filter.value.query = route.query.query.toString();
        if (route.query.returnUserPlaylistsOnly) filter.value.returnUserPlaylistsOnly = route.query.returnUserPlaylistsOnly.toString() === "true";
        if (route.query.sort) filter.value.sortType = parseInt(route.query.sort.toString());
        if (route.query.order) filter.value.sortOrder = parseInt(route.query.order.toString());
        await getPlaylists();
      }

      async function updateFilter(){
        const query = new URLSearchParams();
        query.append("page", pagination.value.page.toString());
        if (filter.value.query !== null && filter.value.query.match(/^ *$/) == null) query.append("query", filter.value.query);
        query.append("returnUserPlaylistsOnly", filter.value.returnUserPlaylistsOnly.toString());
        query.append("sort", filter.value.sortType.toString());
        query.append("order", filter.value.sortOrder.toString());
        router.push(`/playlists/?${query.toString()}`);

        await getPlaylists();
      }

    async function getPlaylists() {
      pageOffset.value = route.query.page? parseInt(route.query.page.toString()) : 1;

      statusCode.value = null;

      try {

        const bodyContent = {
          page: pagination.value.page,
          numberOfPlaylists: pagination.value.rowsPerPage,
          query: filter.value.query,
          returnUserPlaylistsOnly: filter.value.returnUserPlaylistsOnly,
          sortOrder: filter.value.sortOrder,
          sortType: filter.value.sortType
        }

        console.log(bodyContent);

        const response = await axios.post(`/api/playlist/getplaylists`, bodyContent);
        playlists.value = []
        numberOfPlaylists.value = response.data.totalPlaylists;
        pagination.value.rowsNumber = response.data.totalPlaylists;
        response.data.playlists.forEach(element => {
          playlists.value.push(element as PlaylistViewModel);
        });
        playlists.value = response.data.playlists as PlaylistViewModel[];
        statusCode.value = response.status;
        console.log(numberOfPlaylists.value);
      } catch (error) {
        const ex = error as AxiosError;
        statusCode.value = ex.status || null;
      }

    }

function openFilterAndSortDialog() {
  Dialog.create({
    component: FilterPlaylistDialog,
    componentProps: {
      currentFilter: filter.value
    }
  }).onOk(async (data) => {
    console.log(data);
    filter.value = data;
    pagination.value.page = 1;
    await updateFilter();
  })
}
  </script>
