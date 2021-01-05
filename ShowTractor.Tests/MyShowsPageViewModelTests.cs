using NUnit.Framework;
using ShowTractor.Interfaces;
using ShowTractor.Pages;
using ShowTractor.Pages.Details;
using ShowTractor.Plugins.Interfaces;
using ShowTractor.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static ShowTractor.Tests.TestFixtures.ExampleSearchResults;

namespace ShowTractor.Tests
{
    [TestFixture]
    public class MyShowsPageViewModelTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private MyShowsPageViewModel subject;
        private DbConnection connection;
        private InMemoryDbContext context;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [SetUp]
        public void Setup()
        {
            connection = InMemoryDbContext.CreateConnection();
            context = new InMemoryDbContext(connection);
            context.Database.EnsureCreated();
            AddSeason(TestTvSeason1, true);
            AddSeason(TestTvSeason2, true);
            AddSeason(TestTvSeason3, true);
            AddSeason(TestTvSeason4, true);
            AddSeason(TestTvSeason5);
            AddSeason(TestTvSeason6);
            subject = new MyShowsPageViewModel(new DelegateFactory<Database.ShowTractorDbContext>(() => new InMemoryDbContext(connection)));
        }
        [TearDown]
        public void TestCleanup()
        {
            context.Dispose();
            connection.Dispose();
        }
        private void AddSeason(TvSeason data, bool followed = false)
        {
            var season = Database.TvSeason.FromRecord(data, Assembly.GetExecutingAssembly().GetName().Name ?? throw new Exception());
            season.Following = followed;
            context.TvSeasons.Add(season);
            context.SaveChanges();
            AddEpisodesToSeason(season, data.Episodes);
        }
        private void AddEpisodesToSeason(Database.TvSeason season, IEnumerable<TvEpisode> episodes)
        {
            foreach (var item in episodes)
            {
                var episode = Database.TvEpisode.FromRecord(item);
                episode.TvSeasonId = season.Id;
                context.TvEpisodes.Add(episode);
            }
            context.SaveChanges();
        }
        [TestCase]
        public async Task AllFollowedTest()
        {
            var libraryVm = subject.AllFollowed;
            await libraryVm.WaitForLoadingAsync();
            Assert.That(libraryVm.ErrorMessage, Is.Empty);
            if (libraryVm.View == null) throw new AssertionException($"{nameof(libraryVm.View)} is null.");
            Assert.That(((IEnumerable<PosterViewModel>)libraryVm.View).Count(), Is.EqualTo(4));
            foreach (var season in (IEnumerable<PosterViewModel>)libraryVm.View)
            {
                Assert.That(season.ShowName + season.Season, Is.AnyOf(TestTvSeason1.ShowName + TestTvSeason1.Season, TestTvSeason2.ShowName + TestTvSeason2.Season, TestTvSeason3.ShowName + TestTvSeason3.Season, TestTvSeason4.ShowName + TestTvSeason4.Season));
                Assert.That(season.ShowName + season.Season, Is.Not.AnyOf(TestTvSeason5.ShowName + TestTvSeason5.Season, TestTvSeason6.ShowName + TestTvSeason6.Season));
            }
        }
        [TestCase]
        public async Task CurrentShowsTest()
        {
            var libraryVm = subject.CurrentShows;
            await libraryVm.WaitForLoadingAsync();
            Assert.That(libraryVm.ErrorMessage, Is.Empty);
            if (libraryVm.View == null) throw new AssertionException($"{nameof(libraryVm.View)} is null.");
            Assert.That(((IEnumerable<PosterViewModel>)libraryVm.View).Count(), Is.EqualTo(3));
            foreach (var season in (IEnumerable<PosterViewModel>)libraryVm.View)
            {
                Assert.That(season.ShowName + season.Season, Is.AnyOf(TestTvSeason1.ShowName + TestTvSeason1.Season, TestTvSeason2.ShowName + TestTvSeason2.Season, TestTvSeason3.ShowName + TestTvSeason3.Season));
                Assert.That(season.ShowName + season.Season, Is.Not.AnyOf(TestTvSeason4.ShowName + TestTvSeason4.Season, TestTvSeason5.ShowName + TestTvSeason5.Season, TestTvSeason6.ShowName + TestTvSeason6.Season));
            }
        }
        [TestCase]
        public async Task EndedShowsTest()
        {
            var libraryVm = subject.EndedShows;
            await libraryVm.WaitForLoadingAsync();
            Assert.That(libraryVm.ErrorMessage, Is.Empty);
            if (libraryVm.View == null) throw new AssertionException($"{nameof(libraryVm.View)} is null.");
            Assert.That(((IEnumerable<PosterViewModel>)libraryVm.View).Count(), Is.EqualTo(1));
            foreach (var season in (IEnumerable<PosterViewModel>)libraryVm.View)
            {
                Assert.That(season.ShowName + season.Season, Is.AnyOf(TestTvSeason4.ShowName + TestTvSeason4.Season));
                Assert.That(season.ShowName + season.Season, Is.Not.AnyOf(TestTvSeason1.ShowName + TestTvSeason1.Season, TestTvSeason2.ShowName + TestTvSeason2.Season, TestTvSeason3.ShowName + TestTvSeason3.Season, TestTvSeason5.ShowName + TestTvSeason5.Season, TestTvSeason6.ShowName + TestTvSeason6.Season));
            }
        }
        [TestCase]
        public async Task UnfollowedTest()
        {
            var libraryVm = subject.Unfollowed;
            await libraryVm.WaitForLoadingAsync();
            Assert.That(libraryVm.ErrorMessage, Is.Empty);
            if (libraryVm.View == null) throw new AssertionException($"{nameof(libraryVm.View)} is null.");
            Assert.That(((IEnumerable<PosterViewModel>)libraryVm.View).Count(), Is.EqualTo(2));
            foreach (var season in (IEnumerable<PosterViewModel>)libraryVm.View)
            {
                Assert.That(season.ShowName + season.Season, Is.AnyOf(TestTvSeason5.ShowName + TestTvSeason5.Season, TestTvSeason6.ShowName + TestTvSeason6.Season));
                Assert.That(season.ShowName + season.Season, Is.Not.AnyOf(TestTvSeason3.ShowName + TestTvSeason3.Season, TestTvSeason4.ShowName + TestTvSeason4.Season, TestTvSeason1.ShowName + TestTvSeason1.Season, TestTvSeason2.ShowName + TestTvSeason2.Season));
            }
        }
    }
}
