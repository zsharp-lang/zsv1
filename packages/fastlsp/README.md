# pkg:fastlsp

A framework for building language servers, in the spirit of FastAPI. An endpoint is a
function decorated with the method it answers; its parameter and return types are the
protocol's own (`pkg:lsp`). The framework reads the JSON, calls it with the typed value,
and writes what it returns.

```zs
// src/.zs
import { compile, emit } from "core:project";
import { request, build } from "pkg:fastlsp";
emit(build(compile("MyServer")));

// src/highlight.zs
@request("textDocument/documentHighlight")
fun highlight(params: DocumentHighlightParams): List[DocumentHighlight] { ... }
```

`build` reads the module's endpoints and adds to it, as HIR: a reader and a writer for each
structure they reach, built from the fields of its class; a function for each endpoint
that reads, calls and writes; and `register(server)`. Nothing in it names a platform. See
`examples/lspserver`, run by `examples/lspdemo.zs` (in memory) and `examples/lspstdio.zs`
(over standard input and output).

- `Server`: JSON-RPC and the lifecycle (`initialize`, `shutdown`, `exit`). `handle(text)`
  answers one message. The capabilities it reports are made from the methods registered.
- `Transport`: `Stdio` (the base protocol's headers) and `MemoryTransport` (for tests).
- `JsonValue`, `parseJson`, `writeJson`: JSON in pure Z#.
- `@request(method)` and `@notification(method)`: annotations, as `std:framework`'s are.

Not yet: a union's value is a `JsonValue` (or a string, number or boolean) both ways, as
Z# has no unions and can't tell a class from an `Object`; a parameter other than the
first is ignored; progress, cancellation and requests the server makes.

## Two packages

`pkg:fastlsp` is the generator: `request`, `notification` and `build`. What a server runs
against, the JSON, `Server`, the transports and the helpers the generated code calls, is
`pkg:fastlsp/runtime` (`packages/fastlsp/runtime`), a library of its own with no building
code. A server's program imports `Server` and a transport from the runtime.
