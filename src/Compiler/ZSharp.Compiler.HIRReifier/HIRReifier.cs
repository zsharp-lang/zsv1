namespace ZSharp.Compiler
{
    public delegate IResult<HIR.Code.Expression, Error> ReifyExpression(HIR.Code.Expression expression);

    public delegate IResult<HIR.Code.Expression, Error> ReifyStatement(HIR.Code.Statement statement);

    public delegate IResult<HIR.Code.Expression, Error> ReifyPattern(HIR.Code.Pattern pattern);

    /// <summary>
    /// Encodes a value into code: the result is code that stands for that value. Code
    /// that builds code uses it to put a value it holds into the code it builds.
    /// </summary>
    public delegate IResult<HIR.Code.Expression, Error> EncodeValue(object? value);

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

        public ReifyPattern ReifyPattern { get; set; } = pattern
            => Result<HIR.Code.Expression>.Error(Unsupported(pattern));

        public EncodeValue EncodeValue { get; set; } = value
            => Result<HIR.Code.Expression>.Error(new UnsupportedError(
                $"Can't encode a value of type {value?.GetType().Name ?? "null"} into code."
            ));

        public HIRReifier Clone() => (HIRReifier)MemberwiseClone();

        private static UnsupportedError Unsupported(object node)
            => new($"Can't reify HIR node of type {node.GetType().Name}.");
    }
}
