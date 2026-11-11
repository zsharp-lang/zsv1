namespace ZSharp.HIR.Code
{
    /// <summary>
    /// A number with a fraction written in code.
    /// </summary>
    public sealed class DecimalLiteral(double value)
        : Expression
    {
        public double Value { get; set; } = value;
    }
}
