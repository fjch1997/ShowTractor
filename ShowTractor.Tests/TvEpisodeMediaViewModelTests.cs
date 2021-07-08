using NUnit.Framework;
using ShowTractor.Pages.Details;
using ShowTractor.Plugins.Interfaces;
using ShowTractor.Tests.Mocks;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ShowTractor.Tests.TestFixtures.ExampleSearchResults;

namespace ShowTractor.Tests
{
    [TestFixture]
    public class TvEpisodeMediaViewModelTests
    {
        [TestCase]
        public async Task ShowPlayTestAsync()
        {
            var vm = new MediaPlayerStateViewModel();
            var subject = new TvEpisodeMediaViewModel(
                TestTvSeason1,
                TestEpisode1,
                new TestAggregateMediaSourceProvider(Enumerable.Repeat(GetTestSource(), 1)),
                new TestAggregateMediaPlayer(vm));
            await subject.initializationTask;
            Assert.True(subject.ShowPlay);
            Assert.False(subject.ShowMediaUnavailable);
            Assert.False(subject.ShowPlayerUnavailable);
            Assert.False(subject.ShowPlaying);
        }

        [TestCase]
        public async Task NoMediaTestAsync()
        {
            var vm = new MediaPlayerStateViewModel();
            var subject = new TvEpisodeMediaViewModel(
                TestTvSeason1,
                TestEpisode1,
                new TestAggregateMediaSourceProvider(Enumerable.Empty<MediaSource>()),
                new TestAggregateMediaPlayer(vm));
            await subject.initializationTask;
            Assert.False(subject.ShowPlay);
            Assert.True(subject.ShowMediaUnavailable);
            Assert.False(subject.ShowPlayerUnavailable);
            Assert.False(subject.ShowPlaying);
        }

        [TestCase]
        public async Task RememberExternalPlayerStateTestAsync()
        {
            var vm = new MediaPlayerStateViewModel();
            var subject = new TvEpisodeMediaViewModel(
                TestTvSeason1,
                TestEpisode1,
                new TestAggregateMediaSourceProvider(Enumerable.Repeat(GetTestSource(), 1)),
                new TestAggregateMediaPlayer(vm));
            await subject.initializationTask;
            Assert.True(subject.ShowPlay);
            Assert.False(subject.ShowMediaUnavailable);
            Assert.False(subject.ShowPlayerUnavailable);
            Assert.False(subject.ShowPlaying);
            await subject.PlayCommand.ExecuteAsync(null);
            Assert.AreEqual(vm.State, MediaPlayerState.Playing);
            Assert.False(subject.ShowPlay);
            Assert.False(subject.ShowMediaUnavailable);
            Assert.False(subject.ShowPlayerUnavailable);
            Assert.True(subject.ShowPlaying);

            // Open second instance.
            subject = new TvEpisodeMediaViewModel(
                TestTvSeason1,
                TestEpisode1,
                new TestAggregateMediaSourceProvider(Enumerable.Repeat(GetTestSource(), 1)),
                new TestAggregateMediaPlayer(vm));
            await subject.initializationTask;
            Assert.False(subject.ShowPlay);
            Assert.False(subject.ShowMediaUnavailable);
            Assert.False(subject.ShowPlayerUnavailable);
            Assert.True(subject.ShowPlaying);

            vm.State = MediaPlayerState.Closed;

            Assert.True(subject.ShowPlay);
            Assert.False(subject.ShowMediaUnavailable);
            Assert.False(subject.ShowPlayerUnavailable);
            Assert.False(subject.ShowPlaying);
        }

        [TestCase]
        public async Task NoSupportedPlayerTestAsync()
        {

        }

        [TestCase]
        public async Task TurnsOutToHaveNoSupportedPlayerTestAsync()
        {

        }

        private static GenericMediaSource<string> GetTestSource() =>
            new GenericMediaSource<string>(
                PredefinedMediaSources.LocalFile,
                Path.Combine(Path.GetTempPath(), "Test.mp4"),
                MediaResolution.SevenTwentyP,
                MediaCodec.H254,
                "Test.mp4",
                12924563);
    }
}
