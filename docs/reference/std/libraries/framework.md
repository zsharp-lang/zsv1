# std:framework

What a library declares it is to what imports it, as decorators of its functions, which
`build(module)` turns into the module's entry point:

| Decorator | What the function is |
|---|---|
| `@entrypoint` | Called when the library is imported. |
| `@metaType(Node)` | The meta-compiler of a kind of definition. |
| `@definitionLoader`, `@instanceLoader`, `@typeLoader` | What loads a kind of node to what the compiler uses. |
| `@binder` | What binds a built node to what it was built as. |
