using ShowTractor.Plugins.Interfaces;
using System.Collections.Generic;

namespace ShowTractor.Plugins
{
    public interface IAggregateMediaSourceProvider
    {
        IAsyncEnumerable<MediaSource> GetMediaSourcesAsync(TvSeason season, TvEpisode episode);
    }
}
