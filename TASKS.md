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
- [ ] **`std:numbers/integer`.** `FixedBitWidthInteger`, a meta-type whose objects are
  types, of a bit width and a signedness; `I8`, `I16`, `I32`, `I64`, `U8`, `U16`, `U32`,
  `U64`; arithmetic and comparison operators declared for each, with no code.
- [ ] **`std:numbers/real`.** The same for floating point: `F16`, `F32`, `F64`.
- [ ] **The CLR's mapping.** `std:zsharp/clr` gives the runtime what the types and
  operators are on the CLR: a capability the types lower through, and the operators
  premapped to the CLR's.
- [ ] **Literals into Z#'s types.** An integer or decimal literal goes to any of the types
  that holds its value.
- [ ] **`std:boolean`.** `Bool`, its operators, and conditions taking it.
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
