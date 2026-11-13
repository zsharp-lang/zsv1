// std:numbers/compiler — what the compiler needs of std:numbers' types.
//
// - A definition whose meta-type is a FixedBitWidthInteger or a FloatingPoint, as `def
//   I32 of FixedBitWidthInteger(32, true) {}`, is a type of numbers: its meta-compiler
//   builds the definition's node, named as it is, with what the meta-type says.
// - A type of numbers is one CO for each kind, bit width and signedness, so every node of
//   one, wherever it was built or decoded, is one type. It lowers to what its target says
//   its numbers are, through the Numbers capability.
// - A platform says what each type is in its own types, which code that uses them sees,
//   by `represent`: a cast between a type and what represents it changes nothing, and a
//   cast between two types of numbers, or to another of the platform's types, is the
//   platform's cast of what represents it. Nothing converts implicitly yet.

import { String, Object, Boolean, Int32, Dictionary } from "core:runtime";
import { Class as DotNet_Class, public } from "pkg:clr/hir";
import { Instance, Type } from "pkg:zsharp/hir";
import { Block, Local, StringLiteral, IntegerLiteral, BooleanLiteral } from "pkg:zsharp/hir/code";
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
import { Numbers } from "std:zsharp/capabilities";
import { FixedBitWidthInteger, FloatingPoint } from "std:numbers";

// ---- The types

// What a type of numbers is, as a key: its kind, signedness and bit width, as "I32",
// "U8" or "F64". Every node of one has it.
fun keyOf(node: Instance): String {
    if (node is let integer: FixedBitWidthInteger) {
        if (integer.Signed) return "I" + integer.BitWidth;
        return "U" + integer.BitWidth;
    }
    if (node is let real: FloatingPoint) return "F" + real.BitWidth;
    return null;
}

// What a platform says represents each type in its own types.
let representations: Dictionary[String, CompilerObject] = Dictionary[String, CompilerObject]();

// Says what represents a type of numbers in the platform's own types, by its key, as the
// CLR's Int32 is I32: a cast between them changes nothing.
fun represent(key: String, represented: CompilerObject) {
    representations[key] = represented;
}

// A type of numbers: one for each kind, bit width and signedness.
def NumberType : CompilerObject, ICompileIRType, IAssignableTo, IRTCastTo, ICTCastFrom, ICTImplicitCastTo, ICompileIRDefinitionIn[LIRModule], IRepresented {
    let node: Instance;
    let key: String;

    new(this, node: Instance, key: String) {
        this.node = node;
        this.key = key;
    }

    // What represents it on the platform, as the platform's own types see it.
    fun get_Representation(this): CompilerObject { return this.representation(); }

    // What represents it on the platform, or null if the platform hasn't said.
    fun representation(this): CompilerObject {
        if (representations.ContainsKey(this.key)) return representations[this.key];
        return null;
    }

    // Where what describes a type is expected, as in a signature, a type of numbers goes
    // as its node, as a definition does.
    fun ImplicitCast(this, type: CompilerObject): IResult[CompilerObject, Error] {
        if (ts.IsAssignableTo(hirLoader.LoadType(Type as Type).Unwrap(), type) == false) return Result[CompilerObject].Error(UnsupportedError("A type of numbers goes as its node only where a type is described."));
        let described = hirLoader.LoadType(Instance as Type).Unwrap();
        if (this.node is let integer: FixedBitWidthInteger) described = hirLoader.LoadType(FixedBitWidthInteger as Type).Unwrap();
        if (this.node is let real: FloatingPoint) described = hirLoader.LoadType(FloatingPoint as Type).Unwrap();
        if (ts.IsAssignableTo(type, described) == false) return Result[CompilerObject].Error("A type of numbers is only described by its node.");
        return Result[CompilerObject].Ok(Known(this.node, described));
    }

    // A module that holds it has nothing of it to lower: its numbers are the platform's.
    fun CompileIRDefinition(this, owner: LIRModule, target: Object): Error { return null; }

    // A value of a type goes to the type alone: converting changes the value, so it is a
    // cast, written.
    fun IsAssignableTo(this, target: CompilerObject): Boolean { return target == this; }

    fun CompileIRType(this, target: Object): IResult[LIRType, Error] {
        if ((target is let numbers: Numbers) == false) return Result[LIRType].Error("The target has no numbers, so " + this.key + " can't be lowered for it.");
        if (this.node is let integer: FixedBitWidthInteger) return Result[LIRType].Ok(numbers.integer(integer.BitWidth, integer.Signed));
        let real = this.node as FloatingPoint;
        return Result[LIRType].Ok(numbers.real(real.BitWidth));
    }

    // A value of it cast to another type: to what represents it, which changes nothing;
    // to another type of numbers, as what represents each; or to another of the
    // platform's types, as what represents it.
    fun Cast(this, value: CompilerObject, target: CompilerObject): IResult[CompilerObject, Error] {
        if (target == this) return Result[CompilerObject].Ok(value);

        let represented = this.representation();
        if (represented == null) return Result[CompilerObject].Error(UnsupportedError("The platform hasn't said what " + this.key + " is, so it can't be cast."));
        if (target == represented) return Result[CompilerObject].Ok(Retyped(value, represented));

        if (target is let other: NumberType) {
            let to = other.representation();
            if (to == null) return Result[CompilerObject].Error(UnsupportedError("The platform hasn't said what " + other.key + " is, so it can't be cast."));
            let converted = cg.Cast(Retyped(value, represented), to);
            if (converted.IsError) return converted;
            return Result[CompilerObject].Ok(Retyped(converted.Unwrap(), other));
        }

        return cg.Cast(Retyped(value, represented), target);
    }

    // A value of another type cast to it: as a cast to what represents it.
    fun Cast(this, value: CompilerObject): IResult[CompilerObject, Error] {
        let represented = this.representation();
        if (represented == null) return Result[CompilerObject].Error(UnsupportedError("The platform hasn't said what " + this.key + " is, so nothing can be cast to it."));
        let converted = cg.Cast(value, represented);
        if (converted.IsError) return converted;
        return Result[CompilerObject].Ok(Retyped(converted.Unwrap(), this));
    }
}

