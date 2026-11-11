namespace ZSharp.AST
{
    public sealed class IndexExpression : Expression
    {
        public required Expression Target { get; set; }

        public List<CallArgument> Arguments { get; set; } = [];
    }
}
