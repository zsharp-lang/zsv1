# Classes

A function gives a name to code. A class gives a name to data and the code that goes with
it: an account and what can be done to it, a point and how it moves. `def` defines one.

## Fields, constructors and methods

A class holds its **fields**, which are `let`s in it, is made by its **constructors**,
which are `new`, and has **methods**, which are `fun`s:

```zs
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

- Calling the class, `Account("Ada")`, runs its constructor and gives the new object.
- A constructor's first parameter, `this`, is the object it is making.
- A method reaches the object's fields by their names, as `deposit` does with `balance`.
- A field's default value, as `balance`'s `0`, is given to each object as it is made.
  A field written with a value and no type has its value's type.

## The object a method runs on

A method can name its object too, by taking `this` as its first parameter, as a
constructor does. It needs to only where something else has a field's name, as a
parameter here does:

```zs
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

In `moved`, `x` is the parameter, so the field is `this.x`; `y` is still the field.

## Classes are values

A class is a value too, and you've been using it as one: `Account("Ada")` calls it. The
value a class's name stands for is its **type object**, which can be kept in a variable
and passed around like any value:

```zs
import { print } from "std:io";
import { class } from "std:oop";
import { Type } from "std:types";
import { String } from "core:runtime";

def Ticket {
    new() { }
    @class fun kind(): String { return "ticket"; }
}

let kind: Type[Ticket] = Ticket;
print(kind.kind());
```

```output
ticket
```

`Type[Ticket]`, from `std:types`, is the type of `Ticket`'s type object. A member marked
`@class` is the type object's, so `kind()` runs on the class, not on a ticket.

## No inheritance

A Z# class has no base class. What classes have in common, they share by implementing
[interfaces](interfaces.md), which is the next page. [More on classes](more-classes.md)
goes through the rest of what a class can have: named constructors, static and class
members, properties, overloads, methods as values and indexing.
