using ShowTractor.Plugins.Interfaces;
using System.Threading.Tasks;

namespace ShowTractor.Plugins
{
    public interface IAggregateMediaPlayer
    {
        Task<(object? controlTemplate, MediaPlayerStateViewModel? vm)> PlayAsync(MediaSource source, TvSeason tvSeason, TvEpisode tvEpisode);
    }
}