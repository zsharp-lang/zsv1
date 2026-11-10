# How Z# compiles

## The layers

```text
text  →  tokens  →  AST  →  HIR  →  COs  →  LIR  →  a platform's code
```

- **The AST** is what was written, as the parser gives it.
- **HIR** is what was defined: the structure of each definition and its code, which a
  meta-compiler builds from the AST. It is what reflection gives back.
- **COs**, compiler objects, are what the compiler works with. Loading a node gives its
  CO, which knows how it is called, what its members are, and how it lowers.
- **LIR** is what was lowered: code and types any platform can realize, which names
  nothing of any platform.
- **A platform** makes LIR something that runs: the runtime, which runs it in the process
  as it is compiled, or the Cecil platform, which writes a .NET assembly.

A document runs statement by statement: each statement is compiled, lowered and run
before the next is compiled. So what a statement defines is there for the next one, and
code that builds a definition runs as it is compiled.

## Definitions, instances and COs

- A **definition** is what can be reflected on, or has its own semantics: a class, a
  method, a constructor, a field, a parameter.
- An **instance** is what can appear in code, which needn't be a definition, as the object
  a constructor makes.
- A **CO** is only a handle, which the compiler's operations use. A library's COs can
  complement each other, but two things interact through a shared language, never by
  what they are.

## The REIs

The compiler's operations are REIs, sets of replaceable operations: code generation
(`cg`), types (`ts`), lowering (`ir`), loading HIR to COs, and overloading. An REI says
what an operation takes and gives, never what it does. A CO takes part by implementing
the operations it supports, as `ICompileIRExpression` is how it lowers as an expression,
and anything can replace how an operation is done.

So a CO never asks what another CO is: it asks the other through an operation. A class
reaches a parameter as what the parameter is referred to as, and as its type, not as a
parameter of `std:functions`'.

## Lowering, by capability

A CO lowers for a target by what the target can do. A target says it has a capability,
as `Polymorphism`, which lowering a class needs, or `PlatformInterfaces`, which a class
that implements a .NET interface needs. A capability is an interface, and a platform
that exists has it through a trait: an implementation of the capability for the
platform, defined apart from both.

## Building itself

The standard library is built by itself: the build of each library before is what builds
the next, its seed. [The bootstrap](../toolchain/bootstrap.md) builds it in generations,
until a generation builds what it loaded. So a change reaches the seed in one bootstrap
before anything that builds the library uses it, and a change to what built libraries
are made of comes in steps: a transition one bootstrap reads both ways, then the change.
