using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;
using ZSharp.Lang;
using ZSharp.Lang.DotNet;
using ZSharp.Parser;
using ZSharp.Platform.Runtime;
using ZSharp.Tokenizer;
using C = ZSharp.HIR.Code;
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
new ZSharp.Compiler.OverloadingDispatchers.Static.Dispatcher(Scope.Overloading).Apply();

CodeLoaders.Register(mappedCode, mapped, Scope.HIRCodeLoader, Scope.HIRLoader, Scope.CG);
il.Install(mapped, mappedCode);

var reifier = new MappedReifier(Scope.HIRReifier);
reifier.Apply();
new CodeReifiers(Scope.HIRReifier, il).Register(reifier);

var runner = new HIRRunner();
var mappedRunner = new MappedRunner(
    runner, Scope.HIRCodeLoader,
    co => { runtime.Execute(Scope.IR.CompileStatement(co, runtime).Unwrap()); return null; },
    co => Result<object?>.Ok(runtime.Evaluate(Scope.IR.CompileExpression(co, runtime).Unwrap()))
);
mappedRunner.Apply();
CodeRunners.Register(mappedRunner, runner);

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

Core.ZSharp.ModuleScope.Compiler = compiler;
DocumentCompiler.Install(mapped, Scope.HIRLoader);

var core = new ZSharp.Importer.String.LibraryImporter("core");
core.Add("runtime", il.Load(typeof(Core.Runtime.ModuleScope).Module));
core.Add("zsharp", il.Load(typeof(Core.ZSharp.ModuleScope).Module));

var importer = new ZSharp.Importer.String.StringImporter();
var host = new DotNetDocuments(
    runtime, il,
    source => source is string text ? importer.Import(text) : Result<ZSharp.HIR.Instance>.Error("Only a string can be imported here."),
    instance => Scope.HIRLoader.LoadInstance(instance).When(out _).Error(out var error) ? error : null
);
host.Install(mapped);
DocumentCompiler.Install(mappedBinder);

var document = new DocumentCompiler(language, host);
var documents = new DocumentImporter(language, compiler, host);
importer.Register("core", core);
importer.DefaultImporter = documents;
language.Install(mappedCompiler, IsUnderlying);
Core.ZSharp.ModuleScope.Importer = importer.Import;
DotNetMetaTypes.Install(
    language, mappedCompiler, mappedBinder, runner, il,
    () => new C.NamedMember { Target = new C.Get(il.Load(typeof(Core.ZSharp.ModuleScope).Module)), Name = "import" }
);

var probe = new C.Get(il.Load(typeof(Probe)));
document.Scope.Add("Keep", new C.NamedMember { Target = probe, Name = "Keep" });
document.Scope.Add("String", new C.Get(il.Load(typeof(string))));
document.Scope.Add("Boolean", new C.Get(il.Load(typeof(bool))));
document.Scope.Add("Keep2", new C.NamedMember { Target = probe, Name = "Keep2" });
document.Scope.Add("Which", new C.NamedMember { Target = probe, Name = "Which" });
document.Scope.Add("Either", new C.NamedMember { Target = probe, Name = "Either" });

var coreRuntime = new C.Get(il.Load(typeof(Core.Runtime.ModuleScope).Module));
document.Scope.Add("explicitThis", new C.NamedMember { Target = coreRuntime, Name = "explicitThis" });
document.Scope.Add("static", new C.NamedMember { Target = coreRuntime, Name = "static" });
document.Scope.Add("export", new C.NamedMember { Target = new C.Get(il.Load(typeof(Core.ZSharp.ModuleScope).Module)), Name = "export" });

foreach (var decorator in new[] { "first", "second", "mark", "markMethod" })
    document.Scope.Add(decorator, new C.NamedMember { Target = probe, Name = decorator });
document.Scope.Add("Object", new C.Get(il.Load(typeof(object))));
document.Scope.Add("DotNet_Class", new C.Get(il.Load(typeof(Package.DotNet.Class))));
document.Scope.Add("DotNet_Module", new C.Get(il.Load(typeof(Package.DotNet.Module))));
document.Scope.Add("FieldCount", new C.NamedMember { Target = probe, Name = "FieldCount" });

ZSharp.AST.Statement ParseOne(string source)
{
    var parser = new Parser(Tokenizer.Tokenize(new(new StringReader(source))));
    return Syntax.Install(parser).Document.ParseStatement(parser);
}

// Runs source statement by statement, returning the first error.
string? Run(string source)
{
    var parser = new Parser(Tokenizer.Tokenize(new(new StringReader(source))));
    var syntax = Syntax.Install(parser);

    while (parser.HasTokens && !parser.Is(ZSharp.Text.TokenType.EOF))
    {
        var statement = syntax.Document.ParseStatement(parser);

        if (document.Run(compiler, statement) is { } error)
            return error.ToString();
    }

    return null;
}

object Kept()
{
    var value = Probe.Kept[^1];
    Probe.Kept.Clear();
    return value;
}

Try("class compiles and runs", () => Run("""
    class Animal { let name: String; }
    class Dog : Animal { let friend: Dog; }
    Keep(Dog());
    """) is null);

var dog = Kept();
var type = dog.GetType();

Try("it is a real CLR class", () => type.IsClass && type.Name == "Dog" && type.IsPublic);
Try("self reference is its own type", () => type.GetField("friend")?.FieldType == type);
Try("base class is real", () => type.BaseType?.Name == "Animal");
Try("inherited field is real", () => type.GetField("name")?.FieldType == typeof(string));
Try("default constructor", () => Activator.CreateInstance(type)?.GetType() == type);

Try("fields hold values", () =>
{
    type.GetField("name")!.SetValue(dog, "rex");
    type.GetField("friend")!.SetValue(dog, dog);
    return (string?)type.GetField("name")!.GetValue(dog) == "rex"
        && ReferenceEquals(type.GetField("friend")!.GetValue(dog), dog);
});

Try("a name always means the same type", () =>
{
    Run("Keep(Dog());");
    return Kept().GetType() == type;
});

Try("members are reached like any class", () =>
{
    Probe.Kept.Clear();
    var error = Run("Keep(Dog().name); Keep(Dog().friend);");
    var values = Probe.Kept.ToArray();
    Probe.Kept.Clear();
    return error is null && values is [null, null];
});

Try("cast is structure, with no type loaded", () =>
{
    if (Run("Keep(class Plan { let note: String; let self: Plan; } as DotNet_Class);") is { } error)
        throw new Exception(error);

    return Kept() is Package.DotNet.Class { Name: "Plan", Fields: [{ Name: "note" }, { Name: "self" }] } plan
        && plan.Fields[1].Type is Package.DotNet.Class self
        && ReferenceEquals(self, plan);
});

