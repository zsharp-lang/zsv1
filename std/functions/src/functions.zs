// std:functions — Z#'s own functions, defined in Z#.
//
// A Z# function is a function of its own, not a method of anything. This document
// defines its HIR node, the meta-compiler that compiles `fun` to it, and the loader that
// makes it a CO, and makes it what `fun` is wherever nothing else compiles one.

import { String, Object, Boolean, Int32, List, Dictionary, explicitThis, DotNet_Class, DotNet_Module } from "core:runtime";
import { Instance, Definition, Variable, Type, Named as NamedDefinition, Annotated, Overloadable } from "std:zsharp/hir";
import { Code, Block, Local, Expression, ExpressionStatement, Set, StringLiteral, Get } from "std:zsharp/hir/code";
import { Function as FunctionSyntax, Definition as DefinitionSyntax } from "std:zsharp/lang/ast";
import { CompilerObject } from "std:zsharp/mir";
import { Error, Result, IResult } from "std:zsharp/common";
import { Argument, ArgumentStream } from "std:zsharp/compiler/cg";
import { ICTCallable, ICTMatchParameter, ICTCastTo } from "std:zsharp/compiler/cg/direct";
import { ICompileIRExpression, ICompileIRDefinitionIn, ICompileIRReference } from "std:zsharp/compiler/ir";
import { ITyped } from "std:zsharp/compiler/ts";
import { FunctionHandle, Constant } from "std:zsharp/lir";
import { ASTCompiler, CompiledDefinition } from "std:zsharp/compiler/ast";
import { Function as LIRFunction, Parameter as LIRParameter, Signature, Call as LIRCall, GetLocal, Expression as LIRExpression, Statement as LIRStatement, Code as LIRCode, Module as LIRModule, ICallable } from "std:zsharp/lir";
import { Bodies } from "std:zsharp/lang";
import { CodeLoaders } from "std:zsharp/compiler/code";
import { Holder, Held, keep } from "std:zsharp/lowering";
import { metaTypes, bodies } from "core:zsharp";
import { entrypoint, definitionLoader, instanceLoader, metaType, binder } from "std:framework";
import { ilLoader } from "core:runtime";
import { cg, ir, ts, hirLoader, hirCodeLoader, mappedLoader } from "core:compiler";

// ---- The HIR

// A Z# function. Its meta-compiler fills it through bodies, by these names. What is
// said of it, as a decorator says it is an entry point, is its annotations. Functions of
// one name overload.
def Function of DotNet_Class : Instance, Definition, NamedDefinition, Annotated, Overloadable {
    let name: String;
    let Parameters: List[Definition];
    let Locals: List[Local];
    let ReturnType: Type;
    let Body: Code;
    let annotations: List[Object];

    @explicitThis new(this, name: String) {
        this.name = name;
        this.Parameters = List[Definition]();
        this.Locals = List[Local]();
        this.annotations = List[Object]();
    }

    @explicitThis get Name(this): String { return this.name; }

    @explicitThis get Annotations(this): List[Object] { return this.annotations; }
}

// A parameter that a positional argument fills, or else its default value.
def Positional of DotNet_Class : Instance, Definition, Variable {
    let Name: String;
    let Type: Type;
    let DefaultValue: Expression;

    @explicitThis new(this, name: String) { this.Name = name; }
    @explicitThis new(this, name: String, type: Type) { this.Name = name; this.Type = type; }
}

// A parameter that an argument with its name fills, or else its default value.
def Named of DotNet_Class : Instance, Definition, Variable {
    let Name: String;
    let Type: Type;
    let DefaultValue: Expression;

    @explicitThis new(this, name: String) { this.Name = name; }
    @explicitThis new(this, name: String, type: Type) { this.Name = name; this.Type = type; }
}

// ---- The COs

