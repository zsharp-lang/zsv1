# Functions

## Defining one

`fun` defines a function: its parameters, each with a type, the type it gives, if it gives
anything, and its code.

```zs
import { print } from "std:io";
import { Int32 } from "core:runtime";

fun factorial(n: Int32): Int32 {
    if (n < 2) return 1;
    return n * factorial(n - 1);
}

fun announce(n: Int32) {
    print(n + "! is " + factorial(n));
}

announce(5);
```

```output
5! is 120
```

A function that gives a value returns one on every path. One that gives nothing has no
type after its parameters.

## Named parameters and default values

Parameters in braces are named: a call gives them by name, in any order, and a default
value fills one a call leaves out.

```zs
import { print } from "std:io";
import { String } from "core:runtime";

fun greet(name: String, {greeting: String = "Hello", punctuation: String = "!"}): String {
    return greeting + ", " + name + punctuation;
}

print(greet("Ada"));
print(greet("Grace", punctuation: "."));
print(greet("Linus", greeting: "Hi", punctuation: "?"));
```

```output
Hello, Ada!
Hello, Grace.
Hi, Linus?
```

A parameter can have a name for callers and another for its code, written
`name as alias`:

```zs
import { print } from "std:io";
import { String } from "core:runtime";

fun label({text as shown: String = "none"}): String { return "[" + shown + "]"; }

print(label(text: "ok"));
print(label());
```

```output
[ok]
[none]
```

## Overloads

Functions of one name are overloads of it, and a call calls the one its arguments fit:

```zs
import { print } from "std:io";
import { String, Int32 } from "core:runtime";

fun describe(value: Int32): String { return "the number " + value; }
fun describe(value: String): String { return "the text " + value; }

print(describe(42));
print(describe("forty-two"));
```

```output
the number 42
the text forty-two
```

A call none of them fits is an error, which says why each one doesn't.

## Operators

A function marked `@operator`, from `core:zsharp`, with an operator in parentheses, is one
of the functions the operator stands for, so the operator calls it where its operands fit:

```zs
import { print } from "std:io";
import { operator } from "core:zsharp";
import { Int32 } from "core:runtime";

def Money {
    let cents: Int32;
    new(this, cents: Int32) { this.cents = cents; }
}

@operator((+))
fun add(a: Money, b: Money): Money { return Money(a.cents + b.cents); }

print((Money(150) + Money(275)).cents);
```

```output
425
```

A library's operators are the language's once it's imported.

## Functions as values

A function of a [module](modules.md) goes where a .NET delegate is expected, as a
delegate of it:

```zs
import { print } from "std:io";
import { Int32, Func } from "core:runtime";

module Maths {
    fun square(value: Int32): Int32 { return value * value; }
}

let operation: Func[Int32, Int32] = Maths.square;
print(operation(7));
```

```output
49
```

A function defined in a document, outside any module, can't be a delegate yet, as
[what isn't there yet](not-yet.md) says. A method of an object is a delegate too, bound to
its object, as [classes](classes.md) shows.
