# Open questions and decisions

## Open questions

- Exceptions in LIR. The CLR, Python and JS have them, and x86 does not.
- How a reference is tested for null, and what `If` means with no boolean type
  in LIR.
- How an extension is packaged, and what a loadable library consists of.
- Whether the CLI may use anything that Z# cannot reach. The working rule is no.
- Whether a pluggable tokenizer is needed for new token forms.
- Nullable types such as `Foo?`. They are meant to be pure operator overloading, which
  is a future milestone.
- Which `Package.DotNet` entities are needed to fully describe the CTS, including how
  a value type becomes an object.
- **The IL loader describes the CLR from the running process, not the target.** A CLR
  platform built for another .NET version resolves the shared CLR handles against that
  version's reference assemblies, but the IL loader chose overloads and members against
  the running .NET. So code built for net8.0 can pick an overload net8.0 doesn't have,
  which fails only when its handle is mapped. The fix is for the IL loader to describe
  the target's reference assemblies, without loading them, as `MetadataLoadContext`
  reads them.
- **What still needs a design of its own.** The IL importer imports the CLR's entities
  and also compiles the CLR's definitions written in Z#, `DotNet_Class` and its kin, with
  local functions and their closures: the second could be a unit of its own, on top of
  it. A built class's metaclass is found by the name the platform gives it, the class's
  and `.Class`, rather than by what binds it, so a built Z# class's type object carries
  no id that `reflect` reads. And the bootstrap's list of libraries and what each needs
  says again what each library's imports say.
- **A signature the seed binds can't change in place.** The built standard library is
  compiled code: it names the toolchain's types and members by their signatures, and
  implements its interfaces. A member can be added and the old one removed a bootstrap
  later, but a type can't be renamed, and an interface member that the seed's classes
  implement can't change its type, since both shapes would have to exist at once. So
  `LoaderMap` keeps its name for now, and `Annotated.Annotations` stays a list of objects
  until there is a way: a seed that binds by name, or a step that builds the standard
  library with the toolchain it was built for.
- **A generation writes over the seed.** Each generation of the bootstrap writes its
  builds where the seed is. So a generation that compiles wrongly, as one did when a
  hook the seed installs was no longer called, replaces a good seed with a broken one,
  and the next generation fails with nothing to go back to. The seed was recovered from
  the release's builds and rebuilt forward through each change's transitions. A
  bootstrap could write each generation apart and replace the seed only once a
  generation builds itself the same.
- **A module's global whose type's code uses the global.** A global is declared once its
  type is lowered, and lowering a class lowers its code, so a global of type
  `List[ClassObject]` that `ClassObject`'s code uses isn't declared when that code asks
  for it. `std:oop` types such a global by an interface instead. A global could be
  declared before its type is complete, as a class's structure is.

## Decided in the review of 2026-11-12

Decided in the review of the reimplementation, for the language features to come.

- **`let` and `var`.** A `let` local can't be set again after its declaration, and a
  `var` local can. This is the local's property, not its type's; any other form of
  immutability, such as a `const`, is the type's or a decorator's. The seed takes it in
  three bootstraps: `var` is added, every `let` that is set again becomes a `var`, and
  then a `let`'s second `Set` fails.
- **The prelude.** The CLI inserts `import "std:prelude";` into the single file it runs
  as a script, as an AST node, unless it's given `--no-prelude`. Nothing else inserts it:
  not the project compiler, and not a document a script imports.
- **`|` with `Error`.** Errors live together as a value, but values and errors don't:
  - `Error | Error` is `Error`;
  - an `Error` with a subclass of `Error` is the ordinary union of the two;
  - an `Error` with any other `T` is `IResult[T, Error]`.

  `T` and `Error` both cast implicitly to `IResult[T, Error]`, and an error value always
  converts as the error, even where `T` would take it too, as `Object` does.
- **Indexing a property.** `map.height[2, 6]` errors if both an indexed `height` and the
  value of a parameterless `height` take the index. This holds for now, and may change.
- **Namespaces on import aren't decided.** They won't be modules, and for now an import
  doesn't reach a namespace. Today an assembly's types are found by name alone, whatever
  their namespace, so two types of one name in one assembly have to be an error rather
  than one of them.
- **Integers.** The integer concept is a set of operator functions and casts. A
  fixed-width integer is a type of its own `Integer` meta-type, which gives its width.

### Captures

- Closures capture variables, not values: a write in either function is seen by the
  other.
- A capture is the capturing function's relation to the code around it. HIR represents
  it, if at all, on the function's node; neither `Local` nor `Parameter` knows it. For
  implicit captures, as Z#'s, the structure says it already: the inner code gets the outer
  local.
- The code loader realizes a variable as a variable. Whether it is a cell is the
  platform's: lowering the outer function, the CLR asks the functions nested in its code
  what they capture, and lowers those variables as cells. Loading comes before lowering,
  so they all exist by then.

### Patterns

- A pattern is a node kind of its own, not an expression. It tests a value and binds
  parts of it; binding is setting, so what is bound can be any settable CO.
- A name pattern always says `let` or `var`, as in `{ .x: 0, .y: let y }`, so where a
  name comes from, and whether it can be set again, is written. `let circle: Circle` is a
  name and a type test at once. A bare expression in a pattern is a value pattern,
  matched with an operator: in `case`, its `of` operator, `~=` by default.
