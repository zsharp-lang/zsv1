# Platforms

The runtime runs code as it is compiled, and the Cecil platform writes it as a .NET
assembly. Code meant for elsewhere, a browser, a microcontroller, another language's
runtime, needs a platform of its own. In Z#, writing one doesn't mean changing the
compiler: a platform is just what reads the low-level code lowering gives.

To Z#, a platform is nothing in particular. Lowering takes a target, an object, and gives
LIR for it; something then makes that LIR run, or writes it somewhere. That something is a
platform, and Z# asks nothing of it but that it read LIR. So a platform is a library like
any other, and a build chooses which one to lower for.

## What a platform does

A platform is given LIR modules, and realizes them:

- each **structure**, as whatever the platform keeps data in;
- each **function** and its code, as code the platform runs;
- each **global**, and the module's initializer, which runs before anything of the module
  is used;
- each **handle**, a reference to something with no LIR definition, as a .NET type or
  method, which the platform maps to what it has.

How it does each is its own. The same LIR can be run in the process as it is compiled,
written as a .NET assembly, or written as text for another language, and nothing above the
platform knows which.

## The toolchain's platforms

| Platform | What it does | Written in |
|---|---|---|
| **The runtime** | Runs LIR in the process, as it is compiled, with Reflection.Emit. The CLI runs every script on it. | C#, as it has to be there before anything in Z# can run |
| **The Cecil platform**, `pkg:cecil` | Writes LIR as a .NET assembly, with Mono.Cecil, for a version of .NET or the running one, with a PDB of where its code was written. | Z# |

Both are CLR platforms, `ClrPlatform`s of `pkg:clr/lir`: they realize the same LIR, and
the CLR's own LIR, its dialect.

## How a build uses one

A build makes the platform, lowers for it, and gives it what was lowered:

```zs fragment
let target = Cecil("Shop", "net8.0");
target.add(lower(shop, target));
target.write("Shop.dll");
```

`lower(module, target)` passes the target to every CO it lowers, so each can lower for what
the target can do. The platform is only ever an object to them: a CO asks it what it can
do by matching it against a capability, and never asks what it is.

## Writing one

A platform written in Z# is a library that reads LIR, from `pkg:zsharp/lir`, and does
something with it. The pages after this one go through what that takes:

- [LIR](lir.md), the core language every platform reads.
- [Capabilities](capabilities.md), how a platform says what it can do beyond the core, and
  how COs use it.
- [Dialects](dialects.md), LIR of a platform's own, and when a CO may lower to one.
- [Source maps](source-maps.md), where code was written, for a platform's debugging
  information.

The Cecil platform is the toolchain's example of one written in Z#, in `packages/cecil`.
