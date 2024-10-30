namespace ZSharp.AST
{
    public enum LiteralType
    {
        String,

        Number,
        Decimal,
    }

    public sealed class LiteralExpression(string value, LiteralType type) : Expression
    {
        public string Value { get; set; } = value;

        public LiteralType Type { get; set; } = type;

        public Expression? UnitType { get; set; }

        public static LiteralExpression String(string value)
            => new(value, LiteralType.String);
    }
}
