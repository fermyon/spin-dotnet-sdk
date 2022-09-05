using System.Collections.Immutable;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fermyon.Spin.Sdk;

/// <summary>
/// An HTTP method.
/// </summary>
public enum HttpMethod : byte
{
    /// <summary>
    /// The GET method.
    /// </summary>
    Get = 0,
    /// <summary>
    /// The POST method.
    /// </summary>
    Post = 1,
    /// <summary>
    /// The PUT method.
    /// </summary>
    Put = 2,
    /// <summary>
    /// The DELETE method.
    /// </summary>
    Delete = 3,
    /// <summary>
    /// The PATCH method.
    /// </summary>
    Patch = 4,
    /// <summary>
    /// The HEAD method.
    /// </summary>
    Head = 5,
    /// <summary>
    /// The OPTIONS method.
    /// </summary>
    Options = 6,
}

/// <summary>
/// A HTTP response.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct HttpResponse
{
    private static IReadOnlyDictionary<string, string> Empty = ImmutableDictionary.Create<string, string>();
    private int _status;
    private Optional<HttpKeyValues> _headers;
    /// <summary>
    /// Gets or sets the response body.  This provides access to the raw Wasm Canonical ABI
    /// representation - applications will usually find it move convenient to use BodyAsString
    /// or BodyAsBytes.
    /// </summary>
    public Optional<Buffer> Body;

    /// <summary>
    /// Gets or sets the response HTTP status code.
    /// </summary>
    public HttpStatusCode StatusCode
    {
        get => (HttpStatusCode)_status;
        set => _status = (int)value;
    }

    /// <summary>
    /// Gets or sets the response headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers
    {
        get => _headers.TryGetValue(out var headers) ? headers : Empty;
        set => _headers = value.Count == 0 ? Optional<HttpKeyValues>.None : Optional.From(HttpKeyValues.FromDictionary(value));
    }

    /// <summary>
    /// Gets or sets the response body as a string.
    /// </summary>
    public string? BodyAsString
    {
        get => Body.TryGetValue(out var buffer) ? buffer.ToInteropString().ToString() : null;
        set => Body = value is null ? Optional<Buffer>.None : Optional.From(Buffer.FromString(value));
    }

    /// <summary>
    /// Gets or sets the response body as a sequence of bytes.
    /// </summary>
    public IEnumerable<byte> BodyAsBytes
    {
        get => Body.TryGetValue(out var buffer) ? buffer : Enumerable.Empty<byte>();
        set => Body = value is null ? Optional<Buffer>.None : Optional.From(Buffer.FromBytes(value));
    }
}

/// <summary>
/// A HTTP request.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct HttpRequest
{
    private HttpMethod _method;
    private InteropString _url;
    private HttpKeyValues _headers;
    private HttpKeyValues _parameters_unused;
    private Optional<Buffer> _body;

    /// <summary>
    /// Gets or sets the request method.
    /// </summary>
    public HttpMethod Method
    {
        get => _method;
        set => _method = value;
    }

    /// <summary>
    /// Gets or sets the request URL.
    /// </summary>
    public string Url
    {
        get => _url.ToString();
        set => _url = InteropString.FromString(value);
    }

    /// <summary>
    /// Gets or sets the request headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers
    {
        get => _headers;
        set => _headers = HttpKeyValues.FromDictionary(value);
    }

    /// <summary>
    /// Gets or sets the request body.
    /// </summary>
    public Optional<Buffer> Body
    {
        get => _body;
        set => _body = value;
    }
}

/// <summary>
/// A set of key-value pairs in the Canonical ABI, such as headers or query string parameters.
/// </summary>
/// <inheritdoc />
[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct HttpKeyValues : IReadOnlyDictionary<string, string>
{
    private readonly HttpKeyValue* _valuesPtr;
    private readonly int _valuesLen;

    internal HttpKeyValues(HttpKeyValue* ptr, int length)
    {
        _valuesPtr = ptr;
        _valuesLen = length;
    }

    /// <summary>
    /// Createa a Canonical ABI representation from a .NET dictionary.
    /// </summary>
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

    private Span<HttpKeyValue> AsSpan()
        => new Span<HttpKeyValue>(_valuesPtr, _valuesLen);
    
    // IReadOnlyDictionary

    /// <inheritdoc />
    public bool ContainsKey(string key) => false;
    /// <inheritdoc />
    public bool TryGetValue(string key, out string value)
    {
        foreach (var entry in AsSpan())
        {
            if (entry.Key.ToString() == key)
            {
                value = entry.Value.ToString();
                return true;
            }
        }
        value = String.Empty;
        return false;
    }
    /// <inheritdoc />
    public string this[string key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException(key);
    /// <inheritdoc />
    public IEnumerable<string> Keys => this.Select(kvp => kvp.Key);
    /// <inheritdoc />
    public IEnumerable<string> Values => this.Select(kvp => kvp.Value);
    /// <inheritdoc />
    public int Count => _valuesLen;
    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => new Enumerator(_valuesPtr, _valuesLen);
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    // We can't lazy enumerate by foreach-yielding over a Span because
    // ref struct so POINTER ARITHMETIC AVENGERS ASSEMBLE
    private struct Enumerator : IEnumerator<KeyValuePair<string, string>>
    {
        private HttpKeyValue* _valuesPtr;
        private int _valuesLen;
        private int _index = -1;

        public Enumerator(HttpKeyValue* valuesPtr, int valuesLen)
        {
            _valuesPtr = valuesPtr;
            _valuesLen = valuesLen;
        }

        public KeyValuePair<string, string> Current
        {
            get
            {
                if (_index < 0 || _index >= _valuesLen)
                {
                    throw new InvalidOperationException();
                }
                var ptr = _valuesPtr + _index;
                return KeyValuePair.Create(ptr->Key.ToString(), ptr->Value.ToString());
            }
        }

        public bool MoveNext()
        {
            ++_index;
            return _index < _valuesLen;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        object System.Collections.IEnumerator.Current => Current;
        void IDisposable.Dispose() {}
    }
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct HttpKeyValue
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
    internal static extern unsafe byte wasi_outbound_http_request(ref HttpRequest req, ref HttpResponse ret0);
}
