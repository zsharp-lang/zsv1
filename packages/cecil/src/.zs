// The Cecil platform: a CLR platform that writes LIR as a .NET assembly, through
// Mono.Cecil. It is a project of its own, which a build script compiles and loads into
// the running platform, and then gives LIR to.
//
// What its sources use is imported here, once: they are one module, compiled in this
// file's scope. Mono.Cecil is a package, which NuGet installed.

import "std:dotnet";
import { compile, emit } from "core:project";
import {
    Object, String, Boolean, Int16, Int32, Int64, Type, List, Dictionary, DotNet_Class, DotNet_Module,
    explicitThis, static, Path, Directory, File, Environment, Assembly, AppDomain, ClrPlatform
} from "core:runtime";

let objectType: Type = Object;
import { BindingFlags, Convert, Enum, Version, AppContext, Module as ReflectedModule } from objectType.Assembly.Location;

import {
    AssemblyDefinition, AssemblyNameDefinition, ModuleDefinition, ModuleKind,
    TypeDefinition, TypeReference, TypeAttributes, MethodDefinition, MethodReference, MethodAttributes,
    FieldDefinition, FieldReference, FieldAttributes, ParameterDefinition, ParameterAttributes,
    GenericInstanceType, GenericInstanceMethod, GenericParameter, GenericParameterType,
    ArrayType, ByReferenceType, PointerType, RequiredModifierType, OptionalModifierType,
    CallSite, OpCode, OpCodes, ILProcessor, Instruction, VariableDefinition, InterfaceImplementation, CustomAttribute,
    PropertyDefinition, PropertyAttributes, WriterParameters
} from "pkg:Mono.Cecil@0.11.6";

import {
    Module as LIRModule, Function as LIRFunction, Global as LIRGlobal, Structure as LIRStructure, Field as LIRField,
    Type as LIRType, FunctionType as LIRFunctionType, Reference as LIRReference, Signature as LIRSignature,
    Parameter as LIRParameter, Local as LIRLocal, Variable as LIRVariable, ICallable,
    GenericInstance, InstanceMember, MethodInstance,
    Expression as LIRExpression, Statement as LIRStatement, Block as LIRBlock, Evaluate, If as LIRIf, Loop as LIRLoop,
    Break as LIRBreak, Continue as LIRContinue, Return as LIRReturn, SetLocal, SetGlobal, SetField, Store,
    Constant, GetLocal, GetGlobal, GetField, GetFunction, Call as LIRCall, CallIndirect, New as LIRNew,
    Allocate, Load as LIRLoad, Sequence, EntryPoint as LIREntryPoint
} from "std:zsharp/lir";
import {
    ClrEntity, ClrType, ClrArrayType, ClrPointerType, ClrGenericParameter, ClrMethod, ClrField, ClrCode, ClrLocal,
    ByRefType, AddressOf, CastDown, TypeTest, MethodOf, Polymorphic, Reflected,
    ClassDefinition, ClassMethod, ClassConstructor, ClassField, ClassFieldAccess, ClassGenericParameter, ClassProperty, DefinesClass
} from "std:zsharp/lir/clr";
import { Error, Result, IResult } from "std:zsharp/common";

emit(compile("CecilPlatform"));
