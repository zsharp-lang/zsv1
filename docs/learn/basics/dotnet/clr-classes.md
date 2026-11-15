# Classes of the CLR

A Z# class is Z#'s own. When code needs a class that .NET itself sees as an ordinary .NET class, with its accessibility, it defines a class of the CLR's instead.

A class of the CLR's defined in Z#, as `def Name of Class` with `Class` from the CLR's
HIR, `pkg:clr/hir`, has the CLR's accessibility, which that package's decorators set:
`@public`, `@private`, `@internal` and `@protected`. They apply to the CLR's definitions
alone. What isn't written is decided by where it is:

- a class and its members are private, and a top-level class, which the CLR can't make
  private, is internal to its assembly;
- an interface and its members are public;
- a module's functions are public, and its globals private, so a public global is
  always written.

```zs fragment
import { Int32 } from "core:runtime";
import { Class, public } from "pkg:clr/hir";

@public
def Counter of Class {
    let count: Int32;                                   // private

    @public
    new() { count = 0; }

    @public
    fun next(): Int32 { count = count + 1; return count; }
}
```

A method's code reaches its class's members by their names, as C# code does, over a name
outside the class of the same name. A parameter of the name hides one, and a base class's
members are reached through `this`. `@explicitThis` names the object a method runs on as
its first parameter, `this`, for code that needs it as a value:

```zs fragment
import { Class, public, explicitThis } from "pkg:clr/hir";

@public
def Node of Class {
    @public @explicitThis
    fun register(this, registry: Registry) { registry.add(this); }
}
```

A constructor can have a name, as a Z# class's can, and is called through it:

```zs
import { print } from "std:io";
import { Int32 } from "core:runtime";
import { Class, public } from "pkg:clr/hir";

@public
def Counter of Class {
    @public
    let count: Int32;

    @public
    new() { count = 0; }

    @public
    new from(start: Int32) { count = start; }
}

print(Counter.from(41).count);
print(Counter().count);
```

```output
41
0
```

The CLR has no named constructors, so `from` is a static method of the class, with the
constructor's accessibility, which calls a private constructor that takes a private
marker type, `Counter.<from>`, so no other constructor has its signature. A generic class
can't have one yet.

A method that implements an interface's needn't be public: one that isn't implements it
explicitly, as C#'s explicit implementations do. Accessibility is checked by the CLR as
the code runs, not yet as it's compiled.
