# HIR

`pkg:zsharp/hir`: the core of HIR, the language of structure. It is a handful of
interfaces, which every node of every library implements as it is. HIR's core defines no
node: classes, functions, modules and code are all extensions.

## Concepts

| Interface | Is |
|---|---|
| `Instance` | Anything code can refer to. |
| `Type` | A slot where a type is expected. A type that is also an instance implements both. |
| `Identity` | What is loaded once and kept: loading it again gives the same CO. |
| `Definition` | An `Identity` that is a named slot a container holds: a class, a function, a field, a parameter. |
| `Variable` | What has a value only while code runs: a local, a parameter, the object a method runs on. Code built by code that reads one gets the value it had then. |

## Capabilities

| Interface | Says |
|---|---|
| `Named` | The definition has a name of its own, `Name`. |
| `Annotated` | What has been said of it, `Annotations`, as a decorator says: data only, for whatever reads the definition. |
| `Overloadable` | Definitions of its kind overload by their name. |
| `Encoded` | Of an annotation's type: an encoder writes it with the definition, for an importer to give again. |
| `Kept` | Of an annotation's type: it is kept as the definition lowers, onto what it lowers to, so it is there as the code runs. |
| `Infer` | Not a type: a request for one to be inferred, which whoever loads it does, or fails. |

## Rules

- HIR refers only to HIR. A node may refer to any node, of its own graph or another
  module's.
- A node is structure only: no behavior, and nothing it resolves later.
- An invalid node is fine until it is loaded, where its loader says why.

## Code

`pkg:zsharp/hir/code` is the extension of HIR for code, oriented to Z#:

| Kind | Nodes |
|---|---|
| Containers | `Code`, `Block`, `Local`, `Parameter`, `Capture` |
| Expressions | `Get`, `Set`, `GetValue`, `Reference`, `NamedMember`, `PositionalMember`, `Index`, `Call`, `Argument`, `Cast`, `Match`, `TypeOf`, `BlockExpression`, `ArrayLiteral`, `OverloadGroup`, `Unquote` |
| Literals | `StringLiteral`, `IntegerLiteral`, `DecimalLiteral`, `BooleanLiteral`, `NullLiteral` |
| Statements | `ExpressionStatement`, `IfStatement`, `Loop`, `Return`, `Declare`, `Block` |
| Patterns | `ValuePattern`, `NamePattern` |
| Imports | `Import`, `ImportedMember` |
