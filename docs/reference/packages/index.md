# Standard packages

What a build uses, as packages: definitions and tools for building, imported with `pkg:`.
The toolchain gives its own packages, so they are there before the package manager is,
and the package manager finds the rest.

| Package | Is |
|---|---|
| [Project compiler](project-compiler.md), `std:project` | Finds a project and runs its module files. |
| [Package manager](package-manager.md), `std:packages` | What `pkg:` finds beyond the toolchain's own packages. |
| [.NET platform](cecil.md), `pkg:cecil` | The Cecil platform, which writes LIR as a .NET assembly. |
| [CLR packages](clr.md), `pkg:clr/hir`, `pkg:clr/lir`, `pkg:clr/platform` | The CLR's HIR, its LIR dialect, and what its platforms share. |
| [Z# packages](zsharp.md), `pkg:zsharp/...` | The toolchain's and the language's own definitions: HIR, MIR, LIR, the AST, the parser, source maps. |

The project compiler and the package manager are written as standard libraries, `std:`,
today, though they are build-time tools.
