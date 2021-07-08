using ShowTractor.Plugins;
using ShowTractor.Plugins.Interfaces;
using System;
using System.Threading.Tasks;

namespace ShowTractor.Tests.Mocks
{
    public class TestAggregateMediaPlayer : IAggregateMediaPlayer
    {
        private readonly MediaPlayerStateViewModel vm;

        public TestAggregateMediaPlayer(MediaPlayerStateViewModel vm)
        {
            this.vm = vm ?? throw new ArgumentNullException(nameof(vm));
        }
        public Task<(object? controlTemplate, MediaPlayerStateViewModel? vm)> PlayAsync(MediaSource source, TvSeason tvSeason, TvEpisode tvEpisode)
        {
            if (vm != null)
                return Task.FromResult<(object?, MediaPlayerStateViewModel?)>((null, this.vm));
            return Task.FromResult<(object?, MediaPlayerStateViewModel?)>((null, null));
        }
    }
}
