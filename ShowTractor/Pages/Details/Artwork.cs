using ShowTractor.Interfaces;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ShowTractor.Pages.Details
{
    public enum ArtworkType
    {
        SeasonDefault, Season, EpisodeDefault, Episode, Plugin
    }
    public struct ArtworkCacheKey
    {
        public ArtworkType Type { get; init; }
        public Guid SeasonId { get; init; }
        public int? EpisodeNumber { get; init; }
        public int HashCode { get; init; }
        public override int GetHashCode()
        {
            switch (Type)
            {
                case ArtworkType.SeasonDefault:
                    return 0;
                case ArtworkType.EpisodeDefault:
                    return 1;
                default:
                    var hash = new HashCode();
                    hash.Add(SeasonId);
                    hash.Add(EpisodeNumber);
                    hash.Add(HashCode);
                    var code = hash.ToHashCode();
                    if (code == 0 || code == 1)
                        return code + 2;
                    return code;
            }
        }
        public override bool Equals(object obj)
        {
            var other = (ArtworkCacheKey)obj;
            return other.Type == Type && other.SeasonId == SeasonId && other.EpisodeNumber == EpisodeNumber && other.HashCode == HashCode;
        }
        public static bool operator ==(ArtworkCacheKey left, ArtworkCacheKey right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(ArtworkCacheKey left, ArtworkCacheKey right)
        {
            return !(left == right);
        }
    }
    public class Artwork
    {
        private readonly IFactory<ValueTask<Stream>> streamFactory;

        internal Artwork(ArtworkCacheKey cacheKey, IFactory<ValueTask<Stream>> streamFactory)
        {
            CacheKey = cacheKey;
            this.streamFactory = streamFactory;
        }

        public ArtworkCacheKey CacheKey { get; }

        public async ValueTask<Stream> GetStreamAsync()
        {
            return await streamFactory.Get();
        }
    }
    public class TvSeasonDefaultArtwork : Artwork
    {
        public TvSeasonDefaultArtwork() : base(new ArtworkCacheKey { Type = ArtworkType.SeasonDefault }, new DelegateFactory<ValueTask<Stream>>(
                () => new ValueTask<Stream>(Assembly.GetExecutingAssembly().GetManifestResourceStream("ShowTractor.Assets.poster-placeholder.jpg"))))
        { }
    }
    public class TvEpisodeDefaultArtwork : Artwork
    {
        public TvEpisodeDefaultArtwork() : base(new ArtworkCacheKey { Type = ArtworkType.EpisodeDefault }, new DelegateFactory<ValueTask<Stream>>(
                () => new ValueTask<Stream>(Assembly.GetExecutingAssembly().GetManifestResourceStream("ShowTractor.Assets.episode-placeholder.jpg"))))
        { }
    }
}
