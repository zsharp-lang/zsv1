// std:modules — Z#'s own modules, defined in Z#.
//
// A module is structure: its globals and its definitions. A `let` in it is a global, an
// import runs as the code that builds the module runs, and a definition in it is compiled
// by its own meta-type. The CO of a module finds its members by the names the `Named`
// capability gives them, and holds them: it lowers, for each target, to an LIR module of
// its own, as LIR has no submodules, and lowers each of its members into it. The target
// realizes that module: the CLR realizes it as a module.
//
// Importing it makes `module` a Z# module wherever nothing else compiles one, as `module`
// is `def` of Module. Its own definitions are the CLR's, as it says.

import { String, Object, Boolean, Int32, List, Dictionary, DotNet_Class, DotNet_Module, explicitThis, static } from "core:runtime";
import { Instance, Definition, Variable, Type, Named as NamedDefinition, Annotated } from "std:zsharp/hir";
import { Call } from "std:zsharp/hir/code";
import { Block, Local, Expression, ExpressionStatement, Set, StringLiteral, Get, Declare, BlockExpression, Statement, Cast } from "std:zsharp/hir/code";
import {
    Module as ModuleSyntax, TypeDefinition as TypeSyntax, Definition as DefinitionSyntax, LetExpression as LetSyntax,
    Expression as ExpressionSyntax, Statement as StatementSyntax, ImportStatement as ImportSyntax
} from "std:zsharp/lang/ast";
import { CompilerObject } from "std:zsharp/mir";
import { Error, Result, IResult } from "std:zsharp/common";
import { UnsupportedError } from "std:zsharp/compiler";
import { ICTGetMember, ICTSet } from "std:zsharp/compiler/cg/direct";
import { ICompileIRExpression, ICompileIRStatement, ICompileIRDefinitionIn } from "std:zsharp/compiler/ir";
import { ITyped } from "std:zsharp/compiler/ts";
import { ASTCompiler, CompiledDefinition, CompileASTExpression, CompileASTStatement, CompileASTDefinition } from "std:zsharp/compiler/ast";
import {
    Module as LIRModule, Global as LIRGlobal, GetGlobal, SetGlobal,
    Expression as LIRExpression, Statement as LIRStatement, Function as LIRFunction, ICallable, EntryPoint as LIREntryPoint
} from "std:zsharp/lir";
import { Bodies, ContainerCompiler } from "std:zsharp/lang";
import { Holder, Held, Initializing, keep } from "std:zsharp/lowering";
import { metaTypes, bodies, language, ct } from "core:zsharp";
import { entrypoint, definitionLoader, instanceLoader, metaType, binder } from "std:framework";
import { ilLoader } from "core:runtime";
import { cg, ir, ts, hirLoader, hirCodeLoader, mappedLoader } from "core:compiler";

// ---- The HIR

// A global of a module: one variable, of the type written or of its initializer, which
// is the code that gives its value.
def Global of DotNet_Class : Instance, Definition, Variable, NamedDefinition, Annotated {
    let name: String;
    let Type: Type;
    let Initializer: Expression;
    let annotations: List[Object];

    @explicitThis new(this, name: String) {
        this.name = name;
        this.annotations = List[Object]();
    }

    @explicitThis get Name(this): String { return this.name; }

    @explicitThis get Annotations(this): List[Object] { return this.annotations; }
}

// A module: its globals and its definitions. It is the root of what it holds, an
// instance and not a definition, so nothing that holds definitions can hold it. Its
// entry point, if it has one, is a definition of it that is called, with nothing, when
// the module is imported: what loads it into the environment that imports it, as a
// plugin is. Building it, as compiling its project does, doesn't call it.
def Module of DotNet_Class : Instance, NamedDefinition, Annotated {
    let name: String;
    let Globals: List[Global];
    let Definitions: List[Definition];
    let EntryPoint: Instance;
    let annotations: List[Object];

    @explicitThis new(this, name: String) {
        this.name = name;
        this.Globals = List[Global]();
        this.Definitions = List[Definition]();
        this.annotations = List[Object]();
    }

    @explicitThis get Name(this): String { return this.name; }

    @explicitThis get Annotations(this): List[Object] { return this.annotations; }
}

// ---- The COs

// A global, held by its module: what holds it lowers it into its LIR module, in its
// order, and gives it its initial value in that module's initializer.
def GlobalObject of DotNet_Class : CompilerObject, ICompileIRExpression, ICTSet, ITyped, Held, ICompileIRDefinitionIn[LIRModule] {
    let node: Global;
    let holder: Holder;
    let globals: Dictionary[Object, LIRGlobal];

    @explicitThis new(this, node: Global) {
        this.node = node;
        this.globals = Dictionary[Object, LIRGlobal]();
    }

    @explicitThis fun hold(this, holder: Holder) { this.holder = holder; }

    @explicitThis fun initializer(this): CompilerObject {
        return hirCodeLoader.LoadExpression(this.node.Initializer).Unwrap();
    }

    @explicitThis fun get_Type(this): CompilerObject {
        if (this.node.Type != null) return hirLoader.LoadType(this.node.Type).Unwrap();
        return ts.TypeOf(this.initializer()).Unwrap();
    }

    @explicitThis fun CompileIRDefinition(this, owner: LIRModule, target: Object): Error {
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
    @explicitThis fun global(this, target: Object): LIRGlobal {
        if (this.globals.ContainsKey(target) == false) this.holder.module(target);
        return this.globals[target];
    }

    // A global that was built is what it was built as.
    @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
        if (bound.ContainsKey(this.node)) return ir.CompileExpression(bound[this.node], target);
        return Result[LIRExpression].Ok(GetGlobal(this.global(target)));
    }

    @explicitThis fun Set(this, value: CompilerObject): IResult[CompilerObject, Error] {
        if (bound.ContainsKey(this.node)) return cg.Set(bound[this.node], value);
        let converted = cg.ImplicitCast(value, this.get_Type());
        if (converted.IsError) return converted;

        return Result[CompilerObject].Ok(GlobalSet(this, converted.Unwrap()));
    }
}

