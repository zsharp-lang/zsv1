using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;
using ZSharp.Lang;
using ZSharp.Lang.DotNet;
using ZSharp.Parser;
using ZSharp.Platform.Runtime;
using ZSharp.Tokenizer;
using C = ZSharp.HIR.Code;
using Scope = ZSharp.Compiler.Current;

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
Scope.MappedLoader = mapped;
Scope.MappedCodeLoader = mappedCode;
mapped.Apply();
mappedCode.Apply();

new ZSharp.Compiler.CGDispatchers.Direct.Dispatcher(Scope.CG).Apply();
new ZSharp.Compiler.CGDispatchers.Typed.Dispatcher(Scope.CG, Scope.TS).Apply();
new ZSharp.Compiler.MatchingDispatchers.Static.Dispatcher(Scope.Matching, Scope.TS).Apply();

// Members from outside an object's own, as extension methods, come after its own.
var external = new ZSharp.Compiler.CGDispatchers.External.Dispatcher(Scope.CG, Scope.Overloading);
external.Apply();
external.Add(il.Extensions);
new ZSharp.Compiler.TSDispatchers.Static.Dispatcher(Scope.TS).Apply();
new ZSharp.Compiler.IRDispatchers.Static.Dispatcher(Scope.IR).Apply();
new ZSharp.Compiler.OverloadingDispatchers.Static.Dispatcher(Scope.Overloading).Apply();

CodeLoaders.Register(mappedCode, mapped, Scope.HIRCodeLoader, Scope.HIRLoader, Scope.CG, Scope.Matching, Scope.Overloading);
il.Install(mapped, mappedCode);

// A kind of parameter defined outside the toolchain: it is injected, so a call gives it
// nothing, and the code it belongs to uses it as any parameter.
mapped.Definitions.Register<Injected>(parameter => Result<ZSharp.MIR.CompilerObject>.Ok(new InjectedMatch(parameter.Value)));
mapped.Instances.Register<Injected>(parameter => il.LoadParameter(parameter));

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
Core.ZSharp.ModuleScope.Language = language;
Core.ZSharp.ModuleScope.Runner = statement => runner.RunStatement(statement);
Core.ZSharp.ModuleScope.Evaluator = expression => runner.EvaluateExpression(expression);
ZSharp.Compiler.DocumentLoaders.Install(mapped, Scope.HIRLoader);

var core = new ZSharp.Importer.String.LibraryImporter("core");
core.Add("runtime", il.Load(typeof(Core.Runtime.ModuleScope).Module));
core.Add("zsharp", il.Load(typeof(Core.ZSharp.ModuleScope).Module));
core.Add("compiler", il.Load(typeof(Core.Compiler.ModuleScope).Module));
Core.ZSharp.ModuleScope.Libraries = core;

var std = new ZSharp.Importer.String.LibraryImporter("standard");

foreach (var (name, module) in ZSharp.Toolchain.ToolchainModules.All())
    std.Add(name, il.Load(module));

var importer = new ZSharp.Importer.String.StringImporter();
var host = new ZSharp.Lang.Runtime.RuntimeDocuments(
    runtime, il,
    instance => Scope.HIRLoader.LoadInstance(instance).When(out _).Error(out var error) ? error : null
);
host.Install(mapped);
host.Install(reifier);
DocumentCompiler.Install(mappedBinder);

var document = new DocumentCompiler(language, host);
var documents = new DocumentImporter(language, compiler, host);
importer.Register("core", core);
importer.Register("std", std);
var fileImporter = new FileImporter(compiler);
fileImporter.Register(".zs", documents.Import);
importer.DefaultImporter = fileImporter;
Core.ZSharp.ModuleScope.Files = fileImporter;
Core.ZSharp.ModuleScope.Importers = importer;
Core.ZSharp.ModuleScope.Documents = documents;

var assemblies = new Assemblies(il, Core.ZSharp.ModuleScope.Describers) { ImportLibrary = source => importer.Import(source) };
Core.ZSharp.ModuleScope.Enter = assemblies.Enter;
fileImporter.Register(".dll", assemblies.ImportFile);

if (ZSharp.Importer.String.DocumentLibrary.Above(AppContext.BaseDirectory, "std") is { } standard)
{
    var library = new ZSharp.Importer.String.DocumentLibrary(standard, documents, "std", path => CommonZ.Result<ZSharp.HIR.Instance>.Ok(assemblies.Import(Assemblies.Read(path))));
    std.Fallback = library;
    System.Runtime.Loader.AssemblyLoadContext.Default.Resolving += (_, name) => library.Assembly(name.Name!) is { } path ? Assemblies.Read(path) : null;
}
language.Install(mappedCompiler, IsUnderlying);
Core.ZSharp.ModuleScope.Importer = importer.Import;
Core.ZSharp.ModuleScope.MetaTypes = DotNetMetaTypes.Install(language, mappedCompiler, mappedBinder, runner, il, out var bodies);
Core.ZSharp.ModuleScope.Bodies = bodies;

foreach (var library in new[] { typeof(Core.Runtime.ModuleScope), typeof(Core.ZSharp.ModuleScope) })
    foreach (var (syntax, function) in il.Operators(library))
        language.Add(syntax, function);

foreach (var (syntax, function) in il.ClrOperators())
    language.Add(syntax, function);

foreach (var (syntax, function) in il.LiftedOperators(language.Functions))
    language.Add(syntax, function);

var probe = new C.Get(il.Load(typeof(Probe)));
document.Scope.Add("Keep", new C.NamedMember { Target = probe, Name = "Keep" });
document.Scope.Add("String", new C.Get(il.Load(typeof(string))));
document.Scope.Add("Boolean", new C.Get(il.Load(typeof(bool))));
document.Scope.Add("Int32", new C.Get(il.Load(typeof(int))));

// CLR types the runtime integration tests use.
foreach (var (name, clr) in new (string, Type)[] {
    ("TimeSpan", typeof(TimeSpan)), ("DateTime", typeof(DateTime)), ("Guid", typeof(Guid)), ("Double", typeof(double)),
    ("DayOfWeek", typeof(DayOfWeek)), ("Environment", typeof(Environment)), ("Probe", typeof(Probe)),
    ("Int64", typeof(long)), ("Decimal", typeof(decimal)), ("Math", typeof(Math)), ("Object", typeof(object)),
    ("IDisposable154", typeof(IDisposable)), ("Interlocked", typeof(System.Threading.Interlocked)),
    ("Bell", typeof(Bell)), ("EventArgs", typeof(EventArgs)),
    ("IHasLabel", typeof(IHasLabel)),
    ("GenericProbe", typeof(GenericProbe)),
    ("Array", typeof(Array)), ("Enumerable", typeof(System.Linq.Enumerable)), ("Nullable", typeof(Nullable<>)),
    ("Task", typeof(System.Threading.Tasks.Task)) })
    document.Scope.Add(name, new C.Get(il.Load(clr)));
document.Scope.Add("Keep2", new C.NamedMember { Target = probe, Name = "Keep2" });
document.Scope.Add("Which", new C.NamedMember { Target = probe, Name = "Which" });
foreach (var (name, member) in new[] { ("PairOf", "Pair"), ("GreetWith", "Greet"), ("NamedBy", "Named"), ("NextNumber", "Next"), ("Same", "Same"), ("Upper", "Upper"), ("ApplyTo", "ApplyTo") })
    document.Scope.Add(name, new C.NamedMember { Target = probe, Name = member });
document.Scope.Add("Either", new C.NamedMember { Target = probe, Name = "Either" });

var coreRuntime = new C.Get(il.Load(typeof(Core.Runtime.ModuleScope).Module));
document.Scope.Add("explicitThis", new C.NamedMember { Target = coreRuntime, Name = "explicitThis" });
document.Scope.Add("static", new C.NamedMember { Target = coreRuntime, Name = "static" });
foreach (var name in new[] { "virtual", "abstract", "override", "DotNet_Interface", "public", "private", "internal", "protected" })
    document.Scope.Add(name, new C.NamedMember { Target = coreRuntime, Name = name });
document.Scope.Add("export", new C.NamedMember { Target = new C.Get(il.Load(typeof(Core.ZSharp.ModuleScope).Module)), Name = "export" });
foreach (var name in new[] { "ct", "load" })
    document.Scope.Add(name, new C.NamedMember { Target = new C.Get(il.Load(typeof(Core.ZSharp.ModuleScope).Module)), Name = name });

foreach (var decorator in new[] { "first", "second", "mark", "markMethod", "seen", "twice", "inject" })
    document.Scope.Add(decorator, new C.NamedMember { Target = probe, Name = decorator });
document.Scope.Add("Object", new C.Get(il.Load(typeof(object))));
document.Scope.Add("DotNet_Class", new C.Get(il.Load(typeof(Package.DotNet.Class))));
document.Scope.Add("DotNet_Module", new C.Get(il.Load(typeof(Package.DotNet.Module))));
document.Scope.Add("FieldCount", new C.NamedMember { Target = probe, Name = "FieldCount" });

ZSharp.AST.Statement ParseOne(string source)
{
    var parser = new Parser(Tokenizer.Tokenize(new(new StringReader(source))));
    return ZSharp.Syntax.Grammar.Install(parser).Document.ParseStatement(parser);
}

// Runs source statement by statement, returning the first error.
// Whether code fails, as code nothing defines the meaning of does, whether its syntax
// or the language is what doesn't define it.
bool Fails(string source)
{
    try
    {
        return Run(source) is not null;
    }
    catch (Exception)
    {
        return true;
    }
}

string? Run(string source)
{
    var parser = new Parser(Tokenizer.Tokenize(new(new StringReader(source))));
    var syntax = ZSharp.Syntax.Grammar.Install(parser);

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
    @public def Animal { @public let name: String; }
    @public def Dog : Animal { @public let friend: Dog; }
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
    if (Run("Keep(def Plan { @public let note: String; @public let self: Plan; } as DotNet_Class);") is { } error)
        throw new Exception(error);

    return Kept() is Package.DotNet.Class { Name: "Plan", Fields: [{ Name: "note" }, { Name: "self" }] } plan
        && plan.Fields[1].Type is Package.DotNet.Class self
        && ReferenceEquals(self, plan);
});

Try("structure loads only when asked", () =>
{
    Run("Keep(def Later { @public let self: Later; } as DotNet_Class);");
    var node = (Package.DotNet.Class)Kept();
    var loaded = Scope.HIRLoader.LoadType(node).Unwrap();
    return Scope.IR.CompileType(loaded, runtime).IsOk
        && Scope.HIRLoader.LoadType(node).IsOk;
});

Try("expression definition has identity but binds no name outside", () =>
{
    if (Run("Keep(def Temp {}());") is { } error)
        throw new Exception(error);

    return Kept().GetType().Name == "Temp" && Run("Keep(Temp());")!.Contains("not defined");
});

Try("expression definition binds its name inside itself", () =>
{
    if (Run("Keep(def Node { @public let next: Node; }());") is { } error)
        throw new Exception(error);

    var node = Kept().GetType();
    return node.GetField("next")?.FieldType == node;
});

Try("of with the CLR class meta-type", () =>
    Run("@public def Explicit of DotNet_Class {}") is null && Run("Keep(Explicit());") is null && Kept().GetType().Name == "Explicit");

Try("two classes can share a name", () =>
{
    Run("Keep(def Twin {}());");
    var first = Kept().GetType();
    Run("Keep(def Twin {}());");
    return Kept().GetType() != first;
});

Try("unsupported meta-type is an error", () => Run("def X of String {}") is { } e && e.Contains("meta-type"));
Try("field without a type is an error", () => Run("""@public def X { @public let a = "x"; }""") is { } e && e.Contains("needs a type"));
Try("unknown field type is an error", () => Run("@public def X { @public let a: Missing; }") is { } e && e.Contains("not defined"));
Try("non-type field type is an error", () => Run("""@public def X { @public let a: "text"; }""") is not null);
Try("two base classes is an error", () => Run("@public def TwoBases129 : String, String {}") is { } e && e.Contains("one base class"));

Try("a failed class leaves the name unusable, not half made", () =>
    Run("@public def Bad { @public let a: Missing; }") is not null && Run("Keep(Bad());")!.Contains("not defined"));

Try("build code reifies into a body", () =>
{
    var definition = (ZSharp.AST.Definition)((ZSharp.AST.DefinitionStatement)ParseOne("def Made { let self: Made; let s: String; }")).Definition;
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
            @public def A { @public let b: B; }
            @public def B { @public let a: A; }
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
            @public def C : D {}
            @public def D { @public let tag: String; }
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
            @public def Foo {}
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
            @public def F { @public let x: String; @public let y: String; }
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
            @public def F { @public let x: String; }
            Keep(def Inner {});
        }
        """) is { } error)
        throw new Exception(error);

    return Kept() is Package.DotNet.Class { Name: "Inner" };
});

Try("a statement can't use a later definition", () =>
    Run("""
        module M6 {
            Keep(Later);
            @public def Later {}
        }
        """) is { } e && e.Contains("not defined"));

Try("a definition can't be called in its own module", () =>
    Run("""
        module M7 {
            @public def Z {}
            Keep(Z());
        }
        """) is not null);

Try("a definition can't be a meta-type in its own module", () =>
    Run("""
        module M8 {
            @public def Meta {}
            def Y of Meta {}
        }
        """) is { } e && e.Contains("own module"));

Try("module members stay inside the module", () =>
    Run("module M9 { @public def OnlyInside {} }") is null && Run("Keep(OnlyInside());") is { } e && e.Contains("not defined"));

Try("a missing module member is an error", () =>
    Run("Keep(M9.Nothing());") is { } e && e.Contains("no member"));

Try("a whole file module", () =>
{
    if (Run("""
        module M10;
        @public def Whole { @public let self: Whole; }
        """) is { } error)
        throw new Exception(error);

    return Run("Keep(M10.Whole());") is null && Kept().GetType().Name == "Whole";
});

Try("class level code runs while the class is built", () =>
{
    Probe.Kept.Clear();
    return Run("""@public def Cl { Keep("at class"); @public let s: String; }""") is null && (string)Kept() == "at class";
});

Try("an identity definition in class code is plain", () =>
{
    if (Run("""@public def Cm { Keep(def Nested {}); }""") is { } error)
        throw new Exception(error);

    return Kept() is Package.DotNet.Class { Name: "Nested" };
});

Try("a module can use earlier classes", () =>
{
    if (Run("""
        @public def Outside {}
        module M11 { @public def Uses : Outside { @public let o: Outside; } }
        Keep(M11.Uses());
        """) is { } error)
        throw new Exception(error);

    return Kept().GetType().BaseType?.Name == "Outside";
});

Try("document level has no forward references", () =>
    Run("""
        @public def Early { @public let l: Latecomer; }
        @public def Latecomer {}
        """) is { } e && e.Contains("not defined"));

Try("base cycle is an error", () =>
    Run("""
        module M12 {
            @public def P : Q {}
            @public def Q : P {}
        }
        """) is { } e && e.Contains("derives from itself"));

Try("sealed base is an error", () =>
    Run("@public def S1 : String {}") is { } e && e.Contains("sealed"));

Try("module cast is structure", () =>
{
    var module = (ZSharp.AST.Definition)((ZSharp.AST.DefinitionStatement)ParseOne("module M13 { @public def V {} }")).Definition;
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
        @public def Greeter {
            @public let name: String;

            @public @explicitThis new(this, name: String) { this.name = name; }

            @public @explicitThis fun hello(this): String { return this.name; }
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
        @public def Maker {
            @public @static fun make(text: String): String { return text; }
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
        @public def Chain {
            @public @explicitThis fun first(this): String { return this.second("end"); }
            @public @explicitThis fun second(this, text: String): String { return text; }
        }
        Keep(Chain().first());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "end";
});

Try("a method reads an inherited field", () =>
{
    if (Run("""
        @public def Parent { @public let tag: String; @public @explicitThis new(this) { this.tag = "inherited"; } }
        @public def Child : Parent {
            @public @explicitThis fun read(this): String { return this.tag; }
        }
        Keep(Child().read());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "inherited";
});

