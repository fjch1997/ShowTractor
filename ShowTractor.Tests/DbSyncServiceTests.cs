using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ShowTractor.Database;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static ShowTractor.Tests.TestFixtures.ExampleSearchResults;

namespace ShowTractor.Tests
{
    [TestFixture]
    class DbSyncServiceTests
    {
        class NotificationService : INotificationService
        {
            ValueTask<INotificationService.SyncConflictResolution> INotificationService.ShowSyncConflict(string remoteFilename, DateTime remoteLastModifiedTimeUtc, DateTime localLastModifiedTimeUtc)
            {
                return ValueTask.FromResult(INotificationService.SyncConflictResolution.Cancel);
            }
        }
        private ShowTractorDbContext? context;
        private ShowTractorDbContext? remoteContext;
        private GeneralSettings settings = new GeneralSettings();
        private readonly string testDirectoryName = Path.Combine(Path.GetTempPath(), $"{nameof(ShowTractor)}.{nameof(Tests)}");
        private readonly string remoteDirectoryName = Path.Combine(Path.GetTempPath(), $"{nameof(ShowTractor)}.{nameof(Tests)}", "Remote");
        private ShowTractorDbContext GetLocalContext()
        {
            if (context == null)
            {
                context = new ShowTractorDbContext(testDirectoryName);
                context.Database.EnsureCreated();
                AddTestData(context, TestTvSeason1);
                context.SaveChanges();
            }
            return context;
        }
        private void CloseLocalContext()
        {
            if (context != null)
            {
                var connection = (SqliteConnection)context.Database.GetDbConnection();
                SqliteConnection.ClearPool(connection);
                context.Dispose();
                context = null;
            }
        }
        private ShowTractorDbContext GetRemoteContext()
        {
            if (remoteContext == null)
            {
                remoteContext = new ShowTractorDbContext(remoteDirectoryName);
                remoteContext.Database.EnsureCreated();
                AddTestData(remoteContext, TestTvSeason1);
                remoteContext.SaveChanges();
            }
            return remoteContext;
        }
        private void CloseRemoteContext()
        {
            if (remoteContext != null)
            {
                var connection = (SqliteConnection)remoteContext.Database.GetDbConnection();
                SqliteConnection.ClearPool(connection);
                remoteContext.Dispose();
                remoteContext = null;
            }
        }
        [SetUp]
        public void Setup()
        {
            settings = new GeneralSettings();
            CloseLocalContext();
            CloseRemoteContext();
            if (Directory.Exists(testDirectoryName))
                Directory.Delete(testDirectoryName, true);
        }
        private void AddTestData(ShowTractorDbContext context, Plugins.Interfaces.TvSeason tvSeason)
        {
            var season = TvSeason.FromRecord(tvSeason, Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty);
            season.Following = true;
            if (context.TvSeasons.Any(s => s.Season == season.Season && s.ShowName == season.ShowName))
                return;
            context.TvSeasons.Add(season);
            context.SaveChanges();
        }
        [Test]
        public async ValueTask LoadTestAsync()
        {
            var localContext = GetLocalContext();
            var subject = new DbSyncService(settings, localContext, new NotificationService());
            localContext.TvSeasons.ExecuteDelete();
            await localContext.SaveChangesAsync();
            Assert.That(localContext.TvSeasons.Count(), Is.EqualTo(0));
            GetRemoteContext();
            await subject.LoadAsync(
                Path.Combine(
                    Path.GetTempPath(),
                    $"{nameof(ShowTractor)}.{nameof(Tests)}",
                    "Remote",
                    "data.sqlite"));
            Assert.That(GetLocalContext().TvSeasons.Count(), Is.EqualTo(1));
        }
        [Test]
        public async ValueTask SaveTestAsync()
        {
            var remoteFilename = Path.Combine(remoteDirectoryName, "data2.sqlite");
            if (File.Exists(remoteFilename))
                File.Delete(remoteFilename);
            var subject = new DbSyncService(settings, GetLocalContext(), new NotificationService());
            await subject.SaveAsync(settings.RemoteDatabaseFilename);
            Assert.That(File.Exists(settings.RemoteDatabaseFilename), Is.True);
        }
        private async ValueTask InitInSyncDatabases(DbSyncService subject)
        {
            GetLocalContext().PendingUpload = false;
            var remoteFilename = GetRemoteContext().DataSource;
            CloseRemoteContext();
            settings.RemoteDatabaseFilename = remoteFilename;
            settings.RemoteDatabaseLastSyncTimeUtc = File.GetLastWriteTimeUtc(remoteFilename);

            // No update if nothing has changed.
            Assert.That(subject.LastDownloadTimeUtc, Is.EqualTo(default(DateTime)));
            Assert.That(subject.LastUploadTimeUtc, Is.EqualTo(default(DateTime)));
            await subject.DoWorkAsync();
            Assert.That(subject.LastDownloadTimeUtc, Is.EqualTo(default(DateTime)));
            Assert.That(subject.LastUploadTimeUtc, Is.EqualTo(default(DateTime)));
        }
        [Test]
        public async ValueTask UpdateTestAsync()
        {
            var localContext = GetLocalContext();
            var subject = new DbSyncService(settings, localContext, new NotificationService());
            await InitInSyncDatabases(subject);

            // Act: Update the season's description and Following status.
            var season = localContext.TvSeasons.First();
            var originalDescription = season.SeasonDescription;
            var newDescription = "Updated season description";
            var newFollowing = !season.Following;

            season.SeasonDescription = newDescription;
            season.Following = newFollowing;
            await localContext.SaveChangesAsync();

            // Act: Use DbSyncService to save and reload, simulating sync.
            await subject.DoWorkAsync();

            // Assert: Data is updated.
            var remoteSeason = GetRemoteContext().TvSeasons.First();
            Assert.That(remoteSeason.SeasonDescription, Is.EqualTo(newDescription));
            Assert.That(remoteSeason.Following, Is.EqualTo(newFollowing));
            Assert.That(subject.LastUploadTimeUtc, Is.GreaterThan(DateTime.MinValue));
            Assert.That(subject.LastDownloadTimeUtc, Is.EqualTo(default(DateTime)));

            // Assert: no sync occurs if no changes.
            var lastModifiedTime = File.GetLastWriteTimeUtc(settings.RemoteDatabaseFilename);
            await subject.DoWorkAsync();
            var newModifiedTime = File.GetLastWriteTimeUtc(settings.RemoteDatabaseFilename);
            Assert.That(newModifiedTime, Is.EqualTo(lastModifiedTime));
        }
        [Test]
        public async ValueTask DownloadTestAsync()
        {
            var localContext = GetLocalContext();
            var subject = new DbSyncService(settings, localContext, new NotificationService());
            await InitInSyncDatabases(subject);

            // Download after remote change.
            AddTestData(GetRemoteContext(), TestTvSeason2);
            CloseRemoteContext();
            var lastDownloadTime = subject.LastDownloadTimeUtc;
            var lastUploadTime = subject.LastUploadTimeUtc;
            await subject.DoWorkAsync();
            Assert.That(localContext.TvSeasons.Count(), Is.EqualTo(2));
            Assert.That(subject.LastDownloadTimeUtc, Is.GreaterThan(lastDownloadTime));
            Assert.That(subject.LastUploadTimeUtc, Is.EqualTo(lastUploadTime));
        }
        [Test]
        public async ValueTask ConflictTestAsync()
        {
            var localContext = GetLocalContext();
            var subject = new DbSyncService(settings, localContext, new NotificationService());
            await InitInSyncDatabases(subject);

            // Simulate a local update.
            var localSeason = localContext.TvSeasons.First();
            localSeason.SeasonDescription = "Local update";
            localSeason.Following = false;
            await localContext.SaveChangesAsync();

            // Simulate a conflicting remote update.
            AddTestData(GetRemoteContext(), TestTvSeason2);
            CloseRemoteContext();
            // Act & Assert: Try to sync.
            await subject.DoWorkAsync();
            Assert.That(settings.RemoteDatabaseFilename, Is.Null);
            Assert.That(settings.RemoteDatabaseLastSyncTimeUtc, Is.Default);
        }
    }
}
