using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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
    public Optional<HttpBuffer> Body;

    public string? BodyAsString
    {
        get => Body.TryGetValue(out var buffer) ? buffer.ToHttpString().ToString() : null;
        set => Body = value is null ? Optional<HttpBuffer>.None : Optional.From(HttpBuffer.FromString(value));
    }
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct HttpRequest
{
    public readonly HttpMethod Method;
    public readonly HttpString Url;
    public readonly HttpKeyValues Headers;
    public readonly HttpKeyValues Parameters;
    public readonly Optional<HttpBuffer> Body;
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct HttpBuffer
{
    private readonly nint _ptr;
    private readonly int _length;

    private HttpBuffer(nint ptr, int length)
    {
        _ptr = ptr;
        _length = length;
    }

    public unsafe ReadOnlySpan<byte> AsSpan() => new ReadOnlySpan<byte>((void*)_ptr, _length);

    public static unsafe HttpBuffer FromString(string value)
    {
        var httpString = HttpString.FromString(value);
        return new HttpBuffer(httpString._utf8Ptr, httpString._utf8Length);
    }

    internal HttpString ToHttpString()
        => new HttpString(_ptr, _length);
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct Optional<T>
{
    private readonly byte _isSome;
    private readonly T _value;

    internal Optional(T value)
    {
        _isSome = 1;
        _value = value;
    }

    public bool TryGetValue(out T value)
    {
        value = _value;
        return _isSome != 0;
    }

    public static readonly Optional<T> None = default;
}

public static class Optional
{
    // Just so the caller doesn't have to specify <T>
    public static Optional<T> From<T>(T value) => new Optional<T>(value);
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct HttpString
{
    internal readonly nint _utf8Ptr;
    internal readonly int _utf8Length;

    internal HttpString(nint ptr, int length)
    {
        _utf8Ptr = ptr;
        _utf8Length = length;
    }

    public override string ToString()
        => Marshal.PtrToStringUTF8(_utf8Ptr, _utf8Length);

    public static unsafe HttpString FromString(string value)
    {
        var exactByteCount = checked(Encoding.UTF8.GetByteCount(value));
        var mem = Marshal.AllocHGlobal(exactByteCount);
        var buffer = new Span<byte>((void*)mem, exactByteCount);
        int byteCount = Encoding.UTF8.GetBytes(value, buffer);
        return new HttpString(mem, byteCount);
    }
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
            span[index] = new HttpKeyValue(HttpString.FromString(key), HttpString.FromString(value));
            index++;
        }
        return new HttpKeyValues(unmanagedValues, dictionary.Count);
    }

    public Span<HttpKeyValue> AsSpan()
        => new Span<HttpKeyValue>(_valuesPtr, _valuesLen);
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct HttpKeyValue
{
    public readonly HttpString Key;
    public readonly HttpString Value;

    internal HttpKeyValue(HttpString key, HttpString value)
    {
        Key = key;
        Value = value;
    }
}
