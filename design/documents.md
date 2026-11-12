# Documents, definitions and decorators

## Document level and modules

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

## Definition handling

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

## Decorators

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
CLR HIR's own module, `std:zsharp/package/dotnet`, supplies `@static`, `@virtual`,
`@abstract`, `@override` and the accessibilities for .NET definitions, and
`core:runtime` supplies `@explicitThis`, which names an attribute of the IL loader's.

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

## Members of a class

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
