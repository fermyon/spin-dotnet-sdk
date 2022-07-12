# Work in progress...

### Building

First, grab the prerequisites:

- [.NET 7 Preview 5](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- WIT Bindgen (`make bootstrap`)

Then, build and run the `hello-world` sample:

```
$ cd samples/hello-world
$ dotnet build
$ wasmtime bin/Debug/net7.0/HelloWorld.Spin.wasm 
```

If everything worked, you should see the following error:

```
Error: failed to run main module `bin/Debug/net7.0/HelloWorld.Spin.wasm`

Caused by:
    0: failed to instantiate "bin/Debug/net7.0/HelloWorld.Spin.wasm"
    1: unknown import: `test::test` has not been defined
```

This means the function defined in the WIT file `test.wit` is correctly imported
in .NET and a Wasm import is generated.

Next steps: TODO
