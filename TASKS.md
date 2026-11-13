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

- [?] **Imports run as their definition is constructed, not while compiling.** They run
  while compiling since c034a2c, so that a type could use an imported name. Direction
  (proposed): compiling a module generates its building code (HIR, loaded, then run), as
  a function body is. A type stays code in it: `let x: String?` builds
  `Global("x", String?)`, so loading that code calls `(?)` on `String`. An import does two
  things: (1) its details, while the code that holds it is loaded: an HIR `Import` node
  (an HIR change) that the loader resolves, giving the import's names their types and
  mapping the operators it brings, for the code after it; (2) its references, when that
  code runs: a plain call of `import`, which gives the live objects. No record stays on
  the definition: a body is reified in the building code, so what it uses of an import
  (an operator's group, a name) is unquoted building code, resolved where the `Import`
  node is in scope. The operator table holds markers, not groups: `a + b` calls the marker
  for `+`, and the loader's scope maps each marker to an overload
  group, an outer one plus what its imports give. Module code is ordered: a
  definition comes after the imports it uses. A group with the same entity twice (the
  prelude's `String` and std:string's) is the overload object's concern. Compile-time slots
  (`of`) use the context around them: a project's module file is that context for its
  other files, which already works (a meta-type imported in `src/.zs` reaches
  `def B of Truth()` in another file), so std's 23 `def … of` files move their meta-type
  imports to their module files. A source known only when the code runs has no `Import`
  node, so its names have no types where the code is loaded.
- [ ] Named constructors on generic CLR classes.
- [ ] `import * as X` and `@importer` (need design).
- [ ] Function values: partial application, Bound/Invocation.
- [ ] The remaining patterns: `not`, sequence, `{}`, `case`.
- [ ] Compile-time access checks (need design).
- [ ] Deferred: resolvers, the IL importer split, metaclass binding, namespace imports.

## Done

- [x] The prelude is imported into every script `zs` runs, unless `--no-prelude` (#37).
- [x] A local function captures the one beside it that it calls (#36).
- [x] Captures by reassigning the variable the code refers to, with no marks (#35).
- [x] A variable apart from a definition (#34).
- [x] A local function reaches its class's members by their names (#33).
- [x] A CLR method's code reaches its class's members by their names (#32).
- [x] Explicit this, `Ref` and `Out` in `pkg:clr/hir` (#31).
