using System.Net;
using System.Net.Http;

namespace Fermyon.Spin.Sdk;

public static class OutboundHttp
{
    // TODO: wonder if we can use a HttpClient with custom HttpMessageHandler,
    // or HttpMessageInvoker?  Trouble is those are async and our handler can't
    // be async...
    public static HttpResponse Send(HttpRequest request)
    {
        var resp = new HttpResponse();
        var err = OutboundHttpInterop.wasi_outbound_http_request(ref request, ref resp);

        if (err == 0 || err == 255)
        {
            return resp;
        }
        else
        {
            throw new HttpRequestException($"Outbound error {err}");
        }
    }
}
