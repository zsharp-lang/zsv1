# Type Definition

A type definition in Z# is described by a `TypeDefinition` AST node
which is parsed from a `class` declaration.
This is where Z# takes from both Python and C#.

Class declaration can have an `of` clause, which specifies the meta-
type to use. The meta-type is the type of the reflection object.
That is, when we do `class Person of ORMModel`, we actually ask the
compiler to produce code that constructs a new `ORMModel` object and
fill it in with the body.

There's no root `MetaType` object. Instead, all meta-types must simply
be marked as a `Definition`. That is, meta-types are ORS types.
Which makes sense, since the whole reason to have meta-types is so we
can reflect in the domain of the meta-type.

Since meta-types have different structures, and there's no shared 
structure whatsoever, we can't know how to translate from the AST node
to ORS code graph. That's why we need to have a mapping from meta-type
to a meta-compiler. The meta-compiler is also an ORS entity that a
meta-type author should supply.

The reason meta-type and meta-compilers are separate is because 
meta-types should be used for reflection and programmatic code-generation,
while meta-compilers are used solely to go from AST -> ORS.

So when we see a type definition we need to look at the meta-type, then
we need to lookup a meta-compiler, then construct a `Call` node and inject
the AST node, then evaluate it. This should result with an ORS code object
that we can emit. The code object constructs an instance of the meta-type
and populates it. The code object *must not* contain references to the 
AST node, since that nodes will be gone by the time the code actually runs.

This is oddly similar to `reification.md`.
