namespace ZSharp.SourceCompiler
{
    partial class LiteralCompiler
    {
        public IResult Compile(AST.LiteralExpression expression)
            => expression.Type switch
            {
                AST.LiteralType.String => CompileString(expression),
                AST.LiteralType.Number => NotImplemented(expression),
                AST.LiteralType.Decimal => NotImplemented(expression),
                AST.LiteralType.Null => NotImplemented(expression),
                AST.LiteralType.True => CompileTrue(expression),
                AST.LiteralType.False => CompileFalse(expression),
                _ => Result.Error($"Invalid literal type: {expression.Type}"),
            };

        private IResult NotImplemented(AST.LiteralExpression expression)
            => Result.Error($"Literal type not implemented: {expression.Type}");
    }
}
