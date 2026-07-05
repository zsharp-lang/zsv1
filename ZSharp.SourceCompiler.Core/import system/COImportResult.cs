namespace ZSharp.SourceCompiler
{
    internal sealed class COImportResult(CompilerObject co)
        : HIR.Expression
    {
        public CompilerObject CO { get; } = co;
    }
}
