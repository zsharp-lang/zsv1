# Declarative code generation

Every definition is built by code. Written as a statement, a definition is built and
loaded where it is, and its name is bound to what it defines. Cast to its meta-type, it is
built and not loaded: the cast gives the definition's node, its HIR, as a value. That is
code generation written as declarations.

## Casting a definition

`def`, `module` and `fun` can each be written as an expression and cast to what it is:

```zs
import { print } from "std:io";
import { Class } from "std:oop";
import { Module } from "std:modules";
import { Function } from "std:functions";
import { Int32 } from "core:runtime";

let point = def Point { let x: Int32 = 0; let y: Int32 = 0; } as Class;
let tools = module Tools { let version = 1; } as Module;
let twice = fun twice(value: Int32): Int32 { return value * 2; } as Function;

print(point.Name + " has " + point.Fields.Count + " fields");
print(tools.Name);
print(twice.Name + " takes " + twice.Parameters.Count);
```

```output
Point has 2 fields
Tools
twice takes 1
```

Each value is a node: `Class`, `Module` and `Function` are the meta-types of `def`,
`module` and `fun`, and a node of one has what that meta-type describes, as a class's
`Fields` or a function's `Parameters`. A cast definition binds no name outside itself:
`Point` names nothing after the cast. It is structure, with no identity of its own until
something loads it.

## When it is built

A cast definition is built when the code it is in runs, not when that code is compiled.
In a function, each call builds a new one, from the values the function has then:

```zs
import { print } from "std:io";
import { Module } from "std:modules";
import { String } from "core:runtime";

fun make(name: String): Module {
    return module Made { let greeting = "hello, " + name; } as Module;
}

let first = make("Ada");
let second = make("Grace");
print(first.Name);
print(first == second);
```

```output
Made
False
```

Building code that builds code is reification: the compiler turns the definition into the
code that builds it, and puts that code where the cast is. A variable of the code around
it, as `name` here, is read as the definition is built, so the definition holds its value
from then on.

## Using what was built

A node is used as any HIR is: added to a definition that is used, or loaded. Here a
function is built and added to a module, which a call then reaches:

```zs
import { print } from "std:io";
import { Module } from "std:modules";
import { Function } from "std:functions";
import { ct } from "core:zsharp";
import { Get, NamedMember, IntegerLiteral } from "pkg:zsharp/hir/code";
import { Bodies } from "pkg:zsharp/lang/hir";
import { Int32 } from "core:runtime";

let tools = module Tools { } as Module;
tools.Definitions.Add(fun twice(value: Int32): Int32 { return value * 2; } as Function);

let member = NamedMember();
member.Target = Get(tools);
member.Name = "twice";
print(ct(Bodies.Call(member, IntegerLiteral(21))));
```

```output
42
```

`ct(code)` runs code given as HIR as it is compiled, and gives its value. The call is
HIR too, built by hand: [programmatic code generation](programmatic.md) is that.

A build script does the same with what it builds, but for a platform: it lowers the module
and writes it, as [compiling projects](../build-scripts/compiling-projects.md) does.

## What it may refer to

A cast definition's code can use what any code around it can: its imports, the
definitions around it, and the variables of the code that builds it, which it holds the
values of. A cast definition inside a function that is itself written to an assembly is
code that builds code, run in another process: everything it holds then has to be
something the platform writing it can write. [Custom platforms](../../custom-platforms/overview.md)
says what that means.
