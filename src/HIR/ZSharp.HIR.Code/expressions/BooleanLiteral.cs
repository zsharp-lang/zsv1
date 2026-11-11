namespace ZSharp.HIR.Code
{
    public sealed class BooleanLiteral(bool value)
        : Expression
    {
        public bool Value { get; set; } = value;
    }
}
