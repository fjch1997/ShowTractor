using System;
using System.Collections.Generic;

namespace ShowTractor.Plugins.Interfaces
{
    public record TvSeason(
        string? UniqueId,
        string ShowName,
        int Season,
        IReadOnlyList<string> Genres,
        IReadOnlyList<string> Ratings,
        string ShowDescription,
        string SeasonDescription,
        byte[]? Artwork,
        Uri? ArtworkUri,
        bool ShowEnded,
        bool ShowFinale,
        IReadOnlyList<TvEpisode> Episodes,
        IReadOnlyDictionary<string, string> AdditionalAttributes);
}
