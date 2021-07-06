using ShowTractor.Mvvm;
using ShowTractor.Plugins;
using ShowTractor.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShowTractor.Pages.Details
{
    public enum TvEpisodeMediaViewModelState
    {
        Loading,
        Unavailable,
        DownloadAvailable,
        Available,
    }
    public class TvEpisodeMediaViewModel : INotifyPropertyChanged
    {
        private readonly TvSeason tvSeason;
        private readonly TvEpisode tvEpisode;
        private IAggregateMediaSourceProvider mediaSourceProvider;

        internal TvEpisodeMediaViewModel(TvSeason tvSeason, TvEpisode tvEpisode, IAggregateMediaSourceProvider mediaSourceProvider)
        {
            this.tvSeason = tvSeason;
            this.tvEpisode = tvEpisode;
            this.mediaSourceProvider = mediaSourceProvider;
            Task.Run(UpdateAsync);
            PlayCommand = new DelegateCommand(() =>
            {
                if (MediaSource is GenericMediaSource<string> s && MediaSource.Type.Id == PredefinedMediaSources.LocalFile.Id)
                {
                    var p = Process.Start(new ProcessStartInfo
                    {
                        FileName = s.Value,
                        UseShellExecute = true,
                    });
                }
            });
        }
        public TvEpisodeMediaViewModelState State { get => state; set { state = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowUnavailable)); OnPropertyChanged(nameof(ShowPlay)); } }
        private TvEpisodeMediaViewModelState state = TvEpisodeMediaViewModelState.Unavailable;

        public bool ShowUnavailable => State == TvEpisodeMediaViewModelState.Unavailable;
        public bool ShowPlay => State == TvEpisodeMediaViewModelState.Available;

        public MediaSource? MediaSource { get => mediaSource; set { mediaSource = value; OnPropertyChanged(); } }
        private MediaSource? mediaSource;

        public ICommand PlayCommand { get; set; }

        public async Task UpdateAsync()
        {
            await foreach (var item in mediaSourceProvider.GetAsync(tvSeason, tvEpisode))
            {
                if (MediaSource != null && (MediaSource.Resolution <= item.Resolution))
                    continue;
                MediaSource = item;
            }
            if (MediaSource != null)
                State = TvEpisodeMediaViewModelState.Available;
            else
                State = TvEpisodeMediaViewModelState.Unavailable;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
