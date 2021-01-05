using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ShowTractor.Database.Extensions;
using ShowTractor.Interfaces;
using ShowTractor.Pages.Details;
using ShowTractor.Plugins.Interfaces;
using ShowTractor.Tests.Mocks;
using ShowTractor.Tests.TestPlugins;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using static ShowTractor.Tests.TestFixtures.ExampleSearchResults;

namespace ShowTractor.Tests
{
    [TestFixture]
    public class TvSeasonPageViewModelTests
    {
        private static readonly string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;
        private readonly HttpClient httpClient = new(new TestHttpMessageHandler());
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private DbConnection connection;
        private DelegateFactory<Database.ShowTractorDbContext> factory;
        private TestMetadataProvider provider;
        private TvSeasonPageViewModel subject;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [SetUp]
        public void TestInitialize()
        {
            connection = InMemoryDbContext.CreateConnection();
            using (var context = new InMemoryDbContext(connection))
                context.Database.EnsureCreated();
            factory = new DelegateFactory<Database.ShowTractorDbContext>(() => new InMemoryDbContext(connection));
            provider = new TestMetadataProvider();
            subject = new TvSeasonPageViewModel(new DelegateFactory<IMetadataProvider>(() => provider), httpClient, factory);
        }
        [TearDown]
        public void TestCleanup()
        {
            connection.Dispose();
        }
        [Test]
        public async Task Load_FromSearchResult_NoDatabaseEntry_NoUniqueIdTest([Values("67198", null)] string uniqueId, [Values(TestHttpMessageHandler.ImageUrl, null)] string artworkUrl)
        {
            provider.ShouldFail = true;
            var testSeason = TestTvSeason1 with { UniqueId = uniqueId, ArtworkUri = artworkUrl != null ? new Uri(artworkUrl) : null };
            subject.Parameter = new SearchResultPosterViewModel(testSeason, httpClient);
            await WaitForLoadingAsync();
            AssertTestTvSeason(testSeason);
            await TestFollowingAsync(testSeason);
        }
        [Test]
        public async Task Load_FromSearchResult_WithDatabaseEntryCreatedBySameProvider_NoUpdate_Test([Values("67198", null)] string uniqueId, [Values] bool sameProvider, [Values(TestHttpMessageHandler.ImageUrl, null)] string artworkUrl, [Values] bool following)
        {
            provider.ShouldFail = true;
            var testSeason = TestTvSeason1 with { UniqueId = uniqueId, ArtworkUri = artworkUrl != null ? new Uri(artworkUrl) : null };
            AddExistingDatabaseEntry(testSeason, sameProvider, following);
            subject.Parameter = new SearchResultPosterViewModel(testSeason, httpClient);
            await WaitForLoadingAsync();
            AssertTestTvSeason(testSeason);
            if (!following)
                await TestFollowingAsync(testSeason);
        }
        [Test]
        public async Task Load_FromSearchResult_WithDatabaseEntryCreatedByDifferentProvider_ThenUpdate_Test([Values("67198", null)] string uniqueId, [Values] bool sameProvider, [Values(TestHttpMessageHandler.ImageUrl, null)] string artworkUrl, [Values] bool following)
        {
            provider.ShouldFail = true;
            var testSeason = TestTvSeason1Updated with { UniqueId = uniqueId, ArtworkUri = artworkUrl != null ? new Uri(artworkUrl) : null };
            AddExistingDatabaseEntry(testSeason, sameProvider, following);
            subject.Parameter = new SearchResultPosterViewModel(testSeason, httpClient);
            await WaitForLoadingAsync();
            AssertTestTvSeason(testSeason);
            AssertDatabase(testSeason);
            if (!following)
                await TestFollowingAsync(testSeason);
        }
        [Test]
        public async Task Load_FromDatabase_WithUpdateTestAsync([Values("67198", null)] string uniqueId, [Values] bool withUpdate, [Values(TestHttpMessageHandler.ImageUrl, null)] string artworkUrl, [Values] bool following)
        {
            var testSeason = (withUpdate ? TestTvSeason1Updated : TestTvSeason1) with { UniqueId = uniqueId, ArtworkUri = artworkUrl != null ? new Uri(artworkUrl) : null };
            var parameter = AddExistingDatabaseEntry(testSeason, true, following);
            provider.TestTvSeason = testSeason;
            subject.Parameter = parameter;
            await WaitForLoadingAsync();
            AssertTestTvSeason(testSeason);
            AssertDatabase(testSeason);
            if (!following)
                await TestFollowingAsync(testSeason);
        }
        [Test]
        public async Task MarkSeasonAsWatchedTestAsync([Values("67198", null)] string uniqueId, [Values(TestHttpMessageHandler.ImageUrl, null)] string artworkUrl)
        {
            var testSeason = TestTvSeason1 with { UniqueId = uniqueId, ArtworkUri = artworkUrl != null ? new Uri(artworkUrl) : null };
            var parameter = AddExistingDatabaseEntry(testSeason, true, true);
            provider.TestTvSeason = testSeason;
            subject.Parameter = parameter;
            await WaitForLoadingAsync();
            Assert.IsTrue(subject.MarkSeasonAsWatchedEnabled);
            Assert.IsFalse(subject.MarkSeasonAsUnwatchedEnabled);
            AssertTestTvSeason(testSeason);
            AssertDatabase(testSeason);
            await subject.MarkSeasonAsWatched.ExecuteAsync(null);
            Assert.IsFalse(subject.MarkSeasonAsWatchedEnabled);
            Assert.IsTrue(subject.MarkSeasonAsUnwatchedEnabled);
            using var context = factory.Get();
            var dbSeason = GetTvSeasonFromDb(context, testSeason);
            for (int i = 0; i < subject.Episodes.Count; i++)
            {
                if (subject.Episodes[i].Aired)
                {
                    Assert.AreEqual(100, subject.Episodes[i].WatchProgressPercentage);
                    Assert.AreEqual(TimeSpan.MaxValue, subject.Episodes[i].WatchProgress);
                    Assert.AreEqual(TimeSpan.MaxValue, (dbSeason.Episodes ?? throw new Exception())[i].WatchProgress);
                }
            }
        }
        [Test]
        public async Task MarkSeasonAsUnwatchedTestAsync([Values("67198", null)] string uniqueId, [Values(TestHttpMessageHandler.ImageUrl, null)] string artworkUrl)
        {
            var testSeason = TestTvSeason1 with { UniqueId = uniqueId, ArtworkUri = artworkUrl != null ? new Uri(artworkUrl) : null };
            var parameter = AddExistingDatabaseEntry(testSeason, true, true);
            provider.TestTvSeason = testSeason;
            using (var context = factory.Get())
            {
                var dbSeason = GetTvSeasonFromDb(context, testSeason);
                foreach (var episode in dbSeason.Episodes ?? throw new Exception())
                {
                    episode.WatchProgress = TimeSpan.MaxValue;
                }
                context.SaveChanges();
            }
            subject.Parameter = parameter;
            await WaitForLoadingAsync();
            Assert.IsFalse(subject.MarkSeasonAsWatchedEnabled);
            Assert.IsTrue(subject.MarkSeasonAsUnwatchedEnabled);
            AssertTestTvSeason(testSeason);
            AssertDatabase(testSeason);
            await subject.MarkSeasonAsUnwatched.ExecuteAsync(null);
            Assert.IsFalse(subject.MarkSeasonAsUnwatchedEnabled);
            Assert.IsTrue(subject.MarkSeasonAsWatchedEnabled);
            using (var context = factory.Get())
            {
                var dbSeason = GetTvSeasonFromDb(context, testSeason);
                for (int i = 0; i < subject.Episodes.Count; i++)
                {
                    if (subject.Episodes[i].Aired)
                    {
                        Assert.AreEqual(0, subject.Episodes[i].WatchProgressPercentage);
                        Assert.AreEqual(TimeSpan.Zero, subject.Episodes[i].WatchProgress);
                        Assert.AreEqual(TimeSpan.Zero, (dbSeason.Episodes ?? throw new Exception())[i].WatchProgress);
                    }
                }
            }
        }
        [Test]
        public async Task MarkEpisodeAsWatchedTestAsync([Values("67198", null)] string uniqueId, [Values(TestHttpMessageHandler.ImageUrl, null)] string artworkUrl)
        {
            var testSeason = TestTvSeason1 with { UniqueId = uniqueId, ArtworkUri = artworkUrl != null ? new Uri(artworkUrl) : null };
            var parameter = AddExistingDatabaseEntry(testSeason, true, true);
            provider.TestTvSeason = testSeason;
            subject.Parameter = parameter;
            await WaitForLoadingAsync();
            Assert.IsTrue(subject.MarkSeasonAsWatchedEnabled);
            Assert.IsFalse(subject.MarkSeasonAsUnwatchedEnabled);
            AssertTestTvSeason(testSeason);
            AssertDatabase(testSeason);
            await subject.Episodes[0].MarkAsWatched.ExecuteAsync(null);
            using var context = factory.Get();
            var dbSeason = GetTvSeasonFromDb(context, testSeason);
            Assert.AreEqual(100, subject.Episodes[0].WatchProgressPercentage);
            Assert.AreEqual(TimeSpan.MaxValue, subject.Episodes[0].WatchProgress);
            Assert.AreEqual(TimeSpan.MaxValue, (dbSeason.Episodes ?? throw new Exception())[0].WatchProgress);
            Assert.IsTrue(subject.MarkSeasonAsWatchedEnabled);
            Assert.IsTrue(subject.MarkSeasonAsUnwatchedEnabled);
        }
        [Test]
        public async Task MarkEpisodeAsUnwatchedTestAsync([Values("67198", null)] string uniqueId, [Values(TestHttpMessageHandler.ImageUrl, null)] string artworkUrl)
        {
            var testSeason = TestTvSeason1 with { UniqueId = uniqueId, ArtworkUri = artworkUrl != null ? new Uri(artworkUrl) : null };
            var parameter = AddExistingDatabaseEntry(testSeason, true, true);
            provider.TestTvSeason = testSeason;
            using (var context = factory.Get())
            {
                var dbSeason = GetTvSeasonFromDb(context, testSeason);
                (dbSeason.Episodes ?? throw new Exception())[0].WatchProgress = TimeSpan.MaxValue;
                context.SaveChanges();
            }
            subject.Parameter = parameter;
            await WaitForLoadingAsync();
            Assert.IsTrue(subject.MarkSeasonAsWatchedEnabled);
            Assert.IsTrue(subject.MarkSeasonAsUnwatchedEnabled);
            AssertTestTvSeason(testSeason);
            AssertDatabase(testSeason);
            await subject.Episodes[0].MarkAsUnwatched.ExecuteAsync(null);
            using (var context = factory.Get())
            {
                var dbSeason = GetTvSeasonFromDb(context, testSeason);
                Assert.AreEqual(0, subject.Episodes[0].WatchProgressPercentage);
                Assert.AreEqual(TimeSpan.Zero, subject.Episodes[0].WatchProgress);
                Assert.AreEqual(TimeSpan.Zero, (dbSeason.Episodes ?? throw new Exception())[0].WatchProgress);
                Assert.IsTrue(subject.MarkSeasonAsWatchedEnabled);
                Assert.IsFalse(subject.MarkSeasonAsUnwatchedEnabled);
            }
        }
        [Test]
        public async Task MarkAllSeasonsTestAsync([Values] bool watched)
        {
            AddExistingDatabaseEntry(TestTvSeason1, true, true, watched);
            AddExistingDatabaseEntry(TestTvSeason2, true, true, watched);
            var parameter = AddExistingDatabaseEntry(TestTvSeason3, true, true, watched);
            provider.TestTvSeason = TestTvSeason3;
            subject.Parameter = parameter;
            await WaitForLoadingAsync();
            AssertTestTvSeason(TestTvSeason3);
            AssertDatabase(TestTvSeason3);
            if (watched)
                await subject.MarkAllSeasonsAsUnwatched.ExecuteAsync(null);
            else
                await subject.MarkAllSeasonsAsWatched.ExecuteAsync(null);
            using var context = factory.Get();
            AssertSeason(GetTvSeasonFromDb(context, TestTvSeason1), false);
            AssertSeason(GetTvSeasonFromDb(context, TestTvSeason2), false);
            AssertSeason(GetTvSeasonFromDb(context, TestTvSeason3), true);

            void AssertSeason(Database.TvSeason dbSeason, bool viewModel)
            {
                if (dbSeason.Episodes == null) throw new Exception(nameof(dbSeason.Episodes));

                for (int i = 0; i < dbSeason.Episodes.Count - 1; i++)
                {
                    if (viewModel)
                    {
                        Assert.AreEqual(watched ? 0 : 100, subject.Episodes[i].WatchProgressPercentage);
                        Assert.AreEqual(watched ? TimeSpan.Zero : TimeSpan.MaxValue, subject.Episodes[i].WatchProgress);
                    }
                    Assert.AreEqual(watched ? TimeSpan.Zero : TimeSpan.MaxValue, (dbSeason.Episodes ?? throw new Exception())[i].WatchProgress);
                }
            }
        }
        private void AssertDatabase(TvSeason tvSeason)
        {
            using var context = factory.Get();
            var dbSeason = GetTvSeasonFromDb(context, tvSeason);

            Assert.AreEqual(tvSeason.Episodes.Count, (dbSeason.Episodes ?? throw new Exception()).Count);
            Assert.AreEqual(tvSeason.Season, dbSeason.Season);
            Assert.AreEqual(tvSeason.SeasonDescription, dbSeason.SeasonDescription);
            Assert.AreEqual(tvSeason.ShowDescription, dbSeason.ShowDescription);
            Assert.AreEqual(tvSeason.ShowName, dbSeason.ShowName);
        }
        private static Database.TvSeason GetTvSeasonFromDb(Database.ShowTractorDbContext context, TvSeason tvSeason)
        {
            return context.TvSeasons.Include(nameof(Database.TvSeason.Episodes)).Where(s => s.ShowName == tvSeason.ShowName && s.Season == tvSeason.Season).First();
        }
        private SavedPosterViewModel AddExistingDatabaseEntry(TvSeason testSeason, bool sameProvider, bool following, bool watched = false)
        {
            var dbSeason = Database.TvSeason.FromRecord(testSeason, sameProvider ? assemblyName : "ThirdParty.Plugin");
            dbSeason.Following = following;
            dbSeason.Episodes = new List<Database.TvEpisode>();
            using (var context = factory.Get())
            {
                context.TvSeasons.Add(dbSeason);
                foreach (var episode in testSeason.Episodes)
                {
                    var dbEpisode = Database.TvEpisode.FromRecord(episode);
                    if (dbEpisode.Aired())
                        dbEpisode.WatchProgress = watched ? TimeSpan.MaxValue : TimeSpan.Zero;
                    dbSeason.Episodes.Add(dbEpisode);
                }
                context.SaveChanges();
            }
            return new LibraryPosterViewModel(dbSeason.Id, dbSeason.ShowName, dbSeason.Season, testSeason.Episodes.First().FirstAirDate, factory);
        }
        private async Task TestFollowingAsync(TvSeason tvSeason)
        {
            Assert.IsFalse(subject.Following);
            await subject.FollowCommand.ExecuteAsync(null);
            using (var context = factory.Get())
            {
                var dbSeason = context.TvSeasons.First();
                Assert.IsTrue(dbSeason.Following);
                Assert.AreEqual(tvSeason.ShowName, dbSeason.ShowName);
                Assert.AreEqual(context.TvEpisodes.Count(), tvSeason.Episodes.Count);
                Assert.AreEqual(context.TvSeasons.First().DateFollowed, DateTime.Today);
                await subject.UnfollowCommand.ExecuteAsync(null);
                await WaitForPropertyAsync(nameof(subject.Following));
            }
            Assert.IsFalse(subject.Following);
            using (var context = factory.Get())
            {
                Assert.IsFalse(context.TvSeasons.First().Following);
            }
        }
        private void AssertTestTvSeason(TvSeason tvSeason)
        {
            Assert.IsTrue(string.IsNullOrEmpty(subject.ErrorMessage), subject.ErrorMessage);
            Assert.AreEqual(tvSeason.Episodes.Count, subject.Episodes.Count);
            Assert.AreEqual(tvSeason.Season, subject.Season);
            Assert.AreEqual(tvSeason.SeasonDescription, subject.SeasonDescription);
            Assert.AreEqual(tvSeason.ShowDescription, subject.ShowDescription);
            Assert.AreEqual(tvSeason.ShowName, subject.ShowName);
        }
        private async Task WaitForLoadingAsync()
        {
            while (subject.Loading)
            {
                var tcs = new TaskCompletionSource<bool>();
                void handler(object? s, System.ComponentModel.PropertyChangedEventArgs e) => tcs.TrySetResult(false);
                subject.PropertyChanged += handler;
                await tcs.Task;
                subject.PropertyChanged -= handler;
            }
        }
        private async Task WaitForPropertyAsync(string propertyName)
        {
            if (subject.Loading)
            {
                var tcs = new TaskCompletionSource<bool>();
                _ = Task.Delay(5000).ContinueWith(_ => tcs.TrySetCanceled());
                subject.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == propertyName)
                        tcs.SetResult(false);
                };
                await tcs.Task;
            }
        }
    }
}
