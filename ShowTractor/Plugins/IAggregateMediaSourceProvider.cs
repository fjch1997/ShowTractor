using ShowTractor.Plugins.Interfaces;
using System.Collections.Generic;

namespace ShowTractor.Plugins
{
    internal interface IAggregateMediaSourceProvider
    {
        IAsyncEnumerable<MediaSource> GetAsync(TvSeason tvSeason, TvEpisode tvEpisode);
    }
}