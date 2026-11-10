// std:framework — what a module declares it is to the environment that imports it.
//
// Code that builds a definition runs as it is built, so a library that installs itself,
// as one that registers a meta-type does, says so with declarations instead: a function
// decorated with what it is, `@metaType(Module)`, `@definitionLoader`, or an explicit
// `@entrypoint`. A decorator only says it, as an annotation of the function. `build`
// reads a module's annotations and gives it an entry point, a function that does what
// they declare, in their order, then calls its explicit entry points: what importing the
// module calls, built or compiled from its sources, and building it doesn't.

import { String, Object, Boolean, Int32, List, Func, DotNet_Class, DotNet_Module, explicitThis } from "core:runtime";
import { Instance, Definition, Annotated, Type } from "std:zsharp/hir";
import { Code, Block, Get, Call, Cast, Index, NamedMember, Argument, Expression, ExpressionStatement } from "std:zsharp/hir/code";
import { Function, Positional } from "std:functions";
import { Module } from "std:modules";
import { core } from "core:zsharp";

// ---- What a function is said to be

// Says something of a definition that can be annotated, and gives the definition.
fun say(member: Instance, annotation: Object): Instance {
    if (member is annotated of Annotated) annotated.Annotations.Add(annotation);
    return member;
}

// It is called when the module is imported, after what the module's declarations do.
def EntryPoint of DotNet_Class {
}

// It is what loads a kind of node, the type of its parameter, into a CO: as a definition,
// as an instance, or as a type, by the loader's registry of that name.
def Loader of DotNet_Class {
    let Registry: String;

    @explicitThis new(this, registry: String) { this.Registry = registry; }
}

// It is the meta-compiler of a meta-type, the definition it describes.
def MetaType of DotNet_Class {
    let Type: Instance;

    @explicitThis new(this, type: Instance) { this.Type = type; }

    // As a decorator: says it of the function it is given.
    @explicitThis fun apply(this, member: Instance): Instance { return say(member, this); }
}

// ---- The decorators

fun entrypoint(member: Instance): Instance { return say(member, EntryPoint()); }

fun definitionLoader(member: Instance): Instance { return say(member, Loader("Definitions")); }

fun instanceLoader(member: Instance): Instance { return say(member, Loader("Instances")); }

fun typeLoader(member: Instance): Instance { return say(member, Loader("Types")); }

fun metaType(type: Type): Func[Instance, Instance] { return MetaType(type as Instance).apply; }

// ---- Building

// What the code of an entry point is made of: the members of the toolchain's libraries
// it reaches, as a body's imports are reached, and calls.
fun library(name: String, member: String): Expression {
    let reached = NamedMember();
    reached.Target = Get(core.Import(name).Unwrap());
    reached.Name = member;
    return reached;
}

fun member(target: Expression, name: String): Expression {
    let reached = NamedMember();
    reached.Target = target;
    reached.Name = name;
    return reached;
}

fun argument(value: Expression): Argument {
    let argument = Argument();
    argument.Value = value;
    return argument;
}

fun call(callee: Expression, arguments: List[Argument]): ExpressionStatement {
    let call = Call();
    call.Callee = callee;
    call.Arguments = arguments;
    return ExpressionStatement(call);
}

// `mappedLoader.<registry>.Register[T](function)`, T the type of its parameter.
fun registerLoader(function: Function, registry: String): ExpressionStatement {
    let node = (function.Parameters[0] as Positional).Type as Instance;
    let register = Index();
    register.Target = member(member(library("compiler", "mappedLoader"), registry), "Register");
    register.Arguments.Add(argument(Get(node)));

    let arguments = List[Argument]();
    arguments.Add(argument(Get(function)));
    return call(register, arguments);
}

// `metaTypes.Register(M as Instance, function)`.
fun registerMetaType(function: Function, type: Instance): ExpressionStatement {
    let described = Cast();
    described.Value = Get(type);
    described.Type = Get(Instance as Instance);

    let arguments = List[Argument]();
    arguments.Add(argument(described));
    arguments.Add(argument(Get(function)));
    return call(member(library("zsharp", "metaTypes"), "Register"), arguments);
}

// What a function's annotations declare, as code, added to the entry point's; and
// whether it is an explicit entry point.
fun declared(function: Function, code: Block): Boolean {
    let explicit = false;
    let index = 0;
    while (index < function.Annotations.Count) {
        let annotation = function.Annotations[index];
        if (annotation is said of Loader) code.Statements.Add(registerLoader(function, said.Registry));
        if (annotation is meta of MetaType) code.Statements.Add(registerMetaType(function, meta.Type));
        if (annotation is entry of EntryPoint) explicit = true;
        index = index + 1;
    }
    return explicit;
}

// Gives a module the entry point its declarations make, and gives the module. One built
// already isn't built again.
fun build(node: Module): Module {
    if (node.EntryPoint != null) return node;

    let entry = Function("<entry>");
    entry.Body = Code();
    let explicit = List[Function]();

    let index = 0;
    while (index < node.Definitions.Count) {
        if (node.Definitions[index] is function of Function) {
            if (declared(function, entry.Body.Block)) explicit.Add(function);
        }
        index = index + 1;
    }

    index = 0;
    while (index < explicit.Count) {
        entry.Body.Block.Statements.Add(call(Get(explicit[index]), List[Argument]()));
        index = index + 1;
    }

    node.Definitions.Add(entry);
    node.EntryPoint = entry;
    return node;
}
