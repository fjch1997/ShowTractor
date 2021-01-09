using System;
using System.Threading.Tasks;

namespace ShowTractor.Plugins.Interfaces
{
    /// <summary>
    /// A service provided to <see cref="IPlugin"/> instances to interact with the system.
    /// </summary>
    public interface IPluginService
    {
        public Task SetWatchProgress(Guid teasonId, int episodeNumber, TimeSpan progress);
        public Task SetWatchProgress(Guid teasonId, int episodeNumber, TimeSpan progress, TimeSpan runtime);
    }
}
