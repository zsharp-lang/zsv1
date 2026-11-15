# Custom syntax

The Z# frontend is two parts: a **parser**, which reads text into an AST, and an **AST
compiler**, which builds HIR from the AST. Both are extensible, and neither is fixed by
the core: to everything after it, only the HIR the frontend gives counts.

This part of the guide is about that frontend: the syntax it reads and the definitions it
builds. The rest of the toolchain doesn't know it exists.

## The parser

The parser is a Pratt parser with contexts. Its parts are in the language's packages:

| Package | What it has |
|---|---|
| `pkg:zsharp/lang/tokenizer` | `Tokenizer` and `TextStream`, which make tokens of text |
| `pkg:zsharp/lang/parser` | `Parser`, which reads tokens with what is installed in it |
| `pkg:zsharp/lang/syntax` | `Grammar`, Z#'s own syntax, which installs itself in a parser |
| `pkg:zsharp/lang/ast` | The AST's nodes |

Syntax is installed as parsers of its parts:

- an **expression** is parsed by what starts it, a *nud*, as a literal, a name, `def` or
  `fun`, and by what continues it, an infix operator with its precedence;
- a **statement** of a context is parsed by its keyword, as `let`, `import` or `while`
  are, and a context, as a module's body, a function's or a loop's, has its own;
- what no parser of a context takes is the context's default, as an expression
  statement is a document's.

Code that reads Z# sources of its own parses them with a parser that has `Grammar`
installed, as the project compiler does for a project's sources:

```zs fragment
import { Object } from "core:runtime";
import { File } from Object.Assembly;
import { Parser } from "pkg:zsharp/lang/parser";
import { Tokenizer, TextStream } from "pkg:zsharp/lang/tokenizer";
import { Grammar } from "pkg:zsharp/lang/syntax";

let parser = Parser(Tokenizer.Tokenize(TextStream(File.ReadAllText("hello.zs"))));
let statements = Grammar.Parse(parser).Statements;
```

A frontend of another syntax installs its own parsers in a parser, or brings a parser of
its own: what it gives the rest of the toolchain is HIR, however it read it.

## The AST compiler

`core:zsharp`'s `compiler` is the AST compiler the running document is compiled with. It
has three endpoints, one for each kind of AST node:

| Endpoint | Compiles | To |
|---|---|---|
| `Expression` | an expression | HIR code, an expression |
| `Statement` | a statement | HIR code, a statement |
| `Definition` | a definition | a `CompiledDefinition`: the code that builds its node, and the node's meta-type |

Each endpoint is given the compiler it runs in, which compiles anything inside the node,
and an endpoint can be replaced. An extension that compiles a node of its own handles
that node and gives anything else to the endpoint it replaced.

`Fork()` gives a compiler with the same endpoints, whose endpoints can change without
changing the one it was forked from. That is how a definition compiles its contents its
own way: a class's body compiles a `let` as a field, a function's as a local, by forking
the compiler for the body and replacing what compiles a `let` in the fork. Nothing
outside the body sees the change.

## Syntax a document defines

A document is parsed and run statement by statement, so in principle a statement can
change how the statements after it are read. The document's own parser isn't reachable
from Z# yet, so a document can't install syntax for itself today; an extension of the
AST compiler for nodes the parser already reads is what works now.

A new kind of definition doesn't need new syntax: `def`, `fun` and `module` are compiled
by the meta-type a definition is of, so a library adds a kind of definition by adding a
meta-type, which [meta-types](meta-types.md) describes.
