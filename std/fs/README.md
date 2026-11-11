This library allows users to query the local file system.


It gives a modelled access to the file system, so that you no longer need to deal
with string paths.

```zs
let path: Path = input[Path](); // e.g

if (path is file of File) {
    print("File ${file.name} in ${file.parent.name} is ${file.size}B");
}
```
