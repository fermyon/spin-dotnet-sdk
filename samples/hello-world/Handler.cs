using Fermyon.Spin.Sdk;
using System.Text;

namespace Fermyon.Spin.HelloWorld;

public static class Handler
{
    [HttpHandler]
    public static HttpResponse HandleHttpRequest(HttpRequest request) => request.Url.ToString() switch
    {
        "/info" => LogFullRequestInfo(request),
        _ => HelloWorld(),
    };

    private static HttpResponse HelloWorld() => new HttpResponse
    {
        Status = 200,
        BodyAsString = "Hello, world! For more information, try visiting /info",
    };

    private static HttpResponse LogFullRequestInfo(HttpRequest request)
    {
        var responseText = new StringBuilder();
        responseText.AppendLine($"Called with method {request.Method}, Url {request.Url.ToString()}");

        foreach (var h in request.Headers.AsSpan())
        {
            responseText.AppendLine($"Header '{h.Key}' had value '{h.Value}'");
        }

        foreach (var p in request.Parameters.AsSpan())
        {
            responseText.AppendLine($"Parameter '{p.Key}' had value '{p.Value}'");
        }

        var bodyInfo = request.Body.TryGetValue(out var bodyBuffer) ?
            $"The body (as a string) was: {Encoding.UTF8.GetString(bodyBuffer.AsSpan())}\n" :
            "The body was empty\n";
        responseText.AppendLine(bodyInfo);

        return new HttpResponse
        {
            Status = 200,
            Headers = Optional.From(HttpKeyValues.FromDictionary(new Dictionary<string, string>
            {
                { "Content-Type", "text/plain" },
                { "X-TestHeader", "this is a test" },
            })),
            BodyAsString = responseText.ToString(),
        };
    }
}
