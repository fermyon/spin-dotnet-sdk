using Fermyon.Spin.Sdk;

namespace Fermyon.Spin.HelloWorld;

public static class Handler
{
    [HttpHandler]
    public static HttpResponse HandleHttpRequest(HttpRequest r)
    {
        var requestInfo = $"Called with method {r.Method} on URL {r.Uri}";
        var headerInfo = String.Join("\n", r.Headers.Select(p => $"Header '{p.Key}' had value '{p.Value}'"));
        var bodyInfo = r.Body.Length > 0 ?
            $"The body (as a string) was: {System.Text.Encoding.UTF8.GetString(r.Body)}\n" :
            "The body was empty\n";
        var responseBody = String.Join("\n", new[] { requestInfo, headerInfo, bodyInfo });

        return new HttpResponse {
            Status = 200,
            Headers = new Dictionary<string, string> {
                { "Content-Type", "text/plain" },
                { "X-TestHeader", "this is a test" },
            },
            Body = System.Text.Encoding.UTF8.GetBytes(responseBody),
        };
    }
}
