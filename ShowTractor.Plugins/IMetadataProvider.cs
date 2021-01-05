using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ShowTractor.Plugins.Interfaces
{
    public interface IMetadataProvider : IPlugin
    {
        IAsyncEnumerable<TvSeason> SearchAsync(string keyword, CancellationToken token);
        Task<TvSeason> GetUpdatesAsync(TvSeason season, IReadOnlyDictionary<AssemblyName, IReadOnlyDictionary<string, string>> additionalAttributes, CancellationToken token);
    }
}
