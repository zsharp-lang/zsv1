# Custom source importers

A string names what to import. Sometimes what you have isn't a name but the thing itself:
an assembly already loaded, a schema already read, a connection already open. Turning it
back into a string to import it would lose it.

An import's source is an expression, and the import is a call of the language's `import`
function with it. So an import from a source that isn't a string is another overload of
`import`: a function marked as one of the functions the syntax `import` stands for.

## Importing from a number

`@operator(SyntaxFunctions("import"))` marks a function as an overload of `import`. This
one imports from a number, and gives a module that says what it counted to:

```zs
import { print } from "std:io";
import { Module } from "std:modules";
import { operator } from "core:zsharp";
import { SyntaxFunctions } from "pkg:zsharp/lang/hir";
import { Instance } from "pkg:zsharp/hir";
import { Int32 } from "core:runtime";

module Counting {
    @operator(SyntaxFunctions("import"))
    fun count(to: Int32): Instance {
        return module Counted { let last = to; } as Module;
    }
}

import { last } from 3;
print(last);
```

```output
3
```

What holds the function, here the module `Counting`, adds it to the overloads of `import`
as it is defined. From then on, an import whose source is an `Int32` calls it, and one of a
string still calls `core:zsharp`'s.

## What a source can be

Overloads are chosen by the source's type, as any call's are, so a source can be any value
an overload takes:

- **An object of the running process**, as `Object.Assembly`: `core:runtime` has an
  `import(Assembly)`, which imports an assembly the process has already loaded, with no
  file to find. That is how `import { Path } from Object.Assembly` reaches .NET's types.
- **A description**: `core:zsharp` has an `import(Instance)`, which gives the instance
  itself, so a node a script built can be imported from like any library.
- **A value of a type of your own**, as a URL, a connection, or a schema someone loaded.

An overload gives what an import gives, an HIR instance whose members are what can be
imported. Since a document's import runs as it is compiled, its source can be a value the
document made in an earlier statement.
