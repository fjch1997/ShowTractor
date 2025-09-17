using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ShowTractor.Database
{
    class TvEpisode
    {
        [Key] // See additional configuration in DbContext for composite keys.
        public Guid TvSeasonId { get; set; }
        [Key] // See additional configuration in DbContext for composite keys.
        public int EpisodeNumber { get; set; }
        public string Name
        {
            set => name = value;
            get => name ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Name));
        }
        private string? name;
        public string Description
        {
            set => description = value;
            get => description ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Description));
        }
        private string? description;
        public DateTime FirstAirDate { get; set; }
        public TimeSpan Runtime { get; set; }
        public TimeSpan WatchProgress { get; set; }
        public TvSeason TvSeason
        {
            set => tvSeason = value;
            get => tvSeason ?? throw new InvalidOperationException("Uninitialized property: " + nameof(TvSeason));
        }
        private TvSeason? tvSeason;

        internal Plugins.Interfaces.TvEpisode ToRecord() =>
            new(EpisodeNumber, Name, Description, null, FirstAirDate, Runtime);
        public static TvEpisode FromRecord(Plugins.Interfaces.TvEpisode tvEpisode) =>
            new()
            {
                EpisodeNumber = tvEpisode.EpisodeNumber,
                Name = tvEpisode.Name,
                Description = tvEpisode.Description,
                FirstAirDate = tvEpisode.FirstAirDate,
                Runtime = tvEpisode.Runtime,
            };
        public ValueTask UpdateAsync(Plugins.Interfaces.TvEpisode data)
        {
            if (!string.IsNullOrEmpty(data.Name) && data.Name != Name)
                Name = data.Name;
            if (!string.IsNullOrEmpty(data.Description) && data.Description != Description)
                Description = data.Description;
            if (EpisodeNumber != data.EpisodeNumber)
                throw new InvalidOperationException();
            if (data.FirstAirDate != default && data.FirstAirDate != FirstAirDate)
                FirstAirDate = data.FirstAirDate;
            if (data.Runtime != default && data.Runtime != Runtime)
                Runtime = data.Runtime;
            return default;
        }
    }
}
