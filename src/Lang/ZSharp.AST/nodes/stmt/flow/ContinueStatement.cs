namespace ZSharp.AST
{
    public sealed class ContinueStatement : Statement
    {
        /// <summary>
        /// The name of the loop to continue, or <see langword="null"/> for the innermost one.
        /// </summary>
        public string? From { get; set; }
    }
}
