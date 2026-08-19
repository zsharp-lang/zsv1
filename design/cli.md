# CLI

The Z# CLI is the actual entrypoint of the Z# language. It
uses all the things we've defined and integrates them
together to implement the Z# language.

It's responsible for setting up the parser and the systems,
parsing a file and running the script runner.

Since Z# is "simply" an interpreter, the CLI's entire job
is to parse the input file and pass the AST to the script
runner.


## Bootstrapping

Recall that the runtime defines a custom ORS. Additionally, the
runtime exposes primitives via singleton ORS object, pre-mapped
to the runtime primitives.

So, the CLI should expose all the ORS entities the runtime has
via the import system. It defines an instrinsic module imported
as `core:runtime` which exports all the ORS objects we talked
about.

Then, a script can use these ORS types to construct new CTS
entity definitions. The exact way these entities are loaded
into the runtime to create new types is described below.

Once we're able to define custom .NET types in Z#, we can
define new ORS and OIS entities, which means the whole standard
library, including the usually intrinsic entities like classes,
in Z# itself. That is, the "primitives" in Z# are .NET entities,
and they're too need to be imported.


## Literal

Throught the compilation process, we want to inject raw .NET
objects into an ORS graph. We do that through the `Literal`
ORS entity. This entity is an `Expression` (`Evaluation Flow`).

It should be defined alongside the AST compiler since that's
the only consumer who actually needs to inject live values
into ORS, as part of the compilation process.


## Script Runner

The script runner iterates over each statement in a document
and executes them one-by-one.

> NOTE: This doesn't contradict type definition compilation
> since script code doesn't support forward referencing.
> Forward referencing is only supported inside modules and
> type definitions.

Now, since Z# *is* statically type-safe, the runner should
first compile a statement then execute it.

To compile a statement we create an ORS object that 
represents the statement. Then we execute it.
For that we'll need an ORS interpreter.

The ORS interpreter should tree-walk interpret some types
of ORS entities, but go through the default execution
chain for the rest.

We'll first explain what the default execution chain is.
Recall that Z# defines its own ORS entities. Z# also
provides OIS importers for them, which basically apply
CG's API endpoints directly. That means we can go from
Z# ORS -> CO. Then, we can go from CO -> LIR via the
LIR API. Then, the runtime can directly load that LIR.

This is how Z# can execute directly on top of .NET.

Now, for some types of ORS nodes we don't want that
because going this way requires full compilation.
However, script code should support conditional code
execution without compiling both branches. That's why
the ORS interpreter needs to special case some nodes.
