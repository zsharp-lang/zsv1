// std:modules/compiler — what makes std:modules' definitions do what they mean.
//
// The CO of a module finds its members by the names the `Named` capability gives them,
// and holds them: it lowers, for each target, to an LIR module of its own, as LIR has no
// submodules, and lowers each of its members into it. The target realizes that module:
// the CLR realizes it as a module. A `let` in a module is a global, an import runs as the
// code that builds the module runs, and a definition in it is compiled by its own
// meta-type.
//
// std:modules is their structure, which reflection needs alone; this is what the
// compiler needs of them. Importing it makes `module` a Z# module wherever nothing else
// compiles one, as `module` is `def` of Module.

import { String, Object, Boolean, Int32, List, Dictionary, explicitThis } from "core:runtime";
import { Class as DotNet_Class, Module as DotNet_Module } from "pkg:clr/hir";
import { static, public } from "pkg:clr/hir";
import { Instance, Definition, Variable, Type, Named as NamedDefinition, Annotated } from "pkg:zsharp/hir";
import { Call } from "pkg:zsharp/hir/code";
import { Block, Local, Expression, ExpressionStatement, Set, StringLiteral, Get, Declare, BlockExpression, Statement, Cast } from "pkg:zsharp/hir/code";
import {
    Module as ModuleSyntax, TypeDefinition as TypeSyntax, Definition as DefinitionSyntax, LetExpression as LetSyntax,
    Expression as ExpressionSyntax, Statement as StatementSyntax, ImportStatement as ImportSyntax
} from "pkg:zsharp/lang/ast";
import { CompilerObject } from "pkg:zsharp/mir";
import { Error, Result, IResult } from "pkg:zsharp/core";
import { UnsupportedError } from "core:zsharp/mir/core";
import { ICTGetMember, ICTSet } from "core:zsharp/mir/cg/direct";
import { ICompileIRExpression, ICompileIRStatement, ICompileIRDefinitionIn } from "core:zsharp/m2l";
import { ITyped } from "core:zsharp/mir/ts";
import { ASTCompiler, CompiledDefinition, CompileASTExpression, CompileASTStatement, CompileASTDefinition } from "core:zsharp/lang";
import {
    Module as LIRModule, Global as LIRGlobal, GetGlobal, SetGlobal,
    Expression as LIRExpression, Statement as LIRStatement, Function as LIRFunction, ICallable, EntryPoint as LIREntryPoint
} from "pkg:zsharp/lir";
import { Bodies, ContainerCompiler } from "pkg:zsharp/lang/hir";
import { Holder, Held, Initializing, keep } from "std:zsharp/lowering";
import { metaTypes, bodies, language, ct } from "core:zsharp";
import { entrypoint, definitionLoader, instanceLoader, metaType, binder } from "std:framework";
import { ilLoader } from "core:runtime";
import { cg, ir, ts, hirLoader, hirCodeLoader, mappedLoader, overloading } from "core:compiler";
import { Global, Module } from "std:modules";

// ---- The COs

