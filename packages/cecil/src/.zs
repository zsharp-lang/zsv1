// The Cecil platform: a CLR platform that writes LIR as a .NET assembly, through
// Mono.Cecil. It is a project of its own, which a build script compiles and loads into
// the running platform, and then gives LIR to.
//
// What its sources use is imported here, once: they are one module, compiled in this
// file's scope. Mono.Cecil is a package, which NuGet installed.

import { compile, emit } from "core:project";
import {
    Object, String, Boolean, Int16, Int32, Int64, UInt64, Single, Double, Type, List, Dictionary
} from "core:runtime";
import { Class as DotNet_Class, Module as DotNet_Module } from "pkg:clr/hir";
import { ClrPlatform } from "pkg:clr/lir";
import { Path, Directory, File, Environment, Assembly, AppDomain } from Object.Assembly;
import { static, public, explicitThis } from "pkg:clr/hir";

import { BindingFlags, Convert, Enum, Version, AppContext, Byte, Module as ReflectedModule } from Object.Assembly;
import { Assemblies } from Path.Combine(AppContext.BaseDirectory, "ZSharp.Importer.ILLoader.dll");
// The attributes the IL loader reads, which an assembly it imports carries.
import { EntryPointAttribute, ModuleScopeAttribute, MetadataAttribute, ReflectionBlobAttribute } from Path.Combine(AppContext.BaseDirectory, "ZSharp.Importer.ILLoader.Attributes.dll");

import {
    AssemblyDefinition, AssemblyNameDefinition, ModuleDefinition, ModuleKind,
    TypeDefinition, TypeReference, TypeAttributes, MethodDefinition, MethodReference, MethodAttributes,
    FieldDefinition, FieldReference, FieldAttributes, ParameterDefinition, ParameterAttributes,
    GenericInstanceType, GenericInstanceMethod, GenericParameter, GenericParameterType,
    ArrayType, ByReferenceType, PointerType, RequiredModifierType, OptionalModifierType,
    CallSite, OpCode, OpCodes, ILProcessor, Instruction, VariableDefinition, InterfaceImplementation, CustomAttribute,
    PropertyDefinition, PropertyAttributes, WriterParameters,
    CustomAttributeArgument, ICustomAttributeProvider, EmbeddedResource, ManifestResourceAttributes
} from "pkg:Mono.Cecil@0.11.6";

import {
    Module as LIRModule, Function as LIRFunction, Global as LIRGlobal, Structure as LIRStructure, Field as LIRField,
    Type as LIRType, FunctionType as LIRFunctionType, Reference as LIRReference, Signature as LIRSignature,
    Parameter as LIRParameter, Local as LIRLocal, Variable as LIRVariable, ICallable,
    GenericInstance, InstanceMember, MethodInstance,
    Expression as LIRExpression, Statement as LIRStatement, Block as LIRBlock, Evaluate, If as LIRIf, Loop as LIRLoop, Breakable,
    Break as LIRBreak, Continue as LIRContinue, Return as LIRReturn, SetLocal, SetGlobal, SetField, Store,
    Constant, GetLocal, GetGlobal, GetField, GetFunction, Call as LIRCall, CallIndirect, New as LIRNew,
    Allocate, Load as LIRLoad, Sequence, EntryPoint as LIREntryPoint, KeptAnnotation, Metadata as LIRMetadata
} from "pkg:zsharp/lir";
import {
    ClrEntity, ClrType, ClrArrayType, ClrPointerType, ClrGenericParameter, ClrMethod, ClrField, ClrCode, ClrLocal,
    ByRefType, AddressOf, CastDown, TypeTest, MethodOf, Polymorphic, Reflected,
    ClassDefinition, ClassMethod, ClassConstructor, ClassField, ClassFieldAccess, ClassGenericParameter, ClassProperty, DefinesClass,
    Accessibility as ClrAccessibility, Accessible as ClrAccessible
} from "pkg:clr/lir";
import { Error, Result, IResult } from "pkg:zsharp/core";

emit(compile("CecilPlatform"));
