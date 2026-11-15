# std:oop

Z#'s own classes and interfaces, defined in Z#.

- **Classes.** A Z# class has no inheritance. It has fields, methods, properties and named
  constructors, and it implements interfaces. A method reaches its class's members by
  name, and calls the object `this` if its first parameter is `this`.
- **Interfaces.** An interface declares methods that a value of any class implementing it
  has.
- **Lowering.** A class or an interface lowers through the target's capabilities. A
  target with polymorphism gets it as an object type with methods.

The library is in two halves, each its own library:

- **`std:oop`** is the structure: the nodes of a class, an interface and their members,
  which is all that reflection needs, and the decorators that mark members.
- **`std:oop/compiler`** is what the compiler needs of them: their COs, their
  meta-compilers, and what binds the built ones.

Importing `std:oop` imports its compiler, through its entry point. That makes `def` a Z#
class wherever nothing else compiles one, and `def Name of Interface` a Z# interface.

## The structure: `std:oop`

| Node | Capabilities | What it is |
|---|---|---|
| `Class` | `Instance`, `Definition`, `Type`, `Named`, `Annotated` | a class: its fields, methods, properties, constructors and the interfaces it implements |
| `Interface` | `Instance`, `Definition`, `Type`, `Named` | an interface: methods with no code |
| `Field` | `Instance`, `Definition` | a field |
| `Method` | `Instance`, `Definition`, `Annotated`, `Overloadable` | a method. Its object is its first parameter, `ThisParameter`, unless it is static. Methods of one name overload. |
| `Property` | `Instance`, `Definition`, `Named` | what its accessors, the methods `get Name` and `set Name`, read and write |
| `Constructor` | `Instance`, `Definition`, `Annotated` | makes an object, runs its code on it, and gives it. The unnamed one is what calling the class runs; a named one is a member of the class. |
| `Receiver` | `Instance` | the object a constructor makes, as its code refers to it |
| `ClassOf` | `Instance`, `Definition`, `Type` | the type of a class's or an interface's type object: the class's metaclass, or the interface's class shape |

### Decorators

| Decorator | What it marks |
|---|---|
| `@static` | a member as the class's own, not its objects'. A static field is one variable, and a static method is a function with no object. |
| `@operator` | (from `core:zsharp`) a method as an operator, as `@operator(Operator.Index)` makes it the class's index operator: an `OperatorAnnotation` the class finds among its methods' annotations. |
| `@class` | a member as the class's type object's. A class field is one for the class, and a class method runs on the class. An interface's class method is one that the class of each of its values has. |

A class is a value: the one object of its metaclass, whose fields are the class fields
and whose methods are the class methods. So the type of any class that implements `Shape`,
its class shape, is `Type[Shape]`, as `Type[Square]` is the type of `Square`'s type object.

## The compiler: `std:oop/compiler`

### COs

The types and values:

| CO | What it is |
|---|---|
| `ClassObject` | A class. As a type, it is the object type its target lowers it to. Calling it runs its unnamed constructor, and its named constructors are its members. A value of it has its fields and methods as members. |
| `InterfaceObject` | An interface. As a type, it is the object type of any class that implements it, which the target's polymorphism makes. A value of it can be cast down to, and tested for, what implements it, through `IRTMatch`. |
| `Metaclass` | A class's metaclass: the type of its type object, which is one object per target, a global of the class's module. It implements the class shape of each interface its class implements. |
| `InterfaceMetaclass` | An interface's class shape: the type of the type object of any class that implements it. |
| `TypeObject`, `InterfaceValue` | a class's value, its type object, and an interface's value |
| `Narrowing` | a value of an interface cast down to a class or another interface, or tested for being one, through the platform's polymorphism |
| `Retyped` | a built class's object seen as the CLR type it was built as |

The members:

