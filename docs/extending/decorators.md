# Decorators and compile time

Code that builds a definition runs as the definition is compiled, so Z# code can take
part in compiling: it can change what is built, and run while it is.

## Decorators

A decorator is any function, written before what it decorates with `@`. It is given
the definition's node as it is built, and gives the node that is defined: the same one,
changed, or another.

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

`@static` and `@class` are decorators of this kind: `std:oop`'s functions that mark a
member's node. Decorators apply nearest first, and one that is called with arguments, as
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

`ct(code)` runs code given as HIR, and gives its value, and `load(definition)` gives a
definition's runtime object: what a library that builds definitions from code uses.
