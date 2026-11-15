// fastlsp: a framework for building language servers, in the spirit of FastAPI. An
// endpoint is a function, decorated with the method it answers, whose parameter and
// return types are the protocol's own (`pkg:lsp`); the framework reads JSON, calls it
// with the typed value, and writes what it returns.
//
// What its sources use is imported here, once: they are one module, compiled in this
// file's scope.

import { compile, emit } from "core:project";
import { Object, String, Int32, Int64, Boolean, Double, Byte, List, Dictionary, Func, Nullable } from "core:runtime";
import { Interface } from "std:oop";
import { Class as DotNet_Class, Struct as DotNet_Struct, GenericInstance, public, static, explicitThis } from "pkg:clr/hir";
import { Instance, Annotated, Definition, Type } from "pkg:zsharp/hir";
import { Code, Get, Call, Index, NamedMember, Argument, Expression, ExpressionStatement, Statement, Local, Declare, Return, IfStatement, Set, While, Block, IntegerLiteral, StringLiteral, NullLiteral } from "pkg:zsharp/hir/code";
import { Function, Positional } from "std:functions";
import { Module } from "std:modules";
import { Class, Field } from "std:oop";

import {
    JsonValue, Server, isPresent, memberOf, isEmpty, decodeString, decodeInt, decodeDouble, decodeBoolean,
    decodeNullableInt, decodeNullableDouble, decodeNullableBoolean, decodeAny, newObject, putMember,
    isNothing, encodeString, encodeInt, encodeDouble, encodeBoolean, encodeNullableInt,
    encodeNullableDouble, encodeNullableBoolean, encodeAny, below, next, keysOf, newArray, use
} from "pkg:fastlsp/runtime";

emit(compile("FastLSP"));
