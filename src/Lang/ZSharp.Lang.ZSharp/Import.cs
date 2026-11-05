namespace ZSharp.Lang
{
    /// <summary>
    /// Runs the import of a Z# <c>import</c> statement, setting what its names refer to.
    /// </summary>
    public sealed class Import(HIRExpression source, HIR.Code.Reference result)
        : HIRStatement
    {
        public HIRExpression Source { get; } = source;

        public HIR.Code.Reference Result { get; } = result;
    }
}
