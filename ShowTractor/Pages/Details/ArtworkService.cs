using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace ShowTractor.Pages.Details
{
    public interface IArtworkService
    {
        Task SaveArtwork(Uri uri, Guid seasonId);
        Task SaveArtwork(Uri uri, Guid seasonId, int episodeNumber);
        Task SaveArtwork(Stream stream, Guid seasonId);
        Task SaveArtwork(Stream stream, Guid seasonId, int episodeNumber);
        Task<Uri?> LoadAndSaveArtwork(Uri? artworkUri, Guid? seasonId);
        Task<Uri?> LoadAndSaveArtwork(Uri? artworkUri, Guid? seasonId, int episodeNumber);
    }

    class ArtworkService : IArtworkService
    {
        private readonly string artworkDirectory;
        private readonly HttpClient httpClient;

        public ArtworkService(HttpClient httpClient) : this(httpClient, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(ShowTractor), "Artwork")) { }

        internal ArtworkService(HttpClient httpClient, string artworkDirectory)
        {
            this.httpClient = httpClient;
            this.artworkDirectory = artworkDirectory;
        }

        public Stream GetDefaultSeasonArtwork()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("ShowTractor.Assets.poster-placeholder.jpg")
            ?? throw new InvalidOperationException("Failed to load image from manifest resources.");
        }
        public Stream GetDefaultEpisodeArtwork()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("ShowTractor.Assets.episode-placeholder.jpg")
            ?? throw new InvalidOperationException("Failed to load image from manifest resources.");
        }
        private Uri? GetSavedArtwork(string filename)
        {
            if (Path.Exists(filename))
                return new Uri("file:///" + filename);

            return null;
        }
        private string GetSeasonFilename(Guid seasonId)
        {
            return Path.Combine(artworkDirectory, seasonId.ToString(), "poster.jpg");
        }
        private string GetEpisodeFilename(Guid seasonId, int episodeNumber)
        {
            return Path.Combine(artworkDirectory, seasonId.ToString(), episodeNumber.ToString() + ".jpg");
        }
        public Task SaveArtwork(Uri artworkUri, Guid seasonId)
        {
            return SaveArtwork(artworkUri, GetSeasonFilename(seasonId));
        }
        public Task SaveArtwork(Uri artworkUri, Guid seasonId, int episodeNumber)
        {
            return SaveArtwork(artworkUri, GetEpisodeFilename(seasonId, episodeNumber));
        }
        public async Task SaveArtwork(Stream artworkStream, Guid seasonId)
        {
            await SaveArtwork(artworkStream, GetSeasonFilename(seasonId));
        }
        public async Task SaveArtwork(Stream artworkStream, Guid seasonId, int episodeNumber)
        {
            await SaveArtwork(artworkStream, GetEpisodeFilename(seasonId, episodeNumber));
        }
        private async Task SaveArtwork(Uri artworkUri, string filename)
        {
            if (File.Exists(filename) || artworkUri.IsFile)
                return;
            using var artworkStream = await httpClient.GetStreamAsync(artworkUri);
            await SaveArtwork(artworkStream, filename);
        }
        private async Task SaveArtwork(Stream artworkStream, string filename)
        {
            if (File.Exists(filename))
                return;
            Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? throw new NullReferenceException("Path.GetDirectoryName(filename)"));
            using var stream = File.Open(filename, FileMode.CreateNew);
            await artworkStream.CopyToAsync(stream);
        }
        public async Task<Uri?> LoadAndSaveArtwork(Uri? artworkUri, Guid? seasonId)
        {
            if (seasonId.HasValue)
                return await LoadAndSaveArtwork(artworkUri, GetSeasonFilename(seasonId.Value));
            // This season has not been saved locally. Load from the internet and do not save.
            if (artworkUri != null)
                return artworkUri;
            return null;
        }
        public async Task<Uri?> LoadAndSaveArtwork(Uri? artworkUri, Guid? seasonId, int episodeNumber)
        {
            if (seasonId.HasValue)
                return await LoadAndSaveArtwork(artworkUri, GetEpisodeFilename(seasonId.Value, episodeNumber));
            // This season has not been saved locally. Load from the internet and do not save.
            if (artworkUri != null)
                return artworkUri;
            return null;
        }
        private async Task<Uri?> LoadAndSaveArtwork(Uri? artworkUri, string filename)
        {
            // This season had been saved locally. First attempt to load from disk.
            var artwork = GetSavedArtwork(filename);
            if (artwork == null)
            {
                // If it doesn't exist on disk, load from the internet.
                if (artworkUri != null)
                {
                    await SaveArtwork(artworkUri, filename);
                    return new Uri("file:///" + filename);
                }
            }

            return artwork;
        }
    }
}
