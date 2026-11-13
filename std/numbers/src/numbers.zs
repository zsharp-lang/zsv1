// std:numbers — Z#'s numbers: their structure.
//
// A type of numbers is a definition whose meta-type says what its numbers are: a type of
// integers is a FixedBitWidthInteger, of a bit width and a signedness, and a type of
// floating-point numbers a FloatingPoint, of a bit width. So `def I32 of
// FixedBitWidthInteger(32, true) {}` defines I32, and std:numbers/integer and
// std:numbers/real define the types and the operators on them. What a platform's numbers
// are is the platform's: a type lowers to what its target says it is, and an operator,
// declared with no code, to what the platform implements it with. std:numbers/compiler is
// what the compiler needs of them, which importing this installs.
//
// The nodes are the platform's own definitions, written with one meta-type, `Node`, so
// another platform reimplements them by giving that name its own.

import { String, Int32, Boolean } from "core:runtime";
import { Class as Node } from "pkg:clr/hir";
import { public, explicitThis } from "pkg:clr/hir";
import { importers } from "core:zsharp";
import { entrypoint } from "std:framework";
import { Instance, Definition, Type, Named as NamedDefinition } from "pkg:zsharp/hir";

// ---- The HIR

// A type of integers of a fixed bit width, signed or not. Two of one width and signedness
// are the same type, whatever their names.
@public def FixedBitWidthInteger of Node : Instance, Definition, Type, NamedDefinition {
    @public let name: String;
    @public let BitWidth: Int32;
    @public let Signed: Boolean;

    @public @explicitThis new(this, bitWidth: Int32, signed: Boolean) {
        this.name = "";
        this.BitWidth = bitWidth;
        this.Signed = signed;
    }

    @public @explicitThis new(this, name: String, bitWidth: Int32, signed: Boolean) {
        this.name = name;
        this.BitWidth = bitWidth;
        this.Signed = signed;
    }

    @public @explicitThis get Name(this): String { return this.name; }
}

// A type of floating-point numbers of a bit width, as IEEE 754 has them. Two of one width
// are the same type, whatever their names.
@public def FloatingPoint of Node : Instance, Definition, Type, NamedDefinition {
    @public let name: String;
    @public let BitWidth: Int32;

    @public @explicitThis new(this, bitWidth: Int32) {
        this.name = "";
        this.BitWidth = bitWidth;
    }

    @public @explicitThis new(this, name: String, bitWidth: Int32) {
        this.name = name;
        this.BitWidth = bitWidth;
    }

    @public @explicitThis get Name(this): String { return this.name; }
}

// ---- Installing it

// Importing it installs what compiles and loads its types. A program that only reflects
// loads it for its nodes, which doesn't import it, so it gets its structure alone.
@entrypoint
fun install() { importers.Import("std:numbers/compiler").Unwrap(); }
