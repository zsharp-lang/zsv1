using CommonZ;
using ZSharp.Compiler;
using ZSharp.MIR;
using L = ZSharp.LIR;

void Report(string name, bool ok)
{
    if (!ok)
        Environment.ExitCode = 1;

    Console.WriteLine($"{(ok ? "pass" : "FAIL")}  {name}");
}

var ir = ZSharp.Compiler.Current.IR;

var plain = new Plain();
Report("default expression is unsupported", ir.CompileExpression(plain, null).IsUnsupported());
Report("default placement is unsupported", ir.CompileDefinition(plain, new L.Module(), null) is UnsupportedError);

new ZSharp.Compiler.IRDispatchers.Static.Dispatcher(ir).Apply();

var str = new L.TypeHandle();
var literal = new Literal(str, "Hello");

Report("expression compiles", ir.CompileExpression(literal, null).Ok(out var e) && e is L.Constant { Value: "Hello" });
Report("statement falls back to Evaluate", ir.CompileStatement(literal, null).Ok(out var s) && s is L.Evaluate);
Report("real statement is used directly", ir.CompileStatement(new ReturnCo(), null).Ok(out var r) && r is L.Return);

// A module places its members without knowing what kinds they are.
var fn = new FunctionCo();
var global = new GlobalCo(str);
var imported = new ImportedFunctionCo();
var module = new L.Module("Main");

var firstErrors = new[] { (CompilerObject)fn, global, imported }
    .Select(item => ir.CompileDefinition(item, module, null))
    .ToArray();

Report("function placed", firstErrors[0] is null && module.Functions.Count == 1);
Report("global placed", firstErrors[1] is null && module.Globals.Count == 1);
Report("imported object is skipped, not an error", firstErrors[2] is UnsupportedError);

// Asking for the entity places nothing, and placing again adds nothing.
var early = new FunctionCo();
ir.CompileDefinition<L.Function>(early, null).Ok(out var shell);
Report("definition is not placed by asking for it", shell!.Module is null);
ir.CompileDefinition(early, module, null);
Report("placing uses the same entity", shell.Module == module);
Report("placing twice is the object's own concern", ir.CompileDefinition(early, module, null) is null && module.Functions.Count == 2);

// A failure while placing is reported.
var broken = ir.CompileDefinition(new BrokenCo(), module, null);
Report("placement failure is reported", broken is not null and not UnsupportedError);

// A reference is not a definition.
Report("handle has a reference", ir.CompileReference<L.ICallable>(imported, null).Ok(out var callable) && callable is L.FunctionHandle);
Report("handle has no definition", ir.CompileDefinition<L.Function>(imported, null).IsUnsupported());
Report("local function references itself", ir.CompileReference<L.ICallable>(fn, null).Ok(out var own) && own is L.Function);

// A type is not its definition: a class with reference semantics.
var classCo = new ClassCo();
ir.CompileDefinition<L.Structure>(classCo, null).Ok(out var structure);
ir.CompileType(classCo, null).Ok(out var classType);
Report("definition is the structure", structure is not null);
Report("type is a reference to it", classType is L.Reference reference && reference.Target == structure);

// Definitions of a kind satisfy a broader request.
Report("definition is covariant", ir.CompileDefinition<L.Definition>(fn, null).Ok(out var d) && d is L.Function);

var shape = new ShapeBound();
Report("target reaches the object", ir.CompileExpression(shape, new Platform()).IsOk);
var wrong = ir.CompileExpression(shape, "not a platform");
Report("wrong target is a real error", wrong.IsError && !wrong.IsUnsupported());

Report("Is<T> forwarding reaches capability", ir.CompileExpression(new Forward(literal), null).IsOk);

class Plain : CompilerObject { }

class Literal(L.Type type, string value) : CompilerObject, ICompileIRExpression
{
    public IResult<L.Expression, Error> CompileIRExpression(object? target)
        => Result<L.Expression>.Ok(new L.Constant(type, value));
}

class ReturnCo : CompilerObject, ICompileIRStatement
{
    public IResult<L.Statement, Error> CompileIRStatement(object? target)
        => Result<L.Statement>.Ok(new L.Return());
}

class FunctionCo
    : CompilerObject
    , ICompileIRDefinitionAs<L.Function>
    , ICompileIRDefinitionIn<L.Module>
    , ICompileIRReference<L.ICallable>
{
    private readonly L.Function function = new(new L.Signature()) { Name = "f" };

    public IResult<L.Function, Error> CompileIRDefinition(object? target)
        => Result<L.Function>.Ok(function);

    public Error? CompileIRDefinition(L.Module owner, object? target)
    {
        if (function.Module is null)
            owner.Functions.Add(function);

        return null;
    }

    public IResult<L.ICallable, Error> CompileIRReference(object? target)
        => Result<L.ICallable>.Ok(function);
}

class GlobalCo(L.Type type)
    : CompilerObject
    , ICompileIRDefinitionIn<L.Module>
{
    public Error? CompileIRDefinition(L.Module owner, object? target)
    {
        owner.Globals.Add(new L.Global(type));
        return null;
    }
}

class ImportedFunctionCo : CompilerObject, ICompileIRReference<L.ICallable>
{
    private readonly L.FunctionHandle handle = new(new L.Signature());

    public IResult<L.ICallable, Error> CompileIRReference(object? target)
        => Result<L.ICallable>.Ok(handle);
}

class BrokenCo : CompilerObject, ICompileIRDefinitionIn<L.Module>
{
    public Error? CompileIRDefinition(L.Module owner, object? target)
        => new ErrorMessage("member failed to compile");
}

class ClassCo
    : CompilerObject
    , ICompileIRDefinitionAs<L.Structure>
    , ICompileIRType
{
    private readonly L.Structure structure = new();

    public IResult<L.Structure, Error> CompileIRDefinition(object? target)
        => Result<L.Structure>.Ok(structure);

    public IResult<L.Type, Error> CompileIRType(object? target)
        => Result<L.Type>.Ok(new L.Reference(structure));
}

class Platform { }

class ShapeBound : CompilerObject, ICompileIRExpression
{
    public IResult<L.Expression, Error> CompileIRExpression(object? target)
        => target is Platform
            ? Result<L.Expression>.Ok(new L.Constant(new L.TypeHandle(), null))
            : Result<L.Expression>.Error("Invalid target");
}

class Forward(CompilerObject inner) : CompilerObject
{
    bool CompilerObject.Is<T>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? result) where T : class
        => inner.Is(out result);
}
