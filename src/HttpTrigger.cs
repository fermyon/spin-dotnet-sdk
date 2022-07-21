namespace Fermyon.Spin.Sdk;


[AttributeUsage(AttributeTargets.Method)]
public sealed class HttpHandlerAttribute : Attribute {}

public enum HttpMethod
{
    Get,
    Post,
    Put,
    Delete,
    Patch,
    Head,
    Options,
}

// TODO: fix nullability warnings - probably requires a constructor though!
public class HttpRequest
{
    public HttpMethod Method { get; init; }
    public string Uri { get; init; }
    public Dictionary<string, string> Headers { get; init; }
    public Dictionary<string, string> Parameters { get; init; }
    internal byte[] BodyArray { get; init; }
    public ReadOnlySpan<byte> Body => new (BodyArray);
}

public class HttpResponse
{
    public short Status { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public IEnumerable<byte> Body { get; set; }

    internal HttpResponseInterop ToInterop() {
        return new HttpResponseInterop {
            Status = Status,
            Headers = Headers.Select(kvp => new StringPair { Key = kvp.Key, Value = kvp.Value }).ToArray(),
            Body = Body.ToArray(),
        };
    }
}
