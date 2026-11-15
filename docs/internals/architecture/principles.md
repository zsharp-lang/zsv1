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

## Levels of abstraction

**Never mix levels of abstraction.** An operation that crosses levels, as M2L does, only
*translates* from one to the other; it doesn't mix them.

- **HIR and LIR are structure,** so they refer only to themselves: HIR to HIR, LIR to LIR.
  A node represents only the properties and relationships of its own level. A function's
  captures are a function-level detail, so if HIR represents them, the function's node
  does, never the local it captures. How a variable is stored is no HIR concern at all.
- **MIR is interactions.** A CO may hold objects of other layers, as long as its
  interactions don't depend on them.
- **A loader realizes a concept as a CO**: a set of capabilities that implement a set of
  interactions. It works at the level of the node it loads. Dealing with a lower level's
  details mixes implementation into the semantics, as a code loader that chooses a cell
  for a variable would; dealing with a higher level's ties that level to this one.
- **Storage is an implementation detail,** the platform's, decided where MIR is lowered.
  A variable could store its value on a piece of paper read and written through a scanner
  and a printer, and nothing above the platform would know.
- **The one exception:** an object proven to exist only in a specific context may carry
  that context. It's allowed, not encouraged, and rare in HIR, which works with slots; it
  is easier to find in LIR.

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
