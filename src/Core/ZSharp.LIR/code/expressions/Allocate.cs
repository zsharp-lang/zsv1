namespace ZSharp.LIR
{
    /// <summary>
    /// Stores a value somewhere it can be shared, and produces a
    /// <see cref="Reference"/> to it.
    /// </summary>
    public sealed class Allocate(Expression value)
        : Expression
    {
        public Expression Value { get; set; } = value;
    }
}
