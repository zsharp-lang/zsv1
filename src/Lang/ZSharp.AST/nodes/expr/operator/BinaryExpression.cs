namespace ZSharp.AST
{
    public sealed class BinaryExpression : Expression
    {
        public required string Operator { get; set; }

        public required Expression Left { get; set; }

        public required Expression Right { get; set; }
    }
}
