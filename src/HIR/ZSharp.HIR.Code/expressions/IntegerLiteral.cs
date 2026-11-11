namespace ZSharp.HIR.Code
{
    /// <summary>
    /// A whole number written in code.
    /// </summary>
    public sealed class IntegerLiteral(int value)
        : Expression
    {
        public int Value { get; set; } = value;
    }
}
