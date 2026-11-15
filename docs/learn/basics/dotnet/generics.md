# Generics

.NET's generic types and methods are given their type arguments in brackets.

A generic type is given its type arguments in brackets:

```zs
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
