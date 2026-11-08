using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;
using ZSharp.Lang;
using ZSharp.Parser;
using ZSharp.Platform.Runtime;
using ZSharp.Tokenizer;
using C = ZSharp.HIR.Code;
using A = ZSharp.AST;
using Scope = Core.Compiler.ModuleScope;

void Report(string name, bool ok)
{
    if (!ok)
        Environment.ExitCode = 1;

    Console.WriteLine($"{(ok ? "pass" : "FAIL")}  {name}");
}
void Try(string name, Func<bool> check)
{
    try { Report(name, check()); }
    catch (Exception e)
    {
        Environment.ExitCode = 1;
        Console.WriteLine($"FAIL  {name}: {e.GetType().Name}: {e.Message}");
    }
}

var runtime = new Runtime();
var il = new ILLoader(runtime);
Core.Runtime.ModuleScope.Runtime = runtime;
Core.Runtime.ModuleScope.ILLoader = il;

var mapped = new MappedLoader(Scope.HIRLoader);
var mappedCode = new MappedCodeLoader(Scope.HIRCodeLoader);
mapped.Apply();
mappedCode.Apply();

new ZSharp.Compiler.CGDispatchers.Direct.Dispatcher(Scope.CG).Apply();
new ZSharp.Compiler.CGDispatchers.Typed.Dispatcher(Scope.CG, Scope.TS).Apply();
new ZSharp.Compiler.TSDispatchers.Static.Dispatcher(Scope.TS).Apply();
new ZSharp.Compiler.IRDispatchers.Static.Dispatcher(Scope.IR).Apply();

CodeLoaders.Register(mappedCode, mapped, Scope.HIRCodeLoader, Scope.HIRLoader, Scope.CG);
il.Install(mapped, mappedCode);

var reifier = new MappedReifier(Scope.HIRReifier);
reifier.Apply();
new CodeReifiers(Scope.HIRReifier, il).Register(reifier);

var hirRunner = new HIRRunner();
var mappedRunner = new MappedRunner(
    hirRunner, Scope.HIRCodeLoader,
    co => { runtime.Execute(Scope.IR.CompileStatement(co, runtime).Unwrap()); return null; },
    co => Result<object?>.Ok(runtime.Evaluate(Scope.IR.CompileExpression(co, runtime).Unwrap()))
);
mappedRunner.Apply();
CodeRunners.Register(mappedRunner, hirRunner);

C.Expression Get(ZSharp.HIR.Instance instance) => new C.Get(instance);
C.Expression Member(C.Expression target, string name) => new C.NamedMember { Target = target, Name = name };
C.Expression Str(string value) => new C.StringLiteral(value);
C.Expression Call(C.Expression callee, params C.Expression[] arguments) => new C.Call
{
    Callee = callee,
    Arguments = [.. arguments.Select(argument => new C.Argument { Value = argument })]
};

object? Evaluate(C.Expression expression) => hirRunner.EvaluateExpression(expression).Unwrap();
void Run(C.Statement statement)
{
    if (hirRunner.RunStatement(statement) is { } error) throw new Exception(error.ToString());
}
C.Expression Reify(C.Expression e) => Scope.HIRReifier.ReifyExpression(e).Unwrap();
C.Expression ReifyS(C.Statement s) => Scope.HIRReifier.ReifyStatement(s).Unwrap();

var probe = Get(il.Load(typeof(Probe)));
ZSharp.HIR.Type T(Type type) => il.Load(type);

var identity = il.Expose(new Widget());
Probe.Identity = identity;

// ---- Reification

Try("literal", () => Evaluate(Reify(Str("x"))) is C.StringLiteral { Value: "x" });
Try("boolean", () => Evaluate(Reify(new C.BooleanLiteral(true))) is C.BooleanLiteral { Value: true });

Try("call builds a call node that runs", () =>
{
    var built = Evaluate(Reify(Call(Member(probe, "Echo"), Str("hi"))));
    return built is C.Call { Arguments.Count: 1 } node && (string?)Evaluate(node) == "hi";
});

