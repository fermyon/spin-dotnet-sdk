using System.Collections.Immutable;
using System.Net;
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
    private static IReadOnlyDictionary<string, string> Empty = ImmutableDictionary.Create<string, string>();
    private int _status;
    public Optional<HttpKeyValues> _headers;
    public Optional<Buffer> Body;

    public HttpStatusCode StatusCode
    {
        get => (HttpStatusCode)_status;
        set => _status = (int)value;
    }

    public IReadOnlyDictionary<string, string> Headers
    {
        get => _headers.TryGetValue(out var headers) ? headers : Empty;
        set => _headers = value.Count == 0 ? Optional<HttpKeyValues>.None : Optional.From(HttpKeyValues.FromDictionary(value));
    }

    public string? BodyAsString
    {
        get => Body.TryGetValue(out var buffer) ? buffer.ToInteropString().ToString() : null;
        set => Body = value is null ? Optional<Buffer>.None : Optional.From(Buffer.FromString(value));
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct HttpRequest
{
    private HttpMethod _method;
    private InteropString _url;
    private HttpKeyValues _headers;
    private HttpKeyValues _parameters;
    private Optional<Buffer> _body;

    public HttpMethod Method
    {
        get => _method;
        set => _method = value;
    }

    public string Url
    {
        get => _url.ToString();
        set => _url = InteropString.FromString(value);
    }

    public IReadOnlyDictionary<string, string> Headers
    {
        get => _headers;
        set => _headers = HttpKeyValues.FromDictionary(value);
    }

    public IReadOnlyDictionary<string, string> Parameters
    {
        get => _parameters;
        set => _parameters = HttpKeyValues.FromDictionary(value);
    }

    public Optional<Buffer> Body
    {
        get => _body;
        set => _body = value;
    }
}

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

    public Span<HttpKeyValue> AsSpan()
        => new Span<HttpKeyValue>(_valuesPtr, _valuesLen);
    
    // IReadOnlyDictionary
    public bool ContainsKey(string key) => false;
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
    public string this[string key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException(key);
    public IEnumerable<string> Keys => this.Select(kvp => kvp.Key);
    public IEnumerable<string> Values => this.Select(kvp => kvp.Value);
    public int Count => _valuesLen;
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
    internal static extern unsafe byte wasi_outbound_http_request(ref HttpRequest req, ref HttpResponse ret0);
}
