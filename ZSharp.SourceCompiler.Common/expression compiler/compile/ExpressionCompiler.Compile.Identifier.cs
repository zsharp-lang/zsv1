namespace ZSharp.SourceCompiler
{
    partial class ExpressionCompiler
    {
        private IResult Compile(AST.IdentifierExpression identifier)
        {
            foreach (var scope in Interpreter.Compiler.CurrentContext.FindContext<IScopeContext>())
                if (scope.Get(identifier.Name).Ok(out var result))
                    return Result<CompilerObject>.Ok(result);

            return Result<CompilerObject>.Error(
                $"Identifier '{identifier.Name}' not found."
            );
        }
    }
}
