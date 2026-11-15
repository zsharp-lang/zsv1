# Forking the toolchain

A build script and the project it builds run on the same toolchain: the same compiler, the
same operators, the same kinds of definitions. Most of the time that is what you want,
since the project's code means what the script's code means. But it mixes things in a few
ways:

- **The same syntax means different things in different places.** A `let` in a script is
  a local of the script, in a module a global, in a class a field, and in a function a
  local of the function. Whatever compiles a module's body can't compile `let` the way the
  script around it does.
- **What a project installs, the build script gets.** A project's code can add operators,
  kinds of definitions and dispatchers as it is compiled. They are installed for the
  toolchain, so the build script, and every project it compiles after, has them too.
- **A part of the code may need its own rules**, as a sandbox for code you don't trust, or
  a project compiled with a frontend of its own.

What these ask for is a toolchain that starts as the one running, and that one part of the
code can change without the rest seeing it. That is a fork: a toolchain made from another,
which changes what it changes for itself alone.

This page says what forks today, what a fork still shares with the toolchain it came from,
and where the toolchain forks itself.

## What a fork is today

What forks today is the AST compiler, the part of the frontend that builds HIR from what
was written. `core:zsharp`'s `compiler` is the one the running code is compiled with, and
`compiler.Fork()` gives a new compiler with the same three endpoints, `Expression`,
`Statement` and `Definition`. Setting an endpoint of the fork changes only the fork.

The usual way to use one is to keep what the fork had, handle your own case, and give
everything else to what you kept. This is how `std:modules` compiles a module's members,
where a `let` is a global and an import runs as the module is built:

```zs fragment
@public def Members of DotNet_Class {
    @public let fork: ASTCompiler;
    @public let expressions: CompileASTExpression;
    @public let statements: CompileASTStatement;

    @public @explicitThis new(this, self: Local, compiler: ASTCompiler) {
        this.fork = compiler.Fork();
        this.expressions = this.fork.Expression;
        this.statements = this.fork.Statement;
    }

    @public @explicitThis fun expression(this, compiler: ASTCompiler, node: ExpressionSyntax): IResult[Expression, Error] {
        if (node is let local: LetSyntax) return this.global(compiler, local);
        return this.expressions(compiler, node);
    }
    ...
}
```

The fork's endpoints are then set to `expression` and `statement`, and the module's body
is compiled with the fork. Each endpoint is given the compiler it runs in, so whatever is
compiled inside the body is compiled by the fork too, and a fork of the fork, as a class
in the module makes for its own body, starts from it.

A build script compiles with a fork the same way, through the meta-type of what it
compiles: the project compiler compiles a submodule's sources as a module with
`metaTypes.Compile(compiler.Fork(), node, Module as Instance)`.

## What a fork shares

A fork is a fork of the endpoints only. Everything else is the toolchain's, shared by
every fork:

| Part | Shared because |
|---|---|
| **The context** | A fork has the same `Contexts`, the stack of the definitions being compiled. Names are found through it: what a name means is the current context's, not the compiler's, so a fork binds names where its code is, not in a scope of its own. |
| **The operators** | What `+`, `==` and `import` stand for is the language's `Functions`, one table. A definition marked `@operator` is added to it when what holds it is built, for all the code compiled after. |
| **The handlers registered by kind** | What compiles each kind of AST node is registered in the mapped AST compiler's maps, which every compiler's endpoints reach. Registering one is registering it for every fork, those made before included. |
| **The meta-types** | `metaTypes` is one registry: a kind of definition and the default for a keyword are the same for every fork. |
| **The compiler's REIs and loaders** | `cg`, `ts`, `ir`, the HIR loader and the rest are the compiler's, `core:compiler`'s, one each. A dispatcher installed is installed for everything. |
| **The importers and `core:`** | One `importers`, one `files`, one `core`. |

So a fork does more than a copy of a few settings would, since everything inside the code
it compiles is compiled by it, and less than a whole new toolchain, since what it doesn't
fork, it shares.

## Where the toolchain forks itself

Each place a definition compiles its contents its own way is a fork:

- **A document** forks the compiler for each statement, so an import is a statement of the
  document and runs as it is compiled, and a `let` is a local of the document.
- **A body** forks it so a `let` is a local of what is being built, and an import runs in
  the code that builds the body.
- **A module** forks it so a `let` is a global and an import is the module's.
- **A class** forks it so a `let` is a field, and a `fun` or a `new` a method or a
  constructor.
- **A CLR method's code and a local function** fork it for their own locals.
- **The project compiler** forks it to compile a submodule's sources as a module.

## What can't be forked yet

So a fork solves the first problem above, the same syntax meaning different things in
different places, and not the other two. Everything in the table is shared, so a build
can't yet compile part of its code with other operators, other meta-types or other
dispatchers without changing them for all of it, and an extension installed while one
module is compiled is installed for everything compiled after it.
