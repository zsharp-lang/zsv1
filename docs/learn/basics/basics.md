# First steps

Z# runs on .NET. Its values are .NET's values, its strings and numbers are .NET's strings
and numbers, and everything .NET has, Z# can use. That is why the first lines of most
scripts import a few .NET types, and why, later on, a whole .NET assembly is only an
import away.

This page goes through what every script is made of, from its first statement to the
operators between its values.

## Statements run in order

A script is a list of statements. Each one ends with `;`, and runs before the next one is
read:

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

A comment starts with `//` and runs to the end of the line. Braces group statements into
a block, which is a statement of its own.

That "before the next one is read" matters more than it seems. A statement can define
something, and the very next one can already use it, as a real value. Much of what makes
Z# unusual starts there.

## Importing what you use

A script starts with nothing but the statements it has. Whatever it uses, it imports:

```zs fragment
import { print } from "std:io";                          // a name
import { String, Int32 as Number } from "core:runtime";  // several, one renamed
```

The text after `from` says where the names come from:

| Source | Is |
|---|---|
| `std:name` | a library of Z#'s standard library, as `std:io` |
| `core:name` | a library of the toolchain running the script, as `core:runtime`, which has .NET's common types |
| `pkg:name` | a package |
| `"path.zs"` | another file of Z#, relative to this one |
| `"path.dll"` | a .NET assembly |

You'll meet most of these as the pages go on. For now, `std:io` has `print`, and
`core:runtime` has the types: `String`, `Int32`, `Boolean`, `Double` and the rest of
.NET's common ones.

## Variables

`let` makes a variable:

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

A variable has a type: the one you write, or else its value's. `count` is an `Int32`,
because `3` is. Assigning it a value of another type is an error, unless the value
converts implicitly, as an `Int32` does to a `Double`.

## Values

The values you write directly:

| Value | Type |
|---|---|
| `42`, `-7` | `Int32` |
| `1.5` | `Double` |
| `"text"` | `String` |
| `true`, `false` | `Boolean` |
| `null` | any reference type |
| `[1, 2, 3]` | an array of what it goes to |

Since they are .NET's values, they have everything .NET gives them:

```zs
import { print } from "std:io";

let text = "Hello";
print(text.ToUpper() + " " + text.Length);
```

```output
HELLO 5
```

## Operators

Operators work as you'd expect from most languages. They are chosen by the types of what
they combine, so `+` adds numbers and joins strings, and a string joins with any value:

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

There is no `and` or `not` yet. [What isn't there yet](not-yet.md) says what to write
instead.

Next, [control flow](control-flow.md): making the statements choose and repeat.
