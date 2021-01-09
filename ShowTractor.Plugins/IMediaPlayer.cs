using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShowTractor.Plugins.Interfaces
{
    public interface IMediaPlayer : IPlugin
    {
        public IEnumerable<MediaSourceType> SupportedTypes { get; set; }
        /// <summary>
        /// Get a ControlTemplate that can be loaded as a player. The actual underlying type depends on the platform.
        /// </summary>
        /// <returns></returns>
        public ValueTask<object> GetPlayerAsync(MediaSource source, TvSeason tvSeason, TvEpisode tvEpisode, Action closePlayerCallback);
    }
}
