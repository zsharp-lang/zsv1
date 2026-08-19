# LIR - Low-Level Intermediate Representation

The LIR layer is a representation layer that tries to be platform agnostic, while still
capturing important semantics.

The root of every IR graph is `Module`. A module represents a self-contained, closed unit 
of program.

It has a name (optional), a list of functions, an optional entry point, a list of imports 
and exports, type definition and globals.

Functions have a name (optional), parameters, return type, locals, body, and generic 
parameters (optional).

The function body is represented as a list of VM instructions.


For an actual implementation that's a bit different, see `D:/zsharp/zsv1/ZSharp.IR`.
> NOTE: The implementation is different since it actually represents .NET specific
> module definitions. This was supposed to fixed but I had more important things to do.


One crucial thing is that every LIR definition should have a list of custom attributes.
These attributes can be any object that is of type `CustomAttribute`, which is an empty
class.
