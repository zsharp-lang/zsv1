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

HIR describes the structure of definitions and code. Behaviour only makes sense at
compile time, when code is generated and objects interact, so it lives in MIR. That
lets code generation and reflection share one API: a program can find every `ORMEntity`
it defines by iterating its module's definitions and type checking them, instead of
decoding compiled classes, and the same utilities read structure at compile time and
at runtime. This assumes the metadata needed to rebuild the HIR nodes is emitted.

MIR doesn't care about structure. Given the right number of COs, an operation can be
applied without caring what they represent, and it may fail. A CO can come from HIR or
be built in code, and both take part in the CG and TS operations equally. That is what
lets objects interact across definitions, for example generating all the networking
code between a server and a client so that the developer just calls a function.

When a CO finally has to do something useful, it is asked for LIR, without caring how,
and LIR is something that can be understood, inspected and traversed.

An interaction is defined by adding to CG or TS, or by a new REI, such as overloading, or the
planned OOP REI. Only the interaction has the same API. How an object takes
part in it, or overrides it, is up to whoever implements it. Rust-like traits, for
example, need only a new CG dispatcher that implements those endpoints by an external
lookup on the result of `TS.TypeOf`.

Some HIR can't be rebuilt from metadata, such as a reference bound late or an exposed
object. That HIR is temporary: at runtime nothing generates HIR and there is no
evaluation engine, so nothing waits for a value to resolve. An HIR node's shape can
still load lazily, as when `Program as Module` reflects without loading everything,
because that is hidden behind the same API.

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

