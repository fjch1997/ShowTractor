using ShowTractor.Plugins.Interfaces;
using ShowTractor.Plugins.ShellMediaPlayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

[assembly: ShowTractorPluginAssembly(MediaPlayer = typeof(ShellMediaPlayer))]
namespace ShowTractor.Plugins.ShellMediaPlayer
{
    public class ShellMediaPlayer : IMediaPlayer
    {
        public IEnumerable<MediaPlayerSourceType> SupportedTypes => new[]
        {
            new MediaPlayerSourceType
            {
                Id = PredefinedMediaSources.LocalFile.Id,
                DisplayName = PredefinedMediaSources.LocalFile.DisplayName,
                Mode = MediaPlayerMode.Process,
            },
        };

        public string Name => "Windows Shell Player";

        public PluginSettingsDescriptions PluginSettingsDescriptions => null;

        public Stream GetIconStream() => Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(ShowTractor)}.{nameof(Plugins)}.{nameof(ShellMediaPlayer)}.logo.png");

        public ValueTask<object> GetPlayerAsync(MediaSource source, TvSeason tvSeason, TvEpisode tvEpisode)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<MediaPlayerStateViewModel> PlayAsync(MediaSource source, TvSeason tvSeason, TvEpisode tvEpisode)
        {
            var p = await Task.Run(() => Process.Start(new ProcessStartInfo
            {
                FileName = ((GenericMediaSource<string>)source).Value,
                UseShellExecute = true,
            }));

            var vm = new ShellMediaPlayerStateViewModel(p);

            return vm;
        }
    }
}
