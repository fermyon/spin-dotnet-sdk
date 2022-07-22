namespace Fermyon.Spin.Sdk;

// TODO: the idea of these types is that they're easier to convert to/from
// WIT than the more idiomatic types exposes to user code.  But could we
// instead work this by having one type in each direction, with WIT-friendly
// private fields and idiomatic adapter properties, and save the complexity?

internal class HttpResponseInterop
{
    internal HttpResponseInterop(
        short status,
        StringPair[] headers,
        byte[] body
    )
    {
        Status = status;
        Headers = headers;
        Body = body;
    }
    
    public short Status;
    public StringPair[] Headers;
    public byte[] Body;
}

internal class HttpRequestInterop
{
    // Used by the embedding layer which already sets the fields to
    // safe values, but this avoids nullability warnings.
    internal HttpRequestInterop()
    {
        Method = 0;
        Uri = String.Empty;
        Headers = Array.Empty<StringPair>();
        Parameters = Array.Empty<StringPair>();
        Body = Array.Empty<byte>();
    }

    public byte Method;
    public string Uri;
    public StringPair[] Headers;
    public StringPair[] Parameters;
    public byte[] Body;

    public HttpRequest Build()
    {
        // TODO: ToDictionary() crashes on duplicate key - custom safe wrapper?
        return new HttpRequest(
            MethodOf(Method),
            Uri,
            Headers.ToDictionary(p => p.Key, p => p.Value), 
            Parameters.ToDictionary(p => p.Key, p => p.Value),
            Body
        );
    }

    private static HttpMethod MethodOf(byte method)
    {
        // Numeric values MUST be kept in sync with the WIT
        switch (method) {
            case 0: return HttpMethod.Get;
            case 1: return HttpMethod.Post;
            case 2: return HttpMethod.Put;
            case 3: return HttpMethod.Delete;
            case 4: return HttpMethod.Patch;
            case 5: return HttpMethod.Head;
            case 6: return HttpMethod.Options;
            default: throw new ArgumentOutOfRangeException(nameof(method));
        }
    }
}

internal class StringPair
{
    public StringPair()
    {
        Key = String.Empty;
        Value = String.Empty;
    }

    public string Key;
    public string Value;
}
