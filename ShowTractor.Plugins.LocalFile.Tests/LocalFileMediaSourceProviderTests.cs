using NUnit.Framework;
using ShowTractor.Plugins.Interfaces;
using ShowTractor.Plugins.LocalFile.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ShowTractor.Plugins.LocalFile.Tests
{
    [TestFixture()]
    public class LocalFileMediaSourceProviderTests
    {
        private readonly LocalFileMediaSourceProvider subject = new LocalFileMediaSourceProvider();
        private readonly string tmp = Path.Combine(Path.GetTempPath(), "ShowTractor.Plugins.LocalFile.Tests.LocalFileMediaSourceProviderTests");

        [OneTimeSetUp]
        public void SetupSampleFolderStructure()
        {
            Directory.CreateDirectory(tmp);
            Settings.Default.SearchLocation = tmp;
            File.Create(Path.Combine(tmp, "Sex.And.Violence.S01E01.720p.x264.AAC.mp4")).Dispose();
            File.Create(Path.Combine(tmp, "Sex.And.Violence.S01E02.720p.x264.AAC.mp4")).Dispose();
            File.Create(Path.Combine(tmp, "Sex.And.Violence.S01E03.720p.x264.AAC.mp4")).Dispose();
            File.Create(Path.Combine(tmp, "Sex.And.Violence.S01E04.720p.x264.AAC.mp4")).Dispose();
            File.Create(Path.Combine(tmp, "Sex.And.Violence.S01E05.720p.x264.AAC.mp4")).Dispose();
            Directory.CreateDirectory(Path.Combine(tmp, "The.Walking.Dead.S10E17.720p.WEB.x265"));
            File.Create(Path.Combine(tmp, "The.Walking.Dead.S10E17.720p.WEB.x265", "The.Walking.Dead.S10E17.720p.WEB.x265.mkv")).Dispose();
            Directory.CreateDirectory(Path.Combine(tmp, "The.Walking.Dead.S10E18.720p.WEB.x265"));
            File.Create(Path.Combine(tmp, "The.Walking.Dead.S10E18.720p.WEB.x265", "The.Walking.Dead.S10E18.720p.WEB.x265.mkv")).Dispose();
            Directory.CreateDirectory(Path.Combine(tmp, "The.Walking.Dead.S10E19.720p.WEB.x265"));
            File.Create(Path.Combine(tmp, "The.Walking.Dead.S10E19.720p.WEB.x265", "The.Walking.Dead.S10E19.720p.WEB.x265.mkv")).Dispose();
            Directory.CreateDirectory(Path.Combine(tmp, "The.Walking.Dead.S10E20.720p.WEB.x265"));
            File.Create(Path.Combine(tmp, "The.Walking.Dead.S10E20.720p.WEB.x265", "The.Walking.Dead.S10E20.720p.WEB.x265.mkv")).Dispose();
            File.Create(Path.Combine(tmp, "The.Walking.Dead.S10E17.1080p.WEB.x265.mkv")).Dispose();
            File.Create(Path.Combine(tmp, "The.Walking.Dead.S10E18.1080p.WEB.x265.mkv")).Dispose();
            File.Create(Path.Combine(tmp, "The.Walking.Dead.S10E19.1080p.WEB.x265.mkv")).Dispose();
            File.Create(Path.Combine(tmp, "The.Walking.Dead.S10E20.1080p.WEB.x265.mkv")).Dispose();
        }

        [Test]
        public async Task GetAsync_720p_Test()
        {
            var result = subject.GetAsync(
                new TvSeason(null, "Sex and Violence", 1, new List<string>(), new List<string>(), "Sample episode description.", "Sample season description.", null, null, false, false, null, null),
                new TvEpisode(1, "Episode 1", "Episode 1 sample description.", null, null, DateTime.Now, TimeSpan.FromMinutes(60)));
            var count = 0;
            await foreach (var item in result)
            {
                Assert.AreEqual(PredefinedMediaSources.LocalFile.Id, item.Type.Id);
                Assert.AreEqual(MediaCodec.H254, item.MediaCodec);
                Assert.AreEqual(MediaResolution.SevenTwentyP, item.Resolution);
                count++;
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task GetAsync_1080p_Test()
        {
            var result = subject.GetAsync(
                new TvSeason(null, "The Walking Dead", 10, new List<string>(), new List<string>(), "Sample episode description.", "Sample season description.", null, null, false, false, null, null),
                new TvEpisode(17, "Episode 17", "Episode 17 sample description.", null, null, DateTime.Now, TimeSpan.FromMinutes(60)));
            var count = 0;
            await foreach (var item in result)
            {
                if (item.Resolution == MediaResolution.SevenTwentyP)
                    continue;
                Assert.AreEqual(PredefinedMediaSources.LocalFile.Id, item.Type.Id);
                Assert.AreEqual(MediaCodec.HEVC, item.MediaCodec);
                Assert.AreEqual(MediaResolution.TenEightyP, item.Resolution);
                count++;
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task GetAsync_InDirectory_Test()
        {
            var result = subject.GetAsync(
                new TvSeason(null, "The Walking Dead", 10, new List<string>(), new List<string>(), "Sample episode description.", "Sample season description.", null, null, false, false, null, null),
                new TvEpisode(17, "Episode 17", "Episode 17 sample description.", null, null, DateTime.Now, TimeSpan.FromMinutes(60)));
            var count = 0;
            await foreach (var item in result)
            {
                if (item.Resolution == MediaResolution.TenEightyP)
                    continue;
                Assert.AreEqual(PredefinedMediaSources.LocalFile.Id, item.Type.Id);
                Assert.AreEqual(MediaCodec.HEVC, item.MediaCodec);
                Assert.AreEqual(MediaResolution.SevenTwentyP, item.Resolution);
                count++;
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void GetIconStreamTest()
        {
            Assert.NotNull(subject.GetIconStream());
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            Directory.Delete(tmp, true);
        }
    }
}
