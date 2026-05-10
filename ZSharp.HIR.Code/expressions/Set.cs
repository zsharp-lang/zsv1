namespace ZSharp.HIR.Code
{
    public sealed class Set
        : Expression
    {
        public required Expression Target { get; set; }

        public required Expression Value { get; set; }
    }
}
