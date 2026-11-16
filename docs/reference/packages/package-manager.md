# Package manager

`std:packages`, which the prelude imports, installs the package manager: what `pkg:` finds
beyond the toolchain's own packages. The package manager is a project in Z#,
`packages/package-manager`, which `std:packages` compiles the first time, or imports as
its build.

## Finding a package

`pkg:name` is looked for in the package manager's roots, in their order, and then in the
packages NuGet installed. The roots are, at first:

1. `packages` in the folder the process runs from;
2. the toolchain's own `packages`, next to the standard library.

In a root, a package named `a/b` is the first of:

| Found | As |
|---|---|
| `a/b.dll`, or `a/b/b.dll` | an assembly, imported as its file is |
| `a/b.zs`, or `a/b/b.zs` | a document |
| `a/b`, with a module file in it or in its `src` | a project: its build, `a/b/build/b.dll`, if there is one, or else the project compiled |

A project's build is imported from its bytes, so it can be built again while it is in use.
A project with no build is compiled once a process, and every import of it after that
gets the same module.

## Versions

`pkg:Name@1.2.3` is a version: in a root, the assembly `Name@1.2.3/Name.dll`, as a
toolchain carries what its own packages need. In NuGet's packages, `Name` is the newest
version installed, and `Name@1.2.3` that version, of the assembly of the target framework
most like the running one's. NuGet's folder is `NUGET_PACKAGES`, or its default.

## core:packages

The package manager, as `core:packages`, which `std:packages` adds to `core:`:

| Member | Is |
|---|---|
| `add(root)` | Adds a root, after those there are. |
| `load(source)` | Imports a package, as `pkg:` does. |
| `compileProjectsWith(compiler)` | Gives what compiles a package that is a project. |

An assembly a package's assembly refers to, which the running .NET can't find, is asked of
the package manager too.
