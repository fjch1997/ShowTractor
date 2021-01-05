using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ShowTractor.Database
{
    class ShowTractorDbContext : DbContext
    {
        internal DbSet<TvSeason> TvSeasons => Set<TvSeason>();
        internal DbSet<TvEpisode> TvEpisodes => Set<TvEpisode>();
        internal DbSet<AdditionalAttribute> AdditionalAttributes => Set<AdditionalAttribute>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TvEpisode>().HasKey(nameof(TvEpisode.TvSeasonId), nameof(TvEpisode.EpisodeNumber));
            modelBuilder.Entity<AdditionalAttribute>().HasKey(nameof(AdditionalAttribute.TvSeasonId), nameof(AdditionalAttribute.AssemblyName), nameof(AdditionalAttribute.Name));
            base.OnModelCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            optionsBuilder.LogTo(Console.WriteLine);
            optionsBuilder.EnableSensitiveDataLogging(true);
#endif
            if (!optionsBuilder.IsConfigured)
            {
                var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(ShowTractor));
                Directory.CreateDirectory(directory);
                var builder = new SqliteConnectionStringBuilder
                {
                    DataSource = Path.Combine(directory, "data.sqlite"),
                    Cache = SqliteCacheMode.Shared
                };
                optionsBuilder.UseSqlite(builder.ToString());
            }
            base.OnConfiguring(optionsBuilder);
        }
        public override void Dispose()
        {
            base.Dispose();
        }
        public override ValueTask DisposeAsync()
        {
            return base.DisposeAsync();
        }
    }
}
