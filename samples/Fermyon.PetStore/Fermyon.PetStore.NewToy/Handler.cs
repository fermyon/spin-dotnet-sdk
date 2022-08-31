using Fermyon.Spin.Sdk;
using HttpRequest = Fermyon.Spin.Sdk.HttpRequest;
using HttpResponse = Fermyon.Spin.Sdk.HttpResponse;

using Fermyon.PetStore.Common;

namespace Fermyon.PetStore.NewToy;

public static class Handler
{
    [HttpHandler]
    public static HttpResponse HandleHttpRequest(HttpRequest request)
    {
        return request.Method switch {
            Fermyon.Spin.Sdk.HttpMethod.Get => ShowNewToyForm(request),
            Fermyon.Spin.Sdk.HttpMethod.Post => HandleNewToyForm(request),
            _ => new HttpResponse { StatusCode = System.Net.HttpStatusCode.MethodNotAllowed },
        };
    }

    private static HttpResponse ShowNewToyForm(HttpRequest request)
    { 
        var html = request.IsRuntime() ?
            NewToyFormHtml() :
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

    private static string NewToyFormHtml()
    {
        var connectionString = Configuration.DbConnectionString();

        var rows = PostgresOutbound.Query(connectionString, "SELECT id, name FROM pets ORDER BY name").Rows;
        
        if (rows.Count == 0)
        {
            return File.ReadAllText("/assets/NewToyNoPets.html");
        }

        var options = rows.Select(r => $"<option value=\"{r[0].AsInt32()}\">{r[1].AsString()}</option>");

        var template = File.ReadAllText("/assets/NewToy.html");
        return template.Replace("{{ pet_options }}", string.Join('\n', options));
    }

    private static HttpResponse HandleNewToyForm(HttpRequest request)
    {
        var ctx = FormParser.ParsePostedForm(request);
        
        var description = ctx.Request.Form["submitted-description"].ToString();
        var countText = ctx.Request.Form["submitted-count"].ToString();
        var ownerIdText = ctx.Request.Form["submitted-owner"].ToString();
        var pictureFile = ctx.Request.Form.Files["submitted-picture"];

        if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(ownerIdText) || pictureFile == null || pictureFile.Length == 0)
        {
            return new HttpResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "text/html" },
                },
                BodyAsString = File.ReadAllText("/assets/NewToyMissingField.html"),
            };
        }

        int? count = string.IsNullOrWhiteSpace(countText) ? null : int.Parse(countText);
        var ownerId = int.Parse(ownerIdText);

        using var memStream = new MemoryStream((int)(pictureFile.Length));
        pictureFile.CopyTo(memStream);
        var pictureData = memStream.ToArray();

        var connectionString = Configuration.DbConnectionString();

        var maxIdResults = PostgresOutbound.Query(connectionString, "SELECT MAX(id) FROM toys");
        var maxId = maxIdResults.Rows.First()[0].AsNullableInt32();

        var id = (maxId ?? 0) + 1;
        PostgresOutbound.Execute(
            connectionString,
            "INSERT INTO toys (id, description, count, owner_id, picture) VALUES ($1, $2, $3, $4, $5)",
            id,
            description,
            count,
            ownerId,
            pictureData
        );

        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/html" },
            },
            BodyAsString = File.ReadAllText("/assets/NewToyCreated.html"),
        };
    }
}
