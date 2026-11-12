// std:traits — traits, defined in Z#.
//
// An implementation of a trait for a type is a definition of its own, so it is
// first-class for reflection and code generation:
//
//     def StringGreeting of Implementation[Greeting, String] { ... }
//
// For now a trait is a CLR interface, so an implementation of one is the class whose
// objects hold a value of the type as the trait. The dispatchers that make a value of
// the type go where the trait is expected are installed apart, in two flavors:
// std:traits/static decides from the static type, and std:traits/dynamic looks an
// implementation up by the type a value has as the code runs.

import { String, Object, Boolean, Int32, List, Type as ClrType, ilLoader } from "core:runtime";
import { Class as DotNet_Class, Module as DotNet_Module } from "pkg:clr/hir";
import { static, public, explicitThis } from "pkg:clr/hir";
import { Instance, Definition, Type } from "pkg:zsharp/hir";
import { Local, Get, Block, Declare, ExpressionStatement, Set, StringLiteral, Cast, Expression } from "pkg:zsharp/hir/code";
import { Definition as DefinitionSyntax, TypeDefinition as TypeSyntax } from "pkg:zsharp/lang/ast";
import { CompilerObject } from "pkg:zsharp/mir";
import { Error, Result, IResult } from "pkg:zsharp/core";
import { ASTCompiler, CompiledDefinition } from "core:zsharp/lang";
import { Bodies, MetaCompilerOf, MetaCompiler } from "pkg:zsharp/lang/hir";
import { GenericInstance, TypeReference } from "pkg:clr/hir";
import { metaTypes, load } from "core:zsharp";
import { entrypoint, definitionLoader, instanceLoader } from "std:framework";
import { ts, hirLoader, mappedLoader } from "core:compiler";

// ---- The HIR

// The meta-type of an implementation of a trait for a type. It only says what is
// implemented for what, by its type arguments.
@public def Implementation[Trait, For] of DotNet_Class {}

// An implementation of a trait for a type: the trait, the type, and the class whose
// objects hold a value of the type as the trait.
@public def TraitImplementation of DotNet_Class : Instance, Definition {
    @public let Name: String;
    @public let Trait: Type;
    @public let For: Type;
    @public let Adapter: Instance;

    @public @explicitThis new(this, name: String, trait: Type, implemented: Type) {
        this.Name = name;
        this.Trait = trait;
        this.For = implemented;
    }
}

// ---- The implementations that are loaded

// One implementation, as the dispatchers use it: the types and adapter as COs, for
// code being compiled, and as CLR types, for code that runs.
@public def Entry of DotNet_Class {
    @public let implemented: CompilerObject;
    @public let trait: CompilerObject;
    @public let adapter: CompilerObject;
    @public let implementedType: ClrType;
    @public let traitType: ClrType;
    @public let adapterType: ClrType;

    @public @explicitThis new(this, implemented: CompilerObject, trait: CompilerObject, adapter: CompilerObject, implementedType: ClrType, traitType: ClrType, adapterType: ClrType) {
        this.implemented = implemented;
        this.trait = trait;
        this.adapter = adapter;
        this.implementedType = implementedType;
        this.traitType = traitType;
        this.adapterType = adapterType;
    }
}

// The implementations loaded.
let entries: List[Entry] = List[Entry]();

@public def Traits of DotNet_Class {
    @public @static fun add(entry: Entry) { entries.Add(entry); }

    // Whether a type is a trait that something implements.
    @public @static fun isTrait(type: CompilerObject): Boolean {
        let index = 0;
        while (index < entries.Count) {
            let entry = entries[index];
            if (ts.IsAssignableTo(type, entry.trait)) {
                if (ts.IsAssignableTo(entry.trait, type)) return true;
            }
            index = index + 1;
        }
        return false;
    }

    // The adapter that makes a value of the static type of a CO the trait, if any.
    @public @static fun adapterFor(object: CompilerObject, trait: CompilerObject): CompilerObject {
        let typed = ts.TypeOf(object);
        if (typed.IsError) return null;

        let index = 0;
        while (index < entries.Count) {
            let entry = entries[index];
            if (ts.IsAssignableTo(entry.implemented, typed.Unwrap())) {
                if (ts.IsAssignableTo(trait, entry.trait)) {
                    if (ts.IsAssignableTo(entry.trait, trait)) return entry.adapter;
                }
            }
            index = index + 1;
        }
        return null;
    }

    // The implementation of a trait for the type a value has, as the code runs.
    @public @static fun find(value: Object, trait: ClrType): Entry {
        if (value == null) return null;

        let index = 0;
        while (index < entries.Count) {
            let entry = entries[index];
            if (entry.traitType == trait) {
                if (entry.implementedType.IsInstanceOfType(value)) return entry;
            }
            index = index + 1;
        }
        return null;
    }
}

