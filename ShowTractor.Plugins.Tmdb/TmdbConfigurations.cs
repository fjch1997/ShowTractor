using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShowTractor.Plugins.Tmdb
{
    public record TmdbConfigurations(Uri ImageBaseUri, string BackdropSizeCode, string LogoSizeCode, string PosterSizeCode, string StillSizeCode)
    {
        public static async Task<TmdbConfigurations> Load(HttpClient httpClient, Uri baseUri, string apiKey)
        {
            using var stream = await httpClient.GetStreamAsync(new Uri(baseUri, "configuration?api_key=" + apiKey));
            using var document = await JsonDocument.ParseAsync(stream);
            var images = document.RootElement.GetProperty("images");
            return new TmdbConfigurations(
                new Uri(images.GetProperty("secure_base_url").ToString()),
                images.GetProperty("backdrop_sizes").EnumerateArray().Last().GetString() ?? "original",
                images.GetProperty("logo_sizes").EnumerateArray().Last().GetString() ?? "original",
                images.GetProperty("poster_sizes").EnumerateArray().Last().GetString() ?? "original",
                images.GetProperty("still_sizes").EnumerateArray().Last().GetString() ?? "original"
            );
        }

        public Uri GetBackdropUri(string path) => new(ImageBaseUri, BackdropSizeCode + path);
        public Uri GetPosterUri(string path) => new(ImageBaseUri, PosterSizeCode + path);
        public Uri GetLogoUri(string path) => new(ImageBaseUri, LogoSizeCode + path);
        public Uri GetStillUri(string path) => new(ImageBaseUri, StillSizeCode + path);
    }
}
