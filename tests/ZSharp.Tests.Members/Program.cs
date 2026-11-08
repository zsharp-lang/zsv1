using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;
using ZSharp.Platform.Runtime;
using C = ZSharp.HIR.Code;
using Scope = Core.Compiler.ModuleScope;

void Report(string name, bool ok)
{
    if (!ok)
        Environment.ExitCode = 1;

    Console.WriteLine($"{(ok ? "pass" : "FAIL")}  {name}");
}

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
new ZSharp.Compiler.OverloadingDispatchers.Static.Dispatcher(Scope.Overloading).Apply();

CodeLoaders.Register(mappedCode, mapped, Scope.HIRCodeLoader, Scope.HIRLoader, Scope.CG);
il.Install(mapped, mappedCode);

C.Expression Get(ZSharp.HIR.Instance instance) => new C.Get(instance);
C.Expression Member(C.Expression target, string name) => new C.NamedMember { Target = target, Name = name };
C.Expression Str(string value) => new C.StringLiteral(value);
C.Expression Call(C.Expression callee, params C.Expression[] arguments) => new C.Call
{
    Callee = callee,
    Arguments = [.. arguments.Select(argument => new C.Argument { Value = argument })]
};

object? Evaluate(C.Expression expression)
{
    var co = Scope.HIRCodeLoader.LoadExpression(expression).Unwrap();
    return runtime.Evaluate(Scope.IR.CompileExpression(co, runtime).Unwrap());
}

void Run(C.Statement statement)
{
    var co = Scope.HIRCodeLoader.LoadStatement(statement).Unwrap();
    runtime.Execute(Scope.IR.CompileStatement(co, runtime).Unwrap());
}

var widgetType = Get(il.Load(typeof(Widget)));

Report("reference type is a class", il.Load(typeof(string)) is Package.DotNet.Class);
Report("value type is a struct", il.Load(typeof(int)) is Package.DotNet.Struct);
Report("interface is an interface", il.Load(typeof(IDisposable)) is Package.DotNet.Interface);
Report("array has its element", il.Load(typeof(string[])) is Package.DotNet.ArrayType { Element: Package.DotNet.Class });
Report("ref has its target", il.Load(typeof(int).MakeByRefType()) is Package.DotNet.RefType { Target: Package.DotNet.Struct });
Report("a type has one node", ReferenceEquals(il.Load(typeof(string)), il.Load(typeof(string))));

var constructor = il.Load(typeof(Widget).GetConstructor([typeof(string)])!);
Report("constructor is a constructor", constructor is Package.DotNet.Constructor);
Report("constructor node constructs", Evaluate(Call(Get(constructor), Str("via node"))) is Widget { Name: "via node" });

var method = il.Load(typeof(Widget).GetMethod("Make")!);
Report("method knows it is static", method.IsStatic);
Report("method node calls", (string?)Evaluate(Call(Get(method), Str("y"))) == "made y");

Report("constructor", Evaluate(Call(widgetType, Str("a"))) is Widget { Name: "a" });
Report("constructor overload by count", Evaluate(Call(widgetType)) is Widget { Name: "default" });

var shared = new Widget("shared");
var exposed = Get(il.Expose(shared));

Report("exposed object arrives intact", ReferenceEquals(Evaluate(exposed), shared));
Report("instance property", (string?)Evaluate(Member(exposed, "Name")) == "shared");
Report("instance method", (string?)Evaluate(Call(Member(exposed, "Describe"), Str("!"))) == "shared!");
Report("instance field", (string?)Evaluate(Member(exposed, "Tag")) == "tag");

Run(new C.ExpressionStatement(new C.Set { Target = Member(exposed, "Name"), Value = Str("renamed") }));
Report("set property", shared.Name == "renamed");

Run(new C.ExpressionStatement(new C.Set { Target = Member(exposed, "Tag"), Value = Str("new") }));
Report("set field", shared.Tag == "new");

Report("chained members through types", (string?)Evaluate(Member(Member(exposed, "Child"), "Name")) == "child");

Report("static property", (string?)Evaluate(Member(widgetType, "Label")) == "label");
Report("static method", (string?)Evaluate(Call(Member(widgetType, "Make"), Str("x"))) == "made x");
Report("static field", (string?)Evaluate(Member(widgetType, "Version")) == "1");

Report("overload by type: string", (string?)Evaluate(Call(Member(exposed, "Pick"), Str("x"))) == "string");
Report("overload by type: object", (string?)Evaluate(Call(Member(exposed, "Pick"), exposed)) == "widget");
Report("overload through alias", (string?)Evaluate(Member(exposed, "aliased")) == "aliased value");

var list = new List<string>();
Run(new C.ExpressionStatement(Call(Member(Get(il.Expose(list)), "Add"), Str("item"))));
Report("generic instance method", list is ["item"]);

var builder = new System.Text.StringBuilder();
Run(new C.ExpressionStatement(Call(Member(Get(il.Expose(builder)), "Append"), Str("text"))));
Report("BCL overloads resolve", builder.ToString() == "text");

Report("missing member is unsupported", Scope.HIRCodeLoader.LoadExpression(Member(exposed, "Nope")).IsUnsupported());

var readOnly = Scope.HIRCodeLoader.LoadExpression(new C.Set { Target = Member(exposed, "ReadOnly"), Value = Str("x") });
Report("read-only member is a real error", readOnly.IsError && !readOnly.IsUnsupported());

Report("void call stays untyped", Scope.TS.TypeOf(
    Scope.HIRCodeLoader.LoadExpression(Call(Member(exposed, "Reset"))).Unwrap()
).IsUnsupported());

public sealed class Widget
{
    public static string Label => "label";
    public static string Version = "1";

    public string Tag = "tag";

    public Widget() : this("default") { }

    public Widget(string name) => Name = name;

    public string Name { get; set; }

    public Widget Child => new("child");

    public string ReadOnly => "read only";

    [Alias("aliased")]
    public string Hidden => "aliased value";

    public string Describe(string suffix) => Name + suffix;

    public string Pick(string value) => "string";

    public string Pick(Widget value) => "widget";

    public void Reset() { }

    public static string Make(string value) => "made " + value;
}
