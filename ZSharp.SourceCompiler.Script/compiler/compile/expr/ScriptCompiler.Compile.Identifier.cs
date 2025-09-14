using ZSharp.Compiler;

namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        private Result<CompilerObject> Compile(AST.IdentifierExpression identifier)
        {
            foreach (var scope in Interpreter.Compiler.CurrentContext.FindContext<IScopeContext>())
                if (scope.Get(identifier.Name).Ok(out var result))
                    return Result<CompilerObject>.Ok(new Objects.IdentifierBoundObject(this, identifier, result));

            return Result<CompilerObject>.Error(
                $"Identifier '{identifier.Name}' not found."
            );
        }
    }
}
