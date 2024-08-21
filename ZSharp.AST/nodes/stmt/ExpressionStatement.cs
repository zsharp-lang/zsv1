namespace ZSharp.AST
{
    public sealed class ExpressionStatement(Expression expression) : Statement
    {
        public Expression Expression { get; set; } = expression;
    }
}