// A global, held by its module: what holds it lowers it into its LIR module, in its
// order, and gives it its initial value in that module's initializer.
@public def GlobalObject of DotNet_Class : CompilerObject, ICompileIRExpression, ICTSet, ITyped, Held, ICompileIRDefinitionIn[LIRModule] {
    @public let node: Global;
    @public let holder: Holder;
    @public let globals: Dictionary[Object, LIRGlobal];

    @public @explicitThis new(this, node: Global) {
        this.node = node;
        this.globals = Dictionary[Object, LIRGlobal]();
    }

    @public @explicitThis fun hold(this, holder: Holder) { this.holder = holder; }

    @public @explicitThis fun initializer(this): CompilerObject {
        return hirCodeLoader.LoadExpression(this.node.Initializer).Unwrap();
    }

    @public @explicitThis fun get_Type(this): CompilerObject {
        if (this.node.Type != null) return hirLoader.LoadType(this.node.Type).Unwrap();
        return ts.TypeOf(this.initializer()).Unwrap();
    }

    @public @explicitThis fun CompileIRDefinition(this, owner: LIRModule, target: Object): Error {
        if (bound.ContainsKey(this.node)) return null;
        if (this.globals.ContainsKey(target)) return null;

        let type = this.get_Type();
        let global = LIRGlobal(ir.CompileType(type, target).Unwrap());
        global.Name = this.node.Name;
        keep(this.node, global.Metadata);
        owner.Globals.Add(global);
        this.globals[target] = global;

        if (this.node.Initializer != null) {
            let value = cg.ImplicitCast(this.initializer(), type);
            if (value.IsError) return value.UnwrapError();

            let lowered = ir.CompileExpression(value.Unwrap(), target);
            if (lowered.IsError) return lowered.UnwrapError();

            Initializing.into(owner, SetGlobal(global, lowered.Unwrap()));
        }

        return null;
    }

    // The global for the target, which what holds it lowers.
    @public @explicitThis fun global(this, target: Object): LIRGlobal {
        if (this.globals.ContainsKey(target) == false) this.holder.module(target);
        return this.globals[target];
    }

    // A global that was built is what it was built as.
    @public @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        if (bound.ContainsKey(this.node)) return ir.CompileExpression(bound[this.node], target);
        return Result[LIRExpression].Ok(GetGlobal(this.global(target)));
    }

    @public @explicitThis fun Set(this, value: CompilerObject): IResult[CompilerObject, Error] {
        if (bound.ContainsKey(this.node)) return cg.Set(bound[this.node], value);
        let converted = cg.ImplicitCast(value, this.get_Type());
        if (converted.IsError) return converted;

        return Result[CompilerObject].Ok(GlobalSet(this, converted.Unwrap()));
    }
}

@public def GlobalSet of DotNet_Class : CompilerObject, ICompileIRStatement {
    @public let global: GlobalObject;
    @public let value: CompilerObject;

    @public @explicitThis new(this, global: GlobalObject, value: CompilerObject) { this.global = global; this.value = value; }

    @public @explicitThis fun CompileIRStatement(this, target: Object): IResult[LIRStatement, Error] {
        let value = ir.CompileExpression(this.value, target);
        if (value.IsError) return Result[LIRStatement].Error(value.UnwrapError());

        return Result[LIRStatement].Ok(SetGlobal(this.global.global(target), value.Unwrap()));
    }
}

// A Z# module: it holds its members, which it finds by their names, and lowers them
// into its LIR module, its globals first, in their order.
@public def ModuleObject of DotNet_Class : CompilerObject, Holder, ICTGetMember[String] {
    @public let node: Module;
    @public let members: Dictionary[String, List[Definition]];
    @public let modules: Dictionary[Object, LIRModule];
    @public let failures: List[String];

    @public @explicitThis new(this, node: Module) {
        this.node = node;
        this.members = Dictionary[String, List[Definition]]();
        this.modules = Dictionary[Object, LIRModule]();
        this.failures = List[String]();
    }

    // Finds its members by their names, and binds the COs of those it holds to itself.
    @public @explicitThis fun bind(this) {
        let index = 0;
        while (index < this.node.Globals.Count) {
            this.add(this.node.Globals[index]);
            index = index + 1;
        }

        index = 0;
        while (index < this.node.Definitions.Count) {
            this.add(this.node.Definitions[index]);
            index = index + 1;
        }
    }

    @public @explicitThis fun add(this, member: Definition) {
        if (member is let named: NamedDefinition) {
            if (this.members.ContainsKey(named.Name) == false) this.members[named.Name] = List[Definition]();
            this.members[named.Name].Add(member);
        }

        let loaded = hirLoader.LoadDefinition(member);
        if (loaded.IsError) return;
        if (loaded.Unwrap() is let held: Held) held.hold(this);
    }

    @public @explicitThis fun module(this, target: Object): LIRModule {
        if (this.modules.ContainsKey(target)) return this.modules[target];

        let lowered = LIRModule(this.node.Name);
        keep(this.node, lowered.Metadata);
        this.modules[target] = lowered;

        let index = 0;
        while (index < this.node.Globals.Count) {
            this.lower(this.node.Globals[index], lowered, target);
            index = index + 1;
        }

        index = 0;
        while (index < this.node.Definitions.Count) {
            this.lower(this.node.Definitions[index], lowered, target);
            index = index + 1;
        }

        // The entry point is one of its functions, which the target calls as it imports
        // the module.
        if (this.node.EntryPoint != null) {
            let entry = ir.CompileReference[ICallable](hirLoader.LoadInstance(this.node.EntryPoint).Unwrap(), target);
            if (entry.IsOk) {
                if (entry.Unwrap() is let function: LIRFunction) lowered.Metadata.Add(LIREntryPoint(function));
            }
        }

        return lowered;
    }

    // A member that can't be lowered into a module, as a definition of another
    // platform's, lowers by itself; any other failure is the member's to report where
    // it is used.
    @public @explicitThis fun lower(this, member: Definition, lowered: LIRModule, target: Object) {
        let name = "a definition";
        if (member is let named: NamedDefinition) name = named.Name;

        let loaded = hirLoader.LoadDefinition(member);
        if (loaded.IsError) {
            this.failures.Add(name + ": " + loaded.UnwrapError());
            return;
        }

        let failed = ir.CompileDefinition[LIRModule](loaded.Unwrap(), lowered, target);
        if (failed != null) this.failures.Add(name + ": " + failed);
    }

    @public @explicitThis fun Member(this, name: String): IResult[CompilerObject, Error] {
        // A name of several definitions, as overloads, is all of them.
        if (this.members.ContainsKey(name)) {
            let named = this.members[name];
            if (named.Count == 1) return hirLoader.LoadDefinition(named[0]);

            let loaded = List[CompilerObject]();
            let index = 0;
            while (index < named.Count) {
                let each = hirLoader.LoadDefinition(named[index]);
                if (each.IsError) return each;
                loaded.Add(each.Unwrap());
                index = index + 1;
            }
            return overloading.Group(loaded.ToArray());
        }
        return Result[CompilerObject].Error("Module " + this.node.Name + " has no member named " + name + ".");
    }
}

