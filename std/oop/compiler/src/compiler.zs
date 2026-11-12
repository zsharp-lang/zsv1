// std:oop/compiler — what makes std:oop's definitions do what they mean: the COs that
// classes, interfaces, methods and constructors load to, the meta-compilers that build
// their nodes from what is written, and what binds the built ones.
//
// std:oop is their structure, which reflection needs alone; this is what the compiler
// needs of them. Importing it makes `def` a Z# class wherever nothing else compiles one,
// and `def Name of Interface` a Z# interface.

import { String, Object, Boolean, Int32, List, Dictionary, HashSet, DotNet_Interface, explicitThis, static as clrStatic, DotNet_Class, DotNet_Module, ilLoader, Type as ClrType } from "core:runtime";
import { Instance, Definition, Variable, Type, Named as NamedDefinition, Annotated, Overloadable } from "std:zsharp/hir";
import { Code, Block, Local, Expression, ExpressionStatement, Set, StringLiteral, NamedMember, Get, Declare, BlockExpression, Statement, Cast as CastCode } from "std:zsharp/hir/code";
import {
    Function as FunctionSyntax, Definition as DefinitionSyntax, TypeDefinition as TypeSyntax,
    Constructor as ConstructorSyntax, Accessor as AccessorSyntax, AccessorKind, LetExpression as LetSyntax, Expression as ExpressionSyntax,
    Signature as SignatureSyntax, Parameter as ParameterSyntax, Statement as StatementSyntax, DefinitionStatement as DefinitionStatementSyntax,
    DecoratedStatement as DecoratedSyntax, ExpressionStatement as ExpressionStatementSyntax
} from "std:zsharp/lang/ast";
import { CompilerObject } from "std:zsharp/mir";
import { Error, ErrorMessage, Result, IResult } from "std:zsharp/common";
import { Argument, ArgumentStream } from "std:zsharp/compiler/cg";
import { IRTMatch } from "std:zsharp/compiler/matching/static";
import { UnsupportedError } from "std:zsharp/compiler";
import { ICTCallable, ICTBound, ICTGetMember, ICTSet, ICTCastTo, ICTImplicitCastTo, ICTGetIndex, ICTGet } from "std:zsharp/compiler/cg/direct";
import { IRTGetMember, IRTCastTo, IRTGetIndex } from "std:zsharp/compiler/cg/typed";
import { ICompileIRExpression, ICompileIRType, ICompileIRStatement, ICompileIRDefinitionIn, ICompileIRReference } from "std:zsharp/compiler/ir";
import { ITyped, IAssignableTo } from "std:zsharp/compiler/ts";
import { ASTCompiler, CompiledDefinition, CompileASTExpression, CompileASTDefinition, CompileASTStatement, Context } from "std:zsharp/compiler/ast";
import {
    Function as LIRFunction, Parameter as LIRParameter, Local as LIRLocal, Signature, Call as LIRCall,
    GetLocal, SetLocal, Return as LIRReturn, New as LIRNew, GetField, SetField, Constant, Structure,
    Global as LIRGlobal, GetGlobal, SetGlobal,
    Field as LIRField, Module as LIRModule, Expression as LIRExpression, Statement as LIRStatement, Code as LIRCode, Evaluate, Type as LIRType, ICallable
} from "std:zsharp/lir";
import { Bodies, ContainerCompiler, CapabilityContext, Language, Operator, OperatorAnnotation } from "std:zsharp/lang";
import { CodeLoaders } from "std:zsharp/compiler/code";
import { Positional, Named } from "std:functions";
import { Holder, Held, Initializing, keep } from "std:zsharp/lowering";
import { Polymorphism, PlatformInterfaces } from "std:zsharp/capabilities";
import { metaTypes, bodies, language } from "core:zsharp";
import { entrypoint, definitionLoader, instanceLoader, typeLoader, metaType, binder } from "std:framework";
import { cg, ir, ts, hirLoader, hirCodeLoader, mappedLoader, matching, overloading } from "core:compiler";
import { Receiver, Field, Method, Property, Constructor, Class, Interface, ClassOf } from "std:oop";

// ---- The COs

// A field of an object, read and written: the field of the object's structure at its
// index. A class field is a field of the type object. Of a built class, it is the field
// as the type it was built as has it, which what gives the field knows, while its type
// is the field's as it is written.
def FieldValue : CompilerObject, ICompileIRExpression, ICTSet, ITyped {
    let owner: CompilerObject;
    let object: CompilerObject;
    let node: Field;
    let index: Int32;
    let built: CompilerObject;

    // The owner is the type whose structure has the field: the class, or its metaclass.
    new(this, owner: CompilerObject, object: CompilerObject, node: Field, index: Int32, built: CompilerObject) {
        this.owner = owner;
        this.object = object;
        this.node = node;
        this.index = index;
        this.built = built;
    }

    fun get_Type(this): CompilerObject {
        return fieldType(this.node);
    }

    fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        if (this.built != null) return ir.CompileExpression(this.built, target);

        let field = this.field(target);
        if (field.IsError) return Result[LIRExpression].Error(field.UnwrapError());

        let object = ir.CompileExpression(this.object, target);
        if (object.IsError) return object;

        return Result[LIRExpression].Ok(GetField(object.Unwrap(), field.Unwrap()));
    }

    // The LIR field: the one at its index of its owner's structure.
    fun field(this, target: Object): IResult[LIRField, Error] {
        let structure = ir.CompileType(this.owner, target);
        if (structure.IsError) return Result[LIRField].Error(structure.UnwrapError());
        return Result[LIRField].Ok((structure.Unwrap() as Structure).Fields[this.index]);
    }

    fun Set(this, value: CompilerObject): IResult[CompilerObject, Error] {
        if (this.built != null) return cg.Set(this.built, value);

        let converted = cg.ImplicitCast(value, this.get_Type());
        if (converted.IsError) return converted;

        return Result[CompilerObject].Ok(FieldSet(this, converted.Unwrap()));
    }
}

def FieldSet : CompilerObject, ICompileIRStatement {
    let field: FieldValue;
    let value: CompilerObject;

    new(this, field: FieldValue, value: CompilerObject) { this.field = field; this.value = value; }

    fun CompileIRStatement(this, target: Object): IResult[LIRStatement, Error] {
        let field = this.field.field(target);
        if (field.IsError) return Result[LIRStatement].Error(field.UnwrapError());

        let object = ir.CompileExpression(this.field.object, target);
        if (object.IsError) return Result[LIRStatement].Error(object.UnwrapError());

        let value = ir.CompileExpression(this.value, target);
        if (value.IsError) return Result[LIRStatement].Error(value.UnwrapError());

        return Result[LIRStatement].Ok(SetField(object.Unwrap(), field.Unwrap(), value.Unwrap()));
    }
}

// A static field: one variable of the class, a global of its module, which the
// module's initializer gives the field's default value.
def StaticField : CompilerObject, ICompileIRExpression, ICTSet, ITyped {
    let owner: ClassObject;
    let node: Field;
    let globals: Dictionary[Object, LIRGlobal];

    new(this, owner: ClassObject, node: Field) {
        this.owner = owner;
        this.node = node;
        this.globals = Dictionary[Object, LIRGlobal]();
    }

    fun get_Type(this): CompilerObject {
        return fieldType(this.node);
    }

    fun global(this, target: Object): LIRGlobal {
        if (this.globals.ContainsKey(target)) return this.globals[target];
        if (this.owner.structure(target) == null) return null;

        let container = this.owner.modules[target];
        let global = LIRGlobal(ir.CompileType(this.get_Type(), target).Unwrap());
        global.Name = this.owner.node.Name + "." + this.node.Name;
        container.Globals.Add(global);
        this.globals[target] = global;

        Initializing.into(container, SetGlobal(global, defaultValueOf(this.node, target)));
        return global;
    }

    fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        let global = this.global(target);
        if (global == null) return Result[LIRExpression].Error(this.owner.failure(target));
        return Result[LIRExpression].Ok(GetGlobal(global));
    }

    fun Set(this, value: CompilerObject): IResult[CompilerObject, Error] {
        let converted = cg.ImplicitCast(value, this.get_Type());
        if (converted.IsError) return converted;

        return Result[CompilerObject].Ok(StaticFieldSet(this, converted.Unwrap()));
    }
}

def StaticFieldSet : CompilerObject, ICompileIRStatement {
    let field: StaticField;
    let value: CompilerObject;

    new(this, field: StaticField, value: CompilerObject) { this.field = field; this.value = value; }

    fun CompileIRStatement(this, target: Object): IResult[LIRStatement, Error] {
        let global = this.field.global(target);
        if (global == null) return Result[LIRStatement].Error(this.field.owner.failure(target));

        let value = ir.CompileExpression(this.value, target);
        if (value.IsError) return Result[LIRStatement].Error(value.UnwrapError());

        return Result[LIRStatement].Ok(SetGlobal(global, value.Unwrap()));
    }
}

// Matching arguments to parameters, as a function does.
// Arguments with a value first, as a call through an object has it.
fun withFirst(value: CompilerObject, arguments: Argument[]): Argument[] {
    let all = List[Argument]();
    all.Add(Argument(value));
    all.AddRange(arguments);
    return all.ToArray();
}

fun matchArguments(parameters: List[Definition], arguments: Argument[]): List[CompilerObject] {
    let stream = ArgumentStream(arguments);
    let values = List[CompilerObject]();

    let index = 0;
    while (index < parameters.Count) {
        let value = matching.Match(hirLoader.LoadDefinition(parameters[index]).Unwrap(), stream);
        if (value.IsError) return null;
        values.Add(value.Unwrap());
        index = index + 1;
    }

    if (stream.HasArguments) return null;
    return values;
}

fun parameterOf(node: Definition, target: Object): LIRParameter {
    return ir.CompileReference[LIRParameter](hirLoader.LoadDefinition(node).Unwrap(), target).Unwrap();
}

fun parameterType(node: Definition): CompilerObject {
    return ts.TypeOf(hirLoader.LoadDefinition(node).Unwrap()).Unwrap();
}

// The type of a definition's value: a class's metaclass, or an interface's class shape.
fun typeOfValue(definition: CompilerObject): CompilerObject {
    return ts.TypeOf(cg.Get(definition).Unwrap()).Unwrap();
}

// What a method gives, as its node says, or null.
fun returnTypeOf(method: Method): CompilerObject {
    if (method.ReturnType == null) return null;
    return hirLoader.LoadType(method.ReturnType).Unwrap();
}

// Whether a method takes its object, as its first parameter.
fun takesObject(method: Method): Boolean {
    if (method.ThisParameter == null) return false;
    return method.Parameters.Contains(method.ThisParameter);
}

// How many values a method takes, besides its object.
fun valuesTaken(method: Method): Int32 {
    if (takesObject(method)) return method.Parameters.Count - 1;
    return method.Parameters.Count;
}

