# vue-spotify-app

## Current development status
- Working on a refined page to search for items on Spotify, including tracks, albums and playlists
## Overview
This is a web application designed to integrate with Spotify's web API and fetch and save data relating to the user's listening habits, as well as view information relating to tracks on the Spotify platform.
## Development stack
- C#
- ASP.NET MVC
- Entity Framework Core
- Vue
- TypeScript
- SQL Server
## Background
I developed this application to help explore my listening habits on Spotify, as I typically listen to music that's shuffled in my Liked Songs library, as well as create a more automated flow of recording playback history as I work on other tasks.

While I cannot confirm for sure, part of me suspected that Spotify's default shuffling methods used some sort of algorithm when generating a queue, so my main goal was to expose tracks I have saved that are rarely played, or have not been played in a long time.

In addition, I am solving the following issues I have had when using Spotify:

- knowing how often tracks are played in a period of time
- recalling certain tracks that were played at some point
- searching for and finding tracks with additional filters
## Current features
- Viewing and caching data from the user's Liked Songs library and created playlists, including filtering and sorting
- Automated fetching of the user's playback history, and compiling playback records into an all-time record
- Semi-automated track aliasing, matching tracks with different Spotify IDs that are deemed to be the same
- Adding individual tracks to the user's Spotify queue (only works if the user has Spotify Premium)
## Planned and desired features
- Viewing albums within the application
- Highlighting a track's playback records as being from an alias
- Average and overall gaps between a track being played
- Searching for artists, albums, playlists, and so on in addition to tracks
