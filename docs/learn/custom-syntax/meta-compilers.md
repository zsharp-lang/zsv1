# Meta-compilers

A meta-type says what a kind of definition is. Something still has to turn what was
written, a `def`, a `fun` or a `module`, into that kind's node: that is its meta-compiler,
the part of a kind of definition the frontend calls. Given the AST of a definition of its
meta-type, it gives the code that builds the node.

This page's samples are from `std:functions`, whose sources ship in every install, in
`std/functions`.

## Writing one

Definitions refer to each other: a class's method gives the class, two functions call
each other, a field's type is a class defined further down. So a node can't be built all
at once, in the order the definitions are written: when one is filled in, the others it
names have to exist already.

That is why a meta-compiler gives two things, not one. Given the AST of what was written,
it gives code that **makes** the node, which only needs the definition itself, and code
that **fills** it, which runs once every definition around it has been made. Here is
`std:functions`' meta-compiler; `@metaType(Function)` says it is the meta-compiler of
`Function`'s definitions:

```zs fragment
@metaType(Function)
fun compileFunction(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
    if (definition is let function: FunctionSyntax) {
        let filler = Filler(compiler, function, self);

        return Result[CompiledDefinition].Ok(CompiledDefinition(
            self,
            Bodies.Construct(Function as Instance, StringLiteral(function.Name)),
            Function as Instance,
            filler.fill,
            null
        ));
    }

    return Result[CompiledDefinition].Error("A Z# function is compiled from a fun.");
}
```

`Bodies.Construct(Function as Instance, ...)` is the code that makes the node: a
`Function` of the name written. `filler.fill` is the code that fills it, with
`core:zsharp`'s `bodies`: its parameters, its type, and its code, which the language
compiles in a scope where the parameters' names mean the parameters.

## The loader and the CO

The meta-compiler builds the node, and the loader and the CO give it meaning, which
[custom semantics](../custom-semantics/compiler-objects.md) goes through. In short:


A loader is given a node and gives its CO, or why the node isn't valid. It is where a
kind checks what it accepts, since a node can be built as anything:

```zs fragment
@definitionLoader
fun loadFunction(node: Function): IResult[CompilerObject, Error] {
    return Result[CompilerObject].Ok(FunctionObject(node));
}
```

The CO implements the compiler's operations it supports: a function's is called
(`ICTCallable`), referred to as what a call calls (`ICompileIRReference`), and lowered
into the module that holds it (`ICompileIRDefinitionIn`).

## Declaring what a library is

`std:framework`'s decorators say what each function of a library is, and its module
file's `build` turns them into the library's entry point, which importing it calls:

```zs fragment
// src/.zs, the library's module file
import { compile, emit } from "core:project";
import { build } from "std:framework";

emit(build(compile("compiler")));
```

So importing a library registers its meta-types, loaders and binders, and a script that
imports it can write its kind of definition. A kind is the default for a keyword, as
`fun` is for `Function`, when its entry point says so:

```zs fragment
@entrypoint
fun install() { metaTypes.Default[FunctionSyntax](Function as Instance); }
```
