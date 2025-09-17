using System;

namespace ShowTractor.Plugins.Interfaces
{
    public record TvEpisode(
        int EpisodeNumber,
        string Name,
        string Description,
        Uri? ArtworkUri,
        DateTime FirstAirDate,
        TimeSpan Runtime);
}
