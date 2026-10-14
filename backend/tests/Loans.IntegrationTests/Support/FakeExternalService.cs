using System.Net;

namespace Loans.IntegrationTests.Support;

/// <summary>
/// Stands in for the external service inside the test host. Records every request and answers
/// with whatever <see cref="Respond"/> returns (200 by default).
/// </summary>
public sealed class FakeExternalService : HttpMessageHandler
{
    public List<(HttpMethod Method, string Path, string Body)> Requests { get; } = [];

    public Func<string, HttpStatusCode> Respond { get; set; } = _ => HttpStatusCode.OK;

    public void Reset()
    {
        Requests.Clear();
        Respond = _ => HttpStatusCode.OK;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = await request.Content!.ReadAsStringAsync(cancellationToken);
        Requests.Add((request.Method, request.RequestUri!.AbsolutePath, body));
        return new HttpResponseMessage(Respond(body));
    }
}
