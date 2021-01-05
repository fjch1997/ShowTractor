using NUnit.Framework;
using ShowTractor.Interfaces;
using ShowTractor.Pages;
using ShowTractor.Pages.Details;
using ShowTractor.Plugins.Interfaces;
using ShowTractor.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static ShowTractor.Tests.TestFixtures.ExampleSearchResults;

namespace ShowTractor.Tests
{
    [TestFixture]
    public class UnwatchedPageViewModelTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private UnwatchedPageViewModel subject;
        private DbConnection connection;
        private InMemoryDbContext context;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [SetUp]
        public void Setup()
        {
            connection = InMemoryDbContext.CreateConnection();
            context = new InMemoryDbContext(connection);
            context.Database.EnsureCreated();
            AddSeason(TestTvSeason1, 0);
            AddSeason(TestTvSeason2, 0);
            AddSeason(TestTvSeason3, 3);
            AddSeason(TestTvSeason4, 3);
            AddSeason(TestTvSeason5, 3);
            AddSeason(TestTvSeason6, 3);
            subject = new UnwatchedPageViewModel(new DelegateFactory<Database.ShowTractorDbContext>(() => new InMemoryDbContext(connection)), new GeneralSettings());
        }
        [TearDown]
        public void TestCleanup()
        {
            context.Dispose();
            connection.Dispose();
        }
        private void AddSeason(TvSeason data, int watched = 0)
        {
            var season = Database.TvSeason.FromRecord(data, Assembly.GetExecutingAssembly().GetName().Name ?? "");
            season.Following = true;
            context.TvSeasons.Add(season);
            context.SaveChanges();
            AddEpisodesToSeason(season, data.Episodes, watched);
        }
        private void AddEpisodesToSeason(Database.TvSeason season, IEnumerable<TvEpisode> episodes, int watched)
        {
            var watchedCreated = 0;
            foreach (var item in episodes)
            {
                var episode = Database.TvEpisode.FromRecord(item);
                episode.TvSeasonId = season.Id;
                if (watchedCreated < watched)
                {
                    episode.WatchProgress = TimeSpan.MaxValue;
                    watchedCreated++;
                }
                else
                {
                    episode.WatchProgress = TimeSpan.Zero;
                }
                context.TvEpisodes.Add(episode);
            }
            context.SaveChanges();
        }
        [Test]
        public async Task TestAsync()
        {
            var libraryVm = subject.LibraryViewModel;
            await libraryVm.WaitForLoadingAsync();
            Assert.That(libraryVm.ErrorMessage, Is.Empty);
            Assert.That(((IEnumerable<PosterViewModel>?)libraryVm.View)?.Count(), Is.EqualTo(2));
            if (libraryVm.View == null) throw new Exception($"{nameof(libraryVm.View)} is null.");
            foreach (var season in (IEnumerable<PosterViewModel>)libraryVm.View)
            {
                Assert.That(season.Unwatched, Is.EqualTo(2));
                Assert.That(season.ShowUnwatched, Is.True);
                Assert.That(season.ShowName, Is.AnyOf(TestTvSeason1.ShowName, TestTvSeason2.ShowName));
                Assert.That(season.Season, Is.AnyOf(TestTvSeason1.Season, TestTvSeason2.Season));
            }
            await ((Func<bool>)(() => subject.TotalTimeUnwatched != TimeSpan.Zero)).WaitForTrueAsync();
            Assert.That(subject.TotalTimeUnwatched.TotalMinutes, Is.EqualTo(168D).Within(1D));
        }
        [Test]
        public async Task TotalTimeUnwatchedDisplayTextTestAsync()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var libraryVm = subject.LibraryViewModel;
            await libraryVm.WaitForLoadingAsync();
            await ((Func<bool>)(() => subject.TotalTimeUnwatched != TimeSpan.Zero)).WaitForTrueAsync();
            subject.TotalTimeUnwatched = TimeSpan.FromMinutes(30);
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.Empty);
            subject.TotalTimeUnwatched = TimeSpan.FromHours(1.3);
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 hour"));
            subject.TotalTimeUnwatched = TimeSpan.FromHours(2);
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("2 hours"));
            subject.TotalTimeUnwatched = TimeSpan.FromHours(2.3);
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("2 hours"));

            subject.TotalTimeUnwatched = TimeSpan.FromDays(1).Add(TimeSpan.FromHours(0.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 day"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1.3));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 day, 1 hour"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(1).Add(TimeSpan.FromHours(2.3));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 day, 2 hours"));

            subject.TotalTimeUnwatched = TimeSpan.FromDays(30).Add(TimeSpan.FromMinutes(0.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 month"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(30).Add(TimeSpan.FromHours(0.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 month"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(30).Add(TimeSpan.FromHours(2.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 month, 2 hours"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(30).Add(TimeSpan.FromDays(1)).Add(TimeSpan.FromHours(2.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 month, 1 day, 2 hours"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(30).Add(TimeSpan.FromDays(2)).Add(TimeSpan.FromHours(2.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 month, 2 days, 2 hours"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(60).Add(TimeSpan.FromDays(2)).Add(TimeSpan.FromHours(2.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("2 months, 2 days, 2 hours"));

            subject.TotalTimeUnwatched = TimeSpan.FromDays(365).Add(TimeSpan.FromHours(0.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 year"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(365).Add(TimeSpan.FromDays(0.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 year"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(365).Add(TimeSpan.FromHours(2.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 year"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(365).Add(TimeSpan.FromDays(1)).Add(TimeSpan.FromHours(2.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 year, 1 day"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(365).Add(TimeSpan.FromDays(2)).Add(TimeSpan.FromHours(2.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 year, 2 days"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(395).Add(TimeSpan.FromDays(1)).Add(TimeSpan.FromHours(2.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("1 year, 1 month, 1 day"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(365 * 2 + 30).Add(TimeSpan.FromDays(2)).Add(TimeSpan.FromHours(2.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("2 years, 1 month, 2 days"));
            subject.TotalTimeUnwatched = TimeSpan.FromDays(365 * 2 + 30).Add(TimeSpan.FromDays(2)).Add(TimeSpan.FromHours(2.5));
            Assert.That(subject.TotalTimeUnwatchedDisplayText, Is.EqualTo("2 years, 1 month, 2 days"));
        }
    }
}
