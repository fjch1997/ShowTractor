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
        public byte[]? Artwork { get; set; }
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
            new(EpisodeNumber, Name, Description, Artwork, null, FirstAirDate, Runtime);
        public static TvEpisode FromRecord(Plugins.Interfaces.TvEpisode tvEpisode) =>
            new()
            {
                EpisodeNumber = tvEpisode.EpisodeNumber,
                Name = tvEpisode.Name,
                Description = tvEpisode.Description,
                Artwork = tvEpisode.Artwork,
                FirstAirDate = tvEpisode.FirstAirDate,
                Runtime = tvEpisode.Runtime,
            };
        public async ValueTask UpdateAsync(Plugins.Interfaces.TvEpisode data, System.Net.Http.HttpClient httpClient)
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
            if (Artwork == null)
            {
                if (data.Artwork != null)
                    Artwork = data.Artwork;
                else if (data.ArtworkUri != null)
                    Artwork = await httpClient.GetByteArrayAsync(data.ArtworkUri);
            }
        }
    }
}
