using ShowTractor.Plugins.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShowTractor.Tests.TestPlugins
{
    internal class TestMediaSourceProvider : IMediaSourceProvider
    {
        public string Name => "TestMediaSourceProvider";

        public PluginSettingsDescriptions? PluginSettingsDescriptions => null;

        public bool CanFetchDownloadableSources(TvSeason season, TvEpisode episode)
        {
            return true;
        }

        public bool CanFetchPlayableSources(TvSeason season, TvEpisode episode)
        {
            return true;
        }

        public IAsyncEnumerable<MediaSource> FetchMediaSourcesAsync(TvSeason season, TvEpisode episode)
        {
            return AsyncEnumerable.Empty<MediaSource>();
        }

        public Stream GetIconStream() => IPlugin.GetDefaultIconStream();

        public IEnumerable<MediaSource> GetMediaSources(TvSeason season, TvEpisode episode)
        {
            return Enumerable.Empty<MediaSource>();
        }
    }
}
