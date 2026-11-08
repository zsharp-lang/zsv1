// std:zsharp/capabilities — what a platform can do, which a CO lowers to.
//
// A capability is an interface, and a CO asks for one by matching the target it lowers
// for against it. A capability doesn't make LIR: it marks the LIR the CO builds with
// metadata of the platform's own, so what it adds is opaque and the CO still builds the
// definition. A platform that exists, as the CLR runtime, is given a capability by a
// trait.

import { Structure, Function } from "std:zsharp/lir";
import { DotNet_Interface } from "core:runtime";
import { export } from "core:zsharp";

// Polymorphism: a structure's values are objects, which calls on them reach through
// what each object is.
@export
def Polymorphism of DotNet_Interface {
    // The structure's values are objects of a class, with its fields.
    fun objects(structure: Structure);

    // The structure's values are objects of any class that implements it.
    fun interfaces(structure: Structure);

    // The class implements the interface.
    fun implements(structure: Structure, implemented: Structure);

    // The function is a method of the class or interface: it takes the object first,
    // and calling it calls what the object's class has.
    fun method(function: Function, owner: Structure);
}
