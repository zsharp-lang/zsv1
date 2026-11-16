# More on classes

[Classes](classes.md) covered what most classes have: fields, constructors and methods,
and the class as a value. This page goes through the rest, each for when a class needs it.

## Named constructors

A constructor with a name is a member of the class. The unnamed one is what calling the
class runs:

```zs
import { print } from "std:io";
import { Int32 } from "core:runtime";

def Counter {
    let count: Int32 = 0;

    new() { }

    new from(this, start: Int32) { this.count = start; }

    fun next(): Int32 {
        count = count + 1;
        return count;
    }
}

print(Counter().next());
print(Counter.from(41).next());
```

```output
1
42
```

## Static and class members

A member marked `@static` is the class's own, not its objects': a static field is one
variable, and a static method is a function with no object. A member marked `@class`
belongs to the class's type object: a class is a value too, the one object of its
metaclass, and its class methods run on it.

```zs
import { print } from "std:io";
import { static, class } from "std:oop";
import { String, Int32 } from "core:runtime";

def Ticket {
    @static
    let issued: Int32 = 0;

    let number: Int32;

    new(this) { issued = issued + 1; this.number = issued; }

    @static
    fun count(): Int32 { return issued; }

    @class
    fun kind(): String { return "ticket"; }
}

Ticket();
Ticket();
let third = Ticket();
print("" + third.number + " of " + Ticket.count() + ", a " + Ticket.kind());
```

```output
3 of 3, a ticket
```

## Class methods through an interface

An interface can declare class methods, which the class of each value of it has. The type
of any class that implements `Shape` is `Type[Shape]`, from `std:types`, so a class used
as a value goes where one is expected:

```zs
import { print } from "std:io";
import { Interface, class } from "std:oop";
import { Type } from "std:types";
import { String, Int32 } from "core:runtime";

def Shape of Interface {
    @class
    fun unit(): Shape;

    @class
    fun kind(): String;

    fun area(): Int32;
}

def Square : Shape {
    let side: Int32;

    new(this, side: Int32) { this.side = side; }

    @class
    fun unit(): Shape { return Square(1); }

    @class
    fun kind(): String { return "square"; }

    fun area(): Int32 { return side * side; }
}

def Circle : Shape {
    let radius: Int32;

    new(this, radius: Int32) { this.radius = radius; }

    @class
    fun unit(): Shape { return Circle(1); }

    @class
    fun kind(): String { return "circle"; }

    fun area(): Int32 { return 3 * radius * radius; }
}

fun describe(kind: Type[Shape]): String { return "a unit " + kind.kind() + " has area " + kind.unit().area(); }

print(describe(Square));
print(describe(Circle));
```

```output
a unit square has area 1
a unit circle has area 3
```

## Properties

A property is its accessors: `get Name` reads it, and `set Name` writes it.

```zs
import { print } from "std:io";
import { Int32 } from "core:runtime";

def Square {
    let side: Int32;

    new(this, side: Int32) { this.side = side; }

    get Side(): Int32 { return side; }

    set Side(value: Int32) { side = value; }

    get Area(): Int32 { return side * side; }
}

let square = Square(3);
square.Side = 5;
print(square.Area);
```

```output
25
```

## Overloaded methods

Methods of one name are overloads, as functions are:

```zs
import { print } from "std:io";
import { String, Int32 } from "core:runtime";

def Printer {
    new() { }

    fun show(value: Int32): String { return "number " + value; }

    fun show(value: String): String { return "text " + value; }
}

let printer = Printer();
print(printer.show(1));
print(printer.show("one"));
```

```output
number 1
text one
```

## Methods as values

A method reached through an object is a delegate bound to the object, where a .NET
delegate is expected:

```zs
import { print } from "std:io";
import { Int32, Func } from "core:runtime";

def Adder {
    let by: Int32;

    new(this, by: Int32) { this.by = by; }

    fun add(value: Int32): Int32 { return value + by; }
}

let addTen: Func[Int32, Int32] = Adder(10).add;
print(addTen(32));
```

```output
42
```

## Indexing

A method marked as the index operator is what indexing an object calls. Marked `@class`
too, it is what indexing the class calls:

```zs
import { print } from "std:io";
import { class } from "std:oop";
import { operator } from "core:zsharp";
import { Operator } from "pkg:zsharp/lang/hir";
import { Int32 } from "core:runtime";

def Scale {
    let factor: Int32;

    new(this, factor: Int32) { this.factor = factor; }

    @class @operator(Operator.Index)
    fun(factor: Int32): Scale { return Scale(factor); }

    @operator(Operator.Index)
    fun(value: Int32): Int32 { return value * factor; }
}

let triple = Scale[3];
print(triple[14]);
```

```output
42
```
