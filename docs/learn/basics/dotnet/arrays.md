# Arrays

An array is .NET's: a fixed number of values of one type.

Values in brackets are an array where one is expected, and a type given a length is an
array of that length:

```zs
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
