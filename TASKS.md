# Tasks

What is to be added, in the order it is being done. `design/` records what is decided;
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
  `design/imports.md`, `design/code.md` and `design/definitions.md`.
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
  `sourceMap(holder, statement, map)`. The parser records each AST node's `Location`; the
  AST compiler keeps its statements' maps in `ASTCompiler.Maps` (shared by forks), and a
  definition's fill takes the definition's; `Bodies` reifies a body as a `Code` through
  the reifier's new `ReifyCode`, whose building code makes the `Code` first and puts each
  statement in its table as constants, so built code needs nothing live;
  `CodeLoaders.Load(code, into)`, which the IL loader, `std:functions` and `std:oop` lower
  bodies with, puts each lowered statement in its function's table; the Cecil platform
  writes sequence points, source checksums and named locals into an embedded portable PDB.
  A stack trace through a Cecil-built assembly shows the `.zs` file and line. Statements
  only, for now. Folded into `design/definitions.md` (**Source mapping**) and
  `design/cecil.md`.
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
- [ ] **Nine import tests of `ZSharp.Tests.Classes` fail**, and have since the import
  redesign: the project didn't compile (`CodeLoaders.Register` gained `evaluate`), so
  they never ran. Some may test what the redesign changed on purpose (a module's or
  body's import is checked when loaded, not while compiling), others are real failures
  (`NullReferenceException`, "None of the 4 overloads applies"). The same nine fail on
  `lang/overload-groups`.
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
- [ ] **`pkg:fastlsp`**: decorators that collect endpoints as the build runs, the
  dispatcher as platform-agnostic HIR, build script utilities.
- [ ] **`pkg:zsharp/lsp`**: the small server, then a sanitized runtime for the rest.

## Done

- [x] The prelude is imported into every script `zs` runs, unless `--no-prelude` (#37).
- [x] A local function captures the one beside it that it calls (#36).
- [x] Captures by reassigning the variable the code refers to, with no marks (#35).
- [x] A variable apart from a definition (#34).
- [x] A local function reaches its class's members by their names (#33).
- [x] A CLR method's code reaches its class's members by their names (#32).
- [x] Explicit this, `Ref` and `Out` in `pkg:clr/hir` (#31).
