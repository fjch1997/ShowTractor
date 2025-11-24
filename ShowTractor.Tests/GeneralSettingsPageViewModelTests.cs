using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using NUnit.Framework;
using ShowTractor.Database;
using ShowTractor.Tests.Mocks;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static ShowTractor.Tests.TestFixtures.ExampleSearchResults;

namespace ShowTractor.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class GeneralSettingsPageViewModelTests
    {
        private readonly string TestDirectoryName = Path.Combine(Path.GetTempPath(), $"{nameof(ShowTractor)}.{nameof(Tests)}");
        private void CreateDatabase(GeneralSettings settings)
        {
            using var context = new ShowTractorDbContext(settings);
            context.Database.EnsureCreated();
            var season = TvSeason.FromRecord(TestTvSeason1, Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty);
            season.Following = true;
            context.TvSeasons.Add(season);
            context.SaveChanges();
        }
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (Directory.Exists(TestDirectoryName))
            {
                Directory.Delete(TestDirectoryName, true);
            }
            Directory.CreateDirectory(TestDirectoryName);
        }
        [Test]
        public async Task LoadTest()
        {
            var mock = new MockOpenFileDialogService();
            mock.Filename = Path.Combine(TestDirectoryName, "Database1.db");
            var settings = new GeneralSettings
            {
                DatabaseFilename = mock.Filename,
                ArtworkDirectoryName = Path.Combine(TestDirectoryName, "Artwork"),
            };
            CreateDatabase(settings);
            settings.DatabaseFilename = Path.Combine(TestDirectoryName, "Database2.db");
            CreateDatabase(settings);
            var subject = new Pages.Settings.GeneralSettingsPageViewModel(settings, mock);
            Assert.That(settings.DatabaseFilename, Is.Not.EqualTo(mock.Filename));
            await subject.LoadCommand.ExecuteAsync(null);
            Assert.That(subject.DatabaseErrorMessage, Is.Null);
            Assert.That(settings.DatabaseFilename, Is.EqualTo(mock.Filename));
        }
        [Test]
        public async Task MoveTest()
        {
            var mock = new MockOpenFileDialogService();
            mock.Filename = Path.Combine(TestDirectoryName, "DatabaseMoved.db");
            var settings = new GeneralSettings
            {
                DatabaseFilename = Path.Combine(TestDirectoryName, "Database.db"),
                ArtworkDirectoryName = Path.Combine(TestDirectoryName, "Artwork"),
            };
            CreateDatabase(settings);
            var subject = new Pages.Settings.GeneralSettingsPageViewModel(settings, mock);
            Assert.That(settings.DatabaseFilename, Is.Not.EqualTo(mock.Filename));
            await subject.MoveCommand.ExecuteAsync(null);
            Assert.That(settings.DatabaseFilename, Is.EqualTo(mock.Filename));
            Assert.That(subject.DatabaseErrorMessage, Is.Null);
            using var context = new ShowTractorDbContext(settings);
            Assert.That(await context.TvSeasons.CountAsync(), Is.EqualTo(1));
        }
        [Test]
        public async Task ChangeTestAsync()
        {
            var mock = new MockOpenFileDialogService();
            mock.FolderName = Path.Combine(TestDirectoryName, "NewArtwork");
            var settings = new GeneralSettings
            {
                DatabaseFilename = Path.Combine(TestDirectoryName, "Database3.db"),
                ArtworkDirectoryName = Path.Combine(TestDirectoryName, "Artwork"),
            };
            CreateDatabase(settings);
            var subject = new Pages.Settings.GeneralSettingsPageViewModel(settings, mock);
            Assert.That(settings.ArtworkDirectoryName, Is.Not.EqualTo(mock.FolderName));
            await subject.ChangeCommand.ExecuteAsync(null);
            Assert.That(subject.DatabaseErrorMessage, Is.Null);
            Assert.That(settings.ArtworkDirectoryName, Is.EqualTo(mock.FolderName));
        }
    }
}
