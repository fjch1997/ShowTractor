using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ShowTractor.Background;
using ShowTractor.Interfaces;
using ShowTractor.Plugins.Interfaces;
using ShowTractor.Tests.Mocks;
using ShowTractor.Tests.TestPlugins;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static ShowTractor.Tests.TestFixtures.ExampleSearchResults;

namespace ShowTractor.Tests
{
    [TestFixture]
    public class MetadataUpdateBackgroundWorkTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private DbConnection connection;
        private InMemoryDbContext context;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [SetUp]
        public void Setup()
        {
            connection = InMemoryDbContext.CreateConnection();
            context = new InMemoryDbContext(connection);
            context.Database.EnsureCreated();
            var season = Database.TvSeason.FromRecord(TestTvSeason1, Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty);
            season.Following = true;
            context.TvSeasons.Add(season);
            context.SaveChanges();
        }
        [TearDown]
        public void TestCleanup()
        {
            context.Dispose();
            connection.Dispose();
        }
        [Test]
        public async Task TestUpdatedSeasonAsync()
        {
            var subject = new MetadataUpdateBackgroundWork(
                new GeneralSettings(),
                new DelegateFactory<Database.ShowTractorDbContext>(() => new InMemoryDbContext(connection)),
                new DelegateFactory<IMetadataProvider?>(() => new TestMetadataProvider { TestTvSeason = TestTvSeason1Updated }),
                new System.Net.Http.HttpClient(new TestHttpMessageHandler()));
            ClassicAssert.IsTrue(await subject.CanDoWorkAsync());
            await subject.DoWorkAsync();
            using var context = new InMemoryDbContext(connection);
            ClassicAssert.AreEqual(1, context.TvSeasons.Count());
            var season = context.TvSeasons.Include(s => s.Episodes).First();
            AssertTvSeasonEqual(season, TestTvSeason1Updated);
        }
        [Test]
        public async Task TestNewSeasonAvailableAsync()
        {
            var subject = new MetadataUpdateBackgroundWork(
                   new GeneralSettings(),
                   new DelegateFactory<Database.ShowTractorDbContext>(() => new InMemoryDbContext(connection)),
                   new DelegateFactory<IMetadataProvider?>(() => new TestMetadataProvider { TestTvSeason = TestTvSeason1, MoreTvSeasons = new[] {TestTvSeason1Updated, TestTvSeason1, TestTvSeason2, TestTvSeason3 } }),
                   new System.Net.Http.HttpClient(new TestHttpMessageHandler()));
            ClassicAssert.IsTrue(await subject.CanDoWorkAsync());
            await subject.DoWorkAsync();
            using var context = new InMemoryDbContext(connection);
            ClassicAssert.AreEqual(3, context.TvSeasons.Count());
            var season = context.TvSeasons.OrderBy(t=>t.Season).Include(s => s.Episodes).Last();
            AssertTvSeasonEqual(season, TestTvSeason3);
        }
        private static void AssertTvSeasonEqual(Database.TvSeason dbSeason, TvSeason tvSeason)
        {
            ClassicAssert.That(dbSeason.SeasonDescription, Is.EqualTo(tvSeason.SeasonDescription));
            if (dbSeason.Episodes == null) throw new AssertionException($"{nameof(dbSeason.Episodes)} is null.");
            ClassicAssert.That(dbSeason.Episodes.Count, Is.EqualTo(tvSeason.Episodes.Count));
            var episode = tvSeason.Episodes.First();
            ClassicAssert.That(dbSeason.Episodes[0].Description, Is.EqualTo(episode.Description));
            ClassicAssert.That(dbSeason.Episodes[0].Name, Is.EqualTo(episode.Name));
            ClassicAssert.That(dbSeason.Episodes[0].FirstAirDate, Is.EqualTo(episode.FirstAirDate));
            ClassicAssert.That(dbSeason.Episodes[0].Runtime, Is.EqualTo(episode.Runtime));
        }

    }
}
