<template>
    <QCard flat>
        <QCardSection horizontal>
            <QCardSection>
                <QImg :src="track.albumCover" style="width: 100px; height: 100px;" />
            </QCardSection>
            <QCardSection>
                <div class="text-h6"><span>{{ track.name }} ({{ ConvertMilisecondsToMinutesAndSeconds(track.length) }})</span></div>
                    <div class="text-subtitle2">
                        <span v-for="x in track.artists" :key="x.id">
                            {{ x.name }}
                            <span v-if="track.artists.indexOf(x) < track.artists.length - 1">, </span>
                        </span>
                    </div>
                    <div class="text-subtitle2">{{track.albumName}}</div>
            </QCardSection>
        </QCardSection>
    </QCard>
        
</template>
<script setup lang="ts">

    import TrackViewModel from '@/classes/trackViewModel';
    import ConvertMilisecondsToMinutesAndSeconds from '@/helperFunctions/convertMilisecondsToMinutesAndSeconds';

    const props = defineProps<{
        track: TrackViewModel;
    }>();
</script>