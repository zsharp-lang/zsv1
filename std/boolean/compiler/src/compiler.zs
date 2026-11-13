// std:boolean/compiler — what the compiler needs of std:boolean's type.
//
// - A definition whose meta-type is a Truth, as `def Bool of Truth() {}`, is the type of
//   truth values: its meta-compiler builds the definition's node, named as it is.
// - Every node of one is one type, one CO, which lowers to what its target says its truth
//   values are, through the Truths capability.
// - A platform says what represents it in its own types, by `represent`: a cast between
//   them changes nothing. Nothing converts implicitly yet.

import { String, Object, Boolean } from "core:runtime";
import { Class as DotNet_Class, public } from "pkg:clr/hir";
import { Instance, Type } from "pkg:zsharp/hir";
import { Block, Local, StringLiteral } from "pkg:zsharp/hir/code";
import { Definition as DefinitionSyntax, TypeDefinition as TypeSyntax } from "pkg:zsharp/lang/ast";
import { Type as LIRType, Expression as LIRExpression, Module as LIRModule, Constant } from "pkg:zsharp/lir";
import { CompilerObject } from "pkg:zsharp/mir";
import { Error, Result, IResult } from "pkg:zsharp/core";
import { UnsupportedError } from "core:zsharp/mir/core";
import { ICTCastFrom, ICTImplicitCastTo } from "core:zsharp/mir/cg/direct";
import { IRTCastTo } from "core:zsharp/mir/cg/typed";
import { ICompileIRType, ICompileIRExpression, ICompileIRDefinitionIn } from "core:zsharp/m2l";
import { ITyped, IAssignableTo, IRepresented } from "core:zsharp/mir/ts";
import { ASTCompiler, CompiledDefinition } from "core:zsharp/lang";
import { Bodies, MetaCompilerOf, MetaCompiler } from "pkg:zsharp/lang/hir";
import { metaTypes } from "core:zsharp";
import { entrypoint, definitionLoader, instanceLoader, typeLoader } from "std:framework";
import { cg, ir, ts, hirLoader } from "core:compiler";
import { Truths } from "std:zsharp/capabilities";
import { Truth } from "std:boolean/truth";

// What a platform says represents the type of truth values in its own types.
let representation: CompilerObject = null;

// Says what represents the type of truth values in the platform's own types, as the CLR's
// Boolean is Bool: a cast between them changes nothing.
fun represent(represented: CompilerObject) { representation = represented; }

// The type of truth values.
def TruthType : CompilerObject, ICompileIRType, IAssignableTo, IRTCastTo, ICTCastFrom, ICTImplicitCastTo, ICompileIRDefinitionIn[LIRModule], IRepresented {
    let node: Instance;

    new(this, node: Instance) { this.node = node; }

    fun IsAssignableTo(this, target: CompilerObject): Boolean { return target == this; }

    // What represents it on the platform, as the platform's own types see it.
    fun get_Representation(this): CompilerObject { return representation; }

    fun CompileIRType(this, target: Object): IResult[LIRType, Error] {
        if ((target is let truths: Truths) == false) return Result[LIRType].Error("The target has no truth values, so Bool can't be lowered for it.");
        return Result[LIRType].Ok(truths.boolean());
    }

    // A module that holds it has nothing of it to lower: its truth values are the
    // platform's.
    fun CompileIRDefinition(this, owner: LIRModule, target: Object): Error { return null; }

    // Where what describes a type is expected, as in a signature, the type goes as its
    // node, as a definition does.
    fun ImplicitCast(this, type: CompilerObject): IResult[CompilerObject, Error] {
        if (ts.IsAssignableTo(hirLoader.LoadType(Type as Type).Unwrap(), type) == false) return Result[CompilerObject].Error(UnsupportedError("The type of truth values goes as its node only where a type is described."));
        let described = hirLoader.LoadType(Truth as Type).Unwrap();
        if (ts.IsAssignableTo(type, described) == false) return Result[CompilerObject].Error("The type of truth values is only described by its node.");
        return Result[CompilerObject].Ok(Known(this.node, described));
    }

    // A value of it cast to what represents it, which changes nothing, or to another of the
    // platform's types, as what represents it.
    fun Cast(this, value: CompilerObject, target: CompilerObject): IResult[CompilerObject, Error] {
        if (target == this) return Result[CompilerObject].Ok(value);
        if (representation == null) return Result[CompilerObject].Error(UnsupportedError("The platform hasn't said what Bool is, so it can't be cast."));
        if (target == representation) return Result[CompilerObject].Ok(Retyped(value, representation));
        return cg.Cast(Retyped(value, representation), target);
    }

    // A value of another type cast to it: as a cast to what represents it.
    fun Cast(this, value: CompilerObject): IResult[CompilerObject, Error] {
        if (representation == null) return Result[CompilerObject].Error(UnsupportedError("The platform hasn't said what Bool is, so nothing can be cast to it."));
        let converted = cg.Cast(value, representation);
        if (converted.IsError) return converted;
        return Result[CompilerObject].Ok(Retyped(converted.Unwrap(), this));
    }
}

