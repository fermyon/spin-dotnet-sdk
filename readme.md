# Spin SDK for .NET Preview

An experimental preview SDK for building Spin application components using .NET.

### Features

* Handle HTTP requests using the Spin executor
* Make outbound HTTP requests
* Make outbound Redis calls
* Fast startup by preparing the .NET runtime during Wasm compilation (via Wizer)

### Prerequisites

You'll need the following to build Spin applications using this SDK:

- [Spin](https://spin.fermyon.dev)
- [.NET 7 Preview 5](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- This SDK - currently you have to clone it and build from source
- [Rust](https://www.rust-lang.org/tools/install) - needed for `make bootstrap`
- Wizer (`make bootstrap`)

To extend the SDK you also need `wit-bindgen` (which is also installed by `make bootstrap`).

### Building the "hello world" sample

To build and run the `hello-world` sample:

```
$ cd samples/hello-world
$ spin build && spin up --follow-all
```

If everything worked, you should see a Spin "serving routes" message:

```
Serving http://127.0.0.1:3000
Available Routes:
  hello: http://127.0.0.1:3000 (wildcard)
```

You should be able to curl the address and get a response along the lines of:

```
$ curl -v 127.0.0.1:3000
// outbound trace omitted
< HTTP/1.1 200 OK
< content-type: text/plain
< x-testheader: this is a test
< content-length: 451
< date: Thu, 21 Jul 2022 03:11:15 GMT
<
Called with method Get on URL /
Header 'host' had value '127.0.0.1:3000'
// ... more headers info ...
Header 'spin-component-route' had value ''
The body was empty
```

### Building the Postgres sample

`samples/Fermyon.PetStore` includes a sample (very basic) database-backed
application using Postgres. Before running the sample, you must:

* Create a Postgres database contain the tables in the `sql` directory
* Set the `SPIN_APP_PG_CONN_STR` environment variable to the connection string for the database.
  The connection string is in space-separated format e.g. `user=foo password=bar dbname=test host=127.0.0.1`

### Installing the Spin application template

The SDK includes a Spin template for C# projects.  To install it, run:

```
spin templates install --git https://github.com/fermyon/spin-dotnet-sdk
```

You can then run `spin new http-csharp <project-name>` to create a new Spin C# application.

> Because the SDK currently only supports building from source, you'll be prompted for
> the path to where you cloned the SDK. This must be a relative path from the directory
> _where the application will be created._ For example, if you cloned the SDK
> into `~/git/spin-dotnet-sdk`, and your application is being created in
> `~/work/spin-csharp`, you'd pass `../../git/spin-dotnet-sdk`.

### Handling HTTP requests

Your .NET project should contain a method with the `Fermyon.Spin.Sdk.HttpHandler` attribute.
This method must be `static`, and must take one argument of type `Fermyon.Spin.Sdk.HttpRequest`
and return a `Fermyon.Spin.Sdk.HttpRequest`.

```csharp
using Fermyon.Spin.Sdk;

public static class MyHandler
{
    [HttpHandler]
    public static HttpResponse HandleHttpRequest(HttpRequest request)
    {
        // ...
    }
}
```

Your `spin.toml` file should reference the compiled Wasm file built from the project.

```toml
[[component]]
id = "test"
source = "bin/Release/net7.0/MyApplication.wasm"
[component.trigger]
route = "/..."
```

### Making outbound HTTP requests

To make outbound HTTP requests, use the `HttpOutbound.Send()` method.

For an example of constructing an outbound request, see the `UseOutboundHttp` method
in the `hello-world` sample.

### Making Redis requests

To make outbound Redis requests, use the methods of the `RedisOutbound` class -
`Get`, `Set` and `Publish`.

For examples of making Redis requests, see the `UseRedis` method
in the `hello-world` sample.

### Working with Postgres

To access Postgres databases, use the methods of the `PostgresOutbound` class -
`Query` for statements that return database values (`SELECT`), and `Execute`
for statements that modify the database (`INSERT`, `UPDATE`, `DELETE`).

For examples of making Postgres requests, see the `UsePostgresQuery` and
`UsePostgresExec` methods in the `hello-world` sample, or see the
`Fermyon.PetStore` sample.

### Accessing Spin configuration

To access Spin configuration, use the `SpinConfig.Get()` method.

> It is not expected that a Spin component will try to access config entries
> that don't exist. At the moment, the only way to detect if a config setting
> is missing is to catch the exception from `Get`.

For examples of accessing configuration, see the samples.

### Working with Buffers

Both HTTP and Redis represent payload blobs using the `Buffer` type, and Postgres also
uses `Buffer` for values of `binary` (aka 'blob') type. Buffer represents
an unmanaged span of Wasm linear memory.  The SDK provides several convenience methods
to make it easier to work with.  For example:

* Use `HttpRequest.Body.AsString()` and `HttpRequest.Body.AsBytes()` to read a request
  body as text or a binary blob.
* Use `Buffer.ToUTF8String()` to read an arbitrary Buffer as string.
* Use `HttpResponse.BodyAsString` and `HttpResponse.BodyAsBytes` to set the body of
  a HTTP response.
* Use `Buffer.FromString()` to write a string into a buffer using the UTF-8 encoding.
* Use `Buffer.FromBytes()` to write any sequence of bytes into a buffer.

### Fast startup using Wizer

If your project file (`.csproj`) contains `<UseWizer>true</UseWizer>`, then `dotnet build`
will run [Wizer](https://github.com/bytecodealliance/wizer) to pre-initialise your
Wasm file.  Wizer will run a request through your application _at compile time_ and snapshot
the state of your Wasm module at the end of the request.  This means that the resulting
Wasm module contains the .NET runtime in a state where it is already loaded (and the
interpreter has already seen your code), saving startup time when a request comes in at runtime.

Using Wizer has certain observable impacts:

* You should not (and in some cases cannot) call external services from the warmup request
  handler.  If your handler talks to HTTP or Redis, you _must skip those calls at warmup time_.
  If the warmup code fails, then the build will fail.
* Static constructors and static member initialisation happens at warmup time (at least for
  any type used on the warmup path).  For example, if your handler type has a static
  `Random` member, and this gets initialised during warmup, then _the same state of the random
  number generator_ will be used in all requests!

You can identify if a request is the warmup request because the URL will be `/warmupz`.
You can override this in the `HttpHandler` attribute.  However, it is not currently possible
to have Wizer initialise the runtime but omit calling your handler.

If your handler logic doesn't require any external services then you don't need any special
warmup handling.  Otherwise, you'll need to guard those calls, or skip your real handler
altogether, e.g.:

```csharp
[HttpHandler]
public static HttpResponse HandleHttpRequest(HttpRequest request)
{
    if (request.Url == Warmup.DefaultWarmupUrl)
    {
        return new HttpResponse
        {
            StatusCode = HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/plain" },
            },
            BodyAsString = "warmup",
        };
    }

    // ... real handler goes here ...
}
```

## Known issues

The Spin .NET SDK is a preview, built on an implementation of .NET that is currently experimental.
There are several known issues, of which the most severe are:

* Some static methods and properties cause a "indirect call type mismatch" error when Wizer is turned
  on - we have seen this on numeric parse methods and `StringComparer` properties.
  You can work around this by turning Wizer off for affected modules.
* In some cases, unhandled exceptionc also cause "indirect call type mismatch" instead of being
  returned as 500 Internal Server Error responses. You can work around this by catching problematic
  exceptions and returning error responses manually.

You can track issues or report problems at https://github.com/fermyon/spin-dotnet-sdk/issues.

## What's next

The initial version of the SDK closely mirrors the underlying low-level Spin interop interfaces.
This maximises performance but doesn't provide a very idiomatic experience for .NET developers.
We'll be aiming to improve that over future releases, and welcome contributions or suggestions!
