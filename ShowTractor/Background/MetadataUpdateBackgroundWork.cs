using Microsoft.EntityFrameworkCore;
using ShowTractor.Extensions;
using ShowTractor.Interfaces;
using ShowTractor.Plugins.Interfaces;
using System;
using System.Linq;
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
            return new ValueTask<bool>(providerFactory.Get() != null);
        }
        public async ValueTask DoWorkAsync(CancellationToken token = default)
        {
            provider ??= providerFactory.Get();
            if (provider == null)
                return;
            using var context = factory.Get();
            var shows = context.TvSeasons
                .Include(s => s.AdditionalAttributes)
                .Include(s => s.Episodes).OrderBy(s => s.Season).AsAsyncEnumerable().GroupBy(s => s.ShowName);
            await foreach (var show in shows)
            {
                GetLatestSeasonsDelegate? getLatestSeasonsDelegate = null;
                int lastSeasonNumber = 0;
                await foreach (var dbSeason in show)
                {
                    (var latest, getLatestSeasonsDelegate) = await provider.GetUpdatesAsync(dbSeason, token);
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
                    lastSeasonNumber = dbSeason.Season;
                }
                if (getLatestSeasonsDelegate != null)
                {
                    await foreach (var newSeason in getLatestSeasonsDelegate(lastSeasonNumber))
                    {
                        var dbSeason = Database.TvSeason.FromRecord(newSeason ?? throw new ArgumentNullException(nameof(newSeason)), provider.GetAssemblyName());
                        if (dbSeason.Artwork == null && newSeason.ArtworkUri != null)
                        {
                            try
                            {
                                dbSeason.Artwork = await httpClient.GetByteArrayAsync(newSeason.ArtworkUri);
                            }
                            catch (Exception ex)
                            {
                                throw new MetadataUpdateBackgroundWorkArtworkLoadException(ex);
                            }
                        }
                        context.TvSeasons.Add(dbSeason);
                        dbSeason.Following = true;
                        dbSeason.DateFollowed = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
                        await Task.Run(async () => await context.SaveChangesAsync());
                        foreach (var episode in newSeason.Episodes)
                        {
                            var dbEpisode = Database.TvEpisode.FromRecord(episode);
                            dbEpisode.TvSeasonId = dbSeason.Id;
                            await dbEpisode.UpdateAsync(episode, httpClient);
                            await context.TvEpisodes.AddAsync(dbEpisode);
                        }
                        await Task.Run(async () => await context.SaveChangesAsync());
                    }
                }
            }
            await context.SaveChangesAsync(token);
        }
    }
    internal class MetadataUpdateBackgroundWorkArtworkLoadException : Exception
    {
        public MetadataUpdateBackgroundWorkArtworkLoadException(Exception inner) : base("Could not load artwork. ", inner) { }
    }
}
