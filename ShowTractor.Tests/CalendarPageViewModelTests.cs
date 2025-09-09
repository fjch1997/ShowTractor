using NUnit.Framework;
using NUnit.Framework.Legacy;
using ShowTractor.Interfaces;
using ShowTractor.Pages;
using ShowTractor.Plugins.Interfaces;
using ShowTractor.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ShowTractor.Tests.TestFixtures.ExampleSearchResults;

namespace ShowTractor.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class CalendarPageViewModelTests : IAsyncInitializationService
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private DbConnection connection;
        private DelegateFactory<Database.ShowTractorDbContext> factory;
        private CalendarPageViewModel subject;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Task Task => Task.FromResult(false);

        [SetUp]
        public void TestInitialize()
        {
            connection = InMemoryDbContext.CreateConnection();
            using (var context = new InMemoryDbContext(connection))
                context.Database.EnsureCreated();
            factory = new DelegateFactory<Database.ShowTractorDbContext>(() => new InMemoryDbContext(connection));
            AddExistingDatabaseEntry(TestTvSeason1, true, true);
            AddExistingDatabaseEntry(TestTvSeason2, true, false);
            AddExistingDatabaseEntry(TestTvSeason3, true, false);
            AddExistingDatabaseEntry(TestTvSeason4, false, false);
            AddExistingDatabaseEntry(TestTvSeason5, false, false);
            AddExistingDatabaseEntry(TestTvSeason6, false, false);
            subject = new CalendarPageViewModel(factory, this, new GeneralSettings());
        }
        [TearDown]
        public void TestCleanup()
        {
            connection.Dispose();
        }
        [Test]
        public async Task LoadTestAsync()
        {
            // Load a range of dates to simulate the calendar control.
            var date = new DateTimeOffset(DateTime.SpecifyKind(TestTvSeason1.Episodes[0].FirstAirDate, DateTimeKind.Utc));
            date.AddDays(20);
            for (int i = 0; i < 45; i++)
            {
                subject.CalendarDayItemDataContextProvider(date);
                date.AddDays(1);
            }
            await AssertTvSeasonAsync(TestTvSeason1, true);
            await AssertTvSeasonAsync(TestTvSeason2, true);
            await AssertTvSeasonAsync(TestTvSeason3, true);
            await AssertTvSeasonAsync(TestTvSeason4, false);
            await AssertTvSeasonAsync(TestTvSeason5, false);
            await AssertTvSeasonAsync(TestTvSeason6, false);
        }
        [Test]
        public async Task MarkTestAsync([Values] bool watched)
        {
            var testSeason = watched ? TestTvSeason1 : TestTvSeason2;
            var vm = (CalendarDayViewModel)subject.CalendarDayItemDataContextProvider(DateTime.SpecifyKind(testSeason.Episodes[0].FirstAirDate, DateTimeKind.Utc));
            await WaitForViewModelToLoadAsync(vm);
            var episode = (vm.TvEpisodes ?? throw new Exception()).First(e => e.Season == testSeason.Season && e.ShowName == testSeason.ShowName && e.EpisodeNumber == testSeason.Episodes[0].EpisodeNumber);
            ClassicAssert.That(episode.Watched, Is.EqualTo(watched));
            episode.Watched = !watched;
            await ((Func<bool>)(() => !episode.Loading)).WaitForTrueAsync();
            ClassicAssert.That(episode.Watched, Is.EqualTo(!watched));
            using var context = factory.Get();
            var dbEpisode = ((IQueryable<Database.TvEpisode>)context.TvEpisodes).First(e => e.TvSeason.Season == testSeason.Season && e.TvSeason.ShowName == testSeason.ShowName && e.EpisodeNumber == testSeason.Episodes[0].EpisodeNumber);
            if (watched)
                ClassicAssert.That(dbEpisode.WatchProgress, Is.EqualTo(TimeSpan.Zero));
            else
                ClassicAssert.That(dbEpisode.WatchProgress, Is.GreaterThanOrEqualTo(dbEpisode.Runtime));
        }
        private async Task AssertTvSeasonAsync(TvSeason season, bool shouldExist)
        {
            foreach (var episode in season.Episodes)
            {
                if (episode.FirstAirDate == default || episode.FirstAirDate.Kind != DateTimeKind.Utc)
                    continue;
                var vm = (CalendarDayViewModel)subject.CalendarDayItemDataContextProvider(episode.FirstAirDate);
                await WaitForViewModelToLoadAsync(vm);
                ClassicAssert.AreEqual(shouldExist, vm.TvEpisodes?.Any(e => e.EpisodeName == episode.Name && e.EpisodeNumber == episode.EpisodeNumber && e.Season == season.Season && e.ShowName == season.ShowName));
            }
        }
        private static async Task WaitForViewModelToLoadAsync(CalendarDayViewModel vm)
        {
            if (vm.TvEpisodes == null)
            {
                var tcs = new TaskCompletionSource<bool>();
                if (!Debugger.IsAttached)
                    _ = Task.Delay(5000).ContinueWith(t => tcs.TrySetCanceled());
                PropertyChangedEventHandler? handler = null;
                handler = new PropertyChangedEventHandler((s, e) =>
                {
                    tcs.SetResult(false);
                    vm.PropertyChanged -= handler;
                });
                vm.PropertyChanged += handler;
                await tcs.Task;
            }
        }
        private void AddExistingDatabaseEntry(TvSeason testSeason, bool following, bool watched)
        {
            var dbSeason = Database.TvSeason.FromRecord(testSeason, "ShowTractor.Tests");
            dbSeason.Following = following;
            dbSeason.Episodes = new List<Database.TvEpisode>();
            using var context = factory.Get();
            context.TvSeasons.Add(dbSeason);
            foreach (var episode in testSeason.Episodes)
            {
                var dbEpisode = Database.TvEpisode.FromRecord(episode);
                dbEpisode.WatchProgress = watched ? TimeSpan.MaxValue : TimeSpan.Zero;
                dbSeason.Episodes.Add(dbEpisode);
            }
            context.SaveChanges();
        }
    }
}
