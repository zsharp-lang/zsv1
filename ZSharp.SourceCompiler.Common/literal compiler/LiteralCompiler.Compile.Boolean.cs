namespace ZSharp.SourceCompiler
{
    partial class LiteralCompiler
    {
        private IResult CompileFalse(AST.LiteralExpression expression)
            => Result.Ok(new BooleanLiteral(false));

        private IResult CompileTrue(AST.LiteralExpression expression)
            => Result.Ok(new BooleanLiteral(true));
    }
}
