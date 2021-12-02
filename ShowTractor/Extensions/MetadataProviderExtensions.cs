using ShowTractor.Plugins.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ShowTractor.Extensions
{
    public static class MetadataProviderExtensions
    {
        internal static Task<GetUpdatesResult> GetUpdatesAsync(this IMetadataProvider provider, Database.TvSeason dbSeason, CancellationToken token = default)
        {
            var providerAssemblyName = provider.GetAssemblyName();
            var uniqueId = dbSeason.GetUniqueId(providerAssemblyName);
            return provider
                .GetUpdatesAsync(
                    dbSeason.ToRecord(providerAssemblyName, uniqueId),
                    dbSeason.AdditionalAttributes
                        .GroupBy(a => new AssemblyName(a.AssemblyName))
                        .ToDictionary(a => a.Key, a => (IReadOnlyDictionary<string, string>)a.ToDictionary(a => a.Name, a => a.Value)), token);
        }
        public static string GetAssemblyName(this IMetadataProvider provider) => provider.GetType().Assembly.GetName().Name ?? provider.GetType().Assembly.GetName().ToString();
        internal static string? GetUniqueId(this Database.TvSeason dbSeason, string providerAssemblyName) => dbSeason.AdditionalAttributes.FirstOrDefault(a => a.AssemblyName == providerAssemblyName && a.Name == nameof(TvSeason.UniqueId))?.Value;
    }
}
