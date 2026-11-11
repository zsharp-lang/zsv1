namespace ZSharp.LIR
{
    public sealed class Return(Expression? value = null)
        : Statement
    {
        public Expression? Value { get; set; } = value;
    }
}