// A parameter, as a call matches an argument to it and as its function's code uses it.
// One CO is both, and it owns the LIR parameter the signature holds for each target, so
// whatever lowers a function with it refers to it as that parameter.
def ParameterObject of DotNet_Class : CompilerObject, ICTMatchParameter, ICompileIRExpression, ICompileIRReference[LIRParameter], ITyped {
    let name: String;
    let named: Boolean;
    let type: Type;
    let defaultValue: Expression;
    let lowered: Dictionary[Object, LIRParameter];

    @explicitThis new(this, name: String, named: Boolean, type: Type, defaultValue: Expression) {
        this.name = name;
        this.named = named;
        this.type = type;
        this.defaultValue = defaultValue;
        this.lowered = Dictionary[Object, LIRParameter]();
    }

    @explicitThis fun get_Type(this): CompilerObject {
        return hirLoader.LoadType(this.type).Unwrap();
    }

    @explicitThis fun parameter(this, target: Object): LIRParameter {
        if (this.lowered.ContainsKey(target)) return this.lowered[target];

        let type = ir.CompileType(this.get_Type(), target).Unwrap();
        // Its type can lower what asks for it, which made it first.
        if (this.lowered.ContainsKey(target)) return this.lowered[target];

        let parameter = LIRParameter(type);
        parameter.Name = this.name;
        this.lowered[target] = parameter;

        return parameter;
    }

    @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        return Result[LIRExpression].Ok(GetLocal(this.parameter(target)));
    }

    // It is referred to as the LIR parameter it is for the target, which a signature holds.
    @explicitThis fun CompileIRReference(this, target: Object): IResult[LIRParameter, Error] {
        return Result[LIRParameter].Ok(this.parameter(target));
    }

    // The argument with its name, or the next positional one, or else the default.
    @explicitThis fun Match(this, arguments: ArgumentStream): IResult[CompilerObject, Error] {
        let argument: CompilerObject = null;

        if (this.named) argument = arguments.Pop(this.name);
        else argument = arguments.Pop();

        if (argument == null) {
            if (this.defaultValue == null) return Result[CompilerObject].Error("No argument is given for parameter " + this.name + ".");
            argument = hirCodeLoader.LoadExpression(this.defaultValue).Unwrap();
        }

        return cg.ImplicitCast(argument, this.get_Type());
    }
}

// What each declared function, one with no code of its own, is premapped to: a
// definition it lowers as, wherever it is called.
let premapped: Dictionary[Function, Instance] = Dictionary[Function, Instance]();

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