// Whether two methods take and give the same, besides their objects, as their nodes say.
fun sameSignature(first: Method, second: Method): Boolean {
    if (takesObject(first) != takesObject(second)) return false;
    if (first.Parameters.Count != second.Parameters.Count) return false;
    if (sameType(returnTypeOf(first), returnTypeOf(second)) == false) return false;

    let index = 0;
    if (takesObject(first)) index = 1;
    while (index < first.Parameters.Count) {
        if (sameType(parameterType(first.Parameters[index]), parameterType(second.Parameters[index])) == false) return false;
        index = index + 1;
    }
    return true;
}

// The fields each object of a class has, in order.
fun objectFields(node: Class): List[Field] {
    let fields = List[Field]();
    let index = 0;
    while (index < node.Fields.Count) {
        let field = node.Fields[index];
        if (field.IsStatic == false) {
            if (field.IsClass == false) fields.Add(field);
        }
        index = index + 1;
    }
    return fields;
}

// A field's default value, for a target: its value, as its type, or nothing.
fun defaultValueOf(field: Field, target: Object): LIRExpression {
    let type = ir.CompileType(fieldType(field), target).Unwrap();

    if (field.DefaultValue == null) return Constant(type, null);

    let value = cg.ImplicitCast(hirCodeLoader.LoadExpression(field.DefaultValue).Unwrap(), fieldType(field)).Unwrap();
    return ir.CompileExpression(value, target).Unwrap();
}

fun booleanType(): CompilerObject {
    return hirLoader.LoadType(Boolean as Type).Unwrap();
}

// Whether two types are the same, as each goes to the other.
fun sameType(first: CompilerObject, second: CompilerObject): Boolean {
    if (first == null) return second == null;
    if (second == null) return false;
    if (ts.IsAssignableTo(first, second) == false) return false;
    return ts.IsAssignableTo(second, first);
}

// Lowers code into a function, giving why it can't, or null.
// Lowers the code of a method or a constructor, whose variables are its parameters, and
// its object when that isn't one of them, as what its code reaches it as: each is what
// its code uses it as, while the code is lowered.
fun lowerCode(body: Code, function: LIRFunction, returned: CompilerObject, parameters: List[Definition], receiver: Instance, receiverValue: CompilerObject, target: Object): Error {
    let previous = List[CompilerObject]();
    let index = 0;
    while (index < parameters.Count) {
        previous.Add(CodeLoaders.Define(parameters[index] as Instance, hirLoader.LoadDefinition(parameters[index]).Unwrap()));
        index = index + 1;
    }
    let before: CompilerObject = null;
    if (receiver != null) before = CodeLoaders.Define(receiver, receiverValue);

    let failed = lowerDefined(body, function, returned, target);

    if (receiver != null) CodeLoaders.Redefine(receiver, before);
    index = 0;
    while (index < parameters.Count) {
        CodeLoaders.Redefine(parameters[index] as Instance, previous[index]);
        index = index + 1;
    }
    return failed;
}

fun lowerDefined(body: Code, function: LIRFunction, returned: CompilerObject, target: Object): Error {
    let outer = CodeLoaders.Returns;
    CodeLoaders.Returns = returned;
    let loaded = hirCodeLoader.LoadStatement(body.Block);
    CodeLoaders.Returns = outer;
    if (loaded.IsError) return loaded.UnwrapError();

    let statement = ir.CompileStatement(loaded.Unwrap(), target);
    if (statement.IsError) return statement.UnwrapError();

    function.Body.Block.Statements.Add(statement.Unwrap());
    return null;
}

// A method of a class or an interface: a definition of its own, which what holds it, its
// class or interface, lowers into its module. Its function is made when it is asked for,
// before its code is lowered, so code can call it, and a method of an object is a method
// of its object's structure. A method of an interface has no code: calling it calls what
// the object's class has.
def MethodObject : CompilerObject, ICTCallable, ICompileIRReference[ICallable], Held, ICompileIRDefinitionIn[LIRModule] {
    let node: Method;
    let holder: Holder;
    let lowered: Dictionary[Object, LIRFunction];
    let failures: Dictionary[Object, Error];

    new(this, node: Method) {
        this.node = node;
        this.lowered = Dictionary[Object, LIRFunction]();
        this.failures = Dictionary[Object, Error]();
    }

    fun hold(this, holder: Holder) { this.holder = holder; }

    fun returnType(this): CompilerObject { return returnTypeOf(this.node); }

    // Its function takes the object first, then its parameters.
    fun function(this, target: Object): LIRFunction {
        if (this.lowered.ContainsKey(target)) return this.lowered[target];

        // The object's type is what it is a method of, which can lower what made it first.
        let receiver: LIRType = null;
        if (takesObject(this.node)) {
            let type = ir.CompileType(parameterType(this.node.ThisParameter), target);
            if (type.IsError) {
                this.failures[target] = type.UnwrapError();
                return null;
            }
            receiver = type.Unwrap();
            if (this.lowered.ContainsKey(target)) return this.lowered[target];
        }

        if (this.holder == null) {
            this.failures[target] = ErrorMessage("The method " + this.node.Name + " isn't held by a class or an interface.");
            return null;
        }
        let container = this.holder.module(target);
        if (this.lowered.ContainsKey(target)) return this.lowered[target];
        if (container == null) {
            this.failures[target] = ErrorMessage("The method " + this.node.Name + " can't be lowered for a target without polymorphism.");
            return null;
        }

        let signature = Signature();
        let returned = this.returnType();
        if (returned != null) signature.ReturnType = ir.CompileType(returned, target).Unwrap();

        // Its object is its first parameter, unless it is static, which is a function that
        // takes none.
        let index = 0;
        while (index < this.node.Parameters.Count) {
            signature.Parameters.Add(parameterOf(this.node.Parameters[index], target));
            index = index + 1;
        }

        // Its types can lower what calls it, which made its function first.
        if (this.lowered.ContainsKey(target)) return this.lowered[target];

        let function = LIRFunction(signature);
        function.Name = this.node.Name;
        if (this.node.Body != null) function.Body = LIRCode();
        keep(this.node, function.Metadata);
        this.lowered[target] = function;
        container.Functions.Add(function);

        if (receiver != null) {
            let polymorphism = target as Polymorphism;
            polymorphism.method(function, receiver as Structure);
        }

        // Its code is lowered once it is known, so its code can call it.
        if (this.node.Body != null) {
            let failed = lowerCode(this.node.Body, function, returned, this.node.Parameters, this.unparameterized(), this.classOfStatic(), target);
            if (failed != null) this.failures[target] = failed;
        }

        return function;
    }

    // Its object, when it isn't one of its parameters, as a static method's isn't.
    fun unparameterized(this): Instance {
        if (this.node.ThisParameter == null) return null;
        if (takesObject(this.node)) return null;
        return this.node.ThisParameter as Instance;
    }

    // What a static method's code reaches its class's members through: the class, with no
    // object of it.
    fun classOfStatic(this): CompilerObject {
        let receiver = this.unparameterized();
        if (receiver == null) return null;
        return hirLoader.LoadType((receiver as Positional).Type).Unwrap();
    }

    fun CompileIRDefinition(this, owner: LIRModule, target: Object): Error {
        let called = this.callable(target);
        if (called.IsError) return called.UnwrapError();
        return null;
    }

    // Calling it calls its function, with its object first, as its first parameter, unless
    // it is static.
    fun Call(this, arguments: Argument[]): IResult[CompilerObject, Error] {
        let values = matchArguments(this.node.Parameters, arguments);
        if (values == null) return Result[CompilerObject].Error(this.node.Name + " can't be called with these arguments.");
        return Result[CompilerObject].Ok(Invocation(this, this.returnType(), values));
    }

    // It is referred to as its function, which takes the object first.
    fun CompileIRReference(this, target: Object): IResult[ICallable, Error] { return this.callable(target); }

    // What calling it calls: the method it was built as, if it was, or its function.
    fun callable(this, target: Object): IResult[ICallable, Error] {
        if (builtMethods.ContainsKey(this.node)) return builtCallable(builtMethods[this.node], target);

        let function = this.function(target);
        if (this.failures.ContainsKey(target)) return Result[ICallable].Error(this.failures[target]);
        return Result[ICallable].Ok(function);
    }
}

// A constructor of a class: a definition of its own, which its class holds and lowers
// into its module. Its function makes an object of its class's structure, with each of
// its fields' default values, as its class's HIR has them, runs its code on it, and
// gives it.
def ConstructorObject : CompilerObject, ICTCallable, ICompileIRReference[ICallable], Held, ICompileIRDefinitionIn[LIRModule] {
    let node: Constructor;
    let holder: Holder;
    let object: Constructed;
    let lowered: Dictionary[Object, LIRFunction];
    let failures: Dictionary[Object, Error];

    new(this, node: Constructor) {
        this.node = node;
        this.lowered = Dictionary[Object, LIRFunction]();
        this.failures = Dictionary[Object, Error]();
    }

    fun hold(this, holder: Holder) { this.holder = holder; }

    // The class it makes an object of.
    fun made(this): Class { return this.node.This.Type as Class; }

    // The object it makes, as its code uses it.
    fun receiver(this): Constructed {
        if (this.object == null) this.object = Constructed(hirLoader.LoadDefinition(this.made()).Unwrap());
        return this.object;
    }

    fun function(this, target: Object): LIRFunction {
        if (this.lowered.ContainsKey(target)) return this.lowered[target];

        let type = ir.CompileType(hirLoader.LoadDefinition(this.made()).Unwrap(), target);
        if (type.IsError) {
            this.failures[target] = type.UnwrapError();
            return null;
        }
        if (this.lowered.ContainsKey(target)) return this.lowered[target];
        let structure = type.Unwrap() as Structure;

        if (this.holder == null) {
            this.failures[target] = ErrorMessage("A constructor of " + this.made().Name + " isn't held by its class.");
            return null;
        }
        let container = this.holder.module(target);
        if (this.lowered.ContainsKey(target)) return this.lowered[target];

        let signature = Signature();
        signature.ReturnType = structure;

        let index = 0;
        while (index < this.node.Parameters.Count) {
            signature.Parameters.Add(parameterOf(this.node.Parameters[index], target));
            index = index + 1;
        }

        // Its types can lower what calls it, which made its function first.
        if (this.lowered.ContainsKey(target)) return this.lowered[target];

        let function = LIRFunction(signature);
        function.Name = this.made().Name + "." + this.node.Name;
        function.Body = LIRCode();
        keep(this.node, function.Metadata);
        this.lowered[target] = function;
        container.Functions.Add(function);

        let receiver = ir.CompileReference[LIRLocal](this.receiver(), target).Unwrap();
        function.Body.Block.Locals.Add(receiver);

        let fields = List[LIRExpression]();
        let declared = objectFields(this.made());
        let field = 0;
        while (field < declared.Count) {
            fields.Add(defaultValueOf(declared[field], target));
            field = field + 1;
        }

        function.Body.Block.Statements.Add(SetLocal(receiver, LIRNew(structure, fields)));

        if (this.node.Body != null) {
            let failed = lowerCode(this.node.Body, function, null, this.node.Parameters, this.node.This, this.receiver(), target);
            if (failed != null) {
                this.failures[target] = failed;
                return function;
            }
        }

        function.Body.Block.Statements.Add(LIRReturn(GetLocal(receiver)));
        return function;
    }

    fun CompileIRDefinition(this, owner: LIRModule, target: Object): Error {
        let called = this.callable(target);
        if (called.IsError) return called.UnwrapError();
        return null;
    }

    fun CompileIRReference(this, target: Object): IResult[ICallable, Error] { return this.callable(target); }

    fun callable(this, target: Object): IResult[ICallable, Error] {
        if (builtConstructors.ContainsKey(this.node)) return builtCallable(builtConstructors[this.node], target);

        let function = this.function(target);
        if (this.failures.ContainsKey(target)) return Result[ICallable].Error(this.failures[target]);
        return Result[ICallable].Ok(function);
    }

    fun Call(this, arguments: Argument[]): IResult[CompilerObject, Error] {
        let values = matchArguments(this.node.Parameters, arguments);
        if (values == null) return Result[CompilerObject].Error("A constructor of " + this.made().Name + " can't be called with these arguments.");

        return Result[CompilerObject].Ok(Invocation(this, hirLoader.LoadDefinition(this.made()).Unwrap(), values));
    }
}

