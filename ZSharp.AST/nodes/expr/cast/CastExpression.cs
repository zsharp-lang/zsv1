namespace ZSharp.AST
{
    public sealed class CastExpression : Expression
    {
        public required Expression Expression { get; set; }

        public required Expression TargetType { get; set; }
    }
}
