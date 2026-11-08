namespace ZSharp.Compiler
{
    /// <summary>
    /// Implements <see cref="HIRReifier"/> through registered reifiers.
    /// </summary>
    public sealed class MappedReifier(HIRReifier reifier)
    {
        private readonly HIRReifier reifier = reifier;

        private HIRReifier @base = null!;

        public ReifierMap<HIR.Code.Expression> Expressions { get; } = new();

        public ReifierMap<HIR.Code.Statement> Statements { get; } = new();

        /// <summary>
        /// How values are encoded into code, by the type of the value rather than of a node.
        /// </summary>
        public ValueEncoderMap Values { get; } = new();

        public void Apply()
        {
            @base = reifier.Clone();

            reifier.ReifyExpression = ReifyExpression;
            reifier.ReifyStatement = ReifyStatement;
            reifier.EncodeValue = EncodeValue;
        }

        private IResult<HIR.Code.Expression, Error> EncodeValue(object? value)
        {
            var result = @base.EncodeValue(value);

            return result.IsUnsupported() ? Values.Encode(value) : result;
        }

        private IResult<HIR.Code.Expression, Error> ReifyExpression(HIR.Code.Expression expression)
        {
            var result = @base.ReifyExpression(expression);

            return result.IsUnsupported() ? Expressions.Reify(expression) : result;
        }

        private IResult<HIR.Code.Expression, Error> ReifyStatement(HIR.Code.Statement statement)
        {
            var result = @base.ReifyStatement(statement);

            return result.IsUnsupported() ? Statements.Reify(statement) : result;
        }
    }
}
