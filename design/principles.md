# Principles and implementation

## Principles

- Nothing intrinsic.
- Z# can extend Z#.
- Interpreted language that uses reflection to compile code.
- As few assumptions as possible.
- As few special cases as possible. Everything flows through general mechanisms.
- HIR is pure structure. All type checking and validation happens in MIR.
- Every component knows only what it truly needs.
- Almost everything is written in Z#. Once that is complete, C# isn't touched again.

A *concept* is an empty C# interface unless stated otherwise.

## Implementation

C# is the implementation language. The CLR is the default target: Emit is used
and the CLR acts as the interpreter runtime. It goes through the same code
paths as any other target would.

## Language independence

Nothing from HIR onwards relies on the Z# language. HIR is not bound to Z#
source: custom importers, themselves written in Z#, can import from any language
and even from binaries, and produce HIR.

The Z# language frontend is simply the one the CLI uses. It happens to have an
import system that allows this, which is why import sources are expressions. So
the contract between a source and everything after it is HIR alone, and any
frontend or importer is a peer of the Z# one.

`HIR.Code` isn't language-neutral: it is oriented to Z#. Each frontend brings its own
HIR libraries. A C frontend would add HIR for structs, functions and typedefs, Python
its own kinds of statements, object model and import system, and CSS its own HIR. That
is why Z# can speak directly with any language, as long as there is an importer for it.