// A Z# function as a CO: calling it matches the arguments to its parameters, and it
// is lowered once for each target, to an LIR function of its own: a function of what
// holds it, as a module, if something does.
def FunctionObject of DotNet_Class : CompilerObject, ICTCallable, ICTCastTo, Held, ICompileIRDefinitionIn[LIRModule], ICompileIRReference[ICallable] {
    let node: Function;
    let holder: Holder;
    let lowered: Dictionary[Object, LIRFunction];
    let handles: Dictionary[Object, FunctionHandle];
    // Why its code didn't lower for each target it didn't.
    let failures: Dictionary[Object, Error];

    @explicitThis new(this, node: Function) {
        this.node = node;
        this.handles = Dictionary[Object, FunctionHandle]();
        this.lowered = Dictionary[Object, LIRFunction]();
        this.failures = Dictionary[Object, Error]();
    }

    @explicitThis fun hold(this, holder: Holder) { this.holder = holder; }

    // It is referred to as what a call of it calls, as a delegate of it is.
    @explicitThis fun CompileIRReference(this, target: Object): IResult[ICallable, Error] {
        return this.callable(target);
    }

    @explicitThis fun CompileIRDefinition(this, owner: LIRModule, target: Object): Error {
        if (this.declared()) return null;
        if (this.function(target) != null) return null;
        return this.failures[target];
    }

    // A function with no code of its own is declared: what the platform provides, or
    // what it is premapped to.
    @explicitThis fun declared(this): Boolean { return this.node.Body == null; }

    // What a call of it calls, for a target: what it is premapped to, or a declared
    // function's handle, which the platform maps, or its own function.
    @explicitThis fun callable(this, target: Object): IResult[ICallable, Error] {
        if (premapped.ContainsKey(this.node)) {
            let to = hirLoader.LoadInstance(premapped[this.node]);
            if (to.IsError) return Result[ICallable].Error(to.UnwrapError());
            return ir.CompileReference[ICallable](to.Unwrap(), target);
        }

        if (this.declared()) {
            if (this.handles.ContainsKey(target) == false) this.handles[target] = FunctionHandle(this.signature(target));
            return Result[ICallable].Ok(this.handles[target]);
        }

        let function = this.function(target);
        if (function != null) return Result[ICallable].Ok(function);
        if (this.failures.ContainsKey(target)) return Result[ICallable].Error(this.failures[target]);
        return Result[ICallable].Error("The code of " + this.node.Name + " can't be lowered.");
    }

    @explicitThis fun signature(this, target: Object): Signature {
        let signature = Signature();
        let returned = this.returnType();
        if (returned != null) signature.ReturnType = ir.CompileType(returned, target).Unwrap();

        let index = 0;
        while (index < this.node.Parameters.Count) {
            signature.Parameters.Add(this.parameter(index).parameter(target));
            index = index + 1;
        }
        return signature;
    }

    // What describes the function is its node, as code compiled where it is knows it.
    @explicitThis fun Cast(this, type: CompilerObject): IResult[CompilerObject, Error] {
        let described = hirLoader.LoadType(Function as Type).Unwrap();
        if (ts.IsAssignableTo(type, described)) return Result[CompilerObject].Ok(Known(this.node, described));
        return Result[CompilerObject].Error("A function is only described by its node.");
    }

    @explicitThis fun parameter(this, index: Int32): ParameterObject {
        return hirLoader.LoadDefinition(this.node.Parameters[index]).Unwrap() as ParameterObject;
    }

    @explicitThis fun returnType(this): CompilerObject {
        if (this.node.ReturnType == null) return null;
        return hirLoader.LoadType(this.node.ReturnType).Unwrap();
    }

    @explicitThis fun function(this, target: Object): LIRFunction {
        if (this.lowered.ContainsKey(target)) return this.lowered[target];

        // What holds it lowers it with the rest of what it holds.
        let container: LIRModule = null;
        if (this.holder != null) {
            container = this.holder.module(target);
            if (this.lowered.ContainsKey(target)) return this.lowered[target];
        }

        let signature = Signature();
        let returned = this.returnType();

        if (returned != null) signature.ReturnType = ir.CompileType(returned, target).Unwrap();

        let index = 0;
        while (index < this.node.Parameters.Count) {
            signature.Parameters.Add(this.parameter(index).parameter(target));
            index = index + 1;
        }

        // It is known before its code is lowered, so its code can call it.
        let function = LIRFunction(signature);
        keep(this.node, function.Metadata);
        function.Name = this.node.Name;
        this.lowered[target] = function;
        if (container != null) container.Functions.Add(function);

        // Its code is loaded knowing what it returns, so a return converts its value to
        // that, and compiled into the function.
        // Its parameters are what its code uses them as, while its code is lowered.
        let defined = this.defineParameters();
        let outer = CodeLoaders.Returns;
        CodeLoaders.Returns = returned;
        let loaded = hirCodeLoader.LoadStatement(this.node.Body.Block);
        CodeLoaders.Returns = outer;

        let statement: IResult[LIRStatement, Error] = null;
        if (loaded.IsOk) statement = ir.CompileStatement(loaded.Unwrap(), target);
        this.restoreParameters(defined);

        if (loaded.IsError) {
            this.lowered.Remove(target);
            if (container != null) container.Functions.Remove(function);
            this.failures[target] = loaded.UnwrapError();
            return null;
        }

        if (statement.IsError) {
            this.lowered.Remove(target);
            if (container != null) container.Functions.Remove(function);
            this.failures[target] = statement.UnwrapError();
            return null;
        }

        function.Body = LIRCode();
        function.Body.Block.Statements.Add(statement.Unwrap());

        return function;
    }

    // Defines each of its parameters as its code uses it, and gives what each was before.
    @explicitThis fun defineParameters(this): List[CompilerObject] {
        let previous = List[CompilerObject]();
        let index = 0;
        while (index < this.node.Parameters.Count) {
            previous.Add(CodeLoaders.Define(this.node.Parameters[index] as Instance, this.parameter(index)));
            index = index + 1;
        }
        return previous;
    }

    @explicitThis fun restoreParameters(this, previous: List[CompilerObject]) {
        let index = 0;
        while (index < this.node.Parameters.Count) {
            CodeLoaders.Restore(this.node.Parameters[index] as Instance, previous[index]);
            index = index + 1;
        }
    }

    @explicitThis fun Call(this, arguments: Argument[]): IResult[CompilerObject, Error] {
        let stream = ArgumentStream(arguments);
        let values = List[CompilerObject]();

        let index = 0;
        while (index < this.node.Parameters.Count) {
            let value = cg.MatchParameter(this.parameter(index), stream);
            if (value.IsError) return value;
            values.Add(value.Unwrap());
            index = index + 1;
        }

        if (stream.HasArguments) return Result[CompilerObject].Error(this.node.Name + " is given too many arguments.");

        return Result[CompilerObject].Ok(FunctionCall(this, values));
    }
}

