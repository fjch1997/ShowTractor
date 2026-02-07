using System.Collections.Generic;

namespace ShowTractor.Plugins.Interfaces
{
    public interface IMediaSourceProvider : IPlugin
    {
        bool CanProvide(TvSeason season, TvEpisode episode);
        IAsyncEnumerable<MediaSource> GetMediaSourcesAsync(TvSeason season, TvEpisode episode);
    }
}
