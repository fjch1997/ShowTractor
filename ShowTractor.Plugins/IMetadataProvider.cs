using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ShowTractor.Plugins.Interfaces
{
    public record struct GetUpdatesResult(TvSeason TvSeason, GetLatestSeasonsDelegate GetLatestSeasonsDelegate);

    public delegate IAsyncEnumerable<TvSeason> GetLatestSeasonsDelegate(int afterSeasonNumber);

    public interface IMetadataProvider : IPlugin
    {
        IAsyncEnumerable<TvSeason> SearchAsync(string keyword, CancellationToken token);
        Task<GetUpdatesResult> GetUpdatesAsync(TvSeason season, IReadOnlyDictionary<AssemblyName, IReadOnlyDictionary<string, string>> additionalAttributes, CancellationToken token);
    }
}
