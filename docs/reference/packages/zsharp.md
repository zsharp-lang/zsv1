# Z# packages

The toolchain's own definitions, under `pkg:zsharp/`: what its languages and its frontend
are made of. They have no state of their own, and the toolchain gives them, so they are
there before the package manager is.

## The core languages

| Package | Is |
|---|---|
| `pkg:zsharp/core` | What every layer shares: `Error`, `ErrorMessage`, `Result`, `IResult`. |
| `pkg:zsharp/hir` | [HIR's core](../core/hir.md). |
| `pkg:zsharp/hir/code` | HIR's code: its expressions, statements, patterns and imports. Code generation and the language's compiler use it; reflection doesn't need it. |
| `pkg:zsharp/mir` | [MIR's core](../core/mir.md), `CompilerObject`. |
| `pkg:zsharp/lir` | [Core LIR](../core/lir.md). |
| `pkg:zsharp/sourcemap` | Where code was written: `SourceMap`, `CodeMaps` of HIR code, `FunctionMaps` of an LIR function, and `SourceMaps` to read and set them. |

## The language

The Z# frontend's parts:

| Package | Is |
|---|---|
| `pkg:zsharp/lang/tokenizer` | `Tokenizer`, `TextStream`: text to tokens. |
| `pkg:zsharp/lang/parser` | `Parser`: tokens to AST, by what is installed in it. |
| `pkg:zsharp/lang/syntax` | `Grammar`: Z#'s syntax, installed in a parser. |
| `pkg:zsharp/lang/ast` | The AST's nodes. |
| `pkg:zsharp/lang/hir` | The language's HIR and helpers for building code: `Bodies`, `Language`, `SyntaxFunctions`, `ContainerCompiler`. |

## The standard definitions' compilers

The build-time halves of the standard definitions: their COs, meta-compilers and loaders.
A program that only reflects on their definitions needs the structure, `std:oop`,
`std:functions` and `std:modules`, and not these.

| Library | Has |
|---|---|
| `std:oop/compiler` | The COs and meta-compilers of Z#'s classes and interfaces. |
| `std:functions/compiler` | The CO and meta-compiler of Z#'s functions, and `premap(declared, to)`, which makes a declared function lower as another. |
| `std:modules/compiler` | The CO and meta-compiler of Z#'s modules, `lower(module, target)`, and `failures(module)`. |

These are under `std:` today, though they aren't standard libraries in the sense the rest
are: they are what a build uses. They are to move to `pkg:`, in the code and in these
pages.
