# vue-spotify-app

## Currently working on
- Investigating and optimising long database calls
## Overview
This is a web application designed to communicate with Spotify's web API and fetch information related to the user's listening habits. This was originally built to track when and how often music has been played by the user, and has since expanded into a tool to help make using Spotify easier.
## Current features
- Automated periodic syncing with the user's Spotify libraries, specifically Liked Songs and playlists
- Adding tracks to the user's queue (only works with Spotify Premium)
- Filtering and sorting of track lists
- Background fetching of tracks the user has most recently played, and viewing tracks grouped by number of times played
- Internal user-made lists of tracks
## How to run
The following is required for the application to work properly.

- An active Spotify Premium membership
- A Spotify Developer account
- .NET 10

The following instructions are for Visual Studio 2026.

1. Navigate to your Spotify Developer account and go to the dashboard.
2. Click on the "create app" button and ensure the following fields are filled out or selected:
	- App name
	- App description
	- Redirect URIs
	- Web API is selected
	- Spotify's Developer Terms of Service and Design Guidelines are marked as read
3. Clone the project to a desired local repository.
4. Open the vue-spotify-app solution in Visual Studio.
5. Create a database to store application records and sync the database to ensure all tables are created (e.g., with EF Core's Update-Database command in the console).
6. In the Solution Explorer window, find and right click on the project that says "vue-spotify-app.Server" and click on "Manage User Secrets"
7. In the secrets.json file, add the following values.
```
{
	"Spotify:ClientID": [your Spotify web app's Client ID]
	"ConnectionStrings:DatabaseContext": [your database connection string]
	"Spotify: RedirectURL" [your Spotify web app's redirect URL]
}
```
Note that the value of RedirectURL cannot be a localhost address.

To start the application, do the following steps.

- Server: click on start or press F5
- Client: in the command line, navigate to "vue-spotify-app.client" folder and run the command "npm run dev -- --host"
## Notes
### Playback history
The call to return recently played tracks will usually return a track if one of the following happens.

- A track reaches the end of playback
- Spotify is terminated
- The active listening device is switched

This could lead to any of these happening.

- A track appearing, even if little to none of it has been played
- A track not showing up if the end has not been reached.

In addition, the call returns up to the 50 most recently played tracks by the user, so tracks may not be recorded if the application is not running or a large number of tracks has been played.

### Spotify track redirecting
On occasion, a track saved to a Spotify list may redirect to another with a different track ID. This can result in the following.

- Tracks that sound (nigh)-identical and come from an alternate album
- "Substitute tracks", if the originally saved track is no longer available

To mitigate issues with tracks not appearing as played, an alias feature is used to pair tracks that may have multiple IDs for various reasons, mainly using the track's ISRC value. While this generally helps, there are instances where a track may be incorrectly matched or not matched, so manual inspection of values are recommended.
## Future and desired features
- Viewing albums
- More advanced playback history analysis
- Blacklisting and random playlist generation
- Standardised dialog box sizes
- Searches for albums, artists, playlists, etc.
# Noted glitches
