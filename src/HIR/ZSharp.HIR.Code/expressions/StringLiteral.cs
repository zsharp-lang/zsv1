namespace ZSharp.HIR.Code
{
    public sealed class StringLiteral(string value)
        : Expression
    {
        public string Value { get; set; } = value;
    }
}
