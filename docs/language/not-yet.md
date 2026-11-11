# What isn't there yet

Z# is young. These are what a reader of other languages might reach for, and what to
write instead for now.

| Not yet | Instead |
|---|---|
| `and`, `not`, `!` | Nest the conditions, or compare with `false`: `if (done == false)`. `or` is there. |
| `%` | `value - value / divisor * divisor`, or .NET's `Math.DivRem`. |
| `**` for numbers | `Math.Pow`. `**` parses, but no type defines it yet. |
| `for` loops | `while`, with a variable for the index. |
| `try`, `catch` and `throw` | Nothing yet: a .NET exception ends the script. |
| A document's function as a delegate | A function of a [module](modules.md) is one. |
| Generic classes in Z# | A class of .NET's, or one defined in Z# with `def Name of DotNet_Class`. |

And what isn't planned, by design:

- **Inheritance.** A Z# class has no base class. What classes share, they share by
  implementing [interfaces](interfaces.md).
- **A fixed set of definitions.** What `def`, `fun` and `module` mean is defined by the
  standard library, and a library can define more.
