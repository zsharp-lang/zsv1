# Importing assemblies

Most of .NET's ecosystem is in assemblies: .NET's own beyond its common types, and every library on NuGet. Any of them is an import away.

An assembly is imported by its path, as a document is:

```zs fragment
import { JsonSerializer } from "path/to/System.Text.Json.dll";
```

The path is any code that gives a string, so a script can compute it, as from where .NET
is, the folder of `Object`'s assembly:

```zs
import { print } from "std:io";
import { Object } from "core:runtime";
import { Path } from Object.Assembly;

let runtime = Path.GetDirectoryName(Object.Assembly.Location);
import { Process } from Path.Combine(runtime, "System.Diagnostics.Process.dll");

print(Process.GetCurrentProcess().Id > 0);
```

```output
True
```

An assembly the running process has is imported as itself, with no path, as
`import { Math } from Object.Assembly;`. It's the same module importing its file gives.

A package of the package manager is imported as `pkg:Name`, or `pkg:Name@1.2.3` for a
version: it looks in the project's `packages`, the toolchain's, and the packages NuGet
installed.
