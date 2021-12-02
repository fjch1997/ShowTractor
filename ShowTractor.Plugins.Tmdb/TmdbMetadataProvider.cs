using ShowTractor.Plugins.Interfaces;
using ShowTractor.Plugins.Tmdb;
using ShowTractor.Plugins.Tmdb.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

[assembly: ShowTractorPluginAssembly(MetadataProvider = typeof(TmdbMetadataProvider))]

namespace ShowTractor.Plugins.Tmdb
{
    public class TmdbMetadataProvider : IMetadataProvider
    {
        private readonly HttpClient httpClient;
        private readonly Uri BaseUri = new("https://api.themoviedb.org/3/");
        private TmdbConfigurations? configs;
        private static readonly IReadOnlyDictionary<string, string> emptyDictionary = new Dictionary<string, string>();
        public TmdbMetadataProvider(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public string Name => Resources.Tmdb;
        public PluginSettingsDescriptions? PluginSettingsDescriptions => new PredefinedPluginSettingsDescriptions(PluginSettingsDescriptionsType.Flyout)
        {
            Groups = new PluginSettingsDescriptionsGroup[]
            {
                new PluginSettingsDescriptionsGroup
                {
                    Descriptions = new PluginSettingsDescription[]
                    {
                        new StringPluginSettingsDescription(() => Settings.Default.ApiKey, s => { Settings.Default.ApiKey = s; Settings.Default.Save(); })
                        {
                            Name = Resources.ApiKey,
                        },
                    }
                }
            },
        };
        public Stream GetIconStream() => Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(ShowTractor)}.{nameof(Plugins)}.{nameof(Tmdb)}.logo.png");
        private string GetApiKey()
        {
            var value = Settings.Default.ApiKey;
            if (string.IsNullOrEmpty(value))
            {
                throw new ApiKeyNotSetException();
            }
            return value;
        }
        private async Task<TmdbConfigurations> GetConfigsAsync() => configs ??= await TmdbConfigurations.Load(httpClient, BaseUri, GetApiKey());
        public async IAsyncEnumerable<TvSeason> SearchAsync(string keyword, [EnumeratorCancellation] CancellationToken token)
        {
            int[] ids = await SearchForTvIdsAsync(keyword, token);
            foreach (var id in ids)
            {
                using var tvStream = await httpClient.GetStreamAsync(new Uri(BaseUri, "tv/" + id + "?api_key=" + GetApiKey()));
                using var tv = await JsonDocument.ParseAsync(tvStream, cancellationToken: token);
                int[] seasons = tv.RootElement.GetProperty("seasons").EnumerateArray().Select(s => s.GetProperty("season_number").GetInt32()).ToArray();
                foreach (var seasonNumber in seasons)
                {
                    var showDescription = tv.RootElement.TryGetProperty("overview", out var showOverview) ? showOverview.ToString() ?? string.Empty : string.Empty;
                    var showEnded = tv.RootElement.TryGetProperty("status", out var status) && status.GetString() == "Ended";
                    yield return await LoadTvSeasonAsync(
                        id,
                        seasonNumber,
                        tv.RootElement.GetProperty("name").GetString() ?? string.Empty,
                        tv.RootElement.TryGetProperty("genres", out var geners) ? geners.EnumerateArray().Select(g => g.GetProperty("name").GetString() ?? string.Empty).ToList() : new List<string>(),
                        new List<string>(),
                        showDescription,
                        showEnded,
                        showEnded && seasonNumber == seasons.Last()
                    );
                }
            }
        }
        private async Task<int[]> SearchForTvIdsAsync(string keyword, CancellationToken token)
        {
            using var stream = await httpClient.GetStreamAsync(new Uri(BaseUri, "search/tv?query=" + HttpUtility.UrlEncode(keyword) + "&api_key=" + GetApiKey()));
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: token);
            return document.RootElement.GetProperty("results").EnumerateArray().Select(tv => tv.GetProperty("id").GetInt32()).ToArray();
        }
        public async Task<GetUpdatesResult> GetUpdatesAsync(TvSeason season, IReadOnlyDictionary<AssemblyName, IReadOnlyDictionary<string, string>> additionalAttributes, CancellationToken token)
        {
            int id;
            if (season.UniqueId != null)
            {
                id = int.Parse(season.UniqueId);
            }
            else
            {
                var ids = await SearchForTvIdsAsync(season.ShowName, token);
                id = ids.Length > 0 ? ids.First() : throw new ShowNotFoundException(season.ShowName);
            }
            using var tvStream = await httpClient.GetStreamAsync(new Uri(BaseUri, "tv/" + id + "?api_key=" + GetApiKey()));
            using var tv = await JsonDocument.ParseAsync(tvStream, cancellationToken: token);
            var seasons = tv.RootElement.GetProperty("seasons").EnumerateArray().Select(s => s.GetProperty("season_number").GetInt32()).ToArray();
            var showEnded = tv.RootElement.TryGetProperty("status", out var status) && status.GetString() == "Ended";
            async IAsyncEnumerable<TvSeason> GetLatestSeasons(int afterSeasonNumber)
            {
                for (int i = 0; i < seasons.Length; i++)
                {
                    if (seasons[i] > afterSeasonNumber)
                    {
                        yield return await LoadTvSeasonAsync(id, seasons[i], season.ShowName, season.Genres, season.Ratings, season.ShowDescription, showEnded, showEnded && seasons[i] == seasons.Last());
                    }
                }
                yield break;
            }
            return new (await LoadTvSeasonAsync(id, season.Season, season.ShowName, season.Genres, season.Ratings, season.ShowDescription,
                        showEnded, showEnded && season.Season == seasons.Last()), GetLatestSeasons);
        }
        private async Task<TvSeason> LoadTvSeasonAsync(int id, int seasonNumber, string showName, IList<string> genre, IList<string> ratings, string showDescriptions, bool showEnded, bool showFinale)
        {
            using var seasonStream = await httpClient.GetStreamAsync(new Uri(BaseUri, $"tv/{id}/season/{seasonNumber}?api_key={GetApiKey()}"));
            using var season = await JsonDocument.ParseAsync(seasonStream);
            var configs = await GetConfigsAsync();
            return new TvSeason(
                id.ToString(),
                showName,
                seasonNumber,
                genre,
                ratings,
                showDescriptions,
                season.RootElement.TryGetProperty("overview", out var overview) ? overview.GetString() ?? string.Empty : string.Empty,
                null,
                season.RootElement.TryGetProperty("poster_path", out var posterPath) ?
                    posterPath.GetString() is string posterPathString ? configs.GetPosterUri(posterPathString) : null
                    :
                    null,
                showEnded,
                showFinale,
                season.RootElement.TryGetProperty("episodes", out var episodes) ?
                    episodes.EnumerateArray().Select(e => new TvEpisode(
                        e.TryGetProperty("episode_number", out var episodeNumber) ? episodeNumber.GetInt32() : 0,
                        e.TryGetProperty("name", out var episodeName) ? episodeName.GetString() ?? string.Empty : string.Empty,
                        e.TryGetProperty("overview", out var episodeOverview) ? episodeOverview.GetString() ?? string.Empty : string.Empty,
                        null,
                        e.TryGetProperty("still_path", out var stillPath) ?
                            stillPath.GetString() is string stillPathString ? configs.GetPosterUri(stillPathString) : null
                            :
                            null,
                        e.TryGetProperty("air_date", out var airDate) ?
                            DateTime.TryParse(airDate.ToString(), out var dateTime) ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc) : default : default,
                        default
                    )).ToList()
                    :
                    new List<TvEpisode>(0),
                emptyDictionary
            );
        }
    }
}
