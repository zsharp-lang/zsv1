namespace ZSharp.AST
{
    public sealed class ForStatement : Statement
    {
        public required Expression CurrentItem { get; set; }

        public required Expression Iterable { get; set; }

        public required Statement Body { get; set; }

        public Statement? Else { get; set; }
    }
}