| CO | What it is |
|---|---|
| `MethodObject` | A method, a definition its class or interface holds and lowers into its module. Its function is made when it's asked for, before its code is lowered, so code can call it. An interface's method has no code: calling it calls the object's class's method. |
| `ConstructorObject` | A constructor. Its function makes an object of its class's structure with each field's default value, runs its code on it, and gives it. |
| `Constructed` | the object a constructor makes, as the constructor's code uses it |
| `FieldValue`, `FieldSet` | an object's field, read and written |
| `StaticField`, `StaticFieldSet` | a static field: one variable, a global of the class's module |
| `PropertyValue` | a property: reading it calls its getter, and writing it calls its setter |

Calls:

| CO | What it is |
|---|---|
| `Bound` | what is reached through an object, as a method is: calling it calls the callee with the object first. It implements `ICTBound`, so a delegate of it is closed over the object. It knows nothing of what the callee is. |
| `Invocation` | a call of whatever its callee is referred to as, for the target |
| `Evaluated` | a call evaluated as a statement, for what it does |
| `Known` | a value that exists as the code is compiled, as a definition's node does |

### How it plugs into the compiler

| Hook | Function | What it does |
|---|---|---|
| `@metaType(Class)` | `compileClass` | builds a `Class` from a `def` |
| `@metaType(Interface)` | `compileInterface` | builds an `Interface` from `def Name of Interface` |
| `@metaType(Method)` | `compileMethod` | builds a `Method` from a `fun`, `get` or `set` in a class or an interface |
| `@metaType(Constructor)` | `compileConstructor` | builds a `Constructor` from a `new` |
| `@definitionLoader` | `loadClass`, `loadInterface`, `loadMethod`, `loadConstructor` | a node as a definition gives its CO |
| `@instanceLoader`, `@typeLoader` | `instanceOf...`, `typeOf...` | a node used in code, or as a type, is its one CO. A `Receiver` is what its constructor defined it as, in the code's scope. |
| `@binder` | `bindClass`, `bindMethod`, `bindConstructor` | a built class, method or constructor is what it was built as |
| `@entrypoint` | `install` | makes `Class` the default meta-type of a `def` |

A class's or an interface's members are compiled within a context that is its members
(`Members`). That is how the meta-compiler of a method or a constructor finds them:
- a `let` is a field;
- a `fun` or a `new` written directly in it is a method or a constructor;
- a definition inside a member is the member's to compile.

`ClassFiller`, `MethodFiller` and `ConstructorFiller` fill each node as it is built. What
a class is given as its bases are the interfaces it implements.

### State it keeps

- **Structures being built** (`beingBuilt`, `deferred`). While a structure's field types
  are lowered for a target, a class used as a type is only its structure, so a class's
  methods can use a class whose field has its type. The classes that waited are lowered
  once no structure is being built, in order.
- **What was built** (`builtClasses`, `builtMethods`, `builtConstructors`): each built
  class, method and constructor as what it was built as, which its CO uses instead of
  lowering it again.

## What it depends on

- **`std:functions`:** its parameter nodes, `Positional` and `Named`, which a method's
  and a constructor's parameters are.
- **`std:zsharp/capabilities`:** `Polymorphism` and `PlatformInterfaces`, the target's
  polymorphism.
- **`core:runtime`**, **`core:zsharp`** and **`core:compiler`**.
- **The toolchain's packages and layers:** HIR, MIR, LIR and the AST, under `pkg:`, and
  the compiler's protocols, under `core:zsharp/`.
- **`std:framework`** and **`std:zsharp/lowering`**.

## Open points for review

- **One file of 2,300 lines.** The compiler is a single source holding types, members,
  calls, build state and meta-compilers. It would read better as a file per section.
- **`Bound` and `Invocation` are language-neutral.** Nothing in them is about classes:
  they bind an object to any callee, and call whatever a callee is referred to as. They
  would fit a shared library, where any definition that reaches a function through an
  object could use them.
- **`Known` is defined twice**, here and in `std:functions/compiler`.
- **A built class's metaclass is found by its name**, the class's name plus `.Class`
  (`builtTypeObjectField`), rather than bound by its kept metadata. Because of this,
  `reflect` of a built class's type object gives null. It's a follow-up.
- **Global state**: the build state above is module globals, as in the other two
  libraries.
- **An interface has no annotations.** `Interface` isn't `Annotated`, while `Class` is, so
  a decorator can't say anything of an interface that is kept with it.
