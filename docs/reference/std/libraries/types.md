# std:types

`Type[T]` is the type of `T`'s value: for a class, the type of its type object, and for
an interface, the type of any class that implements it.

```zs
import { print } from "std:io";
import { Interface, class } from "std:oop";
import { Type } from "std:types";
import { String } from "core:runtime";

def Named of Interface {
    @class
    fun title(): String;
}

def Book : Named {
    new() { }

    @class
    fun title(): String { return "a book"; }
}

fun describe(kind: Type[Named]): String { return kind.title(); }

print(describe(Book));
```

```output
a book
```
