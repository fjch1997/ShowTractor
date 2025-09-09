using NUnit.Framework;
using NUnit.Framework.Legacy;
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
                ClassicAssert.AreEqual("Star Trek: Discovery", item.ShowName);
                ClassicAssert.AreEqual(i, item.Season);
                ClassicAssert.IsFalse(string.IsNullOrEmpty(item.ShowDescription));
                ClassicAssert.AreNotEqual(item.ShowDescription, item.SeasonDescription);
                if (i == 0)
                {
                    ClassicAssert.AreEqual(32, item.Episodes.Count);
                    ClassicAssert.AreEqual("“Will You Take My Hand?” Bonus Scene", item.Episodes[0].Name);
                    ClassicAssert.That(item.ShowFinale, Is.False);
                    ClassicAssert.That(item.ShowEnded, Is.EqualTo(ended));
                }
                else if (i == 1)
                {
                    ClassicAssert.AreEqual(15, item.Episodes.Count);
                    ClassicAssert.AreEqual("The Vulcan Hello", item.Episodes[0].Name);
                    ClassicAssert.That(item.ShowFinale, Is.False);
                    ClassicAssert.That(item.ShowEnded, Is.EqualTo(ended));
                }
                else if (i == 2)
                {
                    ClassicAssert.AreEqual(14, item.Episodes.Count);
                    ClassicAssert.AreEqual("Brother", item.Episodes[0].Name);
                    ClassicAssert.That(item.ShowFinale, Is.False);
                    ClassicAssert.That(item.ShowEnded, Is.EqualTo(ended));
                }
                else if (i == 3)
                {
                    ClassicAssert.AreEqual(13, item.Episodes.Count);
                    ClassicAssert.AreEqual("That Hope Is You, Part 1", item.Episodes[0].Name);
                    ClassicAssert.That(item.ShowFinale, Is.EqualTo(ended));
                    ClassicAssert.That(item.ShowEnded, Is.EqualTo(ended));
                }
                else
                {
                    ClassicAssert.Fail("Too many seasons returned.");
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
                ClassicAssert.That(item.Episodes, Is.All.Matches((TvEpisode e) => e.FirstAirDate.Kind == DateTimeKind.Utc || e.FirstAirDate == default));
            }
        }
        [Test]
        public async Task GetUpdatesAsyncTestAsync([Values(null, "67198")] string? uniqueId)
        {
            var season = new TvSeason(uniqueId, "Star Trek: Discovery", 1, new List<string>(), new List<string>(), "Old Description", "Old Season Description", null, null, false, false, new List<TvEpisode>(), new Dictionary<string, string>());
            var subject = new TmdbMetadataProvider(new HttpClient(this));
            var (updated, getNewSeasonsFunc) = await subject.GetUpdatesAsync(season, AdditionalAttributes.Empty, default);
            AssertSeason1(updated);
            await TestGetNewSeasonsAfterSeason1(updated, getNewSeasonsFunc);
            await TestGetNewSeasonsAfterSeason2(updated, getNewSeasonsFunc);
        }
        private static async Task TestGetNewSeasonsAfterSeason1(TvSeason updated, GetLatestSeasonsDelegate getNewSeasonsFunc)
        {
            int i = 0;
            await foreach (var newSeason in getNewSeasonsFunc(1))
            {
                switch (newSeason.Season)
                {
                    case 2:
                        AssertSeason2(newSeason);
                        break;
                    case 3:
                        AssertSeason3(newSeason);
                        break;
                    default:
                        throw new AssertionException("Unrecognized season returned by TmdbMetadataProvider.");
                }
                i++;
            }
            ClassicAssert.AreEqual(2, i);
        }
        private static async Task TestGetNewSeasonsAfterSeason2(TvSeason updated, GetLatestSeasonsDelegate getNewSeasonsFunc)
        {
            int i = 0;
            await foreach (var newSeason in getNewSeasonsFunc(2))
            {
                switch (newSeason.Season)
                {
                    case 3:
                        AssertSeason3(newSeason);
                        break;
                    default:
                        throw new AssertionException("Unrecognized season returned by TmdbMetadataProvider.");
                }
                i++;
            }
            ClassicAssert.AreEqual(1, i);
        }
        private static void AssertSeason1(TvSeason updated)
        {
            ClassicAssert.AreEqual("Star Trek: Discovery", updated.ShowName);
            ClassicAssert.AreEqual(15, updated.Episodes.Count);
            ClassicAssert.AreEqual("The Vulcan Hello", updated.Episodes[0].Name);
        }
        private static void AssertSeason2(TvSeason newSeason)
        {
            ClassicAssert.AreEqual("Star Trek: Discovery", newSeason.ShowName);
            ClassicAssert.AreEqual(14, newSeason.Episodes.Count);
            ClassicAssert.AreEqual("Brother", newSeason.Episodes[0].Name);
        }
        private static void AssertSeason3(TvSeason newSeason)
        {
            ClassicAssert.AreEqual("Star Trek: Discovery", newSeason.ShowName);
            ClassicAssert.AreEqual(13, newSeason.Episodes.Count);
            ClassicAssert.AreEqual("That Hope Is You, Part 1", newSeason.Episodes[0].Name);
        }
        [Test]
        public void IconTest()
        {
            var subject = new TmdbMetadataProvider(new HttpClient(this));
            using var stream = subject.GetIconStream();
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            ClassicAssert.That(memoryStream.ToArray(), Is.Not.Empty);
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