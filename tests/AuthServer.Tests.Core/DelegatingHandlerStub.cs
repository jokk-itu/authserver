using System.Net;
using System.Text;
using AuthServer.Constants;

namespace AuthServer.Tests.Core;
public class DelegatingHandlerStub : DelegatingHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;
    public DelegatingHandlerStub(string content, HttpStatusCode statusCode)
    {
        _handlerFunc = (_, _) => Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, MimeTypeConstants.Json)
        });
    }

    public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
    {
        _handlerFunc = handlerFunc;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _handlerFunc(request, cancellationToken);
    }
}