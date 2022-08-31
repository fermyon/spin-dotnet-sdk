// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

using HttpRequest = Fermyon.Spin.Sdk.HttpRequest;
using HttpResponse = Fermyon.Spin.Sdk.HttpResponse;

namespace Fermyon.PetStore.Common;

public static class FormParser
{
    public static HttpContext ParsePostedForm(HttpRequest request)
    {
        var hd = request.Headers.ToDictionary(
            kvp => kvp.Key,
            kvp => new Microsoft.Extensions.Primitives.StringValues(kvp.Value),
            StringComparer.InvariantCultureIgnoreCase
        );
        
        var requestContext = new RequestFeature(
            "POST",
            request.Url,
            new HeaderDictionary(hd),
            new MemoryStream(request.Body.AsBytes().ToArray())
        );
        var requestFeatures = new FeatureCollection();
        requestFeatures[typeof(IHttpRequestFeature)] = requestContext;

        var sp = new NullServiceProvider();
        var contextFactory = new DefaultHttpContextFactory(sp);
        var ctx = contextFactory.Create(requestFeatures);

        return ctx;
    }
}

public class RequestFeature : IHttpRequestFeature
{
    public RequestFeature(string httpMethod, string url, IHeaderDictionary headers, Stream requestBody)
    {
        var queryStartPos = url.IndexOf('?');
        var path = queryStartPos < 0 ? url : url.Substring(0, queryStartPos);
        var query = queryStartPos < 0 ? string.Empty : url.Substring(queryStartPos);

        Method = httpMethod;
        Path = path;
        QueryString = query;
        Headers = headers;
        Body = requestBody;
    }

    public string Protocol { get; set; } = "HTTP/1.1";
    public string Scheme { get; set; } = "http";
    public string Method { get; set; } = "GET";
    public string PathBase { get; set; } = string.Empty;
    public string Path { get; set; } = "/";
    public string QueryString { get; set; } = string.Empty;
    public string RawTarget { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
    public Stream Body { get; set; } = default!;
}

internal class NullServiceScope : Microsoft.Extensions.DependencyInjection.IServiceScope
{
    public NullServiceScope(IServiceProvider sp) { ServiceProvider = sp; }
    public IServiceProvider ServiceProvider { get; init; }
    public void Dispose() {}
}

internal class NullServiceScopeFactory : Microsoft.Extensions.DependencyInjection.IServiceScopeFactory
{
    private readonly IServiceProvider _sp;
    public NullServiceScopeFactory(IServiceProvider sp) { _sp = sp; }

    public Microsoft.Extensions.DependencyInjection.IServiceScope CreateScope() =>
        new NullServiceScope(_sp);
}

internal class NullServiceProvider : IServiceProvider
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
