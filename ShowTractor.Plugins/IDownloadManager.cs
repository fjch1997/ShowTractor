using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace ShowTractor.Plugins.Interfaces
{
    public record DownloadItem(
        MediaSourceType Type,
        Guid TvSeasonId,
        int EpisodeNumber,
        MediaResolution Resolution,
        MediaCodec MediaCodec,
        string DisplayName,
        long TotalSizeBytes,
        Point FrameDimension,
        long DownloadedSizeBytes,
        bool Downloaded) : MediaSource(Type, TvSeasonId, EpisodeNumber, Resolution, MediaCodec, DisplayName, TotalSizeBytes);
    public interface IDownloadManager : IPlugin
    {
        public IEnumerable<MediaSourceType> SupportedTypes { get; set; }
        public ValueTask InitializeAsync();
        public ValueTask<DownloadItem?> GetDownloadAsync(TvSeason tvSeason, TvEpisode tvEpisode);
        public ValueTask<DownloadItem?> GetDownloadAsync(DownloadItem download);
        public IAsyncEnumerable<DownloadItem> GetAllDownloadsAsync();
        public ValueTask StartDownloadAsync(DownloadItem download);
        public ValueTask CancelDownloadAsync(DownloadItem download);
        public ValueTask PauseDownloadAsync(DownloadItem download);
        public ValueTask ResumeDownloadAsync(DownloadItem download);
        public ValueTask<MediaSource> GetDownloadedMediaAsync(DownloadItem download);
    }
}
