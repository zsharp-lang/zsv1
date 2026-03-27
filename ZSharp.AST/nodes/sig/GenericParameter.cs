namespace ZSharp.AST
{
    public sealed class GenericParameter : Node
    {
        public required string Name { get; set; }

        public Expression? Constraint { get; set; }
    }
}
