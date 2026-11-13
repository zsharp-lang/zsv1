// std:numbers/integer — the concept of an integer: Z#'s types of integers, of a fixed bit
// width, signed or not, and the operators on them.
//
// Each operator is declared with no code, so it is what the platform implements it with:
// the platform says, as std:zsharp/clr does for the CLR. Each type has the arithmetic
// operators, and the signed ones negation, of operands of the type, and the comparisons.
// A comparison gives the CLR's Boolean until Z# has its own.

import { Boolean } from "core:runtime";
import { operator } from "core:zsharp";
import { FixedBitWidthInteger } from "std:numbers";

// ---- The types

def I8 of FixedBitWidthInteger(8, true) {}
def I16 of FixedBitWidthInteger(16, true) {}
def I32 of FixedBitWidthInteger(32, true) {}
def I64 of FixedBitWidthInteger(64, true) {}
def U8 of FixedBitWidthInteger(8, false) {}
def U16 of FixedBitWidthInteger(16, false) {}
def U32 of FixedBitWidthInteger(32, false) {}
def U64 of FixedBitWidthInteger(64, false) {}

// ---- The operators

// I8
@operator((+)) fun add(left: I8, right: I8): I8;
@operator((-)) fun subtract(left: I8, right: I8): I8;
@operator((*)) fun multiply(left: I8, right: I8): I8;
@operator((/)) fun divide(left: I8, right: I8): I8;
@operator((-)) fun negate(operand: I8): I8;
@operator((<)) fun lessThan(left: I8, right: I8): Boolean;
@operator((>)) fun greaterThan(left: I8, right: I8): Boolean;
@operator((<=)) fun atMost(left: I8, right: I8): Boolean;
@operator((>=)) fun atLeast(left: I8, right: I8): Boolean;
@operator((==)) fun equal(left: I8, right: I8): Boolean;
@operator((!=)) fun notEqual(left: I8, right: I8): Boolean;

// I16
@operator((+)) fun add(left: I16, right: I16): I16;
@operator((-)) fun subtract(left: I16, right: I16): I16;
@operator((*)) fun multiply(left: I16, right: I16): I16;
@operator((/)) fun divide(left: I16, right: I16): I16;
@operator((-)) fun negate(operand: I16): I16;
@operator((<)) fun lessThan(left: I16, right: I16): Boolean;
@operator((>)) fun greaterThan(left: I16, right: I16): Boolean;
@operator((<=)) fun atMost(left: I16, right: I16): Boolean;
@operator((>=)) fun atLeast(left: I16, right: I16): Boolean;
@operator((==)) fun equal(left: I16, right: I16): Boolean;
@operator((!=)) fun notEqual(left: I16, right: I16): Boolean;

// I32
@operator((+)) fun add(left: I32, right: I32): I32;
@operator((-)) fun subtract(left: I32, right: I32): I32;
@operator((*)) fun multiply(left: I32, right: I32): I32;
@operator((/)) fun divide(left: I32, right: I32): I32;
@operator((-)) fun negate(operand: I32): I32;
@operator((<)) fun lessThan(left: I32, right: I32): Boolean;
@operator((>)) fun greaterThan(left: I32, right: I32): Boolean;
@operator((<=)) fun atMost(left: I32, right: I32): Boolean;
@operator((>=)) fun atLeast(left: I32, right: I32): Boolean;
@operator((==)) fun equal(left: I32, right: I32): Boolean;
@operator((!=)) fun notEqual(left: I32, right: I32): Boolean;

// I64
@operator((+)) fun add(left: I64, right: I64): I64;
@operator((-)) fun subtract(left: I64, right: I64): I64;
@operator((*)) fun multiply(left: I64, right: I64): I64;
@operator((/)) fun divide(left: I64, right: I64): I64;
@operator((-)) fun negate(operand: I64): I64;
@operator((<)) fun lessThan(left: I64, right: I64): Boolean;
@operator((>)) fun greaterThan(left: I64, right: I64): Boolean;
@operator((<=)) fun atMost(left: I64, right: I64): Boolean;
@operator((>=)) fun atLeast(left: I64, right: I64): Boolean;
@operator((==)) fun equal(left: I64, right: I64): Boolean;
@operator((!=)) fun notEqual(left: I64, right: I64): Boolean;

// U8
@operator((+)) fun add(left: U8, right: U8): U8;
@operator((-)) fun subtract(left: U8, right: U8): U8;
@operator((*)) fun multiply(left: U8, right: U8): U8;
@operator((/)) fun divide(left: U8, right: U8): U8;
@operator((<)) fun lessThan(left: U8, right: U8): Boolean;
@operator((>)) fun greaterThan(left: U8, right: U8): Boolean;
@operator((<=)) fun atMost(left: U8, right: U8): Boolean;
@operator((>=)) fun atLeast(left: U8, right: U8): Boolean;
@operator((==)) fun equal(left: U8, right: U8): Boolean;
@operator((!=)) fun notEqual(left: U8, right: U8): Boolean;

// U16
@operator((+)) fun add(left: U16, right: U16): U16;
@operator((-)) fun subtract(left: U16, right: U16): U16;
@operator((*)) fun multiply(left: U16, right: U16): U16;
@operator((/)) fun divide(left: U16, right: U16): U16;
@operator((<)) fun lessThan(left: U16, right: U16): Boolean;
@operator((>)) fun greaterThan(left: U16, right: U16): Boolean;
@operator((<=)) fun atMost(left: U16, right: U16): Boolean;
@operator((>=)) fun atLeast(left: U16, right: U16): Boolean;
@operator((==)) fun equal(left: U16, right: U16): Boolean;
@operator((!=)) fun notEqual(left: U16, right: U16): Boolean;

// U32
@operator((+)) fun add(left: U32, right: U32): U32;
@operator((-)) fun subtract(left: U32, right: U32): U32;
@operator((*)) fun multiply(left: U32, right: U32): U32;
@operator((/)) fun divide(left: U32, right: U32): U32;
@operator((<)) fun lessThan(left: U32, right: U32): Boolean;
@operator((>)) fun greaterThan(left: U32, right: U32): Boolean;
@operator((<=)) fun atMost(left: U32, right: U32): Boolean;
@operator((>=)) fun atLeast(left: U32, right: U32): Boolean;
@operator((==)) fun equal(left: U32, right: U32): Boolean;
@operator((!=)) fun notEqual(left: U32, right: U32): Boolean;

// U64
@operator((+)) fun add(left: U64, right: U64): U64;
@operator((-)) fun subtract(left: U64, right: U64): U64;
@operator((*)) fun multiply(left: U64, right: U64): U64;
@operator((/)) fun divide(left: U64, right: U64): U64;
@operator((<)) fun lessThan(left: U64, right: U64): Boolean;
@operator((>)) fun greaterThan(left: U64, right: U64): Boolean;
@operator((<=)) fun atMost(left: U64, right: U64): Boolean;
@operator((>=)) fun atLeast(left: U64, right: U64): Boolean;
@operator((==)) fun equal(left: U64, right: U64): Boolean;
@operator((!=)) fun notEqual(left: U64, right: U64): Boolean;
