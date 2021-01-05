using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
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
        public async Task TestAsync()
        {
            var subject = new MetadataUpdateBackgroundWork(
                new GeneralSettings(),
                new DelegateFactory<Database.ShowTractorDbContext>(() => new InMemoryDbContext(connection)),
                new DelegateFactory<IMetadataProvider?>(() => new TestMetadataProvider { TestTvSeason = TestTvSeason1Updated }),
                new System.Net.Http.HttpClient(new TestHttpMessageHandler()));
            Assert.IsTrue(await subject.CanDoWorkAsync());
            await subject.DoWorkAsync();
            using var context = new InMemoryDbContext(connection);
            var season = context.TvSeasons.Include(s => s.Episodes).First();
            Assert.That(season.SeasonDescription, Is.EqualTo(TestTvSeason1Updated.SeasonDescription));
            if (season.Episodes == null) throw new AssertionException($"{nameof(season.Episodes)} is null.");
            Assert.That(season.Episodes.Count, Is.EqualTo(4));
            Assert.That(season.Episodes[0].Description, Is.EqualTo(TestEpisode1Updated.Description));
            Assert.That(season.Episodes[0].Name, Is.EqualTo(TestEpisode1Updated.Name));
            Assert.That(season.Episodes[0].FirstAirDate, Is.EqualTo(TestEpisode1Updated.FirstAirDate));
            Assert.That(season.Episodes[0].Runtime, Is.EqualTo(TestEpisode1Updated.Runtime));
        }
    }
}
