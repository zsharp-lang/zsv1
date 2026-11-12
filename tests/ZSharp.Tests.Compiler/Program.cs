using CommonZ;
using ZSharp.Compiler;
using ZSharp.MIR;
using IResult = IResult<ZSharp.MIR.CompilerObject, CommonZ.Error>;
using Res = CommonZ.Result<ZSharp.MIR.CompilerObject>;

void Report(string name, bool ok)
{
    if (!ok)
        Environment.ExitCode = 1;

    Console.WriteLine($"{(ok ? "pass" : "FAIL")}  {name}");
}

var cg = ZSharp.Compiler.Current.CG;
var ts = ZSharp.Compiler.Current.TS;

var plain = new Plain();

Report("default entrypoint is unsupported", cg.Call(plain, []).IsUnsupported());
Report("default typeof is unsupported", ts.TypeOf(plain).IsUnsupported());
Report("default match is unsupported", ZSharp.Compiler.Current.Matching.Match(plain, plain).IsUnsupported());

var stream = new ArgumentStream([new Argument(plain), new Argument(new Plain()), new Argument("named", new Plain())]);
stream.Pop();
var restPositional = stream.RestPositional();
Report("the positional arguments left are what no parameter took, in order", restPositional.Length == 1 && stream.HasArguments);
var restNamed = stream.RestNamed();
Report("the named arguments left are what no parameter took, by name", restNamed.Count == 1 && restNamed.ContainsKey("named") && !stream.HasArguments);

Report("an error that wraps another has its capabilities", Res.Error(new Located(new UnsupportedError("not here"))).IsUnsupported());
Report("an error without the capability isn't unsupported", !Res.Error(new Located(new ErrorMessage("failed"))).IsUnsupported());

new ZSharp.Compiler.CGDispatchers.Direct.Dispatcher(cg).Apply();
new ZSharp.Compiler.CGDispatchers.Typed.Dispatcher(cg, ts).Apply();
new ZSharp.Compiler.TSDispatchers.Static.Dispatcher(ts).Apply();

var callable = new Callable();
Report("direct call runs", cg.Call(callable, []).Ok(out var called) && called == callable.Marker);
Report("unsupported stays unsupported after dispatchers", cg.Call(plain, []).IsUnsupported());

var failing = new Failing();
var failure = cg.Call(failing, []);
Report("real failure is not unsupported", failure.IsError && !failure.IsUnsupported());
Report("real failure keeps its message", failure.UnwrapError().ToString() == "real failure");

var forward = new Forward(callable);
Report("Is<T> forwarding reaches capability", cg.Call(forward, []).Ok(out var viaForward) && viaForward == callable.Marker);

var indexable = new Indexable();
cg.Index(indexable, [], plain);
Report("set index reaches ICTSetIndex", indexable.SetCalled);
Report("get index still unsupported", cg.Index(indexable, []).IsUnsupported());

var type = new TypeDescriptor();
var value = new Typed(type);

Report("typeof through ITyped", ts.TypeOf(value).Ok(out var t) && t == type);
Report("runtime call through descriptor", cg.Call(value, []).Ok(out var rtCalled) && rtCalled == value);

Report("cast with no assignability is unsupported", cg.Cast(value, plain).IsUnsupported());
ts.IsAssignableTo = (target, source) => target == source;
Report("cast to assignable returns object", cg.Cast(value, type).Ok(out var cast) && cast == value);
Report("implicit cast to assignable returns object", cg.ImplicitCast(value, type).Ok(out var implicitCast) && implicitCast == value);

var previous = cg.Call;
cg.Call = (callee, arguments) =>
    callee is Plain ? Res.Ok(callee) : previous(callee, arguments);
Report("extension checks itself first", cg.Call(plain, []).Ok(out var own) && own == plain);
Report("extension delegates upwards", cg.Call(callable, []).Ok(out var delegated) && delegated == callable.Marker);

Report("library sees dispatcher changes", ZSharp.Compiler.Current.CG.Call == cg.Call);

var separate = new CG();
Report("a second CG is independent", separate.Call(callable, []).IsUnsupported());

class Plain : CompilerObject { }

class Callable : CompilerObject, ICTCallable
{
    public CompilerObject Marker { get; } = new Plain();
    public IResult Call(Argument[] arguments) => Res.Ok(Marker);
}

class Failing : CompilerObject, ICTCallable
{
    public IResult Call(Argument[] arguments) => Res.Error("real failure");
}

class Forward(CompilerObject inner) : CompilerObject
{
    bool CompilerObject.Is<T>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? result) where T : class
        => inner.Is(out result);
}

class Indexable : CompilerObject, ICTSetIndex
{
    public bool SetCalled { get; private set; }
    public IResult Index(Argument[] arguments, CompilerObject value) { SetCalled = true; return Res.Ok(value); }
}

class TypeDescriptor : CompilerObject, IRTCallable
{
    public IResult Call(CompilerObject @object, Argument[] arguments) => Res.Ok(@object);
}

class Typed(CompilerObject type) : CompilerObject, ITyped
{
    public CompilerObject Type { get; } = type;
}

/// <summary>
/// An error with where it happened, which has what the error it wraps has.
/// </summary>
class Located(Error inner) : Error
{
    public override bool Is<T>(out T? capability) where T : class
        => base.Is(out capability) || inner.Is(out capability);
}
