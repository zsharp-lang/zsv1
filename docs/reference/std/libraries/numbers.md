# std:numbers

Z#'s own numbers, apart from the CLR's: `std:numbers/integer` has `I8`, `I16`, `I32`,
`I64`, `U8`, `U16`, `U32` and `U64`, and `std:numbers/real` has `F16`, `F32` and `F64`, each
with its arithmetic operators and comparisons. A type of them is a definition whose
meta-type says what its numbers are, as `def I32 of FixedBitWidthInteger(32, true) {}`, so
a library can define more. The CLR runs them as its own types of their width, which a
cast goes to and from, with nothing changed:

```zs
import { print } from "std:io";
import { Int32 } from "core:runtime";
import { I32, U8 } from "std:numbers/integer";

let a = 5 as I32;
let b = 7 as I32;
print((a * b) as Int32);
print(a < b);

let small = 200 as U8;
print((small + (100 as U8)) as Int32);
```

```output
35
True
44
```

Nothing converts implicitly yet, so a number is cast to one of them, and back.
A comparison gives the CLR's `Boolean` for now.
