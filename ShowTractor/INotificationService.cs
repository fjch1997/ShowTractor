using System;
using System.Threading.Tasks;

namespace ShowTractor
{
    public interface INotificationService
    {
        public enum SyncConflictResolution
        {
            Cancel,
            OverwriteLocal,
            OverwriteRemote,
        }
        ValueTask<SyncConflictResolution> ShowSyncConflict(string remoteFilename, DateTime remoteLastModifiedTimeUtc, DateTime localLastModifiedTimeUtc);
    }
}
