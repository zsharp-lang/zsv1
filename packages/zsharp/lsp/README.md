# pkg:zsharp/lsp

The Z# language server. It answers what an editor asks of Z# code from the code alone: it
parses a document, tolerating what is wrong with it (`Recovery` of `pkg:zsharp/lang/syntax`),
and runs none of it.

| Method | What it answers |
|---|---|
| `textDocument/didOpen`, `didChange`, `didClose` | Keeps the text (the whole of it is sent each time), and publishes a diagnostic for each statement that doesn't parse. |
| `textDocument/documentSymbol` | The outline: imports, definitions, modules, classes with their fields, methods, constructors and properties, and lets. |
| `textDocument/definition` | Every declaration of the name at the position, in the document. |
| `textDocument/hover` | The first line of the name's declaration. |

It is built with `pkg:fastlsp`: each endpoint is a decorated function of the protocol's own
types (`pkg:lsp`), in `src/zendpoints.zs`.

## Running it

```bash
zs packages/zsharp/lsp/serve.zs
```

speaks the protocol over standard input and output, which is what an editor's extension
starts. The extension is a separate project and a client of this program, so nothing in it
needs to know Z#: it launches `zs <toolchain>/packages/zsharp/lsp/serve.zs` for documents
of the language `zsharp`, the files `*.zs`.

```bash
zs packages/zsharp/lsp/test.zs
```

talks to it in memory and prints what it says.

Not yet, and why: see **Language server** in `TASKS.md`.
