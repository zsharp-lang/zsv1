namespace ZSharp.HIR.Code
{
    public sealed class NamedMember
        : Expression
    {
        public required Expression Target { get; set; }

        public required string Name { get; set; }
    }
}
