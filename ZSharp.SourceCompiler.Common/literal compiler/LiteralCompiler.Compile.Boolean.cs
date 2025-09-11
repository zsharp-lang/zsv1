namespace ZSharp.SourceCompiler
{
    partial class LiteralCompiler
    {
        private Result CompileFalse(AST.LiteralExpression expression)
            => Result.Ok(new BooleanLiteral(false));

        private Result CompileTrue(AST.LiteralExpression expression)
            => Result.Ok(new BooleanLiteral(true));
    }
}
