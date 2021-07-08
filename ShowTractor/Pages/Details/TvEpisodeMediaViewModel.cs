using ShowTractor.Mvvm;
using ShowTractor.Plugins;
using ShowTractor.Plugins.Interfaces;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ShowTractor.Pages.Details
{
    public enum TvEpisodeMediaViewModelState
    {
        Loading,
        MediaUnavailable,
        DownloadAvailable,
        PlayerUnavailable,
        Available,
        Playing,
    }

    public class TvEpisodeMediaViewModel : INotifyPropertyChanged
    {
        private readonly TvSeason tvSeason;
        private readonly TvEpisode tvEpisode;
        private IAggregateMediaSourceProvider mediaSourceProvider;
        private static ConcurrentDictionary<(string showName, int season, int episode), MediaPlayerStateViewModel> playing = new ConcurrentDictionary<(string showName, int season, int episode), MediaPlayerStateViewModel>();
        private DateTime playStartTime;
        internal Task initializationTask;

        internal TvEpisodeMediaViewModel(TvSeason tvSeason, TvEpisode tvEpisode, IAggregateMediaSourceProvider mediaSourceProvider, IAggregateMediaPlayer aggregateMediaPlayer)
        {
            if (aggregateMediaPlayer is null)
            {
                throw new ArgumentNullException(nameof(aggregateMediaPlayer));
            }

            this.tvSeason = tvSeason ?? throw new ArgumentNullException(nameof(tvSeason));
            this.tvEpisode = tvEpisode ?? throw new ArgumentNullException(nameof(tvEpisode));
            this.mediaSourceProvider = mediaSourceProvider ?? throw new ArgumentNullException(nameof(mediaSourceProvider));
            initializationTask = Task.Run(async () => await UpdateAsync());
            PlayCommand = new AwaitableDelegateCommand(async () =>
            {
                if (MediaSource == null)
                    return; // No available media.
                if (MediaPlayerState != null)
                    return; // It is playing.
                var (controlTemplate, vm) = await aggregateMediaPlayer.PlayAsync(MediaSource, tvSeason, tvEpisode);
                if (controlTemplate != null)
                {
                    throw new NotImplementedException();
                }
                else if (vm != null)
                {
                    MediaPlayerState = vm;
                    State = TvEpisodeMediaViewModelState.Playing;
                    MediaPlayerState.PropertyChanged += MediaPlayerState_PropertyChanged;
                    playStartTime = DateTime.UtcNow;
                    playing.AddOrUpdate((tvSeason.ShowName, tvSeason.Season, tvEpisode.EpisodeNumber), vm, (_, _) => vm);
                }
                else
                {
                    State = TvEpisodeMediaViewModelState.PlayerUnavailable;
                }
            });
        }

        private void MediaPlayerState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (MediaPlayerState == null)
                throw new InvalidOperationException();
            if (e.PropertyName == nameof(MediaPlayerState.State) && MediaPlayerState.State == Plugins.Interfaces.MediaPlayerState.Closed)
            {
                MediaPlayerState.PropertyChanged -= MediaPlayerState_PropertyChanged;
                MediaPlayerState = null;
                NotPlaying();
                if (playStartTime == default)
                    return; // This instance is not the view model that tracks play time. Only the instance that launched the player does.
                var playTime = DateTime.UtcNow - playStartTime;
                // TODO
                playing.TryRemove((tvSeason.ShowName, tvSeason.Season, tvEpisode.EpisodeNumber), out _);
            }
        }

        public MediaPlayerStateViewModel? MediaPlayerState { get => mediaPlayerState; set { mediaPlayerState = value; OnPropertyChanged(); } }
        private MediaPlayerStateViewModel? mediaPlayerState;

        public TvEpisodeMediaViewModelState State
        {
            get => state; set
            {
                state = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowMediaUnavailable));
                OnPropertyChanged(nameof(ShowPlayerUnavailable));
                OnPropertyChanged(nameof(ShowPlay));
            }
        }
        private TvEpisodeMediaViewModelState state = TvEpisodeMediaViewModelState.MediaUnavailable;

        public bool ShowMediaUnavailable => State == TvEpisodeMediaViewModelState.MediaUnavailable;
        public bool ShowPlayerUnavailable => State == TvEpisodeMediaViewModelState.PlayerUnavailable;
        public bool ShowPlay => State == TvEpisodeMediaViewModelState.Available;
        public bool ShowPlaying => State == TvEpisodeMediaViewModelState.Playing;

        public MediaSource? MediaSource { get => mediaSource; set { mediaSource = value; OnPropertyChanged(); } }
        private MediaSource? mediaSource;

        public IAsyncCommand PlayCommand { get; set; }

        public async Task UpdateAsync()
        {
            await foreach (var item in mediaSourceProvider.GetAsync(tvSeason, tvEpisode))
            {
                if (MediaSource != null && (MediaSource.Resolution <= item.Resolution))
                    continue;
                MediaSource = item;
            }
            NotPlaying();
            if (playing.TryGetValue((tvSeason.ShowName, tvSeason.Season, tvEpisode.EpisodeNumber), out var vm))
            {
                MediaPlayerState = vm;
                State = TvEpisodeMediaViewModelState.Playing;
                MediaPlayerState.PropertyChanged += MediaPlayerState_PropertyChanged;
            }
        }

        private void NotPlaying()
        {
            if (MediaSource != null)
                State = TvEpisodeMediaViewModelState.Available;
            else
                State = TvEpisodeMediaViewModelState.MediaUnavailable;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
