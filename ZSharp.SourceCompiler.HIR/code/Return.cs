namespace ZSharp.SourceCompiler.HIR.Code
{
    public sealed class Return
        : Statement
    {
        public Node? Value { get; set; }
    }
}
