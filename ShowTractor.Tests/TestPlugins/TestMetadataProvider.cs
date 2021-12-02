using ShowTractor.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static ShowTractor.Tests.TestFixtures.ExampleSearchResults;

namespace ShowTractor.Tests.TestPlugins
{
    class TestMetadataProvider : IMetadataProvider
    {
        public TestMetadataProvider() { }
        public TestMetadataProvider(bool shouldFail)
        {
            ShouldFail = shouldFail;
        }
        public bool ShouldFail { get; set; }
        public TvSeason TestTvSeason { get; set; } = TestTvSeason1;
        public TvSeason[] MoreTvSeasons { get; set; } = new TvSeason[0];
        public string Name => nameof(TestMetadataProvider);
        public PluginSettingsDescriptions? PluginSettingsDescriptions => null;
        public Stream GetIconStream() => IPlugin.GetDefaultIconStream();
        public Task<GetUpdatesResult> GetUpdatesAsync(TvSeason season, IReadOnlyDictionary<AssemblyName, IReadOnlyDictionary<string, string>> additionalAttributes, CancellationToken _)
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            async IAsyncEnumerable<TvSeason> GetLatestSeasons(int afterSeasonNumber)
#pragma warning restore CS1998
            {
                for (int i = 0; i < MoreTvSeasons.Length; i++)
                {
                    if (i > afterSeasonNumber)
                    {
                        yield return MoreTvSeasons[i];
                    }
                }
            }
            return Task.FromResult(new GetUpdatesResult(TestTvSeason, GetLatestSeasons));
        }
        public IAsyncEnumerable<TvSeason> SearchAsync(string keyword, CancellationToken token)
        {
            if (ShouldFail)
                throw new Exception("Could not access the internet.");
            return new TvSeason[] { TestTvSeason, TestTvSeason2, TestTvSeason3, TestTvSeason6 }.ToAsyncEnumerable();
        }
    }
}
