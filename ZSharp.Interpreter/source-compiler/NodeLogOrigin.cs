namespace ZSharp.ZSSourceCompiler
{
    public sealed class NodeLogOrigin(Node origin) : Compiler.LogOrigin
    {
        public Node Origin { get; } = origin;

        public override string? ToString()
        {
            return Origin.TokenInfo?.ToString();
        }
    }
}
