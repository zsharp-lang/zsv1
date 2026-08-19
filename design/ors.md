# ORS - Object Representation System

The ORS system defines 5 types:
* `Definition` / `Identity`
* `Reference`
* `Type`
* `Expression` / `Evaluation Flow`
* `Statement` / `Execution Flow`

> NOTE: the entries that have multiple options are there because I've yet to decide
> which of the names are better. As we see more and more of the system, we might be
> able to figure out the better names to choose.

Each of these 5 types is implemented as an *empty* interface. Essentially, they all
represent marker/slot types.

The ORS itself *does not* define structures. It only gives us the tools to define
structures in a composable, type-agnostic way.


The ORS system is designed to allow user-defined types to be composed without them
having to know about each other. This means users/libraries can (and should!) 
define custom ORS types for their own domain. The extensibility of the other and
ORS related systems will make sure that every ORS graph can be processed, regardless
of the actual composition and the concrete type of each node.
