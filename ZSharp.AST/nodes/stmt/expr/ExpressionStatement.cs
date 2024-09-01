namespace ZSharp.AST
{
    public sealed class ExpressionStatement : Statement
    {
        public required Expression Expression { get; set; }

        public override string ToString()
            => $"{Expression};";
    }
}
