namespace ZSharp.HIR.Code
{
    public sealed class Cast
        : Expression
    {
        public required Expression Value { get; set; }

        public required Expression Type { get; set; }
    }
}