// The object a constructor makes, as the constructor's code uses it: a local of its
// function for each target, of the class it makes, which the constructor defines its
// receiver as.
def Constructed : CompilerObject, ICompileIRExpression, ICompileIRReference[LIRLocal], ITyped {
    let type: CompilerObject;
    let locals: Dictionary[Object, LIRLocal];

    new(this, type: CompilerObject) {
        this.type = type;
        this.locals = Dictionary[Object, LIRLocal]();
    }

    fun get_Type(this): CompilerObject { return this.type; }

    // It is referred to as its local.
    fun CompileIRReference(this, target: Object): IResult[LIRLocal, Error] {
        if (this.locals.ContainsKey(target)) return Result[LIRLocal].Ok(this.locals[target]);

        let type = ir.CompileType(this.type, target);
        if (type.IsError) return Result[LIRLocal].Error(type.UnwrapError());

        // Its type can lower what asks for it, which made it first.
        if (this.locals.ContainsKey(target)) return Result[LIRLocal].Ok(this.locals[target]);

        let local = LIRLocal(type.Unwrap());
        local.Name = "this";
        this.locals[target] = local;
        return Result[LIRLocal].Ok(local);
    }

    fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        let local = this.CompileIRReference(target);
        if (local.IsError) return Result[LIRExpression].Error(local.UnwrapError());
        return Result[LIRExpression].Ok(GetLocal(local.Unwrap()));
    }
}

// A call of what is referred to as a function, with its values: what it is referred to
// as for the target is called. It knows nothing else of what it calls.
def Invocation : CompilerObject, ICompileIRExpression, ITyped {
    let callee: CompilerObject;
    let type: CompilerObject;
    let arguments: List[CompilerObject];

    new(this, callee: CompilerObject, type: CompilerObject, arguments: List[CompilerObject]) {
        this.callee = callee;
        this.type = type;
        this.arguments = arguments;
    }

    fun get_Type(this): CompilerObject { return this.type; }

    fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        let called = ir.CompileReference[ICallable](this.callee, target);
        if (called.IsError) return Result[LIRExpression].Error(called.UnwrapError());

        let values = List[LIRExpression]();
        let index = 0;
        while (index < this.arguments.Count) {
            let value = ir.CompileExpression(this.arguments[index], target);
            if (value.IsError) return value;
            values.Add(value.Unwrap());
            index = index + 1;
        }

        return Result[LIRExpression].Ok(LIRCall(called.Unwrap(), values));
    }
}

// What is reached through an object, as a method is: calling it calls the callee with
// the object first, and it is referred to as the callee, so a delegate of it is closed
// over the object. Bound to nothing, as a static method is, it is the callee. It knows
// nothing of what the callee is.
def Bound : CompilerObject, ICTCallable, ICTBound, ICompileIRReference[ICallable] {
    let callee: CompilerObject;
    let object: CompilerObject;

    new(this, callee: CompilerObject, object: CompilerObject) { this.callee = callee; this.object = object; }

    fun Call(this, arguments: Argument[]): IResult[CompilerObject, Error] {
        if (this.object == null) return cg.Call(this.callee, arguments);
        return cg.Call(this.callee, withFirst(this.object, arguments));
    }

    fun CompileIRReference(this, target: Object): IResult[ICallable, Error] {
        return ir.CompileReference[ICallable](this.callee, target);
    }

    fun get_Receiver(this): CompilerObject { return this.object; }
}

// A class's metaclass: the type of its type object, which is one object for each
// target, a global of the class's module. Its fields are the class fields, and its
// methods the class methods. It implements the class shape of each interface its
// class implements.
def Metaclass : CompilerObject, ICompileIRType, ICTCastTo, ICTImplicitCastTo, IRTGetMember[String], IAssignableTo {
    let owner: ClassObject;
    let node: ClassOf;
    let structures: Dictionary[Object, Structure];
    let instances: Dictionary[Object, LIRGlobal];

    new(this, owner: ClassObject) {
        this.owner = owner;
        this.node = ClassOf(owner.node);
        this.structures = Dictionary[Object, Structure]();
        this.instances = Dictionary[Object, LIRGlobal]();
    }

    fun Cast(this, type: CompilerObject): IResult[CompilerObject, Error] {
        return description(this.node, hirLoader.LoadType(ClassOf as Type).Unwrap(), type);
    }

    fun ImplicitCast(this, type: CompilerObject): IResult[CompilerObject, Error] {
        return describedImplicitly(this.node, hirLoader.LoadType(ClassOf as Type).Unwrap(), type);
    }

    fun failure(this, target: Object): String { return this.owner.failure(target); }

    fun fields(this): List[Field] {
        let fields = List[Field]();
        let index = 0;
        while (index < this.owner.node.Fields.Count) {
            if (this.owner.node.Fields[index].IsClass) fields.Add(this.owner.node.Fields[index]);
            index = index + 1;
        }
        return fields;
    }

    // A metaclass goes to itself, one CO for each class, and to the class shape of each
    // interface its class implements, which is the type of the interface's value.
    fun IsAssignableTo(this, target: CompilerObject): Boolean {
        if (target == this) return true;

        let index = 0;
        while (index < this.owner.node.Interfaces.Count) {
            if (this.owner.node.Interfaces[index] is shape of Interface) {
                if (ts.IsAssignableTo(target, typeOfValue(this.owner.base(index)))) return true;
            }
            index = index + 1;
        }
        return false;
    }

    fun structure(this, target: Object): Structure {
        if (this.structures.ContainsKey(target)) return this.structures[target];
        if (this.owner.structure(target) == null) return null;

        let polymorphism = target as Polymorphism;
        let structure = Structure();
        structure.Name = this.owner.node.Name + ".Class";
        polymorphism.objects(structure);
        this.owner.modules[target].Structures.Add(structure);
        this.structures[target] = structure;
        beginStructure(target);

        let fields = this.fields();
        let index = 0;
        while (index < fields.Count) {
            let field = LIRField(ir.CompileType(fieldType(fields[index]), target).Unwrap());
            field.Name = fields[index].Name;
            structure.Fields.Add(field);
            index = index + 1;
        }

        index = 0;
        while (index < this.owner.node.Interfaces.Count) {
            if (this.owner.node.Interfaces[index] is implemented of Interface) {
                let shape = ir.CompileType(typeOfValue(this.owner.base(index)), target);
                if (shape.IsOk) polymorphism.implements(structure, shape.Unwrap() as Structure);
            }
            index = index + 1;
        }

        endStructure(target);
        return structure;
    }

    // Its class methods are declared with its class.
    fun lower(this, target: Object): Boolean { return this.owner.lower(target); }

    fun CompileIRType(this, target: Object): IResult[LIRType, Error] {
        if (builtClasses.ContainsKey(this.owner.node)) return ir.CompileType(builtMetaclass(builtClasses[this.owner.node], this.owner.node.Name), target);
        if (structuring(target)) {
            if (this.owner.structure(target) == null) return Result[LIRType].Error(this.failure(target));
            deferLowering(this.owner, target);
            return Result[LIRType].Ok(this.structure(target));
        }
        if (this.lower(target) == false) return Result[LIRType].Error(this.failure(target));
        return Result[LIRType].Ok(this.structure(target));
    }

    // The type object: a global of the class's module, which the module's initializer
    // makes, with its class fields' default values.
    fun instance(this, target: Object): LIRGlobal {
        if (this.instances.ContainsKey(target)) return this.instances[target];

        let structure = this.structure(target);

        let defaults = List[LIRExpression]();
        let fields = this.fields();
        let index = 0;
        while (index < fields.Count) {
            defaults.Add(defaultValueOf(fields[index], target));
            index = index + 1;
        }

        let container = this.owner.modules[target];
        let global = LIRGlobal(structure);
        global.Name = structure.Name;
        container.Globals.Add(global);
        this.instances[target] = global;

        Initializing.into(container, SetGlobal(global, LIRNew(structure, defaults)));
        return global;
    }

    // The type object's members are the class fields and methods, and the class's
    // static ones.
    fun Member(this, object: CompilerObject, name: String): IResult[CompilerObject, Error] {
        let fields = this.fields();
        let index = 0;
        while (index < fields.Count) {
            if (fields[index].Name == name) {
                // A built class's type object has the field as the type it was built as.
                let built: CompilerObject = null;
                if (builtClasses.ContainsKey(this.owner.node)) built = cg.Member(Retyped(object, builtMetaclass(builtClasses[this.owner.node], this.owner.node.Name)), name).Unwrap();
                return Result[CompilerObject].Ok(FieldValue(this, object, fields[index], index, built));
            }
            index = index + 1;
        }

        let property = findProperty(this.owner.node.Properties, name);
        if (property != null) {
            let accessor = accessorOf(property);
            if (accessor.IsClass) return Result[CompilerObject].Ok(propertyValue(property, object));
            if (accessor.IsStatic) return Result[CompilerObject].Ok(propertyValue(property, null));
            return Result[CompilerObject].Error("There is no object here: property " + name + " is the objects' of the class " + this.owner.node.Name + ", and a class method runs on the class.");
        }

        let methods = List[CompilerObject]();
        let objects = false;
        index = 0;
        while (index < this.owner.node.Methods.Count) {
            let method = this.owner.node.Methods[index];
            if (method.Name == name) {
                if (method.IsClass) methods.Add(Bound(this.owner.method(index), object));
                else if (method.IsStatic) methods.Add(Bound(this.owner.method(index), null));
                else objects = true;
            }
            index = index + 1;
        }
        if (methods.Count > 0) return overloading.Group(methods.ToArray());
        if (objects) return Result[CompilerObject].Error("There is no object here: method " + name + " is the objects' of the class " + this.owner.node.Name + ", and a class method runs on the class.");

        index = 0;
        while (index < this.owner.node.Fields.Count) {
            let field = this.owner.node.Fields[index];
            if (field.Name == name) {
                if (field.IsStatic) return Result[CompilerObject].Ok(this.owner.staticField(field));
                return Result[CompilerObject].Error("There is no object here: field " + name + " is the objects' of the class " + this.owner.node.Name + ", and a class method runs on the class.");
            }
            index = index + 1;
        }

        return Result[CompilerObject].Error("The class " + this.owner.node.Name + " has no class member named " + name + ".");
    }
}

