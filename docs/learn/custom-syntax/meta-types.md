# Meta-types

Classes, functions and modules cover most programs. Some domains have definitions of their
own, though: an entity of a database, a message of a protocol, a state of a machine. In
most languages, those are classes with attributes on them, and whatever reads them decodes
the attributes. In Z#, they can be definitions of their own kind, as first-class as a
class.

A kind of definition is a meta-type. A class, a function and a module are each a
definition of a meta-type, and the meta-type is the type of the node that describes it:
`std:oop`'s `Class`, `std:functions`' `Function`, `std:modules`' `Module`. None of them is
built into the compiler. Each is defined by a library, and a library of yours can define
another the same way.

## What a meta-type gives

A meta-type is four parts, each of which a later page or guide goes through:

| Part | What it is | In std:functions |
|---|---|---|
| The node | The definition's structure, in the meta-type's own terms. It is what reflection gives, and what a cast gives. | `Function`, with `Positional` and `Named` for its parameters |
| The meta-compiler | Builds the node from what was written, as code that runs. | `compileFunction` |
| The loader | Loads a node to its CO, and checks the node is valid. | `loadFunction` |
| The CO | What the compiler works with: how it is called, and how it lowers. | `FunctionObject` |

The node and the meta-compiler are the frontend's half, which this part of the guide is
about: [meta-compilers](meta-compilers.md). The loader and the CO give the definition its
meaning, which [custom semantics](../custom-semantics/hir-to-lir.md) is about.

## The node

A node is plain structure: a class with fields, which the toolchain's HIR interfaces
describe, as `Definition` and `Named`. Only what the meta-type's concept has goes in it. A
function has parameters, locals, a return type and a body, and nothing of how a platform
realizes one:

```zs fragment
def Function of Node : Instance, Definition, NamedDefinition, Annotated, Overloadable {
    let name: String;
    let Parameters: List[Definition];
    let Locals: List[Local];
    let ReturnType: Type;
    let Body: Code;
    let annotations: List[Object];
    ...
}
```

A library keeps its nodes in a library of their own, as `std:functions`, apart from what
compiles them, as `std:functions/compiler`. So a program that only reads the structure, as
one that reflects does, loads the nodes and not the compiler.

## Which meta-type a definition is of

A keyword has a default meta-type, which a library makes it: importing `std:oop` makes
`def` a Z# class, and `std:functions` makes `fun` a Z# function. `of` says which
meta-type a definition is of instead:

```zs
import { print } from "std:io";
import { Class } from "std:oop";
import { Class as DotNet_Class } from "pkg:clr/hir";

let zsharp = def Point { } as Class;
let clr = def Counter of DotNet_Class { } as DotNet_Class;
print(zsharp.Name + " is a Z# class");
print(clr.Name + " is a CLR class");
```

```output
Point is a Z# class
Counter is a CLR class
```

`Point` is `def`'s default, a Z# class. `Counter` is a CLR class, the CLR's own meta-type
from `pkg:clr/hir`: the same syntax, built by another meta-compiler into another node,
with another meaning. `of` is decided as the definition is compiled, so it names a
meta-type the code around the definition already has.

## Meta-types and reflection

The node a meta-compiler builds is what describes the definition from then on. Cast to
its meta-type, a definition gives its node as a value, as [declarative code
generation](../advanced/code-generation/declarative.md) does. Built into a library,
the node is what the library carries, so [reflection](../advanced/reflection.md) gives it
back as the program runs, and importing the library gives back the definitions as
themselves.

So a meta-type is also the API of its definitions, for code that generates them and for
code that inspects them: an ORM library's `Entity` meta-type has the entity's columns as
its node's fields, and a program finds its entities by going through its module's
definitions and testing each for `Entity`, with no attributes to decode.
