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

    private class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type t)
        {
            if (t == typeof(Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Features.FormOptions>))
            {
                return new Microsoft.Extensions.Options.OptionsWrapper<Microsoft.AspNetCore.Http.Features.FormOptions>(new Microsoft.AspNetCore.Http.Features.FormOptions());
            }
            if (t == typeof(Microsoft.Extensions.DependencyInjection.IServiceScopeFactory))
            {
                return new NullServiceScopeFactory(this);
            }
            return null;
        }
    }

    private class NullServiceScope : Microsoft.Extensions.DependencyInjection.IServiceScope
    {
        public NullServiceScope(IServiceProvider sp) { ServiceProvider = sp; }
        public IServiceProvider ServiceProvider { get; init; }
        public void Dispose() {}
    }

    private class NullServiceScopeFactory : Microsoft.Extensions.DependencyInjection.IServiceScopeFactory
    {
        private readonly IServiceProvider _sp;
        public NullServiceScopeFactory(IServiceProvider sp) { _sp = sp; }

        public Microsoft.Extensions.DependencyInjection.IServiceScope CreateScope() =>
            new NullServiceScope(_sp);
    }

    private static HttpResponse HandleNewPetForm(HttpRequest request)
    {
        var hd = request.Headers.ToDictionary(
            kvp => kvp.Key,
            kvp => new Microsoft.Extensions.Primitives.StringValues(kvp.Value),
            StringComparer.InvariantCultureIgnoreCase
        );
        
        var requestContext = new WasiServerRequestContext(
            "POST",
            request.Url,
            new HeaderDictionary(hd),
            new MemoryStream(request.Body.AsBytes().ToArray())
        );
        var requestFeatures = new FeatureCollection();
        requestFeatures[typeof(IHttpRequestFeature)] = requestContext;

        var sp = new NullServiceProvider();
        var cf = new Microsoft.AspNetCore.Http.DefaultHttpContextFactory(sp);
        var ctx = cf.Create(requestFeatures);

        // var m = ctx.Request.Method;
        // var p = ctx.Request.Path;
        // var fcc = ctx.Request.Form.Count;
        // var fcfc = ctx.Request.Form.Files.Count;
        // var s = $"Meth {m} Path {p} Fcount {fcc} Ffilecount {fcfc}";

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

        var connectionString = "user=ivan password=pg314159$ dbname=ivantest host=127.0.0.1";

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

internal static class Kludge
{
    public static Dictionary<TKey, TElement>? KludgeyKludge<TSource, TKey, TElement>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector,
        IEqualityComparer<TKey>? comparer
    ) where TKey : notnull
    {
        foreach (TSource element in source)
        {
        }

        return null;
    }
}
