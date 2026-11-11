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
  `Structure`, `Reference`, `GenericInstance`, `InstanceMember`, `MethodInstance`,
  `FunctionType`, `Type`, `Handle`, `Initialized`, metadata, and the `Code` of a
  function body.
- A `Signature` is ordered parameters and an optional return type. With no type
  system, a function that returns nothing has no return type.
- `Module` holds common code and data. The smallest data piece is a simple data
  structure, and layout is inferred from field order.
- Concepts such as a method are metadata attached to a `Function`. A target may
  interpret metadata, but it is not strictly required for functionality.
- `Initialized` is a capability of a definition: code that runs once to initialize it,
  before any of it is used, which whatever is lowered into it can add to, as a global its
  initial value. A `Module` has it. Every target initializes what it loads, as the CLR's
  type initializers, a JS module's body or LLVM's global constructors, so it is a core
  concept. The CLR runtime runs a module's initializer as the type initializer of the
  holder its globals are declared in, which it declares together.
- `MethodInstance` is a generic function given LIR type arguments, as `GenericInstance`
  is for a type: the platform makes the instance, so its arguments may be types it is
  still building.
- `Reference` is a core LIR type that holds its target type. The lowerer (a CO,
  for example) decides the exact type of a field, and so whether it is stored
  inline. It can move out of the core if more targets show it isn't common.
- `GenericInstance` is a generic definition given its type arguments, both LIR types.
  The definition can be any type that takes arguments, as a handle to one a platform
  has, so it is lowered once however many instances there are, and the platform makes
  each instance. `InstanceMember` is a member of the definition called on an instance,
  with the member's signature as the instance has it. LIR says nothing of what makes a
  definition generic: that, with constraints and variance, stays in HIR.
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
`Reflection.Emit`, and depends only on LIR and the CLR handles. It realizes what it
reaches as it reaches it: a class definition with the classes its declarations reach,
created once the outermost class load is complete, as a class reached by another's code
may be what that class derives from. Loading declares everything first and
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
| The CLR handles | What they describe, in the running process |

Mapping a function handle to emitted code is how a platform can make its own handle
an instruction rather than a call. The CLR's own entities don't go through it.

### CLR handles

CLR platforms share a shape, the CTS and IL, so `ZSharp.LIR.CLR` describes it as LIR
handles every CLR platform realizes, which the IL loader makes instead of telling one
runtime what its handles mean:

- `ClrType`, `ClrMethod` and `ClrField` stand for an entity of the CTS by data only: the
  assembly and module that define it and its metadata token, and its names. A platform
  of the running process finds it by its token, among its declaring type's members,
  which a generic instance's members share with its definition's. A platform targeting
  other assemblies, as another version's reference assemblies, finds it by its names, as
  a type the reference assemblies forward. A method the runtime makes up, as an array's,
  has no token, so it is found by its name and its declared parameter types.
- A generic instance is `LIR.GenericInstance` of a `ClrType`; arrays, pointers, by-ref
  types and type parameters are types of their own.
- `ClrCode` is a function whose code is IL: instructions by the value of their opcodes,
  which every platform shares, with operands that are numbers, strings, LIR types, CLR
  methods and fields, methods of classes being defined, or its own locals. Boxing, field
  access, casts, type tests, comparisons, conversions, arrays, delegates and `typeof`
  are all `ClrCode`. ECMA-335 makes them uniform: `unbox.any` casts a reference type as
  `castclass` does, and `box` does nothing to a reference.
- The CLR's own LIR, a class definition and its members, and the address of a variable,
  are in it too, as only CLR platforms realize them. A module that defines a class, as a
  Z# module holding a CLR class does, is marked `DefinesClass`, so a platform that writes
  the module writes the class with it; LIR modules have no classes of their own. Something only a CLR platform can
  lower lowers for a target that is a `ClrPlatform`.
- `Reflected` describes reflection's objects as these handles, each entity once.
- Live values stay the running process's: a document's frame is a constant of the
  runtime. A platform that writes an assembly can't map one, and has no need to reject
  it: nothing defines it.

### IL loader

`ZSharp.Importer.ILLoader` is the CLR's meta-type, for what exists and for what is
defined in Z#. It describes existing CLR entities as `Package.DotNet` HIR, which holds
no reflection, so the link back to the CLR stays in the loader, which remembers which
node each entity is. Its registered HIR -> MIR loaders give COs of them, which lower to
the CLR handles of **CLR handles**, data every CLR platform realizes.

IL metadata is already fully resolved, unlike HIR, which has nodes such as `NamedMember`
and `Call`. So the IL loader never calls any CO API.

COs can do anything, so laziness lives there. Reflecting a whole type's members up
front would reach much of the BCL, so a class CO discovers members when asked.
Nothing a CO gives depends on a platform: what it lowers to is lowered for the target it
is asked for, as a cast down, a type test or `null` lowers its type then.

A class, interface or module defined in Z# with the CLR's meta-type is a definition, as
a Z# definition is, not a type of the running process:

- Its CO, `DefinedClass` or `DefinedModule`, stays its CO. Its members are what its HIR
  says, and a call of one finds the member as the target it is lowered for has it.
- It is lowered once for each CLR platform it is used on, to that platform's LIR: a
  `ClassDefinition` of `ZSharp.LIR.CLR`, or an LIR module whose initializer gives its
  globals their values. A platform that isn't a CLR one can't lower it.
- Lowering one for a target declares it, its members and the types of their signatures,
  and lowers its code once whatever it reached is declared too, so definitions that
  reach each other, through their signatures or their code, are lowered together, by the
  first one asked for. A module lowers its classes with it.
- A parameter is one variable of its code for every target, which each target's lowering
  gives its LIR parameter: what refers to it, as a local's initial value, is loaded
  once, as a local is a definition, so it is lowered for whichever target lowers it.
- A delegate value is called as its `Invoke` is, whether its type is a CLR type or an
  instance of one over classes defined in Z#.
- A generic class given its arguments is an instance of it: its members are typed with
  its arguments in place of its type parameters, and called on the instance of the class
  each target has, as LIR `InstanceMember`s. A generic method given its arguments is a
  `MethodInstance` of the method each target has.
- A method is a value, a delegate of its own signature, when every type in it is the
  CLR's.
- A value of a class or interface defined in Z# is matched against a type and cast down
  to one as a CLR reference is, `isinst` and a cast, with both types as the target lowers
  them.
- A document runs in the runtime, so a definition a document defines is lowered for the
  runtime and loaded into it as it is defined, which is when what it accepts is checked.
  Anything else is lowered for each platform as code using it is.
- A local function's captures are the running process's cells, so a local function is
  lowered for the runtime only.

A method is an instance method unless it is marked static, as the members of a class
describe.

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
then defining generic CLR types and methods.

An interface is a type definition with `of DotNet_Interface`, so it needs no keyword;
one can be added later, in Z#. Its methods have no body, written `fun f(): T;`, and its
bases are interfaces it extends. A class's bases are sorted as it is constructed, once
they are built: an interface is one it implements, and anything else is its one base
class. `@virtual`, `@abstract` and `@override` come from `core:runtime`, as `@static`
does, so methods aren't virtual unless marked, as in C#. On the CLR, the IL loader
defines an interface through a class-shaped view of it, which the class machinery
handles, and maps the type back to the interface's node. A method that implements an
interface method by its name is made virtual and final, as C# makes it, so the CLR maps
it; interfaces are created before the classes that implement them. An interface that
already exists, as one of the CLR's, isn't defined again. Calls on an instance already
go through `callvirt`, so dispatch needs nothing more. Properties, explicit
implementations and default interface methods aren't supported yet.

A CLR method is a value: a delegate. With nothing asking for a type, it is a delegate of
its own signature, a `Func` or an `Action`; where a delegate of another type is expected,
or it is cast to one, it is a delegate of that type if its signature fits. Casting an
overload group is applied to each candidate, so the delegate type picks the overload, as
in `Which as Func[Boolean, String]`. A method reached through an object is bound to it,
and a virtual one through the object, so the delegate calls what the object overrides.
On the CLR, the runtime makes the delegate from the method's pointer. Methods of a class
or module still being loaded, and local functions, aren't made this way yet: a local
function still becomes a delegate through expression trees.

A generic CLR class is written `def Box[T] { let value: T; }`. Its type parameters are
`Package.DotNet.GenericParameter`s, bound inside the class before its members are
compiled, so they are types like any other there, and unknown outside it. On the CLR,
the class's type builder defines them, and the class's own code refers to its members
through the class instantiated over its own parameters, as C# emits it. Once loaded it
is an ordinary generic CLR type, so `Box[Int32](5)` instantiates it as any other.
A generic method is written `fun id[T](value: T): T`, and its type parameters are bound
before its parameters, whose types use them. On the CLR the method is defined before its
signature, which uses its type parameters. Once its class is loaded, `Ids.id[Int32](7)`
instantiates it as any CLR generic method.

Constraints, boxing a value of a type parameter, inferring type arguments, and using a
generic class or method written in Z# while its class is still being loaded aren't
supported yet.

A generic CLR type can be given a type that isn't a CLR type yet, as a class still being
loaded with the code that uses it: `List[Item]` in the module that defines `Item`. Such an
instance isn't a CLR entity reflection can describe, so its CO describes it from the
definition, with its type arguments in place of the definition's type parameters, and it
lowers to an `LIR.GenericInstance` of the definition's handle, its members to
`LIR.InstanceMember`s. The runtime makes the instance as it emits the code, and finds each
member through `TypeBuilder.GetMethod`/`GetConstructor` while an argument is still being
built, or through the member's handle once it exists. Which of the two is the runtime's
business: the IL loader never asks. Its constructors, methods, properties and indexers are
reached this way; its generic methods and its conversions to the interfaces it implements
aren't yet. A generic instance over CLR types is a CLR entity, and reflection describes it
as any other.

The IL loader asks the runtime for an operation on a type by the type's LIR form, so one
function serves a CLR type and a type that isn't one yet: a cast down, a type test, a
`null`, and a delegate of a method of a class still being loaded are each found as the
code is emitted. Only what needs a CLR type to answer, as whether a value type can be
null, asks for one.

Any CLR entity is exposed as C# sees it, so the rest of the runtime integration follows
C#'s rules rather than new ones:

- A value type's instance methods take their receiver by address, as C# calls them, so
  the runtime records which methods do and emits the receiver's address and a `call`. A
  method a value type inherits from a class gets the receiver boxed, through
  `CG.ImplicitCast`. Calling a value type with no arguments, where it has no such
  constructor, is its default value.
- A constant field, as an enum's member, is its value. A type's nested types are its
  members. Static fields and properties can be assigned.
