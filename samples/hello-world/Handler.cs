using Fermyon.Spin.Sdk;
using System.Text;

namespace Fermyon.Spin.HelloWorld;

public static class Handler
{
    [HttpHandler]
    public static void HandleHttpRequest(HttpRequest witRequest, out HttpResponse witResponse)
    {
        var response = new StringBuilder();
        response.AppendLine($"Called with method {witRequest.Method}, Url {witRequest.Url.ToString()}");

        foreach (var h in witRequest.Headers.AsSpan())
        {
            response.AppendLine($"Header '{h.Key}' had value '{h.Value}'");
        }

        foreach (var p in witRequest.Parameters.AsSpan())
        {
            response.AppendLine($"Parameter '{p.Key}' had value '{p.Value}'");
        }

        var bodyInfo = witRequest.Body.TryGetValue(out var bodyBuffer) ?
            $"The body (as a string) was: {Encoding.UTF8.GetString(bodyBuffer.AsSpan())}\n" :
            "The body was empty\n";
        response.AppendLine(bodyInfo);

        witResponse.Status = 200;
        witResponse.Headers = WitOptional.From(HttpKeyValues.FromDictionary(new Dictionary<string, string>
        {
            { "Content-Type", "text/plain" },
            { "X-TestHeader", "this is a test" },
        }));
        witResponse.Body = WitOptional.From(Sdk.HttpBuffer.FromString(response.ToString()));
    }
}