Try("a method constructs its own class", () =>
{
    if (Run("""
        @public def Self {
            @public let text: String;
            @public @explicitThis new(this, text: String) { this.text = text; }
            @public @explicitThis fun copy(this): Self { return Self(this.text); }
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
            @public def Left {
                @public @explicitThis fun other(this): Right { return Right(); }
            }
            @public def Right {
                @public @explicitThis fun name(this): String { return "right"; }
                @public @explicitThis fun back(this): Left { return Left(); }
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
        @public def Defaulted {
            @public let label: String = "default";
        }
        Keep(Defaulted().label);
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "default";
});

Try("field default with a constructor", () =>
{
    if (Run("""
        @public def Both {
            @public let label: String = "first";
            @public let other: String;
            @public @explicitThis new(this) { this.other = this.label; }
        }
        Keep(Both().other);
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "first";
});

Try("locals and control flow", () =>
{
    if (Run("""
        @public def Flow {
            @public @explicitThis fun pick(this, yes: Boolean): String {
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
        @public def Assign {
            @public @static fun change(text: String): String {
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
        @public def Factory {
            @public @explicitThis fun make(this): DotNet_Class {
                return def Made2 { @public let x: String; } as DotNet_Class;
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
        @public def Holder {
            @public @explicitThis fun make(this): Object { return def Once {}(); }
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
        @public def Capture {
            @public @static fun make(name: String): DotNet_Class {
                return def Holder2 {
                    @public @explicitThis fun get(this): String { return name; }
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
    Run("""@public def E1 { @public @explicitThis fun f(this) { return "x"; } }""") is { } e && e.Contains("nothing is returned"));
Try("a missing return is an error", () =>
    Run("@public def E2 { @public @explicitThis fun f(this): String { } }") is { } e && e.Contains("every path"));
Try("an empty return with a type is an error", () =>
    Run("@public def E3 { @public @explicitThis fun f(this): String { return; } }") is { } e && e.Contains("needs a value"));
Try("an untyped parameter is an error", () =>
    Run("@public def E4 { @public @explicitThis fun f(this, x) { } }") is { } e && e.Contains("needs a type"));
Try("a missing method is an error", () =>
    Run("@public def E6 { @public @explicitThis fun f(this) { this.nope(); } } ") is { } e && e.Contains("no member"));
Try("a bare method name is an error", () =>
    Run("@public def E7 { @public @explicitThis fun f(this) { g(); } @public @explicitThis fun g(this) { } }") is not null);
Try("a function outside a class is an error", () =>
    Run("fun loose() { }") is not null);

// ---- Definitions go to the level that builds the code

Try("a definition in a field default is built once", () =>
{
    if (Run("""
        @public def Defaulted2 {
            @public let value: Object = def Once2 {}();
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
    Run("""Keep(def First {}());""") is null && Kept().GetType().Name == "First");

Try("a definition in a document block", () =>
{
    if (Run("""
        if (true) {
            Keep("x");
            Keep(def InBlock {}());
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
            Keep(def Early2 {});
            @public def Other2 {}
        }
        """) is { } error)
        throw new Exception(error);

    return Kept() is Package.DotNet.Class { Name: "Early2" };
});

// ---- Types are checked in MIR

Try("a local infers its type", () =>
{
    if (Run("""
        @public def Inferred {
            @public @explicitThis fun get(this): String {
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
        @public def InferredCall {
            @public @explicitThis fun make(this): InferredCall { return InferredCall(); }
            @public @explicitThis fun get(this): String {
                let other = this.make();
                return other.text();
            }
            @public @explicitThis fun text(this): String { return "called"; }
        }
        Keep(InferredCall().get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "called";
});

Try("an inferred local keeps its type", () =>
    Run("""@public def I2 { @public @explicitThis fun f(this) { let text = "a"; text = true; } }""") is { } e && e.Contains("doesn't match"));

Try("a wrong argument is an error", () =>
    Run("""@public def T1 { @public @explicitThis fun take(this, text: String) { } @public @explicitThis fun f(this) { this.take(true); } }""") is { } e && e.Contains("can't be called with these arguments"));

Try("a wrong constructor argument is an error", () =>
    Run("""@public def T2 { @public @explicitThis new(this, text: String) { } @public @explicitThis fun f(this) { T2(true); } }""") is { } e && e.Contains("can't be called with these arguments"));

Try("a wrong argument to an imported method is an error", () =>
    Run("""@public def T3 { @public @explicitThis fun f(this) { Keep2(true); } }""") is { } e && e.Contains("can't be called with these arguments"));

Try("a wrong return value is an error", () =>
    Run("""@public def T4 { @public @explicitThis fun f(this): String { return true; } }""") is { } e && e.Contains("doesn't match"));

Try("a wrong field value is an error", () =>
    Run("""@public def T5 { @public let text: String; @public @explicitThis fun f(this) { this.text = true; } }""") is { } e && e.Contains("doesn't match"));

Try("a wrong field default is an error", () =>
    Run("""@public def T6 { @public let text: String = true; }""") is { } e && e.Contains("doesn't match"));

Try("a wrong parameter assignment is an error", () =>
    Run("""@public def T7 { @public @static fun f(text: String) { text = true; } }""") is { } e && e.Contains("doesn't match"));

Try("a derived class goes where its base is expected", () =>
{
    if (Run("""
        module M30 {
            @public def Base1 {
                @public @explicitThis fun name(this): String { return "base"; }
            }
            @public def Derived1 : Base1 {
                @public @explicitThis fun use(this): String { return Helper().read(this); }
            }
            @public def Helper {
                @public @explicitThis fun read(this, value: Base1): String { return value.name(); }
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
            @public def Base2 { @public @explicitThis fun use(this) { Helper2().read(this); } }
            @public def Derived2 : Base2 { }
            @public def Helper2 { @public @explicitThis fun read(this, value: Derived2) { } }
        }
        """) is { } e && e.Contains("can't be called with these arguments"));

Try("a class in progress goes where Object is expected", () =>
{
    if (Run("""
        @public def AsObject {
            @public @explicitThis fun self(this): Object { return this; }
        }
        Keep(AsObject().self());
        """) is { } error)
        throw new Exception(error);

    return Kept().GetType().Name == "AsObject";
});

Try("a value type is boxed as an argument", () =>
{
    if (Run("""
        @public def Boxes {
            @public @static fun id(value: Object): Object { return value; }
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
        @public def Boxes2 {
            @public @explicitThis fun get(this): Object { return true; }
        }
        Keep(Boxes2().get());
        """) is { } error)
        throw new Exception(error);

    return Kept() is true;
});

Try("a value type is boxed when assigned", () =>
{
    if (Run("""
        @public def Boxes3 {
            @public let value: Object;
            @public @explicitThis new(this) { this.value = false; }
        }
        Keep(Boxes3().value);
        """) is { } error)
        throw new Exception(error);

    return Kept() is false;
});

Try("a value type parameter in a cast definition", () =>
{
    if (Run("""
        @public def Maker2 {
            @public @static fun make(flag: Boolean): DotNet_Class {
                return def Flagged {
                    @public @explicitThis fun get(this): Boolean { return flag; }
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
        @public def Named {
            @public @explicitThis fun name(this): String { return this.ToString(); }
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
        @public def Implicit {
            @public fun get(): String { return "implicit"; }
        }
        Keep(Implicit().get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "implicit" && !Kept2Method("Implicit", "get").IsStatic;
});

Try("this is not a special name", () =>
    Run("@public def NotThis { @public fun f(this) { } }") is { } e && e.Contains("needs a type"));

Try("an explicit this can have any name", () =>
{
    if (Run("""
        @public def Named2 {
            @public let value: String = "named";
            @public @explicitThis fun get(self): String { return self.value; }
        }
        Keep(Named2().get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "named" && Kept2Method("Named2", "get").GetParameters().Length == 0;
});

Try("an explicit this in a constructor", () =>
{
    if (Run("""
        @public def Built {
            @public let text: String;
            @public @explicitThis new(me, text: String) { me.text = text; }
        }
        Keep(Built("built").text);
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "built";
});

Try("a decorator makes a method static", () =>
{
    if (Run("""
        @public def Statics {
            @public @static fun make(): String { return "static"; }
        }
        Keep(Statics.make());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "static" && Kept2Method("Statics", "make").IsStatic;
});

Try("a static method has no explicit this", () =>
    Run("@public def S2 { @public @static @explicitThis fun f(self) { } }") is { } e && e.Contains("no this"));

Try("an explicit this needs a parameter", () =>
    Run("@public def S3 { @public @explicitThis fun f() { } }") is { } e && e.Contains("needs a parameter"));

Try("decorators apply nearest first", () =>
{
    Probe.Order.Clear();
    return Run("@public def Ordered { @public @first @second fun f() { } }") is null && Probe.Order is ["second", "first"];
});

Try("a decorated class at document level", () =>
{
    Probe.Marked.Clear();
    return Run("@mark @public def Marked1 {}") is null
        && Probe.Marked is ["Marked1"]
        && Run("Keep(Marked1());") is null
        && Kept().GetType().Name == "Marked1";
});

Try("a decorated class in a module", () =>
{
    Probe.Marked.Clear();
    return Run("module M40 { @mark @public def Inner40 {} }") is null
        && Probe.Marked is ["Inner40"]
        && Run("Keep(M40.Inner40());") is null
        && Kept().GetType().Name == "Inner40";
});

Try("a decorator runs after the fill", () =>
{
    Probe.Marked.Clear();
    return Run("@public def Filled { @public @markMethod fun later() { } }") is null && Probe.Marked is ["later"];
});

Try("a decorator can be defined in Z#", () =>
{
    if (Run("""
        @public def Decorators {
            @public @static fun named(node: DotNet_Class): DotNet_Class { Keep(node); return node; }
        }
        @Decorators.named def DecoratedInZ {}
        Keep(DecoratedInZ());
        """) is { } error)
        throw new Exception(error);

    return Probe.Kept[^2] is Package.DotNet.Class { Name: "DecoratedInZ" } && Kept().GetType().Name == "DecoratedInZ";
});

Try("a decorator is type checked", () =>
    Run("@markMethod @public def NotAMethod {}") is { } e && e.Contains("can't be called with these arguments"));

Try("a decorator that doesn't take a statement can't decorate one", () =>
    Run("""@mark Keep("x");""") is { } e && e.Contains("can't be called with these arguments"));

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
    Run("module M50 { @public def Twice {} @public def Twice {} }") is { } e && e.Contains("already defined"));

Try("two methods with one name in a class merge", () =>
    Run("""@public def Overloaded { @public fun f() { } @public fun f(text: String) { } }""") is null);

Try("a let can hide a definition from an outer scope", () =>
    Run("""@public def LetOverDefinition { @public fun f() { let LetOverDefinition = "x"; } }""") is null);

Try("let reuses a name in the same block", () =>
{
    if (Run("""
        @public def Reuse {
            @public fun get(): String {
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
    Run("""@public def Clash { @public fun f() { let Keep = "x"; } }""") is null);

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

// ---- Captures

string Captured(string source)
{
    if (Run(source) is { } error)
        throw new Exception(error);

    return (string)Kept();
}

Try("a local function reads a variable around it by reference", () => Captured("""
    @public def Cap1 {
        @public @static fun run(): String {
            let x = "before";
            fun read(): String { return x; }
            x = "after";
            return read();
        }
    }
    Keep(Cap1.run());
    """) == "after");

Try("a local function assigns a variable around it", () => Captured("""
    @public def Cap2 {
        @public @static fun run(): String {
            let x = "before";
            fun change() { x = "changed"; }
            change();
            return x;
        }
    }
    Keep(Cap2.run());
    """) == "changed");

Try("local functions share a capture", () => Captured("""
    @public def Cap3 {
        @public @static fun run(): String {
            let x = "start";
            fun set() { x = "set"; }
            fun get(): String { return x; }
            set();
            return get();
        }
    }
    Keep(Cap3.run());
    """) == "set");

Try("code from before a capture uses the captured variable", () => Captured("""
    @public def Cap4 {
        @public @static fun run(): String {
            let x = "before";
            let y = x;
            fun change() { x = "changed"; }
            change();
            return x;
        }
    }
    Keep(Cap4.run());
    """) == "changed");

Try("a capture made in a branch is the variable after the branch", () => Captured("""
    @public def Cap12 {
        @public @static fun run(): String {
            let x = "before";
            if (true) {
                fun change() { x = "changed"; }
                change();
            }
            return x;
        }
    }
    Keep(Cap12.run());
    """) == "changed");

Try("a capture made in a branch not taken leaves the variable as it is", () => Captured("""
    @public def Cap13 {
        @public @static fun run(): String {
            let x = "before";
            if (false) {
                fun change() { x = "changed"; }
                change();
            }
            return x;
        }
    }
    Keep(Cap13.run());
    """) == "before");

Try("a function captures through the function it is in", () => Captured("""
    @public def Cap5 {
        @public @static fun run(): String {
            let x = "before";
            fun middle() {
                fun inner() { x = "deep"; }
                inner();
            }
            middle();
            return x;
        }
    }
    Keep(Cap5.run());
    """) == "deep");

Try("a local function captures a local of the function it is in", () => Captured("""
    @public def Cap6 {
        @public @static fun run(): String {
            fun middle(): String {
                let y = "middle";
                fun inner() { y = "inner"; }
                inner();
                return y;
            }
            return middle();
        }
    }
    Keep(Cap6.run());
    """) == "inner");

Try("a local function's own names aren't captured", () => Captured("""
    @public def Cap7 {
        @public @static fun run(): String {
            fun own(y: String): String { let z = y; return z; }
            return own("own");
        }
    }
    Keep(Cap7.run());
    """) == "own");

Try("a captured local is marked, and an uncaptured one isn't", () =>
{
    if (Run("""
        Keep(def Cap8 {
            @public @static fun run(): String {
                let x = "x";
                let y = "y";
                fun read(): String { return x; }
                return read();
            }
        } as DotNet_Class);
        """) is { } error)
        throw new Exception(error);

    var locals = ((Package.DotNet.Class)Kept()).Methods.First(method => method.Name == "run").Locals;
    return locals.First(local => local.Name == "x").IsCaptured && !locals.First(local => local.Name == "y").IsCaptured;
});

Try("a local function gets a document local's value from when it was built", () => Captured("""
    let doc9 = "built";
    @public def Cap9 {
        @public @static fun run(): String {
            fun read(): String { return doc9; }
            return read();
        }
    }
    doc9 = "later";
    Keep(Cap9.run());
    """) == "built");

Try("a method of a class in a body is still a method", () => Captured("""
    @public def Cap10 {
        @public @static fun run(): String {
            let made = def Inner10 { @public @static fun get(): String { return "method"; } } as DotNet_Class;
            return "ok";
        }
    }
    Keep(Cap10.run());
    """) == "ok");

string BuiltGet(string source)
{
    if (Run(source) is { } error)
        throw new Exception(error);

    var node = (Package.DotNet.Class)Kept();
    var type = runner.EvaluateExpression(new C.Get(node)).Unwrap()!;
    return (string)((Type)type).GetMethod("get")!.Invoke(null, [])!;
}

Try("a cast definition gets a captured variable's value from when it was built", () => BuiltGet("""
    @public def Cap14 {
        @public @static fun make(): DotNet_Class {
            let x = "before";
            fun read(): String { return x; }
            let made = def Made14 { @public @static fun get(): String { return x; } } as DotNet_Class;
            x = "after";
            return made;
        }
    }
    Keep(Cap14.make());
    """) == "before");

Try("a cast definition calls a local function bound to the run it was built in", () => BuiltGet("""
    @public def Cap15 {
        @public @static fun make(): DotNet_Class {
            let x = "before";
            fun read(): String { return x; }
            let made = def Made15 { @public @static fun get(): String { return read(); } } as DotNet_Class;
            x = "after";
            return made;
        }
    }
    Keep(Cap15.make());
    """) == "after");

Try("a local function is a delegate bound to the run of the code around it", () => Captured("""
    @public def Cap16 {
        @public @static fun run(): String {
            let x = "before";
            fun read(): String { return x; }
            let f = read;
            x = "after";
            return f();
        }
    }
    Keep(Cap16.run());
    """) == "after");

Try("a cast definition sees a variable by value and a local function sees it by reference", () =>
{
    if (Run("""
        @public def Cap17 {
            @public @static fun make(): DotNet_Class {
                let x = "before";
                fun read(): String { return x; }
                let made = def Made17 {
                    @public @static fun direct(): String { return x; }
                    @public @static fun through(): String { return read(); }
                } as DotNet_Class;
                x = "after";
                return made;
            }
        }
        Keep(Cap17.make());
        """) is { } error)
        throw new Exception(error);

    var type = (Type)runner.EvaluateExpression(new C.Get((Package.DotNet.Class)Kept())).Unwrap()!;
    return (string?)type.GetMethod("direct")!.Invoke(null, []) == "before"
        && (string?)type.GetMethod("through")!.Invoke(null, []) == "after";
});

Try("a local function captures a parameter by reference", () => Captured2("""
    @public def CapP1 {
        @public @static fun run(n: Int32): Int32 {
            fun add() { n = n + 1; }
            add();
            add();
            return n;
        }
    }
    Keep(CapP1.run(5));
    """) is 7);

Try("a local function sees a parameter assigned after it is defined", () => Captured2("""
    @public def CapP2 {
        @public @static fun run(s: String): String {
            fun read(): String { return s; }
            s = s + "!";
            return read();
        }
    }
    Keep(CapP2.run("a"));
    """) is "a!");

Try("a parameter given by name can be captured", () => Captured2("""
    @public def CapP3 {
        @public @static fun run(n: Int32): Int32 {
            fun twice(): Int32 { return n * 2; }
            return twice();
        }
    }
    Keep(CapP3.run(n: 21));
    """) is 42);

Try("a local function captures a parameter of the local function it is in", () => Captured2("""
    @public def CapP4 {
        @public @static fun run(): String {
            fun outer(text: String): String {
                fun inner() { text = text + "!"; }
                inner();
                return text;
            }
            return outer("hi");
        }
    }
    Keep(CapP4.run());
    """) is "hi!");

Try("a captured parameter is marked", () =>
{
    if (Run("""
        Keep(def CapP5 {
            @public @static fun run(a: String, b: String): String {
                fun read(): String { return a; }
                return read();
            }
        } as DotNet_Class);
        """) is { } error)
        throw new Exception(error);

    var parameters = ((Package.DotNet.Class)Kept()).Methods.First(method => method.Name == "run").Parameters;
    return parameters[0].IsCaptured && !parameters[1].IsCaptured;
});

object Captured2(string source)
{
    if (Run(source) is { } error)
        throw new Exception(error);

    return Kept();
}

Try("a string that looks like an operator or a keyword is a string", () =>
    ResetKept()
    && Run("""Keep("-"); Keep("/"); Keep("from"); Keep("as"); Keep("a" + "-" + "b");""") is null
    && Probe.Kept is ["-", "/", "from", "as", "a-b"]);

Try("a whole number is an Int32", () => Run("Keep(42);") is null && Kept() is 42);

Try("a number with a fraction is a Double", () => Run("Keep(1.5);") is null && Kept() is 1.5);

Try("operators follow their precedence", () => Run("Keep(1 + 2 * 3);") is null && Kept() is 7);

Try("subtraction and division", () => Run("Keep(10 - 4 / 2);") is null && Kept() is 8);

Try("comparisons give booleans", () =>
    Run("Keep(1 < 2); Keep(2 == 2); Keep(1 != 1);") is null && Probe.Kept is [true, true, false]);

Try("strings concatenate", () => Run("""Keep("a" + "b");""") is null && (string)Kept() == "ab");

Try("or", () => Run("Keep(false or true);") is null && Kept() is true);

Try("an operator is chosen by the types of its operands", () =>
    Run("""Keep(1.5 + 2.0); Keep("x" == "x");""") is null && Probe.Kept is [3.5, true]);

Try("an operator with no overload for its operands is an error", () =>
    Run("""Keep("a" - "b");""") is { } e && e.Contains("None of the") && e.Contains("overloads applies"));

Try("an operator nothing defines is an error", () =>
    Run("Keep(2 ** 3);") is { } e && e.Contains("Operator (**) is not defined"));

Try("operators and numbers in a method body", () =>
{
    if (Run("""
        @public def Arithmetic {
            @public @static fun area(width: Int32, height: Int32): Int32 { return width * height + 1; }
        }
        Keep(Arithmetic.area(3, 4));
        """) is { } error)
        throw new Exception(error);

    return Kept() is 13;
});

Try("a number in a body that is built is encoded into it", () =>
{
    if (Run("""
        @public def Numbers {
            @public @static fun make(): DotNet_Class {
                let n = 40;
                return def Built { @public @static fun get(): Int32 { return n + 2; } } as DotNet_Class;
            }
        }
        Keep(Numbers.make());
        """) is { } error)
        throw new Exception(error);

    var type = (Type)runner.EvaluateExpression(new C.Get((Package.DotNet.Class)Kept())).Unwrap()!;
    return type.GetMethod("get")!.Invoke(null, []) is 42;
});

// ---- Loops


object Looped(string source)
{
    if (Run(source) is { } error)
        throw new Exception(error);

    return Kept();
}

Try("a document loop runs while its condition holds", () => Looped("""
    let i1 = 0;
    let total1 = 0;
    while (i1 < 5) { total1 = total1 + i1; i1 = i1 + 1; }
    Keep(total1);
    """) is 10);

Try("a loop's else runs when its condition stops holding", () => Looped("""
    let i2 = 0;
    while (i2 < 2) { i2 = i2 + 1; } else { Keep("done"); }
    """) is "done");

Try("a break skips the else", () =>
    Run("""
        let i3 = 0;
        while (true) { i3 = i3 + 1; if (i3 == 3) { break; } } else { Keep("else"); }
        Keep(i3);
        """) is null && Probe.Kept is [3] && Kept() is 3);

Try("a continue starts the next iteration", () => Looped("""
    let i4 = 0;
    let counted4 = 0;
    while (i4 < 6) {
        i4 = i4 + 1;
        if (i4 == 2 or i4 == 4) { continue; }
        counted4 = counted4 + 1;
    }
    Keep(counted4);
    """) is 4);

Try("a break from a named loop leaves it", () => Looped("""
    let i5 = 0;
    let n5 = 0;
    while outer5 (i5 < 3) {
        i5 = i5 + 1;
        let j5 = 0;
        while (j5 < 3) {
            j5 = j5 + 1;
            n5 = n5 + 1;
            if (j5 == 2) { break from outer5; }
        }
    }
    Keep(n5);
    """) is 2);

Try("a continue from a named loop continues it", () => Looped("""
    let i6 = 0;
    let n6 = 0;
    while outer6 (i6 < 3) {
        i6 = i6 + 1;
        let j6 = 0;
        while (j6 < 3) {
            j6 = j6 + 1;
            if (j6 == 2) { continue from outer6; }
            n6 = n6 + 1;
        }
    }
    Keep(n6);
    """) is 3);

Try("a statement in a document loop is compiled once", () =>
    Looped("""
        let i7 = 0;
        while (i7 < 3) { let once7 = i7; i7 = i7 + 1; }
        Keep(i7);
        """) is 3
    && document.Document.Locals.Count(local => local.Name == "once7") == 1);

Try("a break from a loop that isn't there is an error", () =>
    Run("while (true) { break from nowhere8; }") is { } e && e.Contains("no loop named nowhere8"));

Try("a loop expression gives the value it is left with", () => Looped("""
    let i9 = 0;
    let found9 = while (i9 < 10) { i9 = i9 + 1; if (i9 * i9 == 16) { break i9; } } else 0;
    Keep(found9);
    """) is 4);

Try("a loop expression gives its else when its condition stops holding", () => Looped("""
    let found10 = while (false) { break 1; } else 7;
    Keep(found10);
    """) is 7);

Try("a loop expression needs an else", () =>
    Run("let v11 = while (true) { break 1; };") is { } e && e.Contains("needs an else"));

Try("leaving a loop expression needs a value", () =>
    Run("let v12 = while (true) { break; } else 1;") is { } e && e.Contains("needs a value"));

Try("a loop statement can't be left with a value", () =>
    Run("""@public def L13 { @public @static fun f() { while (true) { break 1; } } }""") is { } e && e.Contains("Only a loop that is an expression"));

Try("a loop in a method", () => Looped("""
    @public def L14 {
        @public @static fun sum(n: Int32): Int32 {
            let i = 0;
            let total = 0;
            while (i < n) { total = total + i; i = i + 1; }
            return total;
        }
    }
    Keep(L14.sum(5));
    """) is 10);

Try("a loop in a method has an else, and break and continue", () => Looped("""
    @public def L15 {
        @public @static fun count(): Int32 {
            let i = 0;
            let n = 0;
            while (i < 6) {
                i = i + 1;
                if (i == 2) { continue; }
                if (i == 5) { break; }
                n = n + 1;
            } else {
                n = 100;
            }
            return n;
        }
    }
    Keep(L15.count());
    """) is 3);

Try("a loop's else runs in a method when its condition stops holding", () => Looped("""
    @public def L16 {
        @public @static fun count(): Int32 {
            let n = 0;
            while (n < 2) { n = n + 1; } else { n = n + 10; }
            return n;
        }
    }
    Keep(L16.count());
    """) is 12);

Try("named loops in a method", () => Looped("""
    @public def L17 {
        @public @static fun count(): Int32 {
            let i = 0;
            let n = 0;
            while outer (i < 3) {
                i = i + 1;
                let j = 0;
                while (j < 3) {
                    j = j + 1;
                    if (j == 2) { continue from outer; }
                    if (i == 3) { break from outer; }
                    n = n + 1;
                }
            }
            return n;
        }
    }
    Keep(L17.count());
    """) is 2);

Try("a loop expression in a method", () => Looped("""
    @public def L18 {
        @public @static fun find(): Int32 {
            let i = 0;
            return while (i < 10) { i = i + 1; if (i * i == 16) { break i; } } else 0;
        }
    }
    Keep(L18.find());
    """) is 4);

Try("a loop in a body that is built", () =>
{
    if (Run("""
        @public def L19 {
            @public @static fun make(): DotNet_Class {
                return def Built19 {
                    @public @static fun get(): Int32 {
                        let i = 0;
                        while outer (true) { i = i + 1; if (i == 4) { break from outer; } }
                        return i;
                    }
                } as DotNet_Class;
            }
        }
        Keep(L19.make());
        """) is { } error)
        throw new Exception(error);

    var type = (Type)runner.EvaluateExpression(new C.Get((Package.DotNet.Class)Kept())).Unwrap()!;
    return type.GetMethod("get")!.Invoke(null, []) is 4;
});

Try("code from before a capture in a loop shares the captured variable", () => Looped("""
    @public def L20 {
        @public @static fun run(): Int32 {
            let n = 0;
            let i = 0;
            while (i < 3) {
                n = n + 1;
                fun add() { n = n + 10; }
                add();
                i = i + 1;
            }
            return n;
        }
    }
    Keep(L20.run());
    """) is 33);

// ---- Named arguments and default values

bool ResetKept() { Probe.Kept.Clear(); return true; }

Try("a CLR method takes named arguments in any order", () =>
    Run("""Keep(PairOf(second: "b", first: "a"));""") is null && (string)Kept() == "ab");

Try("a CLR default value fills a missing argument", () =>
    Run("""Keep(GreetWith("Ann"));""") is null && (string)Kept() == "Hello, Ann");

Try("a named argument replaces a CLR default value", () =>
    Run("""Keep(GreetWith("Ann", greeting: "Hi"));""") is null && (string)Kept() == "Hi, Ann");

Try("a name no parameter has is an error", () =>
    Run("""Keep(PairOf(first: "a", third: "c"));""") is { } e && e.Contains("can't be called with these arguments"));

Try("a named argument goes to its parameter, and positional ones fill the rest in order", () =>
    Run("""Keep(PairOf("a", first: "b"));""") is null && (string)Kept() == "ba");

Try("named and positional arguments can come in any order", () =>
    Run("""Keep(PairOf(first: "a", "b"));""") is null && (string)Kept() == "ab");

Try("an argument no parameter takes is an error", () =>
    Run("""Keep(PairOf("a", "b", "c"));""") is { } e && e.Contains("can't be called with these arguments"));

Try("overloads are chosen by the names of the arguments", () =>
    ResetKept() &&
    Run("""Keep(NamedBy(text: "x")); Keep(NamedBy(count: 1));""") is null && Probe.Kept is ["text", "count"]);

Try("a CLR method's parameter is given by position or by name, with its default value", () =>
    ResetKept() &&
    Run("""
        @public def Named1 {
            @public @static fun greet(name: String, greeting: String = "Hello"): String { return greeting + ", " + name; }
        }
        Keep(Named1.greet("Ann"));
        Keep(Named1.greet("Bo", greeting: "Hi"));
        """) is null && Probe.Kept is ["Hello, Ann", "Hi, Bo"]);

Try("a CLR constructor takes named arguments and default values", () =>
    ResetKept() &&
    Run("""
        @public def WithDefaultTag {
            @public let tag: String;
            @public @explicitThis new(this, tag: String = "default") { this.tag = tag; }
        }
        Keep(WithDefaultTag().tag);
        Keep(WithDefaultTag(tag: "given").tag);
        """) is null && Probe.Kept is ["default", "given"]);

Try("a CLR method's parameters can all be given by name, as C# lets any be", () =>
    Run("""
        @public def Named5 {
            @public @static fun join(a: String, b: String): String { return a + b; }
        }
        Keep(Named5.join(a: "A", b: "B"));
        """) is null && (string)Kept() == "AB");

Try("a parameter of the CLR's can't be named only, as each can be given by position", () =>
    Run("""@public def Named8 { @public @static fun join(a: String, {b: String}): String { return a + b; } }""") is { } e && e.Contains("is named only"));

Try("a CLR method's parameters can all be given by position", () =>
    Run("""Keep(Named5.join("A", "B"));""") is null && (string)Kept() == "AB");

Try("arguments to a CLR method can come in any order", () =>
    Run("""Keep(Named5.join(b: "B", "A"));""") is null && (string)Kept() == "AB");

Try("a parameter is named outside by its external name and inside by its alias", () =>
    Run("""
        @public def Named6 {
            @public @static fun label(value as v: String, text as t: String = "none"): String { return v + "/" + t; }
        }
        Keep(Named6.label("a", text: "b"));
        """) is null && (string)Kept() == "a/b");

Try("a named argument names the external name, not the alias", () =>
    Run("""Keep(Named6.label("a", t: "b"));""") is { } e && e.Contains("can't be called with these arguments"));

Try("parameters that take the rest of the arguments aren't supported yet", () =>
    Run("""@public def Named7 { @public @static fun all(...rest: String) { } }""") is { } e && e.Contains("take the rest"));

Try("a local function takes named arguments and default values", () =>
    Run("""
        @public def Named3 {
            @public @static fun run(): String {
                fun join(a: String, b: String = "!"): String { return a + b; }
                return join("x") + join(b: "?", "y");
            }
        }
        Keep(Named3.run());
        """) is null && (string)Kept() == "x!y?");

Try("a default value is code that runs at each call", () =>
{
    Probe.Counter = 0;
    Probe.Kept.Clear();

    return Run("""
        @public def Named4 {
            @public @static fun next(n: Int32 = NextNumber()): Int32 { return n; }
        }
        Keep(Named4.next());
        Keep(Named4.next());
        Keep(Named4.next(10));
        """) is null && Probe.Kept is [1, 2, 10];
});

// ---- Module members

Try("a module has functions, which call each other by name", () =>
    Run("""
        module M101 {
            fun twice(n: Int32): Int32 { return n * 2; }
            fun four(): Int32 { return twice(2); }
        }
        Keep(M101.four());
        """) is null && Kept() is 4);

Try("a module's let is a variable of it, with its initial value", () =>
    ResetKept()
    && Run("""
        module M102 {
            @public let greeting: String = "hi";
            fun get(): String { return greeting; }
            fun set(value: String) { greeting = value; }
        }
        Keep(M102.get());
        M102.set("bye");
        Keep(M102.get());
        Keep(M102.greeting);
        """) is null
    && Probe.Kept is ["hi", "bye", "bye"]);

Try("a module's let can have a type the module imports", () =>
    Run("""
        module M103 {
            import { String , public } from "core:runtime";

            @public let name: String = "Z#";

            fun main(): String { return "Hello, " + name; }
        }
        Keep(M103.main());
        """) is null && (string)Kept() == "Hello, Z#");

Try("a class in a module calls the module's functions and reads its lets", () =>
    Run("""
        module M105 {
            @public let suffix: String = "!";
            fun shout(text: String): String { return text + suffix; }

            @public def Speaker105 {
                @public @static fun speak(): String { return shout("hey"); }
            }
        }
        Keep(M105.Speaker105.speak());
        """) is null && (string)Kept() == "hey!");

Try("a module's functions and its classes use each other", () =>
    Run("""
        module M106 {
            fun greet(name: String): String { return "hello " + name; }

            @public def Greeter106 {
                @public let name: String;
                @public @explicitThis new(this, name: String) { this.name = name; }
                @public @explicitThis fun greeting(this): String { return greet(this.name); }
            }

            fun make(): String { return Greeter106("you").greeting(); }
        }
        Keep(M106.make());
        """) is null && (string)Kept() == "hello you");

Try("a function builds a module that imports, has a let and a function, and the module runs", () =>
{
    if (Run("""
        @public def Factory104 {
            @public @static fun make(): DotNet_Module {
                return module {
                    import { String , public } from "core:runtime";

                    let name: String = "Z#";

                    fun main() { Keep("Hello, " + name); }
                } as DotNet_Module;
            }
        }
        Keep(Factory104.make());
        """) is { } error)
        throw new Exception(error);

    var module = (Package.DotNet.Module)Kept();
    var main = new C.Call { Callee = new C.NamedMember { Target = new C.Get(module), Name = "main" } };

    if (runner.EvaluateExpression(main).When(out _).Error(out var failure))
        throw new Exception(failure.ToString());

    return (string)Kept() == "Hello, Z#";
});

// ---- ct and statement decorators

Try("ct evaluates an expression given as HIR", () =>
    Core.ZSharp.ModuleScope.CT(new C.StringLiteral("given")) is "given");

Try("load gives a definition's runtime object", () =>
    Run("""
        let made107 = def LoadMe107 {} as DotNet_Class;
        Keep(load(made107));
        """) is null && Kept() is Type { Name: "LoadMe107" });

Try("a decorator on a statement gets the statement as HIR, and the statement doesn't run", () =>
{
    Probe.Seen.Clear();

    return ResetKept()
        && Run("""@seen Keep("not run");""") is null
        && Probe.Kept is []
        && Probe.Seen is [C.Call];
});

Try("@ct runs a statement as the code around it runs", () =>
    Run("""@ct Keep("ran");""") is null && (string)Kept() == "ran");

Try("@ct in a body runs as the body is built, and leaves nothing in it", () =>
    ResetKept()
    && Run("""
        @public def CtBody108 {
            @public @static fun f(): String {
                @ct Keep("while building");
                return "called";
            }
        }
        """) is null
    && Probe.Kept is ["while building"]
    && ResetKept()
    && Run("Keep(CtBody108.f());") is null
    && Probe.Kept is ["called"]);

Try("a decorator in a body places the code it gives", () =>
    ResetKept()
    && Run("""
        @public def Twice109 {
            @public @static fun f() { @twice Keep("again"); }
        }
        Twice109.f();
        """) is null
    && Probe.Kept is ["again", "again"]);

Try("@ct on a definition in a module loads it as the module is built, and doesn't register it", () =>
{
    if (Run("""
        module M112 {
            @ct @public def Loaded112 {}
            @public def Registered112 {}
        }
        """) is { } error)
        throw new Exception(error);

    var module = (Package.DotNet.Module)((C.Reference)((C.Get)document.Scope.Get("M112")!).Instance).Value!;
    return module.Definitions.Select(definition => ((Package.DotNet.Class)definition).Name).SequenceEqual(["Registered112"])
        && Probe.LastType("Loaded112") is not null;
});

Try("@ct on a definition in a document loads it", () =>
    Run("@ct @public def Loaded113 {}") is null && Probe.LastType("Loaded113") is not null);

Try("@ct on a definition in a body loads it as the body is built", () =>
    Run("""
        @public def Holder114 {
            @public @static fun f() { @ct def Loaded114 {}; }
        }
        """) is null && Probe.LastType("Loaded114") is not null);

// ---- Parameter decorators

Try("a decorator makes a parameter injected, so a call leaves it out", () =>
    Run("""
        @public def Inject110 {
            @public @static fun greet(name: String, @inject greeting: String): String { return greeting + ", " + name; }
        }
        Keep(Inject110.greet("Ann"));
        """) is null && (string)Kept() == "injected, Ann");

Try("an injected parameter can't be given by a call", () =>
    Run("""Keep(Inject110.greet("Ann", "hello"));""") is { } e && e.Contains("can't be called with these arguments"));

Try("a parameter decorator must give a parameter", () =>
    Run("""@public def Inject111 { @public @static fun f(@mark x: String) { } }""") is not null);

// ---- CLR generics

Try("a generic CLR type given its arguments is a type", () =>
    Run("""
        import { List } from "core:runtime";
        let xs116 = List[Int32]();
        xs116.Add(1);
        xs116.Add(2);
        Keep(xs116.Count);
        """) is null && Kept() is 2);

Try("a generic CLR type with two arguments", () =>
    Run("""
        import { Dictionary } from "core:runtime";
        let d117 = Dictionary[String, Int32]();
        d117.Add("a", 1);
        Keep(d117.Count);
        """) is null && Kept() is 1);

Try("an instantiated generic type in a type position", () =>
    Run("""
        import { List , public } from "core:runtime";
        @public def Generic118 {
            @public let items: List[String];
            @public @static fun count(xs: List[String]): Int32 { return xs.Count; }
        }
        let names118 = List[String]();
        names118.Add("a");
        Keep(Generic118.count(names118));
        """) is null && Kept() is 1);

Try("a generic CLR method given its arguments", () =>
    Run("""Keep(Same[String]("same"));""") is null && (string)Kept() == "same");

Try("the arities of a generic family are its overloads", () =>
    Run("""
        import { Func } from "core:runtime";
        Keep(Func[Int32, String]);
        """) is null && Kept() is Type { } func && func == typeof(Func<int, string>));

Try("a generic type given the wrong number of arguments is an error", () =>
    Run("""
        import { List } from "core:runtime";
        let wrong119 = List[Int32, Int32]();
        """) is { } e && e.Contains("type arguments"));

Try("a generic instance is described as its definition and arguments", () =>
    il.Load(typeof(List<int>)) is Package.DotNet.GenericInstance { Definition: Package.DotNet.Class { Name: "System.Collections.Generic.List`1" }, Arguments: [Package.DotNet.Struct { Name: "System.Int32" }] });

// ---- Interfaces and virtual dispatch

Try("a class implements an interface defined in Z#", () =>
    Run("""
        def IGreeter121 of DotNet_Interface { fun greet(): String; }
        @public def English121 : IGreeter121 { @public fun greet(): String { return "hello"; } }
        let greeter121: IGreeter121 = English121();
        Keep(greeter121.greet());
        """) is null && (string)Kept() == "hello");

Try("a member's accessibility is the CLR's, as its decorator says", () =>
{
    if (Run("""
        @public def Open316 { @public let shown: Int32; @private let hidden: Int32; @internal fun inside() { } @protected fun derived() { } }
        Keep(Open316);
        """) is not null || Kept() is not Type type)
        return false;

    var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static;
    return type.GetField("shown", flags)!.IsPublic
        && type.GetField("hidden", flags)!.IsPrivate
        && type.GetMethod("inside", flags)!.IsAssembly
        && type.GetMethod("derived", flags)!.IsFamily;
});

Try("a class and its members are private unless they say otherwise, and an interface and its members public", () =>
{
    var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static;

    if (Run("def Closed318 { let hidden: Int32; fun method() { } } Keep(Closed318);") is not null || Kept() is not Type closed)
        return false;

    if (Run("def IOpen318 of DotNet_Interface { fun shown(): Int32; } Keep(IOpen318);") is not null || Kept() is not Type open)
        return false;

    return closed.IsNotPublic
        && closed.GetField("hidden", flags)!.IsPrivate
        && closed.GetMethod("method", flags)!.IsPrivate
        && open.IsPublic
        && open.GetMethod("shown", flags)!.IsPublic;
});

Try("a class nested in a class is a CLR nested type, a member of its class, private unless it says otherwise", () =>
{
    if (Run("""
        @public def Outer319 { @public def Shown319 { @public @explicitThis new(this) { } } def Hidden319 { } }
        Keep(Outer319.Shown319());
        Keep(Outer319);
        """) is not null || Probe.Kept is not [var shown, Type outer])
        return false;

    var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
    return shown.GetType() is { IsNestedPublic: true } inner && inner.DeclaringType == outer
        && outer.GetNestedType("Hidden319", flags) is { IsNestedPrivate: true };
});

Try("a method that implements an interface's but isn't public implements it explicitly", () =>
    Run("""
        def IGreeter317 of DotNet_Interface { fun greet(): String; }
        @public def Quiet317 : IGreeter317 { @private fun greet(): String { return "hi"; } }
        let greeter317: IGreeter317 = Quiet317();
        Keep(greeter317.greet());
        """) is null && (string)Kept() == "hi");

Try("a virtual method is overridden", () =>
    Run("""
        @public def Animal120 { @public @virtual fun sound(): String { return "..."; } }
        @public def Dog120 : Animal120 { @public @override fun sound(): String { return "woof"; } }
        let animal120: Animal120 = Dog120();
        Keep(animal120.sound());
        """) is null && (string)Kept() == "woof");

Try("an abstract method is implemented by a derived class", () =>
    Run("""
        @public def Shape122 { @public @abstract fun area(): Int32; }
        @public def Square122 : Shape122 { @public @override fun area(): Int32 { return 4; } }
        let shape122: Shape122 = Square122();
        Keep(shape122.area());
        """) is null && Kept() is 4);

Try("an interface extends another", () =>
    Run("""
        def INamed123 of DotNet_Interface { fun name(): String; }
        def IPerson123 of DotNet_Interface : INamed123 { fun age(): Int32; }
        @public def Person123 : IPerson123 {
            @public fun name(): String { return "Ann"; }
            @public fun age(): Int32 { return 30; }
        }
        let named123: INamed123 = Person123();
        Keep(named123.name());
        """) is null && (string)Kept() == "Ann");

Try("a class has a base class and interfaces", () =>
    Run("""
        def IShout125 of DotNet_Interface { fun shout(): String; }
        @public def Base125 { @public fun quiet(): String { return "shh"; } }
        @public def Both125 : Base125, IShout125 { @public fun shout(): String { return "HEY"; } }
        let both125 = Both125();
        let shouter125: IShout125 = both125;
        Keep(both125.quiet() + shouter125.shout());
        """) is null && (string)Kept() == "shhHEY");

Try("a CLR interface is implemented", () =>
    Run("""
        import { IDisposable , public } from "core:runtime";
        @public def Closer126 : IDisposable { @public fun Dispose() { Keep("disposed"); } }
        let closer126: IDisposable = Closer126();
        closer126.Dispose();
        """) is null && (string)Kept() == "disposed");

Try("a class can have only one base class", () =>
    Run("""
        @public def One127 {}
        @public def Two127 {}
        @public def Three127 : One127, Two127 {}
        """) is { } e && e.Contains("one base class"));

Try("an interface is described as an interface", () =>
{
    if (Run("""Keep(def IShape128 of DotNet_Interface { fun area(): Int32; } as DotNet_Interface);""") is { } error)
        throw new Exception(error);

    return Kept() is Package.DotNet.Interface { Name: "IShape128", Methods: [{ Name: "area" }] };
});

// ---- Methods as values

Try("a method is a value of its own delegate type", () =>
    Run("""
        let upper131 = Upper;
        Keep(upper131("x"));
        """) is null && (string)Kept() == "X");

Try("a method is a delegate of a type its signature fits", () =>
    Run("""
        import { Func } from "core:runtime";
        let upper132: Func[String, Object] = Upper;
        Keep(upper132("y"));
        """) is null && (string)Kept() == "Y");

Try("a method is passed where a delegate is expected", () =>
    Run("""Keep(ApplyTo(Upper, "z"));""") is null && (string)Kept() == "Z");

Try("an overload is chosen by the delegate type it is cast to", () =>
    Run("""
        import { Func } from "core:runtime";
        let which133 = Which as Func[Boolean, String];
        Keep(which133(true));
        """) is null && (string)Kept() == "bool");

Try("an overload is chosen by the delegate type it goes to", () =>
    Run("""
        import { Func } from "core:runtime";
        let which134: Func[String, String] = Which;
        Keep(which134("s"));
        """) is null && (string)Kept() == "string");

Try("an instance method is bound to its object", () =>
    Run("""
        import { List } from "core:runtime";
        let xs135 = List[Int32]();
        let add135 = xs135.Add;
        add135(1);
        add135(2);
        Keep(xs135.Count);
        """) is null && Kept() is 2);

Try("a Z# method is a value", () =>
    Run("""
        @public def Twice136 { @public @static fun twice(text: String): String { return text + text; } }
        let twice136 = Twice136.twice;
        Keep(twice136("a"));
        """) is null && (string)Kept() == "aa");

Try("a virtual method as a value calls what the object overrides", () =>
    Run("""
        @public def Animal137 { @public @virtual fun sound(): String { return "..."; } }
        @public def Dog137 : Animal137 { @public @override fun sound(): String { return "woof"; } }
        let animal137: Animal137 = Dog137();
        let sound137 = animal137.sound;
        Keep(sound137());
        """) is null && (string)Kept() == "woof");

Try("a method that doesn't fit a delegate type is an error", () =>
    Run("""
        import { Func } from "core:runtime";
        let wrong138: Func[Int32, String] = Upper;
        """) is not null);

// ---- Generic classes

Try("a generic class is defined and instantiated", () =>
    ResetKept()
    && Run("""
        @public def Box139[T] {
            @public let value: T;
            @public @explicitThis new(this, value: T) { this.value = value; }
            @public @explicitThis fun get(this): T { return this.value; }
        }
        Keep(Box139[Int32](5).get());
        Keep(Box139[String]("x").value);
        """) is null
    && Probe.Kept is [5, "x"]);

Try("a generic class has two type parameters", () =>
    Run("""
        @public def Pair140[A, B] {
            @public let first: A;
            @public let second: B;
            @public @explicitThis new(this, first: A, second: B) { this.first = first; this.second = second; }
        }
        let pair140 = Pair140[String, Int32]("one", 1);
        Keep(pair140.second);
        """) is null && Kept() is 1);

Try("a generic class is a CLR generic type definition", () =>
    Run("@public def Generic141[T] { @public let item: T; }") is null
    && Probe.LastType("Generic141") is { IsGenericTypeDefinition: true } type
    && type.GetField("item")!.FieldType.IsGenericParameter);

Try("a generic method is defined and given its arguments", () =>
    ResetKept()
    && Run("""
        @public def Ids143 { @public @static fun id[T](value: T): T { return value; } }
        Keep(Ids143.id[Int32](7));
        Keep(Ids143.id[String]("seven"));
        """) is null
    && Probe.Kept is [7, "seven"]);

Try("a generic class has a generic method", () =>
    Run("""
        @public def Holder144[T] {
            @public let held: T;
            @public @explicitThis new(this, held: T) { this.held = held; }
            @public @explicitThis fun with[U](this, other: U): U { return other; }
        }
        Keep(Holder144[Int32](1).with[String]("other"));
        """) is null && (string)Kept() == "other");

Try("a generic method's type parameter is a CLR method type parameter", () =>
    Run("@public def Ids145 { @public @static fun id[T](value: T): T { return value; } }") is null
    && Probe.LastType("Ids145") is { } type
    && type.GetMethod("id") is { IsGenericMethodDefinition: true } method
    && method.ReturnType.IsGenericMethodParameter);

Try("a type parameter is only known inside its class", () =>
    Run("""
        @public def Inside142[T] { @public let item: T; }
        let outside142: T = 1;
        """) is not null);

// ---- CLR members

Try("an enum's member is its value", () =>
    Run("Keep(DayOfWeek.Monday);") is null && Kept() is DayOfWeek.Monday);

Try("a constant is its value", () =>
    Run("Keep(Int32.MaxValue);") is null && Kept() is int.MaxValue);

Try("a nested type is a member of its type", () =>
    Run("Keep(Environment.SpecialFolder.Desktop);") is null && Kept() is Environment.SpecialFolder.Desktop);

Try("a static field and a static property are set", () =>
{
    Probe.Shared = "";

    return Run("""Probe.Shared = "set"; Probe.SharedProperty = "too"; Keep(Probe.Shared + Probe.SharedProperty);""") is null
        && (string)Kept() == "settoo";
});

// ---- CLR indexers

Try("an indexer is read", () =>
    Run("""
        import { List } from "core:runtime";
        let xs148 = List[Int32]();
        xs148.Add(4);
        Keep(xs148[0]);
        """) is null && Kept() is 4);

Try("an indexer is assigned", () =>
    Run("""
        import { List } from "core:runtime";
        let xs149 = List[Int32]();
        xs149.Add(4);
        xs149[0] = 5;
        Keep(xs149[0]);
        """) is null && Kept() is 5);

Try("a string's characters are its indexer", () =>
    Run("""Keep("abc"[1]);""") is null && Kept() is 'b');

Try("a dictionary is indexed by its keys", () =>
    Run("""
        import { Dictionary } from "core:runtime";
        let d150 = Dictionary[String, Int32]();
        d150["one"] = 1;
        Keep(d150["one"]);
        """) is null && Kept() is 1);

// ---- CLR operators

Try("an operator a CLR type defines", () =>
    Run("Keep((TimeSpan.FromSeconds(1.0) + TimeSpan.FromSeconds(2.0)).TotalSeconds);") is null && Kept() is 3.0);

Try("a CLR type's equality operator", () =>
    Run("Keep(TimeSpan.FromSeconds(1.0) == TimeSpan.FromSeconds(1.0));") is null && Kept() is true);

Try("strings compare with the operator String defines", () =>
    ResetKept() && Run("""Keep("a" == "a"); Keep("a" != "b");""") is null && Probe.Kept is [true, true]);

Try("greater than, at most and at least", () =>
    ResetKept() && Run("Keep(2 > 1); Keep(1 <= 1); Keep(1.5 >= 2.0);") is null && Probe.Kept is [true, true, false]);

Try("a CLR comparison operator", () =>
    Run("Keep(TimeSpan.FromSeconds(1.0) < TimeSpan.FromSeconds(2.0));") is null && Kept() is true);

// ---- CLR conversions

Try("an Int32 widens to the Double a method takes", () =>
    Run("Keep(Math.Sqrt(16));") is null && Kept() is 4.0);

Try("widening keeps the most specific overload", () =>
    Run("Keep(Math.Abs(0 - 3));") is null && Kept() is 3);

Try("an Int32 cast to Int64", () =>
    Run("Keep(1 as Int64);") is null && Kept() is 1L);

Try("a Double cast to Int32 loses its fraction", () =>
    Run("Keep(2.7 as Int32);") is null && Kept() is 2);

Try("a Double does not narrow implicitly", () =>
    Run("""Keep("abc".Substring(1.5));""") is { } e && e.Contains("overloads applies"));

Try("a conversion a type defines implicitly", () =>
    Run("Keep(Decimal.Add(1, 2));") is null && Kept() is 3m);

Try("a conversion a type defines explicitly, in a cast", () =>
    Run("Keep(1.5 as Decimal);") is null && Kept() is 1.5m);

Try("a conversion from a type that defines it explicitly", () =>
    Run("""Keep(Decimal.Parse("7") as Int32);""") is null && Kept() is 7);

// ---- null, downcasts and type tests

Try("null goes to an object", () =>
    Run("Keep(null);") is null && Kept() is null);

Try("null in a local of a reference type", () =>
    Run("let text150: String = null; Keep(text150 == null);") is null && Kept() is true);

Try("null cast to a reference type", () =>
    Run("Keep(null as String);") is null && Kept() is null);

Try("null does not go to a value type", () =>
    Run("let number151: Int32 = null;") is { } e && e.Contains("doesn't match the type"));

Try("an object cast down to a string", () =>
    Run("""let o152: Object = "text"; Keep((o152 as String).Length);""") is null && Kept() is 4);

Try("an object cast to a value type is unboxed", () =>
    Run("let o153: Object = 5; Keep((o153 as Int32) + 1);") is null && Kept() is 6);

Try("a cast down to a type the value is not of fails as it runs", () =>
{
    try { Run("""let o154: Object = "text"; Keep(o154 as Int32);"""); return false; }
    catch (Exception e) { return e is InvalidCastException || e.InnerException is InvalidCastException; }
});

Try("a sealed type is not cast to an interface it does not implement", () =>
    Run("""Keep("text" as IDisposable154);""") is { } e && e.Contains("conversion"));

Try("a type test", () =>
    ResetKept() && Run("""let o155: Object = "x"; Keep(o155 is String); Keep(o155 is Int32);""") is null
        && Probe.Kept is [true, false]);

Try("a name pattern needs a type for now, until a local can take the type of what first sets it", () =>
    Run("""let o314: Object = 5; if (o314 is let any314) Keep(any314);""") is { } error && error.Contains("needs a type for now"));

Try("a var name pattern binds a value that can be set again", () =>
    Run("""let o315: Object = 41; if (o315 is var number315: Int32) { number315 = number315 + 1; Keep(number315); }""") is null && Kept() is 42);

Try("null is of no type", () =>
    Run("let o156: Object = null; Keep(o156 is Object);") is null && Kept() is false);

Try("a value is of a type it goes to", () =>
    ResetKept() && Run("Keep(5 is Object); Keep(5 is String);") is null && Probe.Kept is [true, false]);

Try("a type test and a cast in a method", () =>
    Run("""
        @public def Lengths157 {
            @public @static fun of(value: Object): Int32 {
                if (value is String) return (value as String).Length;
                return 0;
            }
        }
        Keep(Lengths157.of("four") + Lengths157.of(4));
        """) is null && Kept() is 4);

Try("a name pattern is assigned what matched", () =>
    Run("""let o158: Object = "four"; if (o158 is let text158: String) Keep(text158.Length);""") is null && Kept() is 4);

Try("a name pattern is not assigned when the value does not match", () =>
    ResetKept() && Run("""let o159: Object = 4; Keep(o159 is let text159: String); Keep(text159 == null);""") is null
        && Probe.Kept is [false, true]);

Try("a name pattern can be assigned", () =>
    Run("""let o160: Object = "x"; o160 is let text160: String; text160 = "changed"; Keep(text160);""") is null
        && (string)Kept() == "changed");

Try("a name pattern of a value type unboxes", () =>
    Run("let o161: Object = 41; if (o161 is let number161: Int32) Keep(number161 + 1);") is null && Kept() is 42);

Try("the value a name pattern matches is evaluated once", () =>
    ResetKept() && Run("Keep(NextNumber() is let number162: Int32); Keep(number162 + 1 == NextNumber());") is null
        && Probe.Kept is [true, true]);

Try("_ names nothing", () =>
    Run("""let o163: Object = "x"; Keep(o163 is String);""") is null && Kept() is true);

Try("a name pattern in a method", () =>
    Run("""
        @public def Lengths164 {
            @public @static fun of(value: Object): Int32 {
                if (value is let text: String) return text.Length;
                return 0;
            }
        }
        Keep(Lengths164.of("four") + Lengths164.of(4));
        """) is null && Kept() is 4);

Try("a name pattern in a module function", () =>
    Run("""
        module M165 {
            fun of(value: Object): Int32 {
                if (value is let text: String) return text.Length;
                return 0;
            }
        }
        Keep(M165.of("three"));
        """) is null && Kept() is 5);

// ---- params arrays, Ref and Out

Try("params takes each argument as an element", () =>
    Run("Keep(Probe.CountOf(1, 2, 3));") is null && Kept() is 3);

Try("params takes no arguments as an empty array", () =>
    Run("Keep(Probe.CountOf());") is null && Kept() is 0);

Try("params takes an array as the array", () =>
    Run("Keep(Probe.CountOf(Probe.Numbers()));") is null && Kept() is 3);

Try("params after other parameters, converting each element", () =>
    Run("""Keep(Probe.Labels("n:", 1, "two", 3.5));""") is null && (string)Kept() == "n:1,two,3.5");

Try("String.Join with params", () =>
    Run("""Keep(String.Join(",", "a", "b"));""") is null && (string)Kept() == "a,b");

Try("an out argument is assigned by the call", () =>
    ResetKept() && Run("""let parsed166: Int32 = 0; Keep(Int32.TryParse("42", parsed166)); Keep(parsed166);""") is null
        && Probe.Kept is [true, 42]);

Try("ref arguments are read and assigned", () =>
    Run("""
        @public def Swaps167 {
            @public @static fun swapped(): Int32 {
                let left: Int32 = 1;
                let right: Int32 = 2;
                Probe.Swap(left, right);
                return left * 10 + right;
            }
        }
        Keep(Swaps167.swapped());
        """) is null && Kept() is 21);

Try("a parameter passed by reference", () =>
    Run("""
        @public def Halves168 {
            @public @static fun halved(value: Int32, half: Int32): Int32 {
                Probe.Halve(value, half);
                return half;
            }
        }
        Keep(Halves168.halved(10, 0));
        """) is null && Kept() is 5);

Try("a field is passed by its own address", () =>
{
    Probe.Tally = 5;
    return Run("Keep(Interlocked.Increment(Probe.Tally));") is null && Kept() is 6 && Probe.Tally == 6;
});

Try("a property and a document local are passed through temporaries and assigned back", () =>
    Run("let other170: Int32 = 2; Probe.Level = 1; Probe.Swap(Probe.Level, other170); Keep(Probe.Level * 10 + other170);") is null
        && Kept() is 21 && Probe.Level == 2);

Try("a value that is not a variable is not passed by reference", () =>
    Run("""Keep(Int32.TryParse("1", 5));""") is { } e && e.Contains("overloads applies"));

Try("a variable of another type is not passed by reference", () =>
    Run("""let text169: String = ""; Keep(Int32.TryParse("1", text169));""") is { } e && e.Contains("overloads applies"));

Try("Ref and Out are types of references", () =>
    ResetKept() && Run("""import { Ref, Out } from "core:runtime"; Keep(Ref[Int32]); Keep(Out[Int32]);""") is null
        && Probe.Kept is [Type a, Type b] && a == typeof(int).MakeByRefType() && b == a);

// ---- CLR events

Try("a handler added to a static event runs when it is raised, until it is removed", () =>
    ResetKept() && Run("Probe.Ticked.add(Probe.Record); Probe.Tick(7); Probe.Ticked.remove(Probe.Record); Probe.Tick(8);") is null
        && Probe.Kept is [7]);

Try("a local function handles an instance event and captures a local", () =>
    Run("""
        @public def Rings171 {
            @public @static fun count(): Int32 {
                let bell = Bell();
                let rings = 0;
                fun rang(sender: Object, args: EventArgs) { rings = rings + 1; }
                bell.Rung.add(rang);
                bell.Ring();
                bell.Ring();
                return rings;
            }
        }
        Keep(Rings171.count());
        """) is null && Kept() is 2);

Try("an event has only add and remove", () =>
    Run("Probe.Ticked.raise(1);") is { } e && e.Contains("only add and remove"));

// ---- inferring type arguments

Try("a generic method infers its type argument from an argument", () =>
    ResetKept() && Run("""Keep(Probe.Identity(5)); Keep(Probe.Identity("ab").Length);""") is null && Probe.Kept is [5, 2]);

Try("a method that isn't generic is more specific than a generic one that takes the same", () =>
    ResetKept() && Run("""Keep(Probe.Kind(1)); Keep(Probe.Kind("x"));""") is null && Probe.Kept is ["Int32", "generic"]);

Try("an inferred generic method is compared as it would be called", () =>
    Run("Keep(Array.IndexOf(Probe.Numbers(), 2));") is null && Kept() is 1);

Try("a type argument is inferred through an interface an argument implements", () =>
    ResetKept() && Run("Keep(Enumerable.Count(Probe.Numbers())); Keep(Enumerable.Max(Probe.Numbers()));") is null
        && Probe.Kept is [3, 3]);

Try("a type argument is inferred through a reference", () =>
    ResetKept() && Run("""let text172: String = "x"; Keep(Interlocked.Exchange(text172, "y")); Keep(text172);""") is null
        && Probe.Kept is ["x", "y"]);

Try("type arguments are inferred from a local function", () =>
    Run("""
        @public def Twice173 {
            @public @static fun total(): Int32 {
                fun twice(value: Int32): Int32 { return value * 2; }
                return Enumerable.Sum(Enumerable.Select(Probe.Numbers(), twice));
            }
        }
        Keep(Twice173.total());
        """) is null && Kept() is 12);

Try("type arguments that can't be inferred are an error", () =>
    Run("Keep(Enumerable.Empty());") is { } e && e.Contains("can't be inferred"));

// ---- nullable value types

Try("a nullable value has a value", () =>
    ResetKept() && Run("let n174: Nullable[Int32] = 5; Keep(n174.HasValue); Keep(n174.Value);") is null && Probe.Kept is [true, 5]);

Try("a nullable value can be null", () =>
    ResetKept() && Run("let n175: Nullable[Int32] = null; Keep(n175.HasValue); Keep(n175.GetValueOrDefault(7));") is null
        && Probe.Kept is [false, 7]);

Try("a nullable value is cast to its value", () =>
    Run("let n176: Nullable[Int32] = 3; Keep((n176 as Int32) + 1);") is null && Kept() is 4);

Try("a value and null go to a nullable parameter", () =>
    ResetKept() && Run("Keep(Probe.OrZero(5)); Keep(Probe.OrZero(null));") is null && Probe.Kept is [5, 0]);

Try("a nullable value boxes to its value or to null", () =>
    ResetKept() && Run("let n177: Nullable[Int32] = 9; let m177: Nullable[Int32] = null; Keep(n177); Keep(m177);") is null
        && Probe.Kept is [9, null]);

Try("Nullable comes from core:runtime", () =>
    Run("""
        module M178 {
            import { Nullable, Int32 } from "core:runtime";
            fun one(): Int32 { let n: Nullable[Int32] = 1; return n.Value; }
        }
        Keep(M178.one());
        """) is null && Kept() is 1);

// ---- tasks

Try("a task's result", () =>
    Run("Keep(Task.FromResult(5).Result);") is null && Kept() is 5);

Try("a task waited on", () =>
    Run("Task.Delay(1).Wait(); Keep(Task.CompletedTask.IsCompleted);") is null && Kept() is true);

Try("a local function runs as a task", () =>
    Run("""
        @public def Work179 {
            @public @static fun run(): Int32 {
                let base = 40;
                fun work(): Int32 { return base + 2; }
                return Task.Run(work).Result;
            }
        }
        Keep(Work179.run());
        """) is null && Kept() is 42);

// ---- extension methods

Try("an extension method is no member until its class is added", () =>
    Run("""Keep("hi".Shout());""") is { } e && e.Contains("no member named Shout"));

Try("an extension method is called on a value", () =>
{
    il.Extensions.Add(typeof(ProbeExtensions));
    return Run("""Keep("hi".Shout());""") is null && (string)Kept() == "HI!";
});

Try("a type's own method comes before an extension method", () =>
    Run("""Keep("a".ToUpper());""") is null && (string)Kept() == "A");

Try("an extension method of a value type", () =>
    Run("let n180: Int32 = 4; Keep(n180.Twice());") is null && Kept() is 8);

Try("an extension method takes a value of a type that goes to it", () =>
    Run("""Keep("x".Describe());""") is null && (string)Kept() == "object x");

Try("extension methods of several classes are overloads", () =>
{
    il.Extensions.Add(typeof(MoreProbeExtensions));
    return ResetKept() && Run("""let n181: Int32 = 1; Keep(n181.Label()); Keep("s".Label());""") is null
        && Probe.Kept is ["Int32", "String"];
});

Try("importing a class brings its extension methods", () =>
    ResetKept() && Run("""
        module M182 {
            import { Enumerable } from "core:runtime";
        }
        Keep(Probe.Numbers().Count());
        Keep(Probe.Numbers().Sum());
        """) is null && Probe.Kept is [3, 6]);

Try("an import of a name the module has no member for fails at the import", () =>
    Run("""module M184 { import { Strin } from "core:runtime"; }""") is { } e && e.Contains("import of Strin failed"));

Try("generic extension methods infer from the value and a local function", () =>
    Run("""
        @public def Linq183 {
            @public @static fun run(): Int32 {
                fun odd(value: Int32): Boolean { return value == 1 or value == 3; }
                fun twice(value: Int32): Int32 { return value * 2; }
                return Probe.Numbers().Where(odd).Select(twice).Sum();
            }
        }
        Keep(Linq183.run());
        """) is null && Kept() is 8);

// ---- predefined operators

Try("Int64 arithmetic stays Int64", () =>
    ResetKept() && Run("Keep(Int64.MaxValue - 1); Keep(Int64.MaxValue - 1 > 0);") is null
        && Probe.Kept is [long.MaxValue - 1, true]);

Try("an Int32 and an Int64 add as Int64, as C# promotes them", () =>
    Run("let small185: Int32 = 2; Keep(Int64.MaxValue - small185);") is null && Kept() is long.MaxValue - 2);

Try("a string concatenates with any value", () =>
    ResetKept() && Run("""Keep("n=" + 5); Keep(1.5 + "!"); Keep("a" + "b");""") is null && Probe.Kept is ["n=5", "1.5!", "ab"]);

Try("enums compare by their values", () =>
    ResetKept() && Run("Keep(DayOfWeek.Monday == DayOfWeek.Monday); Keep(DayOfWeek.Monday != DayOfWeek.Friday); Keep(DayOfWeek.Monday < DayOfWeek.Friday); Keep(DayOfWeek.Sunday >= DayOfWeek.Monday);") is null
        && Probe.Kept is [true, true, true, false]);

Try("references are equal if they are one object", () =>
    ResetKept() && Run("let a186: Object = Bell(); let b186: Object = a186; Keep(a186 == b186); Keep(a186 == Bell()); Keep(a186 != Bell());") is null
        && Probe.Kept is [true, false, true]);

Try("an object compares with null", () =>
    ResetKept() && Run("let o187: Object = null; Keep(o187 == null); Keep(o187 != null); Keep(Bell() == null);") is null
        && Probe.Kept is [true, false, false]);

Try("a type's own equality is more specific than reference equality", () =>
    Run("""let a188: String = "ab"; Keep(a188 == "a" + "b");""") is null && Kept() is true);

Try("a value type is not compared as a reference", () =>
    Run("Keep(Bell() == 1);") is { } e && e.Contains("overloads applies"));

// ---- arrays

Try("values in brackets become an array where one is expected", () =>
    ResetKept() && Run("let a189: Int32[] = [1, 2, 3]; Keep(a189.Length); Keep(a189[1]);") is null && Probe.Kept is [3, 2]);

Try("an element of an array is assigned", () =>
    Run("let a190: Int32[] = [1, 2]; a190[0] = 5; Keep(a190[0] + a190[1]);") is null && Kept() is 7);

Try("a type given a length is an array of that length", () =>
    ResetKept() && Run("let a191: String[] = String[3]; a191[2] = \"x\"; Keep(a191.Length); Keep(a191[0] == null); Keep(a191[2]);") is null
        && Probe.Kept is [3, true, "x"]);

Try("a length can be any Int32", () =>
    Run("let n207: Int32 = 2; let a207: Int32[] = Int32[n207 + 1]; Keep(a207.Length);") is null && Kept() is 3);

Try("a type given several lengths is an array of as many dimensions", () =>
    Run("Keep(Int32[2, 3]);") is null && Kept() is int[,] grid && grid.GetLength(0) == 2 && grid.GetLength(1) == 3);

Try("a generic type isn't given a length", () =>
    Run("""module M208 { import { List , public } from "core:runtime"; @public let lists: Int32 = List[3].Length; }""") is { } e && e.Contains("type arguments, not lengths"));

Try("an array type is not called", () =>
    Run("Keep(Int32[](3));") is { } e && e.Contains("Int32[4]"));

Try("values in brackets convert to the element type", () =>
    Run("let a192: Double[] = [1, 2.5]; Keep(a192[0] + a192[1]);") is null && Kept() is 3.5);

Try("empty brackets are an empty array", () =>
    Run("let a193: String[] = []; Keep(a193.Length);") is null && Kept() is 0);

Try("values in brackets go to a params array and to an interface arrays implement", () =>
    ResetKept() && Run("""Keep(Probe.CountOf([4, 5])); Keep(String.Join(",", ["a", "b"]));""") is null
        && Probe.Kept is [2, "a,b"]);

Try("values in brackets have no type of their own", () =>
    Run("let a194 = [1, 2];") is { } e && e.Contains("inferred"));

Try("an array type is a type of arrays", () =>
    Run("Keep(Int32[]);") is null && Kept() is Type type && type == typeof(int[]));

// ---- let as an out argument

Try("a let in a document is an out argument", () =>
    ResetKept() && Run("""Keep(Int32.TryParse("12", let parsed195: Int32)); Keep(parsed195 + 1);""") is null && Probe.Kept is [true, 13]);

Try("a let in a method is an out argument", () =>
    Run("""
        @public def Parse196 {
            @public @static fun number(text: String): Int32 {
                if (Int32.TryParse(text, let parsed: Int32)) return parsed;
                return 0 - 1;
            }
        }
        Keep(Parse196.number("40") + Parse196.number("x"));
        """) is null && Kept() is 39);

// ---- Ref and Out parameters of CLR methods written in Z#

Try("an Out parameter is assigned through", () =>
    ResetKept() && Run("""
        @public def Halves197 {
            @public @static fun halve(value: Int32, half: Out[Int32]): Boolean {
                half = value / 2;
                return value == half * 2;
            }
        }
        Keep(Halves197.halve(9, let half197: Int32));
        Keep(half197);
        """) is null && Probe.Kept is [false, 4]);

Try("a Ref parameter is read and assigned through", () =>
    Run("""
        @public def Bumps198 {
            @public @static fun bump(counter: Ref[Int32]) { counter = counter + 1; }
        }
        let counter198: Int32 = 1;
        Bumps198.bump(counter198);
        Bumps198.bump(counter198);
        Keep(counter198);
        """) is null && Kept() is 3);

Try("a Ref parameter passed on passes the reference it holds", () =>
    ResetKept() && Run("""
        @public def Increments199 {
            @public @static fun increment(counter: Ref[Int32]): Int32 { return Interlocked.Increment(counter); }
        }
        let counter199: Int32 = 5;
        Keep(Increments199.increment(counter199));
        Keep(counter199);
        """) is null && Probe.Kept is [6, 6]);

Try("a Ref parameter is a reference on the CLR", () =>
    Run("""
        @public def Signature200 {
            @public @static fun take(value: Ref[Int32]) { }
        }
        Keep(Signature200);
        """) is null && Kept() is Type type
        && type.GetMethod("take")!.GetParameters()[0].ParameterType == typeof(int).MakeByRefType());

Try("a call with an identifier among its arguments, in a condition in a class", () =>
    Run("""
        @public def Parsing201 {
            @public @static fun same(word: String): Int32 {
                let other = "a";
                if (String.Equals(word, other)) return 1;
                return 0;
            }
        }
        Keep(Parsing201.same("a"));
        """) is null && Kept() is 1);

// ---- lifted operators

Try("nullable values add as their values", () =>
    ResetKept() && Run("let a202: Nullable[Int32] = 2; let b202: Nullable[Int32] = 3; Keep(a202 + b202); Keep((a202 + 1).Value);") is null
        && Probe.Kept is [5, 3]);

Try("a null operand gives null", () =>
    ResetKept() && Run("let a203: Nullable[Int32] = 2; let n203: Nullable[Int32] = null; Keep(a203 + n203); Keep((n203 * 2).HasValue);") is null
        && Probe.Kept is [null, false]);

Try("nullable values are equal if both are null", () =>
    ResetKept() && Run("let a204: Nullable[Int32] = 2; let n204: Nullable[Int32] = null; Keep(n204 == null); Keep(a204 == null); Keep(a204 != n204); Keep(a204 == 2);") is null
        && Probe.Kept is [true, false, true, true]);

Try("a comparison with null is false", () =>
    ResetKept() && Run("let a205: Nullable[Int32] = 2; let n205: Nullable[Int32] = null; Keep(a205 < n205); Keep(a205 >= n205); Keep(a205 < 3);") is null
        && Probe.Kept is [false, false, true]);

Try("an operator a type defines is lifted too", () =>
    Run("let a206: Nullable[TimeSpan] = TimeSpan.FromSeconds(1.0); Keep((a206 + TimeSpan.FromSeconds(2.0)).Value.TotalSeconds);") is null
        && Kept() is 3.0);

// ---- index arguments are matched as call arguments are

Try("a type argument given by its name", () =>
    ResetKept() && Run("Keep(Nullable[T: Int32]); Keep(Probe.Identity[T: String](\"x\"));") is null
        && Probe.Kept is [Type type, "x"] && type == typeof(int?));

Try("type arguments given by name in any order", () =>
    Run("""
        import { Dictionary } from "core:runtime";
        let d209 = Dictionary[TValue: Int32, TKey: String]();
        d209["a"] = 1;
        Keep(d209["a"]);
        """) is null && Kept() is 1);

Try("a type argument with a name the type has no parameter for is an error", () =>
    Run("Keep(Nullable[U: Int32]);") is { } e && e.Length > 0);

Try("an indexer's argument given by its name", () =>
    Run("""
        import { List } from "core:runtime";
        let xs210 = List[Int32]();
        xs210.Add(6);
        Keep(xs210[index: 0]);
        """) is null && Kept() is 6);

Try("an array of several dimensions is indexed by an index for each", () =>
    Run("let grid211 = Int32[2, 3]; grid211[1, 2] = 7; Keep(grid211[1, 2] + grid211[0, 0]);") is null && Kept() is 7);

// ---- the toolchain from Z#

Try("the toolchain's types are imported from its modules", () =>
    ResetKept() && Run("""
        module M212 {
            import { Instance } from "std:zsharp/hir";
            import { Function } from "std:zsharp/lang/ast";
            import { CompilerObject } from "std:zsharp/mir";
            import { Result } from "std:zsharp/common";
            import { String } from "core:runtime";
        }
        """) is null);

Try("a generic type of a module is found by its name, every arity of it", () =>
    Run("""
        module M213 {
            import { Result, Error } from "std:zsharp/common";
            import { String, Type , public } from "core:runtime";
            @public let kind: Type = Result[String, Error];
        }
        Keep(M213.kind);
        """) is null && Kept() is Type type && type == typeof(CommonZ.Result<string, CommonZ.Error>));

Try("Z# reaches the meta-types and the mapped loaders", () =>
    ResetKept() && Run("""
        module M214 {
            import { metaTypes } from "core:zsharp";
            import { mappedLoader } from "core:compiler";
            import { Object , public } from "core:runtime";
            @public let types: Object = metaTypes;
            @public let loader: Object = mappedLoader;
        }
        Keep(M214.types);
        Keep(M214.loader);
        """) is null && Probe.Kept is [ZSharp.Lang.MetaTypes, ZSharp.Compiler.MappedLoader]);

// ---- finding a meta-compiler is an operation Z# can replace

Try("Z# replaces how a meta-compiler is found, asking what it replaced", () =>
    ResetKept() && Run("""
        module M225 {
            import { MetaCompilerOf, MetaCompiler } from "std:zsharp/lang";
            import { Instance } from "std:zsharp/hir";
            import { IResult, Error } from "std:zsharp/common";
            import { Int32 , public } from "core:runtime";

            @public let previous: MetaCompilerOf;
            @public let asked: Int32 = 0;

            fun find(metaType: Instance): IResult[MetaCompiler, Error] {
                asked = asked + 1;
                return previous(metaType);
            }
        }
        import { metaTypes } from "core:zsharp";
        M225.previous = metaTypes.Find;
        metaTypes.Find = M225.find;
        @public def Asked225 {}
        metaTypes.Find = M225.previous;
        Keep(M225.asked);
        """) is null && Kept() is int asked && asked > 0);

// ---- precedence

Try("equality binds looser than a cast and a comparison", () =>
    ResetKept() && Run("""
        let text231: Object = "x";
        Keep(text231 == text231 as Object);
        Keep(1 < 2 == true);
        Keep(2 + 3 == 5);
        """) is null && Probe.Kept is [true, true, true]);

// ---- CLR generics over classes still being loaded

Try("a generic CLR type is given a class loaded with the code that uses it", () =>
    ResetKept() && Run("""
        module M232 {
            import { List , public } from "core:runtime";
            @public def Item232 { @public let value: Int32; @public @explicitThis new(this, value: Int32) { this.value = value; } }
            @public def Items232 {
                @public @static fun total(): Int32 {
                    let items = List[Item232]();
                    items.Add(Item232(40));
                    items.Add(Item232(2));
                    items[1] = Item232(items[1].value);
                    return items[0].value + items[1].value + items.Count - 2;
                }
            }
        }
        Keep(M232.Items232.total());
        """) is null && Probe.Kept is [42]);

Try("a module's code names the module itself", () =>
    Run("""
        module M233 {
            fun half(value: Int32): Int32 { return value / 2; }
            fun quarter(value: Int32): Int32 { return M233.half(M233.half(value)); }
        }
        Keep(M233.quarter(168));
        """) is null && Kept() is 42);

Try("a class's own method is a delegate in its own code", () =>
    Run("""
        import { Func , public } from "core:runtime";
        @public def Holder234 {
            @public @explicitThis fun give(this): Int32 { return 42; }
            @public @explicitThis fun run(this): Int32 {
                let given: Func[Int32] = this.give;
                return given();
            }
        }
        Keep(Holder234().run());
        """) is null && Kept() is 42);

Try("a generic CLR type given a class still being built is the description of its instance", () =>
    Run("""
        module M248 {
            import { Dictionary , public } from "core:runtime";
            @public def Item248 { @public @explicitThis new(this) { } }
            @public def Bag248 {
                @public let items: Dictionary[String, Item248];
                @public @explicitThis new(this) { this.items = Dictionary[String, Item248](); }
                @public @explicitThis fun size(this): Int32 { this.items["x"] = Item248(); return this.items.Count; }
            }
        }
        Keep(M248.Bag248().size());
        """) is null && Kept() is 1);

Try("a CLR class has properties, read and written through their accessors", () =>
    ResetKept() && Run("""
        @public def Box256 {
            @public let value: Int32;
            @public @explicitThis new(this) { this.value = 1; }
            @public @explicitThis get Doubled(this): Int32 { return this.value * 2; }
            @public @explicitThis set Doubled(this, value: Int32) { this.value = value / 2; }
            @public @static get Kind(): String { return "boxes"; }
            @public @explicitThis fun read(this): Int32 { this.Doubled = 40; return this.Doubled; }
        }

        let box256 = Box256();
        Keep(box256.Doubled);
        box256.Doubled = 10;
        Keep(box256.value);
        Keep(box256.read());
        Keep(Box256.Kind);
        """) is null && Probe.Kept is [2, 5, 40, "boxes"]
    && Probe.LastType("Box256").GetProperty("Doubled") is { CanRead: true, CanWrite: true });

Try("a CLR class implements an interface's property with an accessor", () =>
    Run("""
        @public def Labeled257 : IHasLabel {
            @public @explicitThis new(this) { }
            @public @explicitThis get Label(this): String { return "labeled"; }
        }
        Keep(Labeled257());
        """) is null && Kept() is IHasLabel { Label: "labeled" });

Try("a property's setter takes one value and returns nothing", () =>
    Run("""
        @public def Bad258 {
            @public @explicitThis new(this) { }
            @public @explicitThis set Value(this, a: Int32, b: Int32): Int32 { return a; }
        }
        """) is { } e && e.Contains("takes one value"));

Try("a generic CLR method is given a class loaded with the code that uses it", () =>
    Run("""
        module M259 {
            @public def Item259 {
                @public let value: Int32;
                @public @explicitThis new(this, value: Int32) { this.value = value; }
                @public @static fun total(): Int32 {
                    let same = GenericProbe.Same[Item259](Item259(40));
                    let copy: Item259 = null;
                    if (GenericProbe.Copy[Item259](Item259(2), copy)) return same.value + copy.value;
                    return 0;
                }
            }
        }
        Keep(M259.Item259.total());
        """) is null && Kept() is 42);

Try("generic overloads given type arguments are chosen between by the call", () =>
    ResetKept() && Run("""
        Keep(GenericProbe.Pick[Int32](1));
        Keep(GenericProbe.Pick[Int32](1, 2));
        """) is null && Probe.Kept is ["one", "two"]);

// ---- CLR value types

Try("a struct's property is read from a struct a method returns", () =>
    Run("Keep(TimeSpan.FromSeconds(90.0).TotalMinutes);") is null && Kept() is 1.5);

Try("a struct's method is called on a struct", () =>
    Run("Keep(TimeSpan.FromSeconds(1.0).Add(TimeSpan.FromSeconds(2.0)).TotalSeconds);") is null && Kept() is 3.0);

Try("a struct's static property and then its instance property", () =>
    Run("Keep(DateTime.Now.Year);") is null && Kept() is int year && year > 2000);

Try("a struct with no arguments is its default value", () =>
    Run("Keep(Guid().ToString());") is null && (string)Kept() == "00000000-0000-0000-0000-000000000000");

Try("a struct calls a method it inherits from a class", () =>
    Run("Keep(TimeSpan.FromSeconds(1.0).GetType());") is null && Kept() is Type type && type == typeof(TimeSpan));

Try("a struct in a document local", () =>
    Run("let span146 = TimeSpan.FromSeconds(5.0); Keep(span146.TotalSeconds);") is null && Kept() is 5.0);

Try("a struct in a local of a method", () =>
    Run("""
        @public def Spans147 {
            @public @static fun minutes(seconds: Double): Double {
                let span = TimeSpan.FromSeconds(seconds);
                return span.TotalMinutes;
            }
        }
        Keep(Spans147.minutes(120.0));
        """) is null && Kept() is 2.0);

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
        @public def Over1 {
            @public @static fun pick(x: Object): String { return "object"; }
            @public @static fun pick(x: String): String { return "string"; }
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
        @public def Over2 {
            @public @static fun pick(x: String): String { return "string"; }
            @public @static fun pick(x: Boolean): String { return "bool"; }
            @public @static fun get(): String { return Over2.pick(true); }
        }
        Keep(Over2.get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "bool";
});

Try("Z# methods that are as specific as each other are ambiguous", () =>
    Run("""
        @public def Over3 {
            @public @static fun two(a: Object, b: String): String { return "first"; }
            @public @static fun two(a: String, b: Object): String { return "second"; }
        }
        Keep(Over3.two("x", "y"));
        """) is { } e && e.Contains("none of them is more specific"));

Try("a Z# constructor is chosen by its parameter types", () =>
{
    if (Run("""
        @public def Over4 {
            @public let tag: String;
            @public @explicitThis new(this, x: Object) { this.tag = "object"; }
            @public @explicitThis new(this, x: String) { this.tag = "string"; }
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
    && Scope.HIRReifier.EncodeValue(new List<int>()).Unwrap() is C.Get { Instance: ZSharp.Importer.ILLoader.ExposedObject });

Try("a class used as a value is its type object", () =>
    Run("@public def ClassValue {} Keep(ClassValue);") is null && Kept() is Type { Name: "ClassValue" });

Try("a class still loading used as a value is its type object", () =>
{
    if (Run("""
        @public def SelfValue {
            @public fun get(): Object { return SelfValue; }
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
            @public def Bar60 {
                @public @static fun name(): String { return "bar"; }
            }

            @public def Maker60 {
                @public @static fun make(): DotNet_Class {
                    return def Made60 {
                        @public fun get(): String { return Bar60.name(); }
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
        @public def Maker61 {
            @public @static fun make(text: String): DotNet_Class {
                return def Made61 {
                    @public fun get(): String { return text; }
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
            @public def Bar70 {
                @public @static fun name(): String { return "bar"; }
            }

            @public def Maker70 {
                @public @static fun make(outer: String): DotNet_Class {
                    return def Middle70 {
                        @public fun make(inner: String): DotNet_Class {
                            return def Leaf70 {
                                @public fun getBar(): String { return Bar70.name(); }
                                @public fun getOuter(): String { return outer; }
                                @public fun getInner(): String { return inner; }
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
        @public def LocalsOf {
            @public fun get(): String {
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
        @public def Ordered2 {
            @public fun run() {
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
        @public def Nested3 {
            @public fun pick(yes: Boolean): String {
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
        @public def Listed {
            @public fun get(): String {
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
        @public def Maker72 {
            @public @static fun make(text: String): DotNet_Class {
                return def Made72 {
                    @public fun get(): String {
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
    import { String, static, public } from "core:runtime";
    import { export } from "core:zsharp";
    """;

Try("a document exports what is marked", () =>
    Run("@export @public def Exported1 {}") is null && document.Document.Exports.ContainsKey("Exported1"));

Try("a document doesn't export what isn't marked", () =>
    Run("@public def NotExported1 {}") is null && !document.Document.Exports.ContainsKey("NotExported1"));

// A module doesn't know Export, so registering what @export returns is a type error:
// the module's Add doesn't take it.
Try("only a definition in a document can be exported", () =>
    Run("module M80 { @export @public def Inner80 {} }") is { } e && e.Contains("None of the 4 overloads applies"));

Try("an export with an alias exports under the alias", () =>
    Run("""@export("Aliased81") @public def Original81 {}""") is null
    && document.Document.Exports.ContainsKey("Aliased81")
    && !document.Document.Exports.ContainsKey("Original81"));

Try("an exported definition keeps its own name in the document", () =>
    Run("""@export("Aliased82") @public def Original82 { @public @static fun name(): String { return "own"; } } Keep(Original82.name());""") is null
    && (string)Kept() == "own");

Try("an export comes after the decorators nearer the definition", () =>
{
    Probe.Marked.Clear();

    return Run("""@export @mark @public def Marked83 {}""") is null
        && Probe.Marked is ["Marked83"]
        && document.Document.Exports.ContainsKey("Marked83");
});

Try("an export goes to no decorator but the document", () =>
    Run("""@mark @export @public def Marked84 {}""") is { } e && e.Contains("can't be called with these arguments"));

Try("a CLR delegate can be called", () =>
    Run("""Keep(export("Called85")(DotNet_Class as Object));""") is not { } failure
        ? Kept() is ZSharp.Lang.Export { Alias: "Called85" }
        : throw new Exception(failure));

Try("importing a document gives what it exports", () =>
{
    var library = WriteDocument("library1.zs", Header + """
        @export
        def Library1 {
            @public @static fun hello(): String { return "imported"; }
        }
        """);

    return Run($$"""import { Library1 } from "{{library}}"; Keep(Library1.hello());""") is null
        && (string)Kept() == "imported";
});

Try("importing a document gives what it exports under an alias", () =>
{
    var library = WriteDocument("library86.zs", Header + """
        @export("Public86")
        def Private86 {
            @public @static fun hello(): String { return "aliased"; }
        }
        """);

    return Run($$"""import { Public86 } from "{{library}}"; Keep(Public86.hello());""") is null
        && (string)Kept() == "aliased";
});

Try("a name a document doesn't export can't be imported", () =>
{
    var library = WriteDocument("library2.zs", Header + "@public def Hidden2 {}");

    return Run($$"""import { Hidden2 } from "{{library}}"; Keep(Hidden2);""") is { } e && e.Contains("exports nothing named Hidden2");
});

Try("a document runs once however often it is imported", () =>
{
    var library = WriteDocument("library3.zs", Header + "@export @public def Once3 {}");

    return Run($$"""import { Once3 } from "{{library}}";""") is null
        && Run($$"""import { Once3 as Again3 } from "{{library}}"; Keep(Once3 as DotNet_Class); Keep(Again3 as DotNet_Class);""") is null
        && Probe.Kept is [var first, var second] && ReferenceEquals(first, second);
});

Try("a document imports another relative to itself", () =>
{
    WriteDocument("inner4.zs", Header + """
        @export
        def Inner4 {
            @public @static fun name(): String { return "relative"; }
        }
        """);
    var outer = WriteDocument("outer4.zs", Header + """
        import { Inner4 } from "inner4.zs";

        @export
        def Outer4 {
            @public @static fun name(): String { return Inner4.name(); }
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
    @public def {{name}} {
        @public @static fun hello(): String { return "{{text}}"; }
    }
    """);

Try("an import outside any meta-compiler that compiles it is an error", () =>
    compiler.CompileStatement(ParseOne("""import { a } from "x";""")) is { IsError: true } failure
    && failure.UnwrapError().ToString()!.Contains("Nothing here compiles the import"));

Try("an import passes its arguments to the import function after the source", () =>
{
    var library = Library("Library96", "with arguments");
    language.Add("import", il.Load(typeof(Probe).GetMethod(nameof(Probe.ImportWith))!));
    Probe.ImportedWith.Clear();

    return Run($$"""import("noted") { Library96 } from "{{library}}"; Keep(Library96.hello());""") is null
        && (string)Kept() == "with arguments"
        && Probe.ImportedWith is ["noted"];
});

Try("an import passes keyword arguments by name", () =>
{
    var library = Library("Library97", "named");
    Probe.ImportedWith.Clear();

    return Run($$"""import(note: "by name") { Library97 } from "{{library}}"; Keep(Library97.hello());""") is null
        && (string)Kept() == "named"
        && Probe.ImportedWith is ["by name"];
});

Try("an import in a body passes its arguments too", () =>
{
    var library = Library("Library94b", "body");
    Probe.ImportedWith.Clear();

    if (Run($$"""
        @public def Uses94 {
            @public @static fun get(): String {
                import("in a body") { Library94b } from "{{library}}";
                return Library94b.hello();
            }
        }
        Keep(Uses94.get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "body" && Probe.ImportedWith is ["in a body"];
});

Try("an import with arguments no overload takes is an error", () =>
    Run($$"""import(true, false) { Library95 } from "{{Library("Library95", "x")}}";""") is { } e && e.Contains("None of the 3 overloads applies"));

Try("an import in a body runs as the body is built", () =>
{
    var library = Library("Library90", "built");

    if (Run("""
        @public def Maker90 {
            @public @static fun make(src: String): DotNet_Class {
                return def Made90 {
                    @public fun get(): String {
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
        @export @public def Shared91 { @public @static fun hello(): String { return "one"; } }
        """);
    var second = WriteDocument("Again91b.zs", Header + """
        @export @public def Shared91 { @public @static fun hello(): String { return "two"; } }
        """);

    if (Run("""
        @public def Maker91 {
            @public @static fun make(src: String): DotNet_Class {
                return def Made91 {
                    @public fun get(): String {
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
        @public def Uses92 {
            @public @static fun use(src: String): String {
                import { Library92 } from src;
                return Library92.hello();
            }
        }
        """) is { } e && (e.Contains("Import") || e.Contains("no member named Library92"));
});

Try("an import in a module runs as the module is constructed", () =>
{
    var library = Library("Library93", "from a module");

    if (Run($$"""
        module M93 {
            import { Library93 } from "{{library}}";

            @public def C93 {
                @public @static fun get(): String { return Library93.hello(); }
            }
        }
        Keep(M93.C93.get());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "from a module";
});

Try("a module can use what it imports from a compile-time source while it is compiled", () =>
{
    var library = WriteDocument("Library94.zs", """
        import { String, static, DotNet_Class , public } from "core:runtime";
        import { export } from "core:zsharp";

        @export
        def Library94 {
            @public @static fun mark(definition: DotNet_Class): DotNet_Class { return definition; }
        }
        """);

    return Run($$"""
        module M94 {
            import { Library94 } from "{{library}}";

            @Library94.mark
            def C94 {}
        }
        Keep(M94.C94());
        """) is null && Kept().GetType().Name == "C94";
});

Try("a module's definitions can use what it imports as types", () =>
{
    var library = Library("Library98", "typed");

    if (Run($$"""
        module M98 {
            import { Library98 } from "{{library}}";

            @public def Holder98 {
                @public let held: Library98;
                @public @static fun take(value: Library98): Library98 { return value; }
                @public @static fun hello(): String { return Library98.hello(); }
            }
        }
        Keep(M98.Holder98.hello());
        """) is { } error)
        throw new Exception(error);

    return (string)Kept() == "typed";
});

Try("an import with a compile-time source runs once, while compiling", () =>
{
    var library = Library("Library99", "once");
    Probe.ImportedWith.Clear();

    if (Run($$"""
        @public def Uses99 {
            @public @static fun get(): String {
                import("compiled") { Library99 } from "{{library}}";
                return Library99.hello();
            }
        }
        """) is { } error)
        throw new Exception(error);

    var whileCompiling = Probe.ImportedWith is ["compiled"];

    return whileCompiling
        && Run("Keep(Uses99.get()); Keep(Uses99.get());") is null
        && Probe.ImportedWith is ["compiled"]
        && Probe.Kept is ["once", "once"];
});

Try("an import from a document local is a compile-time import", () =>
{
    var library = Library("Library100", "from a local");
    Probe.ImportedWith.Clear();

    if (Run($$"""
        let source100 = "{{library}}";
        @public def Uses100 {
            @public @static fun get(): String {
                import("from a local") { Library100 } from source100;
                return Library100.hello();
            }
        }
        """) is { } error)
        throw new Exception(error);

    return Probe.ImportedWith is ["from a local"]
        && Run("Keep(Uses100.get());") is null
        && (string)Kept() == "from a local";
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
        @public def Reads7 { @public @static fun get(): String { return d7; } }
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

// ---- std:functions: Z#'s own functions, defined in Z#. Installing it makes every fun
// that nothing else compiles a Z# function, so these come last.

Try("std:functions installs what compiles its functions, by its name", () =>
    Run("""import { Function } from "std:functions";""") is null);

Try("a fun in a document is a Z# function", () =>
    Run("""
        fun twice215(value: Int32): Int32 { return value * 2; }
        Keep(twice215(21));
        """) is null && Kept() is 42);

Try("a Z# function has named parameters with default values", () =>
    ResetKept() && Run("""
        fun greet218(name: String, {punctuation: String = "!"}): String { return "Hi " + name + punctuation; }
        Keep(greet218("Z"));
        Keep(greet218("Z", punctuation: "?"));
        """) is null && Probe.Kept is ["Hi Z!", "Hi Z?"]);

Try("a Z# function calls itself", () =>
    Run("""
        fun factorial219(n: Int32): Int32 {
            if (n < 2) return 1;
            return n * factorial219(n - 1);
        }
        Keep(factorial219(5));
        """) is null && Kept() is 120);

Try("a Z# function has locals and loops", () =>
    Run("""
        fun total220(n: Int32): Int32 {
            let total = 0;
            let index = 0;
            while (index < n) {
                total = total + index;
                index = index + 1;
            }
            return total;
        }
        Keep(total220(5));
        """) is null && Kept() is 10);

Try("a Z# function that returns nothing", () =>
    Run("""
        fun hello221() { Keep("hello221"); }
        hello221();
        """) is null && (string)Kept() == "hello221");

Try("a Z# function calls another", () =>
    Run("""
        fun inner222(value: Int32): Int32 { return value + 1; }
        fun outer222(value: Int32): Int32 { return inner222(value) * 10; }
        Keep(outer222(4));
        """) is null && Kept() is 50);

Try("a CLR method calls a Z# function", () =>
    Run("""
        @public def Uses223 { @public @static fun run(): Int32 { return twice215(5); } }
        Keep(Uses223.run());
        """) is null && Kept() is 10);

Try("a fun in a CLR module is still a function of the module", () =>
    Run("""
        module M224 { fun one(): Int32 { return twice215(3) + 1; } }
        Keep(M224.one());
        """) is null && Kept() is 7);

// ---- std:traits: traits, defined in Z#

Try("std:traits installs, with both flavors of dispatchers", () =>
    Run("""
        import { Implementation } from "std:traits";
        import "std:traits/static";
        import "std:traits/dynamic";
        """) is null);

Try("a trait is implemented for a CLR type, and a value of it goes where the trait is expected", () =>
    Run("""
        def Greeting226 of DotNet_Interface { fun greet(): String; }

        def StringGreeting226 of Implementation[Greeting226, String] {
            @public let value: String;
            @public @explicitThis new(this, value: String) { this.value = value; }
            @public @explicitThis fun greet(this): String { return "Hello, " + this.value; }
        }

        @public def Uses226 { @public @static fun hello(greeting: Greeting226): String { return greeting.greet(); } }
        Keep(Uses226.hello("Z#"));
        """) is null && (string)Kept() == "Hello, Z#");

Try("a value whose static type says nothing matches a trait its type implements", () =>
    ResetKept() && Run("""
        let o227: Object = "dynamic";
        Keep(o227 is Greeting226);
        if (o227 is let greeting227: Greeting226) Keep(greeting227.greet());
        let n227: Object = 5;
        Keep(n227 is Greeting226);
        """) is null && Probe.Kept is [true, "Hello, dynamic", false]);

Try("the runtime has the capability of polymorphism, given by a trait", () =>
    ResetKept() && Run("""
        import { runtime } from "core:runtime";
        import { Polymorphism } from "std:zsharp/capabilities";
        import { Structure } from "std:zsharp/lir";
        import "std:zsharp/clr";

        let target228: Object = runtime;
        Keep(target228 is Polymorphism);
        if (target228 is let capability228: Polymorphism) {
            let structure228 = Structure();
            capability228.objects(structure228);
            Keep(structure228.Metadata.Count);
        }
        """) is null && Probe.Kept is [true, 1]);

Try("a Z# function given too many arguments is an error", () =>
    Run("Keep(twice215(1, 2));") is { } e && e.Contains("too many arguments"));

Try("a Z# function given too few arguments is an error", () =>
    Run("Keep(twice215());") is { } e && e.Contains("No argument is given for parameter value"));

Try("a positional parameter isn't given by name", () =>
    Run("Keep(twice215(value: 1));") is { } e && e.Length > 0);

// ---- std:oop: Z#'s own classes, defined in Z#. Installing it, which installs
// std:oop/compiler, makes def a Z# class, so these come after everything else.

Try("std:oop installs what compiles its classes", () =>
    Run("""import { Class } from "std:oop";""") is null);

Try("a Z# class has fields, a constructor and methods", () =>
    ResetKept() && Run("""
        def Point229 {
            let x: Int32;
            let y: Int32;

            new(this, x: Int32, y: Int32) { this.x = x; this.y = y; }

            fun sum(): Int32 { return x + y; }
            fun twice(this): Int32 { return this.sum() * 2; }
            fun move(by: Int32) { x = x + by; }
        }

        let point229 = Point229(1, 2);
        Keep(point229.sum());
        Keep(point229.twice());
        point229.move(10);
        Keep(point229.x);
        """) is null && Probe.Kept is [3, 6, 11]);

Try("a Z# class has named constructors, and fields with default values", () =>
    Run("""
        def Counter230 {
            let count: Int32 = 5;
            new() { }
            new from(this, start: Int32) { this.count = start; }
            fun next(this): Int32 { this.count = this.count + 1; return this.count; }
        }

        Keep(Counter230().next() + Counter230.from(100).next());
        """) is null && Kept() is 107);

Try("a call through a Z# interface runs what the object's class has", () =>
    ResetKept() && Run("""
        import { Interface } from "std:oop";

        def Shape235 of Interface {
            fun area(): Int32;
            fun named(): String;
        }

        def Square235 : Shape235 {
            let side: Int32;
            new(this, side: Int32) { this.side = side; }
            fun area(): Int32 { return side * side; }
            fun named(): String { return "square"; }
        }

        def Rectangle235 : Shape235 {
            let width: Int32;
            let height: Int32;
            new(this, width: Int32, height: Int32) { this.width = width; this.height = height; }
            fun area(): Int32 { return width * height; }
            fun named(): String { return "rectangle"; }
        }

        fun describe235(shape: Shape235): String { return shape.named() + " " + shape.area(); }

        let shape235: Shape235 = Square235(3);
        Keep(describe235(shape235));
        Keep(describe235(Rectangle235(2, 5)));
        """) is null && Probe.Kept is ["square 9", "rectangle 10"]);

Try("a value of a Z# interface is cast down to, and tested for, a class", () =>
    ResetKept() && Run("""
        import { Interface } from "std:oop";

        def Named236 of Interface { fun name(): String; }

        def Cat236 : Named236 {
            let lives: Int32 = 9;
            new() { }
            fun name(): String { return "cat"; }
        }

        def Dog236 : Named236 {
            new() { }
            fun name(): String { return "dog"; }
        }

        fun lives236(named: Named236): Int32 {
            if (named is let cat: Cat236) return cat.lives;
            return 0;
        }

        let named236: Named236 = Cat236();
        Keep(lives236(named236));
        Keep(lives236(Dog236()));
        Keep((named236 as Cat236).lives);
        Keep(named236 is Dog236);
        """) is null && Probe.Kept is [9, 0, 9, false]);

Try("a Z# class must have each method of the interfaces it implements", () =>
    Run("""
        import { Interface } from "std:oop";
        def Sized237 of Interface { fun size(): Int32; }
        def Box237 : Sized237 { new() { } fun size(): String { return "big"; } }
        let sized237: Sized237 = Box237();
        """) is { } e && e.Contains("doesn't have every method"));

Try("a Z# interface has no fields, constructors or code", () =>
    Run("""
        import { Interface } from "std:oop";
        def Fielded238 of Interface { let x: Int32; }
        """) is not null
    && Run("""
        import { Interface } from "std:oop";
        def Coded238 of Interface { fun f(): Int32 { return 1; } }
        """) is { } code && code.Contains("has no code"));

Try("a value of a Z# interface is only cast to what implements it", () =>
    Run("""
        import { Interface } from "std:oop";
        def Thing239 of Interface { fun f(): Int32; }
        def Some239 : Thing239 { new() { } fun f(): Int32 { return 1; } }
        def Other239 { new() { } }
        fun cast239(thing: Thing239): Other239 { return thing as Other239; }
        cast239(Some239());
        """) is { } e && e.Contains("only be cast to what implements it"));

Try("a static field of a Z# class is one variable, given its default value when first used", () =>
    ResetKept() && Run("""
        import { static as static240 } from "std:oop";

        def Counter240 {
            @static240 let made: Int32 = 100;
            let id: Int32;

            new(this) { made = made + 1; this.id = made; }

            fun describe(): String { return "counter " + id + " of " + made; }
        }

        let first240 = Counter240();
        let second240 = Counter240();
        Keep(second240.describe());
        Keep(Counter240.made);
        Counter240.made = 7;
        Keep(first240.describe());
        """) is null && Probe.Kept is ["counter 102 of 102", 102, "counter 101 of 7"]);

Try("a static method of a Z# class is a function, with no object", () =>
    ResetKept() && Run("""
        import { static as static241 } from "std:oop";

        def Point241 {
            let x: Int32;
            let y: Int32;

            new(this, x: Int32, y: Int32) { this.x = x; this.y = y; }

            @static241 fun origin(): Point241 { return Point241(0, 0); }
            @static241 fun sum(a: Int32, b: Int32): Int32 { return a + b; }
            fun total(this): Int32 { return Point241.sum(x, y); }
        }

        Keep(Point241.origin().total());
        Keep(Point241(3, 4).total());
        Keep(Point241.sum(20, 22));
        """) is null && Probe.Kept is [0, 7, 42]);

Try("a static method of a Z# class has no object", () =>
    Run("""
        import { static as static242 } from "std:oop";
        def Thing242 {
            let x: Int32 = 1;
            new() { }
            @static242 fun read(): Int32 { return x; }
        }
        Keep(Thing242.read());
        """) is { } e && e.Contains("Field x of the class Thing242 is its objects'"));

Try("a field of a Z# class's objects isn't the class's", () =>
    Run("""
        def Thing243 {
            let x: Int32 = 1;
            new() { }
        }
        Keep(Thing243.x);
        """) is { } e && e.Contains("is its objects'"));

Try("a class method of a Z# class runs on the class, and a class field is the class's", () =>
    ResetKept() && Run("""
        import { class as class244 } from "std:oop";

        def Pool244 {
            @class244 let opened: Int32 = 10;
            let id: Int32;

            new(this, id: Int32) { this.id = id; }

            @class244 fun open(this): Pool244 {
                opened = opened + 1;
                return Pool244(opened);
            }

            @class244 fun count(): Int32 { return opened; }
        }

        Keep(Pool244.open().id);
        Keep(Pool244.open().id);
        Keep(Pool244.count());
        Keep(Pool244.opened);
        """) is null && Probe.Kept is [11, 12, 12, 12]);

Try("a class is a value, and an interface's class methods are what each class has", () =>
    ResetKept() && Run("""
        import { Interface, class as class245 } from "std:oop";
        import { Type as Type245 } from "std:types";

        def Shape245 of Interface {
            @class245 fun unit(): Shape245;
            @class245 fun kind(): String;
            fun area(): Int32;
        }

        def Square245 : Shape245 {
            let side: Int32;
            new(this, side: Int32) { this.side = side; }
            @class245 fun unit(): Shape245 { return Square245(2); }
            @class245 fun kind(): String { return "square"; }
            fun area(): Int32 { return side * side; }
        }

        def Circle245 : Shape245 {
            let radius: Int32;
            new(this, radius: Int32) { this.radius = radius; }
            @class245 fun unit(): Shape245 { return Circle245(1); }
            @class245 fun kind(): String { return "circle"; }
            fun area(): Int32 { return 3 * radius * radius; }
        }

        fun describe245(shape: Type245[Shape245]): String { return shape.kind() + " " + shape.unit().area(); }

        Keep(describe245(Square245));
        Keep(describe245(Circle245));
        let kind245: Type245[Shape245] = Circle245;
        Keep(kind245.kind());
        """) is null && Probe.Kept is ["square 4", "circle 3", "circle"]);

Try("a Z# class implements an interface's class methods with class methods", () =>
    Run("""
        import { Interface, class as class246 } from "std:oop";
        def Made246 of Interface { @class246 fun make(): Int32; }
        def Plain246 : Made246 { new() { } fun make(): Int32 { return 1; } }
        let made246: Made246 = Plain246();
        """) is { } e && e.Contains("Made246.make isn't one of its methods"));

Try("a class method of a Z# class has no object", () =>
    Run("""
        import { class as class247 } from "std:oop";
        def Thing247 {
            let x: Int32 = 1;
            new() { }
            @class247 fun read(): Int32 { return x; }
        }
        Keep(Thing247.read());
        """) is { } e && e.Contains("a class method runs on the class"));

Try("a Z# class's fields are of generic CLR types over Z# classes", () =>
    Run("""
        import { Dictionary as Dictionary249, List as List249 } from "core:runtime";

        def Item249 {
            let value: Int32;
            new(this, value: Int32) { this.value = value; }
        }

        def Bag249 {
            let items: Dictionary249[String, Item249];
            let order: List249[Item249];
            new(this) { this.items = Dictionary249[String, Item249](); this.order = List249[Item249](); }
            fun put(this, name: String, value: Int32) {
                let item = Item249(value);
                this.items[name] = item;
                this.order.Add(item);
            }
            fun total(this): Int32 { return this.items["a"].value + this.order[1].value; }
        }

        let bag249 = Bag249();
        bag249.put("a", 40);
        bag249.put("b", 2);
        Keep(bag249.total());
        """) is null && Kept() is 42);

Try("a type written in a signature is any code that gives a type", () =>
    Run("""
        import { List as List250 } from "core:runtime";
        import { Type as HIRType250 } from "std:zsharp/hir";

        fun listOf250(element: HIRType250): HIRType250 { return List250[element]; }

        def Item250 { new() { } }

        def Shelf250 {
            let items: listOf250(Item250);
            new(this) { this.items = List250[Item250](); }
            fun count(this): Int32 { this.items.Add(Item250()); return this.items.Count; }
        }

        Keep(Shelf250().count());
        """) is null && Kept() is 1);

Try("a method of a Z# class reaches each member by its name, whatever the member is", () =>
    ResetKept() && Run("""
        import { static as static251, class as class251 } from "std:oop";

        def Meter251 {
            @static251 let scale: Int32 = 10;
            @class251 let made: Int32 = 0;
            let value: Int32;

            new(this, value: Int32) { made = made + 1; this.value = value; }

            fun scaled(): Int32 { return value * scale; }
            fun doubled(): Int32 { return scaled() * 2; }
            @static251 fun times(by: Int32): Int32 { return scale * by; }
            @class251 fun describe(): String { return "made " + made + ", times " + times(3); }
            fun summary(): String { return doubled() + " " + describe(); }
        }

        Keep(Meter251(2).summary());
        """) is null && Kept() is "40 made 1, times 30");

Try("a class method of a Z# class can't call its objects' methods by name", () =>
    Run("""
        import { class as class252 } from "std:oop";
        def Thing252 {
            new() { }
            fun own(): Int32 { return 1; }
            @class252 fun shared(): Int32 { return own(); }
        }
        Keep(Thing252.shared());
        """) is { } e && e.Contains("a class method runs on the class"));

Try("a Z# class has properties, read and written through their accessors", () =>
    ResetKept() && Run("""
        import { static as static269, class as class269 } from "std:oop";

        def Square269 {
            @static269 let made: Int32 = 0;
            @class269 let unit: Int32 = 1;
            let side: Int32;

            new(this, side: Int32) { made = made + 1; this.side = side; }

            get Side(): Int32 { return side; }
            set Side(value: Int32) { side = value; }
            get Doubled(): Int32 { return Side * 2; }

            @static269 get Made(): Int32 { return made; }
            @class269 get Unit(): Int32 { return unit; }
        }

        let square269 = Square269(3);
        Keep(square269.Doubled);
        square269.Side = 5;
        Keep(square269.Side);
        Keep(Square269.Made);
        Keep(Square269.Unit);
        """) is null && Probe.Kept is [6, 5, 1, 1]);

Try("a Z# interface declares properties, which each class that implements it has", () =>
    ResetKept() && Run("""
        import { Interface } from "std:oop";

        def Sized270 of Interface { get Area(): Int32; }

        def Square270 : Sized270 {
            let side: Int32;
            new(this, side: Int32) { this.side = side; }
            get Area(): Int32 { return side * side; }
        }

        fun area270(shape: Sized270): Int32 { return shape.Area; }
        Keep(area270(Square270(4)));
        """) is null && Kept() is 16);

Try("a Z# property's accessors are checked as its class is loaded", () =>
    Run("""
        def Bad271 {
            new() { }
            set Value(a: Int32, b: Int32) { }
        }
        let bad271 = Bad271();
        """) is { } e && e.Contains("takes one value"));

Try("Type[T] is the type of a definition's value, as the compiler finds it", () =>
    ResetKept() && Run("""
        import { Type as Type272 } from "std:types";
        import { class as class272 } from "std:oop";

        def Kind272 {
            @class272 fun name(): String { return "kind"; }
        }

        let kind272: Type272[Kind272] = Kind272;
        Keep(kind272.name());
        let text272: Type272[String] = String;
        Keep(text272);
        """) is null && Probe.Kept is ["kind", var type] && type == typeof(string));

Try("a Z# class's index operator is what indexing the class or one of its objects calls", () =>
    ResetKept() && Run("""
        import { class as class273 } from "std:oop";
        import { operator as operator273 } from "core:zsharp";
        import { Operator as Operator273 } from "std:zsharp/lang";

        def Grid273 {
            let scale: Int32;
            new(this, scale: Int32) { this.scale = scale; }

            @class273 @operator273(Operator273.Index) fun(size: Int32): Grid273 { return Grid273(size); }
            @operator273(Operator273.Index) fun(at: Int32): Int32 { return at * scale; }
        }

        let grid273 = Grid273[10];
        Keep(grid273[4]);
        """) is null && Kept() is 40);

Try("an interface's value has its class shape for a type, and nothing evaluates it", () =>
    Fails("""
        import { Interface } from "std:oop";
        def Shape275 of Interface { fun area(): Int32; }
        let shape275 = Shape275;
        """));

Try("a module has no type object, so it has no type", () =>
    Fails("""
        import { Type as Type274 } from "std:types";
        module Plain274 { let x = 1; }
        let plain274: Type274[Plain274] = Plain274;
        """));

// ---- std:modules: Z#'s own modules, defined in Z#. Installing it makes `module` a Z#
// module, so only these tests and the prelude's come after it.

Try("std:modules installs what compiles its modules", () =>
    Run("""import { Module } from "std:modules";""") is null);

Try("a Z# module's globals are initialized in their order, and its functions use them", () =>
    ResetKept() && Run("""
        module Shop253 {
            let tax: Int32 = 10;
            let sold = 0;
            let label: String = "shop " + tax;

            fun sell(price: Int32): Int32 {
                sold = sold + 1;
                return price + price * tax / 100;
            }
        }

        Keep(Shop253.label);
        Keep(Shop253.sell(200));
        Shop253.tax = 20;
        Keep(Shop253.sell(100));
        Keep(Shop253.sold);
        """) is null && Probe.Kept is ["shop 10", 220, 120, 2]);

Try("what is defined in a Z# module belongs to it", () =>
    ResetKept() && Run("""
        module Geometry254 {
            let unit = 1;

            def Point {
                let x: Int32;
                new(this, x: Int32) { this.x = x; }
                fun moved(): Point { return Point(x + unit); }
            }

            fun origin(): Point { return Point(0); }
        }

        module Labels254 {
            let name = "label";
            fun describe(point: Geometry254.Point): String { return name + " " + point.x; }
        }

        Keep(Geometry254.origin().moved().moved().x);
        Keep(Labels254.describe(Geometry254.Point(7)));
        """) is null && Probe.Kept is [2, "label 7"]);

Try("nothing holds a module, as a module isn't a definition", () =>
    Fails("""module Outer261 { module Inner261 { } }""")
    && Fails("""def Holder262 { module Inner262 { } }""")
    && Fails("""
        import { Module as Module263 } from "std:modules";
        module Outer263 { def Inner263 of Module263 { } }
        """));

Try("a Z# class holds a definition it doesn't define a member for, reached by its name", () =>
    Run("""
        def Holder266 {
            new() { }
            def Inner266 {
                let value: Int32 = 42;
                new() { }
            }
        }
        Keep(Holder266.Inner266().value);
        """) is null && Kept() is 42);

Try("a Z# field has the type of its value, if it is written with none", () =>
    Run("""
        def Counted267 {
            let count = 41;
            new() { }
            fun next(): Int32 { count = count + 1; return count; }
        }
        Keep(Counted267().next());
        """) is null && Kept() is 42);

Try("a Z# class is checked as it is loaded, once it is complete", () =>
    Run("""
        def Empty268 {
            new() { }
            fun nothing(): Int32;
        }
        let empty268 = Empty268();
        """) is { } e && e.Contains("needs code"));

Try("in code, a module is a value, cast to what it is", () =>
    Run("""
        import { Module as Module264 } from "std:modules";
        fun make264(): Module264 {
            return module Made264 {
                let answer = 42;
            } as Module264;
        }
        Keep(make264());
        """) is null && Kept() is ZSharp.HIR.Named { Name: "Made264" });

Try("a module is any definition of Module", () =>
    ResetKept() && Run("""
        import { Module as Module260 } from "std:modules";

        def Counter260 of Module260 {
            let count = 41;
            fun next(): Int32 { count = count + 1; return count; }
        }

        Keep(Counter260.next());
        """) is null && Kept() is 42);

Try("the prelude brings the standard definitions", () =>
    Run("""import "std:prelude";""") is null);

Try("std:dotnet makes a .dll importable by its path, its public types its members", () =>
    ResetKept() && Run($$"""
        import "std:dotnet";
        import { GenericProbe as GenericProbe285, ProbeFlags as ProbeFlags285 } from "{{typeof(GenericProbe).Assembly.Location.Replace('\\', '/')}}";
        Keep(GenericProbe285.Same(5));
        Keep(ProbeFlags285.Low | ProbeFlags285.High);
        """) is null && Probe.Kept is [5, ProbeFlags.Low | ProbeFlags.High]);

Try("two types of one name in different namespaces of an assembly are an error to import, not one of them", () =>
    Run($$"""
        import "std:dotnet";
        import { Twin as Twin313 } from "{{typeof(GenericProbe).Assembly.Location.Replace('\\', '/')}}";
        """) is { } error && error.Contains("more than one type named Twin") && error.Contains("Twins.A") && error.Contains("Twins.B"));

Try("a file nothing is registered for can't be imported", () =>
    Run("""import { x } from "notes.txt";""") is { } error && error.Contains("nothing is registered"));

Try("bitwise operators combine integers, and the flags of an enum as its integers", () =>
    ResetKept() && Run("""
        Keep(6 | 1);
        Keep(6 & 3);
        Keep(2 | 4 & 4);
        """) is null && Probe.Kept is [7, 2, 6]);

Try("the Cecil platform writes a module as an assembly, for a version of .NET, its Z# and CLR classes as classes", () =>
{
    if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget/packages/mono.cecil/0.11.6/lib/netstandard2.0/Mono.Cecil.dll")))
        return true;

    var root = Directory.CreateTempSubdirectory("zsharp-cecil-").FullName;
    var source = Path.Combine(root, "src");
    Directory.CreateDirectory(source);
    File.WriteAllText(Path.Combine(source, ".zs"), """
        import { compile, emit } from "core:project";
        import { Int32, String } from "core:runtime";
        import { Interface as Interface286, static as static286, class as class286 } from "std:oop";
        import { DotNet_Class as DotNet_Class286, DotNet_Interface as DotNet_Interface286, explicitThis as explicitThis286, virtual as virtual286, override as override286, public as public286 } from "core:runtime";
        emit(compile("Twice286"));
        """);
    File.WriteAllText(Path.Combine(source, "clr.zs"), """
        def Greeter286 of DotNet_Interface286 { fun greet(): String; }
        @public286 def Animal286 of DotNet_Class286 {
            @public286 let name: String;
            @public286 @explicitThis286 new(this) { this.name = "animal"; }
            @public286 @virtual286 @explicitThis286 fun sound(this): String { return "..."; }
        }
        @public286 def Dog286 of DotNet_Class286 : Animal286, Greeter286 {
            @public286 @explicitThis286 new(this) { this.name = "dog"; }
            @public286 @override286 @explicitThis286 fun sound(this): String { return "woof"; }
            @public286 @explicitThis286 fun greet(this): String { return this.name + " says " + this.sound(); }
            @public286 def Tag286 of DotNet_Class286 {
                @public286 let text: String;
                @public286 @explicitThis286 new(this) { this.text = "tagged"; }
            }
        }
        @public286 def Holder286[T] of DotNet_Class286 {
            @public286 let held: T;
            @public286 @explicitThis286 new(this, held: T) { this.held = held; }
            @public286 @explicitThis286 fun with[U](this, other: U): U { return other; }
        }
        fun clr(): String {
            let greeter: Greeter286 = Dog286();
            return greeter.greet() + ", " + Holder286[String]("held").held + " " + Holder286[Int32](1).with[String]("with") + ", " + Dog286.Tag286().text;
        }
        """);
    File.WriteAllText(Path.Combine(source, "shapes.zs"), """
        def Shape286 of Interface286 {
            @class286 fun kind(): String;
            fun area(): Int32;
        }
        def Square286 : Shape286 {
            @static286 let made: Int32 = 0;
            let side: Int32;
            new(this, side: Int32) { made = made + 1; this.side = side; }
            @class286 fun kind(): String { return "square"; }
            fun area(): Int32 { return side * side; }
        }
        fun square(side: Int32): Shape286 { return Square286(side); }
        fun describe(shape: Shape286): String {
            if (shape is let square: Square286) return "a square of side " + square.side + " and area " + shape.area() + ", " + Square286.made + " made";
            return "a shape";
        }
        """);
    File.WriteAllText(Path.Combine(source, "twice.zs"), """
        let factor = 2;
        fun twice(value: Int32): Int32 { return value * factor; }
        """);

    var written = Path.Combine(root, "Twice286.dll").Replace('\\', '/');

    return ResetKept() && Run($$"""
        import "std:dotnet";
        import { discover as discover286, Compiler as Compiler286 } from "std:project";
        import { Module as Module286 } from "std:modules";
        import { lower as lower286 } from "std:modules/compiler";
        import { Cecil as Cecil286 } from "pkg:cecil";
        let target286 = Cecil286("Twice286", "net8.0");
        target286.add(lower286(Compiler286(discover286("{{source.Replace('\\', '/')}}")).compile()[0] as Module286, target286));
        target286.write("{{written}}");
        import { twice as twice286, describe as describe286, square as square286, clr as clr286 } from "{{written}}";
        Keep(twice286(21));
        Keep(describe286(square286(3)));
        Keep(clr286());
        """) is null && Probe.Kept is [42, "a square of side 3 and area 9, 1 made", "dog says woof, held with, tagged"];
});

Try("inside its module, a function's name is all of its overloads", () =>
    ResetKept() && Run("""
        def Overloads280 of DotNet_Module {
            fun name(): String { return name("none"); }
            fun name(given: String): String { return "named " + given; }
            fun other(): String { return name("other"); }
        }
        Keep(Overloads280.name());
        Keep(Overloads280.other());
        """) is null && Probe.Kept is ["named none", "named other"]);

Try("a Z# field's type can be made of its own class", () =>
    ResetKept() && Run("""
        import { List as List281 } from "core:runtime";
        def Tree281 {
            let children: List281[Tree281];
            new(this) { this.children = List281[Tree281](); }
        }
        let tree281 = Tree281();
        tree281.children.Add(Tree281());
        Keep(tree281.children.Count);
        """) is null && Kept() is 1);

Try("a CLR module cast to what it is gives what describes it", () =>
    ResetKept() && Run("""
        import { Instance as Instance282 } from "std:zsharp/hir";
        def Described282 of DotNet_Module { fun one(): Int32 { return 1; } }
        Keep(Described282 as Instance282);
        """) is null && Kept() is Package.DotNet.Module { Name: "Described282" });

Try("the project compiler gives what the module files emit, compiled from their sources", () =>
    ResetKept() && Run("""
        import { discover as discover283, Compiler as Compiler283 } from "std:project";
        import { Module as Module283 } from "std:modules";
        let emitted283 = Compiler283(discover283("examples/project/src")).compile();
        Keep((emitted283[0] as Module283).Name);
        Keep((emitted283[1] as Module283).Name);
        Keep((emitted283[1] as Module283).Definitions.Count);
        """) is null && Probe.Kept is ["greetings", "Shop", 3]);

Try("a module file emits anything, as a type, which it is given as what describes it", () =>
{
    var root = Directory.CreateTempSubdirectory("zsharp-project-").FullName;
    File.WriteAllText(Path.Combine(root, ".zs"), """
        import { emit } from "core:project";
        import { Int32 } from "core:runtime";
        emit(Int32);
        """);

    return ResetKept() && Run($$"""
        import { discover as discover284, Compiler as Compiler284 } from "std:project";
        Keep(Compiler284(discover284("{{root.Replace('\\', '/')}}")).compile()[0]);
        """) is null && ReferenceEquals(Kept(), il.Load(typeof(int)));
});

Try("reflect describes a type as the code runs, as what a description cast gives", () =>
    ResetKept() && Run("""
        import { Instance as Instance287 } from "std:zsharp/hir";
        import { reflect as reflect287 } from "std:reflection";
        import { Type as Type287 } from "core:runtime";
        let type287: Type287 = String;
        Keep(reflect287(type287));
        Keep(String as Instance287);
        """) is null && Probe.Kept is [var reflected, var cast] && ReferenceEquals(reflected, il.Load(typeof(string))) && ReferenceEquals(cast, reflected));

Try("a declared function is called as what it is premapped to", () =>
    ResetKept() && Run("""
        import { Instance as Instance288 } from "std:zsharp/hir";
        import { premap as premap288 } from "std:functions/compiler";
        fun answer288(): Int32;
        fun given288(): Int32 { return 42; }
        premap288(answer288 as Instance288, given288 as Instance288);
        Keep(answer288());
        """) is null && Kept() is 42);

Try("an assembly the Cecil platform writes describes types where it runs", () =>
{
    var root = Directory.CreateTempSubdirectory("zsharp-described-").FullName;
    var source = Path.Combine(root, "src");
    Directory.CreateDirectory(source);
    File.WriteAllText(Path.Combine(source, ".zs"), """
        import { compile, emit } from "core:project";
        emit(compile("Described289"));
        """);
    File.WriteAllText(Path.Combine(source, "described.zs"), """
        import { String, Type, Object } from "core:runtime";
        import { Instance } from "std:zsharp/hir";
        import { reflect } from "std:reflection";
        fun direct(): Object { return String as Instance; }
        fun reflected(type: Type): Object { return reflect(type); }
        """);

    var written = Path.Combine(root, "Described289.dll").Replace('\\', '/');

    return ResetKept() && Run($$"""
        import { discover as discover289, Compiler as Compiler289 } from "std:project";
        import { Module as Module289 } from "std:modules";
        import { lower as lower289 } from "std:modules/compiler";
        import { Cecil as Cecil289 } from "pkg:cecil";
        import { Type as Type289 } from "core:runtime";
        let target289 = Cecil289("Described289", "net8.0");
        target289.add(lower289(Compiler289(discover289("{{source.Replace('\\', '/')}}")).compile()[0] as Module289, target289));
        target289.write("{{written}}");
        import { direct as direct289, reflected as reflected289 } from "{{written}}";
        let type289: Type289 = String;
        Keep(direct289());
        Keep(reflected289(type289));
        """) is null && Probe.Kept is [var direct, var reflected] && ReferenceEquals(direct, il.Load(typeof(string))) && ReferenceEquals(reflected, direct);
});

// A project that declares what it is to the environment that imports it: a loader, a
// meta-type and an entry point, which counts how often it is entered, and adds 10 if the
// declarations were registered by then.
string Plugin(string name)
{
    var root = Directory.CreateTempSubdirectory("zsharp-plugin-").FullName;
    var source = Path.Combine(root, "src");
    Directory.CreateDirectory(source);
    File.WriteAllText(Path.Combine(source, ".zs"), $$"""
        import { compile, emit } from "core:project";
        import { build } from "std:framework";
        emit(build(compile("{{name}}")));
        """);
    File.WriteAllText(Path.Combine(source, "plugin.zs"), """
        import { String, Int32, Boolean, DotNet_Class, explicitThis , public } from "core:runtime";
        import { Instance, Definition } from "std:zsharp/hir";
        import { Local } from "std:zsharp/hir/code";
        import { CompilerObject } from "std:zsharp/mir";
        import { Error, Result, IResult } from "std:zsharp/common";
        import { ASTCompiler, CompiledDefinition } from "std:zsharp/compiler/ast";
        import { Definition as DefinitionSyntax } from "std:zsharp/lang/ast";
        import { hirLoader } from "core:compiler";
        import { metaTypes } from "core:zsharp";
        import { entrypoint, definitionLoader, metaType } from "std:framework";

        @public def Thing of DotNet_Class : Instance, Definition {
            @public let Name: String;
            @public @explicitThis new(this, name: String) { this.Name = name; }
        }

        let entered = 0;

        @definitionLoader
        fun loadThing(node: Thing): IResult[CompilerObject, Error] { return Result[CompilerObject].Error("thing " + node.Name); }

        @metaType(Thing)
        fun compileThing(compiler: ASTCompiler, definition: DefinitionSyntax, self: Local): IResult[CompiledDefinition, Error] {
            return Result[CompiledDefinition].Error("compiled");
        }

        @entrypoint
        fun count() {
            entered = entered + 1;
            if (registered()) entered = entered + 10;
        }

        fun times(): Int32 { return entered; }
        fun probe(name: String): String { return hirLoader.LoadDefinition(Thing(name)).UnwrapError().ToString(); }
        fun registered(): Boolean { return metaTypes.Registered(Thing as Instance).IsOk; }
        """);
    return root.Replace('\\', '/');
}

Try("a module compiled from its sources does what it declares when it is entered, once, and not before", () =>
{
    var root = Plugin("Plugin290");

    return ResetKept() && Run($$"""
        import { discover as discover290, Compiler as Compiler290 } from "std:project";
        import { Module as Module290 } from "std:modules";
        import { enter as enter290 } from "std:modules/compiler";
        import { core as core290 } from "core:zsharp";
        let plugin290 = Compiler290(discover290("{{root}}/src")).compile()[0] as Module290;
        core290.Add("plugin290", plugin290);
        import { times as times290, probe as probe290 } from "core:plugin290";
        Keep(times290());
        enter290(plugin290);
        enter290(plugin290);
        Keep(times290());
        Keep(probe290("x"));
        """) is null && Probe.Kept is [0, 11, "thing x"];
});

Try("an assembly the Cecil platform writes does what its module declares when it is imported", () =>
{
    var root = Plugin("Plugin291");

    return ResetKept() && Run($$"""
        import { discover as discover291, Compiler as Compiler291 } from "std:project";
        import { Module as Module291 } from "std:modules";
        import { lower as lower291 } from "std:modules/compiler";
        import { Cecil as Cecil291 } from "pkg:cecil";
        let plugin291 = Compiler291(discover291("{{root}}/src")).compile()[0] as Module291;
        let target291 = Cecil291("Plugin291", "net8.0");
        target291.add(lower291(plugin291, target291));
        target291.write("{{root}}/Plugin291.dll");
        import { times as times291, probe as probe291 } from "{{root}}/Plugin291.dll";
        Keep(times291());
        Keep(probe291("y"));
        """) is null && Probe.Kept is [11, "thing y"];
});

Try("a value of a delegate type over a class defined in Z# is called", () =>
    ResetKept() && Run("""
        import { Func as Func292 , public } from "core:runtime";
        @public def Box292 of DotNet_Class {
            @public let value: Int32;
            @public @explicitThis new(this, value: Int32) { this.value = value; }
        }
        module Doubling292 { fun twice(box: Box292): Box292 { return Box292(box.value * 2); } }
        let doubled292: Func292[Box292, Box292] = Doubling292.twice;
        Keep(doubled292(Box292(21)).value);
        """) is null && Kept() is 42);

Try("a class of a module lowered for the running process lowers again for the Cecil platform", () =>
{
    var root = Directory.CreateTempSubdirectory("zsharp-twice-").FullName;
    var source = Path.Combine(root, "src");
    Directory.CreateDirectory(source);
    File.WriteAllText(Path.Combine(source, ".zs"), """
        import { compile, emit } from "core:project";
        emit(compile("Twice293"));
        """);
    File.WriteAllText(Path.Combine(source, "counter.zs"), """
        import { Object, String, Int32, List, DotNet_Class, explicitThis , public } from "core:runtime";
        @public def Counter of DotNet_Class {
            @public @explicitThis new(this) { }
            @public @explicitThis fun count(this, items: List[Object]): Int32 {
                let found = 0;
                let index = 0;
                while (index < items.Count) {
                    if (items[index] is let text: String) found = found + 1;
                    index = index + 1;
                }
                return found;
            }
        }
        fun strings(): Int32 {
            let items = List[Object]();
            items.Add("a");
            items.Add(items);
            items.Add("b");
            return Counter().count(items);
        }
        """);
    var written = Path.Combine(root, "Twice293.dll").Replace('\\', '/');

    return ResetKept() && Run($$"""
        import { discover as discover293, Compiler as Compiler293 } from "std:project";
        import { Module as Module293 } from "std:modules";
        import { lower as lower293, failures as failures293 } from "std:modules/compiler";
        import { core as core293 } from "core:zsharp";
        import { Cecil as Cecil293 } from "pkg:cecil";
        let twice293 = Compiler293(discover293("{{root.Replace('\\', '/')}}/src")).compile()[0] as Module293;
        core293.Add("twice293", twice293);
        import { strings as strings293 } from "core:twice293";
        Keep(strings293());
        let target293 = Cecil293("Twice293", null);
        target293.add(lower293(twice293, target293));
        Keep(failures293(twice293).Count);
        target293.write("{{written}}");
        import { strings as built293 } from "{{written}}";
        Keep(built293());
        """) is null && Probe.Kept is [2, 0, 2];
});

Try("what a module leaves out as it lowers is a failure of it, by the member's name", () =>
{
    var root = Directory.CreateTempSubdirectory("zsharp-failing-").FullName;
    var source = Path.Combine(root, "src");
    Directory.CreateDirectory(source);
    File.WriteAllText(Path.Combine(source, ".zs"), """
        import { compile, emit } from "core:project";
        emit(compile("Failing294"));
        """);
    File.WriteAllText(Path.Combine(source, "failing.zs"), """
        import { Int32 } from "core:runtime";
        fun good(): Int32 { return 1; }
        fun bad(): Int32 { return "one"; }
        """);

    return ResetKept() && Run($$"""
        import { discover as discover294, Compiler as Compiler294 } from "std:project";
        import { Module as Module294 } from "std:modules";
        import { lower as lower294, failures as failures294 } from "std:modules/compiler";
        import { Cecil as Cecil294 } from "pkg:cecil";
        let failing294 = Compiler294(discover294("{{root.Replace('\\', '/')}}/src")).compile()[0] as Module294;
        let target294 = Cecil294("Failing294", null);
        lower294(failing294, target294);
        let failed294 = failures294(failing294);
        Keep(failed294.Count);
        Keep(failed294[0].StartsWith("bad: "));
        """) is null && Probe.Kept is [1, true];
});

Try("an assembly that carries what was encoded of its module is imported as that module, bound to its code, and reflected as it", () =>
{
    var root = Directory.CreateTempSubdirectory("zsharp-encoded-").FullName;
    var source = Path.Combine(root, "src");
    Directory.CreateDirectory(source);
    File.WriteAllText(Path.Combine(source, ".zs"), """
        import { compile, emit } from "core:project";
        emit(compile("Encoded295"));
        """);
    File.WriteAllText(Path.Combine(source, "encoded.zs"), """
        import { String, Int32, DotNet_Class, explicitThis , public } from "core:runtime";
        @public def Thing of DotNet_Class {
            @public let Name: String;
            @public @explicitThis new(this, name: String) { this.Name = name; }
        }
        let count = 1;
        fun bump(): Int32 {
            count = count + 1;
            return count;
        }
        fun named(name: String): String { return Thing(name).Name; }
        """);
    var written = Path.Combine(root, "Encoded295.dll").Replace('\\', '/');

    return ResetKept() && Run($$"""
        import { discover as discover295, Compiler as Compiler295 } from "std:project";
        import { Module as Module295 } from "std:modules";
        import { lower as lower295 } from "std:modules/compiler";
        import { encode as encode295 } from "std:encoding";
        import { reflect as reflect295 } from "std:reflection";
        import { importers as importers295 } from "core:zsharp";
        import { Cecil as Cecil295 } from "pkg:cecil";
        import { Type as Type295, Object as Object295 } from "core:runtime";
        import { Instance as Instance295 } from "std:zsharp/hir";
        let encoded295 = Compiler295(discover295("{{root.Replace('\\', '/')}}/src")).compile()[0] as Module295;
        let blob295 = encode295(encoded295);
        let target295 = Cecil295("Encoded295", null);
        target295.add(lower295(encoded295, target295));
        target295.embed("ZSharp.Reflection", blob295, "std:encoding");
        target295.write("{{written}}");
        import { bump as bump295, named as named295, count as count295, Thing as Thing295 } from "{{written}}";
        Keep(bump295());
        Keep(bump295());
        Keep(named295("x"));
        let thing295: Type295 = Thing295;
        Keep(reflect295(thing295));
        Keep(Thing295 as Instance295);
        let imported295: Object295 = importers295.Import("{{written}}").Unwrap();
        Keep(imported295);
        let methods295 = thing295.Assembly.GetTypes();
        let found295: Object295 = null;
        let index295 = 0;
        while (index295 < methods295.Length) {
            let method295 = methods295[index295].GetMethod("bump");
            if (method295 != null) found295 = reflect295(method295);
            index295 = index295 + 1;
        }
        Keep(found295);
        """) is null && Probe.Kept is [2, 3, "x", var reflected, var cast, var module, var bump]
        && ReferenceEquals(reflected, cast)
        && module.GetType().Name == "Module"
        && module.GetType().GetField("Definitions")!.GetValue(module) is System.Collections.IList definitions
        && definitions.Cast<object>().Any(definition => ReferenceEquals(definition, bump));
});

Try("a Z# class of an assembly that carries what was encoded of it is that class, bound to what it was built as", () =>
{
    var root = Directory.CreateTempSubdirectory("zsharp-built-class-").FullName;
    var source = Path.Combine(root, "src");
    Directory.CreateDirectory(source);
    File.WriteAllText(Path.Combine(source, ".zs"), """
        import { compile, emit } from "core:project";
        emit(compile("Counters296"));
        """);
    File.WriteAllText(Path.Combine(source, "counter.zs"), """
        import { String, Int32 } from "core:runtime";
        import { class } from "std:oop";

        def Counter {
            let count: Int32;
            new(this, start: Int32) { this.count = start; }
            fun bump(): Int32 {
                count = count + 1;
                return count;
            }
            @class fun kind(): String { return "counter"; }
        }
        """);
    var written = Path.Combine(root, "Counters296.dll").Replace('\\', '/');

    return ResetKept() && Run($$"""
        import { discover as discover296, Compiler as Compiler296 } from "std:project";
        import { Module as Module296 } from "std:modules";
        import { lower as lower296 } from "std:modules/compiler";
        import { encode as encode296 } from "std:encoding";
        import { Cecil as Cecil296 } from "pkg:cecil";
        import { Instance as Instance296 } from "std:zsharp/hir";
        let counters296 = Compiler296(discover296("{{root.Replace('\\', '/')}}/src")).compile()[0] as Module296;
        let blob296 = encode296(counters296);
        let target296 = Cecil296("Counters296", null);
        target296.add(lower296(counters296, target296));
        target296.embed("ZSharp.Reflection", blob296, "std:encoding");
        target296.write("{{written}}");
        import { Counter as Counter296 } from "{{written}}";
        let counter296 = Counter296(41);
        Keep(counter296.bump());
        Keep(counter296.count);
        Keep(Counter296.kind());
        Keep(Counter296 as Instance296);
        """) is null && Probe.Kept is [42, 42, "counter", var described] && described.GetType().Name == "Class";
});

Try("a minus before an operand negates it, binding tighter than * and looser than **", () =>
    ResetKept() && Run("""
        let negative297 = -1;
        Keep(negative297);
        Keep(5 - -3);
        Keep(-2 * 3);
        Keep(-negative297);
        """) is null && Probe.Kept is [-1, 8, -6, 1]);

Try("a return inside a loop returns from what the loop is in", () =>
    ResetKept() && Run("""
        import { Boolean as Boolean298, DotNet_Module as DotNet_Module298 } from "core:runtime";
        def Loops298 of DotNet_Module298 {
            fun first(stop: Boolean298): Int32 {
                let index = 0;
                while (index < 3) {
                    if (stop) return index;
                    index = index + 1;
                }
                return index;
            }
        }
        fun counted298(): Int32 {
            let index = 0;
            while (true) {
                if (index == 2) return index;
                index = index + 1;
            }
            return -1;
        }
        Keep(Loops298.first(true));
        Keep(Loops298.first(false));
        Keep(counted298());
        """) is null && Probe.Kept is [0, 3, 2]);

Try("a name a type that isn't generic shares with generic ones is that type, and indexed, the one of that arity", () =>
    ResetKept() && Run("""
        import { Object as Object299, Type as Type299, List as List299 } from "core:runtime";
        let objectType299: Type299 = Object299;
        import { IList as IList299 } from objectType299.Assembly.Location;
        let numbers299 = List299[Int32]();
        numbers299.Add(3);
        let value299: Object299 = numbers299;
        if (value299 is let list299: IList299) Keep(list299.Count);
        let typed299: IList299[Int32] = numbers299;
        Keep(typed299.Count);
        Keep((value299 as IList299).Count);
        """) is null && Probe.Kept is [1, 1, 1]);

Try("a value of a CLR interface defined in Z# is matched against, and cast down to, a class", () =>
    ResetKept() && Run("""
        import { DotNet_Interface as DotNet_Interface300, DotNet_Module as DotNet_Module300 , public } from "core:runtime";
        def Shape300 of DotNet_Interface300 { fun name(): String; }
        @public def Square300 of DotNet_Class : Shape300 {
            @public @explicitThis new(this) { }
            @public @explicitThis fun name(this): String { return "square"; }
        }
        def Shapes300 of DotNet_Module300 {
            fun kind(shape: Shape300): String {
                if (shape is let square: Square300) return "a " + square.name();
                return "something";
            }
        }
        Keep(Shapes300.kind(Square300()));
        """) is null && Kept() is "a square");

Try("a value of a Z# class goes where an Object is expected in the running process, which makes it one", () =>
    ResetKept() && Run("""
        import { Object as Object301 } from "core:runtime";
        def Point301 {
            let x: Int32;
            new(this, x: Int32) { this.x = x; }
        }
        let point301 = Point301(3);
        let held301: Object301 = point301;
        Keep(held301.GetType().Name);
        Keep(held301 == point301);
        let type301: Object301 = Point301;
        Keep(type301 != null);
        """) is null && Probe.Kept is ["Point301", true, true]);

Try("a value of a Z# class has no members of Object's, as a class has no base", () =>
    Run("""
        def Point302 {
            let x: Int32;
            new(this, x: Int32) { this.x = x; }
        }
        Keep(Point302(1).GetType());
        """) is { } e && e.Contains("has no member named GetType"));

Try("Z# functions of one name overload, in a module and in a document", () =>
    ResetKept() && Run("""
        module Shapes303 {
            fun describe(side: Int32): String { return "a square of " + side; }
            fun describe(name: String): String { return "a " + name; }
            fun both(): String { return describe(2) + ", " + describe("circle"); }
        }
        fun twice303(value: Int32): Int32 { return value * 2; }
        fun twice303(value: String): String { return value + value; }
        Keep(Shapes303.describe(3));
        Keep(Shapes303.describe("triangle"));
        Keep(Shapes303.both());
        Keep(twice303(4));
        Keep(twice303("ab"));
        """) is null && Probe.Kept is ["a square of 3", "a triangle", "a square of 2, a circle", 8, "abab"]);

Try("overloads of a module the Cecil platform writes are imported as overloads, bound to what each was built as", () =>
{
    var root = Directory.CreateTempSubdirectory("zsharp-overloads-").FullName;
    var source = Path.Combine(root, "src");
    Directory.CreateDirectory(source);
    File.WriteAllText(Path.Combine(source, ".zs"), """
        import { compile, emit } from "core:project";
        emit(compile("Overloads304"));
        """);
    File.WriteAllText(Path.Combine(source, "overloads.zs"), """
        import { String, Int32 } from "core:runtime";
        fun twice(value: Int32): Int32 { return value * 2; }
        fun twice(value: String): String { return value + value; }
        """);
    var written = Path.Combine(root, "Overloads304.dll").Replace('\\', '/');

    return ResetKept() && Run($$"""
        import { discover as discover304, Compiler as Compiler304 } from "std:project";
        import { Module as Module304 } from "std:modules";
        import { lower as lower304 } from "std:modules/compiler";
        import { encode as encode304 } from "std:encoding";
        import { Cecil as Cecil304 } from "pkg:cecil";
        let overloads304 = Compiler304(discover304("{{root.Replace('\\', '/')}}/src")).compile()[0] as Module304;
        let blob304 = encode304(overloads304);
        let target304 = Cecil304("Overloads304", null);
        target304.add(lower304(overloads304, target304));
        target304.embed("ZSharp.Reflection", blob304, "std:encoding");
        target304.write("{{written}}");
        import { twice as twice304 } from "{{written}}";
        Keep(twice304(21));
        Keep(twice304("ab"));
        """) is null && Probe.Kept is [42, "abab"];
});

Try("inside a Z# function, its own name is the overloads it is one of", () =>
    ResetKept() && Run("""
        module Names305 {
            fun greet(): String { return greet("world"); }
            fun greet(name: String): String { return "hello " + name; }
        }
        Keep(Names305.greet());
        """) is null && Kept() is "hello world");

Try("a Z# class implements a CLR interface, and goes where it is expected", () =>
    ResetKept() && Run("""
        import { IDisposable as IDisposable306 } from "core:runtime";
        def Resource306 : IDisposable306 {
            let closed: Int32;
            new(this) { this.closed = 0; }
            fun Dispose() { closed = closed + 1; }
        }
        let resource306 = Resource306();
        let disposable306: IDisposable306 = resource306;
        disposable306.Dispose();
        disposable306.Dispose();
        Keep(resource306.closed);
        """) is null && Kept() is 2);

Try("a Z# class is a CO, which the compiler calls through its protocols", () =>
    ResetKept() && Run("""
        import { Object as Object307, Type as Type307, runtime as runtime307, ilLoader as ilLoader307 } from "core:runtime";
        import { CompilerObject as CompilerObject307 } from "std:zsharp/mir";
        import { Error as Error307, Result as Result307, IResult as IResult307 } from "std:zsharp/common";
        import { ICompileIRExpression as ICompileIRExpression307 } from "std:zsharp/compiler/ir";
        import { ITyped as ITyped307 } from "std:zsharp/compiler/ts";
        import { Constant as Constant307, Expression as LIRExpression307 } from "std:zsharp/lir";
        import { ir as ir307, ts as ts307, hirLoader as hirLoader307 } from "core:compiler";
        def Answer307 : CompilerObject307, ICompileIRExpression307, ITyped307 {
            let type: CompilerObject307;
            new(this) {
                let int32: Type307 = Int32;
                this.type = hirLoader307.LoadType(ilLoader307.Load(int32)).Unwrap();
            }
            fun CompileIRExpression(target: Object307): IResult307[LIRExpression307, Error307] {
                return Result307[LIRExpression307].Ok(Constant307(ir307.CompileType(type, target).Unwrap(), 42));
            }
            fun get_Type(): CompilerObject307 { return type; }
        }
        let answer307: CompilerObject307 = Answer307();
        Keep(ts307.TypeOf(answer307).IsOk);
        Keep((ir307.CompileExpression(answer307, runtime307).Unwrap() as Constant307).Value);
        """) is null && Probe.Kept is [true, 42]);

Try("a Z# class the Cecil platform writes implements the CLR interfaces it does", () =>
{
    var root = Directory.CreateTempSubdirectory("zsharp-implements-").FullName;
    var source = Path.Combine(root, "src");
    Directory.CreateDirectory(source);
    File.WriteAllText(Path.Combine(source, ".zs"), """
        import { compile, emit } from "core:project";
        emit(compile("Resources308"));
        """);
    File.WriteAllText(Path.Combine(source, "resource.zs"), """
        import { Int32, IDisposable } from "core:runtime";
        def Resource : IDisposable {
            let closed: Int32;
            new(this) { this.closed = 0; }
            fun Dispose() { closed = closed + 1; }
        }
        fun disposable(): IDisposable { return Resource(); }
        """);
    var written = Path.Combine(root, "Resources308.dll").Replace('\\', '/');

    return ResetKept() && Run($$"""
        import { discover as discover308, Compiler as Compiler308 } from "std:project";
        import { Module as Module308 } from "std:modules";
        import { lower as lower308 } from "std:modules/compiler";
        import { Cecil as Cecil308 } from "pkg:cecil";
        import { IDisposable as IDisposable308 } from "core:runtime";
        let resources308 = Compiler308(discover308("{{root.Replace('\\', '/')}}/src")).compile()[0] as Module308;
        let target308 = Cecil308("Resources308", null);
        target308.add(lower308(resources308, target308));
        target308.write("{{written}}");
        import { disposable as disposable308 } from "{{written}}";
        let made308: IDisposable308 = disposable308();
        made308.Dispose();
        Keep(made308.GetType().GetInterfaces()[0].Name);
        """) is null && Kept() is "IDisposable";
});

Try("methods of a Z# class of one name overload, as functions do", () =>
    ResetKept() && Run("""
        def Shapes309 {
            new(this) { }
            fun describe(this, side: Int32): String { return "a square of " + side; }
            fun describe(this, name: String): String { return "a " + name; }
            fun both(this): String { return describe(2) + ", " + describe("circle"); }
        }
        let shapes309 = Shapes309();
        Keep(shapes309.describe(3));
        Keep(shapes309.describe("triangle"));
        Keep(shapes309.both());
        """) is null && Probe.Kept is ["a square of 3", "a triangle", "a square of 2, a circle"]);

Try("a method reached through a Z# object is a delegate bound to the object", () =>
    ResetKept() && Run("""
        import { Func as Func310 } from "core:runtime";
        def Adder310 {
            let by: Int32;
            new(this, by: Int32) { this.by = by; }
            fun add(this, value: Int32): Int32 { return value + by; }
            fun adding(this): Func310[Int32, Int32] { return add; }
        }
        let add310: Func310[Int32, Int32] = Adder310(10).add;
        Keep(add310(5));
        Keep(Adder310(1).adding()(2));
        """) is null && Probe.Kept is [15, 3]);

Try("classes the Cecil platform writes make each other as they are built, and pass their methods as delegates", () =>
{
    var root = Directory.CreateTempSubdirectory("zsharp-mutual-").FullName;
    var source = Path.Combine(root, "src");
    Directory.CreateDirectory(source);
    File.WriteAllText(Path.Combine(source, ".zs"), """
        import { compile, emit } from "core:project";
        emit(compile("Mutual311"));
        """);
    File.WriteAllText(Path.Combine(source, "mutual.zs"), """
        import { Int32, String, Object, Dictionary, Func } from "core:runtime";
        def Outer {
            let name: String;
            let inner: Inner;
            let others: Dictionary[Object, Inner];
            new(this, name: String) {
                this.name = name;
                this.others = Dictionary[Object, Inner]();
                this.inner = Inner(this);
            }
            fun describe(this): String { return this.inner.describe(); }
        }
        def Inner {
            let owner: Outer;
            let label: String;
            new(this, owner: Outer) { this.owner = owner; this.label = owner.name + "!"; }
            fun describe(this): String { return this.label + " of " + this.owner.name; }
            fun add(this, value: Int32): Int32 { return value + this.label.Length; }
        }
        fun run(): String {
            let add: Func[Int32, Int32] = Outer("x").inner.add;
            return Outer("x").describe() + " " + add(1);
        }
        """);
    var written = Path.Combine(root, "Mutual311.dll").Replace('\\', '/');

    return ResetKept() && Run($$"""
        import { discover as discover311, Compiler as Compiler311 } from "std:project";
        import { Module as Module311 } from "std:modules";
        import { lower as lower311 } from "std:modules/compiler";
        import { Cecil as Cecil311 } from "pkg:cecil";
        let mutual311 = Compiler311(discover311("{{root.Replace('\\', '/')}}/src")).compile()[0] as Module311;
        let target311 = Cecil311("Mutual311", null);
        target311.add(lower311(mutual311, target311));
        target311.write("{{written}}");
        import { run as run311 } from "{{written}}";
        Keep(run311());
        """) is null && Kept() is "x! of x 3";
});

Try("a Z# object goes where Object is expected, as every value does on the CLR, in what the Cecil platform writes too", () =>
{
    var root = Directory.CreateTempSubdirectory("zsharp-objects-").FullName;
    var source = Path.Combine(root, "src");
    Directory.CreateDirectory(source);
    File.WriteAllText(Path.Combine(source, ".zs"), """
        import { compile, emit } from "core:project";
        emit(compile("Objects312"));
        """);
    File.WriteAllText(Path.Combine(source, "objects.zs"), """
        import { Object, String, Int32 } from "core:runtime";
        def Box { let v: Int32; new(this, v: Int32) { this.v = v; } }
        fun describe(value: Object): String { return value.GetType().Name; }
        fun run(): String { return describe(Box(3)) + ", " + describe(7); }
        """);
    var written = Path.Combine(root, "Objects312.dll").Replace('\\', '/');

    return ResetKept() && Run($$"""
        import { discover as discover312, Compiler as Compiler312 } from "std:project";
        import { Module as Module312 } from "std:modules";
        import { lower as lower312 } from "std:modules/compiler";
        import { Cecil as Cecil312 } from "pkg:cecil";
        let objects312 = Compiler312(discover312("{{source.Replace('\\', '/')}}")).compile()[0] as Module312;
        let target312 = Cecil312("Objects312", null);
        target312.add(lower312(objects312, target312));
        target312.write("{{written}}");
        import { run as run312 } from "{{written}}";
        Keep(run312());
        """) is null && Kept() is "Box, Int32";
});

System.Reflection.MethodInfo Kept2Method(string type, string method)
    => Probe.LastType(type).GetMethod(method)!;

/// <summary>
/// Generic methods, for the tests of generic methods given classes still being loaded.
/// </summary>
public static class GenericProbe
{
    public static T Same<T>(T value) => value;

    public static string Pick<T>(T value) => "one";

    public static string Pick<T>(T first, T second) => "two";

    public static bool Copy<T>(T value, out T copy)
    {
        copy = value;
        return value is not null;
    }
}

/// <summary>
/// Flags, for the tests of bitwise operators on an enum.
/// </summary>
[Flags]
public enum ProbeFlags { Low = 1, High = 2 }

/// <summary>
/// An interface with a property, for the property tests.
/// </summary>
public interface IHasLabel
{
    string Label { get; }
}

/// <summary>
/// An object with an instance event, for the event tests.
/// </summary>
public sealed class Bell
{
    public event EventHandler? Rung;

    public void Ring() => Rung?.Invoke(this, EventArgs.Empty);
}

/// <summary>
/// Extension methods for the extension tests.
/// </summary>
public static class ProbeExtensions
{
    public static string Shout(this string text) => text.ToUpperInvariant() + "!";

    public static string ToUpper(this string text) => "extension";

    public static int Twice(this int value) => value * 2;

    public static string Describe(this object value) => "object " + value;

    public static string Label(this int value) => "Int32";
}

/// <summary>
/// More extension methods, with a name the others have too.
/// </summary>
public static class MoreProbeExtensions
{
    public static string Label(this string value) => "String";
}

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

    public static readonly List<string> ImportedWith = [];

    /// <summary>
    /// An overload of import that takes an argument after the source.
    /// </summary>
    public static ZSharp.HIR.Instance ImportWith(string source, string note)
    {
        ImportedWith.Add(note);
        return Core.ZSharp.ModuleScope.Import(source);
    }

    public static string PickString(string value) => "string";

    public static string PickBool(bool value) => "bool";

    public static string Which(object value) => "object";

    public static string Pair(string first, string second) => first + second;

    public static T Same<T>(T value) => value;

    public static string Shared = "";

    public static string SharedProperty { get; set; } = "";

    public static string Upper(string value) => value.ToUpperInvariant();

    public static string ApplyTo(Func<string, string> function, string value) => function(value);

    public static string Greet(string name, string greeting = "Hello") => $"{greeting}, {name}";

    public static string Named(string text) => "text";

    public static string Named(int count) => "count";

    public static int Counter;

    public static int Tally;

    public static int Level { get; set; }

    public static T Identity<T>(T value) => value;

    public static int OrZero(int? value) => value ?? 0;

    public static string Kind<T>(T value) => "generic";

    public static string Kind(int value) => "Int32";

    public static event Action<int>? Ticked;

    public static void Tick(int value) => Ticked?.Invoke(value);

    public static void Record(int value) => Kept.Add(value);

    public static int CountOf(params int[] values) => values.Length;

    public static int[] Numbers() => [1, 2, 3];

    public static string Labels(string prefix, params object[] values) => prefix + string.Join(",", values);

    public static void Swap(ref int left, ref int right) => (left, right) = (right, left);

    public static bool Halve(int value, out int half)
    {
        half = value / 2;
        return value % 2 == 0;
    }

    public static int Next() => ++Counter;

    public static string Which(string value) => "string";

    public static string Which(bool value) => "bool";

    public static string Either(object first, string second) => "first";

    public static string Either(string first, object second) => "second";

    [ZSharp.Importer.ILLoader.Alias("first")]
    public static Package.DotNet.Method First(Package.DotNet.Method method) { Order.Add("first"); return method; }

    [ZSharp.Importer.ILLoader.Alias("second")]
    public static Package.DotNet.Method Second(Package.DotNet.Method method) { Order.Add("second"); return method; }

    public static readonly List<string> Marked = [];

    public static readonly List<ZSharp.HIR.Code.Expression> Seen = [];

    [ZSharp.Importer.ILLoader.Alias("seen")]
    public static object? See(ZSharp.HIR.Code.Expression expression) { Seen.Add(expression); return null; }

    [ZSharp.Importer.ILLoader.Alias("twice")]
    public static ZSharp.HIR.Code.Statement Twice(ZSharp.HIR.Code.Expression expression)
        => new ZSharp.HIR.Code.Block(new ZSharp.HIR.Code.ExpressionStatement(expression), new ZSharp.HIR.Code.ExpressionStatement(expression));

    [ZSharp.Importer.ILLoader.Alias("inject")]
    public static ZSharp.HIR.Code.Parameter Inject(Package.DotNet.Parameter parameter)
        => new Injected(parameter.Name, parameter.Type, "injected");

    [ZSharp.Importer.ILLoader.Alias("mark")]
    public static Package.DotNet.Class Mark(Package.DotNet.Class node) { Marked.Add(node.Name); return node; }

    [ZSharp.Importer.ILLoader.Alias("markMethod")]
    public static Package.DotNet.Method MarkMethod(Package.DotNet.Method node) { Marked.Add(node.Name); return node; }
}

/// <summary>
/// A parameter whose value is injected rather than given by a call.
/// </summary>
public sealed class Injected(string name, ZSharp.HIR.Type? type, string value)
    : ZSharp.HIR.Code.Parameter
{
    public string Name => name;

    public ZSharp.HIR.Type? Type => type;

    public bool IsCaptured { get; set; }

    public string Value => value;
}

/// <summary>
/// An injected parameter takes nothing from the call's arguments.
/// </summary>
sealed class InjectedMatch(string value)
    : ZSharp.MIR.CompilerObject
    , ICTBind
{
    public IResult<ZSharp.MIR.CompilerObject, Error> Bind(ArgumentStream arguments)
        => Scope.HIRCodeLoader.LoadExpression(new C.StringLiteral(value));
}

namespace Twins.A
{
    /// <summary>
    /// One of two types of one name, in different namespaces.
    /// </summary>
    public static class Twin;
}

namespace Twins.B
{
    /// <summary>
    /// The other.
    /// </summary>
    public static class Twin;
}