- An indexer is called by indexing, as in `list[0]`. Its getters and setters are methods,
  so overloading picks between indexers. Indexing gives an element that reads through the
  getter and assigns through the setter with the same arguments.
- An operator also stands for what the operands' CLR types define by the name C# gives
  it, as `op_Addition` for `+`. `ILLoader.ClrOperators` gives a
  `Package.DotNet.Operator` overload for each operator, which finds those methods when
  called. So `String` compares with its own `==`, and `TimeSpan`s add. It also has the
  comparisons C# predefines for types that define none: enums compare by their values,
  and references are `==` if they are one object, which applies only to references and
  `null`, never to a boxed value. What a type defines is more specific than either.
- Conversions are C#'s: a number converts implicitly to a numeric type that keeps its
  value, as `Int32` to `Double`, and a cast converts to any numeric type, through function
  handles mapped to `conv` instructions. A type's `op_Implicit` is an implicit conversion,
  and its `op_Explicit` is one a cast also has. Overloading still picks the most specific
  candidate, so `Math.Abs(1)` calls the `Int32` overload.
- `null` is a `NullLiteral` in HIR. It has no type, and goes to any type that can be null,
  as in C#. A cast also goes down, to a type that derives from the value's or between an
  interface and a type that isn't sealed, and an object cast to a value type is unboxed.
  A cast that fails throws as the code runs, as C#'s `(T)x` does.
- `value is of T` is a `Match` in HIR, which stands for the `CG.Match` operation, with
  the type as its pattern. A CLR type answers it through `IRTTypeMatch`: a reference is
  checked as the code runs, so `null` is of no type, and a value of a value type is
  known to be of a type or not.
- The part between `is` and `of` is a pattern. Patterns will be several things,
  deconstruction `{}`, arrays `[]`, tuples `()` and names, but only names exist so far,
  and how they work isn't settled. For now `value is name of T` defines `name` as a local
  of `T`, through whatever compiles a `let` where the match is, so it is a variable like
  any other: it can be read and assigned after the match, in a document, a method or a
  function. It is assigned the value, cast to `T`, only if the value matched, and the
  value is evaluated once. `_` names nothing.
- A `params` parameter takes the rest of the positional arguments, through the argument
  stream as any parameter takes what it needs: one argument that is already an array of
  its type is the array, as C#'s normal form has it, and otherwise each is converted to an
  element, and an array of them is made through a function handle mapped to `newarr`.
- `Ref[T]` and `Out[T]` are the types of references to variables of `T`, which `ref` and
  `out` parameters take. They look like generic types but aren't: `Ref` and `Out` are
  `Package.DotNet.RefFamily` nodes, which `core:runtime` exports, and indexing one gives a
  `RefType`, which the CLR has as `T&` either way. A static readonly field of a library's
  module scope that holds HIR is exported as that HIR, which is how `core:runtime` gives
  them.
- A variable of exactly `T` goes to `Ref[T]` or `Out[T]`, so nothing marks an argument
  passed by reference. A local, parameter, global, field or captured variable is passed by
  its address, through `AddressOf`, a node of the runtime. One with no address, as a
  document local or a property, is passed through a temporary that the call assigns back
  to it once it returns, as VB passes a property by reference. A value that isn't a
  variable isn't passed by reference. A block that ends in a variable, as a `let` does,
  is that variable once its statements have run, so `Int32.TryParse(text, let n: Int32)`
  declares `n` and passes it, in a document as in a method.
- A CLR method written in Z# takes a reference with a parameter of type `Ref[T]` or
  `Out[T]`, which describes itself as a `RefType`, as a class describes itself as its
  node. To the method's code such a parameter is a variable of `T`: reading it reads what
  the reference refers to and assigning it assigns that, through function handles mapped
  to `ldobj` and `stobj`, and passing it on by reference passes the reference it holds.
  `Ref` and `Out` exist only for the CLR. The runtime doesn't mark parameters `[Out]` yet,
  so to C# an `Out` parameter looks like a `ref` one; the signature is the same.
- An event is what the CLR describes it as, its accessors: `obj.Changed.add(handler)`
  subscribes and `remove` unsubscribes, as in F#. The handler goes to the event's delegate
  type as any argument does, and a local function, like a method, is a delegate of any
  type its signature fits. Raising an event belongs to the type that declares it. C#'s
  `+=` can be written in Z# once there is compound assignment.
- A generic method infers its type arguments from the types of the call's arguments,
  through arrays, references and the constructed types an argument's type is or derives
  from, so `Enumerable.Count(numbers)` and `Task.Run(work)` need none. Overloading has an
  `IInferable` protocol for it: before a call's candidates compete, each that can infer
  its type arguments is replaced by its instantiation, so they compete as they would be
  called, as in C#. One that can't infer them doesn't apply, and a method that isn't
  generic is more specific than a generic one that takes the same.
- A nullable value type is `Nullable[T]`, from `core:runtime`. It needs nothing of its
  own: a value goes to it through the `op_Implicit` it defines, a cast to `T` through its
  `op_Explicit`, `null` goes to it, and it boxes to its value or to `null`.
- Every operator is lifted to nullable operands, as C# lifts them, by one more overload
  of it: a `Package.DotNet.LiftedOperator` that holds the operator it lifts and how it is
  lifted. It applies only when an operand is nullable. It keeps the operands, and if
  they all have a value it calls the operator on their values; if not, an arithmetic
  operator gives `null`, `==` and `!=` compare whether each has a value, and a comparison
  gives `false`. A `null` operand goes to the other's type, so `n == null` works.
  `ILLoader.LiftedOperators` gives them for the operators a language has, and since each
  calls the operator again, what a type defines, as `TimeSpan`'s `+`, is lifted too.
- `[1, 2, 3]` is an `ArrayLiteral` in HIR, which has no type of its own, as a collection
  expression has none in C#: it goes to a CLR array of a type every value goes to, or to
  an interface such an array implements, as `IEnumerable[T]`, so it goes to a `params`
  array as well. Indexing a type says what it is given: nothing, as in `Int32[]`, is the
  type of its arrays; types, as in `List[Int32]`, instantiate a generic type; and values,
  as in `Int32[4]` or `Int32[2, 3]`, make an array of those lengths. That is what a CLR
  type makes of `CG.Index`, which has no single meaning: another type defines it however
  it wants. The array is made by the constructor the CLR gives its array type, and
  indexing an array reads and writes an element through the `Get` and `Set` methods the
  CLR gives it, so an array of several dimensions is indexed as `grid[1, 2]`. An array
  type isn't called, though it has those constructors, so an array is made one way.
- An index takes arguments as a call does, so what indexes them matches them as a call
  matches its parameters: an indexer's by their names, as `list[index: 0]`, and a generic
  type's or method's by its type parameters' names, in any order, as
  `Dictionary[TValue: Int32, TKey: String]`. The CLR gives an array's constructors and
  `Get` and `Set` no names, so an array's are positional.
- Extension methods are members from outside a type, which a CG dispatcher of its own
  gives: `ZSharp.Compiler.CG.External` wraps member access and, only when an object has no
  member of its own by the name, asks the providers added to it through
  `IExternalMembers`. So a type's own members come first, as in C#, and what every
  provider gives is one overload group, so extension methods of several classes compete
  as overloads. Traits are meant to be another provider of the same dispatcher. The IL
  loader's provider, `ExtensionMethods`, has the extension methods of the classes added
  to it, as members of the values their first parameter takes, and a call passes the
  value as the first argument, so a generic one infers from it too:
  `numbers.Where(odd).Select(twice).Sum()`. A class is added when an import reaches it, as
  C#'s `using static` brings a class's extension methods, and a host can add one
  directly. A dispatcher belongs to the whole toolchain, so what is added is seen by
  everything compiled after it, not only by a scope.
- A `Task` is used as any CLR object: waited on, its `Result` read, or run from a local
  function. Defining asynchronous functions, as C#'s `async` and `await` do, is a feature
  of Z# definitions, not of using the CLR, and isn't designed yet.

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

`CG` is an instance, and the pointer to the current one is static. That pointer is the
compiler's, `ZSharp.Compiler.Current`, in the compiler layer above the REIs: the
compiler in use, its operations and its loaders, which the host makes once. What
compiles reaches it there, and `core:compiler` is how Z# reaches it: its members are
`Current`'s, so the library is defined in terms of the compiler and not the other way
around. There is no scoping: a compilation never needs a different setup, because a
library is compiled when it is installed (not our responsibility) and only loaded on
import. A library can export compile-time utilities, which can be imported and used at
compile time.

That the compiler is reached as one, and not passed to what uses it, is a known form to
remove: a CO's operations don't take the compiler, and the standard library's Z# code
implements them, so passing it is a change of every protocol, which the seed takes in
steps.

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

`ZSharp.Lang.ZSharp.DotNet` holds the CLR class meta-type, the default for `def`.
A definition statement runs its build code once, so a name means one object, and
the object is loaded right after. Casting a type to what describes it, as in
`String as Type` with the HIR marker, is reflection. It gives the type's node, which
is how a field's type gets into the HIR being built. That is an operation of the code
as it runs, as **Reflection** describes, not a node the code holds.

The `std:` libraries are written separately in Z#, as a proof of concept, and are
compiled to DLLs or a custom file format that the CLI loads dynamically.

A type definition is written with `def`, which replaced `class`: `def Point { … }`. Its
default meta-type is the CLR class for now, and becomes Z#'s own class once that exists,
as `std/fs` already assumes.

#### Z# definitions

Z#'s own definitions are written in Z#, starting with a function that captures nothing,
in `std:functions`; `Closure` comes after it, and casting a `Closure` to a `Function`
strips its captures. `std/fs` is a sketch of what the standard library needs from them:
`def` with Z#'s class as its default, named constructors as `new cwd() {}`, an `Enum`
meta-type whose members are constructors, operators given as members through a
decorator on an anonymous function, `@operator((/)) fun(other: ItemName): Item`, and a
module made of several files, which `compile()` from `core:project` includes where it is
called. Syntax such as `not` and string interpolation can be written in Z# too.

A Z# definition is its own concept, on a different domain from any CLR entity, so its
HIR node is never translated into `Package.DotNet` HIR, and the standard definitions use
nothing of the CLR's: each has its own path, from its HIR through its own COs to LIR,
which any platform realizes, and that is what makes other targets possible. The one CLR
part is the vehicle: a node or CO class is a CLR class written in Z#, because that is how
a custom type gets into the language to be used as a meta-type.

- The `Function` node is defined in Z#, as a CLR class that implements `HIR.Instance`,
  so `std:functions` also shows whether the parser and the AST compiler can be extended
  from Z#. So are its meta-compiler, which compiles `AST.Function`, and its loader, whose
  CO is a Z# class that implements `CompilerObject` and the protocols it takes part in.
