# Imports and reflection

## Imports

- An import means what the meta-compiler it is in makes it mean, like `let`: where it
  runs and what holds what it imports. A meta-type could record its imports and run
  them in a static constructor. With no meta-compiler that compiles it, an import is
  an error. `Language.BindImported` binds the imported names to whatever holds the
  value, for meta-compilers that want it.
- The current meta-compilers run an import as the definition it is in is
  constructed. A body runs it in the code that builds the body: a local of that code
  holds what it imported, declared in the building block, and nothing of the import
  is left in the body. A name used in the body is reified like any value of the
  building code, so the body gets the imported value encoded. A module declares the
  local in its own construction code. The document runs the import as its statement
  runs.
- So `fun bar(src: String) { return fun() { import { a } from src; print(a); } as
  Function; }` works: the inner function is constructed when `bar` runs, so `src` is
  a value there. The same import directly in `bar`'s body is an error, since while
  `bar` is constructed `src` is its parameter, not a string. Each construction runs
  the import again.
- An import whose source is compile-time, as `Compile-time evaluation` defines it, is
  run while compiling, and its names are bound to what it gave. They are then known
  everywhere: to a meta-type, a decorator, another import's source, and the types of a
  definition. Its arguments are compile-time too, which makes them real parameters of
  the import: fine-grained imports through arguments. Running the source at compile
  time moves its side effects there, as with `of`.
- An import whose source refers to a variable, as in `import { a } from src` where
  `src` is a parameter, can only run as the definition it is in is constructed.
- A document runs as it is compiled, so its imports always run while compiling, and
  their names are bound to what they gave. At
  compile time its names are only an `Instance`, so only code that is built, such as a
  body, can use them. Using one in a type needs a lookup on HIR structure while
  constructing, which isn't there yet.
- An import is a call of the function `import` stands for in `Language.Functions`,
  with the source, then the import's own arguments:
  `import(args..., kwargs...) { names } from source` calls `import(source, args...,
  kwargs...)`. Whatever runs the import runs that call, so a body puts it in its
  building code and the document evaluates it as its statement. `core:zsharp`'s
  `import(String)` is the first overload, and adding overloads lets an import take
  sources other than a string, or arguments. Keyword arguments are named arguments of
  that call. An import has no alias of its own: only the names it brings in can be
  renamed, as in `{ a as b }`.
- Importers route by prefix through the string importer. The prefixes say what a
  library is: `core:` is live and stateful, bound to the toolchain that is running, as
  `core:compiler`'s CG and loaders, `core:zsharp`'s compiler and meta-types, and
  `core:runtime`, which also keeps the CLR's types for now; `pkg:` is an external
  package installed by the package manager; `std:` is the standard library, its common
  APIs; and `std:zsharp/` is the standard API of the toolchain, its types, as
  `std:zsharp/hir`, `std:zsharp/lang/ast`, `std:zsharp/lang/tokenizer` and
  `std:zsharp/lang/parser`. The CLR
  package's HIR, `std:zsharp/package/dotnet`, isn't standard: it is there until a package
  manager can give it as a `pkg:`.
- A `std:` library is a CLR module, or else a library of the standard library written
  in Z#, found from where the toolchain runs: `std:name` is its build,
  `std/name/build/name.dll`, an assembly named `std.name` (`std.zsharp.lowering` for
  `std:zsharp/lowering`), or else a document, `std/name.zs` or `std/name/name.zs`. A
  process keeps what `std:name` gave it the first time, so a library built while it runs
  is what the next process loads. One built library's assembly finds another's by its
  name.
- A source with no prefix is a file, relative to the document importing it, and what
  imports it is what is registered for its extension in `core:zsharp`'s `files`: a
  `.zs` file is a document. A `.dll` is an assembly, which the host imports, since what
  the toolchain is built of has to be loaded before anything in Z# can be: it is loaded
  into the running platform, unless it is already, and importing it gives its module,
  whose members are its public types by their names, whatever their namespaces, once it
  is entered. So `import { AssemblyDefinition } from "Mono.Cecil.dll"` reaches
  Mono.Cecil. A name with a type that isn't generic is that type, and indexing it with
  type arguments gives the generic type of that name and arity, whatever its namespace,
  as C# reads `IList` and `IList<T>`; a name of generic types only is its arities, as
  overloads.
- An assembly the running platform has is a source too, as in `import { BindingFlags }
  from Object.Assembly`: `core:runtime`'s `import(Assembly)` gives what importing its
  file gives, through the same importer, so nothing finds the file.
