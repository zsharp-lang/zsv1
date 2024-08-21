namespace ZSharp.AST
{
    public enum LiteralType
    {
        String,
    }

    public sealed class Literal(string value, LiteralType type) : Expression
    {
        public string Value { get; set; } = value;

        public LiteralType Type { get; set; } = type;
    }
}
