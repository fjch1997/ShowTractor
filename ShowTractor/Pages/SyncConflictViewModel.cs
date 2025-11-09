using System;

namespace ShowTractor.Pages
{
    public class SyncConflictViewModel
    {
        public string RemoteFilename { get; set; } = string.Empty;
        public DateTime RemoteLastModifiedTime { get; set; }
        public DateTime LocalLastModifiedTime { get; set; }
    }
}
