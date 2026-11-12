# Classes

`def` defines a class: its fields, constructors and methods. A Z# class has no
inheritance. What it shares with other classes, it shares by implementing
[interfaces](interfaces.md).

## Fields, constructors and methods

A field is a `let` in the class. A constructor is `new`, whose first parameter is `this`,
the object it makes. A method is a `fun`, which reaches the class's members by their
names:

```zs
import "std:prelude";
import { print } from "std:io";
import { String, Int32 } from "core:runtime";

def Account {
    let owner: String;
    let balance: Int32 = 0;

    new(this, owner: String) { this.owner = owner; }

    fun deposit(amount: Int32) { balance = balance + amount; }

    fun describe(): String { return owner + " has " + balance; }
}

let account = Account("Ada");
account.deposit(30);
account.deposit(12);
print(account.describe());
print(account.balance);
```

```output
Ada has 42
42
```

A field's default value is code, which runs for each object it makes. A field written
with a value and no type has its value's type.

A method names its object `this` if its first parameter is `this`, as a constructor
does, which it needs only where a parameter hides a member's name:

```zs
import "std:prelude";
import { print } from "std:io";
import { Int32 } from "core:runtime";

def Point {
    let x: Int32;
    let y: Int32;

    new(this, x: Int32, y: Int32) { this.x = x; this.y = y; }

    fun moved(this, x: Int32): Point { return Point(this.x + x, y); }
}

let point = Point(1, 2).moved(10);
print("" + point.x + ", " + point.y);
```

```output
11, 2
```

## Named constructors

A constructor with a name is a member of the class. The unnamed one is what calling the
class runs:

```zs
import "std:prelude";
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
import "std:prelude";
import { print } from "std:io";
import { static, class } from "std:oop";
import { String, Int32 } from "core:runtime";

def Ticket {
    @static let issued: Int32 = 0;
    let number: Int32;

    new(this) { issued = issued + 1; this.number = issued; }

    @static fun count(): Int32 { return issued; }
    @class fun kind(): String { return "ticket"; }
}

Ticket();
Ticket();
let third = Ticket();
print("" + third.number + " of " + Ticket.count() + ", a " + Ticket.kind());
```

```output
3 of 3, a ticket
```

## Properties

A property is its accessors: `get Name` reads it, and `set Name` writes it.

```zs
import "std:prelude";
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
import "std:prelude";
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
import "std:prelude";
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
import "std:prelude";
import { print } from "std:io";
import { class } from "std:oop";
import { operator } from "core:zsharp";
import { Operator } from "std:zsharp/lang";
import { Int32 } from "core:runtime";

def Scale {
    let factor: Int32;
    new(this, factor: Int32) { this.factor = factor; }

    @class @operator(Operator.Index) fun(factor: Int32): Scale { return Scale(factor); }
    @operator(Operator.Index) fun(value: Int32): Int32 { return value * factor; }
}

let triple = Scale[3];
print(triple[14]);
```

```output
42
```
