using Moq;
using Moq.Protected;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CalorieMonitor.UnitTests.Mocks.External
{
    public class MockHttpMessageHandler : MyMock<HttpMessageHandler>
    {
        public void MockSendAsync(string url, HttpResponseMessage httpResponse)
        {
            this.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(b => b.RequestUri.OriginalString == url),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse).Verifiable();

            verifications.Add(() =>
                this.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", 
                Times.Once(), 
                ItExpr.Is<HttpRequestMessage>(b => b.RequestUri.OriginalString == url), 
                ItExpr.IsAny<CancellationToken>()));
        }
    }
}