// ---- The meta-compiler

// What the members of a module are compiled with: a let is a global, bound by its name,
// an import runs as the module is built, and a definition is compiled by its own
// meta-type.
@public def Members of DotNet_Class {
    @public let self: Local;
    @public let fork: ASTCompiler;
    @public let expressions: CompileASTExpression;
    @public let statements: CompileASTStatement;

    @public @explicitThis new(this, self: Local, compiler: ASTCompiler) {
        this.self = self;
        this.fork = compiler.Fork();
        this.expressions = this.fork.Expression;
        this.statements = this.fork.Statement;
    }

    @public @explicitThis fun expression(this, compiler: ASTCompiler, node: ExpressionSyntax): IResult[Expression, Error] {
        if (node is let local: LetSyntax) return this.global(compiler, local);
        return this.expressions(compiler, node);
    }

    @public @explicitThis fun statement(this, compiler: ASTCompiler, node: StatementSyntax): IResult[Statement, Error] {
        if (node is let imported: ImportSyntax) {
            let holder = bodies.Imported(compiler, imported);
            if (holder.IsError) return Result[Statement].Error(holder.UnwrapError());
            if (holder.Unwrap() == null) return Result[Statement].Ok(Block([]));
            return Result[Statement].Ok(Declare(holder.Unwrap()));
        }
        return this.statements(compiler, node);
    }

    // A global: code that builds it and adds it to the module. Its initializer is code.
    @public @explicitThis fun global(this, compiler: ASTCompiler, local: LetSyntax): IResult[Expression, Error] {
        let global = Local(local.Name, Global as Type);
        global.Init = Bodies.Construct(Global as Instance, StringLiteral(local.Name));

        let block = BlockExpression();
        block.Statements.Add(Declare(global));

        if (local.Type != null) {
            let type = compiler.CompileExpression(local.Type);
            if (type.IsError) return type;

            let set = Set();
            set.Target = Bodies.Member(global, "Type");
            set.Value = type.Unwrap();
            block.Statements.Add(ExpressionStatement(set));
        }

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
            set.Target = Bodies.Member(global, "Initializer");
            set.Value = value.Unwrap();
            block.Statements.Add(ExpressionStatement(set));
        }

        block.Statements.Add(ExpressionStatement(Bodies.Add(this.self, "Globals", Get(global))));
        block.Result = Get(global);

        let invalid = language.Bind(compiler, local.Name, Get(global));
        if (invalid != null) return Result[Expression].Error(invalid);

        return Result[Expression].Ok(block);
    }

    // What its decorators gave is the kind of definition it is, as a class's member is.
    @public @explicitThis fun register(this, member: CompiledDefinition, value: Expression): Statement {
        let kind = Cast();
        kind.Value = value;
        kind.Type = Get(member.UnderlyingType);
        return ExpressionStatement(Bodies.Add(this.self, "Definitions", kind));
    }

}

