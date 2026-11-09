// std:oop — Z#'s own classes, defined in Z#.
//
// A Z# class has no inheritance. It has fields, methods and named constructors, and it
// implements interfaces, which declare methods a value of any class that implements
// them has. A method reaches the class's members by name, and names the object `this` if
// its first parameter is `this`. A class or an interface lowers through the target's
// capabilities: a target with polymorphism gets it as an object type with methods.
//
// A member marked `@static` is the class's own, not its objects': a static field is one
// variable, and a static method is a function, with no object. A member marked `@class`
// is the class's type object's: a class is a value, the one object of its metaclass,
// whose fields are the class fields and whose methods are the class methods, which run on
// it. An interface can declare class methods too, which the class of each value of it
// has, so the type of any class that implements `Shape`, its class shape, is
// `Type[Shape]`, as `Type[Square]` is the type of `Square`'s type object.
//
// Importing it makes `def` a Z# class wherever nothing else compiles one, and
// `def Name of Interface` a Z# interface.

import { String, Object, Boolean, Int32, List, Dictionary, HashSet, DotNet_Interface, explicitThis, static as clrStatic, DotNet_Class, DotNet_Module } from "core:runtime";
import { Instance, Definition, Variable, Type, Named as NamedDefinition, OperatorDefinition } from "std:zsharp/hir";
import { Code, Block, Local, Expression, ExpressionStatement, Set, StringLiteral, NamedMember, Get, Declare, BlockExpression, Statement, Cast as CastCode } from "std:zsharp/hir/code";
import {
    Function as FunctionSyntax, Definition as DefinitionSyntax, TypeDefinition as TypeSyntax,
    Constructor as ConstructorSyntax, Accessor as AccessorSyntax, LetExpression as LetSyntax, Expression as ExpressionSyntax,
    Signature as SignatureSyntax, Parameter as ParameterSyntax, Statement as StatementSyntax, DefinitionStatement as DefinitionStatementSyntax,
    DecoratedStatement as DecoratedSyntax, ExpressionStatement as ExpressionStatementSyntax
} from "std:zsharp/lang/ast";
import { CompilerObject } from "std:zsharp/mir";
import { Error, ErrorMessage, Result, IResult } from "std:zsharp/common";
import { Argument, ArgumentStream, MatchResult } from "std:zsharp/compiler/cg";
import { UnsupportedError } from "std:zsharp/compiler";
import { ICTCallable, ICTGetMember, ICTSet, ICTCastTo, ICTImplicitCastTo, ICTGetIndex, ICTGet } from "std:zsharp/compiler/cg/direct";
import { IRTGetMember, IRTCastTo, IRTTypeMatch, IRTGetIndex } from "std:zsharp/compiler/cg/typed";
import { ICompileIRExpression, ICompileIRType, ICompileIRStatement, ICompileIRDefinitionIn } from "std:zsharp/compiler/ir";
import { ITyped, IAssignableTo } from "std:zsharp/compiler/ts";
import { ASTCompiler, CompiledDefinition, CompileASTExpression, CompileASTDefinition, CompileASTStatement, Context } from "std:zsharp/compiler/ast";
import {
    Function as LIRFunction, Parameter as LIRParameter, Local as LIRLocal, Signature, Call as LIRCall,
    GetLocal, SetLocal, Return as LIRReturn, New as LIRNew, GetField, SetField, Constant, Structure,
    Global as LIRGlobal, GetGlobal, SetGlobal,
    Field as LIRField, Module as LIRModule, Expression as LIRExpression, Statement as LIRStatement, Code as LIRCode, Evaluate, Type as LIRType
} from "std:zsharp/lir";
import { Bodies, ContainerCompiler, CapabilityContext, Language, Operator } from "std:zsharp/lang";
import { OverloadGroup } from "std:zsharp/compiler/overloading";
import { CodeLoaders } from "std:zsharp/compiler/code";
import { Positional, Named, ParameterObject } from "std:functions";
import { Holder, Held, Initializing } from "std:zsharp/lowering";
import { Polymorphism } from "std:zsharp/capabilities";
import { metaTypes, bodies, language, export } from "core:zsharp";
import { cg, ir, ts, hirLoader, hirCodeLoader, mappedLoader } from "core:compiler";

// ---- The HIR

// The object a method or constructor runs on, which is the class's type object for a
// class method.
@export
def Receiver of DotNet_Class : Instance, Definition, Variable {
    let Owner: Instance;
    let IsClass: Boolean;
}

@export
def Field of DotNet_Class : Instance, Definition {
    let Name: String;
    let Type: Type;
    let DefaultValue: Expression;
    let IsStatic: Boolean;
    let IsClass: Boolean;

    @explicitThis new(this, name: String) { this.Name = name; }
    @explicitThis new(this, name: String, type: Type) { this.Name = name; this.Type = type; }
}

@export
def Method of DotNet_Class : Instance, Definition, OperatorDefinition {
    let Name: String;
    let Parameters: List[Definition];
    let Locals: List[Local];
    let ReturnType: Type;
    let Body: Code;
    let This: Receiver;
    let IsStatic: Boolean;
    let IsClass: Boolean;
    let operator: Object;

    @explicitThis new(this, name: String) {
        this.Name = name;
        this.Parameters = List[Definition]();
        this.Locals = List[Local]();
        this.This = Receiver();
    }

    // The operator it is, if it is one, as `@operator(Operator.Index)` makes it.
    @explicitThis get Operator(this): Object { return this.operator; }
    @explicitThis set Operator(this, value: Object) { this.operator = value; }
}

// A property: what its accessors read and write, which are methods of what holds it,
// named for what they are, `get Name` and `set Name`.
@export
def Property of DotNet_Class : Instance, Definition, NamedDefinition {
    let name: String;
    let Getter: Method;
    let Setter: Method;

    @explicitThis new(this, name: String) { this.name = name; }

    @explicitThis get Name(this): String { return this.name; }
}

// A constructor makes an object, runs its code on it, and gives it. The unnamed one is
// what calling the class runs; a named one is a member of the class.
@export
def Constructor of DotNet_Class : Instance, Definition {
    let Name: String;
    let Parameters: List[Definition];
    let Locals: List[Local];
    let Body: Code;
    let This: Receiver;

    @explicitThis new(this, name: String) {
        this.Name = name;
        this.Parameters = List[Definition]();
        this.Locals = List[Local]();
        this.This = Receiver();
    }
}

@export
def Class of DotNet_Class : Instance, Definition, Type, NamedDefinition {
    let name: String;
    let Fields: List[Field];
    let Methods: List[Method];
    let Constructors: List[Constructor];
    let Properties: List[Property];
    let Interfaces: List[Type];
    // Any other definition written in it.
    let Definitions: List[Definition];

    @explicitThis new(this, name: String) {
        this.name = name;
        this.Fields = List[Field]();
        this.Methods = List[Method]();
        this.Constructors = List[Constructor]();
        this.Properties = List[Property]();
        this.Interfaces = List[Type]();
        this.Definitions = List[Definition]();
    }

    @explicitThis get Name(this): String { return this.name; }
}

// An interface: methods with no code, which a value of any class that implements it
// has.
@export
def Interface of DotNet_Class : Instance, Definition, Type, NamedDefinition {
    let name: String;
    let Methods: List[Method];
    let Properties: List[Property];
    // Any other definition written in it.
    let Definitions: List[Definition];

    @explicitThis new(this, name: String) {
        this.name = name;
        this.Methods = List[Method]();
        this.Properties = List[Property]();
        this.Definitions = List[Definition]();
    }

    @explicitThis get Name(this): String { return this.name; }
}

// The type of a class's or an interface's type object: the class's metaclass, or the
// interface's class shape.
@export
def ClassOf of DotNet_Class : Instance, Definition, Type {
    let Of: Type;

    @explicitThis new(this, of: Type) { this.Of = of; }
}

// ---- The COs

