# Compiler objects

A CO is what the compiler works with. It is opaque: the one thing every CO is, is a
`CompilerObject`, and everything else it can do, it does by implementing a capability, an
interface of the operation it takes part in.

## Capabilities

A CO takes part in an operation by implementing its capability. The common ones:

| To be | Implement | From |
|---|---|---|
| called | `ICTCallable` | `core:zsharp/mir/cg/direct` |
| read as a value | `ICTGet` | `core:zsharp/mir/cg/direct` |
| assigned | `ICTSet` | `core:zsharp/mir/cg/direct` |
| a member's owner | `ICTGetMember`, `ICTSetMember` | `core:zsharp/mir/cg/direct` |
| indexed | `ICTGetIndex`, `ICTSetIndex` | `core:zsharp/mir/cg/direct` |
| cast, or converted implicitly | `ICTCastTo`, `ICTImplicitCastTo`, and from the type's side, `ICTCastFrom`, `ICTImplicitCastFrom` | `core:zsharp/mir/cg/direct` |
| typed | `ITyped` | `core:zsharp/mir/ts` |
| a type others go to, or go from | `IAssignableTo`, `IAssignableFrom` | `core:zsharp/mir/ts` |
| lowered | `ICompileIRExpression`, `ICompileIRStatement`, `ICompileIRType`, `ICompileIRReference`, `ICompileIRDefinitionIn` | `core:zsharp/m2l` |

`CT` is for what is decided as code is compiled. What is decided by a value's type as the
code runs is `CG.Typed`'s, with `RT` capabilities of a type, as `IRTGetMember`, which a
type implements for its values.

A CO implements what it supports and nothing else. What it doesn't implement is
unsupported, an error of its own kind, which the operation can try something else for.

## Each operation gives a result

An operation gives a result: what it made, or why it can't. A CO that is asked something
it supports, with what it can't take, says so with an error, as a call with the wrong
arguments does. So a CO checks what it is given where it is given it, and code that uses a
CO never asks beforehand whether something will work: it does it, and handles the error.

Lowering is the same. A CO lowered for a target with something it can't lower, as a
structure the target has no form of, gives an error. What it doesn't catch, the platform
does.

## What a CO may hold

A CO may hold anything: its node, other COs, objects of other layers, live objects of the
process. What it may not do is depend on another layer in how it interacts with other COs.
A class's CO asks a parameter what it is through the operations, as what a parameter is
referred to as and as its type, not as a parameter of `std:functions`'.

The exception is a CO that only exists for one layer, as a CO of the runtime's types does:
it may use the runtime to decide how it behaves and what it lowers to, since it is never
anything else.

## Rules of thumb

- **Never inspect a CO.** Apply an operation and handle the result. Asking what a CO is,
  or what it implements, restricts what can take part to what you knew of.
- **Never compare COs by identity.** Two objects can stand for one thing. Ask the type
  system, as `ts.IsAssignableTo`, and the COs concerned answer.
- **Forward with `Is`.** A CO that stands for another, as a wrapper does, forwards its
  capabilities by overriding `Is<T>`, rather than by implementing every interface.
- **Lower to core LIR.** A CO that lowers to LIR every platform reads works on every
  platform. One that needs more asks the target for a capability; one that is only ever
  one platform's may lower to that platform's dialect. [Custom
  platforms](../custom-platforms/capabilities.md) says how.
