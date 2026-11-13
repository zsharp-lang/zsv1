# Z# frontend

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
- Z# reaches the toolchain's types through modules made of its assemblies: the
  definitions under `pkg:`, as `pkg:zsharp/hir`, `pkg:zsharp/hir/code`,
  `pkg:zsharp/mir`, `pkg:zsharp/lir`, `pkg:zsharp/lang/hir`, `pkg:zsharp/lang/ast` and
  `pkg:zsharp/core`, and the compiler's layers under `core:zsharp/`, as
  `core:zsharp/mir/cg` and `core:zsharp/m2l`, listed once in `ToolchainModules`. A
  module's generic types are found by their name without their arity, all arities as
  overloads. What is live is under `core:`: `core:zsharp` gives `metaTypes` and
  `bodies`, and `core:compiler` gives `mappedLoader` and `mappedCodeLoader`.
- A CO lowers for the target it is given, and passes it on to what it lowers, so a
  Z# function lowers its code for the target its call is compiled for.
- Lowering is to be by capability: a CO asks the target what it can do and lowers to
  the richest form the target has, or to a shallow one built of LIR's core. The CLR has
  polymorphism, so a Z# class can be a class there, and a target without it gets
  records, tables of functions and `CallIndirect`. A capability is standard, not a
  platform's: its LIR nodes and what they mean are part of the standard, in
  `pkg:zsharp/lir`, and each platform implements those it can. So an LIR structure is a
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
  `std:traits/dynamic` replaces the matching REI's `Pattern` and `CG.Cast` for traits: a match or a cast
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
  Their methods still say `@explicitThis` and reach their fields through `this`, as they
  were written before a CLR method's code reached its class's members by their names.
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
  does, and whether it takes each is its meta-type's to say; a CLR module is `def Name
  of Module`, with the CLR's `Module`, of `pkg:clr/hir`. Z#'s definitions are the
  defaults, and a library that needs another meta-type says `of`, so what a library's
  definitions are doesn't depend on what was imported before it.
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
  `@operator((/))` with an operator literal, says of a definition that it is the
  operator, as an `OperatorAnnotation` among its annotations: what holds the definition
  understands it, as a Z# class finds its index operator among its methods' annotations,
  and a document or a module adds a function that is one of the functions a piece of
  syntax stands for to them (see **Operators** in `code.md`).
  HIR has no notion of an operator. A decorator takes and gives the definition, so
  decorators compose. `M[T]` is `CG.Index`, which a CO handles if it
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
