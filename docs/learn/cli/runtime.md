# The runtime

The runtime is the platform the `zs` command runs code on: the running .NET process. It realizes LIR
with Reflection.Emit, as a dynamic assembly, as code is compiled, so a script's statements
run as soon as they are lowered.

## What it does

- It realizes core LIR and the CLR's dialect, as any `ClrPlatform`: modules as static
  classes, structures as types, functions as methods, handles as .NET's own types and
  methods, found in the running process.
- It holds what only the process has. A constant of an object, as an exposed value, is
  read where the code reads it, so code built in a document can refer to live objects.
  That is also why what the runtime ran can't be written anywhere else.
- It runs each module's initializer as the type initializer of the class its globals are
  declared in.
- It writes no debugging information: a module Reflection.Emit makes in the running
  process can't have any. The Cecil platform, writing an assembly, can.

## What `core:runtime` gives

`core:runtime` is how Z# code reaches the runtime and the .NET types Z# uses most:

- .NET's common types, as `Object`, `String`, `Int32`, `Boolean`, `List` and
  `Dictionary`, and `Type`, a .NET type object;
- the running platform, and the IL loader, `ilLoader`, which describes .NET's types as
  HIR;
- `import(Assembly)`, which imports an assembly the process has, as `Object.Assembly`, and
  `importBuilt(path)`, which imports an assembly file read from its bytes, so it can be
  built again while it is in use.

The [reference](../../reference/runtime.md) lists every member.

## Code that only runs here

Code that runs as it is compiled, a document's statements and what `ct` runs, runs on the
runtime, so it may use whatever the process has. Code meant to be built for another
platform may not: what it lowers to has to be LIR that platform can write, as [dialects](../custom-platforms/dialects.md)
says.
