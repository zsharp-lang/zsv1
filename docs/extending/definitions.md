# New kinds of definitions

A kind of definition is a meta-type, defined by a library, as Z#'s own are: `std:oop`'s
classes, `std:functions`' functions and `std:modules`' modules. These pages' samples are
from `std:functions`, whose sources ship in every install, in `std/functions`.

## The parts of a kind

| Part | What it is | In std:functions |
|---|---|---|
| The node | The definition's structure, which reflection gives. | `Function`, and `Positional` and `Named` for its parameters |
| The meta-compiler | Builds the node from what is written, as code that runs. | `compileFunction` |
| The loader | Loads a node to its CO, checking it is valid. | `loadFunction` |
| The CO | What the compiler works with: how it is called, and how it lowers. | `FunctionObject` |

A library keeps its nodes in a structure library, `std:functions`, and the rest in its
compiler, `std:functions/compiler`, which importing the structure installs.

## The node

A node is a class of the platform's, plain structure: fields, and what the toolchain's
interfaces ask of it, as a name.

```zs fragment
def Function of Node : Instance, Definition, NamedDefinition, Annotated, Overloadable {
    let name: String;
    let Parameters: List[Definition];
    let Locals: List[Local];
    let ReturnType: Type;
    let Body: Code;
    let annotations: List[Object];
    ...
}
```

## The meta-compiler

A meta-compiler is given the AST of what is written, and gives code that builds the
node: how to make it, and what fills it once every definition around it exists, so
definitions can refer to each other. `@metaType(Function)` says it is the meta-compiler
of `Function`'s definitions:

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

The filler builds the rest of the node with `core:zsharp`'s `bodies`: its parameters, its
type, and its code, which the language compiles in a scope where the parameters' names
mean the parameters.

## The loader and the CO

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
