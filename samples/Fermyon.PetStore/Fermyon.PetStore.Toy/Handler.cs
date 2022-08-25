using Fermyon.Spin.Sdk;

using Fermyon.PetStore.Common;

namespace Fermyon.PetStore.Toy;

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
            [var id] => ToyPage(id),
            [var id, "picture"] => ToyPicture(id),
            _ => GenericResponse.NotFound(),
        };
    }

    public static HttpResponse ToyPage(string idHeader)
    {
        if (int.TryParse(idHeader, out var id))
        {
            var connectionString = Configuration.DbConnectionString();

            var rows = PostgresOutbound.Query(connectionString, "SELECT toys.id, toys.description, toys.count, toys.owner_id, pets.name FROM toys INNER JOIN pets ON pets.id = toys.owner_id WHERE toys.id = $1", id).Rows;

            if (rows.Count == 0)
            {
                return ToyNotFound();
            }

            var toyId = rows[0][0].AsInt32();
            var description = rows[0][1].AsString();
            var count = rows[0][2].AsNullableInt32();
            var ownerId = rows[0][3].AsInt32();
            var ownerName = rows[0][4].AsString();

            var countText = count switch {
                null => "an unknown number",
                int n => n.ToString(),
            };

            var template = File.ReadAllText("/assets/ToyTemplate.html");

            var responseText = template
                .Replace("{{ description }}", description)
                .Replace("{{ count }}", countText)
                .Replace("{{ toy_id }}", toyId.ToString())
                .Replace("{{ owner_id }}", ownerId.ToString())
                .Replace("{{ owner_name }}", ownerName);

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
        return ToyNotFound();
    }

    public static HttpResponse ToyPicture(string idHeader)
    {
        if (int.TryParse(idHeader, out var id))
        {
            var connectionString = Configuration.DbConnectionString();

            var rows = PostgresOutbound.Query(connectionString, "SELECT id, picture FROM toys WHERE id = $1", id).Rows;

            if (rows.Count == 0)
            {
                return ToyNotFound();
            }

            var picture = rows[0][1].AsNullableBuffer().GetValueOrDefault();

            if (picture.Length == 0)
            {
                return MysteryToyPicture();
            }

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
        return ToyNotFound();
    }

    public static HttpResponse ToyNotFound()
    {
        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.NotFound,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/html" },
            },
            BodyAsString = File.ReadAllText("/assets/ToyNotFound.html"),
        };
    }

    public static HttpResponse MysteryToyPicture()
    {
        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.NotFound,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "image/png" },
            },
            BodyAsBytes = File.ReadAllBytes("/assets/mystery-toy.png"),
        };
    }
}
