namespace ZSharp.AST
{
    public sealed class CallArgument : Node
    {
        public string? Name { get; set; }

        public required Expression Value { get; set; }
    }
}
