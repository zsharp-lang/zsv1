namespace ZSharp.LIR
{
    /// <summary>
    /// Leaves a <see cref="Breakable"/> statement that contains it, which need not be the
    /// innermost one.
    /// </summary>
    public sealed class Break(Breakable target)
        : Statement
    {
        public Breakable Target { get; set; } = target;
    }
}