// An interface's class shape: the type of the type object of any class that
// implements the interface, whose methods are the interface's class methods.
def InterfaceMetaclass : CompilerObject, ICompileIRType, ICTCastTo, ICTImplicitCastTo, IRTGetMember[String], IAssignableTo {
    let owner: InterfaceObject;
    let node: ClassOf;
    let structures: Dictionary[Object, Structure];

    new(this, owner: InterfaceObject) {
        this.owner = owner;
        this.node = ClassOf(owner.node);
        this.structures = Dictionary[Object, Structure]();
    }

    fun Cast(this, type: CompilerObject): IResult[CompilerObject, Error] {
        return description(this.node, hirLoader.LoadType(ClassOf as Type).Unwrap(), type);
    }

    fun ImplicitCast(this, type: CompilerObject): IResult[CompilerObject, Error] {
        return describedImplicitly(this.node, hirLoader.LoadType(ClassOf as Type).Unwrap(), type);
    }

    fun failure(this, target: Object): String { return this.owner.failure(target); }

    // A class shape goes only to itself, one CO for each interface.
    fun IsAssignableTo(this, target: CompilerObject): Boolean { return target == this; }

    fun structure(this, target: Object): Structure {
        if (this.structures.ContainsKey(target)) return this.structures[target];
        if (this.owner.structure(target) == null) return null;

        let structure = Structure();
        structure.Name = this.owner.node.Name + ".Class";
        (target as Polymorphism).interfaces(structure);
        this.owner.modules[target].Structures.Add(structure);
        this.structures[target] = structure;
        return structure;
    }

    fun lower(this, target: Object): Boolean { return this.owner.lower(target); }

    fun CompileIRType(this, target: Object): IResult[LIRType, Error] {
        if (this.lower(target) == false) return Result[LIRType].Error(this.failure(target));
        return Result[LIRType].Ok(this.structure(target));
    }

    fun Member(this, object: CompilerObject, name: String): IResult[CompilerObject, Error] {
        let methods = List[CompilerObject]();
        let index = 0;
        while (index < this.owner.node.Methods.Count) {
            let method = this.owner.node.Methods[index];
            if (method.Name == name) {
                if (method.IsClass) methods.Add(Bound(this.owner.method(index), object));
            }
            index = index + 1;
        }
        if (methods.Count > 0) return overloading.Group(methods.ToArray());

        return Result[CompilerObject].Error("The interface " + this.owner.node.Name + " has no class method named " + name + ".");
    }
}

// A property of an object, or of a class or its type object: reading it calls its
// getter, and writing it its setter, as calling them through the object does.
def PropertyValue : CompilerObject, ICompileIRExpression, ICTSet, ITyped {
    let property: Property;
    let object: CompilerObject;

    new(this, property: Property, object: CompilerObject) {
        this.property = property;
        this.object = object;
    }

    fun get_Type(this): CompilerObject {
        if (this.property.Getter != null) return returnTypeOf(this.property.Getter);
        let setter = this.property.Setter;
        return parameterType(setter.Parameters[setter.Parameters.Count - 1]);
    }

    fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        if (this.property.Getter == null) return Result[LIRExpression].Error("Property " + this.property.Name + " can't be read.");

        let read = Bound(hirLoader.LoadDefinition(this.property.Getter).Unwrap(), this.object).Call([]);
        if (read.IsError) return Result[LIRExpression].Error(read.UnwrapError());
        return ir.CompileExpression(read.Unwrap(), target);
    }

    fun Set(this, value: CompilerObject): IResult[CompilerObject, Error] {
        if (this.property.Setter == null) return Result[CompilerObject].Error("Property " + this.property.Name + " can't be written.");

        let written = Bound(hirLoader.LoadDefinition(this.property.Setter).Unwrap(), this.object).Call([Argument(value)]);
        if (written.IsError) return written;
        return Result[CompilerObject].Ok(Evaluated(written.Unwrap()));
    }
}

// A call evaluated as a statement, for what it does.
def Evaluated : CompilerObject, ICompileIRStatement {
    let call: CompilerObject;

    new(this, call: CompilerObject) { this.call = call; }

    fun CompileIRStatement(this, target: Object): IResult[LIRStatement, Error] {
        let call = ir.CompileExpression(this.call, target);
        if (call.IsError) return Result[LIRStatement].Error(call.UnwrapError());
        return Result[LIRStatement].Ok(Evaluate(call.Unwrap()));
    }
}

// Whether a method is an operator, as `@operator` says of it.
fun isOperator(method: Method, which: Object): Boolean {
    let index = 0;
    while (index < method.Annotations.Count) {
        if (method.Annotations[index] is said of OperatorAnnotation) {
            if (which.Equals(said.Operator)) return true;
        }
        index = index + 1;
    }
    return false;
}

// What indexing a class or one of its values calls: its methods that are its index
// operator, the class's for the class and the objects' for a value, called with what
// is indexed as a call of them is.
fun callIndex(methods: List[Method], onClass: Boolean, object: CompilerObject, arguments: Argument[], owner: String): IResult[CompilerObject, Error] {
    let candidates = List[CompilerObject]();
    let index = 0;
    while (index < methods.Count) {
        let method = methods[index];
        if (isOperator(method, Operator.Index)) {
            if (method.IsClass == onClass) {
                if (method.IsStatic == false) candidates.Add(Bound(hirLoader.LoadDefinition(method).Unwrap(), object));
            }
        }
        index = index + 1;
    }

    if (candidates.Count == 0) return Result[CompilerObject].Error(owner + " has no index operator.");
    let group = overloading.Group(candidates.ToArray());
    if (group.IsError) return group;
    return cg.Call(group.Unwrap(), arguments);
}

// A property of a class or an interface, by its name, reached through an object or a
// class: its accessors' COs, which are its methods'.
fun findProperty(properties: List[Property], name: String): Property {
    let index = 0;
    while (index < properties.Count) {
        if (properties[index].Name == name) return properties[index];
        index = index + 1;
    }
    return null;
}

fun propertyValue(property: Property, object: CompilerObject): PropertyValue {
    return PropertyValue(property, object);
}

// Whether its accessors are the class's or its type object's, as the getter is, or
// the setter if it has none.
fun accessorOf(property: Property): Method {
    if (property.Getter != null) return property.Getter;
    return property.Setter;
}

// The definitions a class or an interface holds, reached by the names `Named` gives
// them.
fun ownedMember(definitions: List[Definition], name: String, owner: String): IResult[CompilerObject, Error] {
    let index = 0;
    while (index < definitions.Count) {
        if (definitions[index] is named of NamedDefinition) {
            if (named.Name == name) return hirLoader.LoadDefinition(definitions[index]);
        }
        index = index + 1;
    }
    return Result[CompilerObject].Error(owner + " has no member named " + name + ".");
}

// A field's type: the one written, or else its default value's.
fun fieldType(field: Field): CompilerObject {
    if (field.Type != null) return hirLoader.LoadType(field.Type).Unwrap();
    return ts.TypeOf(hirCodeLoader.LoadExpression(field.DefaultValue).Unwrap()).Unwrap();
}

// A class's value: its type object, of its metaclass.
def TypeObject : CompilerObject, ICompileIRExpression, ITyped {
    let owner: ClassObject;

    new(this, owner: ClassObject) { this.owner = owner; }

    fun get_Type(this): CompilerObject { return this.owner.meta; }

    fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        return this.owner.CompileIRExpression(target);
    }
}

// An interface's value, of its class shape: what any class that implements it is.
// Nothing lowers it to code, as an interface has no type object of its own.
def InterfaceValue : CompilerObject, ITyped {
    let owner: InterfaceObject;

    new(this, owner: InterfaceObject) { this.owner = owner; }

    fun get_Type(this): CompilerObject { return this.owner.meta; }
}

// A value that exists as the code is compiled, as the node a definition is.
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

// What describes a definition is its node: casting the definition to what describes
// it, which is reflection, gives the node. Where an HIR type is expected, as in a
// signature, the definition goes there as its node without a cast.
fun description(node: Object, described: CompilerObject, type: CompilerObject): IResult[CompilerObject, Error] {
    if (ts.IsAssignableTo(type, described)) return Result[CompilerObject].Ok(Known(node, described));
    return Result[CompilerObject].Error("A definition is only described by its node.");
}

fun describedImplicitly(node: Object, described: CompilerObject, type: CompilerObject): IResult[CompilerObject, Error] {
    if (ts.IsAssignableTo(hirLoader.LoadType(Type as Type).Unwrap(), type)) return description(node, described, type);
    return Result[CompilerObject].Error(UnsupportedError("A definition goes as its node only where a type is described."));
}

// A value of an interface cast down to a class or another interface, or tested for
// being of it. The object knows what it is, so the platform's polymorphism casts and
// tests it, through a function with no code that the capability marks.
def Narrowing : CompilerObject, ICompileIRExpression, ITyped {
    let value: CompilerObject;
    let origin: CompilerObject;
    let to: CompilerObject;
    let test: Boolean;

    new(this, value: CompilerObject, origin: CompilerObject, to: CompilerObject, test: Boolean) {
        this.value = value;
        this.origin = origin;
        this.to = to;
        this.test = test;
    }

    fun get_Type(this): CompilerObject {
        if (this.test) return booleanType();
        return this.to;
    }

    fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        if (target is polymorphism of Polymorphism) {
            let object = ir.CompileExpression(this.value, target);
            if (object.IsError) return object;

            let to = ir.CompileType(this.to, target);
            if (to.IsError) return Result[LIRExpression].Error(to.UnwrapError());

            let source = ir.CompileType(this.origin, target);
            if (source.IsError) return Result[LIRExpression].Error(source.UnwrapError());

            let signature = Signature();
            signature.Parameters.Add(LIRParameter(source.Unwrap()));
            let function = LIRFunction(signature);

            if (this.test) {
                signature.ReturnType = ir.CompileType(booleanType(), target).Unwrap();
                polymorphism.test(function, to.Unwrap() as Structure);
            } else {
                signature.ReturnType = to.Unwrap();
                polymorphism.cast(function, to.Unwrap() as Structure);
            }

            return Result[LIRExpression].Ok(LIRCall(function, [object.Unwrap()]));
        }

        return Result[LIRExpression].Error("Only a target with polymorphism knows what an object is.");
    }
}

