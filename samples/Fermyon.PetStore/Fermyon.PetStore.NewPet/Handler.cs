using System.Net.Http.Headers;

using Fermyon.Spin.Sdk;

using Fermyon.PetStore.Common;

namespace Fermyon.PetStore.NewPet;

public static class Handler
{
    [HttpHandler]
    public static HttpResponse HandleHttpRequest(HttpRequest request)
    {
        return request.Method switch {
            Fermyon.Spin.Sdk.HttpMethod.Get => ShowNewPetForm(request),
            Fermyon.Spin.Sdk.HttpMethod.Post => HandleNewPetForm(request),
            _ => new HttpResponse { StatusCode = System.Net.HttpStatusCode.MethodNotAllowed },
        };
    }

    private static HttpResponse ShowNewPetForm(HttpRequest request)
    { 
        var html = request.IsRuntime() ?
            File.ReadAllText("/assets/NewPet.html") :
            String.Empty;

        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/html" },
            },
            BodyAsString = html,
        };
    }

    private static HttpResponse HandleNewPetForm(HttpRequest request)
    {
        var streamContent = new StreamContent(data);
        // var streamContent = new ByteArrayContent(request.Body.AsBytes().ToArray());
        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(request.Headers["content-type"]);

        var provider = streamContent.ReadAsMultipartAsync();

        foreach (var httpContent in provider.Contents)
        {
            var fileName = httpContent.Headers.ContentDisposition.FileName;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                continue;
            }
            using (Stream fileContents = await httpContent.ReadAsStreamAsync())
            {
                fileProcessor(fileName, fileContents);
            }
        }

        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/html" },
            },
            BodyAsString = request.Body.AsString(),
        };
    }
}
