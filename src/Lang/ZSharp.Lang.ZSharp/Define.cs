namespace ZSharp.Lang
{
    /// <summary>
    /// Runs the build code of a definition statement once, and sets what its name
    /// refers to.
    /// </summary>
    public sealed class Define(HIRExpression build, HIR.Code.Reference result)
        : HIRStatement
    {
        public HIRExpression Build { get; } = build;

        public HIR.Code.Reference Result { get; } = result;
    }
}
