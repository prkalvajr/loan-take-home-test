using System.Net;

namespace Loans.UnitTests.Fakes;

/// <summary>Answers every request with a fixed status (or throws) and records what was sent.</summary>
public sealed class StubHttpMessageHandler(HttpStatusCode status = HttpStatusCode.OK, Exception? throws = null) : HttpMessageHandler
{
    public List<(HttpMethod Method, string Path, string? IdempotencyKey, string Body)> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancellationToken);
        var key = request.Headers.TryGetValues("Idempotency-Key", out var values) ? values.Single() : null;
        Requests.Add((request.Method, request.RequestUri!.AbsolutePath, key, body));

        if (throws is not null)
            throw throws;

        return new HttpResponseMessage(status);
    }
}
