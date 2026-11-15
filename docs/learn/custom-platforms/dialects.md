# Dialects

LIR can be extended, as every language of Z# can, and an extension that only some
platforms read is a dialect. The CLR has one, `pkg:clr/lir`: handles of .NET's types and
methods, `ClrCode` of IL a function holds as it is, references by address, class
definitions, and the metadata `Polymorphic`, `MethodOf`, `CastDown` and `TypeTest`. A
`ClrPlatform`, as the runtime and the Cecil platform are, realizes it.

## What may refer to what

The same rule holds within LIR as between languages, which never refer to each other:

- **Core LIR refers only to core LIR.** Every platform can realize it.
- **A dialect's LIR refers to its own dialect, or to core LIR.** Only the dialect's
  platforms can realize it.

So LIR that uses a dialect is that dialect's, and lowering that produces it ties what it
lowered to the dialect's platforms.

## When a CO may lower to one

Lowering to a dialect means supporting fewer platforms, so a CO lowers to core LIR
whenever it can, and asks for a capability when it needs more. A CO lowers to a dialect
only when what it is can only be realized there: the CO of a .NET method lowers to a
handle of that method, since nothing but a CLR platform has it; a class defined as a CLR
class, `def Name of Class` with `pkg:clr/hir`'s `Class`, is a CLR class definition and
nothing else.

Such a CO is scoped to its platform, and may use what it knows of it. A CO that isn't
scoped to a platform never lowers to its dialect.

## Constants

A `Constant` holds a value, and what values a platform can write is its own. A number, a
string or a truth value any platform can; an object only the running process has, only
the runtime can, which writes it where code reads it. So a constant of a live object is,
in effect, LIR of the runtime's dialect: an assembly can't hold it, and the Cecil platform
refuses it with "A value only the running process has, a ..., can't be written in an
assembly".

That is the platform catching it. A CO that lowers to a constant of a live object makes
code that only runs where it was made, which is fine for code that runs as it is compiled,
and wrong for code meant to be built.

## Writing a platform with a dialect

A platform that has nodes of its own defines them as a library of LIR, as `pkg:clr/lir`
is, and reads them along with core LIR. What lowers to them is a CO scoped to the
platform, or a capability's implementation for it, as `std:zsharp/clr`'s marks the CLR's
metadata. A pure capability never needs the dialect: only what implements it for the
platform does.
