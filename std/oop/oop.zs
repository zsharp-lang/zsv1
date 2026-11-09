// std:oop — Z#'s own classes, defined in Z#.
//
// A Z# class has no inheritance. It has fields, methods and named constructors. A
// method reaches the object's fields by name, and names the object `this` if its first
// parameter is `this`. A class lowers through the target's capabilities: a target with
// polymorphism gets the class as an object type with methods.
//
// Importing it makes `def` a Z# class wherever nothing else compiles one.

import { String, Object, Boolean, Int32, List, Dictionary, explicitThis, static } from "core:runtime";
import { Instance, Definition, Variable, Type } from "std:zsharp/hir";
import { Code, Block, Local, Expression, ExpressionStatement, Set, StringLiteral, NamedMember, Get, Declare, BlockExpression, Statement } from "std:zsharp/hir/code";
import {
    Function as FunctionSyntax, Definition as DefinitionSyntax, TypeDefinition as TypeSyntax,
    Constructor as ConstructorSyntax, LetExpression as LetSyntax, Expression as ExpressionSyntax,
    Signature as SignatureSyntax, Parameter as ParameterSyntax
} from "std:zsharp/lang/ast";
import { CompilerObject } from "std:zsharp/mir";
import { Error, Result, IResult } from "std:zsharp/common";
import { Argument, ArgumentStream } from "std:zsharp/compiler/cg";
import { ICTCallable, ICTGetMember, ICTSet } from "std:zsharp/compiler/cg/direct";
import { IRTGetMember } from "std:zsharp/compiler/cg/typed";
import { ICompileIRExpression, ICompileIRType, ICompileIRStatement } from "std:zsharp/compiler/ir";
import { ITyped, IAssignableTo } from "std:zsharp/compiler/ts";
import { ASTCompiler, CompiledDefinition, CompileASTExpression, CompileASTDefinition, Context } from "std:zsharp/compiler/ast";
import {
    Function as LIRFunction, Parameter as LIRParameter, Local as LIRLocal, Signature, Call as LIRCall,
    GetLocal, SetLocal, Return as LIRReturn, New as LIRNew, GetField, SetField, Constant, Structure,
    Field as LIRField, Module as LIRModule, Expression as LIRExpression, Statement as LIRStatement, Code as LIRCode, Evaluate, Type as LIRType
} from "std:zsharp/lir";
import { Bodies, ContainerCompiler, CapabilityContext } from "std:zsharp/lang";
import { CodeLoaders } from "std:zsharp/compiler/code";
import { Positional, Named, ParameterObject } from "std:functions";
import { Polymorphism } from "std:zsharp/capabilities";
import { metaTypes, bodies, language, export } from "core:zsharp";
import { cg, ir, ts, hirLoader, hirCodeLoader, mappedLoader } from "core:compiler";

// ---- The HIR

// The object a method or constructor runs on.
@export
def Receiver : Instance, Definition, Variable {
    let Owner: Instance;
}

@export
def Field : Instance, Definition {
    let Name: String;
    let Type: Type;
    let DefaultValue: Expression;

    @explicitThis new(this, name: String, type: Type) { this.Name = name; this.Type = type; }
}

@export
def Method : Instance, Definition {
    let Name: String;
    let Parameters: List[Definition];
    let Locals: List[Local];
    let ReturnType: Type;
    let Body: Code;
    let This: Receiver;

    @explicitThis new(this, name: String) {
        this.Name = name;
        this.Parameters = List[Definition]();
        this.Locals = List[Local]();
        this.This = Receiver();
    }
}

