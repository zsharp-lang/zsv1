namespace ZSharp.LIR
{
    /// <summary>
    /// Calls the function that <see cref="Callee"/> evaluates to.
    /// </summary>
    public sealed class CallIndirect(
        Expression callee,
        Signature signature,
        params IEnumerable<Expression> arguments
    )
        : Expression
    {
        public Expression Callee { get; set; } = callee;

        public Signature Signature { get; set; } = signature;

        public List<Expression> Arguments { get; } = [.. arguments];
    }
}
