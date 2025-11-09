using NUnit.Framework;
using ShowTractor.Pages.Details;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShowTractor.Tests.Mocks
{
    class MockArtworkService : IArtworkService
    {
        private string directoryName = Path.Combine(Path.GetTempPath(), "ShowTractor", "MockArtworkService");
        private ArtworkService artworkService;
        public MockArtworkService()
        {
            if (Directory.Exists(directoryName))
                Directory.Delete(directoryName, true);
            Directory.CreateDirectory(directoryName);
            artworkService = new ArtworkService(
                new HttpClient(new TestHttpMessageHandler()),
                new GeneralSettings { ArtworkDirectoryName = directoryName });
        }
        public Task<Uri?> LoadAndSaveArtwork(Uri? artworkUri, Guid? seasonId)
        {
            return artworkService.LoadAndSaveArtwork(artworkUri, seasonId);
        }
        public Task<Uri?> LoadAndSaveArtwork(Uri? artworkUri, Guid? seasonId, int episodeNumber)
        {
            return artworkService.LoadAndSaveArtwork(artworkUri, seasonId, episodeNumber);
        }
        public Task SaveArtwork(Stream networkStream, Guid seasonId)
        {
            return artworkService.SaveArtwork(networkStream, seasonId);
        }
        public Task SaveArtwork(Stream networkStream, Guid seasonId, int episodeNumber)
        {
            return artworkService.SaveArtwork(networkStream, seasonId, episodeNumber);
        }
        public Task SaveArtwork(Uri uri, Guid seasonId)
        {
            return artworkService.SaveArtwork(uri, seasonId);
        }
        public Task SaveArtwork(Uri uri, Guid seasonId, int episodeNumber)
        {
            return artworkService.SaveArtwork(uri, seasonId, episodeNumber);
        }
        public void AssertNothingSaved()
        {
            Assert.That(Directory.EnumerateFileSystemEntries(directoryName), Is.Empty);
        }
    }
}
