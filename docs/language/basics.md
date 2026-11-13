# Basics

## Documents and statements

A file of Z# is a document: a list of statements, which run in order, each as soon as it
is compiled. A statement ends with `;`, and a block, in braces, is a statement of its own.
Comments start with `//` and go to the end of the line.

```zs
import { print } from "std:io";

// Each statement runs before the next one is compiled.
print("first");
print("second");
```

```output
first
second
```

## Imports

`import` brings names from a source into the document:

```zs fragment
import { print } from "std:io";                 // a name
import { String, Int32 as Number } from "core:runtime";  // several, one renamed
import "std:traits/static";                     // a library, for what it installs
```

A source is any of these:

| Source | What it is |
|---|---|
| `std:name` | A library of the standard library, as `std:io`, `std:oop` or `std:project`. |
| `core:name` | A library of the toolchain's own, as `core:runtime`, which has .NET's types. |
| `pkg:name` | A package, through the package manager. |
| `"path.zs"` | Another document, relative to this one. It gives what it exports. |
| `"path.dll"` | A .NET assembly. |

An import that names nothing, as `import "std:traits/static";`, imports the library for
what it installs. The prelude is imported that way: `zs` imports it before a script's
first statement, and it installs the standard definitions.

## Variables

`let` makes a variable. Its type is the one written, or else its value's:

```zs
import { print } from "std:io";
import { String } from "core:runtime";

let count = 3;
let name: String = "Z#";
count = count + 1;

print(name + " has " + count);
```

```output
Z# has 4
```

A variable is assigned with `=`. A value of another type than the variable's is an error,
unless it converts to that type implicitly, as an `Int32` does to a `Double`.

## Values

| Value | Type |
|---|---|
| `42`, `-7` | `Int32` |
| `1.5` | `Double` |
| `"text"` | `String` |
| `true`, `false` | `Boolean` |
| `null` | any reference type |
| `[1, 2, 3]` | an array of what it goes to |

These are .NET's types, from `core:runtime`, and a value has every member its type has in
.NET:

```zs
import { print } from "std:io";

let text = "Hello";
print(text.ToUpper() + " " + text.Length);
```

```output
HELLO 5
```

## Operators

From the loosest to the tightest:

| Operators | Meaning |
|---|---|
| `=` | assignment |
| `or` | either is true |
| `\|` | bitwise or |
| `&` | bitwise and |
| `==` `!=` | equal, not equal |
| `<` `>` `<=` `>=` `as` `is` | comparisons, casts and type tests |
| `+` `-` | addition, subtraction, and joining strings |
| `*` `/` | multiplication, division |
| `-x` | negation |
| `.` `()` `[]` | members, calls and indexing |

An operator is chosen by the types of its operands, as .NET defines them: `+` adds
numbers and joins strings, and a string joins with any value.

```zs
import { print } from "std:io";

print(1 + 2 * 3);
print(7 / 2);
print(7.0 / 2.0);
print("value: " + (10 - 4));
print(2 < 3 or false);
```

```output
7
3
3.5
value: 6
True
```

There is no `and` or `not` yet: [what isn't there yet](not-yet.md) says what to write
instead.