// A Z# interface. As a type, it is the object type of any class that implements it,
// which its target's polymorphism makes. A value of it has its methods as members, and
// is cast down to, and tested for, what implements it.
def InterfaceObject : CompilerObject, Holder, ICompileIRType, ICTCastTo, ICTImplicitCastTo, ICTGetMember[String], IRTGetMember[String], IRTGetIndex, ICTGet, IRTCastTo, IRTMatch, IAssignableTo, Held, ICompileIRDefinitionIn[LIRModule] {
    let node: Interface;
    let holder: Holder;
    let structures: Dictionary[Object, Structure];
    let modules: Dictionary[Object, LIRModule];
    let lowerings: Dictionary[Object, Boolean];
    let meta: InterfaceMetaclass;

    new(this, node: Interface) {
        this.node = node;
        this.structures = Dictionary[Object, Structure]();
        this.modules = Dictionary[Object, LIRModule]();
        this.lowerings = Dictionary[Object, Boolean]();
        this.meta = InterfaceMetaclass(this);
    }

    fun hold(this, holder: Holder) { this.holder = holder; }

    fun CompileIRDefinition(this, owner: LIRModule, target: Object): Error {
        if (this.lower(target)) return null;
        return ErrorMessage(this.failure(target));
    }

    fun container(this, target: Object): LIRModule {
        if (this.holder != null) return this.holder.module(target);
        return LIRModule(this.node.Name);
    }

    // Indexing a value of it calls its methods that are its index operator, which are
    // what the value's class has.
    fun Index(this, object: CompilerObject, arguments: Argument[]): IResult[CompilerObject, Error] {
        return callIndex(this.node.Methods, false, object, arguments, "The interface " + this.node.Name);
    }

    // What it holds is reached by its name.
    fun Member(this, name: String): IResult[CompilerObject, Error] {
        return ownedMember(this.node.Definitions, name, "The interface " + this.node.Name);
    }

    // Its value is of its class shape, the type of any class that implements it, so
    // `Type[Shape]` is that; evaluating it is what fails, as nothing lowers it.
    fun Get(this): IResult[CompilerObject, Error] { return Result[CompilerObject].Ok(InterfaceValue(this)); }

    fun failure(this, target: Object): String {
        return "The interface " + this.node.Name + " can't be lowered for a target without polymorphism.";
    }

    fun Cast(this, type: CompilerObject): IResult[CompilerObject, Error] {
        return description(this.node, hirLoader.LoadType(Interface as Type).Unwrap(), type);
    }

    fun ImplicitCast(this, type: CompilerObject): IResult[CompilerObject, Error] {
        return describedImplicitly(this.node, hirLoader.LoadType(Interface as Type).Unwrap(), type);
    }

    // An interface goes only to itself: one definition is one CO.
    fun IsAssignableTo(this, target: CompilerObject): Boolean { return target == this; }

    fun structure(this, target: Object): Structure {
        if (this.structures.ContainsKey(target)) return this.structures[target];

        if (target is polymorphism of Polymorphism) {
            let container = this.container(target);
            if (this.structures.ContainsKey(target)) return this.structures[target];

            let structure = Structure();
            structure.Name = this.node.Name;
            polymorphism.interfaces(structure);
            container.Structures.Add(structure);
            this.modules[target] = container;
            this.structures[target] = structure;
            return structure;
        }

        return null;
    }

    // Its methods are declared, with no code.
    fun lower(this, target: Object): Boolean {
        if (this.lowerings.ContainsKey(target)) return this.lowerings[target];

        let structure = this.structure(target);
        if (structure == null) return false;
        if (this.lowerings.ContainsKey(target)) return this.lowerings[target];

        this.lowerings[target] = true;
        let container = this.modules[target];

        // Its methods lower into its module, with no code.
        let index = 0;
        while (index < this.node.Methods.Count) {
            let failed = ir.CompileDefinition[LIRModule](this.method(index), container, target);
            if (failed != null) {
                this.lowerings[target] = false;
                return false;
            }
            index = index + 1;
        }

        return true;
    }

    fun method(this, index: Int32): CompilerObject {
        return hirLoader.LoadDefinition(this.node.Methods[index]).Unwrap();
    }

    // What it holds lowers into its module, which it lowers with them.
    fun module(this, target: Object): LIRModule {
        if (this.lower(target) == false) return null;
        if (this.modules.ContainsKey(target)) return this.modules[target];
        return null;
    }

    // Its methods are held by it.
    fun holdMembers(this) {
        let index = 0;
        while (index < this.node.Methods.Count) {
            if (this.method(index) is held of Held) held.hold(this);
            index = index + 1;
        }
    }

    fun CompileIRType(this, target: Object): IResult[LIRType, Error] {
        if (this.lower(target) == false) return Result[LIRType].Error(this.failure(target));
        return Result[LIRType].Ok(this.structure(target));
    }

    fun Member(this, object: CompilerObject, name: String): IResult[CompilerObject, Error] {
        let property = findProperty(this.node.Properties, name);
        if (property != null) {
            if (accessorOf(property).IsClass == false) return Result[CompilerObject].Ok(propertyValue(property, object));
        }

        let methods = List[CompilerObject]();
        let index = 0;
        while (index < this.node.Methods.Count) {
            let method = this.node.Methods[index];
            if (method.Name == name) {
                if (method.IsClass == false) methods.Add(Bound(this.method(index), object));
            }
            index = index + 1;
        }
        if (methods.Count > 0) return overloading.Group(methods.ToArray());

        return Result[CompilerObject].Error("The interface " + this.node.Name + " has no member named " + name + ".");
    }

    // What a value of it might be: a class that implements it, or another interface.
    fun narrows(this, type: CompilerObject): Boolean {
        if (type is other of InterfaceObject) return true;
        return ts.IsAssignableTo(this, type);
    }

    fun Cast(this, value: CompilerObject, type: CompilerObject): IResult[CompilerObject, Error] {
        if (this.narrows(type)) return Result[CompilerObject].Ok(Narrowing(value, this, type, false));
        return Result[CompilerObject].Error("A value of the interface " + this.node.Name + " can only be cast to what implements it.");
    }

    fun Match(this, value: CompilerObject, type: CompilerObject): IResult[CompilerObject, Error] {
        if (this.narrows(type) == false) return Result[CompilerObject].Error("A value of the interface " + this.node.Name + " can only be matched against what implements it.");
        return Result[CompilerObject].Ok(Narrowing(value, this, type, true));
    }
}

