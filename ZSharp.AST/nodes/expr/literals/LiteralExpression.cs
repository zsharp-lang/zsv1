namespace ZSharp.AST
{
    public enum LiteralType
    {
        String,

        Number,
        Decimal,

        Null,
        True,
        False,
    }

    public sealed class LiteralExpression(string value, LiteralType type) : Expression
    {
        public string Value { get; set; } = value;

        public LiteralType Type { get; set; } = type;

        public Expression? UnitType { get; set; }

        public static LiteralExpression String(string value)
            => new(value, LiteralType.String);

        public static LiteralExpression Null()
            => new("null", LiteralType.Null);

        public static LiteralExpression True()
            => new("true", LiteralType.True);

        public static LiteralExpression False()
            => new("false", LiteralType.False);
    }
}
