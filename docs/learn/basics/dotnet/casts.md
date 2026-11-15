# Casts and nulls

Converting between .NET's types, and what `null` is.

`as` converts a value, as .NET does: a number to another number, a reference down to its
type. `null` goes to any reference type.

```zs
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
