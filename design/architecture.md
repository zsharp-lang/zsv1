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
change itself. Only MIR has behaviour. That rules out laziness too: a node never
holds a function to resolve later. Part of the reason for HIR's purity is that it
is meant to be usable for runtime reflection in the future.

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

### IL loader

`ZSharp.Importer.ILLoader` describes CLR entities as `Package.DotNet` HIR. The HIR
holds no reflection, so the link back to the CLR stays in the loader: it pre-creates
LIR handles, maps them on the runtime, and remembers which node they belong to. Its
registered HIR -> MIR loaders use that map to return COs primed with those handles.

COs can do anything, so laziness lives there. Reflecting a whole type's members up
front would reach much of the BCL, so a class CO discovers members when asked.
Because a CO holds only LIR, it does not depend on a particular runtime. Another
.NET platform primes the same kind of CO its own way.

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
service is not carried over. `HIRRunner` is the REI for running HIR immediately,
and it knows nothing about where the HIR came from. `MappedRunner` implements it
with registered statement runners, which is how control flow runs and how an
extension adds statements that act at compile time. Anything else is loaded and
handed to functions the CLI supplies, which lower it and run it.

## Z# frontend

Meta-types are a Z# feature. They exist to construct HIR, so it makes sense for
them to be bound to a specific source. A meta compiler takes a context and an AST
node and emits HIR.

`ASTCompiler` is the AST -> HIR REI, and `MappedASTCompiler` registers a compiler
per node type. `ZSharp.Lang.ZSharp` is the built-in language: its `Language` class
and `Syntax` grammar only register things, as any extension would.

### Defining Z# in Z#

The language can be defined in itself. The bootstrap needs only a minimal set of
AST nodes and the CLR metatypes. A CLR Class metatype can define new .NET types, so
it can define new metatypes, the first of which can be `Class`.

CLR Class compiles a function as a CLR method and `let` as a CLR field. Standalone
functions therefore do not exist until `std:oop` and `std:functions` are written in
Z# and register an `AST.Function` handler for module-level and document-level code.

So the minimal bootstrap is the ability to define and express CLR entities, modules
included. At its minimum, Z# is an interpreted language running on the CLR, though
it is not defined that way, so CLR types are imported from `core:runtime`.

`Package.DotNet` supplies HIR that directly represents .NET entities. The runtime
supplies the ability to load them as COs, so they take part in code generation and
in the runtime itself.

A meta-type extends the AST compiler. It receives the AST and is responsible for
compiling it to HIR that evaluates to the HIR representing the original definition.
A `fun` inside a CLR class is special only because that meta-type handles it, so
the AST compiler needs no context-dependent handlers.

The `std:` libraries are written separately in Z#, as a proof of concept, and are
compiled to DLLs or a custom file format that the CLI loads dynamically.

### Definitions

`class`, `module` and `fun` are all definitions sent to a builder, so there is one
path for them. Every `AST.Definition` compiles to code that produces the definition
object as HIR.

A definition that is not cast has identity. Casting a definition node to its
underlying type is code generation: the object is structure only and can't be used
as a type. Python builds a class anew on each call and C# gives it one identity. Z#
wants both.

Casting an existing type to its underlying type is reflection, which is an ordinary
cast. Code generation can construct entire modules, and they aren't usable until
they are explicitly loaded through the HIR -> MIR -> LIR -> platform chain to be
used. This is essentially how a build script works.

The cast handler is the one accepted exception to definitions being invisible to
the compiler: it checks whether its operand is a definition. So the definition AST
compiler returns HIR that also contains the underlying type. Decorators are a
possible alternative. A decorator is a function call that receives its operand as
an HIR object, right after the object is constructed, and could apply to expressions
and statements as well as definitions, like typed macros.

Timing is relative, not compile time versus runtime. A definition that is not cast
is built at the same time as whatever encloses it. A top-level identity definition
is then executed, loaded as a CO, lowered and loaded into the runtime. A definition
inside another definition's body has one identity per enclosing object.

All statements at module level, including definition statements, are compiled and
executed when the module object is built, by the HIR code that constructs it. The
same goes for class level, at least for `Class`.

A definition always binds its name inside itself. A definition statement also binds
it in the enclosing scope, but a definition expression does not.

### Reification

Reification is code that builds code. It works on HIR through an REI, because it is
about describing structure as code and COs are virtually structureless. A cast
definition inside a body must happen when that body runs, so instead of constructing
the object, the enclosing code adds HIR to the body that will construct it. Reference to an existing definition object
becomes a direct `Get` of that object, not a name. Reification nests, and repeats a
small set of `HIR.Code` nodes such as `Call`, `Argument` and constants. A definition
reifies once, so further casts do nothing.

A definition that captures a parameter must be cast, because the parameter does not
exist when an identity definition is built. `zsv1`'s separate script, module and class compilers do not carry
over. Project discovery belongs in a library that `build.zs` calls.

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
  Compiler/  The CG, TS, IR, loader and runner REIs and implementations
  Importers/ Language-neutral importers, such as the string importer
  Lang/      Text, Tokenizer, AST, Parser, AST -> HIR, the Z# language
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
