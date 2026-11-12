# Control flow

## If

`if` runs a statement when its condition is true, and the statement after `else`
otherwise:

```zs
import "std:prelude";
import { print } from "std:io";

let size = 12;
if (size > 10) print("large");
else print("small");

if (size == 0) {
    print("empty");
} else if (size < 20) {
    print("some");
} else {
    print("many");
}
```

```output
large
some
```

## While

`while` runs its body as long as its condition holds. Its `else`, if it has one, runs when
the condition stops holding:

```zs
import "std:prelude";
import { print } from "std:io";

let index = 0;
while (index < 3) {
    print("step " + index);
    index = index + 1;
} else {
    print("done");
}
```

```output
step 0
step 1
step 2
done
```

`break` leaves the loop, skipping its `else`, and `continue` starts the next iteration.

## Named loops

A loop can have a name, after `while`, which `break from` and `continue from` use to leave
or continue an outer loop:

```zs
import "std:prelude";
import { print } from "std:io";

let row = 0;
while rows (row < 3) {
    let column = 0;
    while (column < 3) {
        if (column == 2) { break from rows; }
        print("" + row + "," + column);
        column = column + 1;
    }
    row = row + 1;
}
```

```output
0,0
0,1
```

## Loops that give a value

A loop is an expression too. `break` with a value leaves it with that value, and its
`else` is its value when its condition stops holding, so a loop used as a value needs one:

```zs
import "std:prelude";
import { print } from "std:io";

let index = 0;
let found = while (index < 10) {
    index = index + 1;
    if (index * index > 30) { break index; }
} else 0;

print("the first square over 30 is of " + found);
```

```output
the first square over 30 is of 6
```

## Return

`return` leaves a function, with a value if the function gives one:

```zs
import "std:prelude";
import { print } from "std:io";
import { Int32 } from "core:runtime";

fun sign(value: Int32): Int32 {
    if (value < 0) return -1;
    if (value == 0) return 0;
    return 1;
}

print(sign(-5) + sign(0) + sign(9));
```

```output
0
```
