# Programmatic code generation

A definition's node is an object like any other, so code can build one by hand, or change
one that a cast built. Declarative and programmatic generation mix freely: the usual way
is to cast what can be written, keep it in a variable, and add what depends on data.

## The nodes

HIR is a set of libraries of nodes:

- `pkg:zsharp/hir` has the core concepts: `Instance`, `Definition`, `Type` and the rest.
- `pkg:zsharp/hir/code` has code: `Get`, `Set`, `NamedMember`, `Call`, literals,
  statements and blocks.
- `pkg:zsharp/lang/hir` has the Z# language's helpers for building code, as
  `Bodies.Call(callee, arguments...)`.
- Each kind of definition has its own: `std:oop` has `Class`, `Field` and `Method`,
  `std:functions` has `Function` and its parameters, and `std:modules` has `Module`.

A node is plain structure. Building one does nothing, and an invalid one is fine until
something loads it, which is where the error is.

## Adding to a cast

This casts an empty function, then adds a statement to its body for each name in a list,
and calls what it built:

```zs
import { print } from "std:io";
import { Module } from "std:modules";
import { Function } from "std:functions";
import { ct } from "core:zsharp";
import { Get, NamedMember, StringLiteral, ExpressionStatement } from "pkg:zsharp/hir/code";
import { Bodies } from "pkg:zsharp/lang/hir";
import { Instance } from "pkg:zsharp/hir";
import { String } from "core:runtime";

let tools = module Tools { } as Module;
let greet = fun greet() { } as Function;

let names: String[] = ["Ada", "Grace"];
let index = 0;
while (index < names.Length) {
    let line = Bodies.Call(Get(print as Instance), StringLiteral("hello, " + names[index]));
    greet.Body.Block.Statements.Add(ExpressionStatement(line));
    index = index + 1;
}
tools.Definitions.Add(greet);

let member = NamedMember();
member.Target = Get(tools);
member.Name = "greet";
ct(Bodies.Call(member));
```

```output
hello, Ada
hello, Grace
```

- `print as Instance` is the node of `print`, and `Get(node)` is the code that refers to
  it.
- `Bodies.Call(callee, arguments...)` is the code of a call.
- `ExpressionStatement(expression)` makes an expression a statement, which the function's
  body takes.

## When to use which

A cast says what is fixed, in Z#'s own syntax, and the compiler builds it. Building by
hand is for what is decided by data: a statement per item of a list, a field per column of
a table, a function per method of a protocol. `pkg:fastlsp` does the second: it reads a
server's endpoints and generates, as HIR, the code that reads and writes each of their
types.

A generator is often a build script's part, as a decorator's or a meta-compiler's is:
[decorators](../decorators.md) change a definition as it is built, and a
[meta-compiler](../../custom-syntax/meta-compilers.md) builds a kind of definition from
what was written.