Try("structure loads only when asked", () =>
{
    Run("Keep(class Later { let self: Later; } as DotNet_Class);");
    var node = (Package.DotNet.Class)Kept();
    var loaded = Scope.HIRLoader.LoadType(node).Unwrap();
    return Scope.IR.CompileType(loaded, runtime).IsOk
        && Scope.HIRLoader.LoadType(node).IsOk;
});

Try("expression definition has identity but binds no name outside", () =>
{
    if (Run("Keep(class Temp {}());") is { } error)
        throw new Exception(error);

    return Kept().GetType().Name == "Temp" && Run("Keep(Temp());")!.Contains("not defined");
});

Try("expression definition binds its name inside itself", () =>
{
    if (Run("Keep(class Node { let next: Node; }());") is { } error)
        throw new Exception(error);

    var node = Kept().GetType();
    return node.GetField("next")?.FieldType == node;
});

Try("of with the CLR class meta-type", () =>
    Run("class Explicit of DotNet_Class {}") is null && Run("Keep(Explicit());") is null && Kept().GetType().Name == "Explicit");

Try("two classes can share a name", () =>
{
    Run("Keep(class Twin {}());");
    var first = Kept().GetType();
    Run("Keep(class Twin {}());");
    return Kept().GetType() != first;
});

Try("unsupported meta-type is an error", () => Run("class X of String {}") is { } e && e.Contains("meta-type"));
Try("field without a type is an error", () => Run("""class X { let a = "x"; }""") is { } e && e.Contains("needs a type"));
Try("unknown field type is an error", () => Run("class X { let a: Missing; }") is { } e && e.Contains("not defined"));
Try("non-type field type is an error", () => Run("""class X { let a: "text"; }""") is not null);
Try("two bases is an error for now", () => Run("class X : String, String {}") is { } e && e.Contains("interfaces"));

Try("a failed class leaves the name unusable, not half made", () =>
    Run("class Bad { let a: Missing; }") is not null && Run("Keep(Bad());")!.Contains("not defined"));

Try("build code reifies into a body", () =>
{
    var definition = (ZSharp.AST.Definition)((ZSharp.AST.DefinitionStatement)ParseOne("class Made { let self: Made; let s: String; }")).Definition;
    var compiled = document.Within(compiler, () => compiler.CompileDefinition(definition)).Unwrap();
    var reified = Scope.HIRReifier.ReifyExpression(compiled.Build().Unwrap()).Unwrap();
    var code = (C.Expression)runner.EvaluateExpression(reified).Unwrap()!;

    var first = runner.EvaluateExpression(code).Unwrap();
    var second = runner.EvaluateExpression(code).Unwrap();

    return first is Package.DotNet.Class { Name: "Made", Fields.Count: 2 } a
        && second is Package.DotNet.Class b
        && !ReferenceEquals(a, b)
        && ReferenceEquals(a.Fields[0].Type, a);
});

// ---- Modules

Try("forward references between classes", () =>
{
    if (Run("""
        module M1 {
            class A { let b: B; }
            class B { let a: A; }
        }
        Keep(M1.A());
        """) is { } error)
        throw new Exception(error);

    var a = Kept().GetType();
    var b = a.GetField("b")!.FieldType;
    return a.Name == "A" && b.Name == "B" && b.GetField("a")?.FieldType == a;
});

Try("forward base class", () =>
{
    if (Run("""
        module M2 {
            class C : D {}
            class D { let tag: String; }
        }
        Keep(M2.C());
        """) is { } error)
        throw new Exception(error);

    var c = Kept();
    return c.GetType().BaseType?.Name == "D" && c.GetType().GetField("tag") is not null;
});

