using ZSharp.Compiler;

namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        private Result<CompilerObject> Compile(AST.Module module)
            => new Module.ModuleCompiler(Interpreter, module).Compile();
    }
}
