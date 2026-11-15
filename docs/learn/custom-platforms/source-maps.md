# Source maps

Where code was written is carried from the parser to the platform, so a platform can write
debugging information: a stack trace that names a `.zs` file and its line, a debugger that
steps through Z#. None of the languages knows about it. It is an annotation of what holds
code, in a library of its own, `pkg:zsharp/sourcemap`.

## The chain

1. **The parser** records where each AST node was written.
2. **The AST compiler** puts each statement's place in the source maps of the code it is
   compiled into: a `Code`'s `CodeMaps`, an annotation of the code, from statement to
   `SourceMap`. The code a definition builds takes its maps along, so code built by
   reification still knows where it was written.
3. **Lowering** puts each lowered statement's map in its function's `FunctionMaps`, which
   is the LIR function's metadata.
4. **The platform** reads a function's maps as it writes the function's code.

A `SourceMap` is a document's path and where in it a statement starts and ends, by line
and column. HIR and LIR nodes have no field for it: the maps are beside the code, as what
holds the code says, so any frontend can map its own source its own way, and a platform
that doesn't care never sees them.

## What a platform does with them

```zs fragment
import { SourceMaps, FunctionMaps } from "pkg:zsharp/sourcemap";

let maps = FunctionMaps.Of(function);
if (maps != null) {
    // For each statement it writes, maps[statement] is where it was written, or null.
}
```

The Cecil platform starts a sequence point at the first instruction of each statement
that has a map, and writes a portable PDB into the assembly it writes, with each source's
SHA-256 checksum and each method's locals by name. So nothing goes beside the assembly,
and a stack trace through it shows each source's file and line. Visual Studio debugs Z#
through it.

The runtime writes no debugging information: a module Reflection.Emit makes in the
running process can't have any.

## What isn't mapped yet

- Only statements are mapped. An expression, a loop's condition and a statement's parts
  have no places of their own.
- A definition's own place, as a language server's go-to-definition would want, isn't
  carried yet.
- A document's language isn't written in the PDB: Mono.Cecil writes only languages it
  knows.