// A value the compiler knows, as a type's node, placed in the code as a constant.
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

// The type each key is, once it is loaded.
let types: Dictionary[String, NumberType] = Dictionary[String, NumberType]();

// The type a node of numbers is: one CO for each key.
fun typeOf(node: Instance): IResult[CompilerObject, Error] {
    let key = keyOf(node);
    if (types.ContainsKey(key) == false) types[key] = NumberType(node, key);
    return Result[CompilerObject].Ok(types[key]);
}

// A type of numbers is a definition a module holds, used as a type and as an instance:
// each is the one type its key is.
@definitionLoader
fun loadInteger(node: FixedBitWidthInteger): IResult[CompilerObject, Error] { return typeOf(node); }
@definitionLoader
fun loadReal(node: FloatingPoint): IResult[CompilerObject, Error] { return typeOf(node); }
@typeLoader
fun typeOfInteger(node: FixedBitWidthInteger): IResult[CompilerObject, Error] { return typeOf(node); }
@instanceLoader
fun instanceOfInteger(node: FixedBitWidthInteger): IResult[CompilerObject, Error] { return typeOf(node); }
@typeLoader
fun typeOfReal(node: FloatingPoint): IResult[CompilerObject, Error] { return typeOf(node); }
@instanceLoader
fun instanceOfReal(node: FloatingPoint): IResult[CompilerObject, Error] { return typeOf(node); }

// ---- Defining them

let previousFind: MetaCompilerOf;
let integerCompiler: MetaCompiler;
let realCompiler: MetaCompiler;

// A type of numbers has nothing to fill: what it is, its meta-type said.
@public def Unfilled of DotNet_Class {
    @public new() { }
    @public fun fill(): IResult[Block, Error] { return Result[Block].Ok(Block([])); }
}

// `def I32 of FixedBitWidthInteger(32, true) {}` is the type of 32-bit signed integers,
// named I32.
fun compileInteger(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
    if (definition is let type: TypeSyntax) {
        let of = self.Type as FixedBitWidthInteger;
        self.Type = FixedBitWidthInteger as Type;

        return Result[CompiledDefinition].Ok(CompiledDefinition(
            self,
            Bodies.Construct(FixedBitWidthInteger as Instance, StringLiteral(type.Name), IntegerLiteral(of.BitWidth), BooleanLiteral(of.Signed)),
            FixedBitWidthInteger as Instance,
            Unfilled().fill,
            null
        ));
    }

    return Result[CompiledDefinition].Error("A type of integers is a type definition.");
}

// `def F64 of FloatingPoint(64) {}` is the type of 64-bit floating-point numbers.
fun compileReal(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
    if (definition is let type: TypeSyntax) {
        let of = self.Type as FloatingPoint;
        self.Type = FloatingPoint as Type;

        return Result[CompiledDefinition].Ok(CompiledDefinition(
            self,
            Bodies.Construct(FloatingPoint as Instance, StringLiteral(type.Name), IntegerLiteral(of.BitWidth)),
            FloatingPoint as Instance,
            Unfilled().fill,
            null
        ));
    }

    return Result[CompiledDefinition].Error("A type of floating-point numbers is a type definition.");
}

// A definition whose meta-type is a type of numbers is compiled by these, and anything
// else as before.
fun findMetaCompiler(metaType: Instance): IResult[MetaCompiler, Error] {
    if (metaType is let integer: FixedBitWidthInteger) return Result[MetaCompiler].Ok(integerCompiler);
    if (metaType is let real: FloatingPoint) return Result[MetaCompiler].Ok(realCompiler);
    return previousFind(metaType);
}

@entrypoint
fun install() {
    integerCompiler = compileInteger;
    realCompiler = compileReal;
    previousFind = metaTypes.Find;
    metaTypes.Find = findMetaCompiler;
}
