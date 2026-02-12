using ShowTractor.Mvvm;
using ShowTractor.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ShowTractor.Pages.Details
{
    public class TvEpisodeDownloadsViewModel : INotifyPropertyChanged
    {
        private readonly IMediaSourceProvider provider;

        public TvEpisodeDownloadsViewModel(IMediaSourceProvider provider, TvEpisodeViewModel parent)
        {
            this.provider = provider;
            Parent = parent;
            if (Parent.Data == null || Parent.Parent.Data == null)
                throw new InvalidOperationException("Parent data is not set.");
            CanPlay = provider.CanFetchPlayableSources(Parent.Parent.Data, Parent.Data);
            CanDownload = provider.CanFetchDownloadableSources(Parent.Parent.Data, Parent.Data);
            foreach (var source in provider.GetMediaSources(Parent.Parent.Data, Parent.Data))
            {
                MediaSources.Add(source);
            }
            CanPlay = CanPlay || MediaSources.Any(i => i.Playable);
            CanDownload = CanDownload || MediaSources.Any(i => i.Uri != null && !i.Playable);
            DownloadCommand = new AwaitableDelegateCommand<MediaSource>(async (mediaSource) =>
            {
                try
                {
                    if (mediaSource != null)
                    {
                        ShowPopup = false;
                        // TODO: Download
                    }
                    else
                    {
                        var sources = await provider.FetchMediaSourcesAsync(Parent.Parent.Data, Parent.Data).ToListAsync();
                        MediaSources = sources;
                        CanPlay = CanPlay || MediaSources.Any(i => i.Playable);
                        CanDownload = CanDownload || MediaSources.Any(i => i.Uri != null && !i.Playable);
                        ShowPopup = true;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error fetching media sources: {ex.Message}";
                }
            });
            PlayCommand = new DelegateCommand(() =>
            {
                throw new NotImplementedException();
            });
        }
        public bool Visible
        {
            get
            {
                if (Parent.Data == null || Parent.Parent.Data == null)
                    throw new InvalidOperationException("Parent data is not set.");
                return provider.CanFetchDownloadableSources(Parent.Parent.Data, Parent.Data);
            }
        }
        public TvEpisodeViewModel Parent { get; }
        public bool CanPlay { get; private set; }
        public bool CanDownload { get; private set; }
        public string ErrorMessage { get => errorMessage; set { errorMessage = value; OnPropertyChanged(); } }
        private string errorMessage = string.Empty;
        public bool ShowPopup { get => showPopup; set { showPopup = value; OnPropertyChanged(); } }
        private bool showPopup;
        public IList<MediaSource> MediaSources { get => mediaSources; set { mediaSources = value; OnPropertyChanged(); } }
        private IList<MediaSource> mediaSources = new List<MediaSource>();
        public ICommand DownloadCommand { get; set; }
        public ICommand PlayCommand { get; set; }
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