- Z# reaches the toolchain's types through the modules of its assemblies, since a CLR
  module's public types are its members, and reaches the AST compiler and the meta-types
  through `core:zsharp`, which lets it register a meta-type and make it the default.
- Once `std:functions` is in, a `fun` that no other meta-compiler handles, as one in a
  document, is a Z# `Function`. A `fun` in a CLR class, module or body is still theirs.
- On the CLR, a `Function` lowers to an LIR function that belongs to no module, which the
  runtime loads as a `DynamicMethod`, as `zsv1` loaded a free function, a module or code.
  Code in a type the runtime builds can't call a `DynamicMethod` directly, since it has
  no metadata token, so the runtime calls one through a delegate there. A generic one
  goes in a type of its own, since a `DynamicMethod` can't be generic. Precompiling is
  another platform, on Mono.Cecil, so it doesn't constrain this one.
- A `Function` is `Annotated`, so a decorator can say what it is, as **Frameworks**
  describes.
- A `fun` with no body, `fun reflect(type: Type): Instance;`, is **declared**: it has no
  code of its own, and lowers as a function handle, for the platform to map. Or it is
  premapped: `premap(declared as Instance, to)`, from `std:functions`, makes every call
  of it, and every reference to it, lower as what the definition `to` lowers to, for any
  target. A function cast to `Instance` gives its node, a value the code knows as it is
  compiled.
- Z#'s object model isn't designed yet, but some of it is settled. A Z# class has no
  inheritance, and implements interfaces. It has named constructors, and static methods
  and fields as well as class methods and fields. Its value is a type object, as in
  Python, and class methods are polymorphic through it, so an interface can declare
  class methods: an interface gives the shape of a class, not only of its objects.
- `std/functions/functions.zs` is the first of these, and works: importing it registers
  its meta-type and loaders and makes it the default for `fun`, after which a `fun` in a
  document is a Z# function, with positional and named parameters, default values,
  recursion, locals and loops, callable from CLR methods. It defines its `Function`,
  `Positional` and `Named` nodes as CLR classes written in Z#, and compiles `fun` through
  `bodies`, with its own parameter kinds. It lowers a function itself: it loads the code
  through the HIR code loader and compiles it with IR into an LIR function, with no
  platform as the target. Nothing imports it for you: a document that wants Z# functions
  imports it. A `fun` inside a Z# function's code is whatever `fun` is there, and a Z#
  function doesn't capture yet: that is `Closure`.
- `Bodies`, in the language, is what meta-compilers of definitions with code share:
  code that builds parameters of the kinds a definition gives, locals, imports and a
  body, and reifies the body. It reaches the host's node types through a function it is
  given, as building code runs on the host. The CLR's `CodeBody` adds the CLR's rules to
  it, for fields, type parameters, members of a container, and a `fun` in a body being a
  local function.
- A return converts its value to what the code being loaded returns, which is
  `CodeLoaders.Returns`: whatever lowers a function's code sets it while it loads that
  code and puts it back after, the IL loader as `std:functions` does.
- Z# reaches the toolchain's types through `std:zsharp/` modules made of its
  assemblies, as `std:zsharp/hir`, `std:zsharp/hir/code`, `std:zsharp/mir`,
  `std:zsharp/lir`, `std:zsharp/lang`, `std:zsharp/lang/ast`, `std:zsharp/common` and
  `std:zsharp/compiler/...`, listed once in `ToolchainModules`. A module's generic types
  are found by their name without their arity, all arities as overloads. What is live
  is under `core:`: `core:zsharp` gives `metaTypes` and `bodies`, and `core:compiler`
  gives `mappedLoader` and `mappedCodeLoader`.
- A CO lowers for the target it is given, and passes it on to what it lowers, so a
  Z# function lowers its code for the target its call is compiled for.
- Lowering is to be by capability: a CO asks the target what it can do and lowers to
  the richest form the target has, or to a shallow one built of LIR's core. The CLR has
  polymorphism, so a Z# class can be a class there, and a target without it gets
  records, tables of functions and `CallIndirect`. A capability is standard, not a
  platform's: its LIR nodes and what they mean are part of the standard under
  `std:zsharp/`, and each platform implements those it can. So an LIR structure is a
  record of fields, and how it is laid out is the target's.
- The minimal target is a platform that writes LIR modules as they are, written in pure
  Z#, which shows how Z# defines a platform. It comes after classes.
- A capability is an interface, and a CO checks for it by matching the target against
  it. It doesn't produce LIR: it marks the LIR the CO builds with metadata of its own, as
  a platform with polymorphism marks a structure polymorphic, so what it adds is opaque
  and the CO still builds the definition. The CLR's own entities lower without
  capabilities, by matching the runtime directly. A capability is given to a platform
  that exists, as the CLR runtime, by a trait: an implementation of the capability for
  the platform's type, which the trait dispatchers make the platform match.
- Finding the meta-compiler of a meta-type is an operation, `MetaTypes.Find`: what is
  registered for the meta-type is one way to find it, and whatever replaces it may find
  one some other way and ask what it replaced, as by what a meta-type is an instance of,
  so a meta-type can take arguments, as `Implementation[Polymorphism, Runtime]`.
- Traits are a meta-type, `Implementation[Trait, For]`, so an implementation is a
  definition of its own, first-class for reflection and code generation; anything else is
  built on it. They are added by dispatchers for the operations they take part in, in two
  flavors that are installed apart: static typing dispatchers decide from the static type
  as code is compiled, and dynamic typing dispatchers look an implementation up by the
  type a value has as the code runs, which a capability check needs, since the target a
  CO is given has no static type.
- `std:traits` is that, in Z#. `Implementation[Trait, For]` is a generic class whose
  instantiations its `Find` recognizes; its meta-compiler reads the trait and the type
  from the meta-type and builds a `TraitImplementation` node, which is what the local
  holds. While a trait is a CLR interface, an implementation of it is a bridge between
  CLR types, so its members compile as a CLR class, its adapter, which holds the value and
  implements the trait. Loading an implementation defines the adapter and adds it to a
  table of what is implemented for what, as COs for code being compiled and as CLR types
  for code that runs. `std:traits/static` replaces `CG.ImplicitCast` and `CG.Cast`: a
  value whose static type implements the trait goes to it as its adapter.
  `std:traits/dynamic` replaces `CG.Match` and `CG.Cast` for traits: a match or a cast
  calls what looks the implementation up by the value's runtime type, so an `Object`
  holding a value of an implementing type matches the trait. It applies to traits that
  have an implementation when the code is compiled. The prelude installs both.
- `std:zsharp/capabilities` has the first capability, `Polymorphism`, an interface
  written in Z#: a structure's values are objects of a class, or of anything that
  implements an interface, a class implements an interface, and a function is a method of
  one. `std:zsharp/clr` gives it to the CLR runtime by a trait, whose implementation marks
  the LIR with the runtime's own metadata, `Polymorphic` and `MethodOf`. The runtime
  realizes the polymorphic structures of a module as one batch of CLR classes and
  interfaces, with the functions marked as theirs as their methods: a call of one is a
  method call, a new value of one is a new object, and its fields are reached through the
  object. A CO lowering for the runtime, which it sees as an `Object`, matches it against
  `Polymorphism` through the dynamic trait dispatchers. The prelude installs both.
- `std:oop` is Z#'s class, in Z#. Its nodes are `Class`, `Field`, `Method`,
  `Constructor` and `Receiver`, the object a method or constructor runs on, and a
  method's parameters are `std:functions`' own. The class meta-compiler compiles its body
  through the language's container compiler: a `let` is a field, and a `fun` or a `new`
  written directly in it is a method or a constructor. In a method, `this` is bound if the
  first parameter is `this`, and each field by its name to `this.field`, unless a
  parameter has the name; both are bound to a local of the building code that holds the
  method's receiver, as a parameter is. A class lowers for a target that has
  `Polymorphism`: into an LIR module of its own, its structure is the type of its objects
  with its fields, a method is a function that takes the object first, marked as the
  class's, and a constructor is a function that makes the object with its fields' default
  values, runs its code on it, and gives it. Calling the class runs its unnamed
  constructor, a named one is a member of the class, and a value has its fields and
  methods as members. Importing it makes `def` a Z# class, so the prelude imports it
  last. A target without polymorphism, static and class members, and type objects come
  next.
- A Z# interface is written `def Shape of Interface { fun area(): Int32; }`: an
  `Interface` node has methods with no code, and nothing defines fields for it. A class
  implements the interfaces it is given as bases, `def Square : Shape`, and must have each
  of their methods, with the name and the parameter and return types; it has no
  inheritance. A class and an interface are both what methods belong to, a `MethodOwner`,
  so a method and a call of one are the same for either. An interface lowers to a
  structure the target's polymorphism makes the type of any object that implements it,
  with its methods declared with no code, and a class's structure is marked as
  implementing it, so a call through an interface calls what the object's class has. A
  class goes to the interfaces it implements, and a value of an interface is cast down
  to, and tested for, a class that implements it or another interface. Polymorphism has
  `cast` and `test` for that: an object knows what it is, so the capability marks a
  function with no code as the platform's cast or test, which the runtime emits in place
  as `castclass` or `isinst`. LIR still has no cast nodes.
- A type written in a signature is evaluated, as any expression is: there is no
  compile-time, only code that runs, and the code that builds a definition is code like
  any other. Typing is part of structure, so a type's value where an HIR type is expected
  is HIR, and composing types is ordinary code that builds HIR. A type converts implicitly
  to what describes it where an `HIR.Type` is expected, which is what a signature's slots
  are; anywhere else it is its type object, and `Foo as Class` is reflection.
  `Dictionary[String, Item]` is `CG.Index` on `Dictionary`: given types, it is the
  instance as a type, which converts to its description, a generic instance node of the
  definition and what describes each argument; given values that describe types, as a
  class still being built is in the code that builds it, it builds that node as the code
  runs. A generic instance node's arguments are any `HIR.Type`, so a CLR generic instance
  can be over a Z# class. So `let items: listOf(Item)`, where `listOf` is a function from
  a type to a type, is a field's type like any other.
- So any type goes anywhere a type does: a CLR method's parameters, return type and
  fields are `HIR.Type`s, which the IL loader loads to COs and lowers to the LIR the
  runtime realizes, whatever the type is. A failed lowering keeps why, so the error a
  call gives is the code's own. A cast from a CLR class to an interface still being
  loaded goes down as C# allows, if the class isn't sealed.
