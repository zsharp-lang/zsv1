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
- [ ] **A platform binds a declared function per target.** `premap` binds it for every
  target; `std:zsharp/clr`'s premaps hold on both CLR platforms, but a platform that isn't
  the CLR would need its own binding.
- [ ] **Importing a library holds its module.** `import "lib";` and `importers.Import`
  give the module's node without loading it, so `std:modules` doesn't hold it, and its
  operators aren't the language's until something loads it, as importing a name from it
  does. `std:zsharp/clr` loads the libraries it implements.
- [?] **`x as A as B`** parses as `x as (A as B)`. Waits on whether `as` is to chain left.
- [ ] **Literals into Z#'s types.** An integer or decimal literal goes to any of the types
  that holds its value.
- [x] **`std:boolean`** (#40). `Bool`, of the `Truth` meta-type, with `==` and `!=`,
  which the CLR runs as its `Boolean`; casts to and from it.
  - [x] A Z# type the platform realizes as one of its own says so (`IRepresented`), so
    it boxes where an `Object` is expected and isn't compared as a reference (a bug of
    #39: `print(5 as I32)` crashed).
  - [?] Conditions taking `Bool`, and comparisons giving it: waits on how Z#'s types and
    the platform's convert, as a condition takes the CLR's `Boolean`, and on what a
    condition is in LIR, which has no boolean (`decisions.md`).
  - [ ] `not`, `and` and `or` on `Bool`: `not` and `and` aren't operators of the language
    yet, and `(or)` isn't an operator literal, as `or` is a word.
- [ ] **`Void`.** The type of no value, its own type.
- [?] **The literals' default types.** The prelude sets what a literal is where nothing
  asks for a type (`I32`, `F64`, `String`). Waits on how Z#'s types and the CLR's convert:
  if each goes to the other implicitly, an operator on one of each is ambiguous between
  the two types' overloads, which std's own code, written with the CLR's types, would hit.
- [?] **`std:string`.** `Text[X]`, of an `Encoding` (concrete) or a `CharSet` (any
  encoding of it), `Char[CharSet]`, casts between encodings, and `String` the alias the
  prelude sets. Waits on what a string's element is (a character of the set, with the
  encoding's units apart, is proposed).
- [?] **`std:collections`.** Conceptual list and dictionary types. Waits on generic Z#
  classes or interfaces, which don't exist yet.

## Language and toolchain

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
