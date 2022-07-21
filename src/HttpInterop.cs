namespace Fermyon.Spin.Sdk;

internal class HttpResponseInterop
{
    public short Status;
    public StringPair[] Headers;
    public byte[] Body;
}

internal class HttpRequestInterop
{
    public byte Method;
    public string Uri;
    public StringPair[] Headers;
    public StringPair[] Parameters;
    public byte[] Body;

    public HttpRequest Build()
    {
        // TODO: ToDictionary() crashes on duplicate key - custom safe wrapper?
        return new HttpRequest {
            Method = MethodOf(Method),
            Uri = Uri,
            Headers = Headers.ToDictionary(p => p.Key, p => p.Value), 
            Parameters = Parameters.ToDictionary(p => p.Key, p => p.Value),
            BodyArray = Body,
        };
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
    public string Key;
    public string Value;
}
