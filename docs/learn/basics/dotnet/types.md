# Types and members

Every value in Z# is a .NET value, so its type's members are what .NET says they are: its constructors, methods, properties,
fields and operators. A method's overload is chosen by its arguments, which can be given
by name, and a parameter with a default value can be left out.

```zs
import { print } from "std:io";
import { Object, String, Int32 } from "core:runtime";
import { Math, DateTime } from Object.Assembly;

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
