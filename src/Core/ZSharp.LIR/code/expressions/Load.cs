namespace ZSharp.LIR
{
    /// <summary>
    /// Reads the value behind a <see cref="Reference"/>.
    /// </summary>
    public sealed class Load(Expression reference)
        : Expression
    {
        public Expression Reference { get; set; } = reference;
    }
}
