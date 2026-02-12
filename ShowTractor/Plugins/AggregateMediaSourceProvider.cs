using ShowTractor.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShowTractor.Plugins
{
    class AggregateMediaSourceProvider : IMediaSourceProvider
    {
        private readonly PluginSettings settings;
        private readonly IServiceProvider serviceProvider;

        public AggregateMediaSourceProvider(PluginSettings settings, IServiceProvider serviceProvider)
        {
            this.settings = settings;
            this.serviceProvider = serviceProvider;
        }

        public string Name => throw new NotImplementedException();
        public PluginSettingsDescriptions? PluginSettingsDescriptions => throw new NotImplementedException();
        public bool CanFetchDownloadableSources(TvSeason season, TvEpisode episode)
        {
            foreach (var provider in LoadProviders())
            {
                if (provider.CanFetchDownloadableSources(season, episode))
                {
                    return true;
                }
            }
            return false;
        }
        public bool CanFetchPlayableSources(TvSeason season, TvEpisode episode)
        {
            foreach (var provider in LoadProviders())
            {
                if (provider.CanFetchPlayableSources(season, episode))
                {
                    return true;
                }
            }
            return false;
        }
        public async IAsyncEnumerable<MediaSource> FetchMediaSourcesAsync(TvSeason season, TvEpisode episode)
        {
            foreach (var provider in LoadProviders())
            {
                if (provider.CanFetchDownloadableSources(season, episode))
                {
                    await foreach (var mediaSource in provider.FetchMediaSourcesAsync(season, episode))
                    {
                        yield return mediaSource;
                    }
                }
            }
        }
        public Stream GetIconStream()
        {
            throw new InvalidOperationException();
        }
        public IEnumerable<MediaSource> GetMediaSources(TvSeason season, TvEpisode episode)
        {
            foreach (var provider in LoadProviders())
            {
                if (provider.CanFetchDownloadableSources(season, episode))
                {
                    foreach (var mediaSource in provider.GetMediaSources(season, episode))
                    {
                        yield return mediaSource;
                    }
                }
            }
        }
        private IEnumerable<IMediaSourceProvider> LoadProviders() => settings.MediaSourceProviders.Select(i => i.Load<IMediaSourceProvider>(serviceProvider));
    }
}
