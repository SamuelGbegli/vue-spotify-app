import AddTrackToQueue from '@/pages/AddTrackToQueue.vue'
import GetTotalSavedTracks from '@/pages/GetTotalSavedTracks.vue'
import GetTrackAndAlbumCover from '@/pages/GetTrackAndAlbumCover.vue'
import GetTracksPlayedPerDay from '@/pages/GetTracksPlayedPerDay.vue'
import MainPage from '@/pages/MainPage.vue'
import NotFound from '@/pages/NotFound.vue'
import PlaybackRecords from '@/pages/PlaybackRecords.vue'
import PlaylistPage from '@/pages/PlaylistPage.vue'
import ReviewPendingPlaybackRecords from '@/pages/ReviewPendingPlaybackRecords.vue'
import SearchPage from '@/pages/SearchPage.vue'
import SearchTracks from '@/pages/SearchTracks.vue'
import ViewPlaylistPage from '@/pages/ViewPlaylistPage.vue'
import ViewTrackPage from '@/pages/ViewTrackPage.vue'
import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'Liked Songs',
      component: MainPage,
      meta: {
        title: "Liked Songs",
      }
    },
    {
      path: '/trackcount',
      component: GetTotalSavedTracks,
    },
    {
      path: '/gettracksperday',
      component: GetTracksPlayedPerDay,
    },
    {
      path: '/gettrackandalbumcover',
      component: GetTrackAndAlbumCover,
    },
    {
      path: "/playbackrecords",
      component: PlaybackRecords,
      meta: {
        title: "Playback Records",
      }
    },
    {
      path: "/addtoqueue",
      component: AddTrackToQueue,
      meta: {
        title: "Add track to queue",
      }
    },
    {
      path: "/playlists",
      component: PlaylistPage,
      meta: {
        title: "Playlists",
      }
    },
    {
      path: "/playlists/:id",
      component: ViewPlaylistPage
    },
    {
      path: "/reviewpendingrecords",
      component: ReviewPendingPlaybackRecords
    },
    {
      path: "/searchfortrackinplaylist",
      component: SearchTracks
    },
    {
      path: "/viewtrack/:id",
      component: ViewTrackPage
    },
    {
      path: "/search",
      component: SearchPage,
      meta: {
        title: "Search",
      }
    },
    {
      path: '/:pathMatch(.*)*',
      component: NotFound,
      meta: {
        title: "Page Not Found",
      }
    },
  ],
})

export default router
