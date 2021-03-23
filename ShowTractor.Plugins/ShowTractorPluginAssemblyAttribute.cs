using System;

namespace ShowTractor.Plugins.Interfaces
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class ShowTractorPluginAssemblyAttribute : Attribute
    {
        public Type? MetadataProvider { get; set; }
        public Type? MediaSourceProvider { get; set; }
        public Type? DownloadManager { get; set; }
        public Type? MediaPlayer { get; set; }
    }
}
