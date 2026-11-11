# The three cores

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
