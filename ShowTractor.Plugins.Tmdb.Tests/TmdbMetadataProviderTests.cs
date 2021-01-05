using NUnit.Framework;
using ShowTractor.Plugins.Interfaces;
using ShowTractor.Plugins.Tmdb.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShowTractor.Plugins.Tmdb.Tests
{
    [TestFixture]
    public class TmdbMetadataProviderTests : HttpMessageHandler
    {
        private bool ended;
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Settings.Default.ApiKey = "Test Key";
        }
        [Test]
        public async Task SearchAsyncTest([Values] bool ended)
        {
            this.ended = ended;
            var subject = new TmdbMetadataProvider(new HttpClient(this));
            var i = 0;
            await foreach (var item in subject.SearchAsync("star trek discovery", default))
            {
                Assert.AreEqual("Star Trek: Discovery", item.ShowName);
                Assert.AreEqual(i, item.Season);
                Assert.IsFalse(string.IsNullOrEmpty(item.ShowDescription));
                Assert.AreNotEqual(item.ShowDescription, item.SeasonDescription);
                if (i == 0)
                {
                    Assert.AreEqual(32, item.Episodes.Count);
                    Assert.AreEqual("“Will You Take My Hand?” Bonus Scene", item.Episodes[0].Name);
                    Assert.That(item.ShowFinale, Is.False);
                    Assert.That(item.ShowEnded, Is.EqualTo(ended));
                }
                else if (i == 1)
                {
                    Assert.AreEqual(15, item.Episodes.Count);
                    Assert.AreEqual("The Vulcan Hello", item.Episodes[0].Name);
                    Assert.That(item.ShowFinale, Is.False);
                    Assert.That(item.ShowEnded, Is.EqualTo(ended));
                }
                else if (i == 2)
                {
                    Assert.AreEqual(14, item.Episodes.Count);
                    Assert.AreEqual("Brother", item.Episodes[0].Name);
                    Assert.That(item.ShowFinale, Is.False);
                    Assert.That(item.ShowEnded, Is.EqualTo(ended));
                }
                else if (i == 3)
                {
                    Assert.AreEqual(13, item.Episodes.Count);
                    Assert.AreEqual("That Hope Is You, Part 1", item.Episodes[0].Name);
                    Assert.That(item.ShowFinale, Is.EqualTo(ended));
                    Assert.That(item.ShowEnded, Is.EqualTo(ended));
                }
                else
                {
                    Assert.Fail("Too many seasons returned.");
                }
                i++;
            }
        }
        [Test]
        public async Task TimezoneTest()
        {
            var subject = new TmdbMetadataProvider(new HttpClient(this));
            await foreach (var item in subject.SearchAsync("star trek discovery", default))
            {
                Assert.That(item.Episodes, Is.All.Matches((TvEpisode e) => e.FirstAirDate.Kind == DateTimeKind.Utc || e.FirstAirDate == default));
            }
        }
        [Test]
        public async Task GetUpdatesAsyncTestAsync([Values(null, "67198")] string? uniqueId)
        {
            var season = new TvSeason(uniqueId, "Star Trek: Discovery", 1, new List<string>(), new List<string>(), "Old Description", "Old Season Description", null, null, false, false, new List<TvEpisode>(), new Dictionary<string, string>());
            var subject = new TmdbMetadataProvider(new HttpClient(this));
            var updated = await subject.GetUpdatesAsync(season, AdditionalAttributes.Empty, default);
            Assert.AreEqual("Star Trek: Discovery", updated.ShowName);
            Assert.AreEqual(15, updated.Episodes.Count);
            Assert.AreEqual("The Vulcan Hello", updated.Episodes[0].Name);
        }
        [Test]
        public void IconTest()
        {
            var subject = new TmdbMetadataProvider(new HttpClient(this));
            using var stream = subject.GetIconStream();
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            Assert.That(memoryStream.ToArray(), Is.Not.Empty);
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri == null) throw new InvalidOperationException(nameof(request.RequestUri) + " is null.");
            var message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            if (request.RequestUri.PathAndQuery.StartsWith("/3/search/tv"))
            {
                message.Content = new StreamContent(File.OpenRead("TestFixtures/search.json"));
            }
            else if (request.RequestUri.PathAndQuery.StartsWith("/3/tv/67198/season/0"))
            {
                message.Content = new StreamContent(File.OpenRead("TestFixtures/season_67198_0.json"));
            }
            else if (request.RequestUri.PathAndQuery.StartsWith("/3/tv/67198/season/1"))
            {
                message.Content = new StreamContent(File.OpenRead("TestFixtures/season_67198_1.json"));
            }
            else if (request.RequestUri.PathAndQuery.StartsWith("/3/tv/67198/season/2"))
            {
                message.Content = new StreamContent(File.OpenRead("TestFixtures/season_67198_2.json"));
            }
            else if (request.RequestUri.PathAndQuery.StartsWith("/3/tv/67198/season/3"))
            {
                message.Content = new StreamContent(File.OpenRead("TestFixtures/season_67198_3.json"));
            }
            else if (request.RequestUri.PathAndQuery.StartsWith("/3/tv/67198"))
            {
                if (ended)
                    message.Content = new StreamContent(File.OpenRead("TestFixtures/tv_67198_ended.json"));
                else
                    message.Content = new StreamContent(File.OpenRead("TestFixtures/tv_67198.json"));
            }
            else if (request.RequestUri.PathAndQuery.StartsWith("/3/configuration"))
            {
                message.Content = new StreamContent(File.OpenRead("TestFixtures/configuration.json"));
            }
            else
            {
                message.StatusCode = System.Net.HttpStatusCode.NotFound;
                return Task.FromResult(message);
            }
            message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json")
            {
                CharSet = "utf-8"
            };
            return Task.FromResult(message);
        }
    }
}