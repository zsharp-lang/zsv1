# Interfaces

A Z# class has no base class to share code through. Say a script deals with shapes: a
square and a rectangle are different classes, but code that describes a shape only needs
its area and its name. An interface says exactly that: which methods a value has, with no
code, so any class that has them can be used where the interface is expected.

## Declaring and implementing one

`def Name of Interface` declares one, with `Interface` from `std:oop`, and a class lists
the interfaces it implements after a colon:

```zs
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

`describe` knows nothing of squares or rectangles, only of `Shape`. A class that
implements an interface has to have each of its methods, with the same name and types, or
it is an error where the class is used.

## Getting the class back

Sometimes code that has a `Shape` needs to know which shape it is. `as` casts a value down
to a class that implements the interface, and `is` tests whether it is one. `is let name:
Type` tests and, where it's true, names the value as that type:

```zs
import { print } from "std:io";
import { Interface } from "std:oop";
import { String } from "core:runtime";

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
    if (pet is let dog: Dog) return dog.fetch();
    return pet.sound();
}

print(play(Dog()));
print(play(Cat()));
```

```output
fetching
meow
```

## .NET's interfaces

Since Z# runs on .NET, a class can implement a .NET interface too, as `IDisposable`, and
its objects go wherever .NET expects one:

```zs
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

An interface can also say what a class itself has, not only its objects: [more on
classes](more-classes.md#class-methods-through-an-interface) shows class methods through
an interface.
