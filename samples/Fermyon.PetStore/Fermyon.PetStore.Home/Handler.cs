using System.IO;

using Fermyon.Spin.Sdk;

namespace Fermyon.PetStore;

public static class Handler
{
    [HttpHandler]
    public static HttpResponse HandleHttpRequest(HttpRequest request)
    {
        var html = request.IsRuntime() ?
            ReadAllText("/assets/Home.html") :
            String.Empty;

        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/html" },
            },
            BodyAsString = html,
        };
    }

    public static string ReadAllText(string path)
    {
        using (var stm = File.OpenRead(path))
        {
            using (var rdr = new StreamReader(stm))
            {
                return rdr.ReadToEnd();
            }
        }
    }
}

public static class HttpRequestExtensions
{
    public static bool IsRuntime(this HttpRequest request)
    {
        return request.Url != Warmup.DefaultWarmupUrl;
    }
}
