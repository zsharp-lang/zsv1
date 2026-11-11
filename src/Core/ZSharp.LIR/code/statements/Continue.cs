namespace ZSharp.LIR
{
    /// <summary>
    /// Starts the next iteration of a <see cref="Loop"/> that contains it, which need not
    /// be the innermost one.
    /// </summary>
    public sealed class Continue(Loop loop)
        : Statement
    {
        public Loop Loop { get; set; } = loop;
    }
}
