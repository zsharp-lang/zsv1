# Z# Architecture

This is the record of decisions for the Z# redesign. `zsv1` is the original
source and a reference for intent, not a specification: it is mid-redesign and
parts of it are unfinished. Earlier design material is under `old/` on the `old`
branch and is not used here.

## Principles

- Nothing intrinsic.
- Z# can extend Z#.
- Interpreted language that uses reflection to compile code.
- As few assumptions as possible.
- Every component knows only what it truly needs.

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

## The three cores

The cores are distinct and unconnected. None references another. Layers are
crossed only by external transformers.

| Core | Role                       | Concepts                     |
|------|----------------------------|------------------------------|
| HIR  | Represents structure       | `Definition`, `Instance`, `Type` |
| MIR  | Interactions between objects | `CompilerObject` (CO)      |
| LIR  | Compiling down to a target | Undecided                    |

HIR and LIR are purely structural: nothing in them does anything other than
change itself. Only MIR has behaviour.

### HIR

- `Type` marks a slot where a type is expected, as in `Field.Type: Type`. It
  does not derive from `Instance`. For a type to be an instance, it implements
  both markers.
- `Expression` and `Statement` are extensions of HIR, not core concepts. They
  provide code-like elements.
- A reference is how code refers to an `Instance` (see `Get` in `zsv1`).
- Expressions carry structure, not identity, so they are never cached.

### MIR

`CompilerObject` is carried over from `zsv1`:

```csharp
public interface CompilerObject
{
    public sealed T? As<T>() where T : class
        => Is<T>(out var result) ? result : null;

    public bool Is<T>([NotNullWhen(true)] out T? result) where T : class
        => (result = this as T) is not null;
}
```

Capabilities are reached through `Is<T>`, which an object may override to
forward them.

`Result` and `Error` are not part of MIR, because other layers such as AST -> HIR
use them too. They live in `CommonZ`, and no core depends on them.

- COs are never compared by identity. Code uses the APIs on a CO and acts on
  the result.
- COs never capture toolchain APIs, and protocol methods do not receive a
  `Compiler`.
- Proxies are removed. `Is<T>` forwarding replaces them.

### LIR

To be designed. Settled so far:

- It keeps the idea of `ZSharp.IR` but is general and relatively low-level,
  without the .NET shape (single base classes, for example).
- `Module` is the root of every graph. There are no submodules.
- There is no type system in LIR. A runtime may expose the pre-mapped entities
  it wants, but that has nothing to do with LIR itself.
- Control flow is structured: `Block`, `If`, `Loop`, `Break`, `Continue` and
  `Return`. Python, JS and WebAssembly have no general jumps, and structured code
  turns into jumps easily, while the reverse is hard. `Break` and `Continue` act
  on the innermost loop.
- Code is an expression tree, and child expressions are evaluated in order. A
  stack machine would suit only the CLR and WebAssembly. A tree is simple for
  every target, and a CO lowers to one node holding its children's nodes.
- Arithmetic, comparison and casts are not nodes. They are calls to function
  handles that each platform maps. Virtual dispatch is not a concept either: it
  is a structure of function references plus `CallIndirect`.
- `Parameter` and `Local` are both `Variable`s, read and written the same way.
- LIR is not a form of HIR, although they share a shape. HIR is unresolved and
  open to extension. LIR is resolved and closed, so a target can handle all of
  it. It is the contract between frontends on one side and targets on the other.
- It is mainly a minimal common language for targets to use. Higher-level
  concepts belong in HIR, not in LIR.
- It is target agnostic. Example targets are the web (HTML, CSS, JS and
  WebAssembly together), LLVM, x86, RISC-V and Python.
- A build script invokes targets explicitly, so there is no target API to define.
- The concepts are `Module`, `Function`, `ICallable`, `Signature`, `Global`,
  `Structure`, `Reference`, `FunctionType`, `Type`, `Handle`, metadata, and the
  `Code` of a function body.
- A `Signature` is ordered parameters and an optional return type. With no type
  system, a function that returns nothing has no return type.
- `Module` holds common code and data. The smallest data piece is a simple data
  structure, and layout is inferred from field order.
- Concepts such as a method are metadata attached to a `Function`. A target may
  interpret metadata, but it is not strictly required for functionality.
- `Reference` is a core LIR type that holds its target type. The lowerer (a CO,
  for example) decides the exact type of a field, and so whether it is stored
  inline. It can move out of the core if more targets show it isn't common.
