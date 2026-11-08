using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;
using ZSharp.Platform.Runtime;
using C = ZSharp.HIR.Code;
using L = ZSharp.LIR;
using Scope = Core.Compiler.ModuleScope;

void Report(string name, bool ok)
{
    if (!ok)
        Environment.ExitCode = 1;

    Console.WriteLine($"{(ok ? "pass" : "FAIL")}  {name}");
}

// What the CLI will do: compose everything, locally.
var runtime = new Runtime();
var il = new ILLoader(runtime);

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

// print("Hello"), as any importer could build it: there is no Z# here.
var module = il.Load(typeof(Library));

C.Statement Print(string text) => new C.ExpressionStatement(new C.Call
{
    Callee = new C.NamedMember { Target = new C.Get(module), Name = nameof(Library.Print) },
    Arguments = [new C.Argument { Value = new C.StringLiteral(text) }]
});

void Run(params C.Statement[] statements)
{
    var code = new L.Code();

    foreach (var statement in statements)
    {
        var co = Scope.HIRCodeLoader.LoadStatement(statement).Unwrap();
        code.Block.Statements.Add(Scope.IR.CompileStatement(co, runtime).Unwrap());
    }

    var function = new L.Function(new L.Signature()) { Name = "run", Body = code };
    var lir = new L.Module("Script");
    lir.Functions.Add(function);

    runtime.Load(lir);
    runtime.GetMethod(function).Invoke(null, null);
}

Run(Print("Hello"));
Report("HIR to a running call", Library.Output == "Hello");

Run(Print(", "), Print("World!"));
Report("a second run reuses handles", Library.Output == "Hello, World!");

Report("the same method gives the same node",
    il.Load(typeof(Library).GetMethod(nameof(Library.Print))!) == il.Load(typeof(Library).GetMethod(nameof(Library.Print))!));

var result = Scope.HIRCodeLoader.LoadExpression(new C.NamedMember { Target = new C.Get(module), Name = "Missing" });
Report("a missing member is unsupported", result.IsUnsupported());

var overloaded = Scope.HIRCodeLoader.LoadExpression(new C.NamedMember { Target = new C.Get(il.Load(typeof(Console))), Name = "WriteLine" });
Report("an overloaded name loads, and a call picks", overloaded.IsOk);

var arity = Scope.HIRCodeLoader.LoadExpression(new C.Call
{
    Callee = new C.NamedMember { Target = new C.Get(module), Name = nameof(Library.Print) }
});
Report("a wrong argument count is a real error", arity.IsError && !arity.IsUnsupported());

var loadedType = Scope.HIRLoader.LoadType(il.Load(typeof(string)));
Report("a class loads as a type slot", loadedType.Ok(out var typeCo) && Scope.IR.CompileType(typeCo, null).IsOk);

Report("a class is not loaded eagerly", il.Load(typeof(string)) is Package.DotNet.Class { Fields.Count: 0, Methods.Count: 0, Constructors.Count: 0 });

var reference = new C.Reference();
var statement = new C.ExpressionStatement(new C.Call
{
    Callee = new C.NamedMember { Target = new C.Get(reference), Name = nameof(Library.Print) },
    Arguments = [new C.Argument { Value = new C.StringLiteral("!") }]
});
mapped.Instances.Register<C.Reference>(node => Scope.HIRLoader.LoadInstance(node.Value!));
reference.Value = module;
Run(statement);
Report("a reference set after the code was built", Library.Output == "Hello, World!!");

public static class Library
{
    public static string Output = "";

    public static void Print(string message) => Output += message;
}
