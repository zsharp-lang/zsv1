namespace ZSharp.SourceCompiler
{
    public delegate Result PostProcess(Result result);

    partial class ExpressionCompiler
    {
        public PostProcess PostProcess { get; init; } = r => r;

        public Result Compile(AST.Expression expression)
            => PostProcess(expression switch
            {
                AST.CallExpression call => Compile(call),
                AST.IdentifierExpression identifier => Compile(identifier),
                AST.LiteralExpression literal => Compile(literal),
                _ => Result.Error($"Invalid expression type: {expression.GetType().Name}"),
            });
    }
}
