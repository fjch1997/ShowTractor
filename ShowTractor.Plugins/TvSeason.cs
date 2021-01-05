using System;
using System.Collections.Generic;

namespace ShowTractor.Plugins.Interfaces
{
    public record TvSeason(
        string? UniqueId,
        string ShowName,
        int Season,
        IList<string> Genres,
        IList<string> Ratings,
        string ShowDescription,
        string SeasonDescription,
        byte[]? Artwork,
        Uri? ArtworkUri,
        bool ShowEnded,
        bool ShowFinale,
        IList<TvEpisode> Episodes,
        IReadOnlyDictionary<string, string> AdditionalAttributes);
}
