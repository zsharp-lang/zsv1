namespace ZSharp.HIR.Code
{
    public sealed class ExpressionStatement(Expression expression)
        : Statement
    {
        public Expression Expression { get; } = expression;
    }
}
