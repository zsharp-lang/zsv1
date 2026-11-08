using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;
using ZSharp.Importer.String;
using ZSharp.Lang;
using ZSharp.Parser;
using ZSharp.Platform.Runtime;
using ZSharp.Tokenizer;

using CompilerObject = ZSharp.MIR.CompilerObject;
using Scope = Core.Compiler.ModuleScope;

// This file is the entry point of Z#, and the only place that combines its parts.
// It bootstraps the language and runs a script. What gets compiled after that, and
// how, is up to the script.

// Diagnostics print to standard error, so a program's own output stays as it is.
//   --trace-reification  prints the code each reification receives and produces.
//   --dump-definitions   prints the HIR of each definition a document defines.
var options = args.Where(arg => arg.StartsWith("--")).ToHashSet();
var files = args.Where(arg => !arg.StartsWith("--")).ToArray();

if (files.Length != 1 || options.Except(["--trace-reification", "--dump-definitions"]).Any())
{
    Console.Error.WriteLine("Usage: zs [--trace-reification] [--dump-definitions] <file>");

    return 1;
}

#region Toolchain

var runtime = new Runtime();
var il = new ILLoader(runtime);

Core.Runtime.ModuleScope.Runtime = runtime;
Core.Runtime.ModuleScope.ILLoader = il;

var loader = new MappedLoader(Scope.HIRLoader);
var codeLoader = new MappedCodeLoader(Scope.HIRCodeLoader);
Scope.MappedLoader = loader;
Scope.MappedCodeLoader = codeLoader;
loader.Apply();
codeLoader.Apply();

new ZSharp.Compiler.CGDispatchers.Direct.Dispatcher(Scope.CG).Apply();
new ZSharp.Compiler.CGDispatchers.Typed.Dispatcher(Scope.CG, Scope.TS).Apply();

// Members from outside an object's own, as extension methods, come after its own.
var external = new ZSharp.Compiler.CGDispatchers.External.Dispatcher(Scope.CG);
external.Apply();
external.Add(il.Extensions);
new ZSharp.Compiler.TSDispatchers.Static.Dispatcher(Scope.TS).Apply();
new ZSharp.Compiler.IRDispatchers.Static.Dispatcher(Scope.IR).Apply();
new ZSharp.Compiler.OverloadingDispatchers.Static.Dispatcher(Scope.Overloading).Apply();

CodeLoaders.Register(codeLoader, loader, Scope.HIRCodeLoader, Scope.HIRLoader, Scope.CG);
il.Install(loader, codeLoader);

var reifier = new MappedReifier(Scope.HIRReifier);
reifier.Apply();
new CodeReifiers(Scope.HIRReifier, il).Register(reifier);

if (options.Contains("--trace-reification"))
    TraceReification();

#endregion

#region Execution

Error? Execute(CompilerObject @object)
{
    if (Scope.IR.CompileStatement(@object, runtime).When(out var statement).Error(out var error))
        return error;

    runtime.Execute(statement!);

    return null;
}

IResult<object?, Error> Evaluate(CompilerObject @object)
    => Scope.IR.CompileExpression(@object, runtime).When(out var expression).Error(out var error)
        ? Result<object?>.Error(error)
        : Result<object?>.Ok(runtime.Evaluate(expression!));

var runner = new HIRRunner();
var mappedRunner = new MappedRunner(runner, Scope.HIRCodeLoader, Execute, Evaluate);
mappedRunner.Apply();
CodeRunners.Register(mappedRunner, runner);

#endregion

#region Imports

var std = new LibraryImporter("standard");
std.Add("io", il.Load(typeof(Standard.IO.ModuleScope).Module));

var core = new LibraryImporter("core");
core.Add("compiler", il.Load(typeof(Scope).Module));
core.Add("runtime", il.Load(typeof(Core.Runtime.ModuleScope).Module));
core.Add("diagnostics", il.Load(typeof(ZSharp.Diagnostics.ModuleScope).Module));
core.Add("zsharp", il.Load(typeof(Core.ZSharp.ModuleScope).Module));

foreach (var (name, module) in ZSharp.Lang.DotNet.ToolchainModules.All())
    std.Add(name, il.Load(module));

var importer = new StringImporter();
importer.Register("std", std);
importer.Register("core", core);

#endregion

#region Language

