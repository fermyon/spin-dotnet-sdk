# Work in progress...

### Building

First, grab the prerequisites:

- [.NET 7 Preview 5](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- WIT Bindgen (`make bootstrap`)

Then, build and run the `hello-world` sample:

```
$ cd samples/hello-world
$ dotnet build
$ spin up
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