Try("named argument survives", () =>
{
    var call = new C.Call { Callee = probe, Arguments = [new() { Value = Str("v"), Name = "n" }] };
    return Evaluate(Reify(call)) is C.Call { Arguments: [{ Name: "n" }] };
});

Try("cast survives", () =>
    Evaluate(Reify(new C.Cast { Value = Str("x"), Type = Get(il.Load(typeof(string))) })) is C.Cast);

Try("block with a local runs", () =>
{
    var local = new C.Local("l", T(typeof(string)));
    var block = new C.Block(
        new C.ExpressionStatement(new C.Set { Target = Get(local), Value = Str("via local") }),
        new C.ExpressionStatement(Call(Member(probe, "Record"), Get(local)))
    );
    block.Locals.Add(local);

    var built = (C.Block)Evaluate(ReifyS(block))!;
    Probe.Last = null;
    Run(built);
    return built.Locals is [{ Name: "l" }] && !ReferenceEquals(built.Locals[0], local) && built.Statements.Count == 2 && Probe.Last == "via local";
});

Try("if and return are structure", () =>
{
    var statement = new C.IfStatement
    {
        Condition = new C.BooleanLiteral(true),
        Then = new C.Return { Value = Str("r") },
        Else = new C.Return()
    };
    return Evaluate(ReifyS(statement)) is C.IfStatement { Then: C.Return { Value: C.StringLiteral }, Else: C.Return { Value: null } };
});

Try("block expression runs", () =>
{
    var local = new C.Local("l", T(typeof(string)));
    var block = new C.BlockExpression
    {
        Result = Get(local),
        Statements = [new C.ExpressionStatement(new C.Set { Target = Get(local), Value = Str("v") })]
    };
    block.Locals.Add(local);
    return (string?)Evaluate((C.Expression)Evaluate(Reify(block))!) == "v";
});

Try("outer local is placed by value", () =>
{
    var outer = new C.Local("c_Bar", T(typeof(ZSharp.HIR.Instance)));
    var code = new C.BlockExpression
    {
        Result = Reify(Get(outer)),
        Statements = [new C.ExpressionStatement(new C.Set { Target = Get(outer), Value = Call(Member(probe, "Make")) })]
    };
    code.Locals.Add(outer);
    return Evaluate(code) is C.Get { Instance: var instance } && ReferenceEquals(instance, identity);
});

// A definition that isn't cast: the code that builds the body builds the object in a
// variable of its own, and the body refers to that variable.
C.Expression Built(C.Expression body, C.Local made)
{
    var build = new C.BlockExpression
    {
        Result = body,
        Statements = [new C.ExpressionStatement(new C.Set { Target = Get(made), Value = Call(Member(probe, "Make")) })]
    };

    build.Locals.Add(made);

    return build;
}

Try("identity runs when the code is built, not when it runs", () =>
{
    var made = new C.Local("made", T(typeof(ZSharp.HIR.Instance)));
    var reified = Built(Reify(Call(Member(probe, "Name"), Get(made))), made);
    Probe.Made = 0;
    var built = (C.Expression)Evaluate(reified)!;
    var afterBuild = Probe.Made;
    var name = Evaluate(built);
    Evaluate(built);
    return afterBuild == 1 && Probe.Made == 1 && (string?)name == "Widget";
});

Try("identity once per build of the enclosing code", () =>
{
    var made = new C.Local("made", T(typeof(ZSharp.HIR.Instance)));
    var reified = Built(Reify(Get(made)), made);
    Probe.Made = 0;
    Evaluate(reified);
    Evaluate(reified);
    return Probe.Made == 2;
});

Try("reification nests", () =>
{
    var twice = Reify(Reify(Call(Member(probe, "Record"), Str("twice"))));
    var once = (C.Expression)Evaluate(twice)!;
    var code = (C.Expression)Evaluate(once)!;
    Probe.Last = null;
    Evaluate(code);
    return Probe.Last == "twice";
});

