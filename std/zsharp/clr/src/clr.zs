// std:zsharp/clr — the capabilities every CLR platform has, given to them by traits: the
// running process, or an assembly written for a version of .NET. Each marks LIR with the
// CLR's metadata, which every CLR platform realizes. It also says what std:numbers' types
// are on the CLR: what represents each, and what implements each operator declared on
// them.

import { Polymorphism, PlatformInterfaces, Numbers, Truths } from "std:zsharp/capabilities";
import { Implementation, implement } from "std:traits";
import { entrypoint } from "std:framework";
import { explicitThis } from "pkg:clr/hir";
import { ClrPlatform, Polymorphic, MethodOf, CastDown, TypeTest } from "pkg:clr/lir";
import { public } from "pkg:clr/hir";
import { Structure, Function, Type as LIRType } from "pkg:zsharp/lir";
import { Object, String, Int32, Boolean, List, Type as ClrType, ilLoader, predefined } from "core:runtime";
import { SByte, Int16, Int64, Byte, UInt16, UInt32, UInt64, Single, Double } from "core:runtime";
import { Half } from Object.Assembly;
import { Instance } from "pkg:zsharp/hir";
import { importers, language } from "core:zsharp";
import { ir, hirLoader } from "core:compiler";
import { Function as ZFunction, Positional } from "std:functions";
import { premap } from "std:functions/compiler";
import { Module } from "std:modules";
import { keyOf, represent } from "std:numbers/compiler";
import { represent as representTruth } from "std:boolean/compiler";
import { Truth } from "std:boolean/truth";

// The CLR has polymorphism: a polymorphic structure is a CLR class or interface, and a
// method of one is a method of the class.
def ClrPolymorphism of Implementation[Polymorphism, ClrPlatform] {
    @public let value: ClrPlatform;

    @public @explicitThis new(this, value: ClrPlatform) { this.value = value; }

    @public @explicitThis fun objects(this, structure: Structure) {
        structure.Metadata.Add(Polymorphic());
    }

    @public @explicitThis fun interfaces(this, structure: Structure) {
        let mark = Polymorphic();
        mark.IsInterface = true;
        structure.Metadata.Add(mark);
    }

    @public @explicitThis fun implements(this, structure: Structure, implemented: Structure) {
        let index = 0;
        while (index < structure.Metadata.Count) {
            if (structure.Metadata[index] is let mark: Polymorphic) mark.Interfaces.Add(implemented);
            index = index + 1;
        }
    }

    @public @explicitThis fun method(this, function: Function, owner: Structure) {
        function.Metadata.Add(MethodOf(owner));
    }

    @public @explicitThis fun cast(this, function: Function, to: Structure) {
        function.Metadata.Add(CastDown(to));
    }

    @public @explicitThis fun test(this, function: Function, target: Structure) {
        function.Metadata.Add(TypeTest(target));
    }
}

// A CLR class can implement the CLR's own interfaces, as the toolchain's are: the class
// a polymorphic structure is implements them.
def ClrInterfaces of Implementation[PlatformInterfaces, ClrPlatform] {
    @public let value: ClrPlatform;

    @public @explicitThis new(this, value: ClrPlatform) { this.value = value; }

    @public @explicitThis fun implements(this, structure: Structure, implemented: LIRType) {
        let index = 0;
        while (index < structure.Metadata.Count) {
            if (structure.Metadata[index] is let mark: Polymorphic) mark.Implements.Add(implemented);
            index = index + 1;
        }
    }
}

// The CLR's numbers: each type of std:numbers is a CLR type of its bit width and
// signedness, so it lowers to that type.
def ClrNumbers of Implementation[Numbers, ClrPlatform] {
    @public let value: ClrPlatform;

    @public @explicitThis new(this, value: ClrPlatform) { this.value = value; }

    @public @explicitThis fun integer(this, bitWidth: Int32, signed: Boolean): LIRType {
        return lowered(clrTypeOf(numberKey(bitWidth, signed, false)), this.value);
    }

    @public @explicitThis fun real(this, bitWidth: Int32): LIRType {
        return lowered(clrTypeOf(numberKey(bitWidth, true, true)), this.value);
    }
}

