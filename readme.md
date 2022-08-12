# Spin SDK for .NET Preview

An experimental preview SDK for building Spin application components using .NET.

### Features

* Handle HTTP requests using the Spin executor
* Make outbound HTTP requests
* Make outbound Redis calls
* Fast startup by preparing the .NET runtime during Wasm compilation (via Wizer)

### Prerequisites

You'll need the following to build Spin applications using this SDK:

- [.NET 7 Preview 5](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Experimental .NET WASI SDK](https://github.com/steveSandersonMS/dotnet-wasi-sdk/)
- [The WASI SDK](https://github.com/WebAssembly/wasi-sdk)
- This SDK - currently you have to clone it and build from source
- [Rust](https://www.rust-lang.org/tools/install) - needed for `make bootstrap`
- Wizer (`make bootstrap`)

To extend the SDK you also need `wit-bindgen` (which is also installed by `make bootstrap`).

### Building the sample application

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

For example of making Redis requests, see the `UseRedis` method
in the `hello-world` sample.

### Working with Buffers

Both HTTP and Redis represent payload blobs using the `Buffer` type. Buffer represents
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
You can override this in the `HttpHandler` attribute.  If you want to have partial warmup
but not have to process a warmup request, you can set `SendWarmupRequest = false` on the
attribute; however, this is nowhere near as effective as full warmup in reducing startup
times.
