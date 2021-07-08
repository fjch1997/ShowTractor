using ShowTractor.Plugins;
using ShowTractor.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace ShowTractor.Tests.Mocks
{
    public class TestAggregateMediaSourceProvider : IAggregateMediaSourceProvider
    {
        private readonly IEnumerable<MediaSource> mediaSources;

        public TestAggregateMediaSourceProvider(IEnumerable<MediaSource> mediaSources)
        {
            this.mediaSources = mediaSources ?? throw new ArgumentNullException(nameof(mediaSources));
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerable<MediaSource> GetAsync(TvSeason tvSeason, TvEpisode tvEpisode)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            foreach (var item in mediaSources)
            {
                yield return item;
            }
        }
    }
}
