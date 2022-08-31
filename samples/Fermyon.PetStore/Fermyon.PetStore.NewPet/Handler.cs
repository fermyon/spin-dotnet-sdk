using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

using Fermyon.Spin.Sdk;
using HttpRequest = Fermyon.Spin.Sdk.HttpRequest;
using HttpResponse = Fermyon.Spin.Sdk.HttpResponse;

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
        var ctx = FormParser.ParsePostedForm(request);

        var name = ctx.Request.Form["submitted-name"].ToString();
        var pictureFile = ctx.Request.Form.Files["submitted-picture"];

        if (string.IsNullOrWhiteSpace(name) || pictureFile == null || pictureFile.Length == 0)
        {
            return new HttpResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "text/html" },
                },
                BodyAsString = File.ReadAllText("/assets/NewPetMissingField.html"),
            };
        }

        using var memStream = new MemoryStream((int)(pictureFile.Length));
        pictureFile.CopyTo(memStream);
        var pictureData = memStream.ToArray();

        var connectionString = Configuration.DbConnectionString();

        var maxIdResults = PostgresOutbound.Query(connectionString, "SELECT MAX(id) FROM pets");
        var maxId = maxIdResults.Rows.First()[0].AsNullableInt32();

        var id = (maxId ?? 0) + 1;
        PostgresOutbound.Execute(
            connectionString,
            "INSERT INTO pets (id, name, picture) VALUES ($1, $2, $3)",
            id,
            name,
            pictureData
        );

        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/html" },
            },
            BodyAsString = File.ReadAllText("/assets/NewPetCreated.html"),
        };
    }
}
