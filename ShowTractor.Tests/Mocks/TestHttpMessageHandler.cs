using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShowTractor.Tests.Mocks
{
    public class TestHttpMessageHandler : HttpMessageHandler
    {
        public const string ImageUrl = "http://example.com/1.jpg";
        public const long ImageSize = 1024;
        public static readonly Uri TestTvSeason1Uri = new Uri("https://www.example.com/TestTvSeason1/poster.jpg");
        public const long TestTvSeason1Size = 1023;
        public static readonly Uri TestTvEpisode1Uri = new Uri("https://www.example.com/TestTvSeason1/1.jpg");
        public const long TestTvEpisode1Size = 1022;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri?.ToString() == ImageUrl)
            {
                return Task.FromResult(new HttpResponseMessage() { Content = new ByteArrayContent(new byte[ImageSize]) });
            }
            else if (request.RequestUri == TestTvSeason1Uri)
            {
                return Task.FromResult(new HttpResponseMessage() { Content = new ByteArrayContent(new byte[TestTvSeason1Size]) });
            }
            else if (request.RequestUri == TestTvEpisode1Uri)
            {
                return Task.FromResult(new HttpResponseMessage() { Content = new ByteArrayContent(new byte[TestTvEpisode1Size]) });
            }

            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }
}
