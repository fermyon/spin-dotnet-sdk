namespace Fermyon.Spin.Sdk;

/// <summary>
/// Marks a method as being the handler for the Spin HTTP trigger.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class HttpHandlerAttribute : Attribute {}
