# Using .NET

Z# runs on .NET, and reaches .NET's types as its own. `core:runtime` has the ones Z# needs
most, as `String`, `Int32`, `List` and `Path`, and any assembly can be imported by its
path, as .NET's own are, or by the assembly itself: the rest of `System`'s types are in
`Object`'s assembly, `Object.Assembly`.

## .NET's types

A type's members are what .NET says they are: its constructors, methods, properties,
fields and operators. A method's overload is chosen by its arguments, which can be given
by name, and a parameter with a default value can be left out.

```zs
import "std:prelude";
import { print } from "std:io";
import { Object, String, Int32 } from "core:runtime";
import { Math, DateTime } from Object.Assembly;

print(Math.Max(3, 7));
print(Int32.Parse("41") + 1);
print(String.Join(", ", ["a", "b", "c"]));
print(DateTime(2026, 1, 2).ToString("yyyy-MM-dd"));
```

```output
7
42
a, b, c
2026-01-02
```

## Generics

A generic type is given its type arguments in brackets:

```zs
import "std:prelude";
import { print } from "std:io";
import { String, Int32, List, Dictionary } from "core:runtime";

let numbers = List[Int32]();
numbers.Add(3);
numbers.Add(4);

let ages = Dictionary[String, Int32]();
ages["Ada"] = 36;

print("" + numbers.Count + " numbers, the last " + numbers[1] + "; Ada is " + ages["Ada"]);
```

```output
2 numbers, the last 4; Ada is 36
```

A generic method is given its type arguments the same way, or infers them from its
arguments.

## Arrays

Values in brackets are an array where one is expected, and a type given a length is an
array of that length:

```zs
import "std:prelude";
import { print } from "std:io";
import { Int32 } from "core:runtime";

let squares: Int32[] = Int32[4];
let index = 0;
while (index < squares.Length) {
    squares[index] = index * index;
    index = index + 1;
}

let given: Int32[] = [5, 6, 7];
print("" + squares[3] + " " + given.Length);
```

```output
9 3
```

## Casts and nulls

`as` converts a value, as .NET does: a number to another number, a reference down to its
type. `null` goes to any reference type.

```zs
import "std:prelude";
import { print } from "std:io";
import { Object, String, Int32, Int64 } from "core:runtime";

let wide = 3 as Int64;
let value: Object = "text";
let text = value as String;
let nothing: String = null;

print("" + wide + " " + text.Length + " " + (nothing == null));
```

```output
3 4 True
```

## Accessibility of the CLR's definitions

A class of the CLR's defined in Z#, as `def Name of DotNet_Class`, has the CLR's
accessibility, which the decorators of `std:zsharp/package/dotnet` set: `@public`,
`@private`, `@internal` and `@protected`. They apply to the CLR's definitions alone. What isn't written is
decided by where it is:

- a class and its members are private, and a top-level class, which the CLR can't make
  private, is internal to its assembly;
- an interface and its members are public;
- a module's functions are public, and its globals private, so a public global is
  always written.

```zs fragment
import { Int32, DotNet_Class, explicitThis } from "core:runtime";
import { public } from "std:zsharp/package/dotnet";

@public def Counter of DotNet_Class {
    let count: Int32;                                   // private
    @public @explicitThis new(this) { this.count = 0; }
    @public @explicitThis fun next(this): Int32 { this.count = this.count + 1; return this.count; }
}
```

A constructor can have a name, as a Z# class's can, and is called through it:

```zs
import "std:prelude";
import { print } from "std:io";
import { Int32, DotNet_Class, explicitThis } from "core:runtime";
import { public } from "std:zsharp/package/dotnet";

@public def Counter of DotNet_Class {
    @public let count: Int32;
    @public @explicitThis new(this) { this.count = 0; }
    @public @explicitThis new from(this, start: Int32) { this.count = start; }
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

## Importing an assembly

An assembly is imported by its path, as a document is:

```zs fragment
import { JsonSerializer } from "path/to/System.Text.Json.dll";
```

The path is any code that gives a string, so a script can compute it, as from where .NET
is, the folder of `Object`'s assembly:

```zs
import "std:prelude";
import { print } from "std:io";
import { Object, Path } from "core:runtime";

let runtime = Path.GetDirectoryName(Object.Assembly.Location);
import { Process } from Path.Combine(runtime, "System.Diagnostics.Process.dll");

print(Process.GetCurrentProcess().Id > 0);
```

```output
True
```

An assembly the running process has is imported as itself, with no path, as
`import { Math } from Object.Assembly;`. It's the same module importing its file gives.

A package of the package manager is imported as `pkg:Name`, or `pkg:Name@1.2.3` for a
version: it looks in the project's `packages`, the toolchain's, and the packages NuGet
installed.
