# Decorators and compile time

A definition often needs something done to it that has nothing to do with what it
computes: registering it somewhere, checking it, recording what it is for. Written by
hand, that code lives apart from the definition and drifts away from it. Since a
definition is built by code as it is compiled, Z# lets other code take part right there:
it can look at what is built, change it, or run while it is.

## Decorators

You have already used a few decorators: `@static` and `@class` on a class's members, and
`@operator` on a function. There is nothing special about them. A decorator is any
function, written with `@` above what it decorates. It is given the definition's node as
it is built, and gives the node that is defined: the same one, changed, or another.

```zs
import { print } from "std:io";
import { Function } from "std:functions";
import { Int32 } from "core:runtime";

fun announced(node: Function): Function {
    print("defined " + node.Name + ", which takes " + node.Parameters.Count);
    return node;
}

@announced
fun add(a: Int32, b: Int32): Int32 { return a + b; }

print("calling");
print(add(1, 2));
```

```output
defined add, which takes 2
calling
3
```

`@static` and `@class` work the same way: they are `std:oop`'s functions, which mark a
member's node. Several decorators apply nearest first, and one written with arguments, as
`@operator(Operator.Index)`, is a call that gives the decorator.

## Running code as it is compiled

`@ct`, from `core:zsharp`, runs a statement as the code around it is compiled, and leaves
nothing of it in that code. In a function's body, it runs when the body is built, not
when the function is called:

```zs
import { print } from "std:io";
import { ct } from "core:zsharp";
import { Int32 } from "core:runtime";

fun work(): Int32 {
    @ct print("building work");
    print("running work");
    return 1;
}

print("calling");
print(work() + work());
```

```output
building work
calling
running work
running work
2
```

`ct(code)` runs code given as HIR, and gives its value, as [code
generation](code-generation/declarative.md#using-what-was-built) uses it, and
`load(definition)` loads a definition and gives its runtime object, as a class's type
object.