// A Z# class. As a type, it is the object type its target lowers it to; calling it
// runs its unnamed constructor, and its named constructors are its members. A value
// of it has its fields and methods as members.
def ClassObject : CompilerObject, Holder, ICompileIRType, ICTCallable, ICTCastTo, ICTImplicitCastTo, ICTGetMember[String], IRTGetMember[String], ICTGetIndex, IRTGetIndex, ICTGet, IAssignableTo, ICompileIRExpression, ITyped, Held, ICompileIRDefinitionIn[LIRModule] {
    let node: Class;
    let meta: Metaclass;
    let holder: Holder;
    let statics: Dictionary[Field, StaticField];
    let structures: Dictionary[Object, Structure];
    let modules: Dictionary[Object, LIRModule];
    // Whether the class lowers for each target, which it is taken to while it does,
    // since its functions are declared before any of their code is lowered.
    let lowerings: Dictionary[Object, Boolean];
    let failures: Dictionary[Object, String];
    // The targets its structure is being built for: it exists, so a field's type can
    // be the class, but it has none of its fields yet.
    let building: HashSet[Object];

    new(this, node: Class) {
        this.node = node;
        this.building = HashSet[Object]();
        this.structures = Dictionary[Object, Structure]();
        this.modules = Dictionary[Object, LIRModule]();
        this.lowerings = Dictionary[Object, Boolean]();
        this.failures = Dictionary[Object, String]();
        this.meta = Metaclass(this);
        this.statics = Dictionary[Field, StaticField]();
    }

    fun hold(this, holder: Holder) { this.holder = holder; }

    fun CompileIRDefinition(this, owner: LIRModule, target: Object): Error {
        if (this.lower(target)) return null;
        return ErrorMessage(this.failure(target));
    }

    // Indexing the class calls its class methods that are its index operator, on the
    // class, and indexing one of its objects the methods that are, on the object.
    fun Index(this, arguments: Argument[]): IResult[CompilerObject, Error] {
        return callIndex(this.node.Methods, true, this, arguments, "The class " + this.node.Name);
    }

    fun Index(this, object: CompilerObject, arguments: Argument[]): IResult[CompilerObject, Error] {
        return callIndex(this.node.Methods, false, object, arguments, "The class " + this.node.Name);
    }

    // The LIR module it lowers into: what holds it lowers to, or one of its own.
    fun container(this, target: Object): LIRModule {
        if (this.holder != null) return this.holder.module(target);
        return LIRModule(this.node.Name);
    }

    // The class's value is its type object.
    fun Get(this): IResult[CompilerObject, Error] { return Result[CompilerObject].Ok(TypeObject(this)); }

    // A class used as a value is its type object, as long as what gives a definition
    // gives its CO, rather than its value.
    fun get_Type(this): CompilerObject { return this.meta; }

    fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        if (builtClasses.ContainsKey(this.node)) return ir.CompileExpression(builtTypeObject(builtClasses[this.node], this.node.Name), target);
        if (this.lower(target) == false) return Result[LIRExpression].Error(this.failure(target));
        return Result[LIRExpression].Ok(GetGlobal(this.meta.instance(target)));
    }

    // The fields each object has, in order.
    fun fields(this): List[Field] { return objectFields(this.node); }

    fun staticField(this, node: Field): StaticField {
        if (this.statics.ContainsKey(node) == false) this.statics[node] = StaticField(this, node);
        return this.statics[node];
    }

    fun failure(this, target: Object): String {
        if (this.failures.ContainsKey(target)) return this.failures[target];
        return "The class " + this.node.Name + " can't be lowered for a target without polymorphism.";
    }

    fun Cast(this, type: CompilerObject): IResult[CompilerObject, Error] {
        return description(this.node, hirLoader.LoadType(Class as Type).Unwrap(), type);
    }

    fun ImplicitCast(this, type: CompilerObject): IResult[CompilerObject, Error] {
        return describedImplicitly(this.node, hirLoader.LoadType(Class as Type).Unwrap(), type);
    }

    // A class goes to itself, one CO for each definition, and to what any of its bases
    // goes to: a Z# interface it implements, or a type of the platform's own.
    fun IsAssignableTo(this, target: CompilerObject): Boolean {
        if (target == this) return true;

        let index = 0;
        while (index < this.node.Interfaces.Count) {
            if (ts.IsAssignableTo(target, this.base(index))) return true;
            index = index + 1;
        }
        return false;
    }

    // A base of the class, as its CO: a Z# interface, or a type of a platform's own, as a
    // CLR interface, which the target implements as it can.
    fun base(this, index: Int32): CompilerObject {
        return hirLoader.LoadType(this.node.Interfaces[index]).Unwrap();
    }

    // Whether it has the method an interface requires: one of its name and kind, which
    // takes and gives what the required one does.
    fun hasMethod(this, required: Method): Boolean {
        let index = 0;
        while (index < this.node.Methods.Count) {
            let found = this.node.Methods[index];
            if (found.Name == required.Name) {
                if (found.IsClass == required.IsClass) {
                    if (sameSignature(found, required)) return true;
                }
            }
            index = index + 1;
        }
        return false;
    }

    // The object type: a structure the target's polymorphism makes the type of
    // objects, with its fields, that implements its interfaces.
    fun structure(this, target: Object): Structure {
        if (this.structures.ContainsKey(target)) return this.structures[target];

        if (target is polymorphism of Polymorphism) {
            let container = this.container(target);
            if (this.structures.ContainsKey(target)) return this.structures[target];

            let structure = Structure();
            structure.Name = this.node.Name;
            keep(this.node, structure.Metadata);
            polymorphism.objects(structure);
            container.Structures.Add(structure);
            this.modules[target] = container;
            this.structures[target] = structure;
            this.building.Add(target);
            beginStructure(target);

            let fields = this.fields();
            let index = 0;
            while (index < fields.Count) {
                let field = LIRField(ir.CompileType(fieldType(fields[index]), target).Unwrap());
                field.Name = fields[index].Name;
                structure.Fields.Add(field);
                index = index + 1;
            }

            this.building.Remove(target);

            // A Z# interface is a structure the target's polymorphism implements, and a
            // platform's own type is implemented as the platform implements it.
            index = 0;
            while (index < this.node.Interfaces.Count) {
                if (this.node.Interfaces[index] is shape of Interface) {
                    let implemented = ir.CompileType(this.base(index), target);
                    if (implemented.IsOk) polymorphism.implements(structure, implemented.Unwrap() as Structure);
                } else {
                    if (target is platform of PlatformInterfaces) {
                        let type = ir.CompileType(this.base(index), target);
                        if (type.IsOk) platform.implements(structure, type.Unwrap());
                    }
                }
                index = index + 1;
            }

            endStructure(target);
            return structure;
        }

        return null;
    }

    // The class's functions, each declared before any code is lowered, so code can
    // call any of them, and each marked as what it is.
    fun lower(this, target: Object): Boolean {
        // A class that was built is what it was built as, and isn't lowered again.
        if (builtClasses.ContainsKey(this.node)) return true;
        if (this.lowerings.ContainsKey(target)) return this.lowerings[target];

        let structure = this.structure(target);
        if (structure == null) return false;
        if (this.lowerings.ContainsKey(target)) return this.lowerings[target];

        this.lowerings[target] = true;
        let container = this.modules[target];
        let polymorphism = target as Polymorphism;

        // It has each method of each interface it implements. A platform's own type it
        // implements is the platform's to check, as it realizes the class, and a target
        // can't have the class if it can't implement it.
        let index = 0;
        while (index < this.node.Interfaces.Count) {
            if (this.node.Interfaces[index] is shape of Interface) {
                let implemented = ir.CompileType(this.base(index), target);
                if (implemented.IsError) return this.failed(target, implemented.UnwrapError().ToString());

                let method = 0;
                while (method < shape.Methods.Count) {
                    let required = shape.Methods[method];
                    if (this.hasMethod(required) == false) return this.failed(target, "The class " + this.node.Name + " doesn't have every method of the interfaces it implements: " + shape.Name + "." + required.Name + " isn't one of its methods.");
                    method = method + 1;
                }
            } else {
                if ((target is platform of PlatformInterfaces) == false) return this.failed(target, "The class " + this.node.Name + " implements a type of a platform's own, which this target can't implement.");
                let type = ir.CompileType(this.base(index), target);
                if (type.IsError) return this.failed(target, "The class " + this.node.Name + " implements what can't be lowered for this target: " + type.UnwrapError());
            }
            index = index + 1;
        }

        // Its methods and constructors lower into its module, as a module's definitions
        // do into it.
        index = 0;
        while (index < this.node.Methods.Count) {
            let failed = ir.CompileDefinition[LIRModule](this.method(index), container, target);
            if (failed != null) return this.failed(target, failed.ToString());
            index = index + 1;
        }

        index = 0;
        while (index < this.node.Constructors.Count) {
            let failed = ir.CompileDefinition[LIRModule](this.constructor(index), container, target);
            if (failed != null) return this.failed(target, failed.ToString());
            index = index + 1;
        }

        // Its static fields and its type object are globals of its module, which are
        // initialized with it.
        index = 0;
        while (index < this.node.Fields.Count) {
            if (this.node.Fields[index].IsStatic) this.staticField(this.node.Fields[index]).global(target);
            index = index + 1;
        }
        this.meta.instance(target);

        return true;
    }

    fun failed(this, target: Object, why: String): Boolean {
        this.lowerings[target] = false;
        this.failures[target] = why;
        return false;
    }

    fun method(this, index: Int32): CompilerObject {
        return hirLoader.LoadDefinition(this.node.Methods[index]).Unwrap();
    }

    fun constructor(this, index: Int32): CompilerObject {
        return hirLoader.LoadDefinition(this.node.Constructors[index]).Unwrap();
    }

    // What it holds lowers into its module, which it lowers with them.
    fun module(this, target: Object): LIRModule {
        if (this.lower(target) == false) return null;
        if (this.modules.ContainsKey(target)) return this.modules[target];
        return null;
    }

    // Its methods and constructors are held by it, as a module's definitions are.
    fun holdMembers(this) {
        let index = 0;
        while (index < this.node.Methods.Count) {
            if (this.method(index) is held of Held) held.hold(this);
            index = index + 1;
        }

        index = 0;
        while (index < this.node.Constructors.Count) {
            if (this.constructor(index) is held of Held) held.hold(this);
            index = index + 1;
        }
    }

    // A class used as a type is lowered with it, so whatever has its objects has its
    // methods.
    // While its structure is being built, as for a field of a type that has the class
    // in it, the class is only its structure: what builds it lowers it once it can.
    fun CompileIRType(this, target: Object): IResult[LIRType, Error] {
        if (builtClasses.ContainsKey(this.node)) return ir.CompileType(builtType(builtClasses[this.node]), target);
        let structure = this.structure(target);
        if (structure == null) return Result[LIRType].Error(this.failure(target));
        if (this.building.Contains(target)) return Result[LIRType].Ok(structure);
        if (structuring(target)) {
            deferLowering(this, target);
            return Result[LIRType].Ok(structure);
        }
        if (this.lower(target) == false) return Result[LIRType].Error(this.failure(target));
        return Result[LIRType].Ok(structure);
    }

    fun named(this, name: String): CompilerObject {
        let index = 0;
        while (index < this.node.Constructors.Count) {
            if (this.node.Constructors[index].Name == name) return this.constructor(index);
            index = index + 1;
        }
        return null;
    }

    fun Call(this, arguments: Argument[]): IResult[CompilerObject, Error] {
        let constructor = this.named("");
        if (constructor == null) return Result[CompilerObject].Error("The class " + this.node.Name + " has no unnamed constructor.");
        return cg.Call(constructor, arguments);
    }

    // The class's members are its named constructors, its static fields and methods,
    // and its class fields and methods, reached through its type object. No name is
    // the class's own: the type of its type object is `Type[Name]`.
    fun Member(this, name: String): IResult[CompilerObject, Error] {

        let constructor = this.named(name);
        if (constructor != null) return Result[CompilerObject].Ok(constructor);

        let property = findProperty(this.node.Properties, name);
        if (property != null) {
            let accessor = accessorOf(property);
            if (accessor.IsStatic) return Result[CompilerObject].Ok(propertyValue(property, null));
            if (accessor.IsClass) return Result[CompilerObject].Ok(propertyValue(property, this));
            return Result[CompilerObject].Error("Property " + name + " of the class " + this.node.Name + " is its objects'.");
        }

        let index = 0;
        while (index < this.node.Fields.Count) {
            let field = this.node.Fields[index];
            if (field.Name == name) {
                if (field.IsStatic) return Result[CompilerObject].Ok(this.staticField(field));
                if (field.IsClass) return this.meta.Member(this, name);
                return Result[CompilerObject].Error("Field " + name + " of the class " + this.node.Name + " is its objects'.");
            }
            index = index + 1;
        }

        let methods = List[CompilerObject]();
        let objects = false;
        index = 0;
        while (index < this.node.Methods.Count) {
            let method = this.node.Methods[index];
            if (method.Name == name) {
                if (method.IsStatic) methods.Add(Bound(this.method(index), null));
                else if (method.IsClass) methods.Add(Bound(this.method(index), this));
                else objects = true;
            }
            index = index + 1;
        }
        if (methods.Count > 0) return overloading.Group(methods.ToArray());
        if (objects) return Result[CompilerObject].Error("Method " + name + " of the class " + this.node.Name + " is its objects'.");

        return ownedMember(this.node.Definitions, name, "The class " + this.node.Name);
    }

    // A value's members are its fields and methods, and the class's static and class
    // ones, which are the same whatever object reaches them: with no inheritance, a
    // value of a class is of that class, so its type object is the class's.
    fun Member(this, object: CompilerObject, name: String): IResult[CompilerObject, Error] {
        let fields = this.fields();
        let index = 0;
        while (index < fields.Count) {
            if (fields[index].Name == name) {
                // A built class's objects have the field as the type they were built as.
                let built: CompilerObject = null;
                if (builtClasses.ContainsKey(this.node)) built = cg.Member(Retyped(object, builtType(builtClasses[this.node])), name).Unwrap();
                return Result[CompilerObject].Ok(FieldValue(this, object, fields[index], index, built));
            }
            index = index + 1;
        }

        let property = findProperty(this.node.Properties, name);
        if (property != null) {
            if (accessorOf(property).IsClass) return Result[CompilerObject].Ok(propertyValue(property, this));
            return Result[CompilerObject].Ok(propertyValue(property, object));
        }

        index = 0;
        while (index < this.node.Fields.Count) {
            let field = this.node.Fields[index];
            if (field.Name == name) {
                if (field.IsClass) return this.meta.Member(this, name);
                return Result[CompilerObject].Ok(this.staticField(field));
            }
            index = index + 1;
        }

        let methods = List[CompilerObject]();
        index = 0;
        while (index < this.node.Methods.Count) {
            let method = this.node.Methods[index];
            if (method.Name == name) {
                if (method.IsClass) methods.Add(Bound(this.method(index), this));
                else methods.Add(Bound(this.method(index), object));
            }
            index = index + 1;
        }
        if (methods.Count > 0) return overloading.Group(methods.ToArray());

        return Result[CompilerObject].Error("The class " + this.node.Name + " has no member named " + name + ".");
    }
}

// ---- Structures being built

// How many structures are being built for each target, as the types of their fields are
// lowered. While one is, a class used as a type is only its structure: its code may use
// what has no fields yet, as a class's methods use the class whose field has its type. It
// is lowered once no structure is being built, and the classes that waited are, in order.
let beingBuilt: Dictionary[Object, Int32] = Dictionary[Object, Int32]();
let deferred: Dictionary[Object, List[CompilerObject]] = Dictionary[Object, List[CompilerObject]]();

fun structuring(target: Object): Boolean {
    if (beingBuilt.ContainsKey(target) == false) return false;
    return beingBuilt[target] > 0;
}

fun beginStructure(target: Object) {
    if (beingBuilt.ContainsKey(target) == false) beingBuilt[target] = 0;
    beingBuilt[target] = beingBuilt[target] + 1;
}

