# Interfaces

An interface declares methods, with no code, which a value of any class that implements
it has. `def Name of Interface` defines one, and a class lists what it implements after a
colon.

```zs
import "std:prelude";
import { print } from "std:io";
import { Interface } from "std:oop";
import { String, Int32 } from "core:runtime";

def Shape of Interface {
    fun area(): Int32;
    fun name(): String;
}

def Square : Shape {
    let side: Int32;
    new(this, side: Int32) { this.side = side; }
    fun area(): Int32 { return side * side; }
    fun name(): String { return "square"; }
}

def Rectangle : Shape {
    let width: Int32;
    let height: Int32;
    new(this, width: Int32, height: Int32) { this.width = width; this.height = height; }
    fun area(): Int32 { return width * height; }
    fun name(): String { return "rectangle"; }
}

fun describe(shape: Shape): String { return "a " + shape.name() + " of area " + shape.area(); }

print(describe(Square(3)));
print(describe(Rectangle(2, 5)));
```

```output
a square of area 9
a rectangle of area 10
```

A class must have every method of each interface it implements, with the same name and
types, or it is an error when the class is used.

## Casts and type tests

A value of an interface can be cast down to a class that implements it with `as`, and
tested for one with `is`. `is name of Type` also names the value as that type, where the
test is true:

```zs
import "std:prelude";
import { print } from "std:io";
import { Interface } from "std:oop";
import { String, Int32 } from "core:runtime";

def Pet of Interface { fun sound(): String; }

def Dog : Pet {
    new() { }
    fun sound(): String { return "woof"; }
    fun fetch(): String { return "fetching"; }
}

def Cat : Pet {
    new() { }
    fun sound(): String { return "meow"; }
}

fun play(pet: Pet): String {
    if (pet is dog of Dog) return dog.fetch();
    return pet.sound();
}

print(play(Dog()));
print(play(Cat()));
```

```output
fetching
meow
```

## Class methods of an interface

An interface can declare class methods, which the class of each value of it has. The type
of any class that implements `Shape` is `Type[Shape]`, from `std:types`, so a class used
as a value goes where one is expected:

```zs
import "std:prelude";
import { print } from "std:io";
import { Interface, class } from "std:oop";
import { Type } from "std:types";
import { String, Int32 } from "core:runtime";

def Shape of Interface {
    @class fun unit(): Shape;
    @class fun kind(): String;
    fun area(): Int32;
}

def Square : Shape {
    let side: Int32;
    new(this, side: Int32) { this.side = side; }
    @class fun unit(): Shape { return Square(1); }
    @class fun kind(): String { return "square"; }
    fun area(): Int32 { return side * side; }
}

def Circle : Shape {
    let radius: Int32;
    new(this, radius: Int32) { this.radius = radius; }
    @class fun unit(): Shape { return Circle(1); }
    @class fun kind(): String { return "circle"; }
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

## .NET's interfaces

A class can implement a .NET interface too, as `IDisposable`, and its objects go where one
is expected:

```zs
import "std:prelude";
import { print } from "std:io";
import { IDisposable, Int32 } from "core:runtime";

def Resource : IDisposable {
    let closed: Int32 = 0;
    new() { }
    fun Dispose() { closed = closed + 1; }
}

let resource = Resource();
let disposable: IDisposable = resource;
disposable.Dispose();
print("closed " + resource.closed + " time");
```

```output
closed 1 time
```
