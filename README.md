![Logo](ShowTractor.WinUI/ShowTractor.WinUI%20(Package)/Images/Wide310x150Logo.scale-200.png)

# ShowTractor

A TV show tracker program with highly extensible components that enables media download, IMDB integration, and more.
The main motivation over existing products are

1. One day, Episodecalendar.com was down, and I couldn't see which TV show I should watch until the next day.
2. This runs absolutely locally with complete control over user data.
3. Do not depend on the availability of cloud-based products.

Having a software that can run independtly of the cloud shouldn't be that big of a ask. But in 2020, there are no viable TV show tracker that does't run on the cloud.

# Technologies used

1. WinUI 3 (Windows App SDK 1.0)
2. C# 9
3. .NET 6 and .NET Standard 2.1
4. EntityFramework Core
5. SQLite

# Build and run

Open Solution in Visual Studio 2022. F5

# Requirements

- Metadata Provider Plugin
  - [x] A System that allows plugins to search for TV show metadata. Data Must include title, description, whether a season is a finale, list of all episode, air date and time of each episode, title of each episode, description of each episode.
  - Built-in Metadata Provider Plugin
    - [x] TMDB
    - [ ] IMDB
- Rating Source Plugin
  - [ ] Be able to view ratings from a Rating Source plugin.
  - [ ] Be able to set IMDB ratings if supported.
  - Built-in Rating Source plugin
    - [ ] IMDB 
    - [ ] Metacritic
- Download Source Plugin
  - [ ] A system that allows plugins to start downloading given a TV Episode.
  - [ ] A suggested download folder can be provided to the plugin but need not be honored by the plugin.
  - [ ] User can select from a list of download options.
  - [ ] A default resolution can be configured. Options are, 4K, 1080p, 720p, SD.
  - [ ] Downloaded file can be watched by the built-in media player or passed to another plugin for playback.
  - [ ] A local file download source plugin that discovers media files in a specified folder and make them available for a Media Player Plugin.
- Media Player Plugin
  - [ ] A system that allows plugins to stream episodes, or play downloaded media.
  - Built-in Media Player Plugin
    - [ ] Amazon Prime Video Streaming
    - [ ] Showtime Streaming
    - [ ] Local file playback of downloaded media
      - [ ] HEVC, AAC support.
      - [ ] MP4, MKV support.
  - [ ] Remember playback position.
- TV Show Browser
  - [x] Shall display TV shows with Artwork images.
  - [x] Shall display all information obtained in 2.
  - [ ] Rate a show or episode with a Rating Source.
  - [x] Display all unwatched episodes in one view.
  - [x] Display all followed episodes in a calendar view.
  - [x] Calendar item should indicate if an episode is watched.
  - [x] Add TV Show to followed shows from search result.
  - [x] Subscribe to new season when they become available.
- Miscellaneous
  - [ ] Archiving shows that are no longer active, so that they do not clutter the view.
  - [ ] Download progress shall be displayed.
  - [ ] Cleanup rules can be specified to delete downloaded files.
  - [ ] Automatic download can be configured.

# Local data file

When debugging in Visual Studio, data files are in `%LOCALAPPDATA%\ShowTractor`

When packaged, `%LOCALAPPDATA%\Packages\ff67f6ff-3707-446e-a79d-3e95f4d04f68_d7w9j395gsp6m\LocalCache\Local\ShowTractor`
