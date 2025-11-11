namespace ZSharp.SourceCompiler
{
    public delegate IResult PostProcess(IResult result);

    partial class ExpressionCompiler
    {
        public PostProcess PostProcess { get; init; } = r => r;

        public IResult Compile(AST.Expression expression)
            => PostProcess(expression switch
            {
                AST.BinaryExpression binary => Compile(binary),
                AST.CallExpression call => Compile(call),
                AST.IdentifierExpression identifier => Compile(identifier),
                AST.LiteralExpression literal => Compile(literal),
                _ => Result.Error($"Invalid expression type: {expression.GetType().Name}"),
            });
    }
}