- A member of a Z# class marked `@static`, which `std:oop` exports, is the class's own: a
  static field is one variable, and a static method is a function, which takes no object
  and isn't marked as a method. The decorator is a Z# function that sets the node's
  `IsStatic`, run as the class is built, as any decorator is, and a decorated `let` in a
  class's body is a field given to its decorators, which the members compiler handles. A
  member is registered as what it is, whatever its decorators gave.
- HIR describes structure and MIR behavior, so a member's code holds references, and
  only loading it, once the class is complete, says what they mean. In a method, each of
  the class's members, a field or a method, is reached by its name, bound through the
  object the method runs on, unless a parameter has the name: `bar()` binds and
  `Foo.bar()` doesn't, so the binding is in the code. What binding means is the member's
  CO's to say: an object's member is reached through the object, binding a static member
  does nothing, and a class member is reached through the class's type object. Using the
  object where there is none, in a static or class method, is an error when the code is
  loaded.
- A static field lowers to an LIR global of the class's module, which the module's
  initializer gives the field's default value. A class's static fields and its type object
  are lowered with it, so its module is whole once the class is lowered. The class's CO
  holds the COs of its static fields.
- A class is a value: its type object, the one object of its metaclass for each target. A member marked `@class`, which `std:oop` exports, is the
  type object's: a class field is a field of the metaclass, and a class method a method of
  it, which runs on the type object, named `this` if its first parameter says so. An
  interface declares class methods too, on its class shape, the metaclass every class that
  implements it has, so a class implements an interface's class methods with class
  methods, and a call on a value of `Type[Shape]`, the type of any class that implements
  `Shape`, calls what that class has. A metaclass and a class shape lower to structures
  the target's polymorphism makes an object type and an interface, in the class's or
  interface's module, and a class's metaclass implements its interfaces' class shapes. A
  class's type object is an LIR global of its module, which the module's initializer makes
  a new object of its metaclass. `ClassOf` is the node that describes a metaclass, so it can be written in a
  signature.
- With no inheritance, a value of a class is of that class, so a class member reached
  through it, or by name in one of its methods, is its class's type object's. Only a value
  of an interface could be of any class, so what class it is would come from the object,
  which isn't reachable yet.
- A class's bases are what it implements: Z# interfaces, whose methods `std:oop` checks
  it has, and types of a platform's own, as the CLR's interfaces, which `std:oop` asks
  the target to implement through the capability `PlatformInterfaces`, of
  `std:zsharp/capabilities`, and which the platform checks as it realizes the class. The
  CLR platforms have it, by `std:zsharp/clr`: a polymorphic structure's `Implements`
  are interfaces of the class they make of it. So a Z# class can be a CO, as
  `def Retyped : CompilerObject, ICompileIRExpression, ITyped` in `std:oop` is, and the
  compiler calls it through its protocols; a target without the capability can't have
  such a class.
- A library of standard definitions is two: its structure and its compiler. `std:oop`,
  `std:functions` and `std:modules` are their HIR nodes, which reflection needs alone,
  and what only changes nodes, as the decorators `@static` and `@class` do;
  `std:oop/compiler`, `std:functions/compiler` and `std:modules/compiler` are their COs,
  meta-compilers, loaders and binders, and what lowers, as `lower` and `premap` are,
  which importing them installs. A library keeps its name for its structure, since a
  blob names a node's type by its assembly, so what was encoded before still decodes. So
  the compilers depend on the structures and not on one another's COs, and the COs of
  any of them can be Z# classes, which `std:oop/compiler` compiles, with no cycle.
- The structures are the platform's own definitions, CLR classes, which a new runtime
  reimplements: each is written with one meta-type, `Node`, which its library imports as
  `DotNet_Class`, so reimplementing it is ideally giving that name another meta-type.
  Their getters still say `@explicitThis`, which a CLR class needs and a Z# class doesn't.
- `std:oop/compiler`'s COs are Z# classes, built by itself: what loads a class, method,
  constructor or interface, and the meta-compilers that compile them. What has no object,
  as matching arguments or finding what a built class was built as, is a function of its
  module, since a module holds no module. So a runtime other than the CLR's gets the COs
  with it.
- Importing a structure library installs its compiler: its entry point imports it by
  name, as `std:project` installs `core:project`, so `import { Function } from
  "std:functions"` makes `fun` a Z# function, and the structure's assembly doesn't refer
  to the compiler's. A library is a module, whose root `Module` node only
  `std:modules/compiler` loads, and its blob has modules' nodes, so importing any library
  enters `std:modules`, which installs what loads it before its root is loaded. An entry
  point is the toolchain's `import`, which installs a library into a compiler, not what
  loading it does: a program that reflects loads the libraries its nodes are of, as a
  native platform loads the library a type's metadata is in, and runs none of their
  entry points, so it gets their structure alone. What has no blob, as a module built
  without reflection, has none of it.
- The decoder enters the assemblies of a unit's kinds of nodes once every node is made,
  as entering one can import what refers to the unit's nodes, as a structure's entry
  point imports its compiler, whose blob refers to the structure's.
- A binder that replaces another binds again what the other bound, so while a library and
  a newer one of it are both loaded, as when a library is split, every node is bound as
  the newer one binds it.
- What can be reflected on, or has its own semantics, is a definition of its own: a
  method, a constructor, a field, a parameter. What can appear in code is an instance,
  which needn't be a definition. A CO is only a handle, which interactions use. An
  extension's COs can complement each other, but two things interact through a shared
  language, as the REIs are: a CO is almost never matched by what it is, and asks the
  other through an operation. So `std:oop` reaches a parameter as what it is referred to
  as (`ICompileIRReference[Parameter]`, which `std:functions` gives it) and by its type
  (`ts.TypeOf`), a type goes to another as the other is itself (one definition is one CO)
  or as any of its bases goes to it, and the type of a definition's value is its
  metaclass or class shape (`ts.TypeOf(cg.Get(definition))`).