- `Args`, `KwArgs` and full generics move to HIR.
- The LIR core has no generics, and everything in it is concrete. Simple generic
  types, with placeholder markers on type definitions, may be added later as an
  extension. Every use outside a generic definition would have concrete
  arguments, so any target could compile it by substitution.
- MIR -> LIR takes an optional target, so a CO can report an error when the
  target has the wrong shape.
- A CO returns whatever it wants from lowering, and nothing is enforced about
  which LIR entity it returns. Custom COs fully control how custom HIR nodes
  lower.
- An external reference is an LIR handle. Each platform maps a handle however it
  wants, so a platform should be able to map every reference in the `std:`
  library.
- LIR entities may directly reference entities owned by a different `Module`,
  such as a `Global` or `Function`. Handles are only for what has no LIR
  definition at all. Data with no LIR definition is reached through function
  handles, so there is no global handle.
- LIR stays properly shaped. `Call` requires a signature, as `ICallable` does in
  `zsv1`, so anything callable carries a `Signature`, including a handle.
- Primitive types such as `Int32` are handles too, so LIR names no type. To keep
  a handle out of the wrong slot, a handle also implements the marker of what it
  stands for: a type handle is a `Type`, and a function handle is an `ICallable`.

## REI: Replacable Entrypoints Interface

An REI is a set of replaceable operations. CG, TS, IR and HIR -> MIR are all
REIs.

- An REI specifies only the signature of its operations, never their behaviour.
  Behaviour can always be changed, overridden or extended by replacing an
  entrypoint.
- An extension replaces an entrypoint and decides for itself when to delegate.
  It may check its own concepts first and delegate upwards, or the reverse. No
  fallback order is enforced.
- Default entrypoints return a known error type, distinct from other errors, so
  "unsupported" can be matched against a real failure.
- A concrete implementation is used directly only by those who know it. For the
  HIR -> MIR loader, that is the CLI and the CLR extension. Everyone else uses
  the REI.

### CG and TS

- `ZSharp.Compiler.Core` holds what the REIs share, including the known
  `UnsupportedError`. A dispatcher falls through only on that error, so a real
  failure is never replaced by a later dispatcher.
- `CG` and `TS` are plain instance classes and know nothing about a current
  instance. Dispatchers receive the instances they use.
- `CG` and `TS` know nothing about LIR, so `MatchResult` carries no instructions.
- `TS` names no types. An extension or the runtime exposes the ones it wants.
- `CG.Typed` asks `TS.IsAssignableTo` whether a cast is a no-op, rather than
  comparing COs or asking a separate reflection service.

### IR

`IR` is the MIR -> LIR REI. Every entrypoint takes an optional target. Its
entrypoints ask different questions, so they stay separate:

| Entrypoint | Question |
|------------|----------|
| `CompileExpression`, `CompileStatement` | What code does this produce where it is used? |
| `CompileType` | What goes in a type slot? It need not be the definition: a class may define a `Structure` and have a `Reference` to it as its type. |
| `CompileDefinition<T>(object)` | What entity does this define? It places nothing, so a reference can reach an entity before its owner places it. Imported objects have none. |
| `CompileDefinition(object, owner)` | Place this in that owner. The owner need not know what kinds its members are. It returns `null`, or why it was not placed. Being unsupported means there is nothing to place. |
| `CompileReference<T>` | What does code use to refer to this? It may be something the object does not define, such as a handle. |

`zsv1`'s `CompileTypeAs<T>` is not carried over. It existed to require a specific
class-like type, such as a base class, and LIR has no base classes.

### HIR -> MIR

`HIRLoader` is the REI, with three entrypoints, one per concept. Extensions add
their own REI through the same mechanism: `HIRCodeLoader` does this for
`Expression` and `Statement`.

`MappedLoader` and `MappedCodeLoader` are the concrete implementations.
Transformations are registered by exact type. Registering replaces any existing
loader and returns it, so the new one can decide whether to delegate.

The concrete loader may cache Definitions so that one Definition gives one CO,
which can then own its LIR entity. This is the concrete loader's behaviour, not
part of the REI, and an extension may skip it. Instance and Type simply delegate
to the registered loader.

## CLR target

`ZSharp.Platform.Runtime` loads LIR into the running process through
`Reflection.Emit`, and depends only on LIR. Loading declares everything first and
emits bodies afterwards, so modules can refer to each other, including in cycles.
Reaching a member of another module loads that member too.

