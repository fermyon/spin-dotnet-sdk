using Fermyon.Spin.Sdk;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace Fermyon.Spin.HelloWorld;

public static class Handler
{
    [HttpHandler]
    public static unsafe void HandleHttpRequest(WitRequest* witRequest, WitResponse* witResponse)
    {
        var requestInfo = $"Called with method {witRequest->Method}, Url {witRequest->Url}";

        var headerInfo = String.Join("\n", witRequest->Headers.ToEnumerable().Select(p => $"Header '{p.Key}' had value '{p.Value}'"));
        var parameterInfo = String.Join("\n", witRequest->Parameters.ToEnumerable().Select(p => $"Parameter '{p.Key}' had value '{p.Value}'"));

        var bodyInfo = witRequest->Body.Value is WitBuffer bodyBuffer ?
            $"The body (as a string) was: {Encoding.UTF8.GetString(bodyBuffer.AsSpan())}\n" :
            "The body was empty\n";

        var responseBody = String.Join("\n", new[] { requestInfo, headerInfo, parameterInfo, bodyInfo });
        witResponse->Status = 201;
        witResponse->Body = new WitOptionalBuffer(WitBuffer.StringAsUtf8(responseBody));
        /*
        return new WitResponse
        {
            Status = 201,
            Headers = new Dictionary<string, string> {
                { "Content-Type", "text/plain" },
                { "X-TestHeader", "this is a test" },
            },
            Body = Encoding.UTF8.GetBytes(responseBody),
        };
        */
    }
}

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
public struct WitResponse
{
    public int Status;
    public byte HasHeaders;
    public WitKeyValues Headers;
    public WitOptionalBuffer Body;
}

[StructLayout(LayoutKind.Sequential)]
public struct WitRequest
{
    public HttpMethod Method;
    public WitString Url;
    public WitKeyValues Headers;
    public WitKeyValues Parameters;
    public WitOptionalBuffer Body;
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
public struct WitOptionalBuffer
{
    private byte _isSome;
    private WitBuffer _value;

    public WitBuffer? Value => _isSome == 0 ? default : _value;

    public WitOptionalBuffer(WitBuffer? Value)
    {
        _isSome = Value.HasValue ? (byte)1 : (byte)0;
        _value = Value.GetValueOrDefault();
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct WitString
{
    byte* _utf8Ptr;
    int _utf8Length;

    public override string ToString()
        => Marshal.PtrToStringUTF8((nint)_utf8Ptr, _utf8Length);
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct WitKeyValues
{
    public WitKeyValue* _valuesPtr;
    public int _valuesLen;

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
