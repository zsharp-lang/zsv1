// std:zsharp/capabilities — what a platform can do, which a CO lowers to.
//
// A capability is an interface, and a CO asks for one by matching the target it lowers
// for against it. A capability doesn't make LIR: it marks the LIR the CO builds with
// metadata of the platform's own, so what it adds is opaque and the CO still builds the
// definition. A platform that exists, as the CLR runtime, is given a capability by a
// trait.

import { Int32, Boolean } from "core:runtime";
import { Structure, Function, Type } from "pkg:zsharp/lir";
import { Interface as DotNet_Interface } from "pkg:clr/hir";

// Polymorphism: a structure's values are objects, which calls on them reach through
// what each object is.
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

    // The function, which has no code, casts an object down to the class or interface,
    // failing if the object isn't of it, since an object knows what it is.
    fun cast(function: Function, to: Structure);

    // The function, which has no code, tells whether an object is of the class or
    // interface.
    fun test(function: Function, target: Structure);
}

// What a platform's classes can implement of the platform's own: a type of it, as an
// interface of the CLR's is, which a class whose bases are such types needs. A target
// without it has classes that implement only Z# interfaces.
def PlatformInterfaces of DotNet_Interface {
    // The class implements the platform's type.
    fun implements(structure: Structure, implemented: Type);
}

// Numbers: what the platform's numbers are, which std:numbers' types lower to.
def Numbers of DotNet_Interface {
    // The type of integers of a bit width, signed or not.
    fun integer(bitWidth: Int32, signed: Boolean): Type;

    // The type of floating-point numbers of a bit width.
    fun real(bitWidth: Int32): Type;
}

// Truths: what the platform's truth values are, which std:boolean's Bool lowers to.
def Truths of DotNet_Interface {
    // The type of truth values.
    fun boolean(): Type;
}
