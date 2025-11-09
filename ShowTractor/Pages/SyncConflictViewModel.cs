using System;

namespace ShowTractor.Pages
{
    public class SyncConflictViewModel
    {
        public string RemoteFilename { get; set; } = string.Empty;
        public DateTime RemoteLastModifiedTimeUtc { get; set; }
        public DateTime LocalLastModifiedTimeUtc { get; set; }
    }
}
