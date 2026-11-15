# Layering

Decided on 2026-11-16. How Z#'s parts relate: which languages there are, what goes
between them, what each part may rely on, and where a platform comes in. The other pages
record decisions inside one part; this one records the rules every part keeps.

## Languages and transformations

- **A layer is a language.** AST, HIR, MIR, LIR and IL are languages, each describing a
  program at a level of its own. A language never refers to another. Never.
- **A transformation goes between two languages**, and only translates: AST -> HIR is the
  source compiler, IL -> HIR the IL loader, HIR -> MIR loading, MIR -> LIR lowering, and
  LIR -> IL the runtime and the Cecil platform.
- **The core is HIR -> MIR -> LIR.** Z#, as a core, defines only the transformations
  between its core languages. AST and IL don't exist to it. AST -> HIR is a frontend, with
  its own extension points, and LIR -> a platform is a backend, with its own.
- **The core languages are minimal.** HIR is a few interfaces: identity, definition,
  instance, type, expression, statement, annotated. MIR is `CompilerObject`. LIR is
  structure, module, global, function, code and the rest of its core.
- **A language is extended without knowing it.** HIR is extended by the CLR's HIR and by
  the standard definitions. MIR has layers of its own: CG's core is an extension, and
  `CG.Typed` is built on it and on TS, while CG itself knows nothing, TS included. LIR is
  extended by the CLR's dialect. That separation is what lets semantics be anything.

## HIR

- HIR refers only to HIR. A graph may refer directly to nodes outside it, as another
  module's.
- HIR core doesn't assume semantics, so a definition is represented in its own domain's
  terms with no lowering at all. Its loader gives it behavior, and its CO says how it
  translates to a simpler, common language, or to a dialect where its domain needs one.

## MIR

- A CO is opaque, and may hold anything.
- A CO -> CO interaction never relies on another layer, unless the CO is scoped to that
  layer.
- A CO -> LIR lowering may refer to any CO or LIR, unless the CO is scoped to another
  layer: a CO of the runtime's may use the runtime to decide how it behaves and what it
  lowers to.
- MIR is for extenders to define and enforce semantics, not for inspection. That is why it
  has a single type.
- The LIR REIs return a result, so a CO that finds an invalid structure as it lowers gives
  an error. What it doesn't catch, the platform does.

## LIR and dialects

- Core LIR refers only to core LIR, so every platform can realize it.
- A dialect's LIR refers to its own dialect or to core LIR. A capability's LIR is of the
  shape it is tied to, or core.
- Specializing LIR supports fewer platforms, so it is avoided. When a CO is known to
  compile only to one platform's dialect, it lowers to that dialect.
- A constant of an object only one process has is, in effect, that process's dialect: only
  the runtime realizes it.

## Platforms and capabilities

- Z# doesn't define what a platform is, so a platform is passed as an `Object`. The CLI
  hard-codes the runtime; a build script uses another, or none.
- A CO lowers to core LIR, or uses a platform's capabilities: matching the target against
  an interface, so it asks what the platform can do, not what it is.
- A capability never speaks the platform's language. At most it speaks a dialect of LIR,
  and is tied to that dialect. A pure capability speaks only core LIR.
- A capability is given to a platform that exists by a trait, an implementation of the
  capability for the platform's type, apart from both.

### How it is built

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

## Opt-in

- Z# never checks for restrictions. Everything is opt-in, and an undefined case is an
  error, or a fallback. This holds in the AST compiler, the HIR loader, the REIs, the
  platforms and everywhere else, and it is what makes each part extensible: a new case is
  allowed by defining how to process it.
- Every part assumes as little as possible, and still lets custom information be attached:
  as annotations, in side tables of the process, or however an extender likes.

## Preservability

- A meta-type is *preserved* under a platform and an importer if importing what the
  platform made of any of its objects gives back an equal structure.
- It is a concept, not something enforced: there is no preserving platform. It is a
  property an implementer of an importer or a platform keeps in mind. How is out of scope;
  Z#'s reflection, the encoded description a build embeds, is the suggested way.

## Frontends and the official chain

- The official chain starts at HIR and ends at LIR: a general mechanism for describing
  structure, in high-level terms and in low-level ones.
- The CLI hard-codes the Z# frontend and the runtime. Z# source is not privileged: a
  frontend of another language would make build scripts of that language.
- The Z# frontend exists to give syntax that uses and interacts with the pipeline as much
  as possible. The project compiler is a frontend of its own, a source to a node, and the
  import system lets anything be one: an assembly importer is a frontend, which is why
  import functions give HIR.
