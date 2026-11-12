namespace ZSharp.LIR
{
    /// <summary>
    /// Evaluates an expression and discards its value.
    /// </summary>
    public sealed class Evaluate(Expression expression)
        : Statement
    {
        public Expression Expression { get; set; } = expression;
    }
}
