namespace ZSharp.AST
{
    public sealed class ForStatement : Statement
    {
        public required ForValue Value { get; set; }

        public required Expression Source { get; set; }

        public required Statement Body { get; set; }

        public Statement? Else { get; set; }
    }
}
