# pkg:lsp

The Language Server Protocol as types: what a client and a server say to each other, and
nothing about how either does. It is no framework and knows no language; `pkg:fastlsp`
builds servers on it.

```zs
import { Position, DiagnosticSeverity, find } from "pkg:lsp";
```

- **Structures.** A class for each structure of the protocol, with the properties of what
  it extends and mixes in, since a Z# class has no inheritance. A literal structure the
  protocol writes inline is a class named by what holds it and the property, as
  `ServerCapabilitiesWorkspace`. A property that is a keyword of Z# has a trailing `_`
  (`from_`), and an optional or nullable one is `null` by default (`Nullable[T]` for a
  value type).
- **Unions.** Z# has none yet, so a field of a union is an `Object`, with the union
  written in the comment beside it. A union of one type and `null`, or of one type
  alone, is that type.
- **Enumerations.** A class of constants, as `DiagnosticSeverity.Error`.
- **Methods.** `methods` lists every request and notification as a `MethodInfo`: its name,
  its direction, and its parameter, result and registration option types as the
  specification writes them. `find(name)` gives one.

All but `MethodInfo` and `find` is generated, from the specification's own model,
`metaModel.json` (LSP 3.17), by `zs packages/lsp/generate.zs`, run from the toolchain's
directory. The generated files, under `src/generated`, are checked in.
