# Functions

A function names a piece of code, so it can be used again with different values.

## Defining one

`fun` defines a function: its name, its parameters, each with a type, the type it gives
back, if it gives anything, and its code:

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

`factorial` gives an `Int32`, so it returns one on every path. `announce` gives nothing,
so it has no type after its parameters.

## Returning

`return` leaves a function, with a value if the function gives one. It can come anywhere
in the function's code, so a function can leave as soon as it knows its answer:

```zs
import { print } from "std:io";
import { Int32 } from "core:runtime";

fun sign(value: Int32): Int32 {
    if (value < 0) return -1;
    if (value == 0) return 0;
    return 1;
}

print(sign(-5) + sign(0) + sign(9));
```

```output
0
```

A function that gives nothing can use `return;` to leave early.

## Named parameters

A call usually gives its arguments in order. Parameters written in braces are named
instead: a call gives each by its name, in any order, which reads better where a function
takes several values of one type, or options:

```zs
import { print } from "std:io";
import { String } from "core:runtime";

fun send(to: String, {subject: String, body: String}): String {
    return "to " + to + ": " + subject + " / " + body;
}

print(send("Ada", body: "See you at noon.", subject: "Lunch"));
```

```output
to Ada: Lunch / See you at noon.
```

A named parameter is given like any other, unless it has a default value: then a call can
leave it out.

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

A parameter can have one name for callers and another for its own code, written
`name as alias`, so an API reads well from outside while its code stays short:

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

Functions of one name are overloads. A call chooses the one its arguments fit:

```zs
import { print } from "std:io";
import { String, Int32 } from "core:runtime";

fun describe(value: Int32): String { return "the number " + value; }
fun describe(value: String): String { return "the text '" + value + "'"; }

print(describe(7));
print(describe("seven"));
```

```output
the number 7
the text 'seven'
```

If more than one fits and none is more specific than the others, the call is an error.

## Functions as values

A function can be passed around as a value: a function of a [module](modules.md) goes
where a .NET delegate of its signature is expected.

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

Z# has no function type of its own yet, so a function value is a .NET delegate, as
`Func`, and a function defined directly in a script, outside any module, can't be one yet.

Functions can also give operators a meaning for your own types: [operators](operators.md)
shows how, once there are [classes](classes.md) to use them with.
