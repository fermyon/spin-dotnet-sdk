using Fermyon.Spin.Sdk;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;

namespace Fermyon.Spin.HelloWorld;

public static class Handler
{
    [HttpHandler]
    public static unsafe HttpResponse HandleHttpRequest(WitRequest* witRequest)
    {
        var requestInfo = $"Called with method {witRequest->Method}, Url {witRequest->Url}";

        var headerInfo = String.Join("\n", witRequest->Headers.ToEnumerable().Select(p => $"Header '{p.Key}' had value '{p.Value}'"));
        /*
        var bodyInfo = r.Body.Length > 0 ?
            $"The body (as a string) was: {System.Text.Encoding.UTF8.GetString(r.Body)}\n" :
            "The body was empty\n";
        */
        var responseBody = String.Join("\n", new[] { requestInfo, headerInfo });

        return new HttpResponse {
            Status = 200,
            Headers = new Dictionary<string, string> {
                { "Content-Type", "text/plain" },
                { "X-TestHeader", "this is a test" },
            },
            Body = System.Text.Encoding.UTF8.GetBytes(responseBody),
        };
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
public struct WitRequest
{
    public HttpMethod Method;
    public WitString Url;
    public WitKeyValues Headers;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct WitString
{
    byte* _utf8Ptr;
    int _utf8Length;

    public override string ToString()
        => Encoding.UTF8.GetString(_utf8Ptr, _utf8Length);
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