// A value the compiler knows, as the type's node, placed in the code as a constant.
def Known : CompilerObject, ICompileIRExpression, ITyped {
    let value: Object;
    let type: CompilerObject;

    new(this, value: Object, type: CompilerObject) { this.value = value; this.type = type; }

    fun get_Type(this): CompilerObject { return this.type; }

    fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        let type = ir.CompileType(this.type, target);
        if (type.IsError) return Result[LIRExpression].Error(type.UnwrapError());
        return Result[LIRExpression].Ok(Constant(type.Unwrap(), this.value));
    }
}

// A value seen as of another type, which compiles to the value itself.
def Retyped : CompilerObject, ICompileIRExpression, ITyped {
    let value: CompilerObject;
    let type: CompilerObject;

    new(this, value: CompilerObject, type: CompilerObject) {
        this.value = value;
        this.type = type;
    }

    fun get_Type(): CompilerObject { return type; }

    fun CompileIRExpression(target: Object): IResult[LIRExpression, Error] { return ir.CompileExpression(value, target); }
}

// The one type every node of truth values is.
let type: TruthType = null;

fun typeOf(node: Instance): IResult[CompilerObject, Error] {
    if (type == null) type = TruthType(node);
    return Result[CompilerObject].Ok(type);
}

// The type is a definition a module holds, used as a type and as an instance: each is the
// one type.
@definitionLoader
fun loadTruth(node: Truth): IResult[CompilerObject, Error] { return typeOf(node); }
@typeLoader
fun typeOfTruth(node: Truth): IResult[CompilerObject, Error] { return typeOf(node); }
@instanceLoader
fun instanceOfTruth(node: Truth): IResult[CompilerObject, Error] { return typeOf(node); }

// ---- Defining it

let previousFind: MetaCompilerOf;
let truthCompiler: MetaCompiler;

// The type has nothing to fill: what it is, its meta-type said.
@public def Unfilled of DotNet_Class {
    @public new() { }
    @public fun fill(): IResult[Block, Error] { return Result[Block].Ok(Block([])); }
}

// `def Bool of Truth() {}` is the type of truth values, named Bool.
fun compileTruth(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
    if (definition is let syntax: TypeSyntax) {
        self.Type = Truth as Type;

        return Result[CompiledDefinition].Ok(CompiledDefinition(
            self,
            Bodies.Construct(Truth as Instance, StringLiteral(syntax.Name)),
            Truth as Instance,
            Unfilled().fill,
            null
        ));
    }

    return Result[CompiledDefinition].Error("The type of truth values is a type definition.");
}

// A definition whose meta-type is a Truth is compiled by this, and anything else as before.
fun findMetaCompiler(metaType: Instance): IResult[MetaCompiler, Error] {
    if (metaType is let truth: Truth) return Result[MetaCompiler].Ok(truthCompiler);
    return previousFind(metaType);
}

@entrypoint
fun install() {
    truthCompiler = compileTruth;
    previousFind = metaTypes.Find;
    metaTypes.Find = findMetaCompiler;
}
