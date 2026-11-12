namespace ZSharp.AST
{
    public sealed class BreakStatement : Statement
    {
        public Expression? Value { get; set; }

        /// <summary>
        /// The name of the loop to leave, or <see langword="null"/> for the innermost one.
        /// </summary>
        public string? From { get; set; }
    }
}
