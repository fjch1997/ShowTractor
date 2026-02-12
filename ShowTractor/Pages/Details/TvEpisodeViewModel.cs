using Microsoft.EntityFrameworkCore;
using ShowTractor.Database.Extensions;
using ShowTractor.Interfaces;
using ShowTractor.Mvvm;
using ShowTractor.Plugins.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ShowTractor.Pages.Details
{
    public class TvEpisodeViewModel : INotifyPropertyChanged
    {
        private readonly IDbContextFactory<Database.ShowTractorDbContext> factory;
        private readonly IArtworkService artworkService;

        internal TvEpisodeViewModel(TvSeasonPageViewModel parent, Guid? seasonId, TvEpisode data, TimeSpan? watchProgress, IDbContextFactory<Database.ShowTractorDbContext> factory, IArtworkService artworkService, IMediaSourceProvider mediaSourceProvider)
        {
            Parent = parent;
            SeasonId = seasonId;
            Data = data;
            this.factory = factory;
            this.artworkService = artworkService;
            if (watchProgress != null)
                WatchProgress = watchProgress.Value;
            parent.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(parent.Following))
                    OnPropertyChanged(nameof(ShowWatchProgress));
            };
            DownloadViewModel = new TvEpisodeDownloadsViewModel(mediaSourceProvider, this);
        }
        public TvEpisode Data { get; private set; }
        public Guid? SeasonId { get; set; }
        public TvSeasonPageViewModel Parent { get; private set; }
        public TvEpisodeDownloadsViewModel DownloadViewModel { get; private set; }

        public string Name { get => Data.Name; set { Data = Data with { Name = value }; OnPropertyChanged(); } }
        public int EpisodeNumber { get => Data.EpisodeNumber; set { Data = Data with { EpisodeNumber = value }; OnPropertyChanged(); } }
        public string Description { get => Data.Description; set { Data = Data with { Description = value }; OnPropertyChanged(); } }
        public string D2Identifier => "S" + Parent.Season.GetValueOrDefault().ToString("D2") + "E" + EpisodeNumber.ToString("D2");
        public DateTime FirstAirDate { get => Data.FirstAirDate; set { Data = Data with { FirstAirDate = value }; OnPropertyChanged(); OnPropertyChanged(nameof(TagsDisplayText)); OnPropertyChanged(nameof(ShowWatchProgress)); OnPropertyChanged(nameof(Aired)); } }
        public TimeSpan Runtime { get => Data.Runtime; set { Data = Data with { Runtime = value }; OnPropertyChanged(); OnPropertyChanged(nameof(WatchProgressPercentage)); OnPropertyChanged(nameof(ShowWatchProgress)); OnPropertyChanged(nameof(TagsDisplayText)); OnPropertyChanged(nameof(MarkAsWatchedEnabled)); } }
        public TimeSpan WatchProgress { get => watchProgress; set { watchProgress = value; OnPropertyChanged(); OnPropertyChanged(nameof(WatchProgressPercentage)); OnPropertyChanged(nameof(MarkAsWatchedEnabled)); } }
        private TimeSpan watchProgress;
        public Uri? Artwork
        {
            get
            {
                if (!artworkInitializingOrInitialized)
                {
                    artworkInitializingOrInitialized = true;
                    // Initialize artwork on first access.
                    artworkService.LoadAndSaveArtwork(Data.ArtworkUri, SeasonId, EpisodeNumber).ContinueWith(t =>
                    {
                        if (t.Exception == null)
                            Artwork = t.Result;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                return artwork;
            }
            set { if (artwork == value) return; artwork = value; OnPropertyChanged(); }
        }
        private Uri? artwork;
        private bool artworkInitializingOrInitialized;
        public string TagsDisplayText => string.Join(" • ", new[] {
            FirstAirDate == default ? null : FirstAirDate.ToLongDateString(),
            Runtime == default ? null : ((int)Math.Round(Runtime.TotalMinutes) + " minutes"),
        }.Where(s => s != null));
        public bool ShowWatchProgress => Aired && Parent.Following;
        public int WatchProgressPercentage => GetWatchPercentage(Runtime, WatchProgress);
        public bool MarkAsWatchedEnabled => WatchProgressPercentage != 100;
        public bool Aired => new Database.TvEpisode { FirstAirDate = FirstAirDate }.Aired();

        public IAsyncCommand MarkAsWatched => new AwaitableDelegateCommand(() => MarkAsync(TimeSpan.MaxValue));
        public IAsyncCommand MarkAsUnwatched => new AwaitableDelegateCommand(() => MarkAsync(TimeSpan.Zero));

        private async ValueTask MarkAsync(TimeSpan value)
        {
            if (SeasonId == null) throw new InvalidOperationException($"{nameof(SeasonId)} is null.");
            using var context = await factory.CreateDbContextAsync();
            await Task.Run(async () => await context.SetWatchProgressAsync(SeasonId.Value, EpisodeNumber, value));
            WatchProgress = value;
        }
        internal async ValueTask UpdateAsync(Guid? seasonId, TvEpisode data, Database.ShowTractorDbContext? context, bool updateInDatabase)
        {
            if (seasonId != null)
                SeasonId = seasonId;
            Data = data;
            OnPropertyChanged(string.Empty);
            if (updateInDatabase)
            {
                if (context == null) throw new ArgumentNullException(nameof(context));
                await CreateOrUpdateInDatabaseAsync(context);
            }
            Artwork = await artworkService.LoadAndSaveArtwork(Data.ArtworkUri, seasonId, EpisodeNumber);
        }
        private async ValueTask CreateOrUpdateInDatabaseAsync(Database.ShowTractorDbContext context)
        {
            if (SeasonId != null)
            {
                var dbEpisode = await Task.Run(async () => await context.TvEpisodes.FindAsync(SeasonId, EpisodeNumber));
                if (dbEpisode == null)
                {
                    dbEpisode = Database.TvEpisode.FromRecord(Data);
                    dbEpisode.TvSeasonId = SeasonId.Value;
                    await context.TvEpisodes.AddAsync(dbEpisode);
                }
                await dbEpisode.UpdateAsync(Data);
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
