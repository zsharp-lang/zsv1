namespace ZSharp.LIR
{
    /// <summary>
    /// Leaves a <see cref="Loop"/> that contains it, which need not be the innermost one.
    /// </summary>
    public sealed class Break(Loop loop)
        : Statement
    {
        public Loop Loop { get; set; } = loop;
    }
}
