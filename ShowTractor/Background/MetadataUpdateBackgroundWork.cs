using Microsoft.EntityFrameworkCore;
using ShowTractor.Extensions;
using ShowTractor.Interfaces;
using ShowTractor.Plugins.Interfaces;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShowTractor.Background
{
    internal class MetadataUpdateBackgroundWork : IBackgroundWork
    {
        private readonly IFactory<Database.ShowTractorDbContext> factory;
        private readonly IFactory<IMetadataProvider?> providerFactory;
        private readonly GeneralSettings generalSettings;
        private readonly HttpClient httpClient;
        private IMetadataProvider? provider;

        internal MetadataUpdateBackgroundWork(GeneralSettings generalSettings, IFactory<Database.ShowTractorDbContext> factory, IFactory<IMetadataProvider?> providerFactory, HttpClient httpClient)
        {
            this.factory = factory;
            this.providerFactory = providerFactory;
            this.generalSettings = generalSettings;
            this.httpClient = httpClient;
        }
        public TimeSpan Interval => generalSettings.MetadataUpdateInterval;
        public ValueTask<bool> CanDoWorkAsync()
        {
            provider = providerFactory.Get();
            return new ValueTask<bool>(providerFactory != null);
        }
        public async ValueTask DoWorkAsync(CancellationToken token = default)
        {
            provider ??= providerFactory.Get();
            if (provider == null)
                return;
            using var context = factory.Get();
            var dbSeasons = context.TvSeasons
                .Include(s => s.AdditionalAttributes)
                .Include(s => s.Episodes).AsAsyncEnumerable();
            await foreach (var dbSeason in dbSeasons)
            {
                var (latest, _) = await provider.GetUpdatesAsync(dbSeason, token);
                await dbSeason.UpdateAsync(latest, httpClient);
                for (int i = 0; i < latest.Episodes.Count; i++)
                {
                    if (dbSeason.Episodes == null) throw new Exception($"{nameof(dbSeason.Episodes)} is null.");
                    if (i >= dbSeason.Episodes.Count)
                    {
                        dbSeason.Episodes.Add(Database.TvEpisode.FromRecord(latest.Episodes[i]));
                    }
                    else
                    {
                        await dbSeason.Episodes[i].UpdateAsync(latest.Episodes[i], httpClient);
                    }
                }
            }
            await context.SaveChangesAsync(token);
        }
    }
}