| LIR | CLR |
|-----|-----|
| `Module` | A static class holding its functions and globals |
| `Function`, `Global` | A static method, a static field |
| `Structure` | A value type, which gives LIR's copy semantics |
| `Reference` | `Box<T>`, because the CLR has no reference that can be stored in a field |
| `FunctionType` | `nint`, called with `calli` |
| `TypeHandle` | A mapped `System.Type` |
| `FunctionHandle` | A mapped method or constructor, or code emitted in place of the call |

Mapping a function handle to emitted code is how arithmetic becomes an `add`
instruction rather than a call.

## Composition

There is no pipeline and no platform as a concept.

```bash
zs build.zs
```

The CLI bootstraps the parser, the Z# source language extension and the runtime,
then runs the script. The compiler is simply a function: `build.zs` imports
compiler utilities and decides what to compile, from selecting files to running
optimisations and generating code.

The toolchain objects are reached from Z# through the `core:` prefix:

```text
import { cg } from "core:compiler";
```

`CG` is an instance, and the pointer to the current one is static. That pointer
lives in the library behind `core:compiler`, not in the API. Z# reaches it by
importing the module and C# reaches it through the library, so both see the same
singleton. There is no scoping: a compilation never needs a
different setup, because a library is compiled when it is installed (not our
responsibility) and only loaded on import. A library can export compile-time
utilities, which can be imported and used at compile time.

## Execution

Parsing and execution both go statement by statement, so an earlier statement
can extend the syntax of later ones.

Compile-time evaluation is an operation, not an API of its own: the Evaluator
service is not carried over. The source layer owns evaluation at HIR level.

## Extensions

Extensions nest without limit, and an extension can have its own extensions.
The built-in language is itself an extension, written in C#.

Every extension worries only about itself, and works with general shapes rather
than concrete types as much as possible. COs for .NET, for example, should work
with any .NET platform, both the runtime and Mono.Cecil.

Z# should be able to reach the parser, the AST compiler and its configuration
(`of`, imports, operators and so on), the string importer, the `std:` and
`core:` importers, the CG, TS and IR APIs, the runtime, the IL loader, and more
as needed. Custom `std:` and `core:` modules can be registered.

## Metaclasses

Meta compilers emit HIR code that, when executed, evaluates to an HIR `Instance`.
This is close to how Python metaclasses work. They emit code rather than the
object so a definition written inside a function constructs a new object on
every call:

```text
fun foo(base: Type) {
    let cls = class A of Class : base {
        let parent: A?; // ok, references itself
    } as Class;

    // let a: A; // error: A is not a real type here

    return cls;
}
```

The `as` cast marking such a definition is not settled.

## Imports

- An import runs at compile time through the string importer, which routes by
  prefix.
- Importers do nothing CO-related. They return real HIR, and imported names
  cross into MIR through the HIR -> MIR REI when used.
- Libraries are supplied by a `pkg:` importer written purely in Z#.
- Checking imported names at the import needs care and is not decided.

## Project layout

```text
src/
  Common/    CommonZ
  Core/      ZSharp.HIR, ZSharp.MIR, ZSharp.LIR
  HIR/       Language-neutral HIR extensions, such as ZSharp.HIR.Code
  Compiler/  CG and TS APIs and implementations
  Lang/      Text, Tokenizer, AST, Parser, AST -> HIR
  Library/   What `core:` and `std:` expose
  Packages/  DotNet (CLR entities, pure structure, HIR level)
  Runtime/   IL loader, LIR -> IL
  CLI/
tests/
```

Each project is designed and built separately. Only the CLI combines them.

`SourceCompiler.*` from `zsv1` needs a redesign and reorganisation.

## Porting from zsv1

| Treatment | Projects |
|-----------|----------|
| Copy almost as is | `CommonZ`, `ZSharp.Text`, `ZSharp.Tokenizer`, `ZSharp.AST`, `ZSharp.Parser` |
| Port with small edits | `Compiler.Core`, `CG`, `CG.Direct`, `CG.Typed`, `TS`, `TS.Static`, `HIR`, `HIR.Code` |
| Rewrite with the old code as reference | `ZSharp.IR`, `Platform.Runtime`, `Importer.ILLoader`, `SourceCompiler.*`, `Interpreter` |
| Leave behind | Evaluator projects, proxy code, `Library.Core.*` (a smaller `core:` surface replaces it) |

The first target is parity with `zsv1`: importing `print` and calling it.

## Open questions

- Exceptions in LIR. The CLR, Python and JS have them, and x86 does not.
- How a reference is tested for null, and what `If` means with no boolean type
  in LIR.
- How an extension is packaged, and what a loadable library consists of.
- Whether the CLI may use anything that Z# cannot reach. The working rule is no.
- Whether a pluggable tokenizer is needed for new token forms.
