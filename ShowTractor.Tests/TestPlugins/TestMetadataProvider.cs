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
        public string Name => nameof(TestMetadataProvider);
        public PluginSettingsDescriptions? PluginSettingsDescriptions => null;
        public Stream GetIconStream() => IPlugin.GetDefaultIconStream();
        public Task<TvSeason> GetUpdatesAsync(TvSeason season, IReadOnlyDictionary<AssemblyName, IReadOnlyDictionary<string, string>> additionalAttributes, CancellationToken token)
        {
            return Task.FromResult(TestTvSeason);
        }
        public IAsyncEnumerable<TvSeason> SearchAsync(string keyword, CancellationToken token)
        {
            if (ShouldFail)
                throw new Exception("Could not access the internet.");
            return new TvSeason[] { TestTvSeason, TestTvSeason2, TestTvSeason3, TestTvSeason6 }.ToAsyncEnumerable();
        }
    }
}
