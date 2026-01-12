namespace ZSharp.AST
{
    public sealed class LetForValue : ForValue
    {
        public required string Name { get; set; }

        public Expression? Type { get; set; }
    }
}