fun endStructure(target: Object) {
    beingBuilt[target] = beingBuilt[target] - 1;
    if (beingBuilt[target] == 0) {
        if (deferred.ContainsKey(target)) {
            let waiting = deferred[target];
            deferred.Remove(target);
            let index = 0;
            while (index < waiting.Count) {
                ir.CompileType(waiting[index], target);
                index = index + 1;
            }
        }
    }
}

fun deferLowering(owner: CompilerObject, target: Object) {
    if (deferred.ContainsKey(target) == false) deferred[target] = List[CompilerObject]();
    deferred[target].Add(owner);
}

// ---- What was built

// The classes, methods and constructors that were built, as what each was built as, which
// their COs are instead of lowering them again.
let builtClasses: Dictionary[Class, ClrType] = Dictionary[Class, ClrType]();
let builtMethods: Dictionary[Method, MethodInfo] = Dictionary[Method, MethodInfo]();
let builtConstructors: Dictionary[Constructor, MethodInfo] = Dictionary[Constructor, MethodInfo]();

@binder
fun bindClass(node: Class, entity: Object): Error {
    builtClasses[node] = entity as ClrType;
    return null;
}

@binder
fun bindMethod(node: Method, entity: Object): Error {
    builtMethods[node] = entity as MethodInfo;
    return null;
}

@binder
fun bindConstructor(node: Constructor, entity: Object): Error {
    builtConstructors[node] = entity as MethodInfo;
    return null;
}

// A value seen as one of another type, whose members are that type's: an object of a
// built class as one of the CLR type it was built as.
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

// How a built class's parts are found from the type its objects were built as: its
// metaclass is the type of its name and `.Class`, and its type object the static field of
// that type, of its module.
fun builtType(built: ClrType): CompilerObject { return hirLoader.LoadType(ilLoader.Load(built)).Unwrap(); }

fun builtCallable(method: MethodInfo, target: Object): IResult[ICallable, Error] {
    let loaded = hirLoader.LoadInstance(ilLoader.Load(method));
    if (loaded.IsError) return Result[ICallable].Error(loaded.UnwrapError());
    return ir.CompileReference[ICallable](loaded.Unwrap(), target);
}

fun builtTypeObjectField(built: ClrType, name: String): FieldInfo {
    let types = built.Assembly.GetTypes();
    let metaclass: ClrType = null;
    let index = 0;
    while (index < types.Length) {
        if (types[index].FullName == name + ".Class") metaclass = types[index];
        index = index + 1;
    }

    index = 0;
    while (index < types.Length) {
        let fields = types[index].GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        let each = 0;
        while (each < fields.Length) {
            if (fields[each].FieldType == metaclass) return fields[each];
            each = each + 1;
        }
        index = index + 1;
    }
    let missing: IResult[FieldInfo, Error] = Result[FieldInfo].Error("The type object of the built class " + name + " isn't in its assembly.");
    return missing.Unwrap();
}

fun builtTypeObject(built: ClrType, name: String): CompilerObject {
    let field = builtTypeObjectField(built, name);
    return cg.Member(builtType(field.DeclaringType), field.Name).Unwrap();
}

fun builtMetaclass(built: ClrType, name: String): CompilerObject {
    return builtType(builtTypeObjectField(built, name).FieldType);
}


// ---- The meta-compilers

// What the members of a class or an interface are compiled with: a let is a field, and a
// fun or a new written directly in it is a method or a constructor. An interface has only
// methods, with no code. A definition inside a member is the member's to compile.
def Members {
    let self: Local;
    let isClass: Boolean;
    // The names of the class's members, which its code reaches by name.
    let names: List[String];
    let fork: ASTCompiler;
    let expressions: CompileASTExpression;
    let definitions: CompileASTDefinition;
    let owner: Context;

    let statements: CompileASTStatement;

    new(this, self: Local, isClass: Boolean, compiler: ASTCompiler) {
        this.self = self;
        this.isClass = isClass;
        this.names = List[String]();
        this.fork = compiler.Fork();
        this.expressions = this.fork.Expression;
        this.definitions = this.fork.Definition;
        this.statements = this.fork.Statement;
        this.owner = Bodies.DefinitionOf(compiler.Contexts.Current);
    }

    fun expression(this, compiler: ASTCompiler, node: ExpressionSyntax): IResult[Expression, Error] {
        if (node is local of LetSyntax) return this.field(compiler, local, List[Expression]());
        return this.expressions(compiler, node);
    }

    // A field can be decorated, as a member defined with `fun` can: its decorators are
    // given the field as it is built, before it is added.
    fun statement(this, compiler: ASTCompiler, node: StatementSyntax): IResult[Statement, Error] {
        if (node is decorated of DecoratedSyntax) {
            let decorators: List[ExpressionSyntax] = null;
            let inner = Language.Undecorate(decorated, decorators);

            if (inner is expression of ExpressionStatementSyntax) {
                if (expression.Expression is local of LetSyntax) {
                    let compiled = Language.CompileDecorators(compiler, decorators);
                    if (compiled.IsError) return Result[Statement].Error(compiled.UnwrapError());

                    let field = this.field(compiler, local, compiled.Unwrap());
                    if (field.IsError) return Result[Statement].Error(field.UnwrapError());
                    return Result[Statement].Ok(ExpressionStatement(field.Unwrap()));
                }
            }
        }

        return this.statements(compiler, node);
    }

    fun definition(this, compiler: ASTCompiler, node: DefinitionSyntax): IResult[CompiledDefinition, Error] {
        // What a class defines its members to be: a fun is a method, an accessor a method
        // of its property, and a new a constructor of a class. Anything else is compiled as
        // it would be anywhere, and held.
        if (Bodies.DefinitionOf(compiler.Contexts.Current) == this.owner) {
            if (node is accessor of AccessorSyntax) {
                if (this.names.Contains(accessor.Name) == false) this.names.Add(accessor.Name);
                return metaTypes.Compile(compiler, node, Method as Instance);
            }
            if (node is function of FunctionSyntax) {
                if (function.Name != null) {
                    if (function.Name.Length > 0) {
                        if (this.names.Contains(function.Name) == false) this.names.Add(function.Name);
                    }
                }
                return metaTypes.Compile(compiler, node, Method as Instance);
            }
            if (node is constructor of ConstructorSyntax) {
                if (this.isClass) return metaTypes.Compile(compiler, node, Constructor as Instance);
            }
        }
        return this.definitions(compiler, node);
    }

    // A field: code that builds it and adds it to the class. Its default value is code.
    fun field(this, compiler: ASTCompiler, local: LetSyntax, decorators: List[Expression]): IResult[Expression, Error] {
        let field = Local(local.Name, Field as Type);
        field.Init = Bodies.Construct(Field as Instance, StringLiteral(local.Name));

        if (local.Type != null) {
            let type = compiler.CompileExpression(local.Type);
            if (type.IsError) return type;
            field.Init = Bodies.Construct(Field as Instance, StringLiteral(local.Name), type.Unwrap());
        }

        let block = BlockExpression();
        block.Statements.Add(Declare(field));

        if (local.Value != null) {
            let built = Block([]);
            let value = bodies.Reified(compiler, built, local.Value);
            if (value.IsError) return value;

            let index = 0;
            while (index < built.Statements.Count) {
                block.Statements.Add(built.Statements[index]);
                index = index + 1;
            }

            let set = Set();
            set.Target = Bodies.Member(field, "DefaultValue");
            set.Value = value.Unwrap();
            block.Statements.Add(ExpressionStatement(set));
        }

        block.Statements.Add(ExpressionStatement(Bodies.Add(this.self, "Fields", kindOf(Language.Decorate(Get(field), decorators), Field as Instance))));
        block.Result = Get(field);
        this.names.Add(local.Name);

        return Result[Expression].Ok(block);
    }

    // A member is added as what it is, whatever its decorators gave: a method or a
    // constructor to its own list, and anything else to the definitions, which takes what
    // is a definition.
    fun register(this, member: CompiledDefinition, value: Expression): Statement {
        let list = "Definitions";
        if (member.UnderlyingType == Method as Instance) list = "Methods";
        if (member.UnderlyingType == Constructor as Instance) list = "Constructors";
        return ExpressionStatement(Bodies.Add(this.self, list, kindOf(value, member.UnderlyingType)));
    }

}

// What a member is added as: what its decorators gave, as the kind of definition it is.
fun kindOf(value: Expression, type: Instance): Expression {
    let cast = CastCode();
    cast.Value = value;
    cast.Type = Get(type);
    return cast;
}

// What a method or constructor's code is compiled with: the object it runs on, which is
// `this` if its first parameter says so, and each of the class's members by its name,
// unless a parameter has the name. A name is bound through the object, as `this.name`
// would be, whatever the member is: what the binding means is the member's to say when
// the code is loaded, once the class is complete. An object's member is reached through
// it, binding a static member does nothing, and a class member is reached through the
// class's type object.
fun bindReceiver(compiler: ASTCompiler, block: Block, self: Local, members: Members, signature: SignatureSyntax, slot: String, kind: Type): SignatureSyntax {
    let receiver = Local("this", kind);
    receiver.Init = Bodies.Member(self, slot);
    block.Statements.Add(Declare(receiver));

    // It is of what it is the object of: the class, or the interface.
    let typed = Set();
    typed.Target = Bodies.Member(receiver, "Type");
    typed.Value = Get(members.self);
    block.Statements.Add(ExpressionStatement(typed));

    let names = List[String]();
    let rest = SignatureSyntax();
    rest.KwArgs = signature.KwArgs;
    rest.Args = List[ParameterSyntax]();

    let index = 0;
    while (index < countOf(signature.Args)) {
        let parameter = signature.Args[index];
        if (index == 0) {
            if (parameter.Name == "this") {
                if (parameter.Type == null) {
                    language.Bind(compiler, "this", Get(receiver));
                    index = index + 1;
                    continue;
                }
            }
        }
        rest.Args.Add(parameter);
        names.Add(parameter.Name);
        index = index + 1;
    }

    index = 0;
    while (index < countOf(signature.KwArgs)) {
        names.Add(signature.KwArgs[index].Name);
        index = index + 1;
    }

    index = 0;
    while (index < members.names.Count) {
        let name = members.names[index];
        if (names.Contains(name) == false) {
            let member = NamedMember();
            member.Target = Get(receiver);
            member.Name = name;
            language.Bind(compiler, name, member);
        }
        index = index + 1;
    }

    return rest;
}

fun countOf(parameters: List[ParameterSyntax]): Int32 {
    if (parameters == null) return 0;
    return parameters.Count;
}

