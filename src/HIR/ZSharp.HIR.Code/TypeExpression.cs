namespace ZSharp.HIR.Code
{
    /// <summary>
    /// Fills a type slot with code that produces the type.
    /// </summary>
    public sealed class TypeExpression(Expression expression)
        : Type
    {
        public Expression Expression { get; set; } = expression;
    }
}
