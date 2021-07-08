using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ShowTractor.Plugins.Interfaces
{
    /// <summary>
    /// The type of media player that an <see cref="IMediaPlayer"/> implementation use.
    /// </summary>
    public enum MediaPlayerMode
    {
        /// <summary>
        /// <see cref="IMediaPlayer"/> will launch a player in a new window or process on its own. 
        /// <see cref="ShowTractor"/> will not handle any display.
        /// </summary>
        Process,
        /// <summary>
        /// <see cref="IMediaPlayer"/> will return a ControlTemplate (The actual underlying type depends on the platform) 
        /// to be displayed in the Main Window. 
        /// </summary>
        ControlTemplate,
    }

    /// <summary>
    /// A superset of <see cref="MediaSourceType"/> that contain the mode an <see cref="IMediaPlayer"/> will use.
    /// </summary>
    public struct MediaPlayerSourceType
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public MediaPlayerMode Mode { get; set; }
    }

    public enum MediaPlayerState
    {
        Playing,
        Paused,
        Closed,
    }

    public class MediaPlayerStateViewModel : INotifyPropertyChanged
    {
        public MediaPlayerState State { get => state; set { state = value; OnPropertyChanged(); } }
        private MediaPlayerState state;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public interface IMediaPlayer : IPlugin
    {
        public IEnumerable<MediaPlayerSourceType> SupportedTypes { get; }
        /// <summary>
        /// Get a ControlTemplate that can be loaded as a player. The actual underlying type depends on the platform.
        /// This method is called only when the reported mode is <see cref="MediaPlayerMode.ControlTemplate"/>.
        /// </summary>
        public ValueTask<object> GetPlayerAsync(MediaSource source, TvSeason tvSeason, TvEpisode tvEpisode);
        /// <summary>
        /// Get a <see cref="MediaPlayerStateViewModel"/> to track the state of a media player opened in a new window or process.
        /// This method is called only when the reported mode is <see cref="MediaPlayerMode.Process"/>.
        /// </summary>
        public ValueTask<MediaPlayerStateViewModel> PlayAsync(MediaSource source, TvSeason tvSeason, TvEpisode tvEpisode);
        /// <summary>
        /// If the return value is true, this media player will be used. Otherwise, an <see cref="IMediaPlayer"/> next in priority will be tried.
        /// </summary>
        public ValueTask<bool> CanPlayAsync(MediaSource source, TvSeason tvSeason, TvEpisode tvEpisode) => new ValueTask<bool>(true);
    }
}
