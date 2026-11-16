# std:fs

:::caution
`std:fs` is a sketch of a library, not one that works yet: its sources describe the API it
is meant to have, with parts Z# doesn't have yet, as enumerations, `not` and string
interpolation, and some of its files are empty. It can't be imported.
:::

What it is meant to be:

This library allows users to query the local file system.


It gives a modelled access to the file system, so that you no longer need to deal
with string paths.

```zs fragment
let path: Path = input[Path](); // e.g

if (path is file of File) {
    print("File ${file.name} in ${file.parent.name} is ${file.size}B");
}
```
