namespace ZSharp.AST.nodes.ptrn
{
    public sealed class DiscardPattern : Pattern
    {
        public uint Amount { get; set; } = 1;
    }
}
