using ShowTractor.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShowTractor.Plugins
{
    internal class AggregateMediaSourceProvider : IAggregateMediaSourceProvider
    {
        private PluginSettings settings;
        private IServiceProvider serviceProvider;

        public AggregateMediaSourceProvider(PluginSettings settings, IServiceProvider serviceProvider)
        {
            this.settings = settings;
            this.serviceProvider = serviceProvider;
        }

        public async IAsyncEnumerable<MediaSource> GetAsync(TvSeason tvSeason, TvEpisode tvEpisode)
        {
            foreach (var definition in settings.MediaSourceProviders.Where(p => p.Enabled))
            {
                var provider = definition.Load<IMediaSourceProvider>(serviceProvider);
                await foreach (var item in provider.GetAsync(tvSeason, tvEpisode))
                {
                    yield return item;
                }
            }
        }
    }
}
