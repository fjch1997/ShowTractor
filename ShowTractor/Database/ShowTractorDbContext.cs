using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ShowTractor.Database
{
    class ShowTractorDbContext : DbContext
    {
        private readonly GeneralSettings generalSettings;
        public ShowTractorDbContext(GeneralSettings generalSettings)
        {
            if (string.IsNullOrEmpty(generalSettings.DatabaseFilename))
            {
                var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(ShowTractor));
                generalSettings.DatabaseFilename = Path.Combine(directory, "data.sqlite");
                generalSettings.Save();
            }
            this.generalSettings = generalSettings;
        }
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
                Directory.CreateDirectory(Path.GetDirectoryName(generalSettings.DatabaseFilename) ?? throw new NullReferenceException());
                var builder = new SqliteConnectionStringBuilder
                {
                    DataSource = generalSettings.DatabaseFilename,
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
