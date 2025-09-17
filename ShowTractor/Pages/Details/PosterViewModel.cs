using ShowTractor.Database.Extensions;
using ShowTractor.Interfaces;
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
    public abstract class PosterViewModel : INotifyPropertyChanged
    {
        protected PosterViewModel(string showName, int season, DateTime firstEpisodeAirDate)
        {
            ShowName = showName;
            Season = season;
            this.firstEpisodeAirDate = firstEpisodeAirDate;
        }

        public string ShowName { get; }
        public int Season { get; }
        public string SeasonText => "Season " + Season;
        public abstract Uri? Artwork { get; }
        public bool ShowUnwatched { get => showUnwatched; set { showUnwatched = value; OnPropertyChanged(); } }
        private bool showUnwatched;
        public int Unwatched { get => unwatched; set { unwatched = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasUnwatched)); } }
        private int unwatched;
        public bool HasUnwatched => Unwatched > 0;
        public DateTime FirstEpisodeAirDate { get => firstEpisodeAirDate; set { firstEpisodeAirDate = value; OnPropertyChanged(); } }
        private DateTime firstEpisodeAirDate;
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public abstract class SavedPosterViewModel : PosterViewModel
    {
        private readonly IFactory<Database.ShowTractorDbContext> contextFactory;
        internal SavedPosterViewModel(Guid id, string showName, int season, DateTime firstEpisodeAirDate, IFactory<Database.ShowTractorDbContext> contextFactory) : base(showName, season, firstEpisodeAirDate)
        {
            Id = id;
            this.contextFactory = contextFactory;
        }
        public Guid Id { get; set; }
    }
    public class LibraryPosterViewModel : SavedPosterViewModel
    {
        private readonly IArtworkService artworkService;

        internal LibraryPosterViewModel(Guid id, string showName, int season, DateTime firstEpisodeAirDate, IFactory<Database.ShowTractorDbContext> contextFactory, IArtworkService artworkService) : base(id, showName, season, firstEpisodeAirDate, contextFactory)
        {
            this.artworkService = artworkService;
            artworkService.LoadAndSaveArtwork(null, id).ContinueWith(t =>
            {
                if (t.Exception == null)
                {
                    this.artwork = t.Result;
                    OnPropertyChanged(nameof(Artwork));
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public override Uri? Artwork => artwork;
        private Uri? artwork;
    }
    public class CalendarPosterViewModel : SavedPosterViewModel
    {
        private readonly GeneralSettings settings;
        private readonly DateTime airDate;
        private readonly IFactory<Database.ShowTractorDbContext> contextFactory;

        internal CalendarPosterViewModel(Guid id, string showName, int season, int episodeNumber, string episodeName, DateTime airDate, bool watched, IFactory<Database.ShowTractorDbContext> contextFactory, GeneralSettings settings) : base(id, showName, season, default, contextFactory)
        {
            this.episodeName = episodeName;
            this.airDate = airDate;
            this.contextFactory = contextFactory;
            this.settings = settings;
            this.episodeNumber = episodeNumber;
            this.watched = watched;
        }
        public string EpisodeName { get => episodeName; set { episodeName = value; OnPropertyChanged(); } }
        private string episodeName;
        public int EpisodeNumber { get => episodeNumber; set { episodeNumber = value; OnPropertyChanged(); OnPropertyChanged(nameof(D2Identifier)); } }
        private int episodeNumber;
        public string D2Identifier => "S" + Season.ToString("D2") + "E" + EpisodeNumber.ToString("D2");
        public bool ShowCheckbox => settings.ShowCheckboxInCalendarPage && airDate <= DateTime.Now;
        public bool ShowNewIcon => !ShowCheckbox && !Watched && airDate <= DateTime.Now;
        public bool Watched
        {
            get => watched;
            set
            {
                if (watched != value)
                {
                    MarkAsAsync(value).ConfigureAwait(false);
                }
            }
        }
        private bool watched;
        public bool Loading { get => loading; set { loading = value; OnPropertyChanged(); } }

        public override Uri? Artwork => null;

        private bool loading;

        private async Task MarkAsAsync(bool watched)
        {
            Loading = true;
            try
            {
                using (var context = contextFactory.Get())
                {
                    await Task.Run(async () => await context.SetWatchProgressAsync(Id, episodeNumber, watched ? TimeSpan.MaxValue : TimeSpan.Zero));
                }
                this.watched = watched;
                OnPropertyChanged(nameof(Watched));
                OnPropertyChanged(nameof(ShowNewIcon));
            }
            finally
            {
                Loading = false;
            }
        }
    }
    public class SearchResultPosterViewModel : PosterViewModel
    {
        public SearchResultPosterViewModel(TvSeason data) : base(data.ShowName, data.Season, data.Episodes.Select(e => e.FirstAirDate).FirstOrDefault())
        {
            Data = data;
        }
        public TvSeason Data { get; set; }
        public override Uri? Artwork => Data.ArtworkUri;
    }
}
