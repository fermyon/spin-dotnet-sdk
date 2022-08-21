using Fermyon.Spin.Sdk;

using Fermyon.PetStore.Common;

namespace Fermyon.PetStore.Pets;

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
        var template = File.ReadAllText("/assets/PetsTemplate.html");
        return template.Replace("{{ db-table }}", DatabaseTableHtml());
    }

    private static string DatabaseTableHtml()
    {
        var connectionString = "user=ivan password=pg314159$ dbname=ivantest host=127.0.0.1";

        var rows = PostgresOutbound.Query(connectionString, "SELECT id, name FROM pets ORDER BY name").Rows;

        if (rows.Count == 0)
        {
            return "<p>No pets registered.</p?";
        }
        else
        {
            var trs = rows.Select(r => $"<tr><td><a href=\"/pet/{r[0].AsInt32()}\">{r[1].AsString()}</a></td></tr>");
            var tbody = String.Join("\n", trs);
            var table = $"<table>\n<th>Name</th>\n{tbody}\n</table>";
            return table;
        }
    }
}
