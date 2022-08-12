namespace Fermyon.Spin.Sdk;

/// <summary>
/// Marks a method as being the handler for the Spin HTTP trigger.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class HttpHandlerAttribute : Attribute
{
    /// <summary>
    /// Gets or sets whether to send a warmup request during build. The default is true.
    /// The warmup request preloads and prepares the request handler code, meaning the load code does
    /// not have to run on every request.  If false, warmup code is run for the .NET runtime
    /// and Spin SDK, but not the request handler; this results in an increased startup cost for
    /// every request at runtime.
    /// </summary>
    public bool SendWarmupRequest { get; set; } = true;

    /// <summary>
    /// Gets or sets the URL passed to the HTTP handler to indicate that the request is occurring during warmup.
    /// The default is Warmup.DefaultWarmupUrl.
    /// </summary>
    public string WarmupUrl { get; set; } = Warmup.DefaultWarmupUrl;
}

/// <summary>
/// Constants that can be used to check if a call is occurring during warmup.
/// </summary>
public static class Warmup
{
    /// <summary>
    /// The default URL passed to a HTTP handler to indicate that the request is occurring during warmup.
    /// </summary>
    public const string DefaultWarmupUrl = "/warmupz";
}