var compiler = new ASTCompiler();
var mappedCompiler = new MappedASTCompiler(compiler);
mappedCompiler.Apply();

bool IsUnderlyingType(ZSharp.HIR.Code.Expression type, ZSharp.HIR.Instance underlying)
    => Scope.HIRCodeLoader.LoadExpression(type).When(out var target).IsOk
    && Scope.HIRLoader.LoadInstance(underlying).When(out var definition).IsOk
    && Scope.TS.IsAssignableTo(target!, definition!)
    && Scope.TS.IsAssignableTo(definition!, target!);

var binder = new ZSharp.Lang.Binder();
var mappedBinder = new ZSharp.Lang.MappedBinder(binder);
mappedBinder.Apply();

var language = new Language(binder);

Core.ZSharp.ModuleScope.Compiler = compiler;
Core.ZSharp.ModuleScope.Runner = statement => runner.RunStatement(statement);
Core.ZSharp.ModuleScope.Evaluator = expression => runner.EvaluateExpression(expression);
Core.ZSharp.ModuleScope.Importer = importer.Import;
DocumentCompiler.Install(loader, Scope.HIRLoader);
language.Install(mappedCompiler, IsUnderlyingType);
DocumentCompiler.Install(mappedBinder);

// Operators and import are calls of functions the libraries provide.
foreach (var library in new[] { typeof(Core.Runtime.ModuleScope), typeof(Core.ZSharp.ModuleScope) })
    foreach (var (syntax, function) in il.Operators(library))
        language.Add(syntax, function);

foreach (var (syntax, function) in il.ClrOperators())
    language.Add(syntax, function);

foreach (var (syntax, function) in il.LiftedOperators(language.Functions))
    language.Add(syntax, function);

// Each document runs with a frame holding its locals.
var host = new ZSharp.Lang.DotNet.DotNetDocuments(runtime, il, instance =>
{
    if (options.Contains("--dump-definitions"))
        Console.Error.WriteLine("definition:" + Environment.NewLine + ZSharp.Diagnostics.HIRPrinter.Print(instance) + Environment.NewLine);

    return Scope.HIRLoader.LoadInstance(instance).When(out _).Error(out var error) ? error : null;
});
host.Install(loader);
host.Install(reifier);

Core.ZSharp.ModuleScope.MetaTypes = ZSharp.Lang.DotNet.DotNetMetaTypes.Install(language, mappedCompiler, mappedBinder, runner, il);

#endregion

#region Run

// A source with no prefix is a document, and the document given to the CLI is the first.
var documents = new DocumentImporter(language, compiler, host);
importer.DefaultImporter = documents;

// The standard library written in Z# is found from where the toolchain runs.
if (DocumentLibrary.Above(AppContext.BaseDirectory, "std") is { } standard)
    std.Fallback = new DocumentLibrary(standard, documents);

if (documents.Run(files[0]).When(out _).Error(out var failure))
{
    Console.Error.WriteLine($"error: {failure}");

    return 1;
}

return 0;

#endregion

// Prints each reification a meta-compiler asks for, with what it produced. Reifying
// code that was reified before shows up as its own entry, so each level can be seen.
void TraceReification()
{
    var reifier = Scope.HIRReifier;
    var reifyStatement = reifier.ReifyStatement;
    var reifyExpression = reifier.ReifyExpression;
    var depth = 0;
    var count = 0;

    reifier.ReifyStatement = statement => Trace(statement, node => reifyStatement(node));
    reifier.ReifyExpression = expression => Trace(expression, node => reifyExpression(node));

    IResult<ZSharp.HIR.Code.Expression, Error> Trace<T>(T node, Func<T, IResult<ZSharp.HIR.Code.Expression, Error>> reify)
    {
        if (depth++ > 0)
            try { return reify(node); } finally { depth--; }

        var number = ++count;

        try
        {
            var result = reify(node);

            Console.Error.WriteLine($"reification {number}, of:" + Environment.NewLine + ZSharp.Diagnostics.HIRPrinter.Print(node));
            Console.Error.WriteLine(result.When(out var reified).Error(out var error)
                ? $"failed: {error}" + Environment.NewLine
                : "produced:" + Environment.NewLine + ZSharp.Diagnostics.HIRPrinter.Print(reified) + Environment.NewLine);

            return result;
        }
        finally
        {
            depth--;
        }
    }
}
