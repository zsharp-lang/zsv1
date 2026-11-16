# Runtime

`core:runtime`: the runtime, the platform the CLI runs code on, and the .NET types Z# uses
most. [The runtime](../learn/cli/runtime.md) in the CLI guide says what the runtime does.

## Types

.NET's types, under the names .NET gives them:

| Kind | Types |
|---|---|
| Values | `Object`, `String`, `Boolean`, `Char`, `Byte`, `SByte`, `Int16`, `UInt16`, `Int32`, `UInt32`, `Int64`, `UInt64`, `Single`, `Double` |
| Types | `Type`, `Nullable`, `IDisposable` |
| Collections | `List`, `Dictionary`, `HashSet`, `IEnumerable`, `Enumerable` |
| Delegates | `Func`, with up to three parameters, and `Action`, with up to three |
| The platform | `Runtime` |

The rest of .NET is imported from its assemblies: `import { Path } from Object.Assembly;`.

## Members

| Name | Is |
|---|---|
| `runtime` | The running platform, a `Runtime`, which realizes LIR in the process. |
| `ilLoader` | The IL loader: what describes .NET's types, methods and modules as HIR, and loads them. |
| `import(assembly)` | An overload of `import`: what importing an assembly the process has loaded gives, as `import { Path } from Object.Assembly`. |
| `importBuilt(path)` | What importing the assembly file at the path gives, with the assembly read from its bytes, so the file can be written again while it is in use. What the package manager imports a built package with. |
| `predefined(syntax, operands)` | The CLR's operator for a piece of syntax and the .NET types of its operands, as its method's node: `Int32`'s `+`, or what the first operand's type defines, as `Half`'s `op_Addition`. `null` if the CLR has none. What a platform implements an operator declared on Z#'s types with. |

## Operators

The operators of .NET's types, as functions of the syntax they stand for: `+`, `-`, `*`,
`/`, `==`, `!=`, `<`, `<=`, `>`, `>=`, `&`, `|` and `or`, for the numbers, strings and
truth values .NET defines them for. A string joins with `+` and any value.