// A call of a Z# function.
def FunctionCall of DotNet_Class : CompilerObject, ICompileIRExpression, ITyped {
    let callee: FunctionObject;
    let arguments: List[CompilerObject];

    @explicitThis new(this, callee: FunctionObject, arguments: List[CompilerObject]) {
        this.callee = callee;
        this.arguments = arguments;
    }

    @explicitThis fun get_Type(this): CompilerObject { return this.callee.returnType(); }

    @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        let called = this.callee.callable(target);
        if (called.IsError) return Result[LIRExpression].Error(called.UnwrapError());
        let function = called.Unwrap();

        let values = List[LIRExpression]();

        let index = 0;
        while (index < this.arguments.Count) {
            let value = ir.CompileExpression(this.arguments[index], target);
            if (value.IsError) return Result[LIRExpression].Error(value.UnwrapError());
            values.Add(value.Unwrap());
            index = index + 1;
        }

        return Result[LIRExpression].Ok(LIRCall(function, values));
    }
}

// ---- The meta-compiler

// What fills a function as it is built: its parameters, what it returns, and its code.
def Filler of DotNet_Class {
    let compiler: ASTCompiler;
    let function: FunctionSyntax;
    let self: Local;

    @explicitThis new(this, compiler: ASTCompiler, function: FunctionSyntax, self: Local) {
        this.compiler = compiler;
        this.function = function;
        this.self = self;
    }

    @explicitThis fun fill(this): IResult[Block, Error] {
        let block = Block([]);

        let invalid = bodies.Parameters(this.compiler, block, this.self, "Parameters", this.function.Signature, Positional as Instance, Named as Instance);
        if (invalid != null) return Result[Block].Error(invalid);

        if (this.function.ReturnType != null) {
            let type = this.compiler.CompileExpression(this.function.ReturnType);
            if (type.IsError) return Result[Block].Error(type.UnwrapError());

            let set = Set();
            set.Target = Bodies.Member(this.self, "ReturnType");
            set.Value = type.Unwrap();
            block.Statements.Add(ExpressionStatement(set));
        }

        // A function with no body is declared, and has no code of its own.
        if (this.function.Body != null) {
            let failed = bodies.Body(bodies.WithLocals(this.compiler, block, this.self, "Locals", null), block, this.self, this.function.Body);
            if (failed != null) return Result[Block].Error(failed);
        }

        return Result[Block].Ok(block);
    }
}

@metaType(Function)
fun compileFunction(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
    if (definition is function of FunctionSyntax) {
        let filler = Filler(compiler, function, self);

        return Result[CompiledDefinition].Ok(CompiledDefinition(
            self,
            Bodies.Construct(Function as Instance, StringLiteral(function.Name)),
            Function as Instance,
            filler.fill,
            null
        ));
    }

    return Result[CompiledDefinition].Error("A Z# function is compiled from a fun.");
}

@definitionLoader
fun loadFunction(node: Function): IResult[CompilerObject, Error] {
    return Result[CompilerObject].Ok(FunctionObject(node));
}

@definitionLoader
fun loadPositional(node: Positional): IResult[CompilerObject, Error] {
    return Result[CompilerObject].Ok(ParameterObject(node.Name, false, node.Type, node.DefaultValue));
}

@definitionLoader
fun loadNamed(node: Named): IResult[CompilerObject, Error] {
    return Result[CompilerObject].Ok(ParameterObject(node.Name, true, node.Type, node.DefaultValue));
}

// A node used as an instance is the one CO its definition gives.
@instanceLoader
fun instanceOfFunction(node: Function): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
// A parameter in code is what the code's function defines it as, as it lowers the code:
// nothing else gives it a meaning there.
@instanceLoader
fun instanceOfPositional(node: Positional): IResult[CompilerObject, Error] { return CodeLoaders.Variable(node); }
@instanceLoader
fun instanceOfNamed(node: Named): IResult[CompilerObject, Error] { return CodeLoaders.Variable(node); }

// ---- Installing it

// `fun` is a Z# function wherever nothing else compiles one.
@entrypoint
fun install() { metaTypes.Default[FunctionSyntax](Function as Instance); }

// Makes a declared function lower as a definition, wherever it is called: what provides
// it, as the prelude makes `reflect` the IL loader's description of a type.
fun premap(declared: Instance, to: Instance) { premapped[declared as Function] = to; }

// A function that was built is premapped onto the method it was built as.
@binder
fun bindFunction(node: Function, entity: Object): Error {
    premapped[node] = ilLoader.Load(entity as MethodInfo);
    return null;
}
