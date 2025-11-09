using ShowTractor.Pages;
using System.Threading.Tasks;

namespace ShowTractor
{
    public interface INotificationService
    {
        public enum SyncConflictResolution
        {
            Cancel,
            KeepRemote,
            KeepLocal,
        }
        ValueTask<SyncConflictResolution> ShowSyncConflict(SyncConflictViewModel syncConflictViewModel);
    }
}
