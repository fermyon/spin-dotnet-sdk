using Fermyon.Spin.Sdk;

using Fermyon.PetStore.Common;

namespace Fermyon.PetStore.Toys;

public static class Handler
{
    [HttpHandler]
    public static HttpResponse HandleHttpRequest(HttpRequest request)
    {
        var html = request.IsRuntime() ?
            PageContent() :
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

    private static string PageContent()
    {
        var template = File.ReadAllText("/assets/ToysTemplate.html");
        return template.Replace("{{ db-table }}", DatabaseTableHtml());
    }

    private static string DatabaseTableHtml()
    {
        var connectionString = Configuration.DbConnectionString();

        var rows = PostgresOutbound.Query(connectionString, "SELECT toys.id, toys.description, toys.count, toys.owner_id, pets.name FROM toys INNER JOIN pets ON pets.id = toys.owner_id ORDER BY toys.description").Rows;

        if (rows.Count == 0)
        {
            return "<p>No toys registered.</p?";
        }
        else
        {
            var trs = rows.Select(r => $"<tr><td><a href=\"/toy/{r[0].AsInt32()}\">{r[1].AsString()}</a></td><td>{CountText(r[2].AsNullableInt32())}</td><td><a href=\"/pet/{r[3].AsInt32()}\">{r[4].AsString()}</a></td></tr>");
            var tbody = String.Join("\n", trs);
            var table = $"<table>\n<th>Toy</th><th>Count</th><th>Owner</th>\n{tbody}\n</table>";
            return table;
        }
    }

    private static string CountText(int? count)
    {
        return count switch {
            null => "Unknown",
            int n => n.ToString(),
        };
    }
}
