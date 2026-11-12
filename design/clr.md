# CLR target

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

An interface is a type definition with `of Interface`, the CLR's HIR's, of
`pkg:clr/hir`, so it needs no keyword; one can be added later, in Z#. Its methods have
no body, written `fun f(): T;`, and its bases are interfaces it extends. A class's bases
are sorted as it is constructed, once they are built: an interface is one it implements,
and anything else is its one base class. `@virtual`, `@abstract` and `@override` come
from CLR HIR's own module, `pkg:clr/hir`, as `@static` does, so methods aren't virtual
unless marked, as in C#. On the CLR, the IL loader defines an interface through a
class-shaped view of it, which the class machinery handles, and maps the type back to
the interface's node. A method that implements an interface method by its name is made
virtual and final, as C# makes it, so the CLR maps it; interfaces are created before the
classes that implement them. An interface that already exists, as one of the CLR's,
isn't defined again. Calls on an instance already go through `callvirt`, so dispatch
needs nothing more. Properties, explicit implementations and default interface methods
aren't supported yet.

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
- `value is T` is a `Match` in HIR, which stands for the matching REI's pattern
  match, with the type as a value pattern. A CLR type answers it through `IRTMatch`: a reference is
  checked as the code runs, so `null` is of no type, and a value of a value type is
  known to be of a type or not.
- What follows `is` is a pattern, a node kind of HIR's own (see the patterns in the
  review's decisions). A value pattern, as a type, is matched with the match operator.
  `value is let name: T` is a name pattern: it declares `name` as a local of `T`, through
  whatever compiles a `let` where the match is, so it is a variable like any other, read
  and assigned after the match. It loads to a binding, which the matching REI asks first:
  the value is evaluated once, matched against `T`, and if it matched, the binding's
  target, which is anything that can be set, is set to the value cast to `T`. `var` binds
  one that can be set again. A name pattern needs its type for now, since a local is
  declared with its type or its value, and a binding has no value where it is declared.
- A `params` parameter takes the rest of the positional arguments, through the argument
  stream as any parameter takes what it needs: one argument that is already an array of
  its type is the array, as C#'s normal form has it, and otherwise each is converted to an
  element, and an array of them is made through a function handle mapped to `newarr`.
- `Ref[T]` and `Out[T]` are the types of references to variables of `T`, which `ref` and
  `out` parameters take. They look like generic types but aren't: `Ref` and `Out` are
  `Package.DotNet.RefFamily` nodes, which `pkg:clr/hir` exports, and indexing one gives a
  `RefType`, which the CLR has as `T&` either way. A static readonly field of a library's
  module scope that holds HIR is exported as that HIR, which is how `pkg:clr/hir` gives
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
