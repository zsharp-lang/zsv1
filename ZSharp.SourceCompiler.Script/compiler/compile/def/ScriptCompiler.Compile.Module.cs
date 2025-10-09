using ZSharp.Compiler;

namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        private Result<CompilerObject> Compile(AST.Module module)
        {
            var moduleCompiler = new Module.ModuleCompiler(Interpreter, module);

            var scope = Interpreter.Compiler.CurrentContext.FindFirstContext<IScopeContext>();
            if (scope is null)
                return Result<CompilerObject>.Error("No scope context found.");
            if (scope.Add(module.Name, moduleCompiler.GetObject()).Error(out var error))
                return Result<CompilerObject>.Error($"Could not add module '{module.Name}' to scope: {error}");

            var result = moduleCompiler.Compile();

            if (!result.Ok(out var definition))
                return result;


            if (
                Interpreter.Compiler.IR.CompileDefinition<IR.Module>(definition, Interpreter.Runtime)
                .When(out var irModule)
                .Error(out error)
            ) return Result.Error(error);

            Interpreter.Runtime.ImportModule(irModule!);

            return result;
        }
    }
}
