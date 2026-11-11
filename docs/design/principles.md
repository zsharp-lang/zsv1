# Principles

These are the ideas Z# is built on. The record of each decision, and why it was made, is
the toolchain's design document, [`design/architecture.md`](../../design/architecture.md).

## A language whose definitions are its own

Most languages fix their kinds of definitions: a class, a function and a module are what
the compiler knows. In Z#, a kind of definition is defined in Z#, by a library. `def`
means a Z# class because `std:oop` says so, `fun` a Z# function because `std:functions`
does, and `module` a module because `std:modules` does. A library can define a new kind
the same way, and a definition can say which kind it is: `def Shape of Interface`.

What a kind of definition is, its **meta-type**, says how its definitions are compiled
and what they mean. So a class isn't a thing the compiler has: it is what `std:oop`
compiles `def` to, in Z#.

## Almost everything in Z#

The toolchain, in C#, is kept to what Z# can't do itself yet: the parser, the compiler's
core, and the platforms that turn its output into running code. Everything else, the
standard definitions, the project compiler, the package manager and the Cecil platform
included, is written in Z#. The goal is that almost everything is, and then nothing more
is C#.

So a new runtime for Z# needs only a handful of libraries of its own: the standard
definitions' structures, which are its own kind of class, and what is the platform's.
The rest, written in Z#, comes with it.

## General mechanisms, not special cases

A feature is a mechanism anything can use, not a rule about one case. Overloads are how
any definitions of one name are reached together, so functions and methods share them.
A method bound to an object is any callee with its first argument fixed. A binder that
replaces another binds again what the other bound, for any library that is replaced.

## Define what is allowed

A meta-type handles what it cares about and defines nothing else. What nothing defines
fails on its own, wherever it is used, so there is no check that forbids it. A parameter
in code means what its function defines it as while the function's code is lowered, so
code that uses a parameter its function doesn't have fails because nothing gives it a
meaning, not because something looked for it.

## Structure is just structure

A definition's HIR is its structure: what it is, with nothing it does. A structure can be
one the definition would reject, and that isn't wrong until the definition is used:
until its node is loaded to what the compiler works with, which is when its loader
checks it. A meta-compiler builds what is written, a decorator transforms what was built,
and the loader says whether it is valid.

## Opt-in

What a definition, a library or a platform has, it has because it says so. A target
lowers a class only if it has the capability of polymorphism. A library carries what
reflection reads only if it was built to. An import installs only what the library it
imports declares.