// What code calls as it runs to match a value against a trait or make it one.
@public def Lookup of DotNet_Class {
    @public @static fun has(value: Object, trait: ClrType): Boolean {
        if (trait.IsInstanceOfType(value)) return true;
        return Traits.find(value, trait) != null;
    }

    @public @static fun adapt(value: Object, trait: ClrType): Object {
        if (trait.IsInstanceOfType(value)) return value;

        let entry = Traits.find(value, trait);
        if (entry == null) return null;

        return entry.adapterType.GetConstructors()[0].Invoke([value]);
    }
}

// ---- The meta-compiler

// Code that gives a type's node, not the type: the type cast to what describes it, which
// is reflection, as what describes it is given.
@public def Described of DotNet_Class {
    @public @static fun node(type: Type, description: Instance): Expression {
        let cast = Cast();
        cast.Value = Get(type as Instance);
        cast.Type = Get(description);
        return cast;
    }
}

// What fills an implementation as it is built: its adapter, a CLR class compiled from
// the definition's own members, which implements the trait.
@public def Filler of DotNet_Class {
    @public let compiler: ASTCompiler;
    @public let definition: DefinitionSyntax;
    @public let self: Local;
    @public let trait: Type;

    @public @explicitThis new(this, compiler: ASTCompiler, definition: DefinitionSyntax, self: Local, trait: Type) {
        this.compiler = compiler;
        this.definition = definition;
        this.self = self;
        this.trait = trait;
    }

    @public @explicitThis fun fill(this): IResult[Block, Error] {
        let compiled = metaTypes.Compile(this.compiler, this.definition, DotNet_Class as Instance);
        if (compiled.IsError) return Result[Block].Error(compiled.UnwrapError());

        let built = compiled.Unwrap().Build();
        if (built.IsError) return Result[Block].Error(built.UnwrapError());

        let block = Block([]);
        let adapter = Local("adapter", DotNet_Class as Type);
        adapter.Init = built.Unwrap();
        block.Statements.Add(Declare(adapter));
        block.Statements.Add(ExpressionStatement(Bodies.Add(adapter, "Interfaces", Described.node(this.trait, TypeReference as Instance))));

        let set = Set();
        set.Target = Bodies.Member(this.self, "Adapter");
        set.Value = Get(adapter);
        block.Statements.Add(ExpressionStatement(set));

        return Result[Block].Ok(block);
    }
}

let previousFind: MetaCompilerOf;
let implementation: Instance;
let metaCompiler: MetaCompiler;

fun compileImplementation(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
    if (definition is let type: TypeSyntax) {
        let metaType = self.Type as GenericInstance;
        let trait = metaType.Arguments[0];
        let implemented = metaType.Arguments[1];

        // What the definition builds is an implementation, of which the meta-type
        // only says what it implements for what.
        self.Type = TraitImplementation as Type;

        let filler = Filler(compiler, definition, self, trait);

        return Result[CompiledDefinition].Ok(CompiledDefinition(
            self,
            Bodies.Construct(TraitImplementation as Instance, StringLiteral(type.Name), Described.node(trait, Type as Instance), Described.node(implemented, Type as Instance)),
            TraitImplementation as Instance,
            filler.fill,
            null
        ));
    }

    return Result[CompiledDefinition].Error("An implementation is a type definition.");
}

// An instantiation of Implementation is compiled by this, and anything else as before.
fun findMetaCompiler(metaType: Instance): IResult[MetaCompiler, Error] {
    if (metaType is let instance: GenericInstance) {
        if (instance.Definition == implementation) return Result[MetaCompiler].Ok(metaCompiler);
    }

    return previousFind(metaType);
}

// Loading an implementation defines its adapter and makes it known.
@definitionLoader
fun loadImplementation(node: TraitImplementation): IResult[CompilerObject, Error] {
    let adapter = hirLoader.LoadDefinition(node.Adapter as Definition);
    if (adapter.IsError) return adapter;

    let trait = hirLoader.LoadType(node.Trait);
    if (trait.IsError) return trait;

    let implemented = hirLoader.LoadType(node.For);
    if (implemented.IsError) return implemented;

    Traits.add(Entry(
        implemented.Unwrap(),
        trait.Unwrap(),
        adapter.Unwrap(),
        load(node.For as Instance) as ClrType,
        load(node.Trait as Instance) as ClrType,
        load(node.Adapter) as ClrType
    ));

    return adapter;
}

@instanceLoader
fun instanceOfImplementation(node: TraitImplementation): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }

// An implementation that was built, by its trait, the type it is for and its adapter, as
// importing what defines it makes it known, as loading its definition does.
fun implement(trait: ClrType, implemented: ClrType, adapter: ClrType) {
    Traits.add(Entry(
        hirLoader.LoadType(ilLoader.Load(implemented)).Unwrap(),
        hirLoader.LoadType(ilLoader.Load(trait)).Unwrap(),
        hirLoader.LoadType(ilLoader.Load(adapter)).Unwrap(),
        implemented,
        trait,
        adapter
    ));
}

// ---- Installing it

// Implementation's instantiations are compiled by its meta-compiler, as importing it
// makes them.
@entrypoint
fun install() {
    implementation = Implementation as Instance;
    metaCompiler = compileImplementation;
    previousFind = metaTypes.Find;
    metaTypes.Find = findMetaCompiler;
}