@public def Filler of DotNet_Class {
    @public let compiler: ASTCompiler;
    @public let body: List[StatementSyntax];
    @public let self: Local;

    @public @explicitThis new(this, compiler: ASTCompiler, body: List[StatementSyntax], self: Local) {
        this.compiler = compiler;
        this.body = body;
        this.self = self;
    }

    @public @explicitThis fun fill(this): IResult[Block, Error] {
        let members = Members(this.self, this.compiler);
        members.fork.Expression = members.expression;
        members.fork.Statement = members.statement;

        return ContainerCompiler(language).Compile(members.fork, this.body, members.register);
    }
}

// The CO of each module loaded: a module is an instance, which its loader keeps one
// CO of, as its members are bound to it.
let loaded: Dictionary[Module, ModuleObject] = Dictionary[Module, ModuleObject]();

// The modules entered in the running process, each once.
let entered: List[Module] = List[Module]();

// A module is any definition with a body: `module Name { ... }`, or `def Name of
// Module { ... }`.
@metaType(Module)
fun compileModule(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
    let name: String = null;
    let body: List[StatementSyntax] = null;

    if (definition is let node: ModuleSyntax) {
        name = node.Name;
        body = node.Body;
    }
    if (definition is let type: TypeSyntax) {
        name = type.Name;
        body = List[StatementSyntax]();
        if (type.Content != null) body = type.Content.Statements;
    }
    if (body == null) return Result[CompiledDefinition].Error("A Z# module is a definition with a body.");

    let filler = Filler(compiler, body, self);
    return Result[CompiledDefinition].Ok(CompiledDefinition(self, Bodies.Construct(Module as Instance, StringLiteral(name)), Module as Instance, filler.fill, null));
}

@instanceLoader
fun loadModule(node: Module): IResult[CompilerObject, Error] {
    if (loaded.ContainsKey(node)) return Result[CompilerObject].Ok(loaded[node]);

    let created = ModuleObject(node);
    loaded[node] = created;
    created.bind();
    return Result[CompilerObject].Ok(created);
}

// A global is what it means once it is loaded: it has a type, or a value to have the
// type of.
@definitionLoader
fun loadGlobal(node: Global): IResult[CompilerObject, Error] {
    if (node.Type == null) {
        if (node.Initializer == null) return Result[CompilerObject].Error("Global " + node.Name + " needs a type or a value.");
    }
    return Result[CompilerObject].Ok(GlobalObject(node));
}

// A global used as an instance is the one CO its definition gives.
@instanceLoader
fun instanceOfGlobal(node: Global): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }

// The globals that were built, as what each was built as: the static member of the class
// its module was built as.
let bound: Dictionary[Global, CompilerObject] = Dictionary[Global, CompilerObject]();

@binder
fun bindGlobal(node: Global, entity: Object): Error {
    let member = entity as MemberInfo;
    let owner = hirLoader.LoadType(ilLoader.Load(member.DeclaringType));
    if (owner.IsError) return owner.UnwrapError();

    let built = cg.Member(owner.Unwrap(), member.Name);
    if (built.IsError) return built.UnwrapError();

    bound[node] = built.Unwrap();
    return null;
}

// ---- Lowering

// A module lowered for a target: the LIR module its CO lowers it to, with its members.
fun lower(node: Module, target: Object): LIRModule {
    return (loadModule(node).Unwrap() as ModuleObject).module(target);
}

// What of a module failed to lower, for any target, by its name and why: what lowering
// it left out, which a build that must have all of it, as a library's, refuses.
fun failures(node: Module): List[String] { return (loadModule(node).Unwrap() as ModuleObject).failures; }

// Imports a module into the running process, as importing a built one does: calls its
// entry point, if it has one, the first time. What imports a module that was compiled
// from its sources does this.
fun enter(node: Module) {
    if (node.EntryPoint == null) return;
    if (entered.Contains(node)) return;
    entered.Add(node);

    // Its members are its own once it is loaded, as its entry point lowers with them.
    loadModule(node).Unwrap();
    let call = Call();
    call.Callee = Get(node.EntryPoint);
    ct(ExpressionStatement(call));
}

// ---- Installing it

// `module` is a Z# module wherever nothing else compiles one.
@entrypoint
fun install() { metaTypes.Default[ModuleSyntax](Module as Instance); }