// The CLR's truth values: Bool is the CLR's Boolean.
def ClrTruths of Implementation[Truths, ClrPlatform] {
    @public let value: ClrPlatform;

    @public @explicitThis new(this, value: ClrPlatform) { this.value = value; }

    @public @explicitThis fun boolean(this): LIRType { return lowered(Boolean, this.value); }
}

// The key std:numbers/compiler gives a type of numbers: "I32", "U8", "F64".
fun numberKey(bitWidth: Int32, signed: Boolean, real: Boolean): String {
    if (real) return "F" + bitWidth;
    if (signed) return "I" + bitWidth;
    return "U" + bitWidth;
}

// The CLR type a type of numbers is, by its key, or null if the CLR has none of it.
fun clrTypeOf(key: String): ClrType {
    if (key == "I8") return SByte;
    if (key == "I16") return Int16;
    if (key == "I32") return Int32;
    if (key == "I64") return Int64;
    if (key == "U8") return Byte;
    if (key == "U16") return UInt16;
    if (key == "U32") return UInt32;
    if (key == "U64") return UInt64;
    if (key == "F16") return Half;
    if (key == "F32") return Single;
    if (key == "F64") return Double;
    return null;
}

// What the CLR computes a type of numbers with: an integer narrower than 32 bits is
// computed with as an Int32, as C# promotes it, and kept to its width where it is stored.
fun operandOf(key: String): ClrType {
    if (key == "I8") return Int32;
    if (key == "I16") return Int32;
    if (key == "U8") return Int32;
    if (key == "U16") return Int32;
    return clrTypeOf(key);
}

fun lowered(type: ClrType, target: Object): LIRType {
    return ir.CompileType(hirLoader.LoadType(ilLoader.Load(type)).Unwrap(), target).Unwrap();
}

// What the CLR computes an integer narrower than 32 bits with: an Int32, as C# promotes
// it, narrowed back to the type, so its value stays in range.

fun addI8(left: SByte, right: SByte): SByte { return ((left as Int32) + (right as Int32)) as SByte; }
fun subtractI8(left: SByte, right: SByte): SByte { return ((left as Int32) - (right as Int32)) as SByte; }
fun multiplyI8(left: SByte, right: SByte): SByte { return ((left as Int32) * (right as Int32)) as SByte; }
fun divideI8(left: SByte, right: SByte): SByte { return ((left as Int32) / (right as Int32)) as SByte; }
fun negateI8(operand: SByte): SByte { return (-(operand as Int32)) as SByte; }
fun addI16(left: Int16, right: Int16): Int16 { return ((left as Int32) + (right as Int32)) as Int16; }
fun subtractI16(left: Int16, right: Int16): Int16 { return ((left as Int32) - (right as Int32)) as Int16; }
fun multiplyI16(left: Int16, right: Int16): Int16 { return ((left as Int32) * (right as Int32)) as Int16; }
fun divideI16(left: Int16, right: Int16): Int16 { return ((left as Int32) / (right as Int32)) as Int16; }
fun negateI16(operand: Int16): Int16 { return (-(operand as Int32)) as Int16; }
fun addU8(left: Byte, right: Byte): Byte { return ((left as Int32) + (right as Int32)) as Byte; }
fun subtractU8(left: Byte, right: Byte): Byte { return ((left as Int32) - (right as Int32)) as Byte; }
fun multiplyU8(left: Byte, right: Byte): Byte { return ((left as Int32) * (right as Int32)) as Byte; }
fun divideU8(left: Byte, right: Byte): Byte { return ((left as Int32) / (right as Int32)) as Byte; }
fun addU16(left: UInt16, right: UInt16): UInt16 { return ((left as Int32) + (right as Int32)) as UInt16; }
fun subtractU16(left: UInt16, right: UInt16): UInt16 { return ((left as Int32) - (right as Int32)) as UInt16; }
fun multiplyU16(left: UInt16, right: UInt16): UInt16 { return ((left as Int32) * (right as Int32)) as UInt16; }
fun divideU16(left: UInt16, right: UInt16): UInt16 { return ((left as Int32) / (right as Int32)) as UInt16; }