- What is allowed is defined, not what isn't checked for. A variable in code, a parameter
  or a method's or constructor's object, is what the function whose code it is defines it
  as while it lowers that code (`CodeLoaders.Define`, which the IL loader's methods, Z#
  functions, methods and constructors use, and `Restore` after, since one function's
  code can lower another's). Nothing else gives a variable a meaning in code, so code that
  uses a variable its function doesn't have fails to load because nothing defines it,
  with no check for it. A parameter as a definition is still what a call matches
  arguments to, anywhere. A local is still its own definition wherever it is used, which
  could move to what its declaration defines for the code after it.
- A method's object is its first parameter, `this`, of what it is a method of, as its
  meta-compiler builds every method; `Method.ThisParameter` says which parameter it is.
  `@static` removes it from the parameters and `@class` retypes it to the class's
  metaclass, as decorators transform what was built. A static method's code still reaches
  its class's members by name through it, so the method defines it for its code as the
  class, with no object of it: a static member is reached, and an object's member fails
  as it does through the class. So a method is called with its object as its first
  argument, as any parameter is matched. A constructor's object is a `Receiver`, an
  instance of the class it makes and not a definition, which the constructor defines for
  its code as the object it makes, a local of its function.
- A class or an interface holds its methods and constructors as a module holds its
  definitions (`Holder`, `Held`), and lowers them as a module does, through
  `ir.CompileDefinition`. Each declares itself when it is asked for, into what holds it,
  as a function does: its function is made before its code is lowered, so code can call
  it, and a method of an object is a method of its object's type's structure. A
  constructor makes an object of its class's structure with the default values its
  class's HIR gives its fields. A call is of what the callee is referred to as.
- Methods of one name overload, as functions do: `Method` is `Overloadable`, so binding a
  second one adds to the first, and a member's name reaches all its methods as one CO.
- A method is called with its object first. What is reached through an object is any
  callee bound to it, `Bound`: calling it calls the callee with the object first, and
  it is referred to as the callee, `ICompileIRReceiver`, so a delegate of it is closed
  over the object, as the CLR closes one over a first argument:
  `let f: Func[Int32, Int32] = adder.add`. That is how the meta-compilers give their own
  methods to the compiler as hooks. What holds any value takes it as its type, as
  `CapabilityContext.Of[T]` does, so it needs no conversion.
- A node type that dropped a field reads what was encoded of it before: the decoder
  leaves out a field the type no longer has, as the standard library's builds are read
  by the next build of the library that defines their nodes.
- While a class's structure is built, the types of its fields are lowered, and lowering a
  class lowers its code, which may use a class whose structure has no fields yet, as a
  metaclass's code uses its class's. So while any structure is being built for a target,
  a class used as a type is only its structure, and it is lowered, in order, once none is.
  And what lowers a function, or a parameter, makes it after compiling its types and finds
  it again then, since compiling them can lower code that asks for it first.
- A class has no base, so a value of one has only its class's members, and goes only to
  its class and to what it implements. `Object` is the CLR's, the root of its type
  system, which every value is on a platform that realizes the CLR's types: so
  `Object`'s CO takes a value of any type that isn't one of the CLR's own, as a Z#
  class's, as it is, and a CLR type converts to it by its own rules, as a value type
  boxes. Code generation asks nothing of what a class is on any platform, as "class"
  isn't the same on every one: the target lowers the value, and a target that has no
  `Object` fails on `Object` itself. So a Z# object goes where `Object` is expected in
  what any CLR platform builds.
- `def` isn't a class or a type: it is a definition, `def ID? (of META)? ...`, and
  `module` is `def` of `Module`. So a definition's body holds definitions as a module's
  does, and whether it takes each is its meta-type's to say; a CLR module is
  `def Name of DotNet_Module`. Z#'s definitions are the defaults, and a library that
  needs another meta-type says `of`, so what a library's definitions are doesn't depend on
  what was imported before it.
- A module is a standard definition too: `std:modules` defines it in Z#, and the CLR
  realizes it as a module. The HIR `Module` is structure: its globals and its
  definitions. A `let` in a module is a `Global`, of the type written or of its
  initializer, which is code; an import runs as the code that builds the module runs; and
  a definition is compiled by its own meta-type. The AST compiler binds names as the AST
  has them.
- Z# is meant to be extended, so the standard definitions work with general concepts
  rather than predicting each use. A meta-type, and its meta-compiler, handles only what
  it cares about: what it doesn't care about, it doesn't define, and that fails on its
  own wherever nothing defines it. What it does care about, it can and should reject.
  A class cares about its fields, methods and constructors, and also about owning any
  other definition written in it, which it holds and makes reachable, without caring what
  that definition is.
- An HIR node does nothing by itself, so a structure the definition would consider
  invalid isn't wrong until it is used as a definition: until it is loaded to a CO, which
  is when its node is complete. So a meta-compiler builds what is written, and rejects
  only what it can't build; what a definition accepts is checked by its loader. A class
  method with no code, an interface method with code, a field or a global with neither a
  type nor a value, and a CLR property whose accessors don't make one are errors of
  loading them, not of building them. Code that builds HIR in steps, as a generator,
  passes through structure that isn't complete yet.
- A module is a unit of state and of loading, not a namespace, so modules don't nest.
  That needs no rule: `module` is syntax only in a document, and a module isn't an HIR
  `Definition` but an `Instance`, the root of what it holds, so nothing that holds
  definitions, as a module's or a class's list of them, can hold one. A module's CO
  doesn't lower into anything, as it has no `ICompileIRDefinitionIn`. In code, a module
  is a value, cast to what it is, as `module { ... } as Module`, which is its structure
  with no identity, as any definition cast to its meta-type is. A module's loader keeps
  one CO for each, as the IL loader does for a CLR module, and its members are bound to
  that CO. `Definition` never meant identity; one CO for each definition is only what the
  definition loader does. Grouping definitions inside a document, if it is needed, would
  be a meta-type of its own, with no state.
- A Z# class or interface has its own lists, of fields, methods and constructors, and
  one for any other definition written in it, which is compiled as it would be anywhere
  and reached through the class's CO by the name `Named` gives it: what a class defines
  its members to be is a `fun`, a `new`, a `let`, and a `get` or `set` accessor. A field
  has the type written, or else its value's.
- No name of a definition's members is its own: what was `Square.Class` would take the
  name `Class` from a class's members, as a Python class's names are taken. So the type
  of a definition's type object is `Type[T]`, from `std:types`: `Type` is a class whose
  class method is its index operator, which builds `TypeOf(GetValue(Get(T)))`, the HIR
  that describes the type of T's value. That runs as code, and is reflection; the type
  itself is what TS finds once the node is loaded, which is the compiler. So it takes
  nothing of a meta-type beyond the value its CO gives: a Z# class's value is its type
  object, of its metaclass; a CLR class's is itself, a `Type`; an interface's is of its
  class shape, the type of any class that implements it, but nothing lowers it, as an
  interface has no type object, so evaluating it is what fails; and a module's CO gives
  no value, so it has no type. A meta-type can say what `M[T]` means of its own, as an
  index operator.
- `Get` gives the instance's CO itself, and `GetValue` its value, which is `CG.Get`. A
  definition and its value are different COs: a class's CO and its type object's. Code
  that uses a class as a value still reads the class's CO as one, so it compiles to the
  type object too, until `Get` is renamed `Reference` and reading a value is always
  `GetValue`, which is the change that remains.
- An operator is a definition that is one: `@operator(Operator.Index)`, or
  `@operator((/))` with an operator literal, marks a definition that has the HIR
  `OperatorDefinition` capability, as a Z# method does. A decorator takes and gives the
  definition, so decorators compose. `M[T]` is `CG.Index`, which a CO handles if it
  defines `ICTGetIndex`, or else the typed dispatcher by the type of the value: a Z# class
  defines both, calling its class methods that are its index operator for the class, and
  its methods that are for one of its objects. The operator's left operand is what the
  method runs on.
- A property of a Z# class or interface is what its accessors read and write. An accessor
  is a method of what holds it, named for what it is, `get Name` or `set Name`, so it is
  lowered, dispatched and implemented as any method is, and never clashes with a method
  named for the property; a `Property` groups the two, and is defined once its accessors
  are built. It is reached by its name as a field is: through an object, through the class
  if it is static, through the type object if it is the class's, and by name in a method.
  Its loader checks what a property needs of its accessors. A Z# property isn't a CLR
  property: it is the class's, and the CLR realizes only its methods.
- `Named` is an HIR capability: a definition with a name of its own. The CO loader of an
  HIR module finds its members by the names `Named` gives them, so a module holds no
  mapping. A node of a CLR definition is `Named` by its name, and a Z# node by an
  accessor of its own.
- Belonging is how a definition lowers, not its structure, so no node holds an owner.
  The CO of what holds definitions, as a module's, binds its members' COs to itself as it
  is loaded, which is MIR, and lowers them top-down: its LIR module is made for a target,
  and each member is lowered into it with `ir.CompileDefinition(member, owner, target)`,
  globals first, in their order. A global gives its initial value to its owner's
  initializer, if the owner has that capability. A member lowered first by itself, as a
  function a call reaches, asks what holds it. `std:zsharp/lowering` defines `Holder`,
  `Held` and `Initializing`, and installs nothing.
- What a definition written in Z# needs from code that uses a class still being loaded
  with it is general: such a class goes to the interfaces it implements, can be cast down
  to and tested for, has a `null`, can be given to a generic CLR type, and has its
  methods as delegates in its own code, each resolved as the code is emitted. A module's
  code can name the module itself, which is known before its code is lowered.
- A definition in Z# lowers once for each target: each CO keeps what it lowered to by
  target, and a CO that owns an LIR variable, as a parameter or the object a method runs
  on, gives it to whatever lowers a function with it. A meta-compiler finds what the
  definition around it is compiling from the compiler's contexts, as a method finds its
  class's members; a `CapabilityContext` makes any object a context's capability.
- The runtime counts how deep it is loading. What a load reaches while code is emitted,
  as a class first used by that code, loads inside it, and only the outermost load
  finishes, so nothing is completed while code is still being emitted.
- A definition has three representations: its meta-type's instance, the HIR that is its
  structure, for reflection and code generation; its CO, for what it does with other
  things; and its type object, what it is as a value, whose shape is the meta-type's to
  say. For `def Foo of Class`, they are a `Class`, a `CompilerObject` and a `Class[Foo]`.
  In the code that builds a definition, its name is bound to the local holding its node,
  since the definition doesn't exist yet: loaded, it is a variable of the meta-type, and
  reading it gives the node. A definition that exists is its CO, whose value is its type
  object, which converts to the node where an HIR type is expected.
- A property is written as its accessors, `get Name(): T { ... }` and
  `set Name(value: T) { ... }`, each a definition of its own named by its property, which
  binds no name. The CLR's meta-compiler compiles them as the methods the CLR names for
  the property, and pairs them into a `Property` once the class's members are built; the
  runtime defines the property and links its accessors, so a C# interface's property is
  implemented by an accessor.
- Giving a group of overloads type arguments gives the group of those it applies to:
  which is meant is the call's to choose.
- A CLR class, interface or module compiles a `fun` written directly in it as a CLR
  method, through `CodeBody.WithMethods`, whatever the default for `fun` is: the meta-type
  of its members is the container's to choose.
- Writing it showed what CLR definitions written in Z# need, which are general: a class
  still being loaded goes to the interfaces it implements, and can be cast down to, the
  cast being resolved as the code is emitted, and a function of a module defined in Z# is
  a delegate where one is expected, once its module is loaded.
- On the runtime, a standalone function's body is emitted once whatever is being loaded
  is complete, since a `DynamicMethod` can only call a method of a type that exists.
- A `Function`'s parameters are nodes of its own, so `std:functions` registers their
  loader. A parameter is both a definition and an instance, and both routes give the
  one CO the definition cache holds for it: the instance loader loads it as a definition.
  That CO is what a call matches an argument to and what the body uses as the variable,
  and it owns its `LIR.Parameter`, which the lowerer asks it for to build the signature,
  as a local's CO owns its `LIR.Local`. So the parameters of the code a lowerer lowers
  need no map: the IL loader keeps one only for the parameter types it owns.

### Definitions

`def`, `module` and `fun` are all definitions sent to a builder, so there is one
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

def Foo of Class { let x: I32; }

fun foo() {
    def Bar : Foo {}

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
let x: Foo /* not ok */ = def Foo { let parent: Foo?; /* ok */ }();
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
def Cap17 {
    @static fun make(): DotNet_Class {
        let x = "before";
        fun read(): String { return x; }
        let made = def Made17 {
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

    def Foo {}

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

Default meta-types are what let `def` and `fun` be Z# terms and not .NET terms once Z#
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

What a module's member's decorators give is registered as the kind of definition the
member is, as a class's member is: it is cast to its underlying type, so a decorator
can give `Instance` and be used anywhere.

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

A loop's body is parsed with what the loop is in: `break` and `continue` are the loop's,
and any other statement, as a function's `return`, is for the parser of what holds the
loop.

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

Equality binds looser than comparisons, `as` and `is`, as in C#: `a == b as T` is
`a == (b as T)`, and `a < b == c` is `(a < b) == c`.

An operator before its operand, as `-x`, is a call of the group of its syntax with the
operand alone, so it is an overload that takes one, as `core:runtime`'s `Negate` is of
`-` for `Int32`, `Int64`, `Single` and `Double`, and a CLR type's `op_UnaryNegation`. A
minus before an operand binds tighter than `*` and looser than `**`, so `-2 ** 2` is
`-(2 ** 2)`, and `5 - -3` is 8.

A C# method marked `[Operator("+")]` is an overload of what `+` stands for, and
`ILLoader.Operators` finds them in a type. For now the CLI adds those of `core:runtime`
and `core:zsharp` to one table for the whole language. Scoped tables can come later.
`core:runtime` gives the operators C# predefines: `+ - * / < > <= >= == !=` on `Int32`,
`UInt32`, `Int64`, `UInt64`, `Single` and `Double`, `+` on `String` and between a `String`
and any value, and `or == !=` on `Boolean`. Smaller integers have none of their own and
widen to `Int32`, and mixed operands widen to the most specific overload both go to, so
an `Int32` and an `Int64` add as `Int64`, as C# promotes them. The operators CLR types
define are overloads too, so `String`'s `==` is its own.

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

Inside a definition, its own name means what binding it where it is gives: for a method,
all the overloads it is one of. It is in a scope of its own, around the definition's
names, and is bound again when the definition's contents are compiled, since by then a
container has bound the definitions next to it too. Binding it there shadows what the
name means outside, and the .NET meta-types register a shadow rule that a method
shadowing methods overloads them, as the merge rule does in one scope; each overload is
in a group once. Where nothing outside has the name, as at document level before the
definition runs, the name is the definition alone.

Code that is built refers to a definition still being built through the local of the
building code that holds it, which reifying encodes as what the local holds when the
code is built. An overload group is reified the same way, built of its items, so a group
of definitions being built is a group of the definitions.

### Overloads

In HIR, an overload is pure structure. A class doesn't hold overloads, it holds methods.

A class loaded to a CO builds whatever is convenient for it, such as a map from name to
CO. Methods that share a name collapse into an overload group CO at that point, because a
group is special in code generation and not in the structure of a class.

Where one HIR value has to stand for many and there is no class, as in the operator
table, an HIR `OverloadGroup` can be an `Instance`.

Inside a definition of such a kind, its own name is the overloads it is one of, as a
method's is.

What overloads by its name is a kind of definition, by the HIR capability `Overloadable`
of its node's type, as a Z# function's: a name bound in a scope to one such definition
and then to another stands for both, as a group, as the binder merges them, in a module,
a document or anywhere else names are bound, and a call chooses by its arguments. A
definition being built is known by its kind, what describes its node's type, as a
document's reference to one is too. A module keeps each definition of a name, and its
member of that name is their group.

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

Bitwise `&` and `|` bind looser than equality, `&` tighter than `|`, as in C#. They are
defined for integers, for a CLR type's `op_BitwiseAnd` and `op_BitwiseOr`, and for the
flags of an enum, which combine as its underlying integers.

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
    let cls = def A of Class : base {
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
- Importers route by prefix through the string importer. The prefixes say what a
  library is: `core:` is live and stateful, bound to the toolchain that is running, as
  `core:compiler`'s CG and loaders, `core:zsharp`'s compiler and meta-types, and
  `core:runtime`, which also keeps the CLR's types for now; `pkg:` is an external
  package installed by the package manager; `std:` is the standard library, its common
  APIs; and `std:zsharp/` is the standard API of the toolchain, its types, as
  `std:zsharp/hir`, `std:zsharp/lang/ast`, `std:zsharp/lang/tokenizer` and
  `std:zsharp/lang/parser`. The CLR
  package's HIR, `std:zsharp/package/dotnet`, isn't standard: it is there until a package
  manager can give it as a `pkg:`.
- A `std:` library is a CLR module, or else a library of the standard library written
  in Z#, found from where the toolchain runs: `std:name` is its build,
  `std/name/build/name.dll`, an assembly named `std.name` (`std.zsharp.lowering` for
  `std:zsharp/lowering`), or else a document, `std/name.zs` or `std/name/name.zs`. A
  process keeps what `std:name` gave it the first time, so a library built while it runs
  is what the next process loads. One built library's assembly finds another's by its
  name.
- A source with no prefix is a file, relative to the document importing it, and what
  imports it is what is registered for its extension in `core:zsharp`'s `files`: a
  `.zs` file is a document. A `.dll` is an assembly, which the host imports, since what
  the toolchain is built of has to be loaded before anything in Z# can be: it is loaded
  into the running platform, unless it is already, and importing it gives its module,
  whose members are its public types by their names, whatever their namespaces, once it
  is entered. So `import { AssemblyDefinition } from "Mono.Cecil.dll"` reaches
  Mono.Cecil. A name with a type that isn't generic is that type, and indexing it with
  type arguments gives the generic type of that name and arity, whatever its namespace,
  as C# reads `IList` and `IList<T>`; a name of generic types only is its arities, as
  overloads.
- Nothing is imported for a script. The prelude, `std:prelude`, imports what a script
  wants, so a script imports it first, `import "std:prelude";`, and gets the standard
  definitions, as Z# functions, through an ordinary import.
- Importers do nothing CO-related. They return real HIR, and imported names
  cross into MIR through the HIR -> MIR REI.
- Libraries are supplied by a `pkg:` importer written purely in Z#, the package manager,
  as **Packages** describes.
- An import that runs while compiling reaches each of its names' members as it runs,
  since that is what importing them means: whatever comes with a member comes then, as
  a class's extension methods do, and a name the imported value has no member for is an
  error of the import, not of its first use. The names are still bound to the member
  as code, `value.name`, so what they stand for is reached again where they are used. An
  import that runs only as a definition is constructed has nothing to reach yet, so its
  names are checked when they are used.

## Reflection

What describes a type is asked as the code runs, wherever it runs, so it isn't part of
what a definition is, and a definition doesn't encode its HIR node.

- **A description is a function call.** A CO's `Cast` to what describes it decides what
  that is. The IL loader's casts of CLR types and of classes defined in Z# call its static
  `DescribeType(Type)`, given the type's type object, which the loader of the process that
  runs answers. A Z# class's cast in `std:oop` gives its node as a known value instead.
- **One type, one description.** The loader describes a type the runtime made of a class
  defined in Z# as that definition, so `reflect` and a cast give the same object however
  the type is reached.
- **`std:reflection`** declares `reflect(type: Type): Instance`, with no code. The prelude
  premaps it onto the loader's `DescribeType`, so the loader knows nothing of
  `std:reflection`, and `std:reflection` nothing of the loader. A platform whose code
  can't reflect leaves it unmapped.
- **Code that runs elsewhere describes types there.** An assembly the Cecil platform
  writes calls `DescribeType` of the loader that runs it, which it refers to as any
  other assembly. For a target without reflection, code that rebuilds a description,
  reified or serialized at build time, is the fallback.

### Reflection and importing

Designed on 2026-11-10, and built, as **How it is built** says, but for what it lists
as not yet. What follows replaces the IL loader as the one source of descriptions.

Reflection and importing are two things. Reflection is getting the HIR node of a
definition, which code generation also uses: a program can reflect and build HIR with
only the HIR libraries, without the language's core. Importing is getting HIR for
something imported, which needn't contain any: a native DLL or a foreign language's
module gives HIR too. Where an importer's HIR comes from is its own business.

- **Reflection is stored metadata.** An encoder, run by the build, stores what describes
  the module somewhere, and `reflect` is matched to how it was stored, through mapping.
  It is the composition of two halves:
  - the platform's: a runtime object that carries metadata gives its metadata, and the
    unit it was compiled into, as `Type.Module` is on the CLR;
  - the encoder's: that metadata, in that unit, gives the HIR node.
- **Any object that carries metadata** can be reflected, not only a type object, so a
  type object carries it too. A Z# class's type object carries it through its runtime
  class, its metaclass, which each class has of its own. `ClassOf` stays the canonical
  node of a metaclass, as `Type[X]` is an expression, which makes a node each time.
- **Encoding happens before lowering**, and can add to the HIR: it gives each node it
  encodes its id, as metadata. A node with no-reflection metadata is skipped, and a node
  it encodes that refers to a skipped one is written with an external reference, or
  fails.
- **The metadata is an id**, unique in its unit. The unit carries what was encoded, as
  the platform gives it a place: on the CLR, by convention, a blob, `[ReflectionBlob]`,
  or a file beside it, `[ReflectionSourceFile("name.hir")]`. A native target can reuse
  either, as they are only what a decoder reads. A build that puts several modules into
  one unit encodes them together.
- **Annotations are the metadata.** An annotation's type is annotated with what keeps
  it: *encoded*, which the encoder honours, and *kept at runtime*, which lowering
  honours, carrying it onto the LIR of what the definition lowers to, which the platform
  writes its own way, as custom attributes on the CLR. Others are dropped, as the
  framework's `EntryPoint` is. A kept annotation's state is constants a platform can
  write.
- **Separate platforms, separate reflection.** Each module can be built by a platform
  instance of its own, mapped onto its own encoder, so one program can hold modules
  reflected different ways. A type with no metadata, as `String`, is described as the
  IL loader describes it.
- **Encoders.** A (de)serializer is registered at compile time, for each node type.
  The structure of a type is inferred by reflection, and a type can supply its own
  (de)serializer instead. Generated ones are what tests Z#'s code generation. The format
  is HIR's, not a platform's: a graph, with ids for what is shared, and an external
  reference, resolved by importing it, for each node another module owns. Bodies are an
  encoder's choice.
- **One node per definition per process.** The importer and `reflect` decode a unit
  once, and give the same nodes.
- **Importing doesn't load.** An importer gives HIR, and only that. A .NET DLL imported
  in a module is read as metadata, and is loaded into the runtime when the importing
  module is, as the runtime platform maps a handle into it. An import in a document is
  loaded right away, and so is one that is evaluated as code compiles. A module's entry
  point runs when the module is loaded into a runtime.
- **The compile-time environment.** An import a module's build needs, as one whose entry
  point installs a meta-type, is `@ct import`, or is at the document level, as in its
  module file. `ct` has an overload for an import, which runs it as code compiles.
- **An imported definition is bound to what it was built as.** The CLR importer walks
  the unit's entities, reflects each, and so has each node with what it was compiled to,
  which it premaps the node onto, so lowering it gives that and not a new definition.
  Every definition kind can be premapped, as a declared function can.
- **A kept annotation is one generic CLR attribute**, `[Metadata(type, names, values)]`:
  its type, and each of its fields by name with its value. Made for each type, an
  attribute class would be what C# reads best, which nothing needs yet.

#### How it is built

- **Annotations.** `Annotated` is on Z# functions, globals, modules, classes, methods and
  constructors, and on CLR classes, interfaces and structures. The capabilities
  `Kept` and `Encoded` of an annotation's type say what keeps it. `keep(node,
  metadata)`, of `std:zsharp/lowering`, is how a CO carries a node's kept annotations
  onto the LIR it makes, as LIR `KeptAnnotation`; the IL loader does it for a CLR class.
  The Cecil platform writes each as `[Metadata]` on the method, field or type it writes.
  The runtime platform doesn't yet, as what it makes is reflected in the process anyway.
- **`std:encoding`** is the encoder and the decoder, in Z#. `encode(module)` gives each
  annotated node it writes an id, kept as a `ReflectionId`, and gives the bytes. What it
  writes is inferred from fields, a graph with each shared object once; code, a
  `Code` or a list of locals, isn't written, and an annotation only if it is encoded.
  A node another unit gave is written as that unit's, by its id; one that describes a
  CLR entity, as that entity, by its names; a CLR definition of the module, as the
  entity built of it, by its id. A registry of custom and generated (de)serializers is
  not yet.
- **The CLR convention.** The build gives the bytes to the platform:
  `target.embed("ZSharp.Reflection", bytes, "std:encoding")`, an embedded resource and
  `[ReflectionBlob(resource, decoder)]`, which names the library that decodes it.
- **Importing.** What imports an assembly asks each describer in `core:zsharp`'s
  `describers` for it, and the first that knows it gives what importing it gives, before
  the assembly is entered; one none knows is the IL loader's view of its module. An
  assembly whose blob names a decoder has the decoder imported first, so it is
  described whatever was imported before it. A describer that fails leaves the assembly
  to the IL loader, with a warning, so a seed built wrong still loads. `std:encoding`'s
  describer decodes a unit once, entering the assembly of each kind of node it makes,
  as what a node means is its library's, and binds each node to the entity that carries
  its id.
- **Binding.** `core:compiler`'s `bindings` maps a kind of node to what binds it, which a
  library declares with `@binder`: a function is premapped onto its method, a global is
  the static member it was built as, and a Z# class, its methods and its constructors are
  the type and methods they were built as, which their COs are instead of lowering them
  again. A node whose kind has no binder yet waits for one, as a library's nodes can be
  decoded before the library that binds them is imported. A built class's metaclass is
  found from it, as the type of its name and `.Class`, and its type object as the static
  field of that type.
- **Reflection.** `reflect(value)`, of `std:reflection`, is premapped by the prelude onto
  `describe`, of `std:encoding`: what carries metadata gives its id and its unit, which
  gives the node, and what carries none is described by the IL loader. A CLR method cast
  to what describes it is a call of the loader's `DescribeMethod`, as a type's is of
  `DescribeType`.
- **Names with a dot.** The Cecil platform writes a type of a dotted name, as
  `Shape.Class`, in the namespace before its last dot, so its full name is its name
  however it is found, as a module Reflection.Emit builds finds a type by its namespace
  and name.
- **Not yet:** importing that reads metadata without loading; the runtime platform
  writing kept annotations; a Z# interface built and imported; a description of a Z#
  definition in code a platform writes, which is a known value of the process that
  compiles it; and generated (de)serializers.

## Frameworks

Code that builds a definition runs as it is built, at every level: a document, a
module, a definition. So a library that installs itself into the environment that
imports it, as one that registers a meta-type does, can't say so with code that runs as
it is built, which would install it where it is built. It declares it instead, and a
framework, a library a build uses, makes what is declared into what the module does
when it is imported. `std:framework` is one.

- **Declarations are annotations.** `Annotated`, an HIR capability, is a definition's
  list of what has been said of it. A Z# function has one. A decorator of
  `std:framework` adds an annotation and gives the definition, so a declaration is data,
  for whatever reads the definition.
- **What it declares.** `@entrypoint` says a function is called when its module is
  imported. `@definitionLoader`, `@instanceLoader` and `@typeLoader` say a function is
  what loads the nodes of the type of its parameter, in the registry of that name.
  `@metaType(M)` says a function is the meta-compiler of `M`. Making `M` a default stays
  code, in an `@entrypoint`.
- **`build(module)`** reads the annotations of a module's functions and gives the module
  an entry point, a function named `<entry>`, whose code does what each declares, in
  their order, as `mappedLoader.Definitions.Register[T](f)` and `metaTypes.Register(M as
  Instance, f)`, then calls the explicit entry points. A project's module file builds
  what it emits: `emit(build(compile("Plugin")))`.
- **A module's entry point** is `std:modules`' `Module.EntryPoint`: a function of it that
  is called, with nothing, when the module is imported, and not when it is built. It
  lowers as LIR `EntryPoint` metadata of the module, which the Cecil platform writes as
  `[EntryPoint]` on the method. Importing an assembly calls each method marked so, the
  first time it is imported. A module compiled from its sources is
  imported with `enter(module)`, which calls it the first time, in the running process,
  as `std:packages` does for a package that is a project. So a module does the same
  built or from its sources.
- **What its declarations name is described as it runs.** `@metaType(Thing)` registers
  `Thing as Instance`, a description cast, so a built module registers the description
  its process gives its own `Thing`, the same one code that imports it gets.
- **Not yet:**
  - A decorator that takes arguments gives a function of a bound method, as
    `@metaType(M)` does, since a Z# function captures nothing. The argument is in
    parentheses, not `@metaType[M]`, as a Z# function has no type parameters to index.
  - A trait implementation is made known by loading its definition, which a built
    module doesn't have, so a module that defines one makes it known in an
    `@entrypoint`, with `std:traits`' `implement(trait, type, adapter)`, as
    `std:zsharp/clr` does.

## Projects

`std:project` is a minimal project compiler, written in Z#, to bootstrap the real one.

- **Discovery.** `discover(directory)` gives a `Project`, which holds its root
  `Submodule`. A submodule is a directory with a module file, `.zs`. Its other `.zs`
  files are its sources, and so are those of a directory in it without a module file;
  a directory in it with one is a submodule of it. Files and directories are taken in the
  order of their names.
- **Compiling.** `Compiler(project).compile()` runs the root's module file as a
  document, through the import system, and gives the `Instance`s that were emitted, in
  the order they were.
- **`core:project`.** A module file imports `compile` and `emit` from it. They are of
  `std:project/core`, what a module file runs in: the submodule running, what was
  emitted, and `compile` and `emit`, each two overloads. `std:project`'s entry point
  adds that module to `core:`, as `core:project`, so a module file can reach the rest of
  what it runs in too.
- **`emit(x)`** adds an instance to what compiling gives. A type is given as what
  describes it, through an overload for `HIR.Type`, so `emit(Int32)` emits the
  description, as a type is where a type is expected.
- **`compile()`** is a convenience, not a requirement:
  - It is a plain call, so it runs as the module file runs, in the document's scope:
    the sources see what the module file imported.
  - It parses the running submodule's sources as one AST module, named after the
    submodule's directory, or by `compile(name)`, and compiles it with the module
    meta-compiler on a fork of the compiler.
  - It builds the result and returns the `Module` HIR node, then runs the module file of
    each of its submodules, which emit for themselves.
  - Sources don't import one another, since they are one module.
- **The build script** decides what to do with what was emitted, as loading it and
  calling into it, or lowering it for a platform.
- **Not yet:**
  - Only the AST compiler is forked, and the HIR loader is shared. A module file runs as
    a document of its own each time its project is compiled, through `core:zsharp`'s
    `documents`, though importing it still gives what it gave when first imported.
  - A path is relative to where the toolchain runs.
  - A definition of a module that fails to load is left out of what it holds, so the
    failure shows only where the definition is used, and less clearly.
  - A syntax error in a source stops the build with the parser's exception.
  - There are no rules for names across submodules.
  - Writing an LIR file, and building the platform as a submodule, come next.

## The Cecil platform

`packages/cecil` is a CLR platform written in Z#, as a project: it writes LIR as a .NET
assembly through Mono.Cecil, which it imports as `pkg:Mono.Cecil@0.11.6`. A build script
imports it as `pkg:cecil`, a package that is a project, which compiles it and loads it
into the running platform, and gives it LIR:
`Cecil(name, version)` is an assembly being written, `add(module)` adds an LIR module,
`lower(module, target)` from `std:modules` gives a Z# module's LIR for it, and
`write(path)` writes the assembly.

- **Versions.** Given a version, as `net8.0`, it reads the newest reference pack of it
  installed with the running .NET, and finds each CLR handle there by its names: a type
  by its full name, a member by its name and declared signature, as keys LIR and Cecil
  give alike. What the pack doesn't have, as the toolchain's own assemblies, it reads
  from the files of the running process's, and the types their signatures name are the
  version's. Given null, it targets the .NET that runs it, and finds each handle in the
  running process, by its token.
- **CLR classes defined in Z#** are written as the runtime loads them: a class or an
  interface, its base and interfaces, its fields, constructors that make an object of
  its base first, methods marked as what they are, properties, and type parameters. A
  generic class's own code refers to its members through its instance over its own type
  parameters, as C# does.
- **What it writes.** A module is a static class of its functions and globals, whose
  type initializer is the module's initializer, marked as a module scope, so importing
  the assembly gives the module: `import { describe } from "Shop.dll"`. Its entry point,
  if it has one, is marked `[EntryPoint]`. A function with
  no module is in a class of its own. A structure is a value type. A polymorphic one, as
  a Z# class or interface lowers to, is a CLR class or interface: its fields are its
  own, its objects start with no code of their own, as the code that makes one sets its
  fields, and its methods are the functions of its module that are its methods, virtual
  so a method implements an interface's of its name and signature, and abstract with no
  code. A reference to a variable is a box the assembly defines for itself. Code is IL,
  as the runtime emits it, and the CLR handles are what they describe.
- **Live values** can't be written: a constant that is an object only the running
  process has fails, as does a type the running process made, which has no file.
- **It writes itself.** `packages/cecil/build.zs` builds it into
  `packages/cecil/build/cecil.dll`, with the platform `pkg:cecil` gives: the project,
  compiled, the first time, and the assembly after.
- **Builds are deterministic.** A module's identity is of its content and an assembly
  has no time of its own, so the same LIR is written as the same bytes, and a platform
  that builds itself builds what it was.
- **What a build needs goes with it.** `dependencies()` gives the files of the
  assemblies the written one needs that neither .NET nor the running toolchain has, as
  Mono.Cecil, with what they need in turn, and `std:dotnet`'s `bundle` copies them into
  the build's folder, leaving one already there the same, as it may be in use.
- **Not yet:**
  - A CLR method with a local function, whose captures only the running process has.
  - A Z# class's constructor is a function of its module, not a CLR constructor, and a
    class method is a method of its metaclass, so code outside Z# makes an object by
    calling the module's function. A capability for constructors could make them the
    class's own, as the platform realizes them.

## Bootstrapping

`bootstrap.zs` builds the toolchain's Z# projects from their sources, as an installer
would, and checks the build.

- **The standard library is its own seed.** A library of it is a project,
  `std/name/src`, built into `std/name/build`, which is what `std:name` loads: so its
  sources are compiled by the toolchain that loads its build, as a compiler that
  compiles itself is. The builds aren't sources, so they aren't in git: a release ships
  them, and a checkout is seeded from one.
- **Generations.** Each run of the bootstrap is a generation. It builds each library, in
  the order of what they need, with the libraries it loads, for the .NET that runs the
  toolchain. If any of them built something else than what was loaded, a new process,
  which loads what was built, is the next generation, until a generation builds what it
  loads, which it must within eight. A library is built only by a generation that loads
  what it needs built, and none of them built differently by it, as its assembly refers
  to theirs, so a library built for the first time from a document is built again by
  the next generation from its build.
- **A library is all of its module.** A member that fails to lower fails its build,
  as `std:modules`' `failures(module)` says, so a broken library isn't a seed.
- **A seed only has what was built.** The bootstrap runs on the seed, so it can only
  use what the seed has: a feature the bootstrap, the prelude, or what the seed decodes
  needs is built into the seed by one bootstrap and used by the next. A library that
  becomes a project keeps its document for one bootstrap, which seeds it.
- **The Cecil platform and the package manager** are built from their sources every
  generation, so nothing an older build of them lacks is needed: what earlier bootstraps
  built of them is removed before the prelude is imported, which then compiles the
  package manager from its source. The Cecil platform then builds itself, compiled from
  its source first and then as what it built, until two generations are the same, which
  they are from the first, and the standard library is built with it. The package
  manager is built once the standard library builds itself the same.
- `tests/run.sh` bootstraps before the tests run, so they run with what a bootstrap
  builds, and a library or platform that doesn't build itself the same fails the run.
- **Every library is encoded**, so importing its build gives the Z# module it was, as
  **Reflection and importing** describes, bound to what it was built as.
- **Not built, by what they are:** the prelude and `std:packages`, which assemble the
  environment, as documents do: `std:packages` binds the package manager, which each
  generation compiles from its sources, so a build of it couldn't refer to it.

## Packages

`pkg:` imports a package, with the package manager, which is a package of the
toolchain's, as the Cecil platform is, in `packages/package-manager`, written in Z#. Its
`build.zs` builds it as an assembly into its `build` with the Cecil platform, and
`std:packages`, which the prelude imports, loads
that assembly, from its bytes so it can be built again while in use, or else, until it is
built, compiles the project, which is how it is built the first time. Either is
`core:packages`.

- **Roots.** A package is found in roots, in order: the project's, `packages` where the
  toolchain runs from, then the toolchain's, `packages` next to the standard library.
  `roots` holds them, its initial ones defined by the package manager, and `add(root)`
  adds one after them.
- **What a package is.** In a root, `a/b` is a file, `a/b.dll` or `a/b/b.dll`, or a
  document, `a/b.zs` or `a/b/b.zs`, imported as files are, or a project, the directory
  `a/b` with its module file in it or in its `src`. A project built as an assembly into
  `a/b/build/b.dll` is that assembly, loaded from its bytes so it can be built again while
  in use. One that isn't is compiled by what the package manager is given,
  `compileProjectsWith`, which `std:packages` gives the project compiler, and what it
  emits first is the package.
- **Dependencies.** An assembly refers to what it was built against by name and version,
  as the Cecil platform's refers to Mono.Cecil. The running .NET asks the package manager
  for an assembly it has none of that name of, and the package manager gives the one a
  build put next to a built package it loaded, or else the one NuGet installed of that
  version, or else the newest.
- **Versions.** A version of a package, `Name@1.2.3`, is in a root as an assembly in
  `Name@1.2.3/Name.dll`, so a toolchain carries the packages its own packages need, as a
  release carries Mono.Cecil for the Cecil platform.
- **NuGet.** What no root has is looked for in the packages NuGet installed: `Name` is
  the newest version that isn't a preview, `Name@1.2.3` that one, and the assembly is that
  of the target framework most like the running one's. Nothing is downloaded yet.
- **The prefix** is registered by Z#: `core:zsharp`'s `importers` routes a prefix to a
  function, and a Z# function is a delegate where one is expected.

A Z# function where a CLR delegate is expected is a delegate of it: a delegate type
converts a value that can be referred to as a function into one, and the instruction
that points to the function takes the LIR function, which a CLR platform finds as the
method it declared. A function of no module can't be pointed to in the runtime, which
runs it as a dynamic method.

## Releases and installing

A release is what an install has, for a platform, as an archive that needs nothing else
installed: `release.zs` builds one, `zs release.zs [--no-bootstrap] [platform...]`, each
platform a .NET runtime identifier, as `win-x64`, `linux-x64` or `osx-arm64`, into
`release`, a `.zip` for Windows and a `.tar.gz` elsewhere, which keeps what runs as a
program. It runs the bootstrap first, so the seed is what the sources build.

- **What it has.** The toolchain, published self-contained in `bin`; the standard
  library's sources and builds, its seed; the toolchain's packages, with the versions of
  packages they import, found in NuGet's packages where the release is built; the
  bootstrap, so an install builds itself again from its sources; the design, the
  documentation and the examples; and the installer. A file at its root,
  `.zsharp-toolchain`, says it is a toolchain. `zs` finds the standard library above its
  own folder, so nothing in it names where it is installed.
- **The installer** is Z#, `install.zs`, which the release's own `zs` runs, through
  `install.cmd` on Windows and `install.sh` elsewhere, so it is one installer for every
  platform. `install [folder] [--no-path]` copies the toolchain to the folder, or to the
  user's own programs (`%LOCALAPPDATA%\Programs\ZSharp`, or `~/.local/share/zsharp`),
  and adds its `bin` to the user's PATH: on Windows the user's own variable, and
  elsewhere marked lines in the shell's profiles. `install --uninstall [folder]` removes
  both. A folder is replaced or removed only if it is a toolchain, so a folder of
  anything else is never written over, and the PATH or a profile is written only if it
  changes. What removes the toolchain that runs the uninstaller waits for it to exit,
  as a running program's files can't go, and on Linux what it hasn't loaded yet would
  be gone; a terminal whose folder is the toolchain keeps the folder, empty.
- **Arguments.** What follows a script on the command line is its own, `core:zsharp`'s
  `arguments`, as the release builder's platforms and the installer's folder are.
- **Versions** are a decision to come: an install is one toolchain, in a layout that a
  folder for each version can hold later. A failure, as a folder that can't be written,
  ends the installer with the runtime's exception, as Z# has none yet.

## Project layout

```text
src/
  Common/    CommonZ
  Core/      ZSharp.HIR, ZSharp.MIR, ZSharp.LIR
  HIR/       Language-neutral HIR extensions, such as ZSharp.HIR.Code
  Compiler/  The CG, TS, IR, loader and runner REIs and implementations, and
             ZSharp.Compiler, the compiler in use, which they make up
  Importers/ Language-neutral importers, such as the string importer
  Lang/      Text, Tokenizer, AST, Parser, AST -> HIR, the Z# language
  Library/   What `core:` and `std:` expose, of what is below them, which nothing
             below reaches
  Packages/  DotNet (CLR entities, pure structure, HIR level)
  Runtime/   IL loader, LIR -> IL
  CLI/
tests/
std/         The standard library: its sources, and its builds, the seed
packages/    The toolchain's packages: the Cecil platform and the package manager
bootstrap.zs Builds std and the packages from their sources
release.zs   Builds a release; install.zs, with install.cmd and install.sh, installs one
docs/        The documentation, every sample of which tests/ZSharp.Tests.Docs runs with zs
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
- **The IL loader describes the CLR from the running process, not the target.** A CLR
  platform built for another .NET version resolves the shared CLR handles against that
  version's reference assemblies, but the IL loader chose overloads and members against
  the running .NET. So code built for net8.0 can pick an overload net8.0 doesn't have,
  which fails only when its handle is mapped. The fix is for the IL loader to describe
  the target's reference assemblies, without loading them, as `MetadataLoadContext`
  reads them.
- **What still needs a design of its own.** The IL importer imports the CLR's entities
  and also compiles the CLR's definitions written in Z#, `DotNet_Class` and its kin, with
  local functions and their closures: the second could be a unit of its own, on top of
  it. A built class's metaclass is found by the name the platform gives it, the class's
  and `.Class`, rather than by what binds it, so a built Z# class's type object carries
  no id that `reflect` reads. And the bootstrap's list of libraries and what each needs
  says again what each library's imports say.
- **A module's global whose type's code uses the global.** A global is declared once its
  type is lowered, and lowering a class lowers its code, so a global of type
  `List[ClassObject]` that `ClassObject`'s code uses isn't declared when that code asks
  for it. `std:oop` types such a global by an interface instead. A global could be
  declared before its type is complete, as a class's structure is.

## Decided in the review of 2026-11-12

Decided in the review of the reimplementation, for the language features to come.

- **`let` and `var`.** A `let` local can't be set again after its declaration, and a
  `var` local can. This is the local's property, not its type's; any other form of
  immutability, such as a `const`, is the type's or a decorator's. The seed takes it in
  three bootstraps: `var` is added, every `let` that is set again becomes a `var`, and
  then a `let`'s second `Set` fails.
- **The prelude.** The CLI inserts `import "std:prelude";` into the single file it runs
  as a script, as an AST node, unless it's given `--no-prelude`. Nothing else inserts it:
  not the project compiler, and not a document a script imports.
- **`|` with `Error`.** Errors live together as a value, but values and errors don't:
  - `Error | Error` is `Error`;
  - an `Error` with a subclass of `Error` is the ordinary union of the two;
  - an `Error` with any other `T` is `IResult[T, Error]`.

  `T` and `Error` both cast implicitly to `IResult[T, Error]`, and an error value always
  converts as the error, even where `T` would take it too, as `Object` does.
- **Indexing a property.** `map.height[2, 6]` errors if both an indexed `height` and the
  value of a parameterless `height` take the index. This holds for now, and may change.
- **Namespaces on import aren't decided.** They won't be modules, and for now an import
  doesn't reach a namespace. Today an assembly's types are found by name alone, whatever
  their namespace, so two types of one name in one assembly have to be an error rather
  than one of them.
- **Integers.** The integer concept is a set of operator functions and casts. A
  fixed-width integer is a type of its own `Integer` meta-type, which gives its width.

## Choices and restrictions from 2026-11-09

Made without you on 2026-11-09. The choices are design for you to keep or change; the
restrictions are what isn't supported yet, with what it would take.

Choices:

- **Interface syntax.** `def Shape of Interface { ... }`, with `Interface` imported from
  `std:oop`, as any meta-type is given with `of`. Alternatives: an `interface` keyword, or
  a decorator.
- **What implements an interface.** A class's bases are the interfaces it implements, as
  `def Square : Shape`, since a Z# class has no inheritance. Alternative: a separate
  clause, such as `implements`.
- **Matching an implementation.** A class's method implements an interface's if it has
  the name, the number of parameters, and types that each go to the other, the return
  type included, and is a class method if the interface's is. Alternatives: matching by
  assignability in one direction (variance), or by explicit implementation.
- **Casts and type tests belong to Polymorphism.** `cast` and `test` were added to the
  capability, since knowing what an object is comes with objects reaching their class's
  methods. Alternative: a separate capability, such as runtime types.
- **Decorator names.** `@static` and `@class`, exported by `std:oop`, so a document that
  uses the CLR's `static` too imports one under another name. Alternatives: other names,
  or one decorator that overloading resolves for each kind of definition.

Restrictions:

- **An interface's methods have no code, and an interface has no fields.** Default
  methods would lower as methods of the interface the runtime realizes; fields would be
  properties, which `std:oop` doesn't have yet.
- **An interface doesn't extend interfaces.** The runtime's `Polymorphic.Interfaces`
  already allows it; `std:oop` would need an interface's bases and assignability through
  them.
- **A value of an interface doesn't give its class.** `shape.Class` would take each
  object reaching its type object, which a polymorphism operation, as `cast` and `test`
  are, could give.
- **Whether a match against a trait is dynamic depends on the implementations that
  exist when the code is compiled.** `Traits.isTrait` is "a type something implements
  now", so `std:oop`, which matches its target against `Polymorphism`, has to be imported
  after `std:zsharp/clr` gives the runtime polymorphism. A trait would need to be one by
  what it is, not by what implements it so far.
- **Only one target has polymorphism.** Lowering for each target is kept by target, but
  nothing yet lowers a class for a target without polymorphism.
