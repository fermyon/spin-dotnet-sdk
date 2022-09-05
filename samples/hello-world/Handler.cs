using Fermyon.Spin.Sdk;
using System.Net;
using System.Text;

namespace Fermyon.Spin.HelloWorld;

public static class Handler
{
    [HttpHandler]
    public static HttpResponse HandleHttpRequest(HttpRequest request)
    {
        if (request.Url.StartsWith("/outbound"))
        {
            return UseOutboundHttp(request);
        }
        return request.Url switch
        {
            "/redis" => UseRedis(request),
            "/pg" => UsePostgresQuery(request),
            "/pgins" => UsePostgresExec(request),
            _ => EchoRequestInfo(request),
        };
    }

    private static HttpResponse UseOutboundHttp(HttpRequest request)
    {
        var onboundRequest = new HttpRequest
        {
            Method = Fermyon.Spin.Sdk.HttpMethod.Delete,
            Url = "http://127.0.0.1:3001/testingtesting?thing=otherthing",
            Headers = HttpKeyValues.FromDictionary(new Dictionary<string, string>
            {
                { "X-Outbound-Test", "From .NET" },
                { "Accept", "text/plain" },
            }),
            Body = Optional.From(Buffer.FromString("see the little goblin, see his little feet")),
        };

        string onboundInfo;

        try
        {
            var response = HttpOutbound.Send(onboundRequest);
            var status = response.StatusCode;
            var onboundSucceeded = (int)status >= 200 && (int)status <= 299;
            var onboundResponseText = status == HttpStatusCode.OK ?
                response.BodyAsString :
                "<error>";
            onboundInfo = onboundSucceeded ?
                $"The onbound request returned status {status} with {response.Headers.Count} headers ({FormatHeadersShort(response.Headers)}) and the body was:\n{onboundResponseText}\n" :
                $"Tragically the onbound request failed with code {status}\n";
        }
        catch (Exception ex)
        {
            onboundInfo = $"Onbound call exception {ex}";
        }

        var responseText = new StringBuilder();
        responseText.AppendLine($"Called with method {request.Method}, Url {request.Url}");

        responseText.AppendLine($"The spin-full-url header was {request.Headers["spin-full-url"]}");
        foreach (var h in request.Headers)
        {
            responseText.AppendLine($"Header '{h.Key}' had value '{h.Value}'");
        }

        var uri = new System.Uri(request.Headers["spin-full-url"]);
        var queryParameters = System.Web.HttpUtility.ParseQueryString(uri.Query);
        foreach (var key in queryParameters.AllKeys)
        {
            responseText.AppendLine($"Parameter '{key}' had value '{queryParameters[key]}'");
        }

        var bodyInfo = request.Body.HasContent() ?
            $"The body (as a string) was: {request.Body.AsString()}\n" :
            "The body was empty\n";
        responseText.AppendLine(bodyInfo);

        responseText.AppendLine(onboundInfo);

        return new HttpResponse
        {
            StatusCode = HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/plain" },
                { "X-TestHeader", "this is a test" },
            },
            BodyAsString = responseText.ToString(),
            // BodyAsBytes = RandomTextBytes(),
        };
    }

    private static HttpResponse EchoRequestInfo(HttpRequest request)
    {
        // Warmup

        var responseText = new StringBuilder();
        responseText.AppendLine($"Called with method {request.Method}, Url {request.Url}");

        foreach (var h in request.Headers)
        {
            responseText.AppendLine($"Header '{h.Key}' had value '{h.Value}'");
        }

        var uri = new System.Uri(request.Headers["spin-full-url"]);
        var queryParameters = System.Web.HttpUtility.ParseQueryString(uri.Query);
        foreach (var key in queryParameters.AllKeys)
        {
            responseText.AppendLine($"Parameter '{key}' had value '{queryParameters[key]}'");
        }

        var bodyInfo = request.Body.HasContent() ?
            $"The body (as a string) was: {request.Body.AsString()}\n" :
            "The body was empty\n";
        responseText.AppendLine(bodyInfo);

        if (request.Url != Warmup.DefaultWarmupUrl)
        {
            responseText.AppendLine("We now present the contents of a static asset:");
            responseText.AppendLine(File.ReadAllText("/assets/asset-text.txt"));
            responseText.AppendLine("And here are some config strings:");
            responseText.AppendLine($"- 'defaulted' has value {SpinConfig.Get("defaulted")}");
            try
            {
                var requiredCfg = SpinConfig.Get("required");
                responseText.AppendLine($"- 'required' has value {requiredCfg}");
            }
            catch
            {
                responseText.AppendLine("- Oh no!  'required' was not set!");
            }
            responseText.AppendLine("We hope you enjoyed this external data!");
        }

        return new HttpResponse
        {
            StatusCode = HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/plain" },
                { "X-TestHeader", "this is a test" },
            },
            BodyAsString = responseText.ToString(),
        };
    }

    private static IEnumerable<byte> RandomTextBytes()
    {
        Random r = new Random();
        var a = (int)'a';
        while (true)
        {
            int rv = r.Next(25);
            yield return (byte)(a + rv);
            if (rv == 0)
            {
                yield break;
            }
        }
    }


    private static HttpResponse UseRedis(HttpRequest request)
    {
        var address = "redis://127.0.0.1:6379";
        var key = "mykey";
        var channel = "messages";

        var payload = request.Body.TryGetValue(out var bodyBuffer) ? bodyBuffer : throw new Exception("cannot read body");
        RedisOutbound.Set(address, key, payload);

        var res = RedisOutbound.Get(address, key).ToUTF8String();

        RedisOutbound.Publish(address, channel, payload);

        return new HttpResponse
        {
            StatusCode = HttpStatusCode.OK,
            BodyAsString = res
        };
    }

    private static HttpResponse UsePostgresQuery(HttpRequest request)
    {
        var connectionString = "user=ivan password=pg314159$ dbname=ivantest host=127.0.0.1";

        var result = PostgresOutbound.Query(connectionString, "SELECT * FROM test");

        var responseText = new StringBuilder();

        responseText.AppendLine($"Got {result.Rows.Count} row(s)");
        responseText.AppendLine($"COL: [{String.Join(" | ", result.Columns.Select(FmtCol))}]");

        string FmtEntry(DbValue v)
        {
            return v.Value() switch
            {
                null => "<DBNULL>",
                var val => val.ToString() ?? "<NULL>",
            };
        }

        foreach (var row in result.Rows)
        {
            responseText.AppendLine($"ROW: [{String.Join(" | ", row.Select(FmtEntry))}]");
        }

        return new HttpResponse
        {
            StatusCode = HttpStatusCode.OK,
            BodyAsString = responseText.ToString(),
        };
    }

    private static HttpResponse UsePostgresExec(HttpRequest request)
    {
        var connectionString = "user=ivan password=pg314159$ dbname=ivantest host=127.0.0.1";

        var id = new Random().Next(100000);
        var result = PostgresOutbound.Execute(connectionString, "INSERT INTO test VALUES ($1, 'something', 'something else')", id);

        var responseText = $"Updates {result} rows\n";

        return new HttpResponse
        {
            StatusCode = HttpStatusCode.OK,
            BodyAsString = responseText,
        };
    }

    private static string FormatHeadersShort(IReadOnlyDictionary<string, string> headers)
    {
        if (headers.Count == 0)
        {
            return "<no headers>";
        }
        return String.Join(" / ", headers.Select(kvp => $"{kvp.Key}={kvp.Value}"));
    }

    private static string FmtCol(PgColumn c)
    {
        return $"{c.Name} ({c.DataType})";
    }
}
