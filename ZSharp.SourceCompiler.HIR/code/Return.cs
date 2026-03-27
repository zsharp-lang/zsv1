namespace ZSharp.SourceCompiler.HIR.code
{
    public sealed class Return
        : Statement
    {
        public Node? Value { get; set; }
    }
}
