namespace ZSharp.SourceCompiler
{
    public sealed class NodeLogOrigin(AST.Node origin) : Logging.LogOrigin
    {
        public AST.Node Origin { get; } = origin;

        public override string? ToString()
        {
            return Origin.TokenInfo?.ToString();
        }
    }
}
