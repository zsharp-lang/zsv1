# std:boolean

Z#'s own truth values, `Bool`, apart from the CLR's `Boolean`, with `==` and `!=`. The CLR
runs them as its `Boolean`, which a cast goes to and from. A `Bool`, like Z#'s numbers,
goes where an object is expected as what the CLR runs it as:

```zs
import { print } from "std:io";
import { Boolean } from "core:runtime";
import { Bool } from "std:boolean";
import { I32 } from "std:numbers/integer";

let yes = true as Bool;
let no = false as Bool;
print(yes == no);
print(yes);
print(5 as I32);
if ((yes != no) as Boolean) print("they differ");
```

```output
False
True
5
they differ
```

A condition still takes the CLR's `Boolean`, which a `Bool` is cast to.
