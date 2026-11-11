namespace ZSharp.LIR
{
    /// <summary>
    /// Writes the value behind a <see cref="Reference"/>.
    /// </summary>
    public sealed class Store(Expression reference, Expression value)
        : Statement
    {
        public Expression Reference { get; set; } = reference;

        public Expression Value { get; set; } = value;
    }
}
