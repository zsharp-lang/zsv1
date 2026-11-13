# The standard library

The standard library is written in Z#, and its sources ship with every install, in `std`.
A library is imported as `std:name`; one with a part, as `std:project/core`.

## std:prelude

What `zs` imports before a script's first statement, unless it's run with `--no-prelude`,
for what it installs: the standard definitions, so `fun` is
a function, `def` a class and `module` a module, the trait dispatchers, the CLR's
capabilities, reflection, `.dll` imports and `pkg:`. It gives no names, so a script
imports what it uses from each library itself:

```zs
import { print } from "std:io";
import { Interface, static } from "std:oop";
import { Type } from "std:types";
```

## std:io

`print(value)` writes a value, as text, and a line.

```zs
import { print } from "std:io";

print(42);
print("forty-two");
```

```output
42
forty-two
```

## std:oop

Z#'s classes and interfaces, which [Classes](../language/classes.md) and
[Interfaces](../language/interfaces.md) describe. How the library is built, its nodes, COs and meta-compilers, is
in [its README](../../std/oop/README.md).

| Name | What it is |
|---|---|
| `Class` | A class's node, which `def` defines unless it says another meta-type. |
| `Interface` | An interface's node, which `def Name of Interface` defines. |
| `static` | Marks a member as the class's own, not its objects'. |
| `class` | Marks a member as the class's type object's: a class field or a class method. |

## std:types

`Type[T]` is the type of `T`'s value: for a class, the type of its type object, and for
an interface, the type of any class that implements it.

```zs
import { print } from "std:io";
import { Interface, class } from "std:oop";
import { Type } from "std:types";
import { String } from "core:runtime";

def Named of Interface { @class fun title(): String; }

def Book : Named {
    new() { }
    @class fun title(): String { return "a book"; }
}

fun describe(kind: Type[Named]): String { return kind.title(); }

print(describe(Book));
```

```output
a book
```

## std:numbers

Z#'s own numbers, apart from the CLR's: `std:numbers/integer` has `I8`, `I16`, `I32`,
`I64`, `U8`, `U16`, `U32` and `U64`, and `std:numbers/real` has `F16`, `F32` and `F64`, each
with its arithmetic operators and comparisons. A type of them is a definition whose
meta-type says what its numbers are, as `def I32 of FixedBitWidthInteger(32, true) {}`, so
a library can define more. The CLR runs them as its own types of their width, which a
cast goes to and from, with nothing changed:

```zs
import { print } from "std:io";
import { Int32 } from "core:runtime";
import { I32, U8 } from "std:numbers/integer";

let a = 5 as I32;
let b = 7 as I32;
print((a * b) as Int32);
print(a < b);

let small = 200 as U8;
print((small + (100 as U8)) as Int32);
```

```output
35
True
44
```

Nothing converts implicitly yet, so a number is cast to one of them, and back.
A comparison gives the CLR's `Boolean` for now.

## std:reflection

`reflect(value)` gives the node a built definition was built from: what describes it, as
its HIR. It takes what carries the definition's metadata, as a .NET type of a built
library:

```zs
import { print } from "std:io";
import { reflect } from "std:reflection";
import { Class } from "std:oop";
import { Named } from "pkg:zsharp/hir";
import { Type } from "core:runtime";

let built: Type = Class;
let node = reflect(built) as Named;
print(node.Name);
```

```output
Class
```

Reflection is opt-in: a library carries what reflection reads only if it was built to,
as the standard library is. [Reflection](../toolchain/reflection.md) says more.

## std:project

Projects: a folder of sources, built by the module files in it. `discover(folder)` finds
a project, and `Compiler(project).compile()` runs its module files, giving what they
emitted. A module file gets `core:project`, whose `compile()` compiles its folder's
sources as one module, and `emit(value)` adds to what compiling the project gives.
[Projects](../toolchain/projects.md) goes through one.

## std:modules

`Module` and `Global`, a module's node and its globals', which [its README](../../std/modules/README.md)
describes with its compiler. `std:modules/compiler` compiles them:

| Name | What it does |
|---|---|
| `lower(module, target)` | Lowers a module for a platform, as the Cecil platform. |
| `failures(module)` | What lowering it left out, and why. |
| `enter(module)` | Imports a module compiled from its sources, calling its entry point. |

## std:functions

`Function`, `Positional` and `Named`: a function's node and its parameters'. [Its
README](../../std/functions/README.md) describes them with their compiler.
`std:functions/compiler` compiles them, and has `premap(declared, to)`, which makes a
function declared with no code call another, as the prelude makes `reflect` call
`std:encoding`'s `describe`.

## std:packages

What `pkg:` has beyond [the toolchain's own packages](core.md#the-toolchains-packages),
from the package manager. `pkg:name` is a package found in the packages folders: the
project's `packages`, the toolchain's, then the packages NuGet installed. In a folder, a
package is an assembly (`name.dll`), a document (`name.zs`), or a project, and a version
of one, `pkg:name@1.2.3`, is in `name@1.2.3/name.dll`.

## std:encoding

What a built module carries so it can be imported as what it was, and reflected on:
`encode(module)` writes its HIR as a blob, which a platform embeds in what it builds, and
`describe(value)` reads the node of what carries a definition's id.

## std:framework

What a library declares it is to what imports it, as decorators of its functions, which
`build(module)` turns into the module's entry point:

| Decorator | What the function is |
|---|---|
| `@entrypoint` | Called when the library is imported. |
| `@metaType(Node)` | The meta-compiler of a kind of definition. |
| `@definitionLoader`, `@instanceLoader`, `@typeLoader` | What loads a kind of node to what the compiler uses. |
| `@binder` | What binds a built node to what it was built as. |

## std:traits

Traits, and their implementations for types, which are definitions of their own:
`def StringGreeting of Implementation[Greeting, String] { ... }`. `std:traits/static` and
`std:traits/dynamic` are what make a value go where its trait is expected: from its
static type, or from the type it has as the code runs.

## Structures and their compilers

`std:oop`, `std:functions` and `std:modules` are their definitions' structure: their
nodes, which a program that reflects needs alone. What compiles them, their meta-compilers
and what the compiler works with, is `std:oop/compiler`, `std:functions/compiler` and
`std:modules/compiler`. Importing a structure library installs its compiler, so a script
imports only the structure.
