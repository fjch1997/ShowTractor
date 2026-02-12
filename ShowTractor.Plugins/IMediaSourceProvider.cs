using System.Collections.Generic;

namespace ShowTractor.Plugins.Interfaces
{
    public interface IMediaSourceProvider : IPlugin
    {
        bool CanFetchDownloadableSources(TvSeason season, TvEpisode episode);
        bool CanFetchPlayableSources(TvSeason season, TvEpisode episode);
        IEnumerable<MediaSource> GetMediaSources(TvSeason season, TvEpisode episode);
        IAsyncEnumerable<MediaSource> FetchMediaSourcesAsync(TvSeason season, TvEpisode episode);
    }
}
