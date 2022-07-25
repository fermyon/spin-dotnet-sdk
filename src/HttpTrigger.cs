namespace Fermyon.Spin.Sdk;

/// <summary>
/// Marks a method as being the handler for the Spin HTTP trigger.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class HttpHandlerAttribute : Attribute {}

/// <summary>
/// The HTTP method of a request.
/// </summary>
public enum HttpMethod
{
    /// <summary>The GET method.</summary>
    Get,
    /// <summary>The POST method.</summary>
    Post,
    /// <summary>The PUT method.</summary>
    Put,
    /// <summary>The DELETE method.</summary>
    Delete,
    /// <summary>The PATCH method.</summary>
    Patch,
    /// <summary>The HEAD method.</summary>
    Head,
    /// <summary>The OPTIONS method.</summary>
    Options,
}

/// <summary>
/// A HTTP request.
/// </summary>
public sealed class HttpRequest
{
    // All these get set by the builder. But this prevents nullability warnings
    // from the use of init rather than a constructor
    internal HttpRequest(
        HttpMethod method,
        string uri,
        Dictionary<string, string> headers,
        Dictionary<string, string> parameters,
        byte[] body
    )
    {
        Method = method;
        Uri = uri;
        Headers = headers;
        Parameters = parameters;
        BodyArray = body;
    }

    /// <summary>Gets the request method.</summary>
    public HttpMethod Method { get; init; }
    /// <summary>Gets the request URI.</summary>
    public string Uri { get; init; }
    /// <summary>Gets the request headers.</summary>
    public Dictionary<string, string> Headers { get; init; }
    /// <summary>Gets the request query string parameters.</summary>
    public Dictionary<string, string> Parameters { get; init; }
    internal byte[] BodyArray { get; init; }
    /// <summary>Gets the request body.</summary>
    public ReadOnlySpan<byte> Body => new (BodyArray);
}

/// <summary>
/// A HTTP response.
/// </summary>
public sealed class HttpResponse
{
    // We don't expect any of these defaults to get used but we want
    // to avoid things being null.
    /// Initializes a new instance of the HttpResponse type.
    public HttpResponse()
    {
        Status = 200;
        Headers = new();
        Body = Enumerable.Empty<byte>();
    }

    /// <summary>Gets or sets the HTTP status code.</summary>
    public short Status { get; set; }
    /// <summary>Gets or sets the response headers.</summary>
    public Dictionary<string, string> Headers { get; set; }
    /// <summary>Gets or sets the response body.</summary>
    public IEnumerable<byte> Body { get; set; }

    internal HttpResponseInterop ToInterop()
    {
        return new HttpResponseInterop(
            Status,
            Headers.Select(kvp => new StringPair { Key = kvp.Key, Value = kvp.Value }).ToArray(),
            Body.ToArray()
        );
    }
}
