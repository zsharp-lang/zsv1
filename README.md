# Z# Language


Welcome to the Z# language repository! This repository holds the first released version of Z#.

Z# is a typesafe, interpreterd language. It is still very much a WIP so don't expect it to work smoothly.

The current state of the project is:
* [x] Tokenizer
* [x] Parser
* [x] Resolver
* [x] CGCompiler
* [x] CGRuntime
* [x] IR Layer
* [ ] ZSRuntime

## Structure

The whole compiler is separated to multiple modules, each with a different purpose.

To understand the architecture of the compiler, one needs to understand the architecture of the language.

The language is interpreted and it allows using statements in top-level scopes (e.g. document, module, etc..).
Also, it is typesafe. The compiler creates an IR of the code and the interpreter holds a runtime representation
of the IR.

The IR defines a static structure, while the runtime objects can vary during execution.

## Running

Simply run the main project (ZSharpTest).

You can edit the `test.zs` file.