Try("nested identity waits for its own level", () =>
{
    var made = new C.Local("made", T(typeof(ZSharp.HIR.Instance)));
    var twice = Reify(Built(Reify(Get(made)), made));
    Probe.Made = 0;
    var once = (C.Expression)Evaluate(twice)!;
    var atOuter = Probe.Made;
    var code = (C.Expression)Evaluate(once)!;
    return atOuter == 0 && Probe.Made == 1 && code is C.Get;
});

Try("unknown node is unsupported, not a crash", () =>
    Scope.HIRReifier.ReifyExpression(new Unknown()).IsUnsupported());

// ---- Definitions and casts

var compiler = new ASTCompiler();
var mappedCompiler = new MappedASTCompiler(compiler);
mappedCompiler.Apply();

bool IsUnderlying(C.Expression type, ZSharp.HIR.Instance underlying)
    => Scope.HIRCodeLoader.LoadExpression(type).When(out var target).IsOk
    && Scope.HIRLoader.LoadInstance(underlying).When(out var definition).IsOk
    && Scope.TS.IsAssignableTo(target!, definition!)
    && Scope.TS.IsAssignableTo(definition!, target!);

var binder = new ZSharp.Lang.Binder();
var mappedBinder = new ZSharp.Lang.MappedBinder(binder);
mappedBinder.Apply();

var language = new Language(binder);
var document = new DocumentCompiler();
language.Install(mappedCompiler, mappedBinder, IsUnderlying);

var dotnetClass = il.Load(typeof(Package.DotNet.Class));
document.Scope.Add("DotNet_Class", Get(dotnetClass));
document.Scope.Add("String", Get(il.Load(typeof(string))));

var compiles = 0;

// A fake meta-compiler for `class`. The real one is step 4.
mappedCompiler.Definitions.Register<A.TypeDefinition>((_, definition) =>
{
    compiles++;
    return Result<CompiledDefinition>.Ok(new(
        new C.Local(definition.Name, dotnetClass),
        Call(Get(dotnetClass), Str(definition.Name)),
        dotnetClass,
        () => Result<C.Block>.Ok(new C.Block())
    ));
});

A.Statement Parse(string source)
{
    var parser = new Parser(Tokenizer.Tokenize(new(new StringReader(source))));
    return Syntax.Install(parser).Document.ParseStatement(parser);
}

A.Definition Definition() => (A.Definition)((A.DefinitionStatement)Parse("class A {}")).Definition;

A.CastExpression Cast(A.Expression value, string type)
{
    var cast = (A.CastExpression)((A.ExpressionStatement)Parse($"\"x\" as {type};")).Expression;
    cast.Expression = value;
    return cast;
}

C.Expression Compile(A.Expression expression) => document.Within(compiler, () => compiler.CompileExpression(expression)).Unwrap();

Try("definition statement stays a statement", () =>
    document.Compile(compiler, Parse("class A {}")).Unwrap() is C.Block { Statements: [Define, C.Block] });

A.ExpressionStatement CastStatement(string type)
{
    var statement = (A.ExpressionStatement)Parse($"\"x\" as {type};");
    ((A.CastExpression)statement.Expression).Expression = Definition();
    return statement;
}

Try("uncast definition runs before its statement", () =>
    document.Compile(compiler, Parse("(class A {}).x;")).Unwrap() is C.Block
    {
        Statements: [Define, C.ExpressionStatement { Expression: C.NamedMember { Target: C.Get } }]
    });

Try("cast to underlying type keeps the build code", () =>
{
    var hir = Compile(Cast(Definition(), "DotNet_Class"));
    return hir is C.BlockExpression && Evaluate(hir) is Package.DotNet.Class { Name: "A" };
});

Try("meta-compiler runs once per cast", () =>
{
    compiles = 0;
    document.Compile(compiler, CastStatement("String")).Unwrap();
    return compiles == 1;
});

