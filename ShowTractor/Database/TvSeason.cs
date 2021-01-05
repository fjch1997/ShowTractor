using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShowTractor.Database
{
    class TvSeason
    {
        private static readonly IReadOnlyDictionary<string, string> emptyDictionary = new Dictionary<string, string>();
        private static readonly char[] CsvForbiddenCharacters = new[] { '\r', '\n', ',', '\"' };
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string ShowName
        {
            set => showName = value;
            get => showName
                    ?? throw new InvalidOperationException("Uninitialized property: " + nameof(ShowName));
        }
        private string? showName;
        public int Season { get; set; }
        public string GenresCsv
        {
            set => genresCsv = value;
            get => genresCsv
                    ?? throw new InvalidOperationException("Uninitialized property: " + nameof(GenresCsv));
        }
        private string? genresCsv;
        public string RatingsCsv
        {
            set => ratingsCsv = value;
            get => ratingsCsv
                    ?? throw new InvalidOperationException("Uninitialized property: " + nameof(RatingsCsv));
        }
        private string? ratingsCsv;
        public string ShowDescription
        {
            set => showDescription = value;
            get => showDescription ?? throw new InvalidOperationException("Uninitialized property: " + nameof(ShowDescription));
        }
        private string? showDescription;
        public string SeasonDescription
        {
            set => seasonDescription = value;
            get => seasonDescription ?? throw new InvalidOperationException("Uninitialized property: " + nameof(SeasonDescription));
        }
        private string? seasonDescription;
        public byte[]? Artwork { get; set; }
        public bool Following { get; set; }
        public bool ShowEnded { get; set; }
        public bool ShowFinale { get; set; }
        public DateTime DateFollowed { get; set; }
        public virtual IList<TvEpisode>? Episodes { get; set; }
        public virtual IList<AdditionalAttribute>? AdditionalAttributes { get; set; }
        public Plugins.Interfaces.TvSeason ToRecord(string? providerAssemblyName, string? uniqueId) =>
            new(
                uniqueId,
                ShowName,
                Season,
                GenresCsv.Split(','),
                RatingsCsv.Split(','),
                ShowDescription,
                SeasonDescription,
                Artwork,
                null,
                ShowEnded,
                ShowFinale,
                Episodes.Select(e => e.ToRecord()).ToList(),
                string.IsNullOrEmpty(providerAssemblyName) ? emptyDictionary : AdditionalAttributes.Where(a => a.AssemblyName == providerAssemblyName).ToDictionary(a => a.Name, a => a.Value));
        public async ValueTask UpdateAsync(Plugins.Interfaces.TvSeason data, HttpClient httpClient)
        {
            ShowName = data.ShowName;
            Season = data.Season;
            GenresCsv = GetCsv(data.Genres);
            RatingsCsv = GetCsv(data.Ratings);
            ShowDescription = data.ShowDescription;
            SeasonDescription = data.SeasonDescription;
            if (data.Artwork != null)
                Artwork = data.Artwork;
            if (Artwork == null && data.ArtworkUri != null)
                Artwork = await httpClient.GetByteArrayAsync(data.ArtworkUri);
        }
        public static TvSeason FromRecord(Plugins.Interfaces.TvSeason data, string providerAssemblyName) =>
            new()
            {
                AdditionalAttributes = data.UniqueId != null ?
                    new List<AdditionalAttribute>()
                    {
                        new AdditionalAttribute
                        {
                            AssemblyName = providerAssemblyName,
                            Name = nameof(Plugins.Interfaces.TvSeason.UniqueId),
                            Value = data.UniqueId,
                        }
                    }
                    :
                    new List<AdditionalAttribute>(),
                Artwork = data.Artwork,
                GenresCsv = GetCsv(data.Genres),
                RatingsCsv = GetCsv(data.Ratings),
                Season = data.Season,
                SeasonDescription = data.SeasonDescription,
                ShowName = data.ShowName,
                ShowDescription = data.ShowDescription,
                ShowEnded = data.ShowEnded,
                ShowFinale = data.ShowFinale,
            };
        private static string GetCsv(IEnumerable<string> values)
        {
            var csv = string.Join(',', values);
            if (!CsvForbiddenCharacters.Any(c => csv.Contains(c)))
                return csv;
            return string.Empty;
        }
    }
}
