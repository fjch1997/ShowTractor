using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShowTractor.Tests.Mocks
{
    public class TestHttpMessageHandler : HttpMessageHandler
    {
        public const string ImageUrl = "http://example.com/1.jpg";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri?.ToString() == ImageUrl)
            {
                return Task.FromResult(new HttpResponseMessage() { Content = new ByteArrayContent(new byte[1024]) });
            }
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }
}
