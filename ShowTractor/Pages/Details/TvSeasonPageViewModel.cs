using Microsoft.EntityFrameworkCore;
using ShowTractor.Database.Extensions;
using ShowTractor.Extensions;
using ShowTractor.Interfaces;
using ShowTractor.Mvvm;
using ShowTractor.Plugins;
using ShowTractor.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ShowTractor.Pages.Details
{
    public class TvSeasonPageViewModel : INotifyPropertyChanged, ISupportNavigationParameter
    {
        private readonly IFactory<IMetadataProvider> providerFactory;
        private readonly HttpClient httpClient;
        private readonly IFactory<Database.ShowTractorDbContext> factory;
        private readonly IAggregateMediaSourceProvider mediaSourceProvider;
        private readonly CancellationTokenSource cts = new();
        private TvSeason? data;
        private Guid? Id;

        internal TvSeasonPageViewModel(IFactory<IMetadataProvider> providerFactory, HttpClient httpClient, IFactory<Database.ShowTractorDbContext> factory, IAggregateMediaSourceProvider mediaSourceProvider)
        {
            this.providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.mediaSourceProvider = mediaSourceProvider ?? throw new ArgumentNullException(nameof(mediaSourceProvider));
        }

        private PosterViewModel? parameter;
        public object? Parameter
        {
            get { return parameter; }
            set
            {
                parameter = (PosterViewModel?)value;
                _ = RefreshAsync();
            }
        }
        public bool Loading { get => loading; set { loading = value; OnPropertyChanged(); } }
        private bool loading;
        public string ErrorMessage { get => errorMessage; set { errorMessage = value; OnPropertyChanged(); } }
        private string errorMessage = string.Empty;
        public string? ShowName { get => showName; private set { showName = value; OnPropertyChanged(); } }
        private string? showName;
        public string TagsDisplayText => string.Join(" • ", new[] {
            "Season " + Season,
            Episodes.FirstOrDefault(e=>e.FirstAirDate != default)?.FirstAirDate.Year == default ? null : (Episodes.Count + " Episodes"),
        }.Where(s => s != null));
        public int? Season { get => season; set { season = value; OnPropertyChanged(); OnPropertyChanged(nameof(TagsDisplayText)); } }
        private int? season;
        public IEnumerable<string>? Genre { get => genre; set { genre = value; OnPropertyChanged(); } }
        private IEnumerable<string>? genre = Enumerable.Empty<string>();
        public IEnumerable<string> Ratings { get => ratings; set { ratings = value; OnPropertyChanged(); } }
        private IEnumerable<string> ratings = Enumerable.Empty<string>();
        public string ShowDescription { get => showDescription; set { showDescription = value; OnPropertyChanged(); } }
        private string showDescription = string.Empty;
        public string SeasonDescription { get => seasonDescription; set { seasonDescription = value; OnPropertyChanged(); } }
        private string seasonDescription = string.Empty;
        public Artwork Artwork { get => artwork; set { artwork = value; OnPropertyChanged(); } }
        private Artwork artwork = new TvSeasonDefaultArtwork();
        public ObservableCollection<TvEpisodeViewModel> Episodes
        {
            get => episodes; set
            {
                episodes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TagsDisplayText));
                OnPropertyChanged(nameof(MarkSeasonAsWatchedEnabled));
                OnPropertyChanged(nameof(MarkSeasonAsUnwatchedEnabled));
            }
        }
        private ObservableCollection<TvEpisodeViewModel> episodes = new();
        public bool Following { get => following; set { following = value; OnPropertyChanged(); } }
        private bool following;
        public bool MarkAllSeasonsAsWatchedEnabled { get => markAllSeasonsAsWatchedEnabled; set { markAllSeasonsAsWatchedEnabled = value; OnPropertyChanged(); } }
        private bool markAllSeasonsAsWatchedEnabled = true;
        public bool MarkAllSeasonsAsUnwatchedEnabled { get => markAllSeasonsAsUnwatchedEnabled; set { markAllSeasonsAsUnwatchedEnabled = value; OnPropertyChanged(); } }
        private bool markAllSeasonsAsUnwatchedEnabled = true;
        public IAsyncCommand FollowCommand => new AwaitableDelegateCommand(async () =>
        {
            Following = true;
            Database.TvSeason dbSeason;
            using var context = factory.Get();
            if (Id == null)
            {
                var provider = providerFactory.Get();
                var providerAssemblyName = provider.GetType().Assembly.GetName().Name;
                dbSeason = Database.TvSeason.FromRecord(data ?? throw new ArgumentNullException(nameof(data)), providerAssemblyName);
                if (dbSeason.Artwork == null && data.ArtworkUri != null)
                {
                    try
                    {
                        dbSeason.Artwork = await httpClient.GetByteArrayAsync(data.ArtworkUri);
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = "Warning: Could not load artwork. " + ex.Message;
                    }
                }
                context.TvSeasons.Add(dbSeason);
                dbSeason.Following = true;
                dbSeason.DateFollowed = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
                await Task.Run(async () => await context.SaveChangesAsync());
                Id = dbSeason.Id;
                for (int i = 0; i < Episodes.Count; i++)
                {
                    await Episodes[i].UpdateAsync(dbSeason.Id, data.Episodes[i], context, true);
                }
                await Task.Run(async () => await context.SaveChangesAsync());
            }
            else
            {
                dbSeason = new Database.TvSeason
                {
                    Id = Id.Value,
                    Following = true,
                    DateFollowed = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc),
                };
                context.TvSeasons.Attach(dbSeason);
                context.Entry(dbSeason).Property(s => s.Following).IsModified = true;
                context.Entry(dbSeason).Property(s => s.DateFollowed).IsModified = true;
                await Task.Run(async () => await context.SaveChangesAsync());
            }
        }, CommandExceptionHandler);
        public IAsyncCommand UnfollowCommand => new AwaitableDelegateCommand(async () =>
        {
            if (Id == null) throw new ArgumentNullException(nameof(Id));
            using var context = factory.Get();
            var dbSeason = new Database.TvSeason { Id = Id.Value, Following = false };
            context.TvSeasons.Attach(dbSeason);
            context.Entry(dbSeason).Property(s => s.Following).IsModified = true;
            await Task.Run(async () => await context.SaveChangesAsync());
            Following = false;
        }, CommandExceptionHandler);
        public bool MarkSeasonAsWatchedEnabled => Episodes.Any(e => e.ShowWatchProgress && e.WatchProgressPercentage < 100);
        public bool MarkSeasonAsUnwatchedEnabled => Episodes.Any(e => e.ShowWatchProgress && e.WatchProgressPercentage != 0);
        public IAsyncCommand MarkSeasonAsWatched => new AwaitableDelegateCommand(() => MarkSeasonAsync(TimeSpan.MaxValue), CommandExceptionHandler);
        public IAsyncCommand MarkSeasonAsUnwatched => new AwaitableDelegateCommand(() => MarkSeasonAsync(TimeSpan.Zero), CommandExceptionHandler);
        public IAsyncCommand MarkAllSeasonsAsWatched => new AwaitableDelegateCommand(() => MarkAllSeasonsAsync(TimeSpan.MaxValue), CommandExceptionHandler);
        public IAsyncCommand MarkAllSeasonsAsUnwatched => new AwaitableDelegateCommand(() => MarkAllSeasonsAsync(TimeSpan.Zero), CommandExceptionHandler);
        private async ValueTask MarkSeasonAsync(TimeSpan value)
        {
            if (Id == null) throw new InvalidOperationException($"{nameof(Id)} is null.");
            using var context = factory.Get();
            var dbSeason = await Task.Run(async () =>
            {
                var dbSeason = await ((IQueryable<Database.TvSeason>)context.TvSeasons).Where(s => s.Id == Id).SelectNoArtwork().FirstAsync();
                context.TvSeasons.Attach(dbSeason);
                await context.Entry(dbSeason).Collection(nameof(dbSeason.Episodes)).LoadAsync();
                return dbSeason;
            });
            for (int i = 0; i < Episodes.Count; i++)
            {
                if (Episodes[i].ShowWatchProgress)
                {
                    (dbSeason.Episodes ?? throw new Exception())[i].WatchProgress = value;
                    Episodes[i].WatchProgress = value;
                }
            }
            await Task.Run(async () => await context.SaveChangesAsync());
        }
        private async ValueTask MarkAllSeasonsAsync(TimeSpan value)
        {
            if (Id == null) throw new InvalidOperationException($"{nameof(Id)} is null.");
            using var context = factory.Get();
            var dbSeasons = await Task.Run(async () => await ((IQueryable<Database.TvSeason>)context.TvSeasons).Where(s => s.ShowName == ShowName && s.Following).Select(s => new Database.TvSeason
            {
                Id = s.Id,
                Episodes = s.Episodes.Select(e => new Database.TvEpisode
                {
                    FirstAirDate = e.FirstAirDate,
                    WatchProgress = e.WatchProgress,
                    EpisodeNumber = e.EpisodeNumber,
                    TvSeasonId = e.TvSeasonId,
                }).ToList(),
            }).ToArrayAsync());
            foreach (var dbSeason in dbSeasons)
            {
                foreach (var dbEpisode in dbSeason.Episodes ?? throw new Exception())
                {
                    if (dbEpisode.Aired())
                    {
                        dbEpisode.WatchProgress = value;
                        context.Entry(dbEpisode).Property(e => e.WatchProgress).IsModified = true;
                        if (dbSeason.Id == Id)
                        {
                            var episodeVm = Episodes.FirstOrDefault(e => e.EpisodeNumber == dbEpisode.EpisodeNumber);
                            if (episodeVm != null)
                                episodeVm.WatchProgress = value;
                        }
                    }
                }
            }
            await Task.Run(async () => await context.SaveChangesAsync());
        }
        private ValueTask CommandExceptionHandler(Exception exception)
        {
            ErrorMessage = exception.Message;
            return new ValueTask();
        }
        private async Task RefreshAsync()
        {
            Loading = true;
            try
            {
                if (parameter == null)
                {
                    return;
                }
                var provider = providerFactory.Get();
                var providerAssemblyName = provider?.GetAssemblyName();
                using var context = factory.Get();
                if (parameter is SearchResultPosterViewModel searchResult)
                {
                    data = searchResult.Data;
                    // Navigated from search results.
                    Database.TvSeason? dbSeason = null;
                    if (data.UniqueId != null)
                    {
                        // Locate season by unique ID.
                        var aa = context.AdditionalAttributes
                            .Include(a => a.TvSeason)
                            .ThenInclude(s => s.Episodes)
                            .FirstOrDefault(a =>
                                a.AssemblyName == providerAssemblyName &&
                                a.Name == nameof(TvSeason.UniqueId) &&
                                a.Value == data.UniqueId &&
                                a.TvSeason.Season == data.Season);
                        dbSeason = aa?.TvSeason;
                    }
                    if (dbSeason == null)
                    {
                        // "Fuzzy" locate season by show name and season number.
                        dbSeason = context.TvSeasons
                            .Include(nameof(Database.TvSeason.Episodes))
                            .FirstOrDefault(s => s.ShowName == data.ShowName && s.Season == data.Season);
                    }
                    if (dbSeason != null)
                    {
                        // This is a show that already exists in the database.
                        Following = dbSeason.Following;
                        await dbSeason.UpdateAsync(data, httpClient);
                        await context.SaveChangesAsync();
                    }
                    Id = dbSeason?.Id;
                    await LoadDataForDisplayAsync(data, dbSeason?.Episodes?.Select(e => e.WatchProgress).ToArray(), parameter.Artwork, false, context);
                }
                else if (parameter is SavedPosterViewModel savedPosterViewModel)
                {
                    // Navigated from library. Load from database.
                    Id = savedPosterViewModel.Id;
                    var dbSeason = await Task.Run(async () =>
                    {
                        var dbSeason = await ((IQueryable<Database.TvSeason>)context.TvSeasons).Where(s => s.Id == savedPosterViewModel.Id).SelectNoArtwork().FirstAsync();
                        context.TvSeasons.Attach(dbSeason);
                        await context.Entry(dbSeason).Collection(nameof(dbSeason.AdditionalAttributes)).LoadAsync();
                        await context.Entry(dbSeason).Collection(nameof(dbSeason.Episodes)).LoadAsync();
                        return dbSeason;
                    });
                    Following = dbSeason.Following;
                    var uniqueId = providerAssemblyName == null ? null : dbSeason.GetUniqueId(providerAssemblyName);
                    Artwork = parameter.Artwork ?? new TvSeasonDefaultArtwork();
                    await LoadDataForDisplayAsync(dbSeason.ToRecord(providerAssemblyName, uniqueId), dbSeason.Episodes.Select(e => e.WatchProgress).ToArray(), parameter.Artwork, false, context);
                    // Update saved data from latest.
                    if (provider != null)
                    {
                        var latest = await provider.GetUpdatesAsync(dbSeason, cts.Token);
                        await dbSeason.UpdateAsync(latest, httpClient);
                        await Task.Run(async () => await context.SaveChangesAsync());
                        await LoadDataForDisplayAsync(
                            latest,
                            null,
                            new Artwork(
                                new ArtworkCacheKey { SeasonId = dbSeason.Id, Type = ArtworkType.Season },
                                new DelegateFactory<ValueTask<Stream>>(() => new ValueTask<Stream>(new Database.TvSeasonArtworkStream(factory.Get(), dbSeason.Id)))),
                            true,
                            context);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                Loading = false;
            }
        }
        private async ValueTask LoadDataForDisplayAsync(TvSeason data, TimeSpan[]? episodeWatchProgresses, Artwork? artwork, bool updateInDatabase, Database.ShowTractorDbContext? context)
        {
            ShowName = data.ShowName;
            Season = data.Season;
            Genre = data.Genres;
            Ratings = data.Ratings;
            ShowDescription = data.ShowDescription;
            SeasonDescription = data.SeasonDescription;
            if (artwork != null)
                Artwork = artwork;

            for (int i = 0; i < data.Episodes.Count; i++)
            {
                if (i >= Episodes.Count)
                {
                    var episodeVm = new TvEpisodeViewModel(this, Id, data, data.Episodes[i], episodeWatchProgresses?.Skip(i).FirstOrDefault(), httpClient, factory, mediaSourceProvider);
                    Episodes.Add(episodeVm);
                    AttachTvEpisodeEventListener(episodeVm);
                }
                await Episodes[i].UpdateAsync(Id, data.Episodes[i], context, updateInDatabase);
            }
#pragma warning disable CA2245 // Do not assign a property to itself
            Episodes = Episodes;
#pragma warning restore CA2245 // Do not assign a property to itself
            if (updateInDatabase)
            {
                if (context == null) throw new ArgumentNullException(nameof(context));
                await Task.Run(async () => await context.SaveChangesAsync());
            }
        }
        private void AttachTvEpisodeEventListener(TvEpisodeViewModel episodeVm)
        {
            episodeVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(episodeVm.WatchProgressPercentage))
                {
                    OnPropertyChanged(nameof(MarkSeasonAsWatchedEnabled));
                    OnPropertyChanged(nameof(MarkSeasonAsUnwatchedEnabled));
                }
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
