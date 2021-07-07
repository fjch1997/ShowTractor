using System.Collections.Generic;

namespace ShowTractor.Plugins.Interfaces
{
    /// <summary>
    /// Implementations of this interface are singleton and must be thread safe.
    /// </summary>
    public interface IMediaSourceProvider : IPlugin
    {
        /// <summary>
        /// Implementations of this interface are singleton and must be thread safe.
        /// </summary>
        IAsyncEnumerable<MediaSource> GetAsync(TvSeason tvSeason, TvEpisode tvEpisode);
    }
}
