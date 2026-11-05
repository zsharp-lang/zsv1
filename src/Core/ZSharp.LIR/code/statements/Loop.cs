namespace ZSharp.LIR
{
    /// <summary>
    /// Repeats its body until a <see cref="Break"/> or <see cref="Return"/> leaves it.
    /// </summary>
    public sealed class Loop(Statement body)
        : Statement
    {
        public Statement Body { get; set; } = body;
    }
}
