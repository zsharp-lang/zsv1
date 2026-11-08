namespace ZSharp.Lang
{
    /// <summary>
    /// Runs the import of a Z# <c>import</c> statement, setting what its names refer to.
    /// </summary>
    /// <param name="importing">The call of the <c>import</c> function.</param>
    public sealed class Import(HIRExpression importing, HIR.Code.Reference result)
        : HIRStatement
    {
        public HIRExpression Importing { get; } = importing;

        public HIR.Code.Reference Result { get; } = result;
    }
}
