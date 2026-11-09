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
import { Instance, Definition, Variable, Type, Named as NamedDefinition } from "std:zsharp/hir";
import { Block, Local, Expression, ExpressionStatement, Set, StringLiteral, Get, Declare, BlockExpression, Statement } from "std:zsharp/hir/code";
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
    Expression as LIRExpression, Statement as LIRStatement
} from "std:zsharp/lir";
import { Bodies, ContainerCompiler } from "std:zsharp/lang";
import { Holder, Held, Initializing } from "std:zsharp/lowering";
import { metaTypes, bodies, language, export } from "core:zsharp";
import { cg, ir, ts, hirLoader, hirCodeLoader, mappedLoader } from "core:compiler";

// ---- The HIR

// A global of a module: one variable, of the type written or of its initializer, which
// is the code that gives its value.
@export
def Global of DotNet_Class : Instance, Definition, Variable, NamedDefinition {
    let name: String;
    let Type: Type;
    let Initializer: Expression;

    @explicitThis new(this, name: String) { this.name = name; }

    @explicitThis get Name(this): String { return this.name; }
}

// A module: its globals and its definitions. It is the root of what it holds, an
// instance and not a definition, so nothing that holds definitions can hold it.
@export
def Module of DotNet_Class : Instance, NamedDefinition {
    let name: String;
    let Globals: List[Global];
    let Definitions: List[Definition];

    @explicitThis new(this, name: String) {
        this.name = name;
        this.Globals = List[Global]();
        this.Definitions = List[Definition]();
    }

    @explicitThis get Name(this): String { return this.name; }
}

// ---- The COs

def Objects of DotNet_Module {
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
            if (this.globals.ContainsKey(target)) return null;

            let type = this.get_Type();
            let global = LIRGlobal(ir.CompileType(type, target).Unwrap());
            global.Name = this.node.Name;
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

        @explicitThis fun CompileIRExpression(this, target: Object): IResult[LIRExpression, Error] {
            return Result[LIRExpression].Ok(GetGlobal(this.global(target)));
        }

        @explicitThis fun Set(this, value: CompilerObject): IResult[CompilerObject, Error] {
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

        @explicitThis new(this, node: Module) {
            this.node = node;
            this.members = Dictionary[String, Definition]();
            this.modules = Dictionary[Object, LIRModule]();
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

            return lowered;
        }

        // A member that can't be lowered into a module, as a definition of another
        // platform's, lowers by itself; any other failure is the member's to report where
        // it is used.
        @explicitThis fun lower(this, member: Definition, lowered: LIRModule, target: Object) {
            let loaded = hirLoader.LoadDefinition(member);
            if (loaded.IsError) return;
            ir.CompileDefinition[LIRModule](loaded.Unwrap(), lowered, target);
        }

        @explicitThis fun Member(this, name: String): IResult[CompilerObject, Error] {
            if (this.members.ContainsKey(name)) return hirLoader.LoadDefinition(this.members[name]);
            return Result[CompilerObject].Error("Module " + this.node.Name + " has no member named " + name + ".");
        }
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

    @explicitThis fun register(this, member: CompiledDefinition, value: Expression): Statement {
        return ExpressionStatement(Bodies.Add(this.self, "Definitions", value));
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

def Meta of DotNet_Module {
    // The CO of each module loaded: a module is an instance, which its loader keeps one
    // CO of, as its members are bound to it.
    let loaded: Dictionary[Module, Objects.ModuleObject] = Dictionary[Module, Objects.ModuleObject]();

    // A module is any definition with a body: `module Name { ... }`, or `def Name of
    // Module { ... }`.
    fun compile(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
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

    fun loadModule(node: Module): IResult[CompilerObject, Error] {
        if (loaded.ContainsKey(node)) return Result[CompilerObject].Ok(loaded[node]);

        let created = Objects.ModuleObject(node);
        loaded[node] = created;
        created.bind();
        return Result[CompilerObject].Ok(created);
    }

    // A global is what it means once it is loaded: it has a type, or a value to have the
    // type of.
    fun loadGlobal(node: Global): IResult[CompilerObject, Error] {
        if (node.Type == null) {
            if (node.Initializer == null) return Result[CompilerObject].Error("Global " + node.Name + " needs a type or a value.");
        }
        return Result[CompilerObject].Ok(Objects.GlobalObject(node));
    }

    // A global used as an instance is the one CO its definition gives.
    fun instanceOfGlobal(node: Global): IResult[CompilerObject, Error] { return hirLoader.LoadDefinition(node); }
}

// ---- Installing it

mappedLoader.Instances.Register[Module](Meta.loadModule);
mappedLoader.Definitions.Register[Global](Meta.loadGlobal);
mappedLoader.Instances.Register[Global](Meta.instanceOfGlobal);

metaTypes.Register(Module as Instance, Meta.compile);
metaTypes.Default[ModuleSyntax](Module as Instance);
