# Z#

A language you can program at every stage. Its compiler is a library your code calls, and everything it does can be extended from Z# itself: its syntax, its definitions, what it imports and what it compiles to.

## Code that takes part in compiling

In Z#, compiling is something your program does, not something done to it. A
definition is built by running code, so other code can watch it being built and change
it:

```zs
import { print } from "std:io";
import { Function } from "std:functions";
import { Int32 } from "core:runtime";

fun announced(node: Function): Function {
    print("defined " + node.Name + ", which takes " + node.Parameters.Count);
    return node;
}

@announced
fun add(a: Int32, b: Int32): Int32 { return a + b; }

print("calling");
print(add(1, 2));
```

```output
defined add, which takes 2
calling
3
```

`announced` is an ordinary function. It is given `add` as `add` is defined, before
anything calls it.

## What that makes possible

<div class="zs-features">
<div>
<h3>Build scripts are programs</h3>
<p>There is no build system to configure. A build is a Z# script that imports the
compiler, chooses what to compile and where it goes, and runs.</p>
</div>
<div>
<h3>Definitions are yours</h3>
<p>Classes, functions and modules are defined in Z#'s standard library, not built into
the compiler. A library can add kinds of definitions of its own, as first-class as
those.</p>
</div>
<div>
<h3>Import anything</h3>
<p>An import is a function call that gives a description of what it imports. .NET
assemblies are imported that way, and an importer for anything else is a library.</p>
</div>
<div>
<h3>Compile to anything</h3>
<p>What Z# compiles to is a low-level description a platform reads. The toolchain runs
it as it goes, or writes it as a .NET assembly, and a platform for another target is a
library too.</p>
</div>
<div>
<h3>Generate code, reflect on it</h3>
<p>The structure a program generates is the one it reflects on, so code generation and
reflection are one API, at build time and as the program runs.</p>
</div>
<div>
<h3>Z# in Z#</h3>
<p>The standard library, the project compiler, the package manager and the .NET platform
are written in Z#, with the same tools a library of yours has.</p>
</div>
</div>

## Where to start

- [Installation](learn/getting-started/installation.md) and
  [Hello world](learn/getting-started/hello-world.md), to have Z# running.
- [The basics](learn/basics/basics.md), for the language itself: enough for any program.
- [Advanced](learn/advanced/overview.md), for build scripts, code generation and
  reflection.
- The custom guides, for extending a stage of the language:
  [syntax and definitions](learn/custom-syntax/syntax.md),
  [importers](learn/custom-importers/how-imports-work.md),
  [semantics](learn/custom-semantics/hir-to-lir.md),
  [dispatchers](learn/custom-dispatchers/reis.md) and
  [platforms](learn/custom-platforms/overview.md).
- The [reference](reference/index.md), for every library and package.

Every sample in these pages is run by the toolchain's tests, so what a page shows is what
Z# does.
