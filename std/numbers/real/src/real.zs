// std:numbers/real — the concept of a real number: Z#'s floating-point types, of a bit
// width, as IEEE 754 has them, and the operators on them.
//
// Each operator is declared with no code, so it is what the platform implements it with:
// the platform says, as std:zsharp/clr does for the CLR. Each type has the arithmetic
// operators, negation and the comparisons, of operands of the type. A comparison gives
// the CLR's Boolean until Z# has its own.

import { Boolean } from "core:runtime";
import { operator } from "core:zsharp";
import { FloatingPoint } from "std:numbers";

// ---- The types

def F16 of FloatingPoint(16) {}
def F32 of FloatingPoint(32) {}
def F64 of FloatingPoint(64) {}

// ---- The operators

// F16
@operator((+)) fun add(left: F16, right: F16): F16;
@operator((-)) fun subtract(left: F16, right: F16): F16;
@operator((*)) fun multiply(left: F16, right: F16): F16;
@operator((/)) fun divide(left: F16, right: F16): F16;
@operator((-)) fun negate(operand: F16): F16;
@operator((<)) fun lessThan(left: F16, right: F16): Boolean;
@operator((>)) fun greaterThan(left: F16, right: F16): Boolean;
@operator((<=)) fun atMost(left: F16, right: F16): Boolean;
@operator((>=)) fun atLeast(left: F16, right: F16): Boolean;
@operator((==)) fun equal(left: F16, right: F16): Boolean;
@operator((!=)) fun notEqual(left: F16, right: F16): Boolean;

// F32
@operator((+)) fun add(left: F32, right: F32): F32;
@operator((-)) fun subtract(left: F32, right: F32): F32;
@operator((*)) fun multiply(left: F32, right: F32): F32;
@operator((/)) fun divide(left: F32, right: F32): F32;
@operator((-)) fun negate(operand: F32): F32;
@operator((<)) fun lessThan(left: F32, right: F32): Boolean;
@operator((>)) fun greaterThan(left: F32, right: F32): Boolean;
@operator((<=)) fun atMost(left: F32, right: F32): Boolean;
@operator((>=)) fun atLeast(left: F32, right: F32): Boolean;
@operator((==)) fun equal(left: F32, right: F32): Boolean;
@operator((!=)) fun notEqual(left: F32, right: F32): Boolean;

// F64
@operator((+)) fun add(left: F64, right: F64): F64;
@operator((-)) fun subtract(left: F64, right: F64): F64;
@operator((*)) fun multiply(left: F64, right: F64): F64;
@operator((/)) fun divide(left: F64, right: F64): F64;
@operator((-)) fun negate(operand: F64): F64;
@operator((<)) fun lessThan(left: F64, right: F64): Boolean;
@operator((>)) fun greaterThan(left: F64, right: F64): Boolean;
@operator((<=)) fun atMost(left: F64, right: F64): Boolean;
@operator((>=)) fun atLeast(left: F64, right: F64): Boolean;
@operator((==)) fun equal(left: F64, right: F64): Boolean;
@operator((!=)) fun notEqual(left: F64, right: F64): Boolean;
