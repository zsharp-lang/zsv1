# Custom nodes and loaders

The importers on the pages before this one describe what they import with HIR Z# already
has: a module of globals. That works when what is imported is like a Z# module. When it
isn't, as a database schema, a protocol's messages or another language's definitions, an
importer describes it in its own terms, with nodes of its own, and a loader gives those
nodes their meaning.

## Nodes of your own

A node is a class that implements the HIR concepts it is: an `Instance` that code can
refer to, a `Definition` that can be reflected on, `Named` if it has a name. Nothing else
is asked of it. A table of a schema is its columns, and a column its name and type:

```zs fragment
import { Instance, Definition, Named } from "pkg:zsharp/hir";
import { Class as DotNet_Class, public, explicitThis } from "pkg:clr/hir";
import { String, List } from "core:runtime";

@public def Column of DotNet_Class : Instance, Definition, Named {
    @public let name: String;
    @public let Type: String;
    @public @explicitThis get Name(this): String { return this.name; }
    ...
}

@public def Table of DotNet_Class : Instance, Definition, Named {
    @public let name: String;
    @public let Columns: List[Column];
    ...
}
```

A node holds structure only: no behavior, and nothing it computes later. That is what
makes it the same thing to a build script that generates code from it, to a program that
reflects on it, and to the compiler.

The nodes are a library of their own, apart from what loads them, so code that only reads
the structure needs only the nodes.

## A loader

A loader is given a node, and gives its CO: the compiler object that says what the node
does when code uses it. It is also where a node is checked, since a node can be built as
anything and is only known to be valid once it is loaded.

`std:framework`'s decorators say what a function of a library is, and the library's entry
point registers it:

```zs fragment
import { definitionLoader } from "std:framework";

@definitionLoader
fun loadTable(node: Table): IResult[CompilerObject, Error] {
    if (node.Columns.Count == 0) return Result[CompilerObject].Error("A table needs a column.");
    return Result[CompilerObject].Ok(TableObject(node));
}
```

- `@definitionLoader` loads a `Definition`, which is loaded once and kept, so one node has
  one CO.
- `@instanceLoader` and `@typeLoader` load a node where code uses it as an instance or as
  a type.

The loaders are registered with the HIR loader by the type of node they take. A node with
no loader is an error where it is used, not where it was imported.

## The CO

The CO is the semantics: what a member of a table is, what calling it does, what it lowers
to. A `TableObject` could give each column as a member, a query as a call, and lower to
whatever a platform keeps rows in. Writing one is [custom
semantics](../custom-semantics/compiler-objects.md).

So an importer of a new kind of thing is three libraries' worth of parts, each of which
can be used without the others: the importer, which gives nodes; the nodes, which describe;
and the loader with its COs, which give the nodes meaning.
