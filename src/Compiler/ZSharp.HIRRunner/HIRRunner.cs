namespace ZSharp.Compiler
{
    /// <returns><see langword="null"/> if it ran, otherwise why it did not.</returns>
    public delegate Error? RunStatement(HIR.Code.Statement statement);

    public delegate IResult<object?, Error> EvaluateExpression(HIR.Code.Expression expression);

    /// <summary>
    /// Runs HIR immediately. It knows nothing about where the HIR came from.
    /// </summary>
    public class HIRRunner
    {
        public RunStatement RunStatement { get; set; } = statement
            => new UnsupportedError($"Can't run a {statement.GetType().Name}.");

        public EvaluateExpression EvaluateExpression { get; set; } = expression
            => Result<object?>.Error(new UnsupportedError($"Can't evaluate a {expression.GetType().Name}."));

        public HIRRunner Clone() => (HIRRunner)MemberwiseClone();
    }
}