- Nothing is imported for a script. The prelude, `std:prelude`, imports what a script
  wants, so a script imports it first, `import "std:prelude";`, and gets the standard
  definitions, as Z# functions, through an ordinary import.
- Importers do nothing CO-related. They return real HIR, and imported names
  cross into MIR through the HIR -> MIR REI.
- Libraries are supplied by a `pkg:` importer written purely in Z#, the package manager,
  as **Packages** describes.
- An import that runs while compiling reaches each of its names' members as it runs,
  since that is what importing them means: whatever comes with a member comes then, as
  a class's extension methods do, and a name the imported value has no member for is an
  error of the import, not of its first use. The names are still bound to the member
  as code, `value.name`, so what they stand for is reached again where they are used. An
  import that runs only as a definition is constructed has nothing to reach yet, so its
  names are checked when they are used.

## Reflection

What describes a type is asked as the code runs, wherever it runs, so it isn't part of
what a definition is, and a definition doesn't encode its HIR node.

- **A description is a function call.** A CO's `Cast` to what describes it decides what
  that is. The IL loader's casts of CLR types and of classes defined in Z# call its static
  `DescribeType(Type)`, given the type's type object, which the loader of the process that
  runs answers. A Z# class's cast in `std:oop` gives its node as a known value instead.
- **One type, one description.** The loader describes a type the runtime made of a class
  defined in Z# as that definition, so `reflect` and a cast give the same object however
  the type is reached.
- **`std:reflection`** declares `reflect(type: Type): Instance`, with no code. The prelude
  premaps it onto the loader's `DescribeType`, so the loader knows nothing of
  `std:reflection`, and `std:reflection` nothing of the loader. A platform whose code
  can't reflect leaves it unmapped.
- **Code that runs elsewhere describes types there.** An assembly the Cecil platform
  writes calls `DescribeType` of the loader that runs it, which it refers to as any
  other assembly. For a target without reflection, code that rebuilds a description,
  reified or serialized at build time, is the fallback.

### Reflection and importing

Designed on 2026-11-10, and built, as **How it is built** says, but for what it lists
as not yet. What follows replaces the IL loader as the one source of descriptions.

Reflection and importing are two things. Reflection is getting the HIR node of a
definition, which code generation also uses: a program can reflect and build HIR with
only the HIR libraries, without the language's core. Importing is getting HIR for
something imported, which needn't contain any: a native DLL or a foreign language's
module gives HIR too. Where an importer's HIR comes from is its own business.

- **Reflection is stored metadata.** An encoder, run by the build, stores what describes
  the module somewhere, and `reflect` is matched to how it was stored, through mapping.
  It is the composition of two halves:
  - the platform's: a runtime object that carries metadata gives its metadata, and the
    unit it was compiled into, as `Type.Module` is on the CLR;
  - the encoder's: that metadata, in that unit, gives the HIR node.
- **Any object that carries metadata** can be reflected, not only a type object, so a
  type object carries it too. A Z# class's type object carries it through its runtime
  class, its metaclass, which each class has of its own. `ClassOf` stays the canonical
  node of a metaclass, as `Type[X]` is an expression, which makes a node each time.
- **Encoding happens before lowering**, and can add to the HIR: it gives each node it
  encodes its id, as metadata. A node with no-reflection metadata is skipped, and a node
  it encodes that refers to a skipped one is written with an external reference, or
  fails.
- **The metadata is an id**, unique in its unit. The unit carries what was encoded, as
  the platform gives it a place: on the CLR, by convention, a blob, `[ReflectionBlob]`,
  or a file beside it, `[ReflectionSourceFile("name.hir")]`. A native target can reuse
  either, as they are only what a decoder reads. A build that puts several modules into
  one unit encodes them together.
- **Annotations are the metadata.** An annotation's type is annotated with what keeps
  it: *encoded*, which the encoder honours, and *kept at runtime*, which lowering
  honours, carrying it onto the LIR of what the definition lowers to, which the platform
  writes its own way, as custom attributes on the CLR. Others are dropped, as the
  framework's `EntryPoint` is. A kept annotation's state is constants a platform can
  write.
- **Separate platforms, separate reflection.** Each module can be built by a platform
  instance of its own, mapped onto its own encoder, so one program can hold modules
  reflected different ways. A type with no metadata, as `String`, is described as the
  IL loader describes it.
- **Encoders.** A (de)serializer is registered at compile time, for each node type.
  The structure of a type is inferred by reflection, and a type can supply its own
  (de)serializer instead. Generated ones are what tests Z#'s code generation. The format
  is HIR's, not a platform's: a graph, with ids for what is shared, and an external
  reference, resolved by importing it, for each node another module owns. Bodies are an
  encoder's choice.
