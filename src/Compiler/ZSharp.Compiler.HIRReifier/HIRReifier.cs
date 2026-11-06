namespace ZSharp.Compiler
{
    public delegate IResult<HIR.Code.Expression, Error> ReifyExpression(HIR.Code.Expression expression);

    public delegate IResult<HIR.Code.Expression, Error> ReifyStatement(HIR.Code.Statement statement);

    /// <summary>
    /// Turns code into code that builds it: each entrypoint returns an expression
    /// that evaluates to the given node.
    /// </summary>
    public class HIRReifier
    {
        public ReifyExpression ReifyExpression { get; set; } = expression
            => Result<HIR.Code.Expression>.Error(Unsupported(expression));

        public ReifyStatement ReifyStatement { get; set; } = statement
            => Result<HIR.Code.Expression>.Error(Unsupported(statement));

        public HIRReifier Clone() => (HIRReifier)MemberwiseClone();

        private static UnsupportedError Unsupported(object node)
            => new($"Can't reify HIR node of type {node.GetType().Name}.");
    }
}
