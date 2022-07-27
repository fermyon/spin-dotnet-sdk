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
public ref struct WitResponse
{
    public int Status;
    public WitOptional<WitKeyValues> Headers;
    public WitOptional<WitBuffer> Body;
}

[StructLayout(LayoutKind.Sequential)]
public readonly ref struct WitRequest
{
    public readonly HttpMethod Method;
    public readonly WitString Url;
    public readonly WitKeyValues Headers;
    public readonly WitKeyValues Parameters;
    public readonly WitOptional<WitBuffer> Body;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct WitBuffer
{
    public byte* _ptr;
    public int _length;

    public ReadOnlySpan<byte> AsSpan() => new ReadOnlySpan<byte>(_ptr, _length);

    public static WitBuffer StringAsUtf8(string value)
    {
        var exactByteCount = checked(Encoding.UTF8.GetByteCount(value));
        var mem = (byte*)Marshal.AllocHGlobal(exactByteCount);
        var buffer = new Span<byte>(mem, exactByteCount);
        int byteCount = Encoding.UTF8.GetBytes(value, buffer);
        return new WitBuffer { _ptr = mem, _length = byteCount };
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct WitOptional<T>
{
    internal byte _isSome;
    internal T _value;

    public bool TryGetValue(out T value)
    {
        value = _value;
        return _isSome != 0;
    }

    public WitOptional<T> None => default;
}

public static class WitOptional
{
    public static WitOptional<T> From<T>(T value) => new WitOptional<T> { _isSome = 1, _value = value };
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct WitString
{
    byte* _utf8Ptr;
    int _utf8Length;

    public override string ToString()
        => Marshal.PtrToStringUTF8((nint)_utf8Ptr, _utf8Length);

    public static WitString FromString(string value)
    {
        var buffer = WitBuffer.StringAsUtf8(value);
        return new WitString { _utf8Ptr = buffer._ptr, _utf8Length = buffer._length };
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct WitKeyValues
{
    public WitKeyValue* _valuesPtr;
    public int _valuesLen;

    public static WitKeyValues FromDictionary(Dictionary<string, string> dictionary)
    {
        var unmanagedValues = (WitKeyValue*)Marshal.AllocHGlobal(dictionary.Count * sizeof(WitKeyValue));
        var currentPtr = unmanagedValues;
        foreach (var (key, value) in dictionary)
        {
            currentPtr->Key = WitString.FromString(key);
            currentPtr->Value = WitString.FromString(value);
            currentPtr++;
        }
        return new WitKeyValues { _valuesPtr = unmanagedValues, _valuesLen = dictionary.Count };
    }

    public IEnumerable<KeyValuePair<string, string>> ToEnumerable()
    {
        var result = new List<KeyValuePair<string, string>>();
        WitKeyValue* entry = _valuesPtr;
        for (var i = 0; i < _valuesLen; i++)
        {
            result.Add(KeyValuePair.Create(entry->Key.ToString(), entry->Value.ToString()));
            entry++;
        }

        return result;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct WitKeyValue
{
    public WitString Key;
    public WitString Value;
}
