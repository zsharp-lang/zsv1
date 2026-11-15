# Definitions and reification

## Definitions

`def`, `module` and `fun` are all definitions sent to a builder, so there is one
path for them. Every `AST.Definition` compiles to code that produces the definition
object as HIR.

A definition that is not cast has identity. Casting a definition node to its
underlying type is code generation: the object is structure only and can't be used
as a type. Python builds a class anew on each call and C# gives it one identity. Z#
wants both.

`HIR.Identity` is that: loaded once and kept, so loading it again gives the same CO.
`Definition : Identity`, since a definition is one kind, but not everything with
identity is a named slot a container holds — a module is an instance and not a
definition another one holds, and so is an `Import`, yet both are loaded once and
kept, the same way a definition is. The loader's cache for each has the same shape,
through separate entrypoints: `LoadDefinition` for what already calls it by that
name, `LoadIdentity` alongside it for everything else.

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

## Reification

Reification is code that builds code. It works on HIR through an REI, because it is
about describing structure as code and COs are virtually structureless. A cast
definition inside a body must happen when that body runs, so instead of constructing
the object, the enclosing code adds HIR to the body that will construct it. Reference to an existing definition object
becomes a direct `Get` of that object, not a name. Reification nests, and repeats a
small set of `HIR.Code` nodes such as `Call`, `Argument` and constants. A definition
reifies once, so further casts do nothing.

### Worked example

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

## Captures

A function defined inside the code of a method can use the variables around it, by
reference, as in C#. A body's meta-compiler makes a `fun` written directly in it a
`Package.DotNet.LocalFunction`, an HIR type of its own whose node holds its `Capture`s; a
method of a class defined in the body is still a method. Z# functions will have a
`Closure` meta-type of their own, so they can capture with different semantics.

A `Capture` (`HIR.Code`) is a variable that holds the variable it captures. The local
function's meta-compiler resolves a name bound inside the function as usual. Any other
name is resolved as the code around the function resolves it, and if that is a
variable of that code, or a member of one, as a member of the class is a member of the
object a method runs on, the function captures that variable. Nothing is bound again,
and nothing is marked:

- Code refers to a variable it has, a local, a parameter or the object it runs on,
  through a variable of the code that builds it, which starts as the variable's node and
  can be set again. The name is bound to that variable, and the definition lists the node
  itself, as its locals and its parameters.
- Capturing sets that variable, in the code that builds the code around, to a capture of
  what it held, before any code that refers to it is built: the function is built where
  it stands, and the code around after everything in it. So all of that code refers to
  the capture, before the function as after it, and the function holds the capture
  among its captures. The code around declares a captured local as its capture.
- A second function that captures the variable sets it again, to a capture of the
  capture, so each function holds its own. Outside a function, its capture is what it
  captures, so the code around reaches the variable through every capture of it.
- Resolving through the code around is what makes nesting work. A function two levels
  in resolves a name as the middle function does, so the middle function captures the
  variable, and then the inner one captures the middle one's capture, never knowing
  what the middle one captured.
- Once a `let` can't be set again, the variables of the building code that captures set
  are `var`s.

Only variables are captured, as `HIR.Variable` marks them: locals, parameters, the object
a method or constructor runs on, its `this` node, which a function captures when it uses a
member of the class by its name, and a local function, whose value exists only while the
code around it runs, which a function beside it captures when it calls it. The code refers
to a local function through the definition's variable, which its meta-compiler makes an
`HIR.Variable`, and it builds the node through a variable of its own. A definition or any
other instance is referred to as it is, as a static method's object is, which is its
class.

The same `Capture` loads differently in each place. Outside the functions that capture
it, it is what it captures, loaded as an instance, so in the capturee it is the
variable's own CO. (A parameter as a definition is what a call matches arguments to,
which is why a capture loads what it captures as an instance.) Inside a function that
captures it, it is what that function was given, and so is every capture it captured,
as each stands for the same variable: a function calling one beside it passes what it
was given. How a captured variable is stored is up to the platform, which finds what is
shared in the code itself. On the CLR:

- A local the code declares as its capture lives in a cell, an `LIR.Allocate`d reference
  made when its declaration runs, so one made in a loop is a new variable each time.
- A parameter, or the object, that the code refers to through a capture is copied into a
  cell when its function starts, the first time it is found to be, and the function uses
  the cell from then on, so a capture in a branch needs no special handling.
- A local function is a function of its own, which takes the cell of each variable it
  captures as a hidden parameter before its own, once for each variable however many
  captures of it it holds. A local function it captured is the variables that function
  captures, so it takes those too, and calls that function with them. Calling it passes
  the cells as the caller has them.
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