def GlobalSet of DotNet_Class : CompilerObject, ICompileIRStatement {
    let global: GlobalObject;
    let value: CompilerObject;

    @explicitThis new(this, global: GlobalObject, value: CompilerObject) { this.global = global; this.value = value; }

    @explicitThis fun CompileIRStatement(this, target: Object): IResult[LIRStatement, Error] {
        let value = ir.CompileExpression(this.value, target);
        if (value.IsError) return Result[LIRStatement].Error(value.UnwrapError());

        return Result[LIRStatement].Ok(SetGlobal(this.global.global(target), value.Unwrap()));
    }
}

// A Z# module: it holds its members, which it finds by their names, and lowers them
// into its LIR module, its globals first, in their order.
def ModuleObject of DotNet_Class : CompilerObject, Holder, ICTGetMember[String] {
    let node: Module;
    let members: Dictionary[String, Definition];
    let modules: Dictionary[Object, LIRModule];
    let failures: List[String];

    @explicitThis new(this, node: Module) {
        this.node = node;
        this.members = Dictionary[String, Definition]();
        this.modules = Dictionary[Object, LIRModule]();
        this.failures = List[String]();
    }

    // Finds its members by their names, and binds the COs of those it holds to itself.
    @explicitThis fun bind(this) {
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

    @explicitThis fun add(this, member: Definition) {
        if (member is named of NamedDefinition) this.members[named.Name] = member;

        let loaded = hirLoader.LoadDefinition(member);
        if (loaded.IsError) return;
        if (loaded.Unwrap() is held of Held) held.hold(this);
    }

    @explicitThis fun module(this, target: Object): LIRModule {
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
                if (entry.Unwrap() is function of LIRFunction) lowered.Metadata.Add(LIREntryPoint(function));
            }
        }

        return lowered;
    }

    // A member that can't be lowered into a module, as a definition of another
    // platform's, lowers by itself; any other failure is the member's to report where
    // it is used.
    @explicitThis fun lower(this, member: Definition, lowered: LIRModule, target: Object) {
        let name = "a definition";
        if (member is named of NamedDefinition) name = named.Name;

        let loaded = hirLoader.LoadDefinition(member);
        if (loaded.IsError) {
            this.failures.Add(name + ": " + loaded.UnwrapError());
            return;
        }

        let failed = ir.CompileDefinition[LIRModule](loaded.Unwrap(), lowered, target);
        if (failed != null) this.failures.Add(name + ": " + failed);
    }

    @explicitThis fun Member(this, name: String): IResult[CompilerObject, Error] {
        if (this.members.ContainsKey(name)) return hirLoader.LoadDefinition(this.members[name]);
        return Result[CompilerObject].Error("Module " + this.node.Name + " has no member named " + name + ".");
    }
}

// ---- The meta-compiler

// What the members of a module are compiled with: a let is a global, bound by its name,
// an import runs as the module is built, and a definition is compiled by its own
// meta-type.
def Members of DotNet_Class {
    let self: Local;
    let fork: ASTCompiler;
    let expressions: CompileASTExpression;
    let statements: CompileASTStatement;

    @explicitThis new(this, self: Local, compiler: ASTCompiler) {
        this.self = self;
        this.fork = compiler.Fork();
        this.expressions = this.fork.Expression;
        this.statements = this.fork.Statement;
    }

    @explicitThis fun expression(this, compiler: ASTCompiler, node: ExpressionSyntax): IResult[Expression, Error] {
        if (node is local of LetSyntax) return this.global(compiler, local);
        return this.expressions(compiler, node);
    }

    @explicitThis fun statement(this, compiler: ASTCompiler, node: StatementSyntax): IResult[Statement, Error] {
        if (node is imported of ImportSyntax) {
            let holder = bodies.Imported(compiler, imported);
            if (holder.IsError) return Result[Statement].Error(holder.UnwrapError());
            if (holder.Unwrap() == null) return Result[Statement].Ok(Block([]));
            return Result[Statement].Ok(Declare(holder.Unwrap()));
        }
        return this.statements(compiler, node);
    }

    // A global: code that builds it and adds it to the module. Its initializer is code.
    @explicitThis fun global(this, compiler: ASTCompiler, local: LetSyntax): IResult[Expression, Error] {
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
    @explicitThis fun register(this, member: CompiledDefinition, value: Expression): Statement {
        let kind = Cast();
        kind.Value = value;
        kind.Type = Get(member.UnderlyingType);
        return ExpressionStatement(Bodies.Add(this.self, "Definitions", kind));
    }

}

def Filler of DotNet_Class {
    let compiler: ASTCompiler;
    let body: List[StatementSyntax];
    let self: Local;

    @explicitThis new(this, compiler: ASTCompiler, body: List[StatementSyntax], self: Local) {
        this.compiler = compiler;
        this.body = body;
        this.self = self;
    }

    @explicitThis fun fill(this): IResult[Block, Error] {
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

    if (definition is node of ModuleSyntax) {
        name = node.Name;
        body = node.Body;
    }
    if (definition is type of TypeSyntax) {
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