- `case`'s `when` takes patterns.
- What a failed match reached stays bound. Where a binding can be read is decided by its
  type, as a type with a fallback value, as a nullable has, can be read anywhere; where
  scoping is vague, as in a condition, reading a binding the match didn't reach is a
  runtime error until there is flow analysis.
- A literal is a pattern that can be evaluated: `[]` is both, and a sequence pattern is
  closed, `[a, ..rest]` opening it. Inside brackets a prefix `..rest` is the rest, and an
  infix `0..20` a range.
- In `{}`, a key is an expression, as `{ x: 0 }` is keyed by `x`'s value, and `.x` is the
  member `x` of the matched value, as an implicit member is of its type. As a literal,
  `{ .x: 1 }` is an object with a member `x`; `{ x: 1 }` a mapping. A mapping pattern is
  closed, opened with `..`; a member pattern is open. Members and keys together are a
  pattern, not a literal. `{}` is the empty mapping, as position and the matched value's
  type and capabilities decide.
- An error for `{ x: ... }` against a value with no mapping but a member `x` suggests `.x`.
- `not` in a pattern is a pattern combinator, apart from the boolean `not`; a pattern under
  it binds nothing.
- Deconstructing a value for a positional pattern is an operation of the matching REI,
  which whatever dispatchers are applied answer.
- Narrowing through `and`, as `nullable and nullable.do()`, is flow typing, a future
  feature.
- So far there are value patterns and name patterns; sequences, `{}`, `not` and `case`
  come next. A name pattern needs its type until a local can take the type of what
  first sets it.

### Accessibility of the CLR's definitions

- CLR HIR represents the CLR, so it has the CLR's accessibility: public, private,
  internal, protected, protected internal and private protected, as a property of each
  node that has one. Decorators of CLR HIR's own module, `std:zsharp/package/dotnet`,
  set it: `@public`, `@private`, `@internal` and `@protected`. Each is one generic
  function over the `Accessible` capability, which gives back what it was given, typed
  as it was. They are HIR's alone and need nothing that runs, so they aren't
  `core:runtime`'s; nor are `@static`, `@virtual`, `@abstract` and `@override`, which
  are there too. A top-level type
  has public or internal alone on the CLR, so a private one is internal.
- A meta-compiler gives each node its default, as it interprets what is written:
  - a class and its contents are private;
  - an interface and its contents are public;
  - a module's functions are public, and its globals private, so a public global, an
    anti-pattern, is always written.
- A non-public method that implements an interface's implements it explicitly: the
  platform records which interface method it implements, as the CLR's method
  implementations do. `@public` isn't required for it.
- **Not written yet for modules:** a module lowers to LIR's own module, whose functions and
  globals have no accessibility, so a module's members are written public whatever they say.
- **Pending:** accessibility is checked as the code runs, by the CLR, and not as it is
  compiled. A CO can't refuse a member to code outside its class until member access
  knows where the code asking for it is, which needs the requester's context in the
  member-access REIs.
- A named constructor is what the CLR class meta-compiler interprets in the CLR's terms:
  a private constructor taking a private marker type of its own, named `<name>`, and a
  public static method of the name that calls it, both marked `[NamedConstructor(name)]`.

## Choices and restrictions from 2026-11-09

Made without you on 2026-11-09. The choices are design for you to keep or change; the
restrictions are what isn't supported yet, with what it would take.

Choices:

- **Interface syntax.** `def Shape of Interface { ... }`, with `Interface` imported from
  `std:oop`, as any meta-type is given with `of`. Alternatives: an `interface` keyword, or
  a decorator.
- **What implements an interface.** A class's bases are the interfaces it implements, as
  `def Square : Shape`, since a Z# class has no inheritance. Alternative: a separate
  clause, such as `implements`.
- **Matching an implementation.** A class's method implements an interface's if it has
  the name, the number of parameters, and types that each go to the other, the return
  type included, and is a class method if the interface's is. Alternatives: matching by
  assignability in one direction (variance), or by explicit implementation.
- **Casts and type tests belong to Polymorphism.** `cast` and `test` were added to the
  capability, since knowing what an object is comes with objects reaching their class's
  methods. Alternative: a separate capability, such as runtime types.
- **Decorator names.** `@static` and `@class`, exported by `std:oop`, so a document that
  uses the CLR's `static` too imports one under another name. Alternatives: other names,
  or one decorator that overloading resolves for each kind of definition.

Restrictions:

- **An interface's methods have no code, and an interface has no fields.** Default
  methods would lower as methods of the interface the runtime realizes; fields would be
  properties, which `std:oop` doesn't have yet.
- **An interface doesn't extend interfaces.** The runtime's `Polymorphic.Interfaces`
  already allows it; `std:oop` would need an interface's bases and assignability through
  them.
- **A value of an interface doesn't give its class.** `shape.Class` would take each
  object reaching its type object, which a polymorphism operation, as `cast` and `test`
  are, could give.
- **Whether a match against a trait is dynamic depends on the implementations that
  exist when the code is compiled.** `Traits.isTrait` is "a type something implements
  now", so `std:oop`, which matches its target against `Polymorphism`, has to be imported
  after `std:zsharp/clr` gives the runtime polymorphism. A trait would need to be one by
  what it is, not by what implements it so far.
- **Only one target has polymorphism.** Lowering for each target is kept by target, but
  nothing yet lowers a class for a target without polymorphism.