Try("module code runs while the module is built", () =>
{
    Probe.Kept.Clear();
    if (Run("""
        module M3 {
            class Foo {}
            Keep((Foo as DotNet_Class).Name);
        }
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "Foo";
});

Try("a statement sees definitions constructed but not filled", () =>
{
    Probe.Counts.Clear();
    if (Run("""
        module M4 {
            class F { let x: String; let y: String; }
            FieldCount(F as DotNet_Class);
        }
        """) is { } error)
        throw new Exception(error);

    return Probe.Counts is [0];
});

Try("the module holds its filled definitions", () =>
{
    if (Run("""
        module M5 {
            class F { let x: String; }
            Keep(class Inner {});
        }
        """) is { } error)
        throw new Exception(error);

    return Kept() is Package.DotNet.Class { Name: "Inner" };
});

Try("a statement can't use a later definition", () =>
    Run("""
        module M6 {
            Keep(Later);
            class Later {}
        }
        """) is { } e && e.Contains("not defined"));

Try("a definition can't be called in its own module", () =>
    Run("""
        module M7 {
            class Z {}
            Keep(Z());
        }
        """) is not null);

Try("a definition can't be a meta-type in its own module", () =>
    Run("""
        module M8 {
            class Meta {}
            class Y of Meta {}
        }
        """) is { } e && e.Contains("own module"));

Try("module members stay inside the module", () =>
    Run("module M9 { class OnlyInside {} }") is null && Run("Keep(OnlyInside());") is { } e && e.Contains("not defined"));

Try("a missing module member is an error", () =>
    Run("Keep(M9.Nothing());") is { } e && e.Contains("no member"));

Try("a whole file module", () =>
{
    if (Run("""
        module M10;
        class Whole { let self: Whole; }
        """) is { } error)
        throw new Exception(error);

    return Run("Keep(M10.Whole());") is null && Kept().GetType().Name == "Whole";
});

Try("class level code runs while the class is built", () =>
{
    Probe.Kept.Clear();
    return Run("""class Cl { Keep("at class"); let s: String; }""") is null && (string)Kept() == "at class";
});

Try("an identity definition in class code is plain", () =>
{
    if (Run("""class Cm { Keep(class Nested {}); }""") is { } error)
        throw new Exception(error);

    return Kept() is Package.DotNet.Class { Name: "Nested" };
});

Try("a module can use earlier classes", () =>
{
    if (Run("""
        class Outside {}
        module M11 { class Uses : Outside { let o: Outside; } }
        Keep(M11.Uses());
        """) is { } error)
        throw new Exception(error);

    return Kept().GetType().BaseType?.Name == "Outside";
});

Try("document level has no forward references", () =>
    Run("""
        class Early { let l: Latecomer; }
        class Latecomer {}
        """) is { } e && e.Contains("not defined"));

Try("base cycle is an error", () =>
    Run("""
        module M12 {
            class P : Q {}
            class Q : P {}
        }
        """) is { } e && e.Contains("derives from itself"));

Try("sealed base is an error", () =>
    Run("class S1 : String {}") is { } e && e.Contains("sealed"));

Try("module cast is structure", () =>
{
    var module = (ZSharp.AST.Definition)((ZSharp.AST.DefinitionStatement)ParseOne("module M13 { class V {} }")).Definition;
    var cast = (ZSharp.AST.CastExpression)((ZSharp.AST.ExpressionStatement)ParseOne("\"x\" as DotNet_Module;")).Expression;
    cast.Expression = module;

    return runner.EvaluateExpression(document.Within(compiler, () => compiler.CompileExpression(cast)).Unwrap()).Unwrap()
        is Package.DotNet.Module { Name: "M13", Definitions: [Package.DotNet.Class { Name: "V" }] };
});

// ---- Bodies

bool Show(string text) { Console.WriteLine($"note  {text}"); return true; }

object? Call(object target, string method, params object?[] arguments)
    => target.GetType().GetMethod(method)!.Invoke(target, arguments);

Try("instance method", () =>
{
    if (Run("""
        class Greeter {
            let name: String;

            @explicitThis new(this, name: String) { this.name = name; }

            @explicitThis fun hello(this): String { return this.name; }
        }
        Keep(Greeter("zs"));
        """) is { } error)
        throw new Exception(error);

    return (string?)Call(Kept(), "hello") == "zs";
});

Try("z# calls its own method", () =>
    Run("""Keep(Greeter("again").hello());""") is null && (string)Kept() == "again");

Try("static method", () =>
{
    if (Run("""
        class Maker {
            @static fun make(text: String): String { return text; }
        }
        Keep(Maker.make("static"));
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "static";
});

Try("a method is a real CLR method", () =>
{
    Run("""Keep(Greeter("x"));""");
    var method = Kept().GetType().GetMethod("hello")!;
    return method.ReturnType == typeof(string) && !method.IsStatic && method.GetParameters().Length == 0;
});

Try("methods call each other forward", () =>
{
    if (Run("""
        class Chain {
            @explicitThis fun first(this): String { return this.second("end"); }
            @explicitThis fun second(this, text: String): String { return text; }
        }
        Keep(Chain().first());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "end";
});

Try("a method reads an inherited field", () =>
{
    if (Run("""
        class Parent { let tag: String; @explicitThis new(this) { this.tag = "inherited"; } }
        class Child : Parent {
            @explicitThis fun read(this): String { return this.tag; }
        }
        Keep(Child().read());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "inherited";
});

Try("a method constructs its own class", () =>
{
    if (Run("""
        class Self {
            let text: String;
            @explicitThis new(this, text: String) { this.text = text; }
            @explicitThis fun copy(this): Self { return Self(this.text); }
        }
        Keep(Self("same").copy().text);
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "same";
});

Try("classes in a module call each other", () =>
{
    if (Run("""
        module Pair {
            class Left {
                @explicitThis fun other(this): Right { return Right(); }
            }
            class Right {
                @explicitThis fun name(this): String { return "right"; }
                @explicitThis fun back(this): Left { return Left(); }
            }
        }
        Keep(Pair.Left().other().name());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "right";
});

Try("field default value", () =>
{
    if (Run("""
        class Defaulted {
            let label: String = "default";
        }
        Keep(Defaulted().label);
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "default";
});

Try("field default with a constructor", () =>
{
    if (Run("""
        class Both {
            let label: String = "first";
            let other: String;
            @explicitThis new(this) { this.other = this.label; }
        }
        Keep(Both().other);
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "first";
});

Try("locals and control flow", () =>
{
    if (Run("""
        class Flow {
            @explicitThis fun pick(this, yes: Boolean): String {
                let result: String = "no";

                if (yes) {
                    result = "yes";
                }

                return result;
            }
        }
        Keep(Flow().pick(true));
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "yes";
});

Try("a parameter can be assigned", () =>
{
    if (Run("""
        class Assign {
            @static fun change(text: String): String {
                text = "changed";
                return text;
            }
        }
        Keep(Assign.change("x"));
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "changed";
});

Try("cast definition in a body builds a new object each call", () =>
{
    if (Run("""
        class Factory {
            @explicitThis fun make(this): DotNet_Class {
                return class Made2 { let x: String; } as DotNet_Class;
            }
        }
        Keep(Factory());
        """) is { } error)
        throw new Exception(error);

    var factory = Kept();
    return Call(factory, "make") is Package.DotNet.Class { Name: "Made2", Fields.Count: 1 } a
        && Call(factory, "make") is Package.DotNet.Class b
        && !ReferenceEquals(a, b);
});

Try("identity definition in a body is built once, with the method", () =>
{
    if (Run("""
        class Holder {
            @explicitThis fun make(this): Object { return class Once {}(); }
        }
        Keep(Holder());
        """) is { } error)
        throw new Exception(error);

    var holder = Kept();
    return Call(holder, "make")!.GetType() == Call(holder, "make")!.GetType()
        && Call(holder, "make")!.GetType().Name == "Once";
});

Try("a definition in a body captures a parameter by being cast", () =>
{
    if (Run("""
        class Capture {
            @static fun make(name: String): DotNet_Class {
                return class Holder2 {
                    @explicitThis fun get(this): String { return name; }
                } as DotNet_Class;
            }
        }
        Keep(Capture.make("captured"));
        """) is { } error)
        throw new Exception(error);

    var node = (Package.DotNet.Class)Kept();
    var instance = runner.EvaluateExpression(new C.Call { Callee = new C.Get(node) }).Unwrap()!;
    return (string?)Call(instance, "get") == "captured";
});

Try("a value return with no return type is an error", () =>
    Run("""class E1 { @explicitThis fun f(this) { return "x"; } }""") is { } e && e.Contains("nothing is returned"));
Try("a missing return is an error", () =>
    Run("class E2 { @explicitThis fun f(this): String { } }") is { } e && e.Contains("every path"));
Try("an empty return with a type is an error", () =>
    Run("class E3 { @explicitThis fun f(this): String { return; } }") is { } e && e.Contains("needs a value"));
Try("an untyped parameter is an error", () =>
    Run("class E4 { @explicitThis fun f(this, x) { } }") is { } e && e.Contains("needs a type"));
Try("a missing method is an error", () =>
    Run("class E6 { @explicitThis fun f(this) { this.nope(); } } ") is { } e && e.Contains("no member"));
Try("a bare method name is an error", () =>
    Run("class E7 { @explicitThis fun f(this) { g(); } @explicitThis fun g(this) { } }") is not null);
Try("a function outside a class is an error", () =>
    Run("fun loose() { }") is not null);
Try("a method in a module is an error", () =>
    Run("module E8 { fun loose() { } }") is not null);

// ---- Definitions go to the level that builds the code

Try("a definition in a field default is built once", () =>
{
    if (Run("""
        class Defaulted2 {
            let value: Object = class Once2 {}();
        }
        Keep(Defaulted2().value);
        """) is { } error)
        throw new Exception(error);

    var first = Kept();
    return first.GetType().Name == "Once2"
        && Run("Keep(Defaulted2().value);") is null
        && Kept().GetType() == first.GetType();
});

Try("a definition at document level runs before its statement", () =>
    Run("""Keep(class First {}());""") is null && Kept().GetType().Name == "First");

Try("a definition in a document block", () =>
{
    if (Run("""
        if (true) {
            Keep("x");
            Keep(class InBlock {}());
        }
        """) is { } error)
        throw new Exception(error);

    return Kept().GetType().Name == "InBlock";
});

Try("a definition in a module statement runs before it", () =>
{
    Probe.Kept.Clear();
    if (Run("""
        module M20 {
            Keep(class Early2 {});
            class Other2 {}
        }
        """) is { } error)
        throw new Exception(error);

    return Kept() is Package.DotNet.Class { Name: "Early2" };
});

// ---- Types are checked in MIR

Try("a local infers its type", () =>
{
    if (Run("""
        class Inferred {
            @explicitThis fun get(this): String {
                let text = "inferred";
                let same = text;
                return same;
            }
        }
        Keep(Inferred().get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "inferred";
});

Try("a local infers its type from a call", () =>
{
    if (Run("""
        class InferredCall {
            @explicitThis fun make(this): InferredCall { return InferredCall(); }
            @explicitThis fun get(this): String {
                let other = this.make();
                return other.text();
            }
            @explicitThis fun text(this): String { return "called"; }
        }
        Keep(InferredCall().get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "called";
});

Try("an inferred local keeps its type", () =>
    Run("""class I2 { @explicitThis fun f(this) { let text = "a"; text = true; } }""") is { } e && e.Contains("doesn't match"));

Try("a wrong argument is an error", () =>
    Run("""class T1 { @explicitThis fun take(this, text: String) { } @explicitThis fun f(this) { this.take(true); } }""") is { } e && e.Contains("can't be called with these arguments"));

Try("a wrong constructor argument is an error", () =>
    Run("""class T2 { @explicitThis new(this, text: String) { } @explicitThis fun f(this) { T2(true); } }""") is { } e && e.Contains("can't be called with these arguments"));

Try("a wrong argument to an imported method is an error", () =>
    Run("""class T3 { @explicitThis fun f(this) { Keep2(true); } }""") is { } e && e.Contains("can't be called with these arguments"));

Try("a wrong return value is an error", () =>
    Run("""class T4 { @explicitThis fun f(this): String { return true; } }""") is { } e && e.Contains("doesn't match"));

Try("a wrong field value is an error", () =>
    Run("""class T5 { let text: String; @explicitThis fun f(this) { this.text = true; } }""") is { } e && e.Contains("doesn't match"));

Try("a wrong field default is an error", () =>
    Run("""class T6 { let text: String = true; }""") is { } e && e.Contains("doesn't match"));

Try("a wrong parameter assignment is an error", () =>
    Run("""class T7 { @static fun f(text: String) { text = true; } }""") is { } e && e.Contains("doesn't match"));

Try("a derived class goes where its base is expected", () =>
{
    if (Run("""
        module M30 {
            class Base1 {
                @explicitThis fun name(this): String { return "base"; }
            }
            class Derived1 : Base1 {
                @explicitThis fun use(this): String { return Helper().read(this); }
            }
            class Helper {
                @explicitThis fun read(this, value: Base1): String { return value.name(); }
            }
        }
        Keep(M30.Derived1().use());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "base";
});

Try("a base class doesn't go where a derived one is expected", () =>
    Run("""
        module M31 {
            class Base2 { @explicitThis fun use(this) { Helper2().read(this); } }
            class Derived2 : Base2 { }
            class Helper2 { @explicitThis fun read(this, value: Derived2) { } }
        }
        """) is { } e && e.Contains("can't be called with these arguments"));

Try("a class in progress goes where Object is expected", () =>
{
    if (Run("""
        class AsObject {
            @explicitThis fun self(this): Object { return this; }
        }
        Keep(AsObject().self());
        """) is { } error)
        throw new Exception(error);

    return Kept().GetType().Name == "AsObject";
});

Try("a value type is boxed as an argument", () =>
{
    if (Run("""
        class Boxes {
            @static fun id(value: Object): Object { return value; }
        }
        Keep(Boxes.id(true));
        """) is { } error)
        throw new Exception(error);

    return Kept() is true;
});

Try("a value type is boxed for an imported method", () =>
    Run("Keep(true);") is null && Kept() is true);

Try("a value type is boxed when returned", () =>
{
    if (Run("""
        class Boxes2 {
            @explicitThis fun get(this): Object { return true; }
        }
        Keep(Boxes2().get());
        """) is { } error)
        throw new Exception(error);

    return Kept() is true;
});

Try("a value type is boxed when assigned", () =>
{
    if (Run("""
        class Boxes3 {
            let value: Object;
            @explicitThis new(this) { this.value = false; }
        }
        Keep(Boxes3().value);
        """) is { } error)
        throw new Exception(error);

    return Kept() is false;
});

Try("a value type parameter in a cast definition", () =>
{
    if (Run("""
        class Maker2 {
            @static fun make(flag: Boolean): DotNet_Class {
                return class Flagged {
                    @explicitThis fun get(this): Boolean { return flag; }
                } as DotNet_Class;
            }
        }
        Keep(Maker2.make(true));
        """) is { } error)
        throw new Exception(error);

    var node = (Package.DotNet.Class)Kept();
    var instance = runner.EvaluateExpression(new C.Call { Callee = new C.Get(node) }).Unwrap()!;
    return Call(instance, "get") is true;
});

Try("a method reaches a member of an imported base", () =>
{
    if (Run("""
        class Named {
            @explicitThis fun name(this): String { return this.ToString(); }
        }
        Keep(Named().name());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "Named";
});

// ---- Decorators, and this with no name

Try("a method is an instance method by default", () =>
{
    if (Run("""
        class Implicit {
            fun get(): String { return "implicit"; }
        }
        Keep(Implicit().get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "implicit" && !Kept2Method("Implicit", "get").IsStatic;
});

Try("this is not a special name", () =>
    Run("class NotThis { fun f(this) { } }") is { } e && e.Contains("needs a type"));

Try("an explicit this can have any name", () =>
{
    if (Run("""
        class Named2 {
            let value: String = "named";
            @explicitThis fun get(self): String { return self.value; }
        }
        Keep(Named2().get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "named" && Kept2Method("Named2", "get").GetParameters().Length == 0;
});

Try("an explicit this in a constructor", () =>
{
    if (Run("""
        class Built {
            let text: String;
            @explicitThis new(me, text: String) { me.text = text; }
        }
        Keep(Built("built").text);
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "built";
});

Try("a decorator makes a method static", () =>
{
    if (Run("""
        class Statics {
            @static fun make(): String { return "static"; }
        }
        Keep(Statics.make());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "static" && Kept2Method("Statics", "make").IsStatic;
});

Try("a static method has no explicit this", () =>
    Run("class S2 { @static @explicitThis fun f(self) { } }") is { } e && e.Contains("no this"));

Try("an explicit this needs a parameter", () =>
    Run("class S3 { @explicitThis fun f() { } }") is { } e && e.Contains("needs a parameter"));

Try("decorators apply nearest first", () =>
{
    Probe.Order.Clear();
    return Run("class Ordered { @first @second fun f() { } }") is null && Probe.Order is ["second", "first"];
});

Try("a decorated class at document level", () =>
{
    Probe.Marked.Clear();
    return Run("@mark class Marked1 {}") is null
        && Probe.Marked is ["Marked1"]
        && Run("Keep(Marked1());") is null
        && Kept().GetType().Name == "Marked1";
});

Try("a decorated class in a module", () =>
{
    Probe.Marked.Clear();
    return Run("module M40 { @mark class Inner40 {} }") is null
        && Probe.Marked is ["Inner40"]
        && Run("Keep(M40.Inner40());") is null
        && Kept().GetType().Name == "Inner40";
});

Try("a decorator runs after the fill", () =>
{
    Probe.Marked.Clear();
    return Run("class Filled { @markMethod fun later() { } }") is null && Probe.Marked is ["later"];
});

Try("a decorator is type checked", () =>
    Run("@markMethod class NotAMethod {}") is { } e && e.Contains("can't be called with these arguments"));

Try("only a definition can be decorated", () =>
    Run("""@mark Keep("x");""") is { } e && e.Contains("Only a definition"));

// ---- Binding names

var methodType = il.Load(typeof(Package.DotNet.Method));

Try("a new name is bound", () =>
{
    var scope = new ZSharp.Lang.Scope();
    var value = new C.StringLiteral("x");
    return binder.Bind(scope, "n", value) is null && scope.Get("n") == value;
});

Try("methods with one name merge into an overload group", () =>
{
    var scope = new ZSharp.Lang.Scope();
    C.Local[] methods = [new("f", methodType), new("f", methodType), new("f", methodType)];

    foreach (var method in methods)
        if (binder.Bind(scope, "f", new C.Get(method)) is { } error)
            throw new Exception(error.ToString());

    return scope.Get("f") is C.Get { Instance: C.OverloadGroup group }
        && group.Items.SequenceEqual(methods);
});

Try("a name bound twice is an error by default", () =>
{
    var scope = new ZSharp.Lang.Scope();
    binder.Bind(scope, "n", new C.StringLiteral("first"));
    return binder.Bind(scope, "n", new C.StringLiteral("second")) is { } error && error.ToString()!.Contains("already defined");
});

Try("a conflict can shadow instead", () =>
{
    var scope = new ZSharp.Lang.Scope();
    var second = new C.StringLiteral("second");
    mappedBinder.Conflict = ZSharp.Lang.Conflict.Shadow;

    try
    {
        binder.Bind(scope, "n", new C.StringLiteral("first"));
        return binder.Bind(scope, "n", second) is null && scope.Get("n") == second;
    }
    finally
    {
        mappedBinder.Conflict = ZSharp.Lang.Conflict.Error;
    }
});

Try("a local that isn't a let doesn't override", () =>
{
    var scope = new ZSharp.Lang.Scope();
    binder.Bind(scope, "x", new C.Get(new C.Local("x", il.Load(typeof(string)))));
    return binder.Bind(scope, "x", new C.Get(new C.Local("x", il.Load(typeof(string))))) is not null;
});

Try("two definitions with one name in a module are an error", () =>
    Run("module M50 { class Twice {} class Twice {} }") is { } e && e.Contains("already defined"));

Try("two methods with one name in a class merge", () =>
    Run("""class Overloaded { fun f() { } fun f(text: String) { } }""") is null);

Try("a let can hide a definition from an outer scope", () =>
    Run("""class LetOverDefinition { fun f() { let LetOverDefinition = "x"; } }""") is null);

Try("let reuses a name in the same block", () =>
{
    if (Run("""
        class Reuse {
            fun get(): String {
                let text = "first";
                let text = text;
                let text: String = "second";
                return text;
            }
        }
        Keep(Reuse().get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "second";
});

Try("a let can hide a name from an outer scope", () =>
    Run("""class Clash { fun f() { let Keep = "x"; } }""") is null);

Try("a name in an inner scope sees what it shadows", () =>
{
    var outer = new ZSharp.Lang.Scope();
    var inner = new ZSharp.Lang.Scope(outer);
    var seen = (C.Expression?)null;
    ZSharp.Lang.Shadow rule = (shadowed, value) => { seen = shadowed; return null; };
    var first = new C.StringLiteral("outer");

    binder.Bind(outer, "n", first);
    mappedBinder.Shadows.Add(rule);

    try
    {
        return binder.Bind(inner, "n", new C.StringLiteral("inner")) is null && seen == first && outer.Get("n") == first;
    }
    finally
    {
        mappedBinder.Shadows.Remove(rule);
    }
});

Try("an overload group calls the one that matches", () =>
{
    var group = new C.OverloadGroup();
    group.Items.Add(il.Load(typeof(Probe).GetMethod(nameof(Probe.PickString))!));
    group.Items.Add(il.Load(typeof(Probe).GetMethod(nameof(Probe.PickBool))!));

    var call = new C.Call { Callee = new C.Get(group), Arguments = [new() { Value = new C.StringLiteral("x") }] };
    return (string?)runner.EvaluateExpression(call).Unwrap() == "string";
});

Try("an overload group with no match is an error", () =>
{
    var group = new C.OverloadGroup();
    group.Items.Add(il.Load(typeof(Probe).GetMethod(nameof(Probe.PickString))!));

    var call = new C.Call { Callee = new C.Get(group), Arguments = [new() { Value = new C.BooleanLiteral(true) }] };
    return runner.EvaluateExpression(call).IsError;
});

// ---- Overloading

Try("a CLR overload is chosen by its parameter types", () =>
    Run("""Keep(Which(true));""") is null && (string)Kept() == "bool");

Try("the most specific CLR overload is chosen", () =>
    Run("""Keep(Which("x"));""") is null && (string)Kept() == "string");

Try("a less specific CLR overload is chosen when it is the only one that applies", () =>
    Run("""Keep(Which(Object));""") is null && (string)Kept() == "object");

Try("CLR overloads that are as specific as each other are ambiguous", () =>
    Run("""Keep(Either("x", "y"));""") is { } e && e.Contains("none of them is more specific"));

Try("the most specific Z# method is chosen", () =>
{
    if (Run("""
        class Over1 {
            @static fun pick(x: Object): String { return "object"; }
            @static fun pick(x: String): String { return "string"; }
        }
        Keep(Over1.pick("a"));
        Keep(Over1.pick(true));
        """) is { } error)
        throw new Exception(error);

    return Probe.Kept[^2] is "string" && (string)Kept() == "object";
});

Try("Z# methods are chosen inside the class too", () =>
{
    if (Run("""
        class Over2 {
            @static fun pick(x: String): String { return "string"; }
            @static fun pick(x: Boolean): String { return "bool"; }
            @static fun get(): String { return Over2.pick(true); }
        }
        Keep(Over2.get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "bool";
});

Try("Z# methods that are as specific as each other are ambiguous", () =>
    Run("""
        class Over3 {
            @static fun two(a: Object, b: String): String { return "first"; }
            @static fun two(a: String, b: Object): String { return "second"; }
        }
        Keep(Over3.two("x", "y"));
        """) is { } e && e.Contains("none of them is more specific"));

Try("a Z# constructor is chosen by its parameter types", () =>
{
    if (Run("""
        class Over4 {
            let tag: String;
            @explicitThis new(this, x: Object) { this.tag = "object"; }
            @explicitThis new(this, x: String) { this.tag = "string"; }
        }
        Keep(Over4("a").tag);
        Keep(Over4(true).tag);
        """) is { } error)
        throw new Exception(error);

    return Probe.Kept[^2] is "string" && (string)Kept() == "object";
});

Try("when no overload applies, each one says why", () =>
    Run("""Keep(Either(true, true));""") is { } e && e.Contains("None of the 2 overloads applies"));

// ---- Encoding values into code

Try("a value is encoded by its type", () =>
    Scope.HIRReifier.EncodeValue("x").Unwrap() is C.StringLiteral { Value: "x" }
    && Scope.HIRReifier.EncodeValue(true).Unwrap() is C.BooleanLiteral { Value: true }
    && Scope.HIRReifier.EncodeValue(typeof(string)).Unwrap() is C.Get { Instance: Package.DotNet.Class { Name: "System.String" } }
    && Scope.HIRReifier.EncodeValue(il.Load(typeof(object))).Unwrap() is C.Get { Instance: Package.DotNet.Class { Name: "System.Object" } }
    && Scope.HIRReifier.EncodeValue(new List<int>()).Unwrap() is C.Get { Instance: Package.DotNet.ExposedObject });

Try("a class used as a value is its type object", () =>
    Run("class ClassValue {} Keep(ClassValue);") is null && Kept() is Type { Name: "ClassValue" });

Try("a class still loading used as a value is its type object", () =>
{
    if (Run("""
        class SelfValue {
            fun get(): Object { return SelfValue; }
        }
        Keep(SelfValue());
        """) is { } error)
        throw new Exception(error);

    var instance = Kept();
    return ReferenceEquals(Call(instance, "get"), instance.GetType());
});

Try("a definition used by a cast definition inside a body", () =>
{
    if (Run("""
        module M60 {
            class Bar60 {
                @static fun name(): String { return "bar"; }
            }

            class Maker60 {
                @static fun make(): DotNet_Class {
                    return class Made60 {
                        fun get(): String { return Bar60.name(); }
                    } as DotNet_Class;
                }
            }
        }
        Keep(M60.Maker60.make());
        """) is { } error)
        throw new Exception(error);

    var node = (Package.DotNet.Class)Kept();
    var instance = runner.EvaluateExpression(new C.Call { Callee = new C.Get(node) }).Unwrap()!;
    return (string?)Call(instance, "get") == "bar";
});

Try("a captured string is encoded as a literal", () =>
{
    if (Run("""
        class Maker61 {
            @static fun make(text: String): DotNet_Class {
                return class Made61 {
                    fun get(): String { return text; }
                } as DotNet_Class;
            }
        }
        Keep(Maker61.make("captured"));
        """) is { } error)
        throw new Exception(error);

    var node = (Package.DotNet.Class)Kept();
    return node.Methods[0].Body!.Block.Statements is [C.Block { Statements: [C.Return { Value: C.StringLiteral { Value: "captured" } }] }];
});

Try("three levels of generation", () =>
{
    if (Run("""
        module M70 {
            class Bar70 {
                @static fun name(): String { return "bar"; }
            }

            class Maker70 {
                @static fun make(outer: String): DotNet_Class {
                    return class Middle70 {
                        fun make(inner: String): DotNet_Class {
                            return class Leaf70 {
                                fun getBar(): String { return Bar70.name(); }
                                fun getOuter(): String { return outer; }
                                fun getInner(): String { return inner; }
                            } as DotNet_Class;
                        }
                    } as DotNet_Class;
                }
            }
        }
        Keep(M70.Maker70.make("o"));
        """) is { } error)
        throw new Exception(error);

    object Create(Package.DotNet.Class node)
        => runner.EvaluateExpression(new C.Call { Callee = new C.Get(node) }).Unwrap()!;

    var middle = Create((Package.DotNet.Class)Kept());
    var leaf = Create((Package.DotNet.Class)Call(middle, "make", "i")!);

    return (string?)Call(leaf, "getBar") == "bar"
        && (string?)Call(leaf, "getOuter") == "o"
        && (string?)Call(leaf, "getInner") == "i";
});

// ---- A let is a local definition

Try("a let is a local of the method", () =>
{
    if (Run("""
        class LocalsOf {
            fun get(): String {
                let first = "a";
                let second: String = first;
                return second;
            }
        }
        Keep(LocalsOf().get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "a";
});

Try("a let starts where it is", () =>
{
    Probe.Kept.Clear();
    if (Run("""
        class Ordered2 {
            fun run() {
                Keep("before");
                let a = "a";
                Keep(a);
            }
        }
        Ordered2().run();
        """) is { } error)
        throw new Exception(error);

    return Probe.Kept is ["before", "a"];
});

Try("a let nested in a block", () =>
{
    if (Run("""
        class Nested3 {
            fun pick(yes: Boolean): String {
                let result = "no";
                if (yes) {
                    let inner = "yes";
                    result = inner;
                }
                return result;
            }
        }
        Keep(Nested3().pick(true));
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "yes";
});

Try("a method lists its locals", () =>
{
    if (Run("""
        class Listed {
            fun get(): String {
                let a = "a";
                if (true) { let b = "b"; }
                return a;
            }
        }
        """) is { } error)
        throw new Exception(error);

    return Run("Keep(Listed as DotNet_Class);") is null
        && Kept() is Package.DotNet.Class { Methods: [{ Locals: [{ Name: "a" }, { Name: "b" }] }] };
});

Try("a let in a cast definition inside a body", () =>
{
    if (Run("""
        class Maker72 {
            @static fun make(text: String): DotNet_Class {
                return class Made72 {
                    fun get(): String {
                        let copy = text;
                        return copy;
                    }
                } as DotNet_Class;
            }
        }
        Keep(Maker72.make("captured"));
        """) is { } error)
        throw new Exception(error);

    var node = (Package.DotNet.Class)Kept();
    var instance = runner.EvaluateExpression(new C.Call { Callee = new C.Get(node) }).Unwrap()!;
    return (string?)Call(instance, "get") == "captured";
});

Try("a type that wasn't inferred is an error", () =>
    Scope.HIRLoader.LoadType(ZSharp.HIR.Infer.Value) is { IsError: true } failure
    && failure.UnwrapError().ToString()!.Contains("inferred"));

Try("unquoted code outside reified code is an error", () =>
    Scope.HIRCodeLoader.LoadExpression(new C.Unquote(Str2("x"))) is { IsError: true } failure
    && failure.UnwrapError().ToString()!.Contains("reified"));

C.Expression Str2(string value) => new C.StringLiteral(value);

// ---- Documents

var documentsDirectory = Directory.CreateTempSubdirectory("zsharp-documents").FullName;

string WriteDocument(string name, string source)
{
    var path = Path.Combine(documentsDirectory, name);
    File.WriteAllText(path, source);
    return path.Replace("\\", "/");
}

const string Header = """
    import { String, static } from "core:runtime";
    import { export } from "core:zsharp";
    """;

Try("a document exports what is marked", () =>
    Run("@export class Exported1 {}") is null && document.Document.Exports.ContainsKey("Exported1"));

Try("a document doesn't export what isn't marked", () =>
    Run("class NotExported1 {}") is null && !document.Document.Exports.ContainsKey("NotExported1"));

// In a module, export's result is only an Instance, which the module can't register, so
// this fails as a type error before the export itself would.
Try("only a definition in a document can be exported", () =>
    Run("module M80 { @export class Inner80 {} }") is not null);

Try("importing a document gives what it exports", () =>
{
    var library = WriteDocument("library1.zs", Header + """
        @export
        class Library1 {
            @static fun hello(): String { return "imported"; }
        }
        """);

    return Run($$"""import { Library1 } from "{{library}}"; Keep(Library1.hello());""") is null
        && (string)Kept() == "imported";
});

Try("a name a document doesn't export can't be imported", () =>
{
    var library = WriteDocument("library2.zs", Header + "class Hidden2 {}");

    return Run($$"""import { Hidden2 } from "{{library}}"; Keep(Hidden2);""") is { } e && e.Contains("exports nothing named Hidden2");
});

Try("a document runs once however often it is imported", () =>
{
    var library = WriteDocument("library3.zs", Header + "@export class Once3 {}");

    return Run($$"""import { Once3 } from "{{library}}";""") is null
        && Run($$"""import { Once3 as Again3 } from "{{library}}"; Keep(Once3 as DotNet_Class); Keep(Again3 as DotNet_Class);""") is null
        && Probe.Kept is [var first, var second] && ReferenceEquals(first, second);
});

Try("a document imports another relative to itself", () =>
{
    WriteDocument("inner4.zs", Header + """
        @export
        class Inner4 {
            @static fun name(): String { return "relative"; }
        }
        """);
    var outer = WriteDocument("outer4.zs", Header + """
        import { Inner4 } from "inner4.zs";

        @export
        class Outer4 {
            @static fun name(): String { return Inner4.name(); }
        }
        """);

    return Run($$"""import { Outer4 } from "{{outer}}"; Keep(Outer4.name());""") is null
        && (string)Kept() == "relative";
});

Try("an import cycle is an error", () =>
{
    WriteDocument("cycle-b.zs", "import { a } from \"cycle-a.zs\";");
    var a = WriteDocument("cycle-a.zs", "import { b } from \"cycle-b.zs\";");

    return Run($$"""import { b } from "{{a}}";""") is { } e && e.Contains("import cycle");
});

Try("a missing document is an error", () =>
    Run("""import { x } from "no-such-document.zs";""") is { } e && e.Contains("no document"));

// ---- Imports are what their meta-compiler makes them

string Library(string name, string text) => WriteDocument(name + ".zs", Header + $$"""
    @export
    class {{name}} {
        @static fun hello(): String { return "{{text}}"; }
    }
    """);

Try("an import outside any meta-compiler that compiles it is an error", () =>
    compiler.CompileStatement(ParseOne("""import { a } from "x";""")) is { IsError: true } failure
    && failure.UnwrapError().ToString()!.Contains("Nothing here compiles the import"));

Try("an import in a body runs as the body is built", () =>
{
    var library = Library("Library90", "built");

    if (Run("""
        class Maker90 {
            @static fun make(src: String): DotNet_Class {
                return class Made90 {
                    fun get(): String {
                        import { Library90 as L } from src;
                        return L.hello();
                    }
                } as DotNet_Class;
            }
        }
        """) is { } error)
        throw new Exception(error);

    string Built(string path)
    {
        if (Run($$"""Keep(Maker90.make("{{path}}"));""") is { } failure)
            throw new Exception(failure);

        var node = (Package.DotNet.Class)Kept();
        var instance = runner.EvaluateExpression(new C.Call { Callee = new C.Get(node) }).Unwrap()!;
        return (string)Call(instance, "get")!;
    }

    return Built(library) == "built";
});

Try("each construction of a body runs its import again", () =>
{
    var first = WriteDocument("Again91a.zs", Header + """
        @export class Shared91 { @static fun hello(): String { return "one"; } }
        """);
    var second = WriteDocument("Again91b.zs", Header + """
        @export class Shared91 { @static fun hello(): String { return "two"; } }
        """);

    if (Run("""
        class Maker91 {
            @static fun make(src: String): DotNet_Class {
                return class Made91 {
                    fun get(): String {
                        import { Shared91 } from src;
                        return Shared91.hello();
                    }
                } as DotNet_Class;
            }
        }
        """) is { } error)
        throw new Exception(error);

    if (Run($$"""Keep(Maker91.make("{{first}}")); Keep(Maker91.make("{{second}}"));""") is { } failure)
        throw new Exception(failure);

    if (Probe.Kept is not [Package.DotNet.Class one, Package.DotNet.Class two])
        return false;
    Probe.Kept.Clear();

    object Make(Package.DotNet.Class node) => runner.EvaluateExpression(new C.Call { Callee = new C.Get(node) }).Unwrap()!;

    return (string?)Call(Make(one), "get") == "one" && (string?)Call(Make(two), "get") == "two";
});

Try("an import in a body can't use what the body is given", () =>
{
    var library = Library("Library92", "x");

    return Run("""
        class Uses92 {
            @static fun use(src: String): String {
                import { Library92 } from src;
                return Library92.hello();
            }
        }
        """) is { } e && e.Contains("Import");
});

Try("an import in a module runs as the module is constructed", () =>
{
    var library = Library("Library93", "from a module");

    if (Run($$"""
        module M93 {
            import { Library93 } from "{{library}}";

            class C93 {
                @static fun get(): String { return Library93.hello(); }
            }
        }
        Keep(M93.C93.get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "from a module";
});

Try("a module can't use what it imports while it is compiled", () =>
{
    var library = WriteDocument("Library94.zs", Header + """
        @export
        class Library94 {
            @static fun mark(definition: Object): Object { return definition; }
        }
        """);

    return Run($$"""
        module M94 {
            import { Library94 } from "{{library}}";

            @Library94.mark
            class C94 {}
        }
        """) is { } e && e.Contains("no member named Library94");
});

// ---- Forking

C.Expression FirstArgument(ZSharp.Compiler.ASTCompiler with)
    => ((C.Call)((C.ExpressionStatement)document.Within(with, () => with.CompileStatement(ParseOne("""Keep("x");"""))).Unwrap()).Expression)
        .Arguments[0].Value;

Try("a fork changes what is compiled inside the node too", () =>
{
    var fork = compiler.Fork();
    var previous = fork.Expression;

    fork.Expression = (current, expression) => expression is ZSharp.AST.LiteralExpression
        ? Result<C.Expression>.Ok(new C.StringLiteral("forked"))
        : previous(current, expression);

    return FirstArgument(fork) is C.StringLiteral { Value: "forked" }
        && FirstArgument(compiler) is C.StringLiteral { Value: "x" };
});

Try("a fork shares the contexts", () =>
    ReferenceEquals(compiler.Fork().Contexts, compiler.Contexts));

Try("a context offers what its parent offers", () =>
{
    var outer = new ZSharp.Lang.BuildContext(null, new C.Block());
    var inner = new ZSharp.Lang.BlockContext(outer, new ZSharp.Lang.Scope());

    return ((ZSharp.Compiler.Context)inner).Is<ZSharp.Lang.IBuildsDefinitions>(out _)
        && ((ZSharp.Compiler.Context)outer).Is<ZSharp.Lang.INames>(out _) is false
        && ((ZSharp.Compiler.Context)inner).Is<ZSharp.Lang.INames>(out var found) && ReferenceEquals(found, inner);
});

Try("a let with nothing that compiles it is an error", () =>
    compiler.CompileStatement(ParseOne("""let loose = "x";""")) is { IsError: true } failure
    && failure.UnwrapError().ToString()!.Contains("Nothing here compiles the let"));

// ---- Document locals

Try("a let in a document is a local of the document", () =>
    Run("""let d1 = "a"; Keep(d1);""") is null
    && (string)Kept() == "a"
    && document.Document.Locals.Any(local => local.Name == "d1"));

Try("a document local can be assigned", () =>
    Run("""let d2 = "a"; d2 = "b"; Keep(d2);""") is null && (string)Kept() == "b");

Try("a document local with a type", () =>
    Run("""let d3: String = "typed"; Keep(d3);""") is null && (string)Kept() == "typed");

Try("a document local's value must match its type", () =>
    Run("""let d4: String = true;""") is { } e && e.Contains("doesn't match"));

Try("a let in a document reuses a name", () =>
    Run("""let d5 = "first"; let d5 = "second"; Keep(d5);""") is null && (string)Kept() == "second");

Try("a document local of a value type", () =>
    Run("""let d6: Boolean = true; d6 = false; Keep(d6);""") is null && Kept() is false);

Try("a method gets the value a document local had when it was built", () =>
{
    if (Run("""
        let d7 = "when built";
        class Reads7 { @static fun get(): String { return d7; } }
        d7 = "later";
        Keep(Reads7.get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "when built";
});

Try("a document local in a nested block", () =>
    Run("""if (true) { let inner8 = "nested"; Keep(inner8); }""") is null && (string)Kept() == "nested");

Try("a document local in a block isn't seen after it", () =>
    Run("""if (true) { let scoped9 = "x"; } Keep(scoped9);""") is { } e && e.Contains("scoped9"));

Try("a nested statement is only compiled when it is reached", () =>
    Run("""if (false) { Keep(neverDefined10); } Keep("reached");""") is null && (string)Kept() == "reached");

Try("a condition sees the document locals", () =>
    Run("""let d11: Boolean = false; if (d11) { Keep("then"); } else { Keep("else"); }""") is null
    && (string)Kept() == "else");

System.Reflection.MethodInfo Kept2Method(string type, string method)
    => Probe.LastType(type).GetMethod(method)!;

public static class Probe
{
    public static Type LastType(string name)
        => AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => { try { return assembly.GetTypes(); } catch { return []; } })
            .Last(type => type.Name == name);

    public static readonly List<int> Counts = [];

    public static void FieldCount(Package.DotNet.Class node) => Counts.Add(node.Fields.Count);

    public static readonly List<object> Kept = [];

    public static void Keep(object value) => Kept.Add(value);

    public static void Keep2(string value) => Kept.Add(value);

    public static readonly List<string> Order = [];

    public static string PickString(string value) => "string";

    public static string PickBool(bool value) => "bool";

    public static string Which(object value) => "object";

    public static string Which(string value) => "string";

    public static string Which(bool value) => "bool";

    public static string Either(object first, string second) => "first";

    public static string Either(string first, object second) => "second";

    [ZSharp.Importer.ILLoader.Alias("first")]
    public static Package.DotNet.Method First(Package.DotNet.Method method) { Order.Add("first"); return method; }

    [ZSharp.Importer.ILLoader.Alias("second")]
    public static Package.DotNet.Method Second(Package.DotNet.Method method) { Order.Add("second"); return method; }

    public static readonly List<string> Marked = [];

    [ZSharp.Importer.ILLoader.Alias("mark")]
    public static Package.DotNet.Class Mark(Package.DotNet.Class node) { Marked.Add(node.Name); return node; }

    [ZSharp.Importer.ILLoader.Alias("markMethod")]
    public static Package.DotNet.Method MarkMethod(Package.DotNet.Method node) { Marked.Add(node.Name); return node; }
}
