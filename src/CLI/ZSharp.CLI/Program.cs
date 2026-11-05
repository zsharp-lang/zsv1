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

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: zs <file>");

    return 1;
}

#region Toolchain

var runtime = new Runtime();
var il = new ILLoader(runtime);

var loader = new MappedLoader(Scope.HIRLoader);
var codeLoader = new MappedCodeLoader(Scope.HIRCodeLoader);
loader.Apply();
codeLoader.Apply();

new ZSharp.Compiler.CGDispatchers.Direct.Dispatcher(Scope.CG).Apply();
new ZSharp.Compiler.IRDispatchers.Static.Dispatcher(Scope.IR).Apply();

CodeLoaders.Register(codeLoader, loader, Scope.HIRCodeLoader, Scope.HIRLoader, Scope.CG);
il.Install(loader, codeLoader);

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
std.Add("io", il.Load(typeof(Standard.IO.ModuleScope)));

var importer = new StringImporter();
importer.Register("std", std);

IResult<ZSharp.HIR.Instance, Error> Import(object? source)
    => source is string text
        ? importer.Import(text)
        : Result<ZSharp.HIR.Instance>.Error($"Can't import from {source?.GetType().Name ?? "null"}.");

#endregion

#region Language

var compiler = new ASTCompiler();
var mappedCompiler = new MappedASTCompiler(compiler);
mappedCompiler.Apply();

new Language(compiler).Install(mappedCompiler);
Language.Install(mappedRunner, runner, Import);

#endregion

#region Run

using var stream = File.OpenText(args[0]);

var parser = new Parser(Tokenizer.Tokenize(new(stream)));
var syntax = Syntax.Install(parser);

try
{
    while (parser.HasTokens && !parser.Is(ZSharp.Text.TokenType.EOF))
    {
        var statement = syntax.Document.ParseStatement(parser);

        if (
            compiler.CompileStatement(statement).When(out var hir).Error(out var error)
            || (error = runner.RunStatement(hir!)) is not null
        )
        {
            Console.Error.WriteLine($"error: {error}");

            return 1;
        }
    }
}
catch (ParseError error)
{
    Console.Error.WriteLine($"syntax error: {error.Message}");

    return 1;
}

return 0;

#endregion
