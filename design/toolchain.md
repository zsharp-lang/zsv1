# The toolchain

## Bootstrapping

`bootstrap.zs` builds the toolchain's Z# projects from their sources, as an installer
would, and checks the build.

- **The standard library is its own seed.** A library of it is a project,
  `std/name/src`, built into `std/name/build`, which is what `std:name` loads: so its
  sources are compiled by the toolchain that loads its build, as a compiler that
  compiles itself is. The builds aren't sources, so they aren't in git: a release ships
  them, and a checkout is seeded from one.
- **Generations.** Each run of the bootstrap is a generation. It builds each library, in
  the order of what they need, with the libraries it loads, for the .NET that runs the
  toolchain. If any of them built something else than what was loaded, a new process,
  which loads what was built, is the next generation, until a generation builds what it
  loads, which it must within eight. A library is built only by a generation that loads
  what it needs built, and none of them built differently by it, as its assembly refers
  to theirs, so a library built for the first time from a document is built again by
  the next generation from its build.
- **A library is all of its module.** A member that fails to lower fails its build,
  as `std:modules`' `failures(module)` says, so a broken library isn't a seed.
- **A seed only has what was built.** The bootstrap runs on the seed, so it can only
  use what the seed has: a feature the bootstrap, the prelude, or what the seed decodes
  needs is built into the seed by one bootstrap and used by the next. A library that
  becomes a project keeps its document for one bootstrap, which seeds it.
- **The Cecil platform and the package manager** are built from their sources every
  generation, so nothing an older build of them lacks is needed: what earlier bootstraps
  built of them is removed before the prelude is imported, which then compiles the
  package manager from its source. The Cecil platform then builds itself, compiled from
  its source first and then as what it built, until two generations are the same, which
  they are from the first, and the standard library is built with it. The package
  manager is built once the standard library builds itself the same.
- `tests/run.sh` bootstraps before the tests run, so they run with what a bootstrap
  builds, and a library or platform that doesn't build itself the same fails the run.
- **Every library is encoded**, so importing its build gives the Z# module it was, as
  **Reflection and importing** describes, bound to what it was built as.
- **Not built, by what they are:** the prelude and `std:packages`, which assemble the
  environment, as documents do: `std:packages` binds the package manager, which each
  generation compiles from its sources, so a build of it couldn't refer to it.

## Packages

`pkg:` imports a package, with the package manager, which is a package of the
toolchain's, as the Cecil platform is, in `packages/package-manager`, written in Z#. Its
`build.zs` builds it as an assembly into its `build` with the Cecil platform, and
`std:packages`, which the prelude imports, loads
that assembly, from its bytes so it can be built again while in use, or else, until it is
built, compiles the project, which is how it is built the first time. Either is
`core:packages`.

- **Roots.** A package is found in roots, in order: the project's, `packages` where the
  toolchain runs from, then the toolchain's, `packages` next to the standard library.
  `roots` holds them, its initial ones defined by the package manager, and `add(root)`
  adds one after them.
- **What a package is.** In a root, `a/b` is a file, `a/b.dll` or `a/b/b.dll`, or a
  document, `a/b.zs` or `a/b/b.zs`, imported as files are, or a project, the directory
  `a/b` with its module file in it or in its `src`. A project built as an assembly into
  `a/b/build/b.dll` is that assembly, loaded from its bytes so it can be built again while
  in use. One that isn't is compiled by what the package manager is given,
  `compileProjectsWith`, which `std:packages` gives the project compiler, and what it
  emits first is the package.
- **Dependencies.** An assembly refers to what it was built against by name and version,
  as the Cecil platform's refers to Mono.Cecil. The running .NET asks the package manager
  for an assembly it has none of that name of, and the package manager gives the one a
  build put next to a built package it loaded, or else the one NuGet installed of that
  version, or else the newest.
- **Versions.** A version of a package, `Name@1.2.3`, is in a root as an assembly in
  `Name@1.2.3/Name.dll`, so a toolchain carries the packages its own packages need, as a
  release carries Mono.Cecil for the Cecil platform.
- **NuGet.** What no root has is looked for in the packages NuGet installed: `Name` is
  the newest version that isn't a preview, `Name@1.2.3` that one, and the assembly is that
  of the target framework most like the running one's. Nothing is downloaded yet.
- **The prefix** is registered by Z#: `core:zsharp`'s `importers` routes a prefix to a
  function, and a Z# function is a delegate where one is expected.

