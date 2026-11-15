# Operators

`+` adds numbers and joins strings because those are the functions `+` stands for, for
those types. An operator in Z# is a call: of the functions its syntax stands for, chosen
by the types of its operands, as an overload is. So a type of your own can have operators
by adding a function to them.

## Defining one

`@operator`, from `core:zsharp`, with the operator in parentheses, makes a function one of
the functions that operator stands for:

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

`(+)` is the operator itself, written as a value. `Money(150) + Money(275)` finds the `+`
whose parameters fit two `Money`s, which is `add`.

## Where they apply

An operator defined in a library is the language's once the library is imported: its
importers get it with the library's types. Z#'s own number and truth types in the
standard library are defined this way, with nothing in the compiler that knows what their
`+` or `==` is.

Indexing, `value[index]`, is an operation of its own, which a class defines with a method,
as [more on classes](more-classes.md#indexing) shows.
