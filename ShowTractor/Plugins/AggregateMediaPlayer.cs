using ShowTractor.Plugins.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShowTractor.Plugins
{
    public class AggregateMediaPlayer : IAggregateMediaPlayer
    {
        private PluginSettings settings;
        private IServiceProvider serviceProvider;


        public AggregateMediaPlayer(PluginSettings settings, IServiceProvider serviceProvider)
        {
            this.settings = settings;
            this.serviceProvider = serviceProvider;
        }

        public async Task<(object? controlTemplate, MediaPlayerStateViewModel? vm)> PlayAsync(MediaSource source, TvSeason tvSeason, TvEpisode tvEpisode)
        {
            foreach (var definition in settings.MediaPlayers.Where(p => p.Enabled))
            {
                var provider = definition.Load<IMediaPlayer>(serviceProvider);
                var type = provider.SupportedTypes.FirstOrDefault(t => t.Id == source.Type.Id);
                if (type.Id != default && await provider.CanPlayAsync(source, tvSeason, tvEpisode))
                {
                    if (type.Mode == MediaPlayerMode.ControlTemplate)
                    {
                        return (await provider.GetPlayerAsync(source, tvSeason, tvEpisode), null);
                    }
                    else
                    {
                        return (null, await provider.PlayAsync(source, tvSeason, tvEpisode));
                    }
                }
            }
            return (null, null);
        }
    }
}
