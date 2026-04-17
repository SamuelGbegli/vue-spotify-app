<script setup lang="ts">
  import axios from 'axios'
  import { useAuthStore } from './stores/authStore'
  import { useRoute } from 'vue-router'
  import { onMounted, ref } from 'vue'
  import { date, type QItemSection } from 'quasar';

  const rightDrawerOpen = ref(false);

  const authStore = useAuthStore()
  const route = useRoute()

  function toggleRightDrawer() {
    rightDrawerOpen.value = !rightDrawerOpen.value;
  }

  async function redirectToSpotifyAuthorise() {
    const response = await axios.get("auth/redirect", {
      headers: {
        "Content-Type": "application/json", "Accept": "application/json"
      }
    });

    window.location.href = response.data;
    console.log(response)
  }


  async function loginWithSpotifyClick() {
    await redirectToSpotifyAuthorise()
  }

  async function logoutClick() {
  authStore.logout()
    try {
      await axios.get("/auth/logout");
    }
    catch (error) {
      console.log(error as AxiosError);
      alert("Error logging out. Please try again.");
    }
  }


</script>

<template>
  <div v-if="authStore.loggedIn != '200'">
    <h3>Unauthenticated</h3>
    <p>You need to be logged in to view this resource.</p>
    <q-btn label="Login" @click="loginWithSpotifyClick()"/>
  </div>
  <div v-else>
    <q-layout view="hhh lpR fFf">
      <q-header class="bg-green text-white">
        <q-toolbar>
          <q-toolbar-title>
            Vue Spotify Portal
          </q-toolbar-title>
          <q-btn dense flat round icon="menu" @click="toggleRightDrawer" />
        </q-toolbar>
      </q-header>
      <QDrawer v-model="rightDrawerOpen" side="right" overlay bordered>
        <QScrollArea class="fit">
          <QList>
            <QItem clickable to="/">
              <QItemSection>
                Main page
              </QItemSection>
            </QItem>
            <QItem clickable to="/playlists">
              <QItemSection>
                Playlists
              </QItemSection>
            </QItem>
            <QItem clickable to="/playbackrecords">
              <QItemSection>
                Playback records
              </QItemSection>
            </QItem>
            <QItem clickable to="/addtoqueue">
              <QItemSection>
                Add tracks to queue
              </QItemSection>
            </QItem>
            <QItem clickable to="/reviewpendingrecords">
              <QItemSection>
                Review pending track records
              </QItemSection>
            </QItem>
            <QItem clickable to="/searchfortrackinplaylist">
              <QItemSection>
                Search for tracks in playlist
              </QItemSection>
            </QItem>
            <QItem clickable @click="logoutClick()">
              <QItemSection>
                Log out
              </QItemSection>
            </QItem>
          </QList>
        </QScrollArea>
      </QDrawer>
      <q-page-container>
      <router-view/>
      </q-page-container>
    </q-layout>
  </div>
</template>

<style scoped></style>
