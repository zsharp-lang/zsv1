// std:oop — Z#'s own classes, defined in Z#.
//
// A Z# class has no inheritance. It has fields, methods and named constructors, and it
// implements interfaces, which declare methods a value of any class that implements
// them has. A method reaches the class's members by name, and names the object `this` if
// its first parameter is `this`. A class or an interface lowers through the target's
// capabilities: a target with polymorphism gets it as an object type with methods.
//
// A member marked `@static` is the class's own, not its objects': a static field is one
// variable, and a static method is a function, with no object. A member marked `@class`
// is the class's type object's: a class is a value, the one object of its metaclass,
// whose fields are the class fields and whose methods are the class methods, which run on
// it. An interface can declare class methods too, which the class of each value of it
// has, so the type of any class that implements `Shape`, its class shape, is
// `Type[Shape]`, as `Type[Square]` is the type of `Square`'s type object.
//
// This is their structure: the nodes a class, an interface and their members are, which
// reflection needs alone, and the decorators that mark them. std:oop/compiler is what
// the compiler needs of them: their COs and meta-compilers, which importing it installs.
//
// The nodes are the platform's own definitions, written with one meta-type, `Node`, so
// another platform reimplements them by giving that name its own.

import { String, Object, Boolean, List, DotNet_Class as Node, explicitThis } from "core:runtime";
import { importers } from "core:zsharp";
import { entrypoint } from "std:framework";
import { Instance, Definition, Variable, Type, Named as NamedDefinition, Annotated, Overloadable } from "std:zsharp/hir";
import { Code, Local, Expression } from "std:zsharp/hir/code";
import { Positional } from "std:functions";

// ---- The HIR

// The object a constructor makes, as its code refers to it: an instance, of the class it
// makes, and not a definition. (A method's object is its first parameter.)
def Receiver of Node : Instance {
    let Type: Type;
}

def Field of Node : Instance, Definition {
    let Name: String;
    let Type: Type;
    let DefaultValue: Expression;
    let IsStatic: Boolean;
    let IsClass: Boolean;

    @explicitThis new(this, name: String) { this.Name = name; }
    @explicitThis new(this, name: String, type: Type) { this.Name = name; this.Type = type; }
}

// Methods of one name overload, as functions do.
def Method of Node : Instance, Definition, Annotated, Overloadable {
    let Name: String;
    let Parameters: List[Definition];
    let Locals: List[Local];
    let ReturnType: Type;
    let Body: Code;
    // Its object: its first parameter, of what it is a method of, unless it is static,
    // whose code still reaches its class's members by name through it.
    let ThisParameter: Positional;
    let IsStatic: Boolean;
    let IsClass: Boolean;
    let annotations: List[Object];

    @explicitThis new(this, name: String) {
        this.Name = name;
        this.Parameters = List[Definition]();
        this.Locals = List[Local]();
        this.ThisParameter = Positional("this");
        this.Parameters.Add(this.ThisParameter);
        this.annotations = List[Object]();
    }

    @explicitThis get Annotations(this): List[Object] { return this.annotations; }
}

// A property: what its accessors read and write, which are methods of what holds it,
// named for what they are, `get Name` and `set Name`.
def Property of Node : Instance, Definition, NamedDefinition {
    let name: String;
    let Getter: Method;
    let Setter: Method;

    @explicitThis new(this, name: String) { this.name = name; }

    @explicitThis get Name(this): String { return this.name; }
}

// A constructor makes an object, runs its code on it, and gives it. The unnamed one is
// what calling the class runs; a named one is a member of the class.
def Constructor of Node : Instance, Definition, Annotated {
    let Name: String;
    let Parameters: List[Definition];
    let Locals: List[Local];
    let Body: Code;
    let This: Receiver;
    let annotations: List[Object];

    @explicitThis new(this, name: String) {
        this.Name = name;
        this.Parameters = List[Definition]();
        this.Locals = List[Local]();
        this.This = Receiver();
        this.annotations = List[Object]();
    }

    @explicitThis get Annotations(this): List[Object] { return this.annotations; }
}

def Class of Node : Instance, Definition, Type, NamedDefinition, Annotated {
    let name: String;
    let annotations: List[Object];
    let Fields: List[Field];
    let Methods: List[Method];
    let Constructors: List[Constructor];
    let Properties: List[Property];
    let Interfaces: List[Type];
    // Any other definition written in it.
    let Definitions: List[Definition];

    @explicitThis new(this, name: String) {
        this.name = name;
        this.Fields = List[Field]();
        this.Methods = List[Method]();
        this.Constructors = List[Constructor]();
        this.Properties = List[Property]();
        this.Interfaces = List[Type]();
        this.Definitions = List[Definition]();
        this.annotations = List[Object]();
    }

    @explicitThis get Name(this): String { return this.name; }

    @explicitThis get Annotations(this): List[Object] { return this.annotations; }

    // Its property of a name: what its accessors of the name read and write.
    @explicitThis fun defineProperty(this, name: String) { pairAccessors(this.Methods, this.Properties, name); }
}

// An interface: methods with no code, which a value of any class that implements it
// has.
def Interface of Node : Instance, Definition, Type, NamedDefinition {
    let name: String;
    let Methods: List[Method];
    let Properties: List[Property];
    // Any other definition written in it.
    let Definitions: List[Definition];

    @explicitThis new(this, name: String) {
        this.name = name;
        this.Methods = List[Method]();
        this.Properties = List[Property]();
        this.Definitions = List[Definition]();
    }

    @explicitThis get Name(this): String { return this.name; }

    // Its property of a name: what its accessors of the name read and write.
    @explicitThis fun defineProperty(this, name: String) { pairAccessors(this.Methods, this.Properties, name); }
}

// The type of a class's or an interface's type object: the class's metaclass, or the
// interface's class shape.
def ClassOf of Node : Instance, Definition, Type {
    let Of: Type;

    @explicitThis new(this, of: Type) { this.Of = of; }
}

// The property of a name a class or an interface has: what its accessors of the name,
// `get Name` and `set Name`, read and write.
fun pairAccessors(methods: List[Method], properties: List[Property], name: String) {
    let property = Property(name);
    let index = 0;
    while (index < methods.Count) {
        if (methods[index].Name == "get " + name) property.Getter = methods[index];
        if (methods[index].Name == "set " + name) property.Setter = methods[index];
        index = index + 1;
    }
    properties.Add(property);
}

// ---- Decorators

// Marks a field or a method of a class as the class's own, not its objects'.
fun static(member: Instance): Instance {
    if (member is let method: Method) {
        method.IsStatic = true;
        method.Parameters.Remove(method.ThisParameter);
    }
    if (member is let field: Field) field.IsStatic = true;
    return member;
}

// Marks a field or a method of a class as its type object's: a class field is one for
// the class, and a class method runs on the class. An interface's class method is one
// the class of each of its values has.
fun class(member: Instance): Instance {
    if (member is let method: Method) {
        method.IsClass = true;
        let receiver = method.ThisParameter as Positional;
        receiver.Type = ClassOf(receiver.Type);
    }
    if (member is let field: Field) field.IsClass = true;
    return member;
}

// ---- Installing it

// Importing it installs what compiles its definitions, so `def` is a Z# class. A program that only
// reflects loads it for its nodes, which doesn't import it, so it gets its structure alone.
@entrypoint
fun install() { importers.Import("std:oop/compiler").Unwrap(); }
