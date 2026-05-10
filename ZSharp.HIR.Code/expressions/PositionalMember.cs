namespace ZSharp.HIR.Code
{
    public sealed class PositionalMember
        : Expression
    {
        public required Expression Target { get; set; }

        public required int Position { get; set; }
    }
}