```zs fragment
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

## Compile-time evaluation

Some code runs while compiling: a document's statements, which include its own imports,
an `of`, a module's meta-types, and later anything marked `@ct`. All of it goes through
one operation: the code is loaded into the runtime as a method and run. A module's or a
body's own import is different: nothing of it runs while compiling, since it compiles to
an `Import` node and an `ImportedMember` of it for each name, resolved only once loaded,
as **Imports and reflection** describes.

An expression is compile-time if it refers to no `HIR.Variable`: nothing whose value only
exists while code that runs later is running. That is the boundary the reifier already
draws, between what it encodes and what it places as it is, so the check is a walk over
HIR with no MIR in it.

The runtime can't load code by itself: it has to make a method. So what loaded code
needs from its surroundings is put into its LIR as it is made. A document local reads
and writes its document's frame, which its CO refers to as a constant: the frame is an
object of the runtime, and the array of values behind it is read each time the code
runs, since it is replaced when the document gains locals. So any code loaded to run
now can use document locals, wherever it comes from: a document statement, an import
among them, or an `of`. Document level is meant to work with the runtime, not as
platform-agnostic structure, so tying it to the process is fine. A method compiled for
keeps never refers to the frame, because code that is built gets a document local's
value encoded instead.

A binding that code needs to work can't be LIR metadata: metadata is optional, and a
target must still produce working code if it ignores it. So what is needed goes in as
a constant, and metadata is for what is optional.

Source mapping is optional, so it is metadata: where code was written goes with it from
the parser to what a platform writes, and a platform that ignores it still works. HIR
knows nothing of it, as **Source mapping** below describes.

### Source mapping

HIR is structure, and knows nothing of a source or of mapping one: what says where code
was written is an annotation, as anything said of a node is, and only what makes it and
what reads it know what it means. So an importer of anything, even of assembly code, can
say where its code came from its own way, and a platform can read it its own way, both
from extensions.

`pkg:zsharp/sourcemap` has the Z# frontend's: `SourceMap`, a span of a document by line
and column, and the tables of where each statement of some code was written.
`sourceMap(holder, statement)` and `sourceMap(holder, statement, map)` give and set one,
for the code of an `Annotated` HIR node, as a `Code` is, and of a lowered LIR definition,
whose metadata it is. Each table is the holder's, so the package is definitions with no
state of its own, and code that is built, or written to a binary, says where it was
written by itself, needing nothing that runs. Only statements are mapped so far.

- **The parser** records where each node it gives was written, and the path of what it
  parses: an AST node has its `Location`, as AST is the source's own structure. Nodes of
  several files put together, as a module's sources are, each keep their own.
- **AST -> HIR.** What holds code makes it before compiling it, and compiles it where its
  table is a capability of the context, as a body does with its `Code`: the AST compiler
  puts each statement it compiles in the table the context offers. So a map is in the
  table of the code it belongs to from the start, and nothing else holds it. What the
  innermost compile gives keeps its own, and the code that fills a definition is where
  the definition was written, but for what says where it was written itself. Code that
  nothing holds, as a document's statements, which run as they are compiled, is mapped
  nowhere.
- **Reification.** A body is reified as its `Code`: the reifier's `ReifyCode` gives code
  that makes a new `Code` first, in a variable of the building code, and puts each
  statement it builds in that code's table, as constants. So the built code says where
  its statements were written, of its own nodes.
- **HIR -> MIR -> LIR.** Whatever lowers a function's code loads it with
  `CodeLoaders.Load(code, into)`, which says what it lowers into: a statement the code
  says where it was written of loads to one that lowers as it does, and puts what it
  lowers to in the table of what the code lowers into. Anything else asked of it is the
  statement's.
- **The platform.** The Cecil platform writes a sequence point at the first instruction
  of each statement its function's table has, into a portable PDB embedded in the
  assembly, with each source's checksum, and its locals by name, so a debugger and a
  stack trace through it show the source. The runtime platform writes nothing of it: a
  module Reflection.Emit makes in the running process has no debugging information.
- **A definition's location** is another thing: where a definition, a handle, was
  written, not where code was. It is an annotation of the definition, a single location,
  encoded with it, so a built library's nodes still have it once decoded. Not yet.
- **Not yet:** expressions, and a loop's or a condition's own points, as a statement's
  point is all of it.

Construction code itself only builds HIR — an import compiles to an `Import` node, not
a call it runs — so it can be compiled to a binary, as for precompiled libraries.
Encoding a value needs only the mapper's rules,
not the compiler. Exposed objects are the exception, but they are runtime nodes that
only exist in the process that made them, and nothing precompiled contains them.
