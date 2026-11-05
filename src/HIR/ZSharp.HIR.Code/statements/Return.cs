namespace ZSharp.HIR.Code
{
    public sealed class Return
        : Statement
    {
        public Expression? Value { get; set; }
    }
}