def MethodFiller {
    let compiler: ASTCompiler;
    let function: FunctionSyntax;
    let self: Local;
    let members: Members;

    new(this, compiler: ASTCompiler, function: FunctionSyntax, self: Local, members: Members) {
        this.compiler = compiler;
        this.function = function;
        this.self = self;
        this.members = members;
    }

    fun fill(this): IResult[Block, Error] {
        let block = Block([]);
        let signature = bindReceiver(this.compiler, block, this.self, this.members, this.function.Signature, "ThisParameter", Positional as Type);

        let invalid = bodies.Parameters(this.compiler, block, this.self, "Parameters", signature, Positional as Instance, Named as Instance);
        if (invalid != null) return Result[Block].Error(invalid);

        if (this.function.ReturnType != null) {
            let type = this.compiler.CompileExpression(this.function.ReturnType);
            if (type.IsError) return Result[Block].Error(type.UnwrapError());

            let set = Set();
            set.Target = Bodies.Member(this.self, "ReturnType");
            set.Value = type.Unwrap();
            block.Statements.Add(ExpressionStatement(set));
        }

        if (this.function.Body == null) return Result[Block].Ok(block);

        let failed = bodies.Body(bodies.WithLocals(this.compiler, block, this.self, "Locals", null), block, this.self, this.function.Body);
        if (failed != null) return Result[Block].Error(failed);

        return Result[Block].Ok(block);
    }
}

def ConstructorFiller {
    let compiler: ASTCompiler;
    let constructor: ConstructorSyntax;
    let self: Local;
    let members: Members;

    new(this, compiler: ASTCompiler, constructor: ConstructorSyntax, self: Local, members: Members) {
        this.compiler = compiler;
        this.constructor = constructor;
        this.self = self;
        this.members = members;
    }

    fun fill(this): IResult[Block, Error] {
        let block = Block([]);
        let signature = bindReceiver(this.compiler, block, this.self, this.members, this.constructor.Signature, "This", Receiver as Type);

        let invalid = bodies.Parameters(this.compiler, block, this.self, "Parameters", signature, Positional as Instance, Named as Instance);
        if (invalid != null) return Result[Block].Error(invalid);

        if (this.constructor.Body != null) {
            let failed = bodies.Body(bodies.WithLocals(this.compiler, block, this.self, "Locals", null), block, this.self, this.constructor.Body);
            if (failed != null) return Result[Block].Error(failed);
        }

        return Result[Block].Ok(block);
    }
}

// A class's or an interface's members are compiled within a context that is its members,
// which is how the meta-compiler of a method or constructor in it finds them. What a
// class is given as its bases are the interfaces it implements.
def ClassFiller {
    let compiler: ASTCompiler;
    let definition: TypeSyntax;
    let self: Local;
    let isClass: Boolean;
    let members: Members;

    new(this, compiler: ASTCompiler, definition: TypeSyntax, self: Local, isClass: Boolean) {
        this.compiler = compiler;
        this.definition = definition;
        this.self = self;
        this.isClass = isClass;
    }

    fun fill(this): IResult[Block, Error] {
        let block = Block([]);

        if (this.definition.Bases != null) {

            let index = 0;
            while (index < this.definition.Bases.Count) {
                let implemented = this.compiler.CompileExpression(this.definition.Bases[index]);
                if (implemented.IsError) return Result[Block].Error(implemented.UnwrapError());
                block.Statements.Add(ExpressionStatement(Bodies.Add(this.self, "Interfaces", implemented.Unwrap())));
                index = index + 1;
            }
        }

        if (this.definition.Content == null) return Result[Block].Ok(block);

        this.members = Members(this.self, this.isClass, this.compiler);
        this.members.fork.Expression = this.members.expression;
        this.members.fork.Definition = this.members.definition;
        this.members.fork.Statement = this.members.statement;

        let filled = this.compiler.Contexts.Within[IResult[Block, Error]](CapabilityContext.Of[Members](this.compiler.Contexts.Current, this.members), this.compileMembers);
        if (filled.IsError) return filled;

        block.Statements.Add(filled.Unwrap());

        // A property is defined once its accessors are, as they are what it is.
        let defined = List[String]();
        let index = 0;
        while (index < this.definition.Content.Statements.Count) {
            let decorators: List[ExpressionSyntax] = null;
            let statement = Language.Undecorate(this.definition.Content.Statements[index], decorators);
            if (statement is written of DefinitionStatementSyntax) {
                if (written.Definition is accessor of AccessorSyntax) {
                    if (defined.Contains(accessor.Name) == false) {
                        defined.Add(accessor.Name);
                        let callee = NamedMember();
                        callee.Target = Get(this.self);
                        callee.Name = "defineProperty";
                        block.Statements.Add(ExpressionStatement(Bodies.Call(callee, StringLiteral(accessor.Name))));
                    }
                }
            }
            index = index + 1;
        }

        return Result[Block].Ok(block);
    }

    fun compileMembers(this): IResult[Block, Error] {
        return ContainerCompiler(language).Compile(this.members.fork, this.definition.Content.Statements, this.members.register);
    }
}

// The members of the class being compiled, from the context they are compiled in.
fun members(compiler: ASTCompiler): Members {
    let members: Members = null;
    compiler.Contexts.Has[Members](members);
    return members;
}

@metaType(Class)
fun compileClass(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
    if (definition is type of TypeSyntax) {
        let filler = ClassFiller(compiler, type, self, true);
        return Result[CompiledDefinition].Ok(CompiledDefinition(self, Bodies.Construct(Class as Instance, StringLiteral(type.Name)), Class as Instance, filler.fill, null));
    }
    return Result[CompiledDefinition].Error("A class is a type definition.");
}

@metaType(Interface)
fun compileInterface(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
    if (definition is type of TypeSyntax) {
        let filler = ClassFiller(compiler, type, self, false);
        return Result[CompiledDefinition].Ok(CompiledDefinition(self, Bodies.Construct(Interface as Instance, StringLiteral(type.Name)), Interface as Instance, filler.fill, null));
    }
    return Result[CompiledDefinition].Error("An interface is a type definition.");
}

@metaType(Method)
fun compileMethod(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
    if (definition is function of FunctionSyntax) {
        // An accessor is the method of its property that reads or writes it.
        let name = function.Name;
        if (name == null) name = "";
        if (definition is accessor of AccessorSyntax) {
            name = "get " + accessor.Name;
            if (accessor.Kind == AccessorKind.Set) name = "set " + accessor.Name;
        }

        let filler = MethodFiller(compiler, function, self, members(compiler));
        return Result[CompiledDefinition].Ok(CompiledDefinition(self, Bodies.Construct(Method as Instance, StringLiteral(name)), Method as Instance, filler.fill, null));
    }
    return Result[CompiledDefinition].Error("A method is a fun.");
}

@metaType(Constructor)
fun compileConstructor(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
    if (definition is constructor of ConstructorSyntax) {
        let name = constructor.Name;
        if (name == null) name = "";

        let filler = ConstructorFiller(compiler, constructor, self, members(compiler));
        return Result[CompiledDefinition].Ok(CompiledDefinition(self, Bodies.Construct(Constructor as Instance, StringLiteral(name)), Constructor as Instance, filler.fill, null));
    }
    return Result[CompiledDefinition].Error("A constructor is a new.");
}

// A class is what it means once it is loaded, when its node is complete: each of its
// methods has code, unless it was built, and each of its fields has a type, or a value to
// have the type of.
@definitionLoader
fun loadClass(node: Class): IResult[CompilerObject, Error] {
    let index = 0;
    while (index < node.Methods.Count) {
        if (node.Methods[index].Body == null) {
            if (builtClasses.ContainsKey(node) == false) return Result[CompilerObject].Error("Method " + node.Methods[index].Name + " of the class " + node.Name + " needs code.");
        }
        index = index + 1;
    }

    index = 0;
    while (index < node.Fields.Count) {
        let field = node.Fields[index];
        if (field.Type == null) {
            if (field.DefaultValue == null) return Result[CompilerObject].Error("Field " + field.Name + " of the class " + node.Name + " needs a type or a value.");
        }
        index = index + 1;
    }

    let invalid = invalidProperty(node.Properties, "the class " + node.Name);
    if (invalid != null) return Result[CompilerObject].Error(invalid);

    let loaded = ClassObject(node);
    loaded.holdMembers();
    return Result[CompilerObject].Ok(loaded);
}

// A getter takes nothing and gives the property's value, and a setter takes its value
// and gives nothing, both of the objects or both not.
fun invalidProperty(properties: List[Property], owner: String): String {
    let index = 0;
    while (index < properties.Count) {
        let property = properties[index];
        if (property.Getter != null) {
            if (valuesTaken(property.Getter) != 0) return "The getter of property " + property.Name + " of " + owner + " takes nothing and gives its value.";
            if (property.Getter.ReturnType == null) return "The getter of property " + property.Name + " of " + owner + " takes nothing and gives its value.";
        }
        if (property.Setter != null) {
            if (valuesTaken(property.Setter) != 1) return "The setter of property " + property.Name + " of " + owner + " takes one value and returns nothing.";
            if (property.Setter.ReturnType != null) return "The setter of property " + property.Name + " of " + owner + " takes one value and returns nothing.";
            if (property.Getter != null) {
                if (property.Getter.IsStatic != property.Setter.IsStatic) return "The accessors of property " + property.Name + " of " + owner + " are both static or both not.";
                if (property.Getter.IsClass != property.Setter.IsClass) return "The accessors of property " + property.Name + " of " + owner + " are both the class's or both not.";
            }
        }
        index = index + 1;
    }
    return null;
}

@definitionLoader
fun loadMethod(node: Method): IResult[CompilerObject, Error] { return Result[CompilerObject].Ok(MethodObject(node)); }

@definitionLoader
fun loadConstructor(node: Constructor): IResult[CompilerObject, Error] { return Result[CompilerObject].Ok(ConstructorObject(node)); }

// A metaclass is its definition's, so its node gives the one its definition has.
@typeLoader
fun typeOfClassOf(node: ClassOf): IResult[CompilerObject, Error] {
    let of = hirLoader.LoadType(node.Of);
    if (of.IsError) return of;
    return Result[CompilerObject].Ok(typeOfValue(of.Unwrap()));
}

// An interface's methods are what each class that implements it has, so they have no
// code of their own.
@definitionLoader
fun loadInterface(node: Interface): IResult[CompilerObject, Error] {
    let index = 0;
    while (index < node.Methods.Count) {
        if (node.Methods[index].Body != null) return Result[CompilerObject].Error("Method " + node.Methods[index].Name + " of the interface " + node.Name + " has no code.");
        index = index + 1;
    }

    let invalid = invalidProperty(node.Properties, "the interface " + node.Name);
    if (invalid != null) return Result[CompilerObject].Error(invalid);

    let loaded = InterfaceObject(node);
    loaded.holdMembers();
    return Result[CompilerObject].Ok(loaded);
}


// A node used as an instance, or as a type, is the one CO its definition gives.
@instanceLoader
fun instanceOfClass(node: Class): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
@typeLoader
fun typeOfClass(node: Class): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
@instanceLoader
fun instanceOfInterface(node: Interface): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
@typeLoader
fun typeOfInterface(node: Interface): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
@instanceLoader
fun instanceOfMethod(node: Method): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
@instanceLoader
fun instanceOfConstructor(node: Constructor): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
// The object in code is what the code's method or constructor defines it as.
@instanceLoader
fun instanceOfReceiver(node: Receiver): IResult[CompilerObject, Error] { return CodeLoaders.Defined(node); }

// ---- Installing it

// `def` is a Z# class wherever nothing else compiles one.
@entrypoint
fun install() { metaTypes.Default[TypeSyntax](Class as Instance); }
