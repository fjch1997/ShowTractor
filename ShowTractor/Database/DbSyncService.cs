using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShowTractor.Background;
using ShowTractor.Pages;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ShowTractor.Database
{
    public class DbSyncService : IBackgroundWork, INotifyPropertyChanged
    {
        [Flags]
        private enum SyncStatus
        {
            None = 0,
            DownloadRequired = 1,
            UploadRequired = 2,
            Conflict = DownloadRequired | UploadRequired,
        }
        private readonly GeneralSettings settings;
        private readonly ShowTractorDbContext context;
        private readonly INotificationService notificationService;
        public bool Running { get => running; set { running = value; OnPropertyChanged(); } }
        private bool running;
        internal DbSyncService(GeneralSettings settings, ShowTractorDbContext context, INotificationService notificationService)
        {
            this.settings = settings;
            this.context = context;
            this.notificationService = notificationService;
        }
        public TimeSpan Interval => TimeSpan.FromMinutes(1);
        public DateTime LastDownloadTimeUtc { get => lastDownloadTimeUtc; set { lastDownloadTimeUtc = value; OnPropertyChanged(); } }
        private DateTime lastDownloadTimeUtc;
        public DateTime LastUploadTimeUtc { get => lastUploadTimeUtc; set { lastUploadTimeUtc = value; OnPropertyChanged(); } }
        private DateTime lastUploadTimeUtc;
        public ValueTask<bool> CanDoWorkAsync()
        {
            return ValueTask.FromResult(!string.IsNullOrEmpty(settings.RemoteDatabaseFilename) && !Running);
        }
        public async ValueTask DoWorkAsync(CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(settings.RemoteDatabaseFilename))
                return;
            Running = true;
            try
            {
                var syncStatusResult = await GetSyncStatusAsync();
                switch (syncStatusResult.status)
                {
                    case SyncStatus.Conflict:
                        await HandleConflict(syncStatusResult);
                        break;
                    case SyncStatus.DownloadRequired:
                        await LoadAsync(settings.RemoteDatabaseFilename);
                        break;
                    case SyncStatus.UploadRequired:
                        await SaveAsync(settings.RemoteDatabaseFilename);
                        break;
                }
            }
            finally
            {
                Running = false;
            }
        }
        private async ValueTask HandleConflict((SyncStatus status, DateTime localModifiedTimeUtc, DateTime remoteModifiedTimeUtc) syncStatusResult)
        {
            var vm = new SyncConflictViewModel
            {
                RemoteFilename = settings.RemoteDatabaseFilename,
                RemoteLastModifiedTime = syncStatusResult.remoteModifiedTimeUtc.ToLocalTime(),
                LocalLastModifiedTime = syncStatusResult.localModifiedTimeUtc.ToLocalTime(),
            };
            var resolution = await notificationService.ShowSyncConflict(vm);
            switch (resolution)
            {
                case INotificationService.SyncConflictResolution.Cancel:
                    Disable();
                    break;
                case INotificationService.SyncConflictResolution.KeepRemote:
                    await LoadAsync(settings.RemoteDatabaseFilename);
                    break;
                case INotificationService.SyncConflictResolution.KeepLocal:
                    await SaveAsync(settings.RemoteDatabaseFilename);
                    break;
            }
        }
        public void Disable()
        {
            settings.RemoteDatabaseFilename = null;
            settings.RemoteDatabaseLastSyncTimeUtc = default;
            settings.Save();
        }
        public ValueTask LoadAsync(string filename)
        {
            var connectionString = new SqliteConnectionStringBuilder();
            connectionString.DataSource = filename;
            connectionString.Mode = SqliteOpenMode.ReadOnly;
            var localDbConnection = (SqliteConnection)context.Database.GetDbConnection();
            using (var remoteConnection = new SqliteConnection(connectionString.ConnectionString))
            {
                remoteConnection.Open();
                remoteConnection.BackupDatabase(localDbConnection);
                SqliteConnection.ClearPool(remoteConnection);
            }
            settings.RemoteDatabaseFilename = filename;
            settings.RemoteDatabaseLastSyncTimeUtc = File.GetLastWriteTimeUtc(filename);
            settings.Save();
            LastDownloadTimeUtc = DateTime.UtcNow;
            return ValueTask.CompletedTask;
        }
        public ValueTask SaveAsync(string filename)
        {
            var connectionString = new SqliteConnectionStringBuilder();
            connectionString.DataSource = filename;
            Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? throw new NullReferenceException());
            using (var remoteConnection = new SqliteConnection(connectionString.ConnectionString))
            {
                var connection = (SqliteConnection)context.Database.GetDbConnection();
                connection.Open();
                connection.BackupDatabase(remoteConnection);
                SqliteConnection.ClearPool(remoteConnection);
            }
            context.PendingUpload = false;
            settings.RemoteDatabaseFilename = filename;
            settings.RemoteDatabaseLastSyncTimeUtc = File.GetLastWriteTimeUtc(filename);
            settings.Save();
            LastUploadTimeUtc = DateTime.UtcNow;
            return ValueTask.CompletedTask;
        }
        private async ValueTask<(SyncStatus status, DateTime localModifiedTimeUtc, DateTime remoteModifiedTimeUtc)> GetSyncStatusAsync()
        {
            var status = SyncStatus.None;
            var localModifiedTimeUtc = await Task.Run(() => GetDatabaseModifiedTimeUtc(context.DataSource));
            if (!File.Exists(settings.RemoteDatabaseFilename))
                return (SyncStatus.UploadRequired, localModifiedTimeUtc, default);
            var remoteModifiedTimeUtc = await Task.Run(() => GetDatabaseModifiedTimeUtc(settings.RemoteDatabaseFilename));
            if (settings.RemoteDatabaseLastSyncTimeUtc != remoteModifiedTimeUtc)
            {
                status |= SyncStatus.DownloadRequired;
            }
            if (context.PendingUpload)
            {
                status |= SyncStatus.UploadRequired;
            }
            return (status, localModifiedTimeUtc, remoteModifiedTimeUtc);
        }
        private DateTime GetDatabaseModifiedTimeUtc(string filename)
        {
            var dbFileModifiedTime = File.GetLastWriteTimeUtc(filename);
            var shmFileModifiedTime = File.GetLastWriteTimeUtc(filename + "-shm");
            var walFileModifiedTime = File.GetLastWriteTimeUtc(filename + "-wal");
            return new[] { dbFileModifiedTime, shmFileModifiedTime, walFileModifiedTime }.Max();
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