Code never inspects a CO. It applies an operation such as `CG.Set` and handles a
possible failure: fire and forget. It never asks what a CO is, what it has or what it
implements. Only a dispatcher may ask, and not always (traits don't), because only a
dispatcher decides how to apply an operation to a CO. Even there, checking
`Is<SomeInterface>` restricts which COs can ever take part, so custom semantics would
need wrappers. `Is<T>` is only ever used with an interface, never a class, because
interfaces add capabilities without the problems of inheritance.

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
  turns into jumps easily, while the reverse is hard. `Break` and `Continue` name
  the `Loop` they act on, which need not be the innermost one, so named loops keep
  their structure. WebAssembly branches by depth, JS has labels and the CLR jumps; a
  target with only innermost breaks, such as Python, lowers them with flags.
- Code is an expression tree, and child expressions are evaluated in order. A
  stack machine would suit only the CLR and WebAssembly. A tree is simple for
  every target, and a CO lowers to one node holding its children's nodes.
- Arithmetic, comparison and casts are not nodes. They are calls to function
  handles that each platform maps. Virtual dispatch is not a concept either: it
  is a structure of function references plus `CallIndirect`.
- `Parameter` and `Local` are both `Variable`s, read and written the same way.
  A `Block` declares its locals, in both LIR and `HIR.Code`.
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

An REI is a set of replaceable operations. CG, TS, IR, HIR -> MIR and overloading are
all REIs.

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

IL metadata is already fully resolved, unlike HIR, which has nodes such as `NamedMember`
and `Call`. So the IL loader never calls any CO API.

COs can do anything, so laziness lives there. Reflecting a whole type's members up
front would reach much of the BCL, so a class CO discovers members when asked.
Because a CO holds only LIR, it does not depend on a particular runtime. Another
.NET platform primes the same kind of CO its own way.

LIR has no classes. Loading a `Package.DotNet.Class` that is being defined emits a
specialised LIR node, `ClassDefinition`, owned by `ZSharp.Platform.Runtime` and not
by the LIR core, which that platform handles specially. No other target ever handles
CLR class definitions, so a CO reports an error for one. Once loaded it is an existing
type, reached like any other.

Classes that reach each other load together. Their members are declared before any
code is lowered, so code can refer to any of them, through a `ClassInProgress` CO since
there is no type to reflect on yet. A method is an instance method unless it is marked
static, as the members of a class describe.

Each target decides how it handles a value: a `.NET` field, local or parameter converts
it with `CG.ImplicitCast`, and another target might want an exact type or something else
entirely. `CG.ImplicitCast` is what makes the common case simple while staying optional,
so it isn't built into `CG.Set` or a call.

Assignability goes both ways: the TS dispatcher asks the source whether it goes to the
target (`IAssignableTo`), and the target whether it takes the source
(`IAssignableFrom`). A CLR type says it only goes to other CLR types (`IILType`). A
class still loading is a different CO that says it goes to itself and wherever its base
goes. Neither needs to know what the other is. A class still loading reaches its base's
members by applying the member operation to the receiver seen as the base. A reference to a base type stays as it is, and a value type
becomes an object through a function handle mapped to the `box` instruction, the same
way arithmetic maps to `add`. A body's returns are checked when it is lowered. A local
with no type is typed by a `TypeOf` node, which stands for the `TS.TypeOf` operation.

A `Package.DotNet` node for something that already exists is identity only, and one
being defined lists its members. A C# library is imported through its CLR module.
The members are the types it imports with `ImportType`, the static members of the
class marked `ModuleScope` (C# has no module-level functions or variables) and its
public types. `core:runtime`, `core:compiler` and `std:io` are all loaded this way.
That is how an import finds a library, not what a module is: a module defined in Z#
reaches the CLR as a static class, through LIR.

Generics on the CLR are written as in `zsv1`: a generic type gets its arguments by
indexing it, as in `List[Int32]`, and so does a generic method, as in `f[Int32](x)`.
Indexing a generic type definition instantiates it, and indexing an overload group
applies to each candidate, so the arities a library imports under one name, as
`core:runtime` does with `Func` and `Action`, are overloads that the arguments pick
from. A constructed type is described in HIR as a `GenericInstance` of its definition
and its arguments, and loading one instantiates the definition with what its arguments
stand for. Types are only given explicitly so far; inferring them is for later.

The standard definitions are written in pure Z#, so the CLR definitions come first:
using CLR generics, then interfaces and virtual dispatch, then methods as values, and
then defining generic CLR types and methods. An interface is a type definition with
`of DotNet_Interface`, and `@virtual`, `@abstract` and `@override` come from
`core:runtime`, as `@static` does, so methods aren't virtual unless marked.

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

- `core:runtime`: what is specific to the runtime, such as the platform, its types and
  the CTS HIR.
- `core:compiler`: the compiler, such as CG and TS.
- `core:zsharp`: the language, such as the parser, the binder and the import system.
  Today it has the compiler, `import` and `@export`.

A project compiler, meant to be written in Z#, would add `core:project`, which gives
insight into the project being compiled.

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

HIR deals in the highest-level concepts possible, so these entities are .NET's
concepts, not how CLR metadata encodes them. A constructor is its own entity, not
a method named `.ctor`, and a static constructor is another: each has only what
that concept has. A struct, an interface and a static class are concepts too, not
a class with flags or a base type. Where two things differ only by an attribute,
as static and instance methods do, a property says so.

A meta-type extends the AST compiler. It receives the AST and is responsible for
compiling it to HIR that evaluates to the HIR representing the original definition.
A `fun` inside a CLR class is special only because that meta-type handles it, so
the AST compiler needs no context-dependent handlers.

`ZSharp.Lang.ZSharp.DotNet` holds the CLR class meta-type, the default for `class`.
A definition statement runs its build code once, so a name means one object, and
the object is loaded right after. Casting a type to what describes it, as in
`String as Type` with the HIR marker, is reflection. It gives the type's node, which
is how a field's type gets into the HIR being built.

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

#### Worked example

```text
module Program; // whole file

class Foo of Class { let x: I32; }

fun foo() {
    class Bar : Foo {}

    return fun bar(): Bar {} as Function;
}
```

This compiles to roughly the following build code:

```text
// the module's default meta-type
let m = Module("Program");

// the Class meta-type
let c_Foo = Class("Foo");
let c_Foo_x = Field("x", I32 as Type); // Type is the HIR marker
c_Foo.fields.add(c_Foo_x);

// the Function meta-type, assuming it is the default for fun
let f_foo = Function("foo");

// Bar is not cast, so it is built with what encloses it, not when foo runs
let c_Bar = Class("Bar");
c_Bar.base = c_Foo; // a Local of type Class: the real type doesn't exist yet

// bar is cast, so its build code belongs in foo's body. Its build code is
//     let f_foo_bar = Function("bar");
//     f_foo_bar.returnType = c_Bar;
// and reified, it becomes:
let f_foo_bar = Local();
f_foo.body.add(
    Set(f_foo_bar, Call(Function, [String("bar")])),
    Set(Member(f_foo_bar, "returnType"), Get(c_Bar))
);

f_foo.body.add(Return(f_foo_bar));
```

Since this module is an identity definition at top level, the code is then executed
to get the `Module` object, which is loaded as a CO, compiled to LIR and loaded into
the runtime. Casting the module to `Module` would keep it as structure instead.

An identity definition inside an expression is loaded along with that expression,
but binds no name outside itself:

```text
let x: Foo /* not ok */ = class Foo { let parent: Foo?; /* ok */ }();
```

So reification takes HIR code and produces HIR code that builds it, and it works on
`HIR.Code` nodes alone. A definition needs no reifier of its own, because it already
produces code that produces HIR: a nested definition is just more code in that build
code. A local of the reified code becomes a `Local()` object at the outer level, and
a value of the outer level, such as `c_Bar`, stays a value and is placed by `Get`.

A parent registers its own members. This is the meta-compiler's job, as with
`c_Foo.fields.add` above, so a member never places itself.

A meta-compiler produces a `CompiledDefinition`: the build code and the underlying
type. It is not HIR. Only the definition's handlers and the cast look at it.

A definition that isn't cast is built with whatever builds the code it is in. The
language tracks that block, which a meta-compiler sets with `BuiltBy`. The definition's
handler emits the build code there and returns a `Get` of its variable. That variable
is a value of the outer level, like `c_Bar`, so reification needs nothing else: the
body gets the object by `Get`, and each definition lands on its own level.

In a container, the block is the container's own, so a definition in an expression runs
right before its statement, as plain code. It still can't be used as a type there. At
document level nothing builds the code, so the definition runs and loads right before
its statement, through `Define`, and the statement refers to the result. A
meta-compiler can still override the cast, but a reified body doesn't need to.

A body is reified as a whole. Each local it declares gets a variable in the code that
does the building, so every build has its own, and a type that depends on that level
is a `TypeExpression`. A parameter or `this` is a variable of the building code as
well, which is why its node ends up in the body by value, like `c_Bar`. A definition
cast inside a body is plain code in that body, so it needs no override to get there.

Code doesn't list its locals. A local is a definition, and a `Declare` of it is where it
starts; for a local, that is where it takes its `init`. A `Declare` inside code being
reified declares a variable of that code, so the reifier gives it a variable of its own in
the building code, which creates the local for each build. `CG.Declare` lets a CO decide
what declaring it means, so other definitions can do something else, such as capture
values. A method or constructor lists its locals as structure, filled by its
construction code like its parameters.

`let` and `var` don't choose their meta-type: the container compiles them, through its
fork of the compiler. A method or constructor builds a `Local` in its construction code,
adds it to its locals and binds the name to it. At the `let`'s position, the body gets a
`Declare` that the construction code builds itself, so it is already in the form of the
building code. `Unquote` carries such code: reifying the code around it places it as it
is, so it runs where that code is built and its result goes there. It can't be loaded,
since it only means something inside code that is reified. It is also what a decorator
that injects code, or `@ct`, would produce; evaluating it at compile time, for code that
isn't reified, is not done yet. A class compiles its `let` as a field. A document has no
`let` yet.

A type that is meant to be inferred is `Infer`. It isn't a type: whoever loads the thing
that has it infers the type, such as a local from its `init`, and loading `Infer` as a type
is an error, so it never reaches a CO. Types are values in construction code, so there is
no node for a type given as code.

A name bound to a local's definition can be bound to another one, which is how a `let`
reuses a name. The rule is keyed by the type of what holds the definition, as the merging
of methods into an overload group is.

A variable of the building code is kept as it is through every reification, and is
resolved at the first execution that contains it. What it holds then has to be encoded
into the code being built, and how depends on the type of the value: a string is a
literal, an instance is a `Get` of it, and a dictionary could be something else
entirely. So the reifier has a second map, by the type of a value rather than of a node,
and it is the one source of truth for putting a value into HIR, literals included.

A class has a value at runtime. A Z# class's value is like Python's `Type[T]`: an object
holding the class's and static variables, with class methods as a vtable. Its type has
the shape `M[T]`, where `M` is the meta-type, and it can be cast implicitly to `M`, which
is how reflection works: `Class[Foo]` to `Class`, `ORMEntity[Person]` to `ORMEntity`. A
CLR class's value is its `System.Type`, so `DotNet_Class[T]` maps to `typeof(T)`, and
encoding one gives the class's own node.

A cast definition is regular code, generated from a more declarative node, so it
doesn't capture anything. A parameter in one is treated exactly as a parameter or a
local in any other code. An identity definition can't use a parameter, because the
parameter doesn't exist when the definition is built.

`zsv1`'s separate script, module and class compilers do not carry over. Project
discovery belongs in a library that `build.zs` calls.

### Captures

A function defined inside the code of a method can use the variables around it, by
reference, as in C#. A body's meta-compiler makes a `fun` written directly in it a
`Package.DotNet.LocalFunction`, an HIR type of its own whose node holds its `Capture`s; a
method of a class defined in the body is still a method. Z# functions will have a
`Closure` meta-type of their own, so they can capture with different semantics.

A `Capture` (`HIR.Code`) is a variable that holds the variable it captures. The local
function's meta-compiler resolves a name bound inside the function as usual. Any other
name is resolved as the code around the function resolves it, and if that is a
variable of that code, the function captures it:

- The code around gets a capture of the variable, bound over the variable's name in the
  scope it was bound in, so it and the function both refer to the capture from then on.
  Nothing already compiled is rewritten.
- The variable's node is marked captured (`IsCaptured`, on a local or a parameter),
  because code compiled before the capture still refers to the variable, and may run
  after it.
- A capture found that way is shared: a second function that uses the name finds the
  capture and adds it to its own list. A variable is captured once.
- Resolving through the code around is what makes nesting work. A function two levels
  in asks the middle function, which captures the variable first, and then shares the
  middle function's capture.

Only variables are captured, as `HIR.Variable` marks them: locals and parameters. A
definition or any other instance is referred to as it is.

The same `Capture` loads differently in each place. Outside the functions that capture
it, it is what it captures, loaded as an instance, so in the capturee it is the
variable's own CO, and that CO stores the variable so it can be shared because it is
marked. (A parameter as a definition is what a call matches arguments to, which is why
a capture loads what it captures as an instance.) Code from before and after the capture uses the same storage, so a capture in
a branch needs no special handling. Inside a function that captures it, it is what that
function was given. How a captured variable is stored is up to the platform. On the CLR:

- A marked local lives in a cell, an `LIR.Allocate`d reference made when its declaration
  runs. A marked parameter is copied into a cell when its function starts, and the
  function uses the cell from then on.
- A local function is a function of its own, which takes the cell of each capture as a
  hidden parameter before its own. Calling it passes the cells as the caller has them, so
  calling it from another local function passes on what that one was given.
- A local function used as a value is a delegate bound to the current run's cells, made
  by a runtime helper with expression trees. Its delegate type comes from its CLR types,
  so a function whose types aren't CLR types yet can be called but isn't a value.

A cast definition doesn't capture: its purpose is code generation, so values are
encoded directly into what it builds. That needs no code of its own. A cast definition's
code is reified, and the reifier encodes every variable of the building code by its
value. A capture is a variable, and in the building code, which is the capturee, it
loads as the variable it captures. So a cast definition reading a captured variable
gets the value it had when the definition was built, on that run. A local function is a
variable too, since its value exists only while the capturee runs, so a cast definition
that uses one encodes the delegate bound to that run's cells, as an exposed object.
Calling it calls the local function, which still sees the variable by reference:

```zs
class Cap17 {
    @static fun make(): DotNet_Class {
        let x = "before";
        fun read(): String { return x; }
        let made = class Made17 {
            @static fun direct(): String { return x; }      // "before"
            @static fun through(): String { return read(); } // "after"
        } as DotNet_Class;
        x = "after";
        return made;
    }
}
```

In general, a cast between meta-types succeeds if the target can represent the source.
Casting a capturing function to one that doesn't capture means its captures become
values, which works only where the captured variables have values: in the code that
builds a cast definition. A closure reflected from a delegate captures variables of a
method that isn't building anything, so casting it to a function that doesn't capture
fails, as using a variable outside its code does. There is no syntax for a cast function
yet (`fun() { ... } as Function`), so this is shown with a cast class.

A captured variable stays marked even when only a cast definition reads it through the
capture. That costs only an indirection, and removing the mark is an optimization for
later.

Document-level variables are never captured by reference. A module compiled to a .NET
DLL no longer has the document it was compiled in, so nothing could refer to its
variables. Code built in a document gets a document local's value from when it was
built, capturing or not: a local function that reads one isn't capturing it.

### Compile-time evaluation

Some code runs while compiling: a document's statements, an `of`, a module's meta-types,
a compile-time import source, and later anything marked `@ct`. All of it goes through one
operation: the code is loaded into the runtime as a method and run.

An expression is compile-time if it refers to no `HIR.Variable`: nothing whose value only
exists while code that runs later is running. That is the boundary the reifier already
draws, between what it encodes and what it places as it is, so the check is a walk over
HIR with no MIR in it.

The runtime can't load code by itself: it has to make a method. So what loaded code
needs from its surroundings is put into its LIR as it is made. A document local reads
and writes its document's frame, which its CO refers to as a constant: the frame is an
object of the runtime, and the array of values behind it is read each time the code
runs, since it is replaced when the document gains locals. So any code loaded to run
now can use document locals, wherever it comes from: a document statement, an `of`, a
compile-time import source. Document level is meant to work with the runtime, not as
platform-agnostic structure, so tying it to the process is fine. A method compiled for
keeps never refers to the frame, because code that is built gets a document local's
value encoded instead.

A binding that code needs to work can't be LIR metadata: metadata is optional, and a
target must still produce working code if it ignores it. So what is needed goes in as
a constant, and metadata is for what is optional.

Source mapping is optional, so it is metadata: a CO attaches where its code came from to
the LIR it makes, and the runtime turns it into debugging information. HIR
has no source positions yet; to keep HIR pure, they would live beside it, keyed by node.

Construction code itself only builds HIR and runs imports, so it can be compiled to a
binary, as for precompiled libraries. Encoding a value needs only the mapper's rules,
not the compiler. Exposed objects are the exception, but they are runtime nodes that
only exist in the process that made them, and nothing precompiled contains them.

### Document level and modules

Every definition is compiled by a meta-compiler, and the document is no exception: it
has its own meta-compiler, which executes statement by statement where other
definitions don't. That is a choice of the meta-compiler, not of where the code is, so
a meta-type can also execute statement by statement, even inside a module. As it goes,
the document meta-compiler builds a `Document` object, which is what importing another
document gives, as in `import { foo } from "foo.zs";`.

`DocumentCompiler` is that meta-compiler. It compiles and runs each statement inside its
own context, so code that runs during a statement, such as a decorator, can find the
document. The document's HIR object is `Package.ZSharp.Document`, which holds its path
and what it exports. A document exports only what is marked `@export`, from
`core:zsharp`.

`@export` returns an `Export` object, which holds the definition and an optional alias,
and `@export("name")` exports under an alias: `export("name")` returns a delegate that
makes the `Export`, and a CLR delegate is called through its `Invoke`, as in C#. Any
meta-compiler that registers what a definition's decorators returned can record an
`Export` in its own exports, so exporting doesn't depend on the context stack or on when
the decorator runs. The document unwraps it: it exports the definition under the alias
or its name, and binds its name to the definition itself. A definition with no name can
only be exported with an alias. A meta-compiler that doesn't know `Export`, such as a
module, fails to register it, as a type error. `@export` comes last, since a decorator
outside it receives the `Export`.

`DocumentImporter` is the default importer, so a source with no prefix is a document. A
relative path is relative to the importing document, each document runs once however
often it is imported, and importing a document while it runs is an error. Loading a
`Document` gives a CO whose members are its exports. The document given to the CLI is
run the same way.

A `let` in a document is a local of the document: a `Package.ZSharp.DocumentLocal`, its
own HIR type since it is its own concept, listed in the document's `Locals`. Its type is
known before its statement runs, so a type written for it is evaluated then. The values
of a document's locals are its frame: an array of objects, one element per local. Each
statement compiles to a function that takes the frame, which lets dynamic methods be
used without creating types for them, and lets the same compiled statements run with
another frame. A document local is loaded as a CO that reads and writes its element.

The runner doesn't know about the frame. Running a document is the document
meta-compiler's own job: it runs its own statements (definitions and imports) and asks
an `IDocumentHost` for the rest. The host runs a statement or evaluates an expression
with the document's frame, and gives what is platform-specific: how a type is described
as HIR and how a definition is loaded. An import is code like any other, a call of the
`import` function, so the host evaluates it. The CLR host is
`DotNetDocuments`, which the CLI gives the document meta-compiler.

Every nested statement runs one at a time too, since in the CLI Z# is a scripting
language. So the document meta-compiler runs blocks, `if` and `while` itself: it
evaluates the condition, then compiles and runs each statement of the branch when it is
reached. A statement that is never reached is never compiled. A block has its own names.
A loop's statements are compiled the first time they are reached and run on each
iteration after that, and a block in a loop keeps its names across iterations, so what
the first iteration defined is still defined, and a `let` in a loop is one local. A
`break` or `continue` goes back to the document loop it names. A loop that is an
expression is code inside a statement, so the host runs it whole.

A document local isn't an `HIR.Variable`. A variable has no value until the code it
belongs to runs, later than the code being compiled; a document runs statement by
statement, so by the time anything after a document local is compiled, the local has a
value. Code that is built gets the value it had then: the document side registers a
reifier rule that encodes it, so a method that reads one keeps that value when the
local changes later. It isn't captured by reference either, as document-level variables
never are.

`let` is a definition like any other, and each meta-compiler sees it in its own terms:
a module sees a global, a CLR class a field, a method a local, and a document a local of
the document. `let` and `var` are
the two definitions that don't get to choose their meta-type.

A CLR module's `let` is a variable of the module, and a `fun` in it a function of the
module. Its functions reach both by name. On the CLR the module becomes an LIR module:
its functions are LIR functions and its `let`s LIR globals, which the runtime makes
static methods and static fields, and an initializer gives the globals their values
once the module is loaded. Its functions and variables are declared along with its
classes, and their code lowered along with theirs, so a class and a function of the
module can use each other. A module is defined once, however often it is reached, so
its variables are its own. A module can be an expression, with no name, as in
`module { ... } as Module`, so a function can build one each time it runs. Z# can't
yet load a module node it got as a value and use it as code, the reverse of casting a
module to what describes it. Its local holding the
definition is typed by what it defines, so name conflicts can be resolved by a rule for
that type, much like the typed CG dispatcher.

The document level runs statement by statement, so it has no forward references.
Statement by statement execution with actual type safety rules them out, especially
with meta-types in the picture.

A module is one full statement: one piece of code that produces a `Module` object.
So forward references work inside a module. The limit is that a definition can't be
used as a compile-time value in the module that defines it: a function can't decorate
another definition in its own module, and a type can't be a meta-type there. A
definition in a module is structure until the module is loaded.

Reading that structure is fine, because this code runs while the module object is
being built, which is also when `Foo` is built:

```text
module Program {

    class Foo {}

    print((Foo as Class).name);
}
```

A module's build code is generated in this order:

1. Walk the module body and emit each statement directly. For a definition, emit only
   the construction of its object, which is the declaration of its variable.
2. After the walk, emit the inner fill code of every definition, in order.
3. Then, right before a definition is registered, apply its decorators in order. Each
   decorator's result is the input of the next.
4. If the last decorator's result is a definition, register it in the module.
   Otherwise, emit it directly.

Only the container sees the final output of the decorators. A name keeps meaning the
object that was constructed, so what a definition refers to doesn't depend on source
order. This means external access may differ from an internal reference. The type of a decorator's result is inferred from the decorator's signature,
so the parent knows what it will register while compiling.

This has consequences:

- A definition compiles in two parts: its construction, and its fill. The fill must
  also be *compiled* after the walk, not only emitted after it, because that is what
  puts every name of the module in scope. At document level and in a cast, the two
  parts run together.
- Everything that can refer to another definition belongs to the fill, base classes
  included. That is what makes forward references work: by then every object exists.
- A statement of the module body sees definitions constructed but not filled. In the
  example, `name` is available and `Foo`'s fields are not.
- A statement can only use names from before it, so forward references work inside
  definitions, not between statements of the module body.

Classes use the same flow. Since a class is compiled by its meta-compiler, the flow
is supplied as a service for meta-compilers to use, not as something the compiler
imposes.

Module-level code is plain code, running as part of the construction of the module
object. The same goes for class-level code, at least for `Class`, and a meta-compiler
can do whatever it wants. So a function is the special case: it is the one whose
contents don't run while it is being built, which is why they are reified.

### Definition handling

Compiling a definition, including its contents, is the meta-compiler's responsibility.
A `Function` node in a definition whose `of` is the CLR class compiles directly as a
`Method`, so static is a first-class attribute of it, and a decorator that accepts a
`Method` can mark it static. The point of the meta-type system is that custom definitions
compile and reason about their content in their own terms, not in imposed ones.

Nothing about how a meta-compiler does that is enforced: all that matters is that it
returns the relevant result. A meta-compiler is the only one responsible for compiling
its nodes, so there are no defaults imposed per context either. Whether it iterates over
its nodes by hand or uses existing utilities is its choice, and what can be done is to
give it better utilities. One is forking a compiler: cloning its current endpoints and
changing them as it likes, with no global contamination since the fork is a new object.
Handlers receive the compiler they run in, and only definitions fork.

The Z# language keeps no context of its own. Its capabilities so far are names (where a
name is looked up and bound), declaring locals (where a `let` puts its local) and
building definitions (where a definition found inside code is built). A definition's
context binds its name inside it; a block pushes a context with its own names and
locals, without forking; a container or a body pushes one that builds definitions into
its block. `DocumentCompiler` compiles a document statement by statement, and its
context builds a definition by running it before the statement.

Every definition keeps its own context. A context stack is shared by a compiler and all
its forks, and a meta-compiler pushes its context onto it. A context is a bag of
capabilities that can be checked, such as having an implicit object, registering
members, having a definition variable or having exports, and it can delegate to the
contexts above it. Nothing cares about the specifics of a context. Document-related
behaviour lives only in the document meta-compiler.

A document exports only what is marked `@export`, which returns an `Export` object that
the document records when it registers the definition (see Document level and modules).
The
compiler can supply common compilers and utilities, such as a map-based lookup from a
meta-type to its compiler, or the construction and fill flow, and a meta-compiler uses
them or doesn't.

The definition node handler can construct the local variable holding the definition,
with the meta-type as its type, so that a meta-compiler doesn't have to. A meta-compiler
can change the type of that variable. The meta-type is also used for reflection, which
doesn't support mutation, so it may be a shape that can only be read. A meta-compiler
can change the local to a more concrete type that is also editable.

`ScriptCompiler` in `zsv1` is the reference for how `of` resolves: it is evaluated to
the HIR object of the meta-type, which finds the compiler to call, and that compiler
produces HIR.

`ZSharp.Lang.ZSharp` supplies both utilities. `MetaTypes` is the map from a meta-type
to its compiler. It resolves the meta-type, declares the local and binds the name
inside the definition. `ContainerCompiler` is the construction and fill flow. It
builds the code in the container, so a definition inside an expression is plain code
too. What a meta-compiler returns is a `CompiledDefinition`:
the local, the code that constructs the object, and a function that compiles its fill.

HIR never references the AST. The AST is an input of a meta-compiler, and HIR is its
output. Once a CO is constructed it is a pure handle, with no properties, and everything
goes through external APIs, so details can't be filled in after that. A mapper can apply
a rule while it constructs the CO, such as giving a `this` parameter its owner's type. A
parameter missing a type is otherwise inferred or an error. Complete doesn't mean fully
resolved: `NamedMember` is an HIR node that stands for the `CG.Member` operation.

Default meta-types are what let `class` and `fun` be Z# terms and not .NET terms once Z#
is defined in Z#. A default applies in a context, and an explicit `of` always overrides
it there.

### Decorators

There are statement decorators and expression decorators, and they work the same way.
The function is resolved and called with the decorated HIR object, with type checking.

A definition's decorators run with the code that builds the definition, not at compile
time: after its fill, right before it is registered. The first receives the HIR
object, and each one after it receives the result of the one before. A decorator
shouldn't go too deep, because definitions it refers to may not be filled entirely yet.

For a definition, the result can be another definition. A definition gets the normal
registration code, and anything else is emitted directly. A container registers through
an overloaded `Register`, so overloading decides by what the decorators gave: a
definition goes in its list, and code is emitted, which registers nothing. So a
definition decorated with `@ct` is loaded where it is and not registered, wherever it
is: in a container, in a body, or in a document. A result that is neither, such as an
`Export` in a module, matches no overload, so it is an error while compiling. A decoration is a call that
runs during the construction of the containing definition, so emitting a result means
handing it to that parent: in a reified body it goes into the body, and in plain code
it runs.

A decorator is written `@expression` before a statement, where the expression is names,
member accesses and calls, as in `@api.post("/login")`. Several decorators apply nearest
first, so in `@a @b fun f()`, `b` receives the method and `a` receives what `b` returned.
Definitions, statements and parameters can be decorated; expressions can't yet.
`core:runtime` supplies `@explicitThis` and `@static` for .NET methods and constructors.

A decorated statement that isn't a definition gives its decorators the statement as
HIR: the code that builds its node, reified. An expression statement gives them its
expression. In plain code, such as a document or a module's construction code, the
statement is the call of its decorators, made as the code runs, and what they give is
dropped. In a body, the call is made as the body is built, and the body gets what they
gave in the statement's place: nothing for nothing, code as it is, and any other value
is a statement with no effect, so it is dropped. So `@ct print("Hello")` in a body
compiles to building code that adds what `ct(Call(print, ["Hello"]))` gives.

A parameter's decorators get its node as it is built, and what they give is the
parameter: it is what the function's parameters hold and what its name is bound to, so
a decorator can turn a parameter into another kind, such as an injected one.

A decorator can be defined in Z# as a static method that takes the definition's HIR
node. A decorator that takes arguments, as in `@named("x")`, has to return something
callable that captures them, which needs function values and captures in Z#. Neither
exists yet: a `fun` is always a method of a class. `@export("name")` works because its
factory is C#.

This means a decorator can act as a code transformer. `ct`, from `core:zsharp`, is an
ordinary function: given an expression as HIR it evaluates it and gives the value, given
a statement it runs it and gives nothing, and given a definition it loads it and gives
code that refers to what it loaded: the encoding of its runtime object, a `Get` of it.
`load(def)` is `ct(Get(def))`, which gives the runtime object itself.
As a decorator it is the same function, called as the code around it is constructed;
the decorator handler is what puts the result back into code. A result that is a value
is encoded through the mapper: a constant is wrapped, a type object is reflected on,
and code is its own encoding. More overloads can give code instead of running it, as a
`ct(For)` could give the flattened loop; overloads are chosen by the static type of the
node given, as for any call. Using `@ct` in a definition should be done with care,
because what it reaches may not be filled yet.

### Members of a class

Members are never added to a class that already exists. A class's members are part of
its definition, so its meta-compiler adds them while it builds its own structure, and
that is not an interaction between objects.

A method compiler is part of the class compiler, and it handles references to other
methods of the same class. It knows its own context, so it can check what a name
resolved to.

`this` is not a keyword or a special name. A method has a `this` HIR node, and methods
are implicit `this` by default, with no name bound to it. A free name compiles to
`NamedMember` on that node. Code that needs the object uses `@explicitThis`, which adds
custom metadata to the method. Its mapper, registered for the .NET `Method` node, then
pre-maps that parameter to the CO that `this` maps to, instead of constructing a new one.
The decorator doesn't touch the code.

A constructor call and a base call use the same call syntax, which covers both
`new Foo()` and `base()` in C#. A base call passes the constructor's first parameter
explicitly. The constructor's call handler checks whether that parameter was passed,
so `this` isn't hardcoded.

### Loops

A loop can be named, as in `while outer (condition) { ... }`. `break` and `continue`
act on the innermost loop, or on the one they name with `from`: `break from outer;`,
`continue from outer;`. A loop's `else` runs when its condition stops holding, but not
when a `break` leaves it.

A loop can be a statement or an expression. A loop statement's `else` is optional and
its `break` has no value. A loop expression must have an `else`, which gives its value
when its condition stops holding, and every `break` of it has a value, as in
`break value;` or `break value from outer;`. Its type is that of its `else`, and each
`break` value has to convert to it. This holds for `for` as well, once it exists; `for`
needs an iteration protocol, which is still to be designed.

In HIR, `While` is a statement and `WhileExpression` an expression, and both are a
`Loop`. `Break` and `Continue` refer to their loop's node, so the frontend resolves the
names and HIR never has to. A loop is reified by building its node first, in a variable
of the building code, so a `break` in its body can refer to it. It lowers to
`loop { if (condition) body else { else-part; break } }`, so a `continue` checks the
condition again and a `break` skips the `else`. A loop expression puts that in a
sequence with a local that a `break` or the `else` sets before leaving.

A break or continue can only reach loops of the code it is in: a loop outside the
function it is in is out of reach.

### Operators

Some syntax compiles to a call of a function the language is given, as in `zsv1`.
`Language.Functions` maps a piece of syntax to an HIR `OverloadGroup`: an operator such
as `+` or `<`, and `import`. Each is a group so that more overloads can be added, and
overloading chooses between them. `a + b` compiles to an ordinary call of the `+` group
with both operands. `=`, `.`, `as` and `is` stay syntax, since they aren't calls.

A C# method marked `[Operator("+")]` is an overload of what `+` stands for, and
`ILLoader.Operators` finds them in a type. For now the CLI adds those of `core:runtime`
and `core:zsharp` to one table for the whole language. Scoped tables can come later.
`core:runtime` gives `+ - * / < == !=` on `Int32` and `Double`, `+ == !=` on `String`,
and `or == !=` on `Boolean`.

A whole number is an `Int32` and a number with a fraction is a `Double`, as in C#. They
are `IntegerLiteral` and `DecimalLiteral` in HIR.

### Binding names

Defining a name asks two separate things: what to bind, and how to handle a name that is
already bound. What to bind depends on the context and is decided by whoever defines
the name. Inside a definition it is the definition's temporary local. Outside it, where
the definition is already loaded, as at document level, it is a `Get` of the
definition's HIR node or of a reference to it.

Conflicts go through the binding REI, `Binder`, which belongs to the Z# frontend and works
during AST -> HIR, so there are no COs. That is right: names are a source-level concept, and conflicts only happen
where there are no actual references. Its dispatchers implement behaviour for HIR
without HIR implementing it. There are three cases:

- The name isn't bound yet: bind it.
- The name is bound in the current scope: merge the two if they can be merged,
  otherwise shadow or report an error.
- The name is bound in an outer scope: bind it here, and the new binding may also do
  something with the one it shadows, such as copying its overloads or adding a lookup
  in the parent.

`MappedBinder` implements it with registered merge and shadow rules and a policy for
conflicts nothing merges, which is an error by default. A name bound again to what it
already stands for, as importing it again does, isn't a conflict. The language registers a rule
that a `let` bound over another `let` overrides it, which is how `let` reuses a name.

A definition compiles roughly to a block that declares the local holding its HIR, as
the meta-type builds it, with the inner construction in a nested block. That local is
never overridden. Inside a container, the locals of its definitions live in the same
scope, but binding works exactly the same, so two definitions with one name are an error
unless a rule merges them: methods merge into an overload group, and a rule could merge
classes that differ in their generic signatures, for example. The .NET meta-types register a rule that merges methods into an
HIR `OverloadGroup`, which loads to a CO that applies an operation to each overload and
keeps the one it succeeds on. `Scope` only stores names; every binding goes through the
binder.

### Overloads

In HIR, an overload is pure structure. A class doesn't hold overloads, it holds methods.

A class loaded to a CO builds whatever is convenient for it, such as a map from name to
CO. Methods that share a name collapse into an overload group CO at that point, because a
group is special in code generation and not in the structure of a class.

Where one HIR value has to stand for many and there is no class, as in the operator
table, an HIR `OverloadGroup` can be an `Instance`.

Name-based overloading on the AST side is similar, since it also collapses multiple
values into one object. Member access defers to the CO. A free name uses the same CO,
through the method's `this`.

For .NET meta-types, overloads across a class and its base behave as they do on the CLR.

A group of methods from existing IL can be built directly as a CO while loading, since the
metadata is fully resolved. An overload group in HIR is structure, and the types of its
definitions are known, which is enough because HIR has no behaviour; only loading it
makes a proper CO. A class's overloads are complete when it is loaded, but at document
level more overloads can be added later.

Overloading has nothing to do with calls in particular. It chooses between candidates for
any operation, such as a call or a cast to a function pointer, so each candidate applies
the operation in its own way.

The `Overloading` REI has two entrypoints:

| Entrypoint | Question |
|------------|----------|
| `Apply(candidates, operation)` | Apply an operation to each candidate. Whose result is the result? |
| `IsMoreSpecific(candidate, other)` | When an operation applies to both, is this candidate the better choice? |

The group CO, `OverloadGroup`, is the same for every kind of candidate: HIR overload
groups, IL methods and constructors, and the methods and constructors of a class being
loaded. Each candidate is a CO of its own, and the group implements each operation it
supports as `Apply(candidates, c => CG.<operation>(c, ...))`, so nothing in it is about
calls. A single candidate stands for itself, with no group. For now the group supports
calls; other operations are added the same way.

The static dispatcher chooses the most specific candidate, as C# does. When one candidate
applies, it is the result. When several do, the result is the one that is more specific
than each of the others, and otherwise the call is ambiguous. When none do, each says
why, and the operation is unsupported only if every candidate says it is.

A candidate answers specificity through `ISpecific.IsAsSpecificAs(other)`. Candidates
are never inspected, not even by each other: a candidate with parameters is at least as
specific as another if the other can be called with values of its own parameter types.
So `f(String)` is more specific than `f(Object)`, since `f(Object)` takes a `String` and
`f(String)` doesn't take an `Object`. The values are placeholders that only have a type,
and nothing compiles them. This replaces the separate "exact match" rule IL method groups
had.

These rules are provisional, and nothing should rely much on them:

- CLR methods and constructors are meant to follow C#'s overloading rules. What is here
  is a subset: there are no generics yet, on the CLR or in Z#, and nothing about
  `params`, optional parameters, named arguments or numeric conversions.
- How Z# standard entities overload may change. They use the same rule for now only
  because it is the simplest one; a different rule is a different answer to
  `IsAsSpecificAs`, or a different dispatcher.

### Arguments

Parameters take their values from a call, as in `zsv1`. A call's arguments become an
`ArgumentStream`: positional ones in order, and named ones by name, in any order. Each
parameter, in order, takes what it needs from the stream through `CG.MatchParameter`,
and an argument left over when they are done is one too many. A callable never looks at
its parameters: it asks each one to match, and a candidate whose parameters don't match
doesn't apply, so overloading doesn't change.

A parameter is any `HIR.Code.Parameter`: a variable that takes its value from a call,
with the name the outside sees and a type, which is all that lowering needs to give it a
place in a signature. How it takes its value is its kind's, through its definition, and
as an instance it is the variable of the code it belongs to. A list of parameters only
holds parameters, so nothing else can get into one; a kind whose definition doesn't
match arguments fails at the call. The kinds so far:

- `PositionalParameter` takes only the next positional argument.
- `NamedParameter` takes only the argument with its name.
- `Package.DotNet.Parameter` takes the argument with its name, or else the next
  positional one, as C# does. A CLR method's parameters match this way too, with the
  default values their metadata gives.

Each takes its default value when it gets no argument. Z# writes the first two:

```zs
fun f(a, b, c, {d, e, f})
```

`a`, `b` and `c` are positional only, and `d`, `e` and `f` named only, so a call can't
name a positional parameter or pass a named one by position, which avoids what mixing
does in Python. The arguments of a call can still come in any order. A CLR method
allows both, as C# does.

A parameter can be written `external as internal`: the external name is what a named
argument names and what documents it, and the internal name is what the code uses, so
an API can read well from outside while the code stays convenient. HIR keeps the
external name; only the frontend knows the internal one, as it binds it.

A Z# default value, `x: T = value`, is code, run at each call that leaves the argument
out, so it isn't limited to constants. A Z# method keeps its parameters once its class
is loaded: the IL loader remembers the parameters each method and constructor it
defined was written with, so a call to it matches those rather than its CLR metadata. A
default can't use the other parameters yet.

How a parameter gets its value is its own, so other kinds fit without changing calls.
An injected parameter takes nothing from the stream and gives the value it is injected
with, so it needs no default value standing in for "not given", and callers never see
it. This generalizes default values and dependency injection. A kind is written with a
parameter decorator that gives one, as an `@inject` decorator gives an injected
parameter; the IL loader's `LoadParameter` is what such a kind registers to be its
variable in the code it belongs to. A parameter that takes
the rest of the arguments, `...rest` or `{...rest}`, is another kind: it takes every
positional or every named argument left. It isn't supported yet, because its values
have to be gathered into a collection, which LIR can't build yet.

## Diagnostics

`ZSharp.Diagnostics.HIR` prints any HIR graph, with the common code nodes printed as
code, so reified code reads as the calls it makes. The CLI's `--trace-reification`
prints what each reification receives and produces, so every level can be seen, and
`--dump-definitions` prints each definition a document defines. Z# code can call
`dump` from `core:diagnostics` to print an HIR object.

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

- An import means what the meta-compiler it is in makes it mean, like `let`: where it
  runs and what holds what it imports. A meta-type could record its imports and run
  them in a static constructor. With no meta-compiler that compiles it, an import is
  an error. `Language.BindImported` binds the imported names to whatever holds the
  value, for meta-compilers that want it.
- The current meta-compilers run an import as the definition it is in is
  constructed. A body runs it in the code that builds the body: a local of that code
  holds what it imported, declared in the building block, and nothing of the import
  is left in the body. A name used in the body is reified like any value of the
  building code, so the body gets the imported value encoded. A module declares the
  local in its own construction code. The document runs the import as its statement
  runs.
- So `fun bar(src: String) { return fun() { import { a } from src; print(a); } as
  Function; }` works: the inner function is constructed when `bar` runs, so `src` is
  a value there. The same import directly in `bar`'s body is an error, since while
  `bar` is constructed `src` is its parameter, not a string. Each construction runs
  the import again.
- An import whose source is compile-time, as `Compile-time evaluation` defines it, is
  run while compiling, and its names are bound to what it gave. They are then known
  everywhere: to a meta-type, a decorator, another import's source, and the types of a
  definition. Its arguments are compile-time too, which makes them real parameters of
  the import: fine-grained imports through arguments. Running the source at compile
  time moves its side effects there, as with `of`.
- An import whose source refers to a variable, as in `import { a } from src` where
  `src` is a parameter, can only run as the definition it is in is constructed.
- A document runs as it is compiled, so its imports always run while compiling, and
  their names are bound to what they gave. At
  compile time its names are only an `Instance`, so only code that is built, such as a
  body, can use them. Using one in a type needs a lookup on HIR structure while
  constructing, which isn't there yet.
- An import is a call of the function `import` stands for in `Language.Functions`,
  with the source, then the import's own arguments:
  `import(args..., kwargs...) { names } from source` calls `import(source, args...,
  kwargs...)`. Whatever runs the import runs that call, so a body puts it in its
  building code and the document evaluates it as its statement. `core:zsharp`'s
  `import(String)` is the first overload, and adding overloads lets an import take
  sources other than a string, or arguments. Keyword arguments are named arguments of
  that call. An import has no alias of its own: only the names it brings in can be
  renamed, as in `{ a as b }`.
- Importers route by prefix through the string importer.
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
- Nullable types such as `Foo?`. They are meant to be pure operator overloading, which
  is a future milestone.
- Which `Package.DotNet` entities are needed to fully describe the CTS, including how
  a value type becomes an object.
