using Fermyon.Spin.Sdk;

using Fermyon.PetStore.Common;

namespace Fermyon.PetStore.Pet;

public static class Handler
{
    [HttpHandler]
    public static HttpResponse HandleHttpRequest(HttpRequest request)
    {
        if (!request.IsRuntime())
        {
            return new HttpResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "text/html" },
                },
                BodyAsString = String.Empty,
            };
        }

        var pathElements = request.Headers["spin-path-info"].Split('/', StringSplitOptions.RemoveEmptyEntries);
        return pathElements switch {
            [var id] => PetPage(id),
            [var id, "picture"] => PetPicture(id),
            _ => GenericResponse.NotFound(),
        };
    }

    public static HttpResponse PetPage(string idHeader)
    {
        if (int.TryParse(idHeader, out var id))
        {
            var connectionString = Configuration.DbConnectionString();

            var rows = PostgresOutbound.Query(connectionString, "SELECT id, name FROM pets WHERE id = $1", id).Rows;

            if (rows.Count == 0)
            {
                return PetNotFound();
            }

            var name = rows[0][1].AsString();

            var template = File.ReadAllText("/assets/PetTemplate.html");

            var responseText = template
                .Replace("{{ name }}", name)
                .Replace("{{ id }}", id.ToString());

            return new HttpResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "text/html" },
                },
                BodyAsString = responseText,
            };
        }
        return PetNotFound();
    }

    public static HttpResponse PetPicture(string idHeader)
    {
        if (int.TryParse(idHeader, out var id))
        {
            var connectionString = Configuration.DbConnectionString();

            var rows = PostgresOutbound.Query(connectionString, "SELECT id, picture FROM pets WHERE id = $1", id).Rows;

            if (rows.Count == 0)
            {
                return PetNotFound();
            }

            var picture = rows[0][1].AsBuffer();

            return new HttpResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "image/*" },
                },
                BodyAsBytes = picture,
            };
        }
        return PetNotFound();
    }

    public static HttpResponse PetNotFound()
    {
        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.NotFound,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/html" },
            },
            BodyAsString = File.ReadAllText("/assets/PetNotFound.html"),
        };
    }
}
