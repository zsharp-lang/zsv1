# Custom syntax

Everything so far was written in Z#'s syntax. But a library may want syntax of its own,
for what it makes common: a query, a state machine, a grammar. Before changing what can
be written, it helps to know what reads it.

The Z# frontend is two parts: a **parser**, which reads text into an AST, and an **AST
compiler**, which builds HIR from the AST. Both are extensible, and neither is the core's:
to everything after the frontend, only the HIR it gives counts.

This part of the guide is about that frontend: the syntax it reads and the definitions it
builds.

## Following one statement

Take a statement you've written many times:

```zs fragment
let total = price + tax;
```

**The parser reads it.** The text becomes tokens, and the parser reads them by what it
has installed for where it is. Here it is in a document, whose statements it reads by
their first keyword: `let` starts a `let`. The `let`'s value is an expression, which the
parser reads by what starts it, a name, `price`, and then by what continues it, `+`, an
infix operator with its precedence. What it gives is AST: a `let` of `total`, whose value
is `price + tax`.

**The AST compiler compiles it.** The AST compiler is given the `let`, and builds HIR of
it. What a `let` becomes depends on where it is: in a document it is a local of the
document, in a module a global, in a class a field. The AST compiler doesn't decide that
itself: whatever is compiling the code around it does, as [forking the
toolchain](../advanced/build-scripts/toolchain-fork.md) explains. `price + tax` becomes a
call of the functions `+` stands for, with the HIR of each name as its arguments.

From there on, only that HIR counts: nothing after the frontend knows there was a `let`.

## The parser's parts

The parser and its syntax are in the language's packages:

| Package | What it has |
|---|---|
| `pkg:zsharp/lang/tokenizer` | `Tokenizer` and `TextStream`, which make tokens of text |
| `pkg:zsharp/lang/parser` | `Parser`, which reads tokens with what is installed in it |
| `pkg:zsharp/lang/syntax` | `Grammar`, Z#'s own syntax, which installs itself in a parser |
| `pkg:zsharp/lang/ast` | The AST's nodes |

Syntax is installed as the parsers of its parts, as the statement above showed:

- an **expression** is parsed by what starts it, as a literal, a name, `def` or `fun`, and
  by what continues it, as an infix operator with its precedence;
- a **statement** is parsed by its keyword, as `let`, `import` or `while`, and each
  context, as a module's body, a function's or a loop's, has its own keywords;
- what no keyword of a context takes is the context's default, as an expression
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

A frontend of another syntax installs parsers of its own, or brings its own parser: what
it gives the rest of the toolchain is HIR, however it read it.

## The AST compiler's endpoints

`core:zsharp`'s `compiler` is the AST compiler the running code is compiled with. It has
three endpoints, one for each kind of AST node:

| Endpoint | Compiles | To |
|---|---|---|
| `Expression` | an expression | HIR code, an expression |
| `Statement` | a statement | HIR code, a statement |
| `Definition` | a definition | a `CompiledDefinition`: the code that builds its node, and the node's meta-type |

Each endpoint is given the compiler it runs in, which compiles whatever is inside the
node, and each can be replaced. An extension that compiles a node of its own handles that
node, and gives anything else to the endpoint it replaced. A fork of the compiler,
`Fork()`, has the same endpoints, and changing them changes only the fork: that is how
each kind of body compiles a `let` its own way.

## Syntax a document defines

A document is parsed and run statement by statement, so in principle a statement can
change how the statements after it are read. The document's own parser isn't reachable
from Z# yet, so a document can't install syntax for itself today; an extension of the
AST compiler for nodes the parser already reads is what works now.

A new kind of definition doesn't need new syntax: `def`, `fun` and `module` are compiled
by the meta-type a definition is of, so a library adds a kind of definition by adding a
meta-type, which [meta-types](meta-types.md) describes.