def Objects of DotNet_Module {
    // What methods belong to, a class or an interface, as the object type it lowers to.
    def MethodOwner of DotNet_Interface {
        fun name(): String;

        fun structure(target: Object): Structure;

        // Declares its functions for the target, and lowers their code.
        fun lower(target: Object): Boolean;

        // Why it didn't lower for the target.
        fun failure(target: Object): String;

        // The type of its type object, whose methods are its class methods.
        fun metaclass(): CompilerObject;
    }

    // The object a method or constructor runs on, as its code uses it. It owns its LIR
    // variable for each target: a method takes it as its first parameter, and a
    // constructor makes it in a local, each asking for it as it lowers.
    def ReceiverObject of DotNet_Class : CompilerObject, ICompileIRExpression, ITyped {
        let node: Receiver;
        let parameters: Dictionary[Object, LIRParameter];
        let locals: Dictionary[Object, LIRLocal];

        @explicitThis new(this, node: Receiver) {
            this.node = node;
            this.parameters = Dictionary[Object, LIRParameter]();
            this.locals = Dictionary[Object, LIRLocal]();
        }

        @explicitThis fun get_Type(this): CompilerObject {
            let owner = hirLoader.LoadDefinition(this.node.Owner as Definition).Unwrap();
            if (this.node.IsClass) return (owner as MethodOwner).metaclass();
            return owner;
        }

        @explicitThis fun parameter(this, target: Object): LIRParameter {
            if (this.parameters.ContainsKey(target)) return this.parameters[target];

            let parameter = LIRParameter(ir.CompileType(this.get_Type(), target).Unwrap());
            parameter.Name = "this";
            this.parameters[target] = parameter;

            return parameter;
        }

        @explicitThis fun local(this, target: Object): LIRLocal {
            if (this.locals.ContainsKey(target)) return this.locals[target];

            let local = LIRLocal(ir.CompileType(this.get_Type(), target).Unwrap());
            local.Name = "this";
            this.locals[target] = local;

            return local;
        }

        @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
            if (this.parameters.ContainsKey(target)) return Result[LIRExpression].Ok(GetLocal(this.parameters[target]));
            if (this.locals.ContainsKey(target)) return Result[LIRExpression].Ok(GetLocal(this.locals[target]));
            return Result[LIRExpression].Error("There is no object here: only a method that isn't static has one.");
        }
    }

    // A field of an object, read and written: the field of the object's structure at its
    // index. A class field is a field of the type object.
    def FieldValue of DotNet_Class : CompilerObject, ICompileIRExpression, ICTSet, ITyped {
        let owner: MethodOwner;
        let object: CompilerObject;
        let node: Field;
        let index: Int32;

        @explicitThis new(this, owner: MethodOwner, object: CompilerObject, node: Field, index: Int32) {
            this.owner = owner;
            this.object = object;
            this.node = node;
            this.index = index;
        }

        @explicitThis fun get_Type(this): CompilerObject {
            return Fields.type(this.node);
        }

        @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
            let structure = this.owner.structure(target);
            if (structure == null) return Result[LIRExpression].Error(this.owner.failure(target));

            let object = ir.CompileExpression(this.object, target);
            if (object.IsError) return object;

            return Result[LIRExpression].Ok(GetField(object.Unwrap(), structure.Fields[this.index]));
        }

        @explicitThis fun Set(this, value: CompilerObject): IResult[CompilerObject, Error] {
            let converted = cg.ImplicitCast(value, this.get_Type());
            if (converted.IsError) return converted;

            return Result[CompilerObject].Ok(FieldSet(this, converted.Unwrap()));
        }
    }

    def FieldSet of DotNet_Class : CompilerObject, ICompileIRStatement {
        let field: FieldValue;
        let value: CompilerObject;

        @explicitThis new(this, field: FieldValue, value: CompilerObject) { this.field = field; this.value = value; }

        @explicitThis fun CompileIRStatement(this, target: Object): IResult[LIRStatement, Error] {
            let structure = this.field.owner.structure(target);
            let object = ir.CompileExpression(this.field.object, target);
            if (object.IsError) return Result[LIRStatement].Error(object.UnwrapError());

            let value = ir.CompileExpression(this.value, target);
            if (value.IsError) return Result[LIRStatement].Error(value.UnwrapError());

            return Result[LIRStatement].Ok(SetField(object.Unwrap(), structure.Fields[this.field.index], value.Unwrap()));
        }
    }

    // A static field: one variable of the class, a global of its module, which the
    // module's initializer gives the field's default value.
    def StaticField of DotNet_Class : CompilerObject, ICompileIRExpression, ICTSet, ITyped {
        let owner: ClassObject;
        let node: Field;
        let globals: Dictionary[Object, LIRGlobal];

        @explicitThis new(this, owner: ClassObject, node: Field) {
            this.owner = owner;
            this.node = node;
            this.globals = Dictionary[Object, LIRGlobal]();
        }

        @explicitThis fun get_Type(this): CompilerObject {
            return Fields.type(this.node);
        }

        @explicitThis fun global(this, target: Object): LIRGlobal {
            if (this.globals.ContainsKey(target)) return this.globals[target];
            if (this.owner.structure(target) == null) return null;

            let container = this.owner.modules[target];
            let global = LIRGlobal(ir.CompileType(this.get_Type(), target).Unwrap());
            global.Name = this.owner.node.Name + "." + this.node.Name;
            container.Globals.Add(global);
            this.globals[target] = global;

            Initializing.into(container, SetGlobal(global, this.owner.defaultValue(this.node, target)));
            return global;
        }

        @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
            let global = this.global(target);
            if (global == null) return Result[LIRExpression].Error(this.owner.failure(target));
            return Result[LIRExpression].Ok(GetGlobal(global));
        }

        @explicitThis fun Set(this, value: CompilerObject): IResult[CompilerObject, Error] {
            let converted = cg.ImplicitCast(value, this.get_Type());
            if (converted.IsError) return converted;

            return Result[CompilerObject].Ok(StaticFieldSet(this, converted.Unwrap()));
        }
    }

    def StaticFieldSet of DotNet_Class : CompilerObject, ICompileIRStatement {
        let field: StaticField;
        let value: CompilerObject;

        @explicitThis new(this, field: StaticField, value: CompilerObject) { this.field = field; this.value = value; }

        @explicitThis fun CompileIRStatement(this, target: Object): IResult[LIRStatement, Error] {
            let global = this.field.global(target);
            if (global == null) return Result[LIRStatement].Error(this.field.owner.failure(target));

            let value = ir.CompileExpression(this.value, target);
            if (value.IsError) return Result[LIRStatement].Error(value.UnwrapError());

            return Result[LIRStatement].Ok(SetGlobal(global, value.Unwrap()));
        }
    }

    // Matching arguments to parameters, as a function does.
    def Matching of DotNet_Class {
        @clrStatic fun values(parameters: List[Definition], arguments: Argument[]): List[CompilerObject] {
            let stream = ArgumentStream(arguments);
            let values = List[CompilerObject]();

            let index = 0;
            while (index < parameters.Count) {
                let value = cg.MatchParameter(hirLoader.LoadDefinition(parameters[index]).Unwrap(), stream);
                if (value.IsError) return null;
                values.Add(value.Unwrap());
                index = index + 1;
            }

            if (stream.HasArguments) return null;
            return values;
        }

        @clrStatic fun parameter(node: Definition, target: Object): LIRParameter {
            return (hirLoader.LoadDefinition(node).Unwrap() as ParameterObject).parameter(target);
        }

        @clrStatic fun receiver(node: Receiver): ReceiverObject {
            return hirLoader.LoadDefinition(node).Unwrap() as ReceiverObject;
        }

        @clrStatic fun typeOf(node: Definition): CompilerObject {
            return (hirLoader.LoadDefinition(node).Unwrap() as ParameterObject).get_Type();
        }

        @clrStatic fun boolean(): CompilerObject {
            return hirLoader.LoadType(Boolean as Type).Unwrap();
        }

        // Whether two types are the same, as each goes to the other.
        @clrStatic fun same(first: CompilerObject, second: CompilerObject): Boolean {
            if (first == null) return second == null;
            if (second == null) return false;
            if (ts.IsAssignableTo(first, second) == false) return false;
            return ts.IsAssignableTo(second, first);
        }

        // Lowers code into a function, giving why it can't, or null.
        @clrStatic fun lower(body: Code, function: LIRFunction, returned: CompilerObject, target: Object): Error {
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
    }

    // A method of a class or an interface. A method of an interface has no code: calling
    // it calls what the object's class has.
    def MethodObject of DotNet_Class : CompilerObject {
        let owner: MethodOwner;
        let node: Method;
        let lowered: Dictionary[Object, LIRFunction];

        @explicitThis new(this, owner: MethodOwner, node: Method) {
            this.owner = owner;
            this.node = node;
            this.lowered = Dictionary[Object, LIRFunction]();
        }

        @explicitThis fun returnType(this): CompilerObject {
            if (this.node.ReturnType == null) return null;
            return hirLoader.LoadType(this.node.ReturnType).Unwrap();
        }

        // Its function takes the object first, then its parameters.
        @explicitThis fun function(this, target: Object): LIRFunction {
            if (this.lowered.ContainsKey(target)) return this.lowered[target];
            if (this.owner.structure(target) == null) return null;

            let signature = Signature();
            let returned = this.returnType();
            if (returned != null) signature.ReturnType = ir.CompileType(returned, target).Unwrap();

            // A static method is a function: it takes no object.
            if (this.node.IsStatic == false) signature.Parameters.Add(Matching.receiver(this.node.This).parameter(target));

            let index = 0;
            while (index < this.node.Parameters.Count) {
                signature.Parameters.Add(Matching.parameter(this.node.Parameters[index], target));
                index = index + 1;
            }

            let function = LIRFunction(signature);
            function.Name = this.node.Name;
            if (this.node.Body != null) function.Body = LIRCode();
            this.lowered[target] = function;

            return function;
        }

        @explicitThis fun lowerBody(this, target: Object): Error {
            if (this.node.Body == null) return null;
            return Matching.lower(this.node.Body, this.function(target), this.returnType(), target);
        }

        // Whether it takes and gives what another method does, so it can be that method.
        @explicitThis fun matches(this, other: MethodObject): Boolean {
            if (this.node.Parameters.Count != other.node.Parameters.Count) return false;
            if (Matching.same(this.returnType(), other.returnType()) == false) return false;

            let index = 0;
            while (index < this.node.Parameters.Count) {
                if (Matching.same(Matching.typeOf(this.node.Parameters[index]), Matching.typeOf(other.node.Parameters[index])) == false) return false;
                index = index + 1;
            }

            return true;
        }
    }

    // A method reached through an object: calling it calls the method with the object. A
    // static method is called with no object, however it is reached.
    def BoundMethod of DotNet_Class : CompilerObject, ICTCallable {
        let method: MethodObject;
        let object: CompilerObject;

        @explicitThis new(this, method: MethodObject, object: CompilerObject) { this.method = method; this.object = object; }

        @explicitThis fun Call(this, arguments: Argument[]): IResult[CompilerObject, Error] {
            let values = Matching.values(this.method.node.Parameters, arguments);
            if (values == null) return Result[CompilerObject].Error(this.method.node.Name + " can't be called with these arguments.");

            if (this.method.node.IsStatic == false) values.Insert(0, this.object);
            return Result[CompilerObject].Ok(Invocation(this.method, null, values));
        }
    }

    def ConstructorObject of DotNet_Class : CompilerObject, ICTCallable {
        let owner: ClassObject;
        let node: Constructor;
        let lowered: Dictionary[Object, LIRFunction];

        @explicitThis new(this, owner: ClassObject, node: Constructor) {
            this.owner = owner;
            this.node = node;
            this.lowered = Dictionary[Object, LIRFunction]();
        }

        // Its function makes the object, runs the constructor's code on it, and gives it.
        @explicitThis fun function(this, target: Object): LIRFunction {
            if (this.lowered.ContainsKey(target)) return this.lowered[target];

            let structure = this.owner.structure(target);
            if (structure == null) return null;

            let signature = Signature();
            signature.ReturnType = structure;

            let index = 0;
            while (index < this.node.Parameters.Count) {
                signature.Parameters.Add(Matching.parameter(this.node.Parameters[index], target));
                index = index + 1;
            }

            let function = LIRFunction(signature);
            function.Name = this.owner.node.Name + "." + this.node.Name;
            function.Body = LIRCode();
            this.lowered[target] = function;

            let receiver = Matching.receiver(this.node.This).local(target);
            function.Body.Block.Locals.Add(receiver);

            let fields = List[LIRExpression]();
            let declared = this.owner.fields();
            let field = 0;
            while (field < declared.Count) {
                fields.Add(this.owner.defaultValue(declared[field], target));
                field = field + 1;
            }

            function.Body.Block.Statements.Add(SetLocal(receiver, LIRNew(structure, fields)));

            return function;
        }

        @explicitThis fun lowerBody(this, target: Object): Error {
            let function = this.function(target);
            if (this.node.Body != null) {
                let failed = Matching.lower(this.node.Body, function, null, target);
                if (failed != null) return failed;
            }

            function.Body.Block.Statements.Add(LIRReturn(GetLocal(Matching.receiver(this.node.This).local(target))));
            return null;
        }

        @explicitThis fun Call(this, arguments: Argument[]): IResult[CompilerObject, Error] {
            let values = Matching.values(this.node.Parameters, arguments);
            if (values == null) return Result[CompilerObject].Error("A constructor of " + this.owner.node.Name + " can't be called with these arguments.");

            return Result[CompilerObject].Ok(Invocation(null, this, values));
        }
    }

    // A call of a method or a constructor.
    def Invocation of DotNet_Class : CompilerObject, ICompileIRExpression, ITyped {
        let method: MethodObject;
        let constructor: ConstructorObject;
        let arguments: List[CompilerObject];

        @explicitThis new(this, method: MethodObject, constructor: ConstructorObject, arguments: List[CompilerObject]) {
            this.method = method;
            this.constructor = constructor;
            this.arguments = arguments;
        }

        @explicitThis fun get_Type(this): CompilerObject {
            if (this.method != null) return this.method.returnType();
            return this.constructor.owner;
        }

        @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
            let function: LIRFunction = null;

            if (this.method != null) {
                if (this.method.owner.lower(target) == false) return Result[LIRExpression].Error(this.method.owner.failure(target));
                function = this.method.function(target);
            } else {
                if (this.constructor.owner.lower(target) == false) return Result[LIRExpression].Error(this.constructor.owner.failure(target));
                function = this.constructor.function(target);
            }

            let values = List[LIRExpression]();
            let index = 0;
            while (index < this.arguments.Count) {
                let value = ir.CompileExpression(this.arguments[index], target);
                if (value.IsError) return value;
                values.Add(value.Unwrap());
                index = index + 1;
            }

            return Result[LIRExpression].Ok(LIRCall(function, values));
        }
    }

    // A class's metaclass: the type of its type object, which is one object for each
    // target, a global of the class's module. Its fields are the class fields, and its
    // methods the class methods. It implements the class shape of each interface its
    // class implements.
    def Metaclass of DotNet_Class : CompilerObject, MethodOwner, ICompileIRType, ICTCastTo, ICTImplicitCastTo, IRTGetMember[String], IAssignableTo {
        let owner: ClassObject;
        let node: ClassOf;
        let structures: Dictionary[Object, Structure];
        let instances: Dictionary[Object, LIRGlobal];

        @explicitThis new(this, owner: ClassObject) {
            this.owner = owner;
            this.node = ClassOf(owner.node);
            this.structures = Dictionary[Object, Structure]();
            this.instances = Dictionary[Object, LIRGlobal]();
        }

        @explicitThis fun Cast(this, type: CompilerObject): IResult[CompilerObject, Error] {
            return Description.of(this.node, hirLoader.LoadType(ClassOf as Type).Unwrap(), type);
        }

        @explicitThis fun ImplicitCast(this, type: CompilerObject): IResult[CompilerObject, Error] {
            return Description.implicitly(this.node, hirLoader.LoadType(ClassOf as Type).Unwrap(), type);
        }

        @explicitThis fun name(this): String { return "The class of " + this.owner.node.Name; }

        @explicitThis fun failure(this, target: Object): String { return this.owner.failure(target); }

        @explicitThis fun metaclass(this): CompilerObject { return this; }

        @explicitThis fun fields(this): List[Field] {
            let fields = List[Field]();
            let index = 0;
            while (index < this.owner.node.Fields.Count) {
                if (this.owner.node.Fields[index].IsClass) fields.Add(this.owner.node.Fields[index]);
                index = index + 1;
            }
            return fields;
        }

        @explicitThis fun IsAssignableTo(this, target: CompilerObject): Boolean {
            if (target is other of Metaclass) return other.owner.node == this.owner.node;
            if (target is shape of InterfaceMetaclass) return this.owner.implements(shape.owner);
            return false;
        }

        @explicitThis fun structure(this, target: Object): Structure {
            if (this.structures.ContainsKey(target)) return this.structures[target];
            if (this.owner.structure(target) == null) return null;

            let polymorphism = target as Polymorphism;
            let structure = Structure();
            structure.Name = this.owner.node.Name + ".Class";
            polymorphism.objects(structure);
            this.owner.modules[target].Structures.Add(structure);
            this.structures[target] = structure;

            let fields = this.fields();
            let index = 0;
            while (index < fields.Count) {
                let field = LIRField(ir.CompileType(Fields.type(fields[index]), target).Unwrap());
                field.Name = fields[index].Name;
                structure.Fields.Add(field);
                index = index + 1;
            }

            index = 0;
            while (index < this.owner.node.Interfaces.Count) {
                let shape = this.owner.interface(index).meta.structure(target);
                if (shape != null) polymorphism.implements(structure, shape);
                index = index + 1;
            }

            return structure;
        }

        // Its class methods are declared with its class.
        @explicitThis fun lower(this, target: Object): Boolean { return this.owner.lower(target); }

        @explicitThis fun CompileIRType(this, target: Object): IResult[LIRType, Error] {
            if (this.lower(target) == false) return Result[LIRType].Error(this.failure(target));
            return Result[LIRType].Ok(this.structure(target));
        }

        // The type object: a global of the class's module, which the module's initializer
        // makes, with its class fields' default values.
        @explicitThis fun instance(this, target: Object): LIRGlobal {
            if (this.instances.ContainsKey(target)) return this.instances[target];

            let structure = this.structure(target);

            let defaults = List[LIRExpression]();
            let fields = this.fields();
            let index = 0;
            while (index < fields.Count) {
                defaults.Add(this.owner.defaultValue(fields[index], target));
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
        @explicitThis fun Member(this, object: CompilerObject, name: String): IResult[CompilerObject, Error] {
            let fields = this.fields();
            let index = 0;
            while (index < fields.Count) {
                if (fields[index].Name == name) return Result[CompilerObject].Ok(FieldValue(this, object, fields[index], index));
                index = index + 1;
            }

            let property = Properties.find(this.owner.node.Properties, name);
            if (property != null) {
                let accessor = Properties.accessorOf(property);
                if (accessor.IsClass) return Result[CompilerObject].Ok(Properties.value(property, object));
                if (accessor.IsStatic) return Result[CompilerObject].Ok(Properties.value(property, null));
                return Result[CompilerObject].Error("There is no object here: property " + name + " is the objects' of the class " + this.owner.node.Name + ", and a class method runs on the class.");
            }

            index = 0;
            while (index < this.owner.node.Methods.Count) {
                let method = this.owner.node.Methods[index];
                if (method.Name == name) {
                    if (method.IsClass) return Result[CompilerObject].Ok(BoundMethod(this.owner.method(index), object));
                    if (method.IsStatic) return Result[CompilerObject].Ok(BoundMethod(this.owner.method(index), null));
                    return Result[CompilerObject].Error("There is no object here: method " + name + " is the objects' of the class " + this.owner.node.Name + ", and a class method runs on the class.");
                }
                index = index + 1;
            }

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
    def InterfaceMetaclass of DotNet_Class : CompilerObject, MethodOwner, ICompileIRType, ICTCastTo, ICTImplicitCastTo, IRTGetMember[String], IAssignableTo {
        let owner: InterfaceObject;
        let node: ClassOf;
        let structures: Dictionary[Object, Structure];

        @explicitThis new(this, owner: InterfaceObject) {
            this.owner = owner;
            this.node = ClassOf(owner.node);
            this.structures = Dictionary[Object, Structure]();
        }

        @explicitThis fun Cast(this, type: CompilerObject): IResult[CompilerObject, Error] {
            return Description.of(this.node, hirLoader.LoadType(ClassOf as Type).Unwrap(), type);
        }

        @explicitThis fun ImplicitCast(this, type: CompilerObject): IResult[CompilerObject, Error] {
            return Description.implicitly(this.node, hirLoader.LoadType(ClassOf as Type).Unwrap(), type);
        }

        @explicitThis fun name(this): String { return "The class shape of " + this.owner.node.Name; }

        @explicitThis fun failure(this, target: Object): String { return this.owner.failure(target); }

        @explicitThis fun metaclass(this): CompilerObject { return this; }

        @explicitThis fun IsAssignableTo(this, target: CompilerObject): Boolean {
            if (target is other of InterfaceMetaclass) return other.owner.node == this.owner.node;
            return false;
        }

        @explicitThis fun structure(this, target: Object): Structure {
            if (this.structures.ContainsKey(target)) return this.structures[target];
            if (this.owner.structure(target) == null) return null;

            let structure = Structure();
            structure.Name = this.owner.node.Name + ".Class";
            (target as Polymorphism).interfaces(structure);
            this.owner.modules[target].Structures.Add(structure);
            this.structures[target] = structure;
            return structure;
        }

        @explicitThis fun lower(this, target: Object): Boolean { return this.owner.lower(target); }

        @explicitThis fun CompileIRType(this, target: Object): IResult[LIRType, Error] {
            if (this.lower(target) == false) return Result[LIRType].Error(this.failure(target));
            return Result[LIRType].Ok(this.structure(target));
        }

        @explicitThis fun Member(this, object: CompilerObject, name: String): IResult[CompilerObject, Error] {
            let index = 0;
            while (index < this.owner.node.Methods.Count) {
                let method = this.owner.node.Methods[index];
                if (method.Name == name) {
                    if (method.IsClass) return Result[CompilerObject].Ok(BoundMethod(this.owner.method(index), object));
                }
                index = index + 1;
            }

            return Result[CompilerObject].Error("The interface " + this.owner.node.Name + " has no class method named " + name + ".");
        }
    }

    // A property of an object, or of a class or its type object: reading it calls its
    // getter, and writing it its setter, as calling them through the object does.
    def PropertyValue of DotNet_Class : CompilerObject, ICompileIRExpression, ICTSet, ITyped {
        let getter: MethodObject;
        let setter: MethodObject;
        let object: CompilerObject;
        let name: String;

        @explicitThis new(this, getter: MethodObject, setter: MethodObject, object: CompilerObject, name: String) {
            this.getter = getter;
            this.setter = setter;
            this.object = object;
            this.name = name;
        }

        @explicitThis fun get_Type(this): CompilerObject {
            if (this.getter != null) return this.getter.returnType();
            return Matching.typeOf(this.setter.node.Parameters[0]);
        }

        @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
            if (this.getter == null) return Result[LIRExpression].Error("Property " + this.name + " can't be read.");

            let read = BoundMethod(this.getter, this.object).Call([]);
            if (read.IsError) return Result[LIRExpression].Error(read.UnwrapError());
            return ir.CompileExpression(read.Unwrap(), target);
        }

        @explicitThis fun Set(this, value: CompilerObject): IResult[CompilerObject, Error] {
            if (this.setter == null) return Result[CompilerObject].Error("Property " + this.name + " can't be written.");

            let written = BoundMethod(this.setter, this.object).Call([Argument(value)]);
            if (written.IsError) return written;
            return Result[CompilerObject].Ok(Evaluated(written.Unwrap()));
        }
    }

    // A call evaluated as a statement, for what it does.
    def Evaluated of DotNet_Class : CompilerObject, ICompileIRStatement {
        let call: CompilerObject;

        @explicitThis new(this, call: CompilerObject) { this.call = call; }

        @explicitThis fun CompileIRStatement(this, target: Object): IResult[LIRStatement, Error] {
            let call = ir.CompileExpression(this.call, target);
            if (call.IsError) return Result[LIRStatement].Error(call.UnwrapError());
            return Result[LIRStatement].Ok(Evaluate(call.Unwrap()));
        }
    }

    // What indexing a class or one of its values calls: its methods that are its index
    // operator, the class's for the class and the objects' for a value, called with what
    // is indexed as a call of them is.
    def Indexing of DotNet_Class {
        @clrStatic fun call(methods: List[Method], load: MethodOwner, onClass: Boolean, object: CompilerObject, arguments: Argument[], owner: String): IResult[CompilerObject, Error] {
            let candidates = List[CompilerObject]();
            let index = 0;
            while (index < methods.Count) {
                let method = methods[index];
                if (Operator.Index.Equals(method.Operator)) {
                    if (method.IsClass == onClass) {
                        if (method.IsStatic == false) candidates.Add(BoundMethod(hirLoader.LoadDefinition(method).Unwrap() as MethodObject, object));
                    }
                }
                index = index + 1;
            }

            if (candidates.Count == 0) return Result[CompilerObject].Error(owner + " has no index operator.");
            return cg.Call(OverloadGroup.Of(candidates), arguments);
        }
    }

    // A property of a class or an interface, by its name, reached through an object or a
    // class: its accessors' COs, which are its methods'.
    def Properties of DotNet_Class {
        @clrStatic fun find(properties: List[Property], name: String): Property {
            let index = 0;
            while (index < properties.Count) {
                if (properties[index].Name == name) return properties[index];
                index = index + 1;
            }
            return null;
        }

        @clrStatic fun accessor(method: Method): MethodObject {
            if (method == null) return null;
            return hirLoader.LoadDefinition(method).Unwrap() as MethodObject;
        }

        @clrStatic fun value(property: Property, object: CompilerObject): PropertyValue {
            return PropertyValue(Properties.accessor(property.Getter), Properties.accessor(property.Setter), object, property.Name);
        }

        // Whether its accessors are the class's or its type object's, as the getter is, or
        // the setter if it has none.
        @clrStatic fun accessorOf(property: Property): Method {
            if (property.Getter != null) return property.Getter;
            return property.Setter;
        }
    }

    // The definitions a class or an interface holds, reached by the names `Named` gives
    // them.
    def Owned of DotNet_Class {
        @clrStatic fun member(definitions: List[Definition], name: String, owner: String): IResult[CompilerObject, Error] {
            let index = 0;
            while (index < definitions.Count) {
                if (definitions[index] is named of NamedDefinition) {
                    if (named.Name == name) return hirLoader.LoadDefinition(definitions[index]);
                }
                index = index + 1;
            }
            return Result[CompilerObject].Error(owner + " has no member named " + name + ".");
        }
    }

    // A field's type: the one written, or else its default value's.
    def Fields of DotNet_Class {
        @clrStatic fun type(field: Field): CompilerObject {
            if (field.Type != null) return hirLoader.LoadType(field.Type).Unwrap();
            return ts.TypeOf(hirCodeLoader.LoadExpression(field.DefaultValue).Unwrap()).Unwrap();
        }
    }

    // A class's value: its type object, of its metaclass.
    def TypeObject of DotNet_Class : CompilerObject, ICompileIRExpression, ITyped {
        let owner: ClassObject;

        @explicitThis new(this, owner: ClassObject) { this.owner = owner; }

        @explicitThis fun get_Type(this): CompilerObject { return this.owner.meta; }

        @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
            return this.owner.CompileIRExpression(target);
        }
    }

    // An interface's value, of its class shape: what any class that implements it is.
    // Nothing lowers it to code, as an interface has no type object of its own.
    def InterfaceValue of DotNet_Class : CompilerObject, ITyped {
        let owner: InterfaceObject;

        @explicitThis new(this, owner: InterfaceObject) { this.owner = owner; }

        @explicitThis fun get_Type(this): CompilerObject { return this.owner.meta; }
    }

    // A value that exists as the code is compiled, as the node a definition is.
    def Known of DotNet_Class : CompilerObject, ICompileIRExpression, ITyped {
        let value: Object;
        let type: CompilerObject;

        @explicitThis new(this, value: Object, type: CompilerObject) { this.value = value; this.type = type; }

        @explicitThis fun get_Type(this): CompilerObject { return this.type; }

        @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
            let type = ir.CompileType(this.type, target);
            if (type.IsError) return Result[LIRExpression].Error(type.UnwrapError());
            return Result[LIRExpression].Ok(Constant(type.Unwrap(), this.value));
        }
    }

    // What describes a definition is its node: casting the definition to what describes
    // it, which is reflection, gives the node. Where an HIR type is expected, as in a
    // signature, the definition goes there as its node without a cast.
    def Description of DotNet_Class {
        @clrStatic fun of(node: Object, described: CompilerObject, type: CompilerObject): IResult[CompilerObject, Error] {
            if (ts.IsAssignableTo(type, described)) return Result[CompilerObject].Ok(Known(node, described));
            return Result[CompilerObject].Error("A definition is only described by its node.");
        }

        @clrStatic fun implicitly(node: Object, described: CompilerObject, type: CompilerObject): IResult[CompilerObject, Error] {
            if (ts.IsAssignableTo(hirLoader.LoadType(Type as Type).Unwrap(), type)) return Description.of(node, described, type);
            return Result[CompilerObject].Error(UnsupportedError("A definition goes as its node only where a type is described."));
        }
    }

    // A value of an interface cast down to a class or another interface, or tested for
    // being of it. The object knows what it is, so the platform's polymorphism casts and
    // tests it, through a function with no code that the capability marks.
    def Narrowing of DotNet_Class : CompilerObject, ICompileIRExpression, ITyped {
        let value: CompilerObject;
        let origin: CompilerObject;
        let to: CompilerObject;
        let test: Boolean;

        @explicitThis new(this, value: CompilerObject, origin: CompilerObject, to: CompilerObject, test: Boolean) {
            this.value = value;
            this.origin = origin;
            this.to = to;
            this.test = test;
        }

        @explicitThis fun get_Type(this): CompilerObject {
            if (this.test) return Matching.boolean();
            return this.to;
        }

        @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
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
                    signature.ReturnType = ir.CompileType(Matching.boolean(), target).Unwrap();
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
    def InterfaceObject of DotNet_Class : CompilerObject, MethodOwner, ICompileIRType, ICTCastTo, ICTImplicitCastTo, ICTGetMember[String], IRTGetMember[String], IRTGetIndex, ICTGet, IRTCastTo, IRTTypeMatch, IAssignableTo, Held, ICompileIRDefinitionIn[LIRModule] {
        let node: Interface;
        let holder: Holder;
        let structures: Dictionary[Object, Structure];
        let modules: Dictionary[Object, LIRModule];
        let lowerings: Dictionary[Object, Boolean];
        let meta: InterfaceMetaclass;

        @explicitThis new(this, node: Interface) {
            this.node = node;
            this.structures = Dictionary[Object, Structure]();
            this.modules = Dictionary[Object, LIRModule]();
            this.lowerings = Dictionary[Object, Boolean]();
            this.meta = InterfaceMetaclass(this);
        }

        @explicitThis fun hold(this, holder: Holder) { this.holder = holder; }

        @explicitThis fun CompileIRDefinition(this, owner: LIRModule, target: Object): Error {
            if (this.lower(target)) return null;
            return ErrorMessage(this.failure(target));
        }

        @explicitThis fun container(this, target: Object): LIRModule {
            if (this.holder != null) return this.holder.module(target);
            return LIRModule(this.node.Name);
        }

        // Indexing a value of it calls its methods that are its index operator, which are
        // what the value's class has.
        @explicitThis fun Index(this, object: CompilerObject, arguments: Argument[]): IResult[CompilerObject, Error] {
            return Indexing.call(this.node.Methods, this, false, object, arguments, "The interface " + this.node.Name);
        }

        @explicitThis fun metaclass(this): CompilerObject { return this.meta; }

        // What it holds is reached by its name.
        @explicitThis fun Member(this, name: String): IResult[CompilerObject, Error] {
            return Owned.member(this.node.Definitions, name, "The interface " + this.node.Name);
        }

        // Its value is of its class shape, the type of any class that implements it, so
        // `Type[Shape]` is that; evaluating it is what fails, as nothing lowers it.
        @explicitThis fun Get(this): IResult[CompilerObject, Error] { return Result[CompilerObject].Ok(InterfaceValue(this)); }

        @explicitThis fun name(this): String { return "The interface " + this.node.Name; }

        @explicitThis fun failure(this, target: Object): String {
            return "The interface " + this.node.Name + " can't be lowered for a target without polymorphism.";
        }

        @explicitThis fun Cast(this, type: CompilerObject): IResult[CompilerObject, Error] {
            return Description.of(this.node, hirLoader.LoadType(Interface as Type).Unwrap(), type);
        }

        @explicitThis fun ImplicitCast(this, type: CompilerObject): IResult[CompilerObject, Error] {
            return Description.implicitly(this.node, hirLoader.LoadType(Interface as Type).Unwrap(), type);
        }

        @explicitThis fun IsAssignableTo(this, target: CompilerObject): Boolean {
            if (target is other of InterfaceObject) return other.node == this.node;
            return false;
        }

        @explicitThis fun structure(this, target: Object): Structure {
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
        @explicitThis fun lower(this, target: Object): Boolean {
            if (this.lowerings.ContainsKey(target)) return this.lowerings[target];

            let structure = this.structure(target);
            if (structure == null) return false;
            if (this.lowerings.ContainsKey(target)) return this.lowerings[target];

            this.lowerings[target] = true;
            let container = this.modules[target];
            let polymorphism = target as Polymorphism;

            let index = 0;
            while (index < this.node.Methods.Count) {
                let method = this.method(index);
                let function = method.function(target);
                container.Functions.Add(function);
                if (method.node.IsClass) polymorphism.method(function, this.meta.structure(target));
                else polymorphism.method(function, structure);
                index = index + 1;
            }

            return true;
        }

        @explicitThis fun method(this, index: Int32): MethodObject {
            return hirLoader.LoadDefinition(this.node.Methods[index]).Unwrap() as MethodObject;
        }

        @explicitThis fun CompileIRType(this, target: Object): IResult[LIRType, Error] {
            if (this.lower(target) == false) return Result[LIRType].Error(this.failure(target));
            return Result[LIRType].Ok(this.structure(target));
        }

        @explicitThis fun Member(this, object: CompilerObject, name: String): IResult[CompilerObject, Error] {
            let property = Properties.find(this.node.Properties, name);
            if (property != null) {
                if (Properties.accessorOf(property).IsClass == false) return Result[CompilerObject].Ok(Properties.value(property, object));
            }

            let index = 0;
            while (index < this.node.Methods.Count) {
                let method = this.node.Methods[index];
                if (method.Name == name) {
                    if (method.IsClass == false) return Result[CompilerObject].Ok(BoundMethod(this.method(index), object));
                }
                index = index + 1;
            }

            return Result[CompilerObject].Error("The interface " + this.node.Name + " has no member named " + name + ".");
        }

        // What a value of it might be: a class that implements it, or another interface.
        @explicitThis fun narrows(this, type: CompilerObject): Boolean {
            if (type is other of InterfaceObject) return true;
            if (type is implementer of ClassObject) return implementer.implements(this);
            return false;
        }

        @explicitThis fun Cast(this, value: CompilerObject, type: CompilerObject): IResult[CompilerObject, Error] {
            if (this.narrows(type)) return Result[CompilerObject].Ok(Narrowing(value, this, type, false));
            return Result[CompilerObject].Error("A value of the interface " + this.node.Name + " can only be cast to what implements it.");
        }

        @explicitThis fun Match(this, value: CompilerObject, type: CompilerObject): IResult[MatchResult, Error] {
            if (this.narrows(type) == false) return Result[MatchResult].Error("A value of the interface " + this.node.Name + " can only be matched against what implements it.");

            let result = MatchResult();
            result.Match = Narrowing(value, this, type, true);
            return Result[MatchResult].Ok(result);
        }
    }

    // A Z# class. As a type, it is the object type its target lowers it to; calling it
    // runs its unnamed constructor, and its named constructors are its members. A value
    // of it has its fields and methods as members.
    def ClassObject of DotNet_Class : CompilerObject, MethodOwner, ICompileIRType, ICTCallable, ICTCastTo, ICTImplicitCastTo, ICTGetMember[String], IRTGetMember[String], ICTGetIndex, IRTGetIndex, ICTGet, IAssignableTo, ICompileIRExpression, ITyped, Held, ICompileIRDefinitionIn[LIRModule] {
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

        @explicitThis new(this, node: Class) {
            this.node = node;
            this.building = HashSet[Object]();
            this.structures = Dictionary[Object, Structure]();
            this.modules = Dictionary[Object, LIRModule]();
            this.lowerings = Dictionary[Object, Boolean]();
            this.failures = Dictionary[Object, String]();
            this.meta = Metaclass(this);
            this.statics = Dictionary[Field, StaticField]();
        }

        @explicitThis fun hold(this, holder: Holder) { this.holder = holder; }

        @explicitThis fun CompileIRDefinition(this, owner: LIRModule, target: Object): Error {
            if (this.lower(target)) return null;
            return ErrorMessage(this.failure(target));
        }

        // Indexing the class calls its class methods that are its index operator, on the
        // class, and indexing one of its objects the methods that are, on the object.
        @explicitThis fun Index(this, arguments: Argument[]): IResult[CompilerObject, Error] {
            return Indexing.call(this.node.Methods, this, true, this, arguments, "The class " + this.node.Name);
        }

        @explicitThis fun Index(this, object: CompilerObject, arguments: Argument[]): IResult[CompilerObject, Error] {
            return Indexing.call(this.node.Methods, this, false, object, arguments, "The class " + this.node.Name);
        }

        // The LIR module it lowers into: what holds it lowers to, or one of its own.
        @explicitThis fun container(this, target: Object): LIRModule {
            if (this.holder != null) return this.holder.module(target);
            return LIRModule(this.node.Name);
        }

        @explicitThis fun metaclass(this): CompilerObject { return this.meta; }

        // The class's value is its type object.
        @explicitThis fun Get(this): IResult[CompilerObject, Error] { return Result[CompilerObject].Ok(TypeObject(this)); }

        // A class used as a value is its type object, as long as what gives a definition
        // gives its CO, rather than its value.
        @explicitThis fun get_Type(this): CompilerObject { return this.meta; }

        @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
            if (this.lower(target) == false) return Result[LIRExpression].Error(this.failure(target));
            return Result[LIRExpression].Ok(GetGlobal(this.meta.instance(target)));
        }

        // The fields each object has, in order.
        @explicitThis fun fields(this): List[Field] {
            let fields = List[Field]();
            let index = 0;
            while (index < this.node.Fields.Count) {
                let field = this.node.Fields[index];
                if (field.IsStatic == false) {
                    if (field.IsClass == false) fields.Add(field);
                }
                index = index + 1;
            }
            return fields;
        }

        @explicitThis fun staticField(this, node: Field): StaticField {
            if (this.statics.ContainsKey(node) == false) this.statics[node] = StaticField(this, node);
            return this.statics[node];
        }

        @explicitThis fun name(this): String { return "The class " + this.node.Name; }

        @explicitThis fun failure(this, target: Object): String {
            if (this.failures.ContainsKey(target)) return this.failures[target];
            return "The class " + this.node.Name + " can't be lowered for a target without polymorphism.";
        }

        @explicitThis fun Cast(this, type: CompilerObject): IResult[CompilerObject, Error] {
            return Description.of(this.node, hirLoader.LoadType(Class as Type).Unwrap(), type);
        }

        @explicitThis fun ImplicitCast(this, type: CompilerObject): IResult[CompilerObject, Error] {
            return Description.implicitly(this.node, hirLoader.LoadType(Class as Type).Unwrap(), type);
        }

        // A class goes to itself and to the interfaces it implements.
        @explicitThis fun IsAssignableTo(this, target: CompilerObject): Boolean {
            if (target is other of ClassObject) return other.node == this.node;
            if (target is implemented of InterfaceObject) return this.implements(implemented);
            return false;
        }

        @explicitThis fun interface(this, index: Int32): InterfaceObject {
            return hirLoader.LoadType(this.node.Interfaces[index]).Unwrap() as InterfaceObject;
        }

        @explicitThis fun implements(this, implemented: InterfaceObject): Boolean {
            let index = 0;
            while (index < this.node.Interfaces.Count) {
                if (this.interface(index).node == implemented.node) return true;
                index = index + 1;
            }
            return false;
        }

        // The method that is the interface's method, as it has the name and takes and
        // gives what the interface's does, or null.
        @explicitThis fun implementation(this, method: MethodObject): MethodObject {
            let index = 0;
            while (index < this.node.Methods.Count) {
                if (this.node.Methods[index].Name == method.node.Name) {
                    let found = this.method(index);
                    if (found.node.IsClass == method.node.IsClass) {
                        if (found.matches(method)) return found;
                    }
                }
                index = index + 1;
            }
            return null;
        }

        // The object type: a structure the target's polymorphism makes the type of
        // objects, with its fields, that implements its interfaces.
        @explicitThis fun structure(this, target: Object): Structure {
            if (this.structures.ContainsKey(target)) return this.structures[target];

            if (target is polymorphism of Polymorphism) {
                let container = this.container(target);
                if (this.structures.ContainsKey(target)) return this.structures[target];

                let structure = Structure();
                structure.Name = this.node.Name;
                polymorphism.objects(structure);
                container.Structures.Add(structure);
                this.modules[target] = container;
                this.structures[target] = structure;
                this.building.Add(target);

                let fields = this.fields();
                let index = 0;
                while (index < fields.Count) {
                    let field = LIRField(ir.CompileType(Fields.type(fields[index]), target).Unwrap());
                    field.Name = fields[index].Name;
                    structure.Fields.Add(field);
                    index = index + 1;
                }

                this.building.Remove(target);

                index = 0;
                while (index < this.node.Interfaces.Count) {
                    let implemented = this.interface(index).structure(target);
                    if (implemented != null) polymorphism.implements(structure, implemented);
                    index = index + 1;
                }

                return structure;
            }

            return null;
        }

        @explicitThis fun defaultValue(this, field: Field, target: Object): LIRExpression {
            let type = ir.CompileType(Fields.type(field), target).Unwrap();

            if (field.DefaultValue == null) return Constant(type, null);

            let value = cg.ImplicitCast(hirCodeLoader.LoadExpression(field.DefaultValue).Unwrap(), Fields.type(field)).Unwrap();
            return ir.CompileExpression(value, target).Unwrap();
        }

        // The class's functions, each declared before any code is lowered, so code can
        // call any of them, and each marked as what it is.
        @explicitThis fun lower(this, target: Object): Boolean {
            if (this.lowerings.ContainsKey(target)) return this.lowerings[target];

            let structure = this.structure(target);
            if (structure == null) return false;
            if (this.lowerings.ContainsKey(target)) return this.lowerings[target];

            this.lowerings[target] = true;
            let container = this.modules[target];
            let polymorphism = target as Polymorphism;

            // It has each method of each interface it implements.
            let index = 0;
            while (index < this.node.Interfaces.Count) {
                let implemented = this.interface(index);
                if (implemented.lower(target) == false) return this.failed(target, implemented.failure(target));

                let method = 0;
                while (method < implemented.node.Methods.Count) {
                    let required = implemented.method(method);
                    if (this.implementation(required) == null) return this.failed(target, "The class " + this.node.Name + " doesn't have every method of the interfaces it implements: " + implemented.node.Name + "." + required.node.Name + " isn't one of its methods.");
                    method = method + 1;
                }

                index = index + 1;
            }

            let methods = List[MethodObject]();
            let constructors = List[ConstructorObject]();

            index = 0;
            while (index < this.node.Methods.Count) {
                let method = this.method(index);
                let function = method.function(target);
                container.Functions.Add(function);
                if (method.node.IsClass) polymorphism.method(function, this.meta.structure(target));
                else if (method.node.IsStatic == false) polymorphism.method(function, structure);
                methods.Add(method);
                index = index + 1;
            }

            index = 0;
            while (index < this.node.Constructors.Count) {
                let constructor = this.constructor(index);
                container.Functions.Add(constructor.function(target));
                constructors.Add(constructor);
                index = index + 1;
            }

            index = 0;
            while (index < methods.Count) {
                let failed = methods[index].lowerBody(target);
                if (failed != null) return this.failed(target, failed.ToString());
                index = index + 1;
            }

            index = 0;
            while (index < constructors.Count) {
                let failed = constructors[index].lowerBody(target);
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

        @explicitThis fun failed(this, target: Object, why: String): Boolean {
            this.lowerings[target] = false;
            this.failures[target] = why;
            return false;
        }

        @explicitThis fun method(this, index: Int32): MethodObject {
            return hirLoader.LoadDefinition(this.node.Methods[index]).Unwrap() as MethodObject;
        }

        @explicitThis fun constructor(this, index: Int32): ConstructorObject {
            return hirLoader.LoadDefinition(this.node.Constructors[index]).Unwrap() as ConstructorObject;
        }

        // A class used as a type is lowered with it, so whatever has its objects has its
        // methods.
        // While its structure is being built, as for a field of a type that has the class
        // in it, the class is only its structure: what builds it lowers it once it can.
        @explicitThis fun CompileIRType(this, target: Object): IResult[LIRType, Error] {
            let structure = this.structure(target);
            if (structure == null) return Result[LIRType].Error(this.failure(target));
            if (this.building.Contains(target)) return Result[LIRType].Ok(structure);
            if (this.lower(target) == false) return Result[LIRType].Error(this.failure(target));
            return Result[LIRType].Ok(structure);
        }

        @explicitThis fun named(this, name: String): ConstructorObject {
            let index = 0;
            while (index < this.node.Constructors.Count) {
                if (this.node.Constructors[index].Name == name) return this.constructor(index);
                index = index + 1;
            }
            return null;
        }

        @explicitThis fun Call(this, arguments: Argument[]): IResult[CompilerObject, Error] {
            let constructor = this.named("");
            if (constructor == null) return Result[CompilerObject].Error("The class " + this.node.Name + " has no unnamed constructor.");
            return constructor.Call(arguments);
        }

        // The class's members are its named constructors, its static fields and methods,
        // and its class fields and methods, reached through its type object. No name is
        // the class's own: the type of its type object is `Type[Name]`.
        @explicitThis fun Member(this, name: String): IResult[CompilerObject, Error] {

            let constructor = this.named(name);
            if (constructor != null) return Result[CompilerObject].Ok(constructor);

            let property = Properties.find(this.node.Properties, name);
            if (property != null) {
                let accessor = Properties.accessorOf(property);
                if (accessor.IsStatic) return Result[CompilerObject].Ok(Properties.value(property, null));
                if (accessor.IsClass) return Result[CompilerObject].Ok(Properties.value(property, this));
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

            index = 0;
            while (index < this.node.Methods.Count) {
                let method = this.node.Methods[index];
                if (method.Name == name) {
                    if (method.IsStatic) return Result[CompilerObject].Ok(BoundMethod(this.method(index), null));
                    if (method.IsClass) return Result[CompilerObject].Ok(BoundMethod(this.method(index), this));
                    return Result[CompilerObject].Error("Method " + name + " of the class " + this.node.Name + " is its objects'.");
                }
                index = index + 1;
            }

            return Owned.member(this.node.Definitions, name, "The class " + this.node.Name);
        }

        // A value's members are its fields and methods, and the class's static and class
        // ones, which are the same whatever object reaches them: with no inheritance, a
        // value of a class is of that class, so its type object is the class's.
        @explicitThis fun Member(this, object: CompilerObject, name: String): IResult[CompilerObject, Error] {
            let fields = this.fields();
            let index = 0;
            while (index < fields.Count) {
                if (fields[index].Name == name) return Result[CompilerObject].Ok(FieldValue(this, object, fields[index], index));
                index = index + 1;
            }

            let property = Properties.find(this.node.Properties, name);
            if (property != null) {
                if (Properties.accessorOf(property).IsClass) return Result[CompilerObject].Ok(Properties.value(property, this));
                return Result[CompilerObject].Ok(Properties.value(property, object));
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

            index = 0;
            while (index < this.node.Methods.Count) {
                let method = this.node.Methods[index];
                if (method.Name == name) {
                    if (method.IsClass) return Result[CompilerObject].Ok(BoundMethod(this.method(index), this));
                    return Result[CompilerObject].Ok(BoundMethod(this.method(index), object));
                }
                index = index + 1;
            }

            return Result[CompilerObject].Error("The class " + this.node.Name + " has no member named " + name + ".");
        }
    }
}

// ---- Properties

// A property of a class or an interface, defined as the building code runs from its
// accessors, the methods `get Name` and `set Name`.
def Accessors of DotNet_Class {
    @clrStatic fun define(owner: Instance, name: String) {
        if (owner is type of Class) Accessors.pair(type.Methods, type.Properties, name);
        if (owner is shape of Interface) Accessors.pair(shape.Methods, shape.Properties, name);
    }

    @clrStatic fun pair(methods: List[Method], properties: List[Property], name: String) {
        let property = Property(name);
        let index = 0;
        while (index < methods.Count) {
            if (methods[index].Name == "get " + name) property.Getter = methods[index];
            if (methods[index].Name == "set " + name) property.Setter = methods[index];
            index = index + 1;
        }
        properties.Add(property);
    }
}

// ---- The meta-compilers

// What the members of a class or an interface are compiled with: a let is a field, and a
// fun or a new written directly in it is a method or a constructor. An interface has only
// methods, with no code. A definition inside a member is the member's to compile.
def Members of DotNet_Class {
    let self: Local;
    let isClass: Boolean;
    // The names of the class's members, which its code reaches by name.
    let names: List[String];
    let fork: ASTCompiler;
    let expressions: CompileASTExpression;
    let definitions: CompileASTDefinition;
    let owner: Context;

    let statements: CompileASTStatement;

    @explicitThis new(this, self: Local, isClass: Boolean, compiler: ASTCompiler) {
        this.self = self;
        this.isClass = isClass;
        this.names = List[String]();
        this.fork = compiler.Fork();
        this.expressions = this.fork.Expression;
        this.definitions = this.fork.Definition;
        this.statements = this.fork.Statement;
        this.owner = Bodies.DefinitionOf(compiler.Contexts.Current);
    }

    @explicitThis fun expression(this, compiler: ASTCompiler, node: ExpressionSyntax): IResult[Expression, Error] {
        if (node is local of LetSyntax) return this.field(compiler, local, List[Expression]());
        return this.expressions(compiler, node);
    }

    // A field can be decorated, as a member defined with `fun` can: its decorators are
    // given the field as it is built, before it is added.
    @explicitThis fun statement(this, compiler: ASTCompiler, node: StatementSyntax): IResult[Statement, Error] {
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

    @explicitThis fun definition(this, compiler: ASTCompiler, node: DefinitionSyntax): IResult[CompiledDefinition, Error] {
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
                    if (function.Name.Length > 0) this.names.Add(function.Name);
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
    @explicitThis fun field(this, compiler: ASTCompiler, local: LetSyntax, decorators: List[Expression]): IResult[Expression, Error] {
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

        block.Statements.Add(ExpressionStatement(Bodies.Add(this.self, "Fields", Members.kind(Language.Decorate(Get(field), decorators), Field as Instance))));
        block.Result = Get(field);
        this.names.Add(local.Name);

        return Result[Expression].Ok(block);
    }

    // A member is added as what it is, whatever its decorators gave: a method or a
    // constructor to its own list, and anything else to the definitions, which takes what
    // is a definition.
    @explicitThis fun register(this, member: CompiledDefinition, value: Expression): Statement {
        let list = "Definitions";
        if (member.UnderlyingType == Method as Instance) list = "Methods";
        if (member.UnderlyingType == Constructor as Instance) list = "Constructors";
        return ExpressionStatement(Bodies.Add(this.self, list, Members.kind(value, member.UnderlyingType)));
    }

    @clrStatic fun kind(value: Expression, type: Instance): Expression {
        let cast = CastCode();
        cast.Value = value;
        cast.Type = Get(type);
        return cast;
    }
}

// What a method or constructor's code is compiled with: the object it runs on, which is
// `this` if its first parameter says so, and each of the class's members by its name,
// unless a parameter has the name. A name is bound through the object, as `this.name`
// would be, whatever the member is: what the binding means is the member's to say when
// the code is loaded, once the class is complete. An object's member is reached through
// it, binding a static member does nothing, and a class member is reached through the
// class's type object.
def Receivers of DotNet_Class {
    @clrStatic fun receiver(compiler: ASTCompiler, block: Block, self: Local, members: Members, signature: SignatureSyntax): SignatureSyntax {
        let receiver = Local("this", Receiver as Type);
        receiver.Init = Bodies.Member(self, "This");
        block.Statements.Add(Declare(receiver));

        let owner = Set();
        owner.Target = Bodies.Member(receiver, "Owner");
        owner.Value = Get(members.self);
        block.Statements.Add(ExpressionStatement(owner));

        let names = List[String]();
        let rest = SignatureSyntax();
        rest.KwArgs = signature.KwArgs;
        rest.Args = List[ParameterSyntax]();

        let index = 0;
        while (index < Receivers.count(signature.Args)) {
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
        while (index < Receivers.count(signature.KwArgs)) {
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

    @clrStatic fun count(parameters: List[ParameterSyntax]): Int32 {
        if (parameters == null) return 0;
        return parameters.Count;
    }
}

def MethodFiller of DotNet_Class {
    let compiler: ASTCompiler;
    let function: FunctionSyntax;
    let self: Local;
    let members: Members;

    @explicitThis new(this, compiler: ASTCompiler, function: FunctionSyntax, self: Local, members: Members) {
        this.compiler = compiler;
        this.function = function;
        this.self = self;
        this.members = members;
    }

    @explicitThis fun fill(this): IResult[Block, Error] {
        let block = Block([]);
        let signature = Receivers.receiver(this.compiler, block, this.self, this.members, this.function.Signature);

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

def ConstructorFiller of DotNet_Class {
    let compiler: ASTCompiler;
    let constructor: ConstructorSyntax;
    let self: Local;
    let members: Members;

    @explicitThis new(this, compiler: ASTCompiler, constructor: ConstructorSyntax, self: Local, members: Members) {
        this.compiler = compiler;
        this.constructor = constructor;
        this.self = self;
        this.members = members;
    }

    @explicitThis fun fill(this): IResult[Block, Error] {
        let block = Block([]);
        let signature = Receivers.receiver(this.compiler, block, this.self, this.members, this.constructor.Signature);

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
def ClassFiller of DotNet_Class {
    let compiler: ASTCompiler;
    let definition: TypeSyntax;
    let self: Local;
    let isClass: Boolean;
    let members: Members;

    @explicitThis new(this, compiler: ASTCompiler, definition: TypeSyntax, self: Local, isClass: Boolean) {
        this.compiler = compiler;
        this.definition = definition;
        this.self = self;
        this.isClass = isClass;
    }

    @explicitThis fun fill(this): IResult[Block, Error] {
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

        let filled = this.compiler.Contexts.Within[IResult[Block, Error]](CapabilityContext(this.compiler.Contexts.Current, this.members), this.compileMembers);
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
                        callee.Target = Get(Accessors as Instance);
                        callee.Name = "define";
                        block.Statements.Add(ExpressionStatement(Bodies.Call(callee, Get(this.self), StringLiteral(accessor.Name))));
                    }
                }
            }
            index = index + 1;
        }

        return Result[Block].Ok(block);
    }

    @explicitThis fun compileMembers(this): IResult[Block, Error] {
        return ContainerCompiler(language).Compile(this.members.fork, this.definition.Content.Statements, this.members.register);
    }
}

def Meta of DotNet_Module {
    // The members of the class being compiled, from the context they are compiled in.
    fun members(compiler: ASTCompiler): Members {
        let members: Members = null;
        compiler.Contexts.Has[Members](members);
        return members;
    }

    fun compileClass(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
        if (definition is type of TypeSyntax) {
            let filler = ClassFiller(compiler, type, self, true);
            return Result[CompiledDefinition].Ok(CompiledDefinition(self, Bodies.Construct(Class as Instance, StringLiteral(type.Name)), Class as Instance, filler.fill, null));
        }
        return Result[CompiledDefinition].Error("A class is a type definition.");
    }

    fun compileInterface(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
        if (definition is type of TypeSyntax) {
            let filler = ClassFiller(compiler, type, self, false);
            return Result[CompiledDefinition].Ok(CompiledDefinition(self, Bodies.Construct(Interface as Instance, StringLiteral(type.Name)), Interface as Instance, filler.fill, null));
        }
        return Result[CompiledDefinition].Error("An interface is a type definition.");
    }

    fun compileMethod(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
        if (definition is function of FunctionSyntax) {
            // An accessor is the method of its property that reads or writes it.
            let name = function.Name;
            if (name == null) name = "";
            if (definition is accessor of AccessorSyntax) {
                name = "get " + accessor.Name;
                if (accessor.IsSetter) name = "set " + accessor.Name;
            }

            let filler = MethodFiller(compiler, function, self, Meta.members(compiler));
            return Result[CompiledDefinition].Ok(CompiledDefinition(self, Bodies.Construct(Method as Instance, StringLiteral(name)), Method as Instance, filler.fill, null));
        }
        return Result[CompiledDefinition].Error("A method is a fun.");
    }

    fun compileConstructor(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
        if (definition is constructor of ConstructorSyntax) {
            let name = constructor.Name;
            if (name == null) name = "";

            let filler = ConstructorFiller(compiler, constructor, self, Meta.members(compiler));
            return Result[CompiledDefinition].Ok(CompiledDefinition(self, Bodies.Construct(Constructor as Instance, StringLiteral(name)), Constructor as Instance, filler.fill, null));
        }
        return Result[CompiledDefinition].Error("A constructor is a new.");
    }

    // A class is what it means once it is loaded, when its node is complete: each of its
    // methods has code, and each of its fields has a type, or a value to have the type of.
    fun loadClass(node: Class): IResult[CompilerObject, Error] {
        let index = 0;
        while (index < node.Methods.Count) {
            if (node.Methods[index].Body == null) return Result[CompilerObject].Error("Method " + node.Methods[index].Name + " of the class " + node.Name + " needs code.");
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

        let invalid = Meta.invalidProperty(node.Properties, "the class " + node.Name);
        if (invalid != null) return Result[CompilerObject].Error(invalid);

        return Result[CompilerObject].Ok(Objects.ClassObject(node));
    }

    // A getter takes nothing and gives the property's value, and a setter takes its value
    // and gives nothing, both of the objects or both not.
    fun invalidProperty(properties: List[Property], owner: String): String {
        let index = 0;
        while (index < properties.Count) {
            let property = properties[index];
            if (property.Getter != null) {
                if (property.Getter.Parameters.Count != 0) return "The getter of property " + property.Name + " of " + owner + " takes nothing and gives its value.";
                if (property.Getter.ReturnType == null) return "The getter of property " + property.Name + " of " + owner + " takes nothing and gives its value.";
            }
            if (property.Setter != null) {
                if (property.Setter.Parameters.Count != 1) return "The setter of property " + property.Name + " of " + owner + " takes one value and returns nothing.";
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

    fun loadMethod(node: Method): IResult[CompilerObject, Error] {
        let owner = hirLoader.LoadDefinition(node.This.Owner as Definition);
        if (owner.IsError) return owner;
        return Result[CompilerObject].Ok(Objects.MethodObject(owner.Unwrap() as Objects.MethodOwner, node));
    }

    fun loadConstructor(node: Constructor): IResult[CompilerObject, Error] {
        let owner = hirLoader.LoadDefinition(node.This.Owner as Definition);
        if (owner.IsError) return owner;
        return Result[CompilerObject].Ok(Objects.ConstructorObject(owner.Unwrap() as Objects.ClassObject, node));
    }

    // A metaclass is its definition's, so its node gives the one its definition has.
    fun typeOfClassOf(node: ClassOf): IResult[CompilerObject, Error] {
        let of = hirLoader.LoadType(node.Of);
        if (of.IsError) return of;
        return Result[CompilerObject].Ok((of.Unwrap() as Objects.MethodOwner).metaclass());
    }

    // An interface's methods are what each class that implements it has, so they have no
    // code of their own.
    fun loadInterface(node: Interface): IResult[CompilerObject, Error] {
        let index = 0;
        while (index < node.Methods.Count) {
            if (node.Methods[index].Body != null) return Result[CompilerObject].Error("Method " + node.Methods[index].Name + " of the interface " + node.Name + " has no code.");
            index = index + 1;
        }

        let invalid = Meta.invalidProperty(node.Properties, "the interface " + node.Name);
        if (invalid != null) return Result[CompilerObject].Error(invalid);

        return Result[CompilerObject].Ok(Objects.InterfaceObject(node));
    }

    fun loadReceiver(node: Receiver): IResult[CompilerObject, Error] { return Result[CompilerObject].Ok(Objects.ReceiverObject(node)); }

    // A node used as an instance, or as a type, is the one CO its definition gives.
    fun instanceOfClass(node: Class): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
    fun typeOfClass(node: Class): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
    fun instanceOfInterface(node: Interface): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
    fun typeOfInterface(node: Interface): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
    fun instanceOfMethod(node: Method): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
    fun instanceOfConstructor(node: Constructor): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
    fun instanceOfReceiver(node: Receiver): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
}

// ---- Decorators

// Marks a field or a method of a class as the class's own, not its objects'.
@export
fun static(member: Instance): Instance {
    if (member is method of Method) method.IsStatic = true;
    if (member is field of Field) field.IsStatic = true;
    return member;
}

// Marks a field or a method of a class as its type object's: a class field is one for
// the class, and a class method runs on the class. An interface's class method is one
// the class of each of its values has.
@export
fun class(member: Instance): Instance {
    if (member is method of Method) {
        method.IsClass = true;
        method.This.IsClass = true;
    }
    if (member is field of Field) field.IsClass = true;
    return member;
}

// ---- Installing it

mappedLoader.Definitions.Register[Class](Meta.loadClass);
mappedLoader.Definitions.Register[Method](Meta.loadMethod);
mappedLoader.Definitions.Register[Constructor](Meta.loadConstructor);
mappedLoader.Definitions.Register[Receiver](Meta.loadReceiver);
mappedLoader.Definitions.Register[Interface](Meta.loadInterface);
mappedLoader.Types.Register[ClassOf](Meta.typeOfClassOf);
mappedLoader.Instances.Register[Interface](Meta.instanceOfInterface);
mappedLoader.Types.Register[Interface](Meta.typeOfInterface);
mappedLoader.Instances.Register[Class](Meta.instanceOfClass);
mappedLoader.Instances.Register[Method](Meta.instanceOfMethod);
mappedLoader.Instances.Register[Constructor](Meta.instanceOfConstructor);
mappedLoader.Instances.Register[Receiver](Meta.instanceOfReceiver);
mappedLoader.Types.Register[Class](Meta.typeOfClass);

metaTypes.Register(Class as Instance, Meta.compileClass);
metaTypes.Register(Method as Instance, Meta.compileMethod);
metaTypes.Register(Constructor as Instance, Meta.compileConstructor);
metaTypes.Register(Interface as Instance, Meta.compileInterface);
metaTypes.Default[TypeSyntax](Class as Instance);
