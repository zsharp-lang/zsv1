namespace ZSharp.HIR.Code
{
    /// <summary>
    /// The type of a value, which is the <c>TS.TypeOf</c> operation.
    /// </summary>
    public sealed class TypeOf(Expression value)
        : Expression
    {
        public Expression Value { get; set; } = value;
    }
}