Try("cast to another type is an ordinary cast", () =>
    document.Compile(compiler, CastStatement("String")).Unwrap() is C.Block
    {
        Statements: [Define, C.ExpressionStatement { Expression: C.Cast { Value: C.Get } }]
    });

Try("further casts do nothing", () =>
    Evaluate(Compile(Cast(Cast(Definition(), "DotNet_Class"), "DotNet_Class"))) is Package.DotNet.Class { Name: "A" });

Try("ordinary cast of a value", () =>
    (string?)Evaluate(Compile((A.CastExpression)((A.ExpressionStatement)Parse("\"x\" as String;")).Expression)) == "x");

// A fake function meta-compiler: a fork that overrides the cast decides where the code goes.
ZSharp.Compiler.ASTCompiler SendingTo(C.Block body, ZSharp.Compiler.ASTCompiler from)
{
    var fork = from.Fork();
    var previous = fork.Expression;

    fork.Expression = (current, expression) => expression is A.CastExpression cast
        ? SendTo(body, current, cast)
        : previous(current, expression);

    return fork;
}

C.Expression CompileWith(ZSharp.Compiler.ASTCompiler with, A.Expression expression)
    => document.Within(with, () => with.CompileExpression(expression)).Unwrap();

IResult<C.Expression, Error> SendTo(C.Block body, ZSharp.Compiler.ASTCompiler current, A.CastExpression cast)
{
    var compiled = document.Within(current, () => current.CompileDefinition((A.Definition)cast.Expression)).Unwrap();
    var local = new C.Local("f_foo_bar", il.Load(typeof(ZSharp.HIR.Instance)));
    body.Locals.Add(local);
    body.Statements.Add(new C.ExpressionStatement(new C.Set { Target = new C.Get(local), Value = compiled.Build().Unwrap() }));
    return Result<C.Expression>.Ok(new C.Get(local));
}

Try("a fork that overrides the cast sends code to the body, and the original doesn't", () =>
{
    var body = new C.Block();
    var hir = CompileWith(SendingTo(body, compiler), Cast(Definition(), "DotNet_Class"));

    return hir is C.Get { Instance: C.Local }
        && body.Statements.Count == 1
        && Compile(Cast(Definition(), "DotNet_Class")) is C.BlockExpression;
});

Try("forks nest", () =>
{
    var outer = new C.Block();
    var inner = new C.Block();

    var a = SendingTo(outer, compiler);
    var b = SendingTo(inner, a);
    CompileWith(b, Cast(Definition(), "DotNet_Class"));
    CompileWith(a, Cast(Definition(), "DotNet_Class"));

    return inner.Statements.Count == 1 && outer.Statements.Count == 1;
});

Try("worked example: reified body builds bar each time it runs", () =>
{
    // foo's body, compiled with the cast overridden, then reified for f_foo.body.
    var body = new C.Block();
    var bar = CompileWith(SendingTo(body, compiler), Cast(Definition(), "DotNet_Class"));

    body.Statements.Add(new C.ExpressionStatement(Call(Member(probe, "Keep"), bar)));

    var built = (C.Block)Evaluate(ReifyS(body))!;

    Probe.Kept.Clear();
    Run(built);
    Run(built);

    return Probe.Kept is [Package.DotNet.Class { Name: "A" } first, Package.DotNet.Class { Name: "A" } second]
        && !ReferenceEquals(first, second);
});

class Unknown : C.Expression;

public sealed class Widget;

public static class Probe
{
    public static string? Last;
    public static int Made;
    public static ZSharp.HIR.Instance Identity = null!;
    public static readonly List<object> Kept = [];

    public static string Echo(string value) => value;
    public static void Record(string value) => Last = value;
    public static string Name(object value) => value.GetType().Name;
    public static void Keep(object value) => Kept.Add(value);
    public static ZSharp.HIR.Instance Make() { Made++; return Identity; }
}