// The function that computes an arithmetic operator on an integer narrower than 32 bits,
// by the operator, the key of its type and how many operands it has, found among this
// library's own functions, or null if it isn't one.
fun narrowed(own: Module, syntax: String, key: String, operands: Int32): Instance {
    if (key != "I8") {
        if (key != "I16") {
            if (key != "U8") {
                if (key != "U16") return null;
            }
        }
    }

    let verb: String = null;
    if (syntax == "+") verb = "add";
    if (syntax == "*") verb = "multiply";
    if (syntax == "/") verb = "divide";
    if (syntax == "-") {
        verb = "subtract";
        if (operands == 1) verb = "negate";
    }
    if (verb == null) return null;

    let index = 0;
    while (index < own.Definitions.Count) {
        if (own.Definitions[index] is let function: ZFunction) {
            if (function.Name == verb + key) return function;
        }
        index = index + 1;
    }
    return null;
}

// Each operator declared with no code on std:numbers' types is what the CLR has for it, on
// the types that represent them, as `+` on two I32s is the CLR's Int32 addition.
fun implementOperators(own: Module) {
    let syntaxes: String[] = ["+", "-", "*", "/", "<", ">", "<=", ">=", "==", "!="];
    let at = 0;
    while (at < syntaxes.Length) {
        let syntax = syntaxes[at];
        if (language.Functions.ContainsKey(syntax)) {
            let items = language.Functions[syntax].Items;
            let index = 0;
            while (index < items.Count) {
                if (items[index] is let function: ZFunction) {
                    if (function.Body == null) {
                        let to = implementationOf(own, syntax, function);
                        if (to != null) premap(function, to);
                    }
                }
                index = index + 1;
            }
        }
        at = at + 1;
    }
}

// What implements an operator declared on std:numbers' types: for an integer narrower
// than 32 bits, its arithmetic narrowed back, and otherwise the CLR's operator on the types
// its operands are computed with. Null if its operands aren't of types of numbers.
fun implementationOf(own: Module, syntax: String, function: ZFunction): Instance {
    let operands = operandsOf(function);
    if (operands == null) return null;

    let first = function.Parameters[0] as Positional;
    let narrow = narrowed(own, syntax, keyOf(first.Type as Instance), function.Parameters.Count);
    if (narrow != null) return narrow;

    return predefined(syntax, operands.ToArray());
}

// The CLR types a function's operands are computed with, if each is of a type of numbers
// or of truth values.
fun operandsOf(function: ZFunction): List[ClrType] {
    let operands = List[ClrType]();
    let index = 0;
    while (index < function.Parameters.Count) {
        if ((function.Parameters[index] is let parameter: Positional) == false) return null;
        if (parameter.Type is let truth: Truth) operands.Add(Boolean);
        else {
            let key = keyOf(parameter.Type as Instance);
            if (key == null) return null;
            operands.Add(operandOf(key));
        }
        index = index + 1;
    }
    return operands;
}

// Importing it makes the implementations known, as loading their definitions does where
// it is built, and says what std:numbers' types are on the CLR.
@entrypoint
fun install() {
    implement(Polymorphism, ClrPlatform, ClrPolymorphism);
    implement(PlatformInterfaces, ClrPlatform, ClrInterfaces);
    implement(Numbers, ClrPlatform, ClrNumbers);
    implement(Truths, ClrPlatform, ClrTruths);

    // The libraries are held, as importing a name from one holds it, so their operators
    // are the language's before they are implemented.
    hirLoader.LoadInstance(importers.Import("std:numbers/integer").Unwrap()).Unwrap();
    hirLoader.LoadInstance(importers.Import("std:numbers/real").Unwrap()).Unwrap();
    hirLoader.LoadInstance(importers.Import("std:boolean").Unwrap()).Unwrap();
    representTruth(hirLoader.LoadType(ilLoader.Load(Boolean)).Unwrap());

    let keys: String[] = ["I8", "I16", "I32", "I64", "U8", "U16", "U32", "U64", "F16", "F32", "F64"];
    let index = 0;
    while (index < keys.Length) {
        represent(keys[index], hirLoader.LoadType(ilLoader.Load(clrTypeOf(keys[index]))).Unwrap());
        index = index + 1;
    }

    implementOperators(importers.Import("std:zsharp/clr").Unwrap() as Module);
}
