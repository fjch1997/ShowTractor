using System.Collections.Generic;

namespace ShowTractor.Plugins.Interfaces
{
    public interface IMediaSourceProvider : IPlugin
    {
        IAsyncEnumerable<MediaSource> GetAsync(TvSeason tvSeason, TvEpisode tvEpisode);
    }
}
