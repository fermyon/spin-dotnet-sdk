using Fermyon.Spin.Sdk;

namespace Fermyon.PetStore.Common;

public static class GenericResponse
{
    public static HttpResponse NotFound()
    {
        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.NotFound,
        };
    }
}