// A constructor makes an object, runs its code on it, and gives it. The unnamed one is
// what calling the class runs; a named one is a member of the class.
@export
def Constructor : Instance, Definition {
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
def Class : Instance, Definition, Type {
    let Name: String;
    let Fields: List[Field];
    let Methods: List[Method];
    let Constructors: List[Constructor];

    @explicitThis new(this, name: String) {
        this.Name = name;
        this.Fields = List[Field]();
        this.Methods = List[Method]();
        this.Constructors = List[Constructor]();
    }
}

// ---- The COs

module Objects {
    // The object a method or constructor runs on, as its code uses it. It owns its LIR
    // variable for each target: a method takes it as its first parameter, and a
    // constructor makes it in a local, each asking for it as it lowers.
    def ReceiverObject : CompilerObject, ICompileIRExpression, ITyped {
        let node: Receiver;
        let parameters: Dictionary[Object, LIRParameter];
        let locals: Dictionary[Object, LIRLocal];

        @explicitThis new(this, node: Receiver) {
            this.node = node;
            this.parameters = Dictionary[Object, LIRParameter]();
            this.locals = Dictionary[Object, LIRLocal]();
        }

        @explicitThis fun get_Type(this): CompilerObject {
            return hirLoader.LoadDefinition(this.node.Owner as Definition).Unwrap();
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
            return Result[LIRExpression].Error("The object is only known in the code of its method.");
        }
    }

    // A field of an object, read and written.
    def FieldValue : CompilerObject, ICompileIRExpression, ICTSet, ITyped {
        let owner: ClassObject;
        let object: CompilerObject;
        let index: Int32;

        @explicitThis new(this, owner: ClassObject, object: CompilerObject, index: Int32) {
            this.owner = owner;
            this.object = object;
            this.index = index;
        }

        @explicitThis fun get_Type(this): CompilerObject {
            return hirLoader.LoadType(this.owner.node.Fields[this.index].Type).Unwrap();
        }

        @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
            let structure = this.owner.structure(target);
            if (structure == null) return Result[LIRExpression].Error("The class " + this.owner.node.Name + " can't be lowered for this target.");

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

    def FieldSet : CompilerObject, ICompileIRStatement {
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

    // Matching arguments to parameters, as a function does.
    def Matching {
        @static fun values(parameters: List[Definition], arguments: Argument[]): List[CompilerObject] {
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

        @static fun parameter(node: Definition, target: Object): LIRParameter {
            return (hirLoader.LoadDefinition(node).Unwrap() as ParameterObject).parameter(target);
        }

        @static fun receiver(node: Receiver): ReceiverObject {
            return hirLoader.LoadDefinition(node).Unwrap() as ReceiverObject;
        }

        @static fun lower(body: Code, function: LIRFunction, returned: CompilerObject, target: Object): Boolean {
            let outer = CodeLoaders.Returns;
            CodeLoaders.Returns = returned;
            let loaded = hirCodeLoader.LoadStatement(body.Block);
            CodeLoaders.Returns = outer;
            if (loaded.IsError) return false;

            let statement = ir.CompileStatement(loaded.Unwrap(), target);
            if (statement.IsError) return false;

            function.Body.Block.Statements.Add(statement.Unwrap());
            return true;
        }
    }

    def MethodObject : CompilerObject {
        let owner: ClassObject;
        let node: Method;
        let lowered: Dictionary[Object, LIRFunction];

        @explicitThis new(this, owner: ClassObject, node: Method) {
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

            signature.Parameters.Add(Matching.receiver(this.node.This).parameter(target));

            let index = 0;
            while (index < this.node.Parameters.Count) {
                signature.Parameters.Add(Matching.parameter(this.node.Parameters[index], target));
                index = index + 1;
            }

            let function = LIRFunction(signature);
            function.Name = this.node.Name;
            function.Body = LIRCode();
            this.lowered[target] = function;

            return function;
        }

        @explicitThis fun lowerBody(this, target: Object): Boolean {
            return Matching.lower(this.node.Body, this.function(target), this.returnType(), target);
        }
    }

    // A method reached through an object: calling it calls the method with the object.
    def BoundMethod : CompilerObject, ICTCallable {
        let method: MethodObject;
        let object: CompilerObject;

        @explicitThis new(this, method: MethodObject, object: CompilerObject) { this.method = method; this.object = object; }

        @explicitThis fun Call(this, arguments: Argument[]): IResult[CompilerObject, Error] {
            let values = Matching.values(this.method.node.Parameters, arguments);
            if (values == null) return Result[CompilerObject].Error(this.method.node.Name + " can't be called with these arguments.");

            values.Insert(0, this.object);
            return Result[CompilerObject].Ok(Invocation(this.method.owner, this.method, null, values));
        }
    }

    def ConstructorObject : CompilerObject, ICTCallable {
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
            let field = 0;
            while (field < structure.Fields.Count) {
                fields.Add(this.owner.defaultValue(field, target));
                field = field + 1;
            }

            function.Body.Block.Statements.Add(SetLocal(receiver, LIRNew(structure, fields)));

            return function;
        }

        @explicitThis fun lowerBody(this, target: Object): Boolean {
            let function = this.function(target);
            if (this.node.Body != null) {
                if (Matching.lower(this.node.Body, function, null, target) == false) return false;
            }

            function.Body.Block.Statements.Add(LIRReturn(GetLocal(Matching.receiver(this.node.This).local(target))));
            return true;
        }

        @explicitThis fun Call(this, arguments: Argument[]): IResult[CompilerObject, Error] {
            let values = Matching.values(this.node.Parameters, arguments);
            if (values == null) return Result[CompilerObject].Error("A constructor of " + this.owner.node.Name + " can't be called with these arguments.");

            return Result[CompilerObject].Ok(Invocation(this.owner, null, this, values));
        }
    }

    // A call of a method or a constructor.
    def Invocation : CompilerObject, ICompileIRExpression, ITyped {
        let owner: ClassObject;
        let method: MethodObject;
        let constructor: ConstructorObject;
        let arguments: List[CompilerObject];

        @explicitThis new(this, owner: ClassObject, method: MethodObject, constructor: ConstructorObject, arguments: List[CompilerObject]) {
            this.owner = owner;
            this.method = method;
            this.constructor = constructor;
            this.arguments = arguments;
        }

        @explicitThis fun get_Type(this): CompilerObject {
            if (this.method != null) return this.method.returnType();
            return this.owner;
        }

        @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
            if (this.owner.lower(target) == false) return Result[LIRExpression].Error("The class " + this.owner.node.Name + " can't be lowered for this target.");

            let function: LIRFunction = null;
            if (this.method != null) function = this.method.function(target);
            else function = this.constructor.function(target);

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

    // A Z# class. As a type, it is the object type its target lowers it to; calling it
    // runs its unnamed constructor, and its named constructors are its members. A value
    // of it has its fields and methods as members.
    def ClassObject : CompilerObject, ICompileIRType, ICTCallable, ICTGetMember[String], IRTGetMember[String], IAssignableTo {
        let node: Class;
        let structures: Dictionary[Object, Structure];
        let modules: Dictionary[Object, LIRModule];
        // Whether the class lowers for each target, which it is taken to while it does,
        // since its functions are declared before any of their code is lowered.
        let lowerings: Dictionary[Object, Boolean];

        @explicitThis new(this, node: Class) {
            this.node = node;
            this.structures = Dictionary[Object, Structure]();
            this.modules = Dictionary[Object, LIRModule]();
            this.lowerings = Dictionary[Object, Boolean]();
        }

        @explicitThis fun IsAssignableTo(this, target: CompilerObject): Boolean {
            if (target is other of ClassObject) return other.node == this.node;
            return false;
        }

        // The object type: a structure the target's polymorphism makes the type of
        // objects, with its fields.
        @explicitThis fun structure(this, target: Object): Structure {
            if (this.structures.ContainsKey(target)) return this.structures[target];

            if (target is polymorphism of Polymorphism) {
                let container = LIRModule(this.node.Name);
                let structure = Structure();
                structure.Name = this.node.Name;
                polymorphism.objects(structure);
                container.Structures.Add(structure);
                this.modules[target] = container;
                this.structures[target] = structure;

                let index = 0;
                while (index < this.node.Fields.Count) {
                    let field = LIRField(ir.CompileType(hirLoader.LoadType(this.node.Fields[index].Type).Unwrap(), target).Unwrap());
                    field.Name = this.node.Fields[index].Name;
                    structure.Fields.Add(field);
                    index = index + 1;
                }

                return structure;
            }

            return null;
        }

        @explicitThis fun defaultValue(this, index: Int32, target: Object): LIRExpression {
            let field = this.node.Fields[index];
            let type = ir.CompileType(hirLoader.LoadType(field.Type).Unwrap(), target).Unwrap();

            if (field.DefaultValue == null) return Constant(type, null);

            let value = cg.ImplicitCast(hirCodeLoader.LoadExpression(field.DefaultValue).Unwrap(), hirLoader.LoadType(field.Type).Unwrap()).Unwrap();
            return ir.CompileExpression(value, target).Unwrap();
        }

        // The class's functions, each declared before any code is lowered, so code can
        // call any of them, and each marked as what it is.
        @explicitThis fun lower(this, target: Object): Boolean {
            if (this.lowerings.ContainsKey(target)) return this.lowerings[target];

            let structure = this.structure(target);
            if (structure == null) return false;

            this.lowerings[target] = true;
            let container = this.modules[target];
            let polymorphism = target as Polymorphism;
            let methods = List[MethodObject]();
            let constructors = List[ConstructorObject]();

            let index = 0;
            while (index < this.node.Methods.Count) {
                let method = this.method(index);
                let function = method.function(target);
                container.Functions.Add(function);
                polymorphism.method(function, structure);
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
                if (methods[index].lowerBody(target) == false) return this.failed(target);
                index = index + 1;
            }

            index = 0;
            while (index < constructors.Count) {
                if (constructors[index].lowerBody(target) == false) return this.failed(target);
                index = index + 1;
            }

            return true;
        }

        @explicitThis fun failed(this, target: Object): Boolean {
            this.lowerings[target] = false;
            return false;
        }

        @explicitThis fun method(this, index: Int32): MethodObject {
            return hirLoader.LoadDefinition(this.node.Methods[index]).Unwrap() as MethodObject;
        }

        @explicitThis fun constructor(this, index: Int32): ConstructorObject {
            return hirLoader.LoadDefinition(this.node.Constructors[index]).Unwrap() as ConstructorObject;
        }

        @explicitThis fun CompileIRType(this, target: Object): IResult[LIRType, Error] {
            let structure = this.structure(target);
            if (structure == null) return Result[LIRType].Error("The class " + this.node.Name + " can't be lowered for a target without polymorphism.");
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

        @explicitThis fun Member(this, name: String): IResult[CompilerObject, Error] {
            let constructor = this.named(name);
            if (constructor == null) return Result[CompilerObject].Error("The class " + this.node.Name + " has no constructor named " + name + ".");
            return Result[CompilerObject].Ok(constructor);
        }

        @explicitThis fun Member(this, object: CompilerObject, name: String): IResult[CompilerObject, Error] {
            let index = 0;
            while (index < this.node.Fields.Count) {
                if (this.node.Fields[index].Name == name) return Result[CompilerObject].Ok(FieldValue(this, object, index));
                index = index + 1;
            }

            index = 0;
            while (index < this.node.Methods.Count) {
                if (this.node.Methods[index].Name == name) return Result[CompilerObject].Ok(BoundMethod(this.method(index), object));
                index = index + 1;
            }

            return Result[CompilerObject].Error("The class " + this.node.Name + " has no member named " + name + ".");
        }
    }
}

// ---- The meta-compilers

// What the members of a class are compiled with: a let is a field, and a fun or a new
// written directly in it is a method or a constructor. A definition inside a member is
// the member's to compile.
def Members {
    let self: Local;
    let fields: List[String];
    let fork: ASTCompiler;
    let expressions: CompileASTExpression;
    let definitions: CompileASTDefinition;
    let owner: Context;

    @explicitThis new(this, self: Local, compiler: ASTCompiler) {
        this.self = self;
        this.fields = List[String]();
        this.fork = compiler.Fork();
        this.expressions = this.fork.Expression;
        this.definitions = this.fork.Definition;
        this.owner = Bodies.DefinitionOf(compiler.Contexts.Current);
    }

    @explicitThis fun expression(this, compiler: ASTCompiler, node: ExpressionSyntax): IResult[Expression, Error] {
        if (node is local of LetSyntax) return this.field(compiler, local);
        return this.expressions(compiler, node);
    }

    @explicitThis fun definition(this, compiler: ASTCompiler, node: DefinitionSyntax): IResult[CompiledDefinition, Error] {
        if (Bodies.DefinitionOf(compiler.Contexts.Current) == this.owner) {
            if (node is function of FunctionSyntax) return metaTypes.Compile(compiler, node, Method as Instance);
            if (node is constructor of ConstructorSyntax) return metaTypes.Compile(compiler, node, Constructor as Instance);
        }
        return this.definitions(compiler, node);
    }

    // A field: code that builds it and adds it to the class. Its default value is code.
    @explicitThis fun field(this, compiler: ASTCompiler, local: LetSyntax): IResult[Expression, Error] {
        if (local.Type == null) return Result[Expression].Error("Field " + local.Name + " needs a type.");

        let type = bodies.Described[Type](compiler, local.Type);
        if (type.IsError) return type;

        let field = Local(local.Name, Field as Type);
        field.Init = Bodies.Construct(Field as Instance, StringLiteral(local.Name), type.Unwrap());

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

        block.Statements.Add(ExpressionStatement(Bodies.Add(this.self, "Fields", Get(field))));
        block.Result = Get(field);
        this.fields.Add(local.Name);

        return Result[Expression].Ok(block);
    }

    @explicitThis fun register(this, member: CompiledDefinition, value: Expression): Statement {
        let list = "Methods";
        if (member.UnderlyingType == Constructor as Instance) list = "Constructors";
        return ExpressionStatement(Bodies.Add(this.self, list, value));
    }
}

// What a method or constructor's code is compiled with: the object it runs on, which is
// `this` if its first parameter says so, and the class's fields by name, unless a
// parameter has the name.
def Receivers {
    @static fun receiver(compiler: ASTCompiler, block: Block, self: Local, members: Members, signature: SignatureSyntax): SignatureSyntax {
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
        while (index < members.fields.Count) {
            let name = members.fields[index];
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

    @static fun count(parameters: List[ParameterSyntax]): Int32 {
        if (parameters == null) return 0;
        return parameters.Count;
    }
}

def MethodFiller {
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
            let type = bodies.Described[Type](this.compiler, this.function.ReturnType);
            if (type.IsError) return Result[Block].Error(type.UnwrapError());

            let set = Set();
            set.Target = Bodies.Member(this.self, "ReturnType");
            set.Value = type.Unwrap();
            block.Statements.Add(ExpressionStatement(set));
        }

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

// A class's members are compiled within a context that is its members, which is how
// the meta-compiler of a method or constructor in it finds them.
def ClassFiller {
    let compiler: ASTCompiler;
    let definition: TypeSyntax;
    let self: Local;
    let members: Members;

    @explicitThis new(this, compiler: ASTCompiler, definition: TypeSyntax, self: Local) {
        this.compiler = compiler;
        this.definition = definition;
        this.self = self;
    }

    @explicitThis fun fill(this): IResult[Block, Error] {
        if (this.definition.Content == null) return Result[Block].Ok(Block([]));

        this.members = Members(this.self, this.compiler);
        this.members.fork.Expression = this.members.expression;
        this.members.fork.Definition = this.members.definition;

        return this.compiler.Contexts.Within[IResult[Block, Error]](CapabilityContext(this.compiler.Contexts.Current, this.members), this.compileMembers);
    }

    @explicitThis fun compileMembers(this): IResult[Block, Error] {
        return ContainerCompiler(language).Compile(this.members.fork, this.definition.Content.Statements, this.members.register);
    }
}

module Meta {
    // The members of the class being compiled, from the context they are compiled in.
    fun members(compiler: ASTCompiler): Members {
        let members: Members = null;
        compiler.Contexts.Has[Members](members);
        return members;
    }

    fun compileClass(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
        if (definition is type of TypeSyntax) {
            let filler = ClassFiller(compiler, type, self);
            return Result[CompiledDefinition].Ok(CompiledDefinition(self, Bodies.Construct(Class as Instance, StringLiteral(type.Name)), Class as Instance, filler.fill, null));
        }
        return Result[CompiledDefinition].Error("A class is a type definition.");
    }

    fun compileMethod(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
        if (definition is function of FunctionSyntax) {
            let filler = MethodFiller(compiler, function, self, Meta.members(compiler));
            return Result[CompiledDefinition].Ok(CompiledDefinition(self, Bodies.Construct(Method as Instance, StringLiteral(function.Name)), Method as Instance, filler.fill, null));
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

    fun loadClass(node: Class): IResult[CompilerObject, Error] { return Result[CompilerObject].Ok(Objects.ClassObject(node)); }

    fun loadMethod(node: Method): IResult[CompilerObject, Error] {
        let owner = hirLoader.LoadDefinition(node.This.Owner as Definition);
        if (owner.IsError) return owner;
        return Result[CompilerObject].Ok(Objects.MethodObject(owner.Unwrap() as Objects.ClassObject, node));
    }

    fun loadConstructor(node: Constructor): IResult[CompilerObject, Error] {
        let owner = hirLoader.LoadDefinition(node.This.Owner as Definition);
        if (owner.IsError) return owner;
        return Result[CompilerObject].Ok(Objects.ConstructorObject(owner.Unwrap() as Objects.ClassObject, node));
    }

    fun loadReceiver(node: Receiver): IResult[CompilerObject, Error] { return Result[CompilerObject].Ok(Objects.ReceiverObject(node)); }

    // A node used as an instance, or as a type, is the one CO its definition gives.
    fun instanceOfClass(node: Class): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
    fun typeOfClass(node: Class): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
    fun instanceOfMethod(node: Method): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
    fun instanceOfConstructor(node: Constructor): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
    fun instanceOfReceiver(node: Receiver): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
}

// ---- Installing it

mappedLoader.Definitions.Register[Class](Meta.loadClass);
mappedLoader.Definitions.Register[Method](Meta.loadMethod);
mappedLoader.Definitions.Register[Constructor](Meta.loadConstructor);
mappedLoader.Definitions.Register[Receiver](Meta.loadReceiver);
mappedLoader.Instances.Register[Class](Meta.instanceOfClass);
mappedLoader.Instances.Register[Method](Meta.instanceOfMethod);
mappedLoader.Instances.Register[Constructor](Meta.instanceOfConstructor);
mappedLoader.Instances.Register[Receiver](Meta.instanceOfReceiver);
mappedLoader.Types.Register[Class](Meta.typeOfClass);

metaTypes.Register(Class as Instance, Meta.compileClass);
metaTypes.Register(Method as Instance, Meta.compileMethod);
metaTypes.Register(Constructor as Instance, Meta.compileConstructor);
metaTypes.Default[TypeSyntax](Class as Instance);
