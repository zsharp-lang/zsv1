# Using .NET

Z# runs on .NET, and reaches .NET's types as its own. `core:runtime` has the ones Z# needs
most, as `String`, `Int32`, `List` and `Path`, and any assembly can be imported by its
path, as .NET's own are: the rest of `System`'s types are in `Object`'s assembly.

## .NET's types

A type's members are what .NET says they are: its constructors, methods, properties,
fields and operators. A method's overload is chosen by its arguments, which can be given
by name, and a parameter with a default value can be left out.

```zs
import "std:prelude";
import { print } from "std:io";
import { Object, Type, String, Int32 } from "core:runtime";

let objectType: Type = Object;
import { Math, DateTime } from objectType.Assembly.Location;

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
import { Object, Type, Path } from "core:runtime";

let objectType: Type = Object;
let runtime = Path.GetDirectoryName(objectType.Assembly.Location);
import { Process } from Path.Combine(runtime, "System.Diagnostics.Process.dll");

print(Process.GetCurrentProcess().Id > 0);
```

```output
True
```

A package of the package manager is imported as `pkg:Name`, or `pkg:Name@1.2.3` for a
version: it looks in the project's `packages`, the toolchain's, and the packages NuGet
installed.
