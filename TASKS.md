# Tasks

What is to be added, in the order it is being done. `docs/internals/architecture/` records what is decided;
this records what is left. Each item says what it is, and an item that waits on a
decision says which.

Legend: `[x]` done (with its PR), `[~]` in progress, `[ ]` to do, `[?]` deferred: waits on
a decision.

## Standard library: Z#'s own types

The concept libraries of the review notes on #19, split from the CLR's: the CLR maps
them to its own types when it lowers them, so the runtime is one platform among others.

- [x] **Operators from libraries and documents** (#38). `@operator((+))` on a function a
  module or a document holds registers it among the functions the syntax stands for.
  - [x] An operator literal, `(+)`, in any expression: a value, `SyntaxFunctions`, that
    names the functions by their syntax, so it encodes as the syntax.
  - [x] A document registers the operators it defines.
  - [x] `std:modules` registers a module's operators as it holds them, so a built
    library's register when it is imported.
  - [ ] `(+)` used as a callable, as `(+)(1, 2)`: the literal is only a value naming the
    functions so far.
- [x] **`std:numbers`** (#39). `FixedBitWidthInteger` and `FloatingPoint`, meta-types
  whose values say what a type's numbers are; a type of them is a definition, as `def I32
  of FixedBitWidthInteger(32, true) {}`, and every node of one kind, width and signedness
  is one type.
  - [x] `std:numbers/integer`: `I8` … `I64`, `U8` … `U64`, with `+ - * /`, negation for
    the signed ones, and the comparisons, declared with no code.
  - [x] `std:numbers/real`: `F16`, `F32`, `F64`, the same.
  - [x] The CLR's mapping in `std:zsharp/clr`: the `Numbers` capability, what represents
    each type (`represent`), and the operators premapped to the CLR's, an integer
    narrower than 32 bits computed as an `Int32` and narrowed back.
  - [x] Casts: to and from what represents a type, and between types of numbers.
  - [ ] Implicit widening between Z#'s types (`I8` to `I32`, `F32` to `F64`): needs a
    conversion the platform implements, as a cast does.
  - [ ] `%`, the bitwise operators and shifts.
  - [ ] Comparisons give the CLR's `Boolean`; they give `Bool` once `std:boolean` is in.
  - [ ] `I128`, `U128` and `F128`, which the CLR has only partly (`Int128`, no 128-bit
    float).
  - [ ] Casting a type of numbers to what describes it, `I32 as Instance`: its CO already
    uses `Cast(value)` for a cast to it.
- [ ] **Premapping is the platform's.** Every function lowers to LIR, a declared one to
  a function with no body, which LIR doesn't judge: the platform does. A platform premaps
  an LIR definition onto something of its own (the runtime: a method or an emitter;
  Cecil: its own), so it doesn't realize it as code; an empty one it hasn't premapped is
  its error. `to` is each platform's own type, so it is each platform's API, not a shared
  capability. `std:functions/compiler`'s `premap` table and the `FunctionHandle` of a
  declared function go (by a transition: the seed binds `premap`), and `@binder`'s built
  functions are the runtime's premaps.
- [ ] **Embedding data is a platform capability.** Asked to embed some data, a platform
  gives a node that reads it back once its output runs. The encoder's blob is one use,
  composed with its decoder; an asset is another. Cecil's `embed`, which knows the
  reflection blob's attribute, becomes it. Reflection itself doesn't change, but the
  prelude premaps `reflect` with `std:functions`' `premap`, which goes.
- [ ] **The CLR's types have no operators of their own** where a std type is what they
  are: `Int32` goes to `I32`, whose `+` the platform premaps, so nothing is ambiguous.
  `core:runtime`'s `[Operator]` methods are no longer the language's. The IL loader,
  which translates the CLR to Z#, premaps the CLR's operators. Note: the change spans two
  generations of the seed; `String`'s stay until `std:string`.
- [ ] **An import loads what it imports.** `import "lib";`, which binds no names, gives
  the module's node and nothing loads it, so its module's CO, which makes its operators
  the language's, is never made. Importing is using it, names or not. Then
  `std:zsharp/clr` no longer loads the libraries it implements.
- [ ] **The default string's encoding** is fixed width, so indexing is O(1), and can
  change without changing an API: the prelude's `String` is typed `Type[String[Unicode]]`,
  so code sees only what a string of the set has, never the encoding. `Type[T]`, a type
  known only to be a `T`, needs handling in std, not in core.
- [ ] **`x as A as B`** parses as `x as (A as B)`; left, `(x as A) as B`, as C# chains it,
  is proposed. Easy to turn back.
- [ ] **Overloads match exactly first.** A candidate every argument goes to without an
  implicit conversion is chosen over one that needs one; then the most specific, as now.
- [ ] **Z#'s types and the platform's convert implicitly, both ways** (decided). A type
  goes to what represents it and back. Code uses std's types; the platform's are the
  runtime's or Cecil's own, so an operator on one of each is ambiguous, and a cast says
  which.
- [ ] **Literals into Z#'s types.** An integer or decimal literal goes to any of the types
  that holds its value, and where nothing asks for a type, it is what the prelude sets
  (`I32`, `F64`). Libraries built without the prelude keep the CLR's.
- [x] **`std:boolean`** (#40). `Bool`, of the `Truth` meta-type, with `==` and `!=`,
  which the CLR runs as its `Boolean`; casts to and from it.
  - [x] A Z# type the platform realizes as one of its own says so (`IRepresented`), so
    it boxes where an `Object` is expected and isn't compared as a reference (a bug of
    #39: `print(5 as I32)` crashed).
  - [x] Conditions taking `Bool`: neither MIR nor LIR types a condition, so `if (b)`
    with `b: Bool` already works.
  - [ ] Comparisons giving `Bool` (std's are declared to give `Boolean`).
  - [ ] `not`, `and` and `or` on `Bool`: `not` and `and` aren't operators of the language
    yet, and `(or)` isn't an operator literal, as `or` is a word.
- [ ] **`std:void`: `Void`**, its own library. A function that returns `Void` returns
  nothing: it has no value, so a `let` can't hold its call. Today a function with no
  return type has `null` for one, and Z#'s functions, the CLR's methods, the check that a
  body returns what it says, and LIR's signatures each special-case it: each asks the type
  instead. A function with no return type written returns `Void`.
- [ ] **`std:unit`: `Unit`**, its own library: a type with one value.
- [ ] **`std:never`: `Never`**, its own library: a type with no value, which goes to any
  type, the type of what doesn't return.
- [ ] **`std:string`.** `Text[X]`, of an `Encoding` (concrete) or a `CharSet` (any
  encoding of it), `Char[CharSet]`, casts between encodings, and `String` the alias the
  prelude sets. Its element is a character of the set, `Char[CharSet]` (decided); the
  encoding's units are apart.
- [?] **`std:collections`.** Conceptual list and dictionary types, related to strings but
  apart from them. Waits on generic Z# classes or interfaces, which don't exist yet.

## Language and toolchain

- [x] **Imports have their own HIR nodes, `Import` and `ImportedMember`** (on
  `lang/overload-groups`). An import compiles to an `Import` (its source, as code) and,
  for each name it brings in, an `ImportedMember` of it, bound to that name — no
  `Code.Local`. Loading an `Import` evaluates its source (reusing the existing HIR
  runner, the same machinery compile-time evaluation already goes through) and expects
  an `HIR.Instance` back; loading an `ImportedMember` loads its `Import` and calls
  `CG.Member` on the result. Both are `HIR.Identity` (see below — not `Definition`,
  caught on review), so the loader's own cache keeps an import from running twice.
  `Bodies.Imported` keeps its old
  signature (`IResult<Code.Local?, Error>`, always `null` now) since already-built std
  libraries call it directly — only its insides changed, an additive transition the seed
  survives. This is what the `Code.Local` it replaced couldn't be: specific enough for
  `CG.Member` to resolve a member on it, whether that member is a parameter's type, a
  field's type, or another import's source. Verified: a plain `fun f(x: Object): Object;`
  and a chained `import { Object } from "core:runtime"; import { Path } from
  Object.Assembly;` both resolve now, and a full bootstrap completes against std's
  original source (no `core:runtime` moved to a project file, only the 19 `of`-related
  moves, which are unrelated and stay).
- [x] **Overloading: done, on `lang/overload-groups` (stacked on `lang/imports`).**
  - `MappedBinder`'s `Merge`/`Joined` now extends an existing `HIR.Code.OverloadGroup`'s
    `Items` in place instead of building a new group each time, so every reference to a
    name sees every contributor regardless of when each was bound. `ImportedMember` is
    `HIR.Overloadable`, so an import and a same-name definition merge instead of
    conflicting — checked both ways (reverting the marker reproduces "X is already
    defined here").
  - Operators needed no change: `Language.Add`'s table already mutated its group in
    place (`Functions[syntax]`'s `OverloadGroup` is created once and extended, never
    replaced), so a `+` compiled before a later `@operator((+))` registration already saw
    it. What looked like "the overloading plan" applying to operators was really this,
    already there before any of this work.
  - The planned new `IRTCastTo` CO turned out unnecessary, checked directly: with
    `Import`/`ImportedMember` in place, a group's contents are always known by load time
    (nothing is a building-code-run-time value any more), so `Overloading.Static`'s
    existing `OverloadGroup.Cast` (`ICTCastTo`) already filters a group to the one
    candidate of the requested kind. Verified with two imports sharing a name, one
    type-kind and one not, cast both orders (order-independent, so it is genuinely
    filtering, not taking the first); a group with no valid candidate still errors
    correctly. That part of the plan predated `Import`/`ImportedMember` and the problem
    it was for no longer exists — nothing built.
  - **Deferred on purpose, unchanged:** importing a module doesn't feed its
    `@operator`-annotated definitions into `Language.Functions`; nothing in `Import`/
    `ImportedMember` does that. Left for a later phase, per the user's call.
- [x] **`HIR.Identity`, separate from `Definition`** (same branch, after review). `Import`
  and `ImportedMember` were marked `HIR.Definition` only to borrow its loader cache;
  `Definition` means something specific (a named slot a container holds), which neither
  is. Added `HIR.Identity` (`Definition : Identity`) and a parallel `LoadIdentity`
  entrypoint with its own cache and its own map (`Identities`), leaving `LoadDefinition`
  itself untouched — name, signature, cache — since already-built std libraries call it
  directly. `MappedLoader.LoadInstance`'s fallback also tries `LoadIdentity` for an
  Identity that isn't a Definition, so this is reached however the node is referred to,
  not only through its own registered rule (a real gap my first attempt left: an
  `OverloadGroup` loading a candidate loads it directly, not through `Get`). `Import`/
  `ImportedMember` moved from `Definition` to `Identity`, and so did `Package.DotNet.Module`
  — it already documented itself as "an instance and not a definition another one
  holds," correctly, but was registered on the uncached `Instances` map, so loading the
  same module node twice built two different COs. Verified directly, with a counter, both
  times: an import's source evaluated once despite three names used four ways (the
  `Import` case); `LoadModule` called 11 times without the `Module` fix and exactly once
  with it, for two imports sharing one source. Bootstrap completes. Folded into
  `docs/internals/architecture/imports.md`, `docs/internals/architecture/code.md` and `docs/internals/architecture/definitions.md`.
- [ ] Named constructors on generic CLR classes.
- [ ] `import * as X` and `@importer` (need design).
- [ ] Function values: partial application, Bound/Invocation.
- [ ] The remaining patterns: `not`, sequence, `{}`, `case`.
- [ ] Compile-time access checks (need design).
- [~] **Source mapping through custom metadata, the whole chain: AST -> HIR -> MIR ->
  LIR -> platform.** HIR knows nothing of a source: where code was written is an
  annotation of what holds the code (`Code` is `Annotated` now), so an extension can map
  any source its own way. `pkg:zsharp/sourcemap` (`src/Packages/ZSharp.SourceMapping`) is
  definitions only: `SourceMap`, `CodeMaps` (a `Code`'s annotation) and `FunctionMaps`
  (an LIR definition's metadata), with `sourceMap(holder, statement)` /
  `sourceMap(holder, statement, map)`. The parser records each AST node's `Location`; `Bodies`
  makes a body's `Code` first and compiles the body with its table as a capability of the
  context, which the AST compiler puts each statement in, so no mapper holds anything, and
  a definition's fill takes the definition's; the body is reified through the reifier's
  new `ReifyCode`, whose building code makes the `Code` first and puts each statement in
  its table as constants, so built code needs nothing live;
  `CodeLoaders.Load(code, into)`, which the IL loader, `std:functions` and `std:oop` lower
  bodies with, puts each lowered statement in its function's table; the Cecil platform
  writes sequence points, source checksums and named locals into an embedded portable PDB.
  A stack trace through a Cecil-built assembly shows the `.zs` file and line. Statements
  only, for now. Folded into `docs/internals/architecture/definitions.md` (**Source mapping**) and
  `docs/internals/architecture/cecil.md`.
  - [ ] A definition's location: an encoded annotation of the definition, a single
    location, set by the code that builds it. A language server's go-to-definition wants
    it; a PDB has nowhere for it.
  - [ ] Expressions, and finer points: a loop's condition and a statement's parts have no
    points of their own.
  - [ ] The runtime platform writes no debugging information: a module Reflection.Emit
    makes in the running process can't have any.
  - [ ] A document's language isn't in the PDB: Mono.Cecil writes only languages it knows.
  - [ ] Crossing this change from an older seed takes about eleven generations, over the
    bootstrap's eight: libraries no others need are built by the seed's `std:functions`
    and `std:oop` compilers before the new ones are, and nothing in what each library
    needs says it is compiled by them. Running the bootstrap again finishes it.
- [x] **Nine import tests of `ZSharp.Tests.Classes` failed** since the import redesign,
  hidden because the project didn't compile. Three bugs, and three tests of the old
  design:
  - The reifier placed an `ImportedMember` as a constant, so every build shared one
    `Import` whose source still named the outer function's parameter: it builds them now,
    once per build, from the source as the building code holds it.
  - A body's import loads as the body is lowered; a document it imports then defined its
    definitions inside that lowering, which loaded them into the runtime with no code
    yet (`ldnull; throw`, so a `NullReferenceException`): `ILLoader.Define` lowers apart
    from a lowering under way now.
  - A name an import's source has no member for was "unsupported", so the loader tried
    others and said "No loader is registered for ImportedMember": it is "The import of X
    failed" now. And an import's source is evaluated on its own, outside the code it is
    in, so one that uses a variable of that code fails clearly.
  - Tests of the old design: extension methods through a module import (now a document
    import), an unknown name a module never uses (now one it uses), and the error text.
- [ ] A module can't import a class only for its extension methods: its import brings
  nothing until a name of it is loaded, and nothing names the class. C#'s `using static`
  is what it would be; a module's imports would need to be reached as the module loads.
- [ ] Deferred: resolvers, the IL importer split, metaclass binding, namespace imports.

## Language server

A real-world use of Z#: LSP support for VS Code, written in Z# as far as it goes. Three
packages: `pkg:lsp` (the protocol as types), `pkg:fastlsp` (a framework in the spirit of
FastAPI) and `pkg:zsharp/lsp` (the server for Z#). It starts small: no execution of user
code, source mapping is its own task.

- [x] **`pkg:lsp`** (`packages/lsp`). Structures, enumerations and the table of methods,
  generated from the specification's model by `generate.zs`.
  - [ ] Real unions, and optionals, in place of `Object` and `null`, once Z# has them.
  - [ ] The base protocol, JSON-RPC's messages and errors, which the model doesn't have.
- [~] **`pkg:fastlsp`** (`packages/fastlsp`). JSON in Z#, the server and its lifecycle,
  transports, `@request`/`@notification`, and `build`, which generates the readers,
  writers and `register` as HIR. Runs `examples/lspserver` over stdio.
  - [x] `publish(method, params)`: an endpoint sends a notification through the server
    `register` was given; the server sends it after the response to the message.
  - [ ] Typed notifications and server-to-client requests: `publish` takes JSON, so what the
    server sends is written by hand. Readers and writers for a type no endpoint reaches.
  - [ ] Union values that are Z# classes, and parameters of an endpoint beyond the first.
  - [ ] Cancellation, progress, and a response to a request the server made.
- [~] **`pkg:zsharp/lsp`** (`packages/zsharp/lsp`). The small server: it parses a document
  tolerating its mistakes (`Recovery`, in `ZSharp.Syntax`), runs none of its code, and
  answers diagnostics, the outline, definition (any declaration of the name in the
  document) and hover (the declaration's first line). `serve.zs` is the program an editor
  starts, over stdio; `test.zs` talks to it in memory. A VS Code extension is a separate
  project that starts `zs packages/zsharp/lsp/serve.zs`.
  - [ ] Scopes, so definition finds what a name refers to and not every declaration of it.
  - [ ] References, rename, completion, semantic tokens, folding.
  - [ ] Import sources: definition into the file or package, completion of `std:`/`pkg:`.
  - [ ] Custom syntax: the parser is the language's own, so a document that extends it
    needs its earlier statements run, which is the sanitized runtime.
  - [ ] The sanitized runtime, and source mapping through the whole chain.
  - [ ] Start-up is about 5 seconds, as the packages are compiled from source each time:
    build `pkg:lsp`, `pkg:fastlsp` and `pkg:zsharp/lsp` with the Cecil platform.
    `packages/lsp/build.zs` (which encodes the module and embeds the reflection blob, as
    `bootstrap.zs` does) writes `pkg:lsp` in about 3 seconds. Nothing is built into
    `build/` yet. `pkg:fastlsp`
    and `pkg:zsharp/lsp` are untried.
    - [x] The package manager imported a built project with `ilLoader.Load`, which skips
      the describers, so the embedded metadata was never read and a Z# class was seen as
      a plain CLR one (statics gone, "Type has no member named Error"). It uses
      `importBuilt(path)` now, new in `core:runtime`: the assembly read from its bytes,
      imported as importing its file is. `packages/package-manager/build/Packages.dll`
      was built again with it, so an older toolchain can't load it.
    - [x] **A field typed as another Z# class of a built assembly rejected its own objects**
      (`range.start = Position()`): a built class's CO and the CLR type its field has were two
      objects for one type, and neither was assignable to the other. A built class goes where
      what it was built as goes, and what it was built as goes to it (`ClassObject`'s
      `IsAssignableTo` and `IsAssignableFrom`, which ask `ts`).
    - [x] **A built class's static values were lost** (`DocumentHighlightKind.Write` gave 0):
      a static field of a built class was made again as a global of a module that is never
      initialized. It is the static member the class was built as now (`builtStaticField`),
      found by the class's name and its own, numbered after a `$` where a static is named
      `Class`.
    - [x] **A field of a generic instance of a built class rejected a new instance**
      (`s.children = List[DocumentSymbol]()`): an instantiation over a Z# class wasn't the
      closed CLR type a field has. `Instantiation` goes to a closed type of its definition
      whose arguments are the same types.
    - [ ] `pkg:lsp` and `pkg:fastlsp/runtime` build and work as assemblies
      (`packages/lsp/build.zs`, `packages/fastlsp/runtime/build.zs`): the runtime is the
      JSON, the server, the transports and the helpers that generated code calls, split
      from the generator so that code imports it as any library. `pkg:fastlsp`, the
      generator (`packages/fastlsp/build.zs`), doesn't build: "A value only the running
      process has, a Function, can't be written in an assembly." Its `build` refers to the
      runtime's functions as HIR, `ref(decodeString as Instance)`, and `x as Instance` of
      an imported function is still a constant when a body is written, so the split
      didn't change it; a function of the module itself is the same. `reflect(decodeString)`
      does build, but calling it from the assembly fails with "A function handle is not
      mapped": the Cecil platform doesn't map `reflect` (docs/internals/architecture/imports.md: an assembly
      "calls `DescribeType` of the loader that runs it"). Until then `pkg:fastlsp` and
      `pkg:zsharp/lsp` compile from source, with a start-up of about 2.4 seconds. Commit
      nothing built.

### Gaps found during LSP implementation

What the language, the toolchain and the standard library lacked, found by building the
packages above. Each says what happened and what was done instead.

Language:

- [ ] **No unions and no optionals.** The protocol is full of both. A union is an `Object`
  with the union in a comment, `T | null` is `T`, and an optional value type is
  `Nullable[T]`. (`pkg:lsp`.)
- [ ] **`Object` can't be told from a Z# class.** `object is let x: Foo` fails with "A value
  of Object can only be matched against a type" and `object as Foo` with "Object has no
  conversion to that type", when `Foo` is a Z# class. So an annotation has to be a CLR
  class (`of DotNet_Class`, as `std:framework`'s are, since the list of annotations holds
  objects), and a union field's value is read through `Unsafe.As[T]`, which doesn't check.
- [ ] **`Action[T]` fails.** `Action` with one type argument gives ".ctor can't be called
  with these arguments" with two overloads; `Func[...]` and `Action` with more arguments
  seem fine. Worked around with an interface (`Transport`) and `Func`s that return null.
- [ ] **No generic functions in Z#, and a CLR generic method can't use its own type
  parameter.** `def Codec of DotNet_Class { @static fun list[T](...) { List[T]() } }` throws
  `NotSupportedException` from `TypeBuilderInstantiation.get_TypeHandle` in the runtime's
  `Runtime.OnInstance`: `ContainsBuilder` doesn't see a method's type parameters. fastlsp
  generates a loop in HIR for each list and map instead.
- [ ] **A document's function is no delegate.** "Function X belongs to no module, so the
  runtime can't point to it". Handlers have to be in a `module`, which the project's
  sources are.
- [ ] **A document has no forward references**, so mutually recursive functions need a
  `module` around them, as `generate.zs` has (`module Generator`).
- [ ] **A global's type is looked up where the global is.** `let current: Server = null` in
  a source file that sorts before the one that defines `Server` fails with "Name 'Server'
  is not defined", though a function in it could name `Server`. Sources are compiled in
  the order of their names, so the file was renamed.
- [ ] **No way to write a name that is a keyword.** The protocol has properties named `from`
  and more: they get a trailing `_`, which fastlsp strips on the wire. And `module` as a
  variable name gives "Expected token 'Semicolon' but got '.'" with no hint that it is a
  keyword.
- [ ] **`and`, `not`, `for` and `try`** (already in `docs/learn/basics/not-yet.md`). Nested `if`s
  stand in for `and`; and, since Z# can't catch an exception, a parse error would end the
  server: `Recovery` is C#, for that reason alone.
- [ ] **Ambiguous overloads that C# resolves.** `text.Split("\n")` and `char == char` give
  "N overloads apply, and none of them is more specific than the others" (the C#
  overloads of `Split`, and the six of `Char` equality). Lines are split with `IndexOf`
  and characters compared as `Convert.ToInt32(c)`.
- [ ] **An array literal has no type of its own.** `let xs = ["a", "b"]` says the type of
  `xs` can't be inferred; `let xs: String[] = [...]` works.
- [ ] **`print(x)` ignores a Z# class's `toString`.** It prints the class's name.
- [ ] **A method's `this` has to be asked for.** Using `this` in `fun toString()` gives
  "Name 'this' is not defined"; `fun toString(this)` works. The message could say so.
- [ ] **A C# `params IEnumerable<T>` constructor can't be called with no arguments.**
  `Block()` of `HIR.Code` fails; `Code().Block` is a block that exists already.
- [ ] **A string literal may span lines.** A literal whose closing quote is on a later line
  was accepted with the line break in it, rather than being an error. Not investigated.

Toolchain:

- [x] **A `pkg:` project was compiled on every import**, so two importers got two
  different classes of one name and an argument of one wasn't the parameter of the other
  (`Object [Invocation] does not support implicit casting to [ClassObject]`). `std:packages`
  compiles a project once for each process now.
- [x] **A project built as an assembly was imported without its reflection blob**: the
  package manager used `ilLoader.Load`, which skips the describers, so a Z# class was seen
  as a plain CLR one and its statics were gone. `importBuilt(path)` of `core:runtime`
  imports it as importing its file does, from its bytes.
- [x] **A field typed as another Z# class of a built assembly rejected that class's objects.** See **Language server**.
- [x] **A built class's static values were lost, and a field of a generic instance of a
  built class rejected a new instance.** See **Language server**.
- [ ] **A definition cast to `Instance` in building code can't be written in an assembly.**
  `decodeString as Instance` (a function of the module, or an imported one: a four-line
  probe shows both) is a constant of the running process's HIR node, so the Cecil platform
  fails with "A value only the running process has, a Function, can't be written in an
  assembly", and building code that generates calls to a library, as `pkg:fastlsp`'s
  `build`, can't be built. Checked: **this is not a Cecil bug.** A live value can't be
  written, as `docs/internals/architecture/cecil.md` says, and the cast gives the node as a known value of the
  process (`FunctionObject.Cast`), which `docs/internals/architecture/imports.md` lists as not yet ("a
  description of a Z# definition in code a platform writes"). It needs a by-id reference
  for a node that carries a `ReflectionId`, lowered as a call that gives the node, in the
  language and not in a platform.
  - [~] **Cast a function in code, as `def` and `module` are** (`fun() { ... } as Function`):
    the cast handler already builds a definition's node as code when it is cast, which is
    what writes in an assembly; `fun` had no expression syntax, so it was loaded as a
    definition instead. A `fun` is an expression now (`Grammar.cs`), and `let x = 1` in
    a cast function no longer holds the process's `Infer` object (it is read from its
    class). A cast function with an empty body, with an import, and with typed lets and
    returns, builds as an assembly. Left: a call of a name an import brought in
    (`import { print } from "std:io"; print("x")`) still reifies the name as a constant of
    the group of definitions it names ("A value only the running process has, a
    OverloadGroup"), not as the `ImportedMember` that `CodeReifiers` builds for a group
    of them. Nothing of `pkg:fastlsp` uses the cast yet.
  - [ ] What was reported of `reflect(decodeString)` was something else. `reflect` is
    premapped onto `describe` by the prelude for any platform, and the Cecil platform
    isn't what fails. A function can't be passed as `Object` ("Object has no conversion
    from that value"), so the function that does it fails to lower, and **a build script
    leaves it out of the assembly without a word**: only `bootstrap.zs` checks
    `failures(module)`. Calling it then compiles a function with no code, a handle, which
    the runtime can't map: "A function handle is not mapped". `reflect(X as Type)` of a
    definition of the same module fails the same way ("A definition is only described by
    its node"). The build scripts (`packages/*/build.zs`) should fail on `failures`, as
    `bootstrap.zs` does.
- [x] **A library's runtime and its generator were one package.** `pkg:fastlsp` is split
  into the generator and `pkg:fastlsp/runtime`, which builds as an assembly.
- [ ] **Imports from the running .NET's other assemblies** (`System.Text.Json`,
  `System.Console`) need their file: `import { JsonDocument } from
  Path.Combine(Path.GetDirectoryName(Object.Assembly.Location), "System.Text.Json.dll")`,
  as `Object.Assembly` is `System.Private.CoreLib` alone. `RuntimeEnvironment` isn't found
  that way either.
- [ ] **Errors have no position.** "2 overloads apply...", ".ctor can't be called...",
  "Name 'X' is not defined" and the like say nothing of where, and a syntax error in a
  project's source names the project's module file, not the source. Finding each was a
  bisection by deleting code; running the parser's `Recovery` over each file found the
  syntax ones. (`source mapping` is its own task: this is what it would be used for.)
- [x] **A parse error had no position but when a token was unexpected, and a token had no
  text.** `UnexpectedTokenError` printed `ZSharp.Text.Token`. `Token.ToString()` gives its
  value now.
- [ ] **The AST has no span for a name.** A definition's `Name` is a string and its node's
  span starts at its keyword, so a selection range is found by searching the first line.
  `ImportedName` has no location at all.
- [ ] **No error-tolerant parse in Z#.** `Recovery.Parse` is C#, skipping to the next `;` or
  `}` after a statement that doesn't parse. A syntax that a document defines for itself
  isn't known to it.
- [ ] **Start-up** of a server is about 5 seconds (see above).

### Forking

- [ ] **Only the AST compiler forks.** `compiler.Fork()` copies its three endpoints and
  shares its `Contexts`; everything else is one per toolchain: the language's operator
  table (`Language.Functions`, which `@operator` definitions add to for all later code),
  the mapped AST compiler's maps (registering reaches every fork, earlier ones included),
  `metaTypes`, the compiler's REIs and loaders (`ZSharp.Compiler.Current`), the importers
  and `core:`. So an extension installed while one module compiles is installed for all
  that follows, and a build can't compile part of its code with other operators,
  meta-types or dispatchers. `docs/internals/architecture/composition.md` says no scoping is needed,
  which forking the rest of the toolchain would change. Written up in
  `docs/learn/advanced/build-scripts/toolchain-fork.md`.

## Rewriting Z# in Z#

The language's own C# is to be rewritten in Z#, which is the goal of the redesign (see
`docs/internals/architecture/principles.md`). It is not tidied in C# first: it is gone through when it is
rewritten.

- [ ] **Audit every part for layering and coupling, one by one, as each is rewritten.**
  The rewrite is the review: nothing is fixed in C# in the meantime beyond what the work
  at hand needs. What to look for, with the cases found so far:
  - A library that knows a platform it shouldn't: `std:oop`'s compiler holds a built
    class's entity as a CLR `ClrType` and reaches it through `ilLoader`
    (`bindClass`, `builtType`), when what a class was built as is the platform's and the
    importer's business, and `std:oop` should hold it as what it is, an entity it only
    passes back. (A check that asked the loader whether a type object was a CLR type was
    added and removed for this: assignability goes through `ts` now.)
  - A layer that reaches into the one above it, or a core that names a library.
  - A special case where a general mechanism would do, and an identity that holds only
    by reference (the loader's `Class(type)` makes a type object each time it is asked).
  - What a part knows that its dependency graph doesn't say.
- [ ] **The build-time halves of `std:` aren't standard libraries.** `std:oop/compiler`,
  `std:functions/compiler` and `std:modules/compiler` hold COs and meta-compilers, what a
  build uses, not what a running program does. They belong with the Z# packages (`pkg:`).
  The docs list them as they are today, with a note (`reference/packages/zsharp.md`);
  moving them is a seed transition, and the docs follow.

## Documentation

The docs are being rewritten on a Starlight site (`docs-site`), in a new structure: Learn
(getting started, basics, advanced, then custom syntax, importers, semantics, dispatchers
and platforms, and the CLI as what combines them), Reference (standard libraries and
packages, the core languages, the Z# language as a specification, the runtime) and
Internals (principles, the bootstrap, and `docs/internals/architecture/`).

- [x] The Starlight site, the new structure, and the existing pages moved into it.
- [x] Every page marked "To be written".
- [x] The existing pages, rewritten for where they are now, and each standard library a
  page of its own.
- [x] The layering stated, in `docs/internals/architecture/layering.md`: the layers as
  languages, the transformations and which are the core's, LIR dialects and pure
  capabilities, what a CO may rely on, opt-in everywhere, and preservability as a concept.
  The capability rules moved there from `frontend.md`.
- [~] The rest of the architecture record, gone through against the layering. Fixed so
  far: one Principles page (`docs/internals/principles.md`, with the record's principles
  under "The record"), LIR's concepts in `cores.md` no longer "Undecided", a CO's errors
  through the LIR REIs' results rather than an optional target, and `composition.md`'s
  "no scoping" pointing at forking. The pages of single parts (`clr.md`, `definitions.md`,
  `documents.md`, `code.md`, `extensions.md`, `imports.md`, `projects.md`, `cecil.md`,
  `toolchain.md`, `decisions.md`) are still to be gone through, as each part is rewritten
  in Z#.
- [ ] **Learn reads as a guide.** Each page builds on the ones before it and presents the
  problem before the solution, where the use case isn't plain. Feedback of 2026-11-16 up to
  code generation is applied; the pages after it get the same pass.
- Future fixes from that feedback, which need code or decisions first:
  - [ ] Installation: a guide, once the installer is what it is to be.
  - [ ] The standard types as built-in names: the prelude imports them, and the docs use
    them, not .NET's (`Int32`, `String`), which the prelude page then explains.
  - [ ] **Integrate the standard types properly**, which the item above needs first. `I8`
    to `U64`, `F16` to `F64` and `Bool` exist, but literals are .NET's (`42` is an
    `Int32`), nothing converts implicitly (`let x: I32 = 5;` and `twice(5)` for an `I32`
    parameter both fail), comparisons give .NET's `Boolean` and a condition takes only
    that, and there is no standard `String`, character, array, collection or function
    type. So every use is a cast, `5 as I32`, and back for .NET's calls.
  - [ ] `Void`, a standard type, and a function that returns nothing inferring it.
  - [ ] Whether a method's `this` is implicit or explicit by default; Classes says what is
    decided.
  - [ ] Generics as standard, not .NET's.
  - [ ] A function type of Z#'s own: a function as a value is a .NET delegate today, so
    Functions as values speaks of delegates.
  - [ ] The project compiler as a package, `pkg:`, not `std:project` (with the other
    build-time libraries).

## Done

- [x] The prelude is imported into every script `zs` runs, unless `--no-prelude` (#37).
- [x] A local function captures the one beside it that it calls (#36).
- [x] Captures by reassigning the variable the code refers to, with no marks (#35).
- [x] A variable apart from a definition (#34).
- [x] A local function reaches its class's members by their names (#33).
- [x] A CLR method's code reaches its class's members by their names (#32).
- [x] Explicit this, `Ref` and `Out` in `pkg:clr/hir` (#31).
