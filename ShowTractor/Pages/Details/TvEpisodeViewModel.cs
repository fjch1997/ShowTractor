using ShowTractor.Database.Extensions;
using ShowTractor.Interfaces;
using ShowTractor.Mvvm;
using ShowTractor.Plugins;
using ShowTractor.Plugins.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ShowTractor.Pages.Details
{
    public class TvEpisodeViewModel : INotifyPropertyChanged
    {
        private readonly TvSeasonPageViewModel parent;
        private TvEpisode data;
        private readonly HttpClient httpClient;
        private readonly IFactory<Database.ShowTractorDbContext> factory;

        internal TvEpisodeViewModel(TvSeasonPageViewModel parent, Guid? seasonId, TvSeason season, TvEpisode data, TimeSpan? watchProgress, HttpClient httpClient, IFactory<Database.ShowTractorDbContext> factory, IAggregateMediaSourceProvider mediaSourceProvider)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            SeasonId = seasonId;
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            if (watchProgress != null)
                WatchProgress = watchProgress.Value;
            parent.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(parent.Following))
                    OnPropertyChanged(nameof(ShowWatchProgress));
            };
            if (seasonId != null)
                artwork = new Artwork(
                    new ArtworkCacheKey { SeasonId = seasonId.Value, Type = ArtworkType.Episode, EpisodeNumber = data.EpisodeNumber },
                    new DelegateFactory<ValueTask<Stream>>(() => new ValueTask<Stream>(new Database.TvEpisodeArtworkStream(factory.Get(), seasonId.Value, data.EpisodeNumber))));
            else if (data.Artwork != null)
                artwork = new Artwork(
                    new ArtworkCacheKey { Type = ArtworkType.Episode, HashCode = data.Artwork.GetHashCode() },
                    new DelegateFactory<ValueTask<Stream>>(() => new ValueTask<Stream>(new MemoryStream(data.Artwork))));
            else if (data.ArtworkUri != null)
                artwork = new Artwork(
                    new ArtworkCacheKey { Type = ArtworkType.Episode, HashCode = data.ArtworkUri.GetHashCode() },
                    new DelegateFactory<ValueTask<Stream>>(async () => await httpClient.GetStreamAsync(data.ArtworkUri)));
            else
                artwork = new TvEpisodeDefaultArtwork();
            Media = new TvEpisodeMediaViewModel(season, data, mediaSourceProvider ?? throw new ArgumentNullException(nameof(mediaSourceProvider)));
        }

        public Guid? SeasonId { get; set; }
        public TvEpisodeMediaViewModel Media { get; private set; }

        public string Name { get => data.Name; set { data = data with { Name = value }; OnPropertyChanged(); } }
        public int EpisodeNumber { get => data.EpisodeNumber; set { data = data with { EpisodeNumber = value }; OnPropertyChanged(); } }
        public string Description { get => data.Description; set { data = data with { Description = value }; OnPropertyChanged(); } }
        public DateTime FirstAirDate { get => data.FirstAirDate; set { data = data with { FirstAirDate = value }; OnPropertyChanged(); OnPropertyChanged(nameof(TagsDisplayText)); OnPropertyChanged(nameof(ShowWatchProgress)); OnPropertyChanged(nameof(Aired)); } }
        public TimeSpan Runtime { get => data.Runtime; set { data = data with { Runtime = value }; OnPropertyChanged(); OnPropertyChanged(nameof(WatchProgressPercentage)); OnPropertyChanged(nameof(ShowWatchProgress)); OnPropertyChanged(nameof(TagsDisplayText)); OnPropertyChanged(nameof(MarkAsWatchedEnabled)); } }
        public TimeSpan WatchProgress { get => watchProgress; set { watchProgress = value; OnPropertyChanged(); OnPropertyChanged(nameof(WatchProgressPercentage)); OnPropertyChanged(nameof(MarkAsWatchedEnabled)); } }
        private TimeSpan watchProgress;

        public Artwork Artwork { get => artwork; set { artwork = value; OnPropertyChanged(); } }
        private Artwork artwork;
        public string TagsDisplayText => string.Join(" • ", new[] {
            FirstAirDate == default ? null : FirstAirDate.ToLongDateString(),
            Runtime == default ? null : ((int)Math.Round(Runtime.TotalMinutes) + " minutes"),
        }.Where(s => s != null));
        public bool ShowWatchProgress => Aired && parent.Following;
        public int WatchProgressPercentage => GetWatchPercentage(Runtime, WatchProgress);
        public bool MarkAsWatchedEnabled => WatchProgressPercentage != 100;
        public bool Aired => new Database.TvEpisode { FirstAirDate = FirstAirDate }.Aired();

        public IAsyncCommand MarkAsWatched => new AwaitableDelegateCommand(() => MarkAsync(TimeSpan.MaxValue));
        public IAsyncCommand MarkAsUnwatched => new AwaitableDelegateCommand(() => MarkAsync(TimeSpan.Zero));

        private async ValueTask MarkAsync(TimeSpan value)
        {
            if (SeasonId == null) throw new InvalidOperationException($"{nameof(SeasonId)} is null.");
            using var context = factory.Get();
            await Task.Run(async () => await context.SetWatchProgressAsync(SeasonId.Value, EpisodeNumber, value));
            WatchProgress = value;
        }

        internal async ValueTask UpdateAsync(Guid? id, TvEpisode data, Database.ShowTractorDbContext? context, bool updateInDatabase)
        {
            if (id != null)
                SeasonId = id;
            this.data = data;
            OnPropertyChanged(string.Empty);
            if (updateInDatabase)
            {
                if (context == null) throw new ArgumentNullException(nameof(context));
                await CreateOrUpdateInDatabaseAsync(context);
            }
        }

        private async ValueTask CreateOrUpdateInDatabaseAsync(Database.ShowTractorDbContext context)
        {
            if (SeasonId != null)
            {
                var dbEpisode = await Task.Run(async () => await context.TvEpisodes.FindAsync(SeasonId, EpisodeNumber));
                if (dbEpisode == null)
                {
                    dbEpisode = Database.TvEpisode.FromRecord(data);
                    dbEpisode.TvSeasonId = SeasonId.Value;
                    await context.TvEpisodes.AddAsync(dbEpisode);
                }
                await dbEpisode.UpdateAsync(data, httpClient);
            }
        }

        public static int GetWatchPercentage(TimeSpan runtime, TimeSpan watchProgress)
        {
            if (runtime == default)
            {
                return watchProgress > TimeSpan.Zero ? 100 : 0;
            }
            return (int)Math.Min(watchProgress / runtime, 100D);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