A Z# function where a CLR delegate is expected is a delegate of it: a delegate type
converts a value that can be referred to as a function into one, and the instruction
that points to the function takes the LIR function, which a CLR platform finds as the
method it declared. A function of no module can't be pointed to in the runtime, which
runs it as a dynamic method.

## Releases and installing

A release is what an install has, for a platform, as an archive that needs nothing else
installed: `release.zs` builds one, `zs release.zs [--no-bootstrap] [platform...]`, each
platform a .NET runtime identifier, as `win-x64`, `linux-x64` or `osx-arm64`, into
`release`, a `.zip` for Windows and a `.tar.gz` elsewhere, which keeps what runs as a
program. It runs the bootstrap first, so the seed is what the sources build.

- **What it has.** The toolchain, published self-contained in `bin`; the standard
  library's sources and builds, its seed; the toolchain's packages, with the versions of
  packages they import, found in NuGet's packages where the release is built; the
  bootstrap, so an install builds itself again from its sources; the design, the
  documentation and the examples; and the installer. A file at its root,
  `.zsharp-toolchain`, says it is a toolchain. `zs` finds the standard library above its
  own folder, so nothing in it names where it is installed.
- **The installer** is Z#, `install.zs`, which the release's own `zs` runs, through
  `install.cmd` on Windows and `install.sh` elsewhere, so it is one installer for every
  platform. `install [folder] [--no-path]` copies the toolchain to the folder, or to the
  user's own programs (`%LOCALAPPDATA%\Programs\ZSharp`, or `~/.local/share/zsharp`),
  and adds its `bin` to the user's PATH: on Windows the user's own variable, and
  elsewhere marked lines in the shell's profiles. `install --uninstall [folder]` removes
  both. A folder is replaced or removed only if it is a toolchain, so a folder of
  anything else is never written over, and the PATH or a profile is written only if it
  changes. What removes the toolchain that runs the uninstaller waits for it to exit,
  as a running program's files can't go, and on Linux what it hasn't loaded yet would
  be gone; a terminal whose folder is the toolchain keeps the folder, empty.
- **Arguments.** What follows a script on the command line is its own, `core:zsharp`'s
  `arguments`, as the release builder's platforms and the installer's folder are.
- **Versions** are a decision to come: an install is one toolchain, in a layout that a
  folder for each version can hold later. A failure, as a folder that can't be written,
  ends the installer with the runtime's exception, as Z# has none yet.

## Project layout

```text
src/
  Common/    CommonZ
  Core/      ZSharp.HIR, ZSharp.MIR, ZSharp.LIR
  HIR/       Language-neutral HIR extensions, such as ZSharp.HIR.Code
  Compiler/  The CG, TS, IR, loader and runner REIs and implementations, and
             ZSharp.Compiler, the compiler in use, which they make up
  Importers/ Language-neutral importers, such as the string importer
  Lang/      Text, Tokenizer, AST, Parser (an extensible parser, no grammar of its
             own), Syntax (Z#'s grammar), AST -> HIR, the Z# language
  Library/   What `core:` and `std:` expose, of what is below them, which nothing
             below reaches
  Packages/  DotNet (CLR entities, pure structure, HIR level)
  Runtime/   IL loader, LIR -> IL
  Toolchain/ ZSharp.Toolchain: the toolchain put together from its parts, and the
             modules of its types Z# imports
  CLI/       The command line, which runs a script with the toolchain
tests/
std/         The standard library: its sources, and its builds, the seed
packages/    The toolchain's packages: the Cecil platform and the package manager
bootstrap.zs Builds std and the packages from their sources
release.zs   Builds a release; install.zs, with install.cmd and install.sh, installs one
docs/        The documentation, every sample of which tests/ZSharp.Tests.Docs runs with zs
```

Each project is designed and built separately. Only the CLI combines them.

`SourceCompiler.*` from `zsv1` needs a redesign and reorganisation.

## Porting from zsv1

| Treatment | Projects |
|-----------|----------|
| Copy almost as is | `CommonZ`, `ZSharp.Text`, `ZSharp.Tokenizer`, `ZSharp.AST`, `ZSharp.Parser` |
| Port with small edits | `Compiler.Core`, `CG`, `CG.Direct`, `CG.Typed`, `TS`, `TS.Static`, `HIR`, `HIR.Code` |
| Rewrite with the old code as reference | `ZSharp.IR`, `Platform.Runtime`, `Importer.ILLoader`, `SourceCompiler.*`, `Interpreter` |
| Leave behind | Evaluator projects, proxy code, `Library.Core.*` (a smaller `core:` surface replaces it) |

The first target is parity with `zsv1`: importing `print` and calling it.
