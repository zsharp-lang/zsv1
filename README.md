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
* [x] ZSRuntime
* [ ] Platform Compiler

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

## ToDo

* [ ] Add support for type-inference
    - [x] Local inference
    - [ ] Function return type
* [ ] Implement the overloading protocol (C#)
* [ ] Implement the `compile` protocol (both `IRObject` and `IR Type`) (C#, Z#)
* [ ] Implement better Z# RT VM API (C#)
    - [ ] Function calls
    - [ ] Object construction, instantiation and initialization
    - [x] IR loading
* [x] Organize IR loading in Z# RT VM (C#)
* [ ] Implement document-level definitions
    - [ ] Functions
    - [ ] Globals
* [ ] Implement OOP types
    - [ ] Class
    - [ ] Interface
    - [ ] Typeclass
    - [ ] Structure
* [ ] Add support for decorators
* [ ] Add support for custom attributes
* [ ] Implement the CG `Execute`/`Evaluate` command.
* [ ] Implement the `import(string)` CG function (C#)
    - [ ] Add support for extensible `StringImporter`
* [ ] Add support for definition templates
    - [ ] Functions
    - [ ] Classes (which includes methods, properties, fields, etc..)
* [x] Fix module references lookup code (consider the fact that IR have 2 sources: `Read` and `EvaluateType`).
    - > The way this works is by analyzing each function code right after the body is compiled.
