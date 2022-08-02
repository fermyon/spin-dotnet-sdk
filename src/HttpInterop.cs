using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fermyon.Spin.Sdk;

public enum HttpMethod : byte
{
    Get = 0,
    Post = 1,
    Put = 2,
    Delete = 3,
    Patch = 4,
    Head = 5,
    Options = 6,
}

[StructLayout(LayoutKind.Sequential)]
public struct HttpResponse
{
    public int Status;
    public Optional<HttpKeyValues> Headers;
    public Optional<Buffer> Body;

    public string? BodyAsString
    {
        get => Body.TryGetValue(out var buffer) ? buffer.ToInteropString().ToString() : null;
        set => Body = value is null ? Optional<Buffer>.None : Optional.From(Buffer.FromString(value));
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct HttpRequestImpl
{
    public HttpMethod Method;
    public InteropString Url;
    public HttpKeyValues Headers;
    public HttpKeyValues Parameters;
    public Optional<Buffer> Body;
}

public sealed class HttpRequest
{
    public HttpRequest()
    {
        Method = HttpMethod.Get;
        Url = String.Empty;
        Headers = new();
        Parameters = new();
        Body = Optional<Buffer>.None;
    }

    public HttpRequest(
        HttpMethod method,
        string url,
        Dictionary<string, string> headers,
        Dictionary<string, string> parameters,
        Optional<Buffer> body
    )
    {
        Method = method;
        Url = url;
        Headers = headers;
        Parameters = parameters;
        Body = body;
    }

    internal static HttpRequest From(HttpRequestImpl impl)
    {
        return new HttpRequest(
            impl.Method,
            impl.Url.ToString(),
            new(), //impl.Headers.ToDictionary(),
            new(), //impl.Parameters.ToDictionary(),
            impl.Body
        );
    }

    internal HttpRequestImpl ToInterop()
    {
        return new HttpRequestImpl
        {
            Method = this.Method,
            Url = InteropString.FromString(this.Url),
            Headers = HttpKeyValues.FromDictionary(this.Headers),
            Parameters = HttpKeyValues.FromDictionary(this.Parameters),
            Body = this.Body,
        };
    }

    public HttpMethod Method { get; set; }
    public string Url { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public Dictionary<string, string> Parameters { get; set; }

    public Optional<Buffer> Body { get; set; }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct HttpKeyValues
{
    private readonly HttpKeyValue* _valuesPtr;
    private readonly int _valuesLen;

    internal HttpKeyValues(HttpKeyValue* ptr, int length)
    {
        _valuesPtr = ptr;
        _valuesLen = length;
    }

    public static HttpKeyValues FromDictionary(IReadOnlyDictionary<string, string> dictionary)
    {
        var unmanagedValues = (HttpKeyValue*)Marshal.AllocHGlobal(dictionary.Count * sizeof(HttpKeyValue));
        var span = new Span<HttpKeyValue>(unmanagedValues, dictionary.Count);
        var index = 0;
        foreach (var (key, value) in dictionary)
        {
            span[index] = new HttpKeyValue(InteropString.FromString(key), InteropString.FromString(value));
            index++;
        }
        return new HttpKeyValues(unmanagedValues, dictionary.Count);
    }

    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        foreach (var entry in AsSpan())
        {
            dict[entry.Key.ToString()] = entry.Value.ToString();
        }
        return dict;
    }

    public Span<HttpKeyValue> AsSpan()
        => new Span<HttpKeyValue>(_valuesPtr, _valuesLen);
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct HttpKeyValue
{
    public readonly InteropString Key;
    public readonly InteropString Value;

    internal HttpKeyValue(InteropString key, InteropString value)
    {
        Key = key;
        Value = value;
    }
}

internal static class OutboundHttpInterop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe byte wasi_outbound_http_request(ref HttpRequestImpl req, ref HttpResponse ret0);
}
