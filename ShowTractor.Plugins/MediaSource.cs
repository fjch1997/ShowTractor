using System;
using System.Net;

namespace ShowTractor.Plugins.Interfaces
{
    public enum MediaResolution
    {
        FourK,
        TenEightyP,
        SevenTwentyP,
        SD,
    }
    public enum MediaCodec
    {
        Unknown,
        H254,
        HEVC,
        Xvid,
    }
    public struct MediaSourceType
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
    }
    public record MediaSource(
        MediaSourceType Type,
        Guid TvSeasonId,
        int EpisodeNumber,
        MediaResolution Resolution,
        MediaCodec MediaCodec,
        string DisplayName,
        long TotalSizeBytes);
    public record GenericMediaSource<T>(
        MediaSourceType Type,
        T Value,
        Guid TvSeasonId,
        int EpisodeNumber,
        MediaResolution Resolution,
        MediaCodec MediaCodec,
        string DisplayName,
        long TotalSizeBytes) : MediaSource(Type, TvSeasonId, EpisodeNumber, Resolution, MediaCodec, DisplayName, TotalSizeBytes);
    public static class PredefinedMediaSources
    {
        /// <summary>
        /// <see cref="GenericMediaSource.Value"/> shall be of type <see cref="string"/>.
        /// </summary>
        public static MediaSourceType LocalFile => new MediaSourceType { Id = Guid.Parse("552CC474-0FBE-436F-A37B-FA5071BB85FC"), DisplayName = "Local File" };
        public static MediaSourceType Smb => new MediaSourceType { Id = Guid.Parse("556E034F-A392-4C07-BD9A-9F185451B3C0"), DisplayName = "SMB" };
        /// <summary>
        /// <see cref="GenericMediaSource.Value"/> shall be of type <see cref="Uri"/>.
        /// </summary>
        public static MediaSourceType BitTorrentMagnet => new MediaSourceType { Id = Guid.Parse("A7726CDF-0680-4B96-8413-66B94E9EF481"), DisplayName = "Magnet" };
        /// <summary>
        /// <see cref="GenericMediaSource.Value"/> shall be of type <see cref="byte[]"/>.
        /// </summary>
        public static MediaSourceType BitTorrent => new MediaSourceType { Id = Guid.Parse("2E0419FF-8E87-4FA2-B167-5FA77C99FB09"), DisplayName = "Torrent" };
        public static GenericMediaSource<string> CreateLocalFile(string filename, Guid tvSeasonId, int episodeNumber, MediaResolution resolution, MediaCodec mediaCodec, string displayName, long totalSizeBytes) => new(LocalFile, filename, tvSeasonId, episodeNumber, resolution, mediaCodec, displayName, totalSizeBytes);
        public static GenericMediaSource<(string nucPath, ICredentials? credential)> CreateSmb(string nucPath, ICredentials? credential, Guid tvSeasonId, int episodeNumber, MediaResolution resolution, MediaCodec mediaCodec, string displayName, long totalSizeBytes) => new(LocalFile, (nucPath, credential), tvSeasonId, episodeNumber, resolution, mediaCodec, displayName, totalSizeBytes);
        public static GenericMediaSource<byte[]> CreateBitTorrentMagnet(byte[] torrent, Guid tvSeasonId, int episodeNumber, MediaResolution resolution, MediaCodec mediaCodec, string displayName, long totalSizeBytes) => new(BitTorrentMagnet, torrent, tvSeasonId, episodeNumber, resolution, mediaCodec, displayName, totalSizeBytes);
        public static GenericMediaSource<Uri> CreateBitTorrent(Uri magnet, Guid tvSeasonId, int episodeNumber, MediaResolution resolution, MediaCodec mediaCodec, string displayName, long totalSizeBytes) => new(BitTorrent, magnet, tvSeasonId, episodeNumber, resolution, mediaCodec, displayName, totalSizeBytes);
    }
}
