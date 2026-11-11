using CommonZ;
using ZSharp.Compiler;
using ZSharp.MIR;
using H = ZSharp.HIR;
using C = ZSharp.HIR.Code;
using Res = CommonZ.Result<ZSharp.MIR.CompilerObject>;

void Report(string name, bool ok)
{
    if (!ok)
        Environment.ExitCode = 1;

    Console.WriteLine($"{(ok ? "pass" : "FAIL")}  {name}");
}

var rei = Core.Compiler.ModuleScope.HIRLoader;
var codeRei = Core.Compiler.ModuleScope.HIRCodeLoader;

Report("default definition is unsupported", rei.LoadDefinition(new Def()).IsUnsupported());
Report("default instance is unsupported", rei.LoadInstance(new Inst()).IsUnsupported());
Report("default type is unsupported", rei.LoadType(new TypeNode()).IsUnsupported());
Report("default expression is unsupported", codeRei.LoadExpression(new C.StringLiteral("x")).IsUnsupported());

var mapped = new MappedLoader(rei);
mapped.Apply();

Report("nothing registered is unsupported", rei.LoadInstance(new Inst()).IsUnsupported());

var defCalls = 0;
mapped.Definitions.Register<Def>(_ => { defCalls++; return Res.Ok(new Co()); });
var def = new Def();
rei.LoadDefinition(def).Ok(out var first);
rei.LoadDefinition(def).Ok(out var second);
Report("definition is cached", defCalls == 1 && first == second);
rei.LoadDefinition(new Def()).Ok(out var third);
Report("different definition is loaded", defCalls == 2 && third != first);

var instCalls = 0;
mapped.Instances.Register<Inst>(_ => { instCalls++; return Res.Ok(new Co()); });
var inst = new Inst();
rei.LoadInstance(inst);
rei.LoadInstance(inst);
Report("instance is not cached", instCalls == 2);

var typeCalls = 0;
mapped.Types.Register<TypeNode>(_ => { typeCalls++; return Res.Ok(new Co()); });
var typeNode = new TypeNode();
rei.LoadType(typeNode);
rei.LoadType(typeNode);
Report("type is not cached", typeCalls == 2);

var both = new TypeInstance();
mapped.Instances.Register<TypeInstance>(_ => Res.Error("as instance"));
mapped.Types.Register<TypeInstance>(_ => Res.Error("as type"));
Report("a node with both markers uses each entrypoint",
    rei.LoadInstance(both).UnwrapError().ToString() == "as instance"
    && rei.LoadType(both).UnwrapError().ToString() == "as type");

var replaced = mapped.Instances.Register<Inst>(node => Res.Error("override"));
Report("register returns previous", replaced is not null);
Report("override wins", rei.LoadInstance(inst).UnwrapError().ToString() == "override");
mapped.Instances.Register<Inst>(node => replaced!(node));
var beforeDelegate = instCalls;
rei.LoadInstance(inst);
Report("override can delegate to previous", instCalls == beforeDelegate + 1);

var failing = new FailingDef();
var failCalls = 0;
mapped.Definitions.Register<FailingDef>(_ => { failCalls++; return Res.Error("nope"); });
var failure = rei.LoadDefinition(failing);
rei.LoadDefinition(failing);
Report("real failure surfaces", failure.IsError && !failure.IsUnsupported());
Report("failure is not cached", failCalls == 2);

var self = new SelfDef();
mapped.Definitions.Register<SelfDef>(node => Res.Ok(new LazyCo(() => rei.LoadDefinition(node).Unwrap())));
rei.LoadDefinition(self).Ok(out var loadedSelf);
Report("lazy CO resolves itself without looping", ((LazyCo)loadedSelf!).Parent() == loadedSelf);

Report("exact type only", rei.LoadInstance(new DerivedInst()).IsUnsupported());

var codeMapped = new MappedCodeLoader(codeRei);
codeMapped.Apply();
codeMapped.Expressions.Register<C.Get>(get => rei.LoadInstance(get.Instance));
Report("extension entrypoint routes through the core one",
    codeRei.LoadExpression(new C.Get(both)).UnwrapError().ToString() == "as instance");
codeMapped.Statements.Register<C.ExpressionStatement>(s => codeRei.LoadExpression(s.Expression));
Report("statement loader reaches expression loader",
    codeRei.LoadStatement(new C.ExpressionStatement(new C.Get(both))).UnwrapError().ToString() == "as instance");
Report("unregistered expression is unsupported", codeRei.LoadExpression(new C.StringLiteral("x")).IsUnsupported());

var lazy = new C.Reference { Value = inst };
Report("reference holds its value", lazy.Value == inst);

class Co : CompilerObject { }
class LazyCo(Func<CompilerObject> parent) : CompilerObject { public Func<CompilerObject> Parent { get; } = parent; }
class Def : H.Definition { }
class FailingDef : H.Definition { }
class SelfDef : H.Definition { }
class Inst : H.Instance { }
class DerivedInst : H.Instance { }
class TypeNode : H.Type { }
class TypeInstance : H.Type, H.Instance { }
