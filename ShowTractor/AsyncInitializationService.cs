using Microsoft.EntityFrameworkCore;
using ShowTractor.Database;
using ShowTractor.Pages.Details;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace ShowTractor
{
    public interface IAsyncInitializationService
    {
        Task Task { get; }
    }

    public class AsyncInitializationService : IAsyncInitializationService
    {
        private readonly IArtworkService artworkService;

        internal AsyncInitializationService(ShowTractorDbContext context, IArtworkService artworkService)
        {
            Task = Task.Run(async () =>
            {
                using (context)
                {
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations != null && pendingMigrations.Any(i => i == "20250916222327_RemoveArtwork"))
                    {
                        await MigrateArtworkFromInDatabaseToFileSystemAsync(context);
                    }
                    await context.Database.MigrateAsync();
                }
            });
            this.artworkService = artworkService;
        }
        async Task MigrateArtworkFromInDatabaseToFileSystemAsync(ShowTractorDbContext context)
        {
            await MigrateTvSeasons(context);
            await MigrateTvEpisodes(context);
        }

        private async Task MigrateTvSeasons(ShowTractorDbContext context)
        {
            var tableName = context.Model.FindEntityType(typeof(TvSeason))?.GetTableName() ?? throw new InvalidOperationException("Cannot read TvSeason table name.");
            var reader = await ExecuteReader(context, $"SELECT {nameof(TvSeason.Id)}, Artwork FROM {tableName}");
            while (reader.Read())
            {
                if (reader.IsDBNull(0) || reader.IsDBNull(1))
                    continue;
                var id = reader.GetGuid(0);
                using var stream = reader.GetStream(1);
                await artworkService.SaveArtwork(stream, id);
            }
        }

        private async Task MigrateTvEpisodes(ShowTractorDbContext context)
        {
            var tableName = context.Model.FindEntityType(typeof(TvEpisode))?.GetTableName() ?? throw new InvalidOperationException("Cannot read TvSeason table name.");
            var reader = await ExecuteReader(context, $"SELECT {nameof(TvEpisode.TvSeasonId)}, {nameof(TvEpisode.EpisodeNumber)}, Artwork FROM {tableName}");
            while (reader.Read())
            {
                if (reader.IsDBNull(0) || reader.IsDBNull(1) || reader.IsDBNull(2))
                    continue;
                var seasonId = reader.GetGuid(0);
                var episodeNumber = reader.GetInt32(1);
                using var stream = reader.GetStream(2);
                await artworkService.SaveArtwork(stream, seasonId, episodeNumber);
            }
        }

        private static async Task<DbDataReader> ExecuteReader(ShowTractorDbContext context, string commandText)
        {
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            return command.ExecuteReader();
        }

        public Task Task { get; }
    }
}