- **One node per definition per process.** The importer and `reflect` decode a unit
  once, and give the same nodes.
- **Importing doesn't load.** An importer gives HIR, and only that. A .NET DLL imported
  in a module is read as metadata, and is loaded into the runtime when the importing
  module is, as the runtime platform maps a handle into it. An import in a document is
  loaded right away, and so is one that is evaluated as code compiles. A module's entry
  point runs when the module is loaded into a runtime.
- **The compile-time environment.** An import a module's build needs, as one whose entry
  point installs a meta-type, is `@ct import`, or is at the document level, as in its
  module file. `ct` has an overload for an import, which runs it as code compiles.
- **An imported definition is bound to what it was built as.** The CLR importer walks
  the unit's entities, reflects each, and so has each node with what it was compiled to,
  which it premaps the node onto, so lowering it gives that and not a new definition.
  Every definition kind can be premapped, as a declared function can.
- **A kept annotation is one generic CLR attribute**, `[Metadata(type, names, values)]`:
  its type, and each of its fields by name with its value. Made for each type, an
  attribute class would be what C# reads best, which nothing needs yet.

#### How it is built

- **Annotations.** `Annotated` is on Z# functions, globals, modules, classes, methods and
  constructors, and on CLR classes, interfaces, structures, methods and constructors. The capabilities
  `Kept` and `Encoded` of an annotation's type say what keeps it. `keep(node,
  metadata)`, of `std:zsharp/lowering`, is how a CO carries a node's kept annotations
  onto the LIR it makes, as LIR `KeptAnnotation`; the IL loader does it for a CLR class
  and its methods and constructors.
  The Cecil platform writes each as `[Metadata]` on the method, field or type it writes.
  The runtime platform doesn't yet, as what it makes is reflected in the process anyway.
- **`std:encoding`** is the encoder and the decoder, in Z#. `encode(module)` gives each
  annotated node it writes an id, kept as a `ReflectionId`, and gives the bytes. What it
  writes is inferred from fields, a graph with each shared object once; code, a
  `Code` or a list of locals, isn't written, and an annotation only if it is encoded.
  A node another unit gave is written as that unit's, by its id; one that describes a
  CLR entity, as that entity, by its names; a CLR definition of the module, as the
  entity built of it, by its id. A registry of custom and generated (de)serializers is
  not yet.
- **The CLR convention.** The build gives the bytes to the platform:
  `target.embed("ZSharp.Reflection", bytes, "std:encoding")`, an embedded resource and
  `[ReflectionBlob(resource, decoder)]`, which names the library that decodes it.
- **Importing.** What imports an assembly asks each describer in `core:zsharp`'s
  `describers` for it, and the first that knows it gives what importing it gives, before
  the assembly is entered; one none knows is the IL loader's view of its module. An
  assembly whose blob names a decoder has the decoder imported first, so it is
  described whatever was imported before it. A describer that fails leaves the assembly
  to the IL loader, with a warning, so a seed built wrong still loads. `std:encoding`'s
  describer decodes a unit once, entering the assembly of each kind of node it makes,
  as what a node means is its library's, and binds each node to the entity that carries
  its id.
- **Binding.** `core:compiler`'s `bindings` maps a kind of node to what binds it, which a
  library declares with `@binder`: a function is premapped onto its method, a global is
  the static member it was built as, and a Z# class, its methods and its constructors are
  the type and methods they were built as, which their COs are instead of lowering them
  again. A node whose kind has no binder yet waits for one, as a library's nodes can be
  decoded before the library that binds them is imported. A built class's metaclass is
  found from it, as the type of its name and `.Class`, and its type object as the static
  field of that type.
- **Reflection.** `reflect(value)`, of `std:reflection`, is premapped by the prelude onto
  `describe`, of `std:encoding`: what carries metadata gives its id and its unit, which
  gives the node, and what carries none is described by the IL loader. A CLR method cast
  to what describes it is a call of the loader's `DescribeMethod`, as a type's is of
  `DescribeType`.
- **Names with a dot.** The Cecil platform writes a type of a dotted name, as
  `Shape.Class`, in the namespace before its last dot, so its full name is its name
  however it is found, as a module Reflection.Emit builds finds a type by its namespace
  and name.
- **Not yet:** importing that reads metadata without loading; the runtime platform
  writing kept annotations; a Z# interface built and imported; a description of a Z#
  definition in code a platform writes, which is a known value of the process that
  compiles it; and generated (de)serializers.
