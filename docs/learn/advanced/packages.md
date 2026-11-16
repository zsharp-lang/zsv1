# Packages

A [project](../basics/projects.md) is built by a script you write for it. That's fine for
an application, but for a library, code meant to be reused or published, it is backwards:
whoever uses the library would have to build it first, and know where it is. A package
turns that around. It is imported by its name, with `pkg:`, and the package manager finds
it, compiles it if it has to, and gives it.

## Using a project as a package

The package manager looks for a package in its roots: first `packages` in the folder a
script runs from, then the toolchain's own. A project in a root is a package by its
folder's name, so with `greetings` there:

```text
packages/
  greetings/
    src/
      .zs
      hello.zs
main.zs
```

`main.zs` imports it like any library:

```zs fragment
// main.zs
import { print } from "std:io";
import { hello } from "pkg:greetings";

print(hello("world"));
```

The first import of a package in a process compiles it from its sources, and every import
after that gets the same module. A root of your own is added with `core:packages`'s
`add(folder)`, before the imports that need it:

```zs fragment
import { add } from "core:packages";
add("libraries");

import { hello } from "pkg:greetings";
```

## Building a package

Compiling a package from its sources in every process that uses it takes time. A package
built as a .NET assembly, into its `build` folder, is imported as that assembly instead:
for `pkg:greetings`, `packages/greetings/build/greetings.dll`.

So a package has a build script of its own, which writes its assembly there, as
[compiling projects](build-scripts/compiling-projects.md) shows: it describes the module
with `encode` before lowering it, so importing the assembly gives back Z#'s definitions and
not only the .NET types they were built as. The toolchain's packages each have one, as
`packages/lsp/build.zs` for `pkg:lsp`. A built package has to be built again when its
sources change, as nothing compares them.

## What else a package can be

A package is whatever the package manager finds by its name: a project, a document, a
.NET assembly, or a package NuGet installed, with a version, as `pkg:Name@1.2.3`. [The
package manager](../../reference/packages/package-manager.md) lists the rules.
