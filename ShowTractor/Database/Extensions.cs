using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShowTractor.Database.Extensions
{
    static class TvSeasonExtensions
    {
        public static IQueryable<TvSeason> SelectNoArtwork(this IQueryable<TvSeason> queryable, bool loadEpisodes = false)
        {
            return queryable.Select(s => new TvSeason
            {
                Id = s.Id,
                Episodes = (loadEpisodes && s.Episodes != null) ? s.Episodes.SelectNoArtwork().ToList() : null,
                Following = s.Following,
                GenresCsv = s.GenresCsv,
                RatingsCsv = s.RatingsCsv,
                Season = s.Season,
                SeasonDescription = s.SeasonDescription,
                ShowEnded = s.ShowEnded,
                ShowFinale = s.ShowFinale,
                ShowName = s.ShowName,
                ShowDescription = s.ShowDescription,
            });
        }
    }
    static class TvEpisodeExtensions
    {
        public static IQueryable<TvEpisode> SelectNoArtwork(this IQueryable<TvEpisode> queryable)
        {
            return queryable.Select(e => new TvEpisode
            {
                Description = e.Description,
                EpisodeNumber = e.EpisodeNumber,
                FirstAirDate = e.FirstAirDate,
                Name = e.Name,
                Runtime = e.Runtime,
                TvSeasonId = e.TvSeasonId,
                WatchProgress = e.WatchProgress,
            });
        }
        public static IEnumerable<TvEpisode> SelectNoArtwork(this IEnumerable<TvEpisode> queryable)
        {
            return queryable.Select(e => new TvEpisode
            {
                Description = e.Description,
                EpisodeNumber = e.EpisodeNumber,
                FirstAirDate = e.FirstAirDate,
                Name = e.Name,
                Runtime = e.Runtime,
                TvSeasonId = e.TvSeasonId,
                WatchProgress = e.WatchProgress,
            });
        }
        public static async ValueTask SetWatchProgressAsync(this ShowTractorDbContext context, Guid seasonId, int episodeNumber, TimeSpan progress)
        {
            var dbEpisode = new TvEpisode { TvSeasonId = seasonId, EpisodeNumber = episodeNumber };
            context.TvEpisodes.Attach(dbEpisode);
            dbEpisode.WatchProgress = progress;
            context.Entry(dbEpisode).Property(nameof(dbEpisode.WatchProgress)).IsModified = true;
            await context.SaveChangesAsync();
        }
        public static bool Aired(this TvEpisode episode)
        {
            return episode.FirstAirDate <= DateTime.Today.AddDays(1);
        }
    }
}
