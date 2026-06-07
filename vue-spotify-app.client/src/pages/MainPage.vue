<!-- Page shows all tracks in the user's Liked Songs library. -->

<template>
  <div class="q-ma-sm  q-gutter-sm">
    <div class="text-h3">Liked tracks</div>
<TrackListNew/>
  </div>
</template>
<script setup lang="ts">
import TrackList from '@/components/TrackList.vue'
import TrackListNew from '@/components/TrackListNew.vue';
</script>
