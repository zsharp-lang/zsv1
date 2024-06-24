# Z# Language


Welcome to the Z# language repository! This repository holds the first released version of Z#.

Z# is a typesafe, interpreterd language. It is still very much a WIP so don't expect it to work smoothly.

The current state of the project is:
* [ ] Tokenizer
* [ ] Parser
* [ ] Resolver
* [x] Compiler
* [x] Interpreter
* [x] IR Layer

## Structure

The whole compiler is separated to multiple modules, each with a different purpose.

To understand the architecture of the compiler, one needs to understand the architecture of the language.

The language is interpreted and it allows using statements in top-level scopes (e.g. document, module, etc..).
Also, it is typesafe. The compiler creates an IR of the code and the interpreter holds a runtime representation
of the IR.

The IR defines a static structure, while the runtime objects can vary during execution.

## Running

Since a parser is not yet implemented, it's not yet possible to write textual code and execute it.
In fact, looking at the [main file](https://github.com/zsharp-lang/zsv1/blob/8fca83b37176cca7e7c45fb1974182953ecbccd3/ZSharpTest/Program.cs#L261) will
reveal that (as of now) for testing, we use a prebuilt RAST.

This will hopefully change in the future.

To run the code, just run the main project. This should result with the output:
```
Hello, world!
Hello, foo!
```

## Features
* [ ] Module (WIP)
  * [x] Global
  * [ ] Function (WIP)
* [ ] OOP Types
  * [ ] Class
  * [ ] Interface
  * [ ] Typeclass
  * [ ] Enum
* [ ] Type System (WIP)
* [x] Import Statement
* [ ] Flow Control
  * [ ] If-Else
  * [ ] While-Else
  * [ ] For-Else
  * [ ] Case-Else
* [X] Function Call (mostly working, still WIP)
  * [X] CGFs - Code Generation Functions (only in RT context, such as inside functions)
  * [X] RTFs - Normal functions
* [x] Operators - All operators are compiled as function calls


## ToDo

* Needs to move the runtime module inside the interpreter to allow access from the compiler
* Implement flow control statements
