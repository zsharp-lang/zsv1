# Capabilities

A class lowers to core LIR as a record of fields with a table of functions, which any
platform can run. But a platform with objects and virtual calls of its own, as .NET, can
do better: realize the class as one of its classes. For that, the class's CO has to know
the platform can, without knowing which platform it is.

That is a capability: an interface a platform implements to say what it can do. A CO
lowering for a target asks for a capability by matching the target against it, and lowers
to the richest form the target has, or to a plainer one of core LIR.

## The standard capabilities

`std:zsharp/capabilities` has them, as interfaces written in Z#:

| Capability | Says the platform has | Used by |
|---|---|---|
| `Polymorphism` | objects of classes, interfaces they implement, methods called through what each object is, casts and type tests | `std:oop`'s classes and interfaces |
| `PlatformInterfaces` | classes that implement the platform's own types, as .NET's interfaces | a Z# class that implements a .NET interface |
| `Numbers` | integers and floating-point numbers of each bit width | `std:numbers` |
| `Truths` | truth values | `std:boolean` |

A capability is standard, not a platform's: what it asks and what it means are part of the
standard library, and each platform implements those it can.

## How a CO uses one

A Z# class lowers to a structure. For a target with `Polymorphism`, it also tells the
target that the structure's values are objects of a class, that its methods are the
class's, and which interfaces it implements:

```zs fragment
if (target is let polymorphism: Polymorphism) {
    polymorphism.objects(structure);
    polymorphism.method(function, structure);
}
```

A capability doesn't make LIR. It marks the LIR the CO built with what the platform needs
to realize it richly, so the CO still builds the definition, and a platform without the
capability can still be given correct LIR. For a target without `Polymorphism`, a class is
meant to be a record, with a table of its functions and `CallIndirect`; that lowering
isn't written yet, so a Z# class needs the capability today.

## Giving a platform a capability

A platform that exists doesn't have to know a capability to have it: it is given one by a
trait, an implementation of the capability for the platform's type, defined apart from
both. `std:zsharp/clr` gives every CLR platform the standard capabilities that way:

```zs fragment
def ClrPolymorphism of Implementation[Polymorphism, ClrPlatform] {
    @public
    let value: ClrPlatform;

    @public @explicitThis
    new(this, value: ClrPlatform) { this.value = value; }

    @public @explicitThis
    fun objects(this, structure: Structure) {
        structure.Metadata.Add(Polymorphic());
    }

    ...
}
```

The prelude installs the trait dispatchers, so a `ClrPlatform` matches `Polymorphism`
through this implementation. A platform of your own can implement a capability directly,
or be given it by a trait from another library.

## Pure capabilities

A capability is pure when it speaks only core LIR: what it asks and what it marks are
core LIR's concepts, so any platform can implement it. `Polymorphism` is: it takes core
structures and functions. Its implementation for the CLR is not: it marks them with
`Polymorphic` and `MethodOf`, metadata of the CLR's dialect, which only CLR platforms
read. That is fine, as the implementation belongs to the CLR, and the capability stays
pure for every other platform. A capability that asked for a dialect's nodes would be tied
to that dialect, and only its platforms could have it.
