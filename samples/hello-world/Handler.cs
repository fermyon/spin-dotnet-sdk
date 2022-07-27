using Fermyon.Spin.Sdk;
using System.Text;

namespace Fermyon.Spin.HelloWorld;

public static class Handler
{
    [HttpHandler]
    public static unsafe void HandleHttpRequest(WitRequest* witRequest, WitResponse* witResponse)
    {
        var requestInfo = $"Called with method {witRequest->Method}, Url {witRequest->Url}";

        var headerInfo = String.Join("\n", witRequest->Headers.ToEnumerable().Select(p => $"Header '{p.Key}' had value '{p.Value}'"));
        var parameterInfo = String.Join("\n", witRequest->Parameters.ToEnumerable().Select(p => $"Parameter '{p.Key}' had value '{p.Value}'"));

        var bodyInfo = witRequest->Body.TryGetValue(out var bodyBuffer) ?
            $"The body (as a string) was: {Encoding.UTF8.GetString(bodyBuffer.AsSpan())}\n" :
            "The body was empty\n";

        var responseBody = String.Join("\n", new[] { requestInfo, headerInfo, parameterInfo, bodyInfo });
        witResponse->Status = 200;
        witResponse->Headers = WitOptional.From(WitKeyValues.FromDictionary(new Dictionary<string, string>
        {
            { "Content-Type", "text/plain" },
            { "X-TestHeader", "this is a test" },
        }));
        witResponse->Body = WitOptional.From(WitBuffer.StringAsUtf8(responseBody));
    }
}
