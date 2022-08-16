using Fermyon.Spin.Sdk;

namespace Fermyon.PetStore.Common;

public static class HttpRequestExtensions
{
    public static bool IsRuntime(this HttpRequest request)
    {
        return request.Url != Warmup.DefaultWarmupUrl;
    }
}
