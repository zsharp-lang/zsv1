namespace ZSharp.Lang
{
    /// <summary>
    /// Runs the build code of a definition at document level once, loads it, and sets
    /// what code refers to.
    /// </summary>
    public sealed class Define(HIRExpression build, HIR.Code.Reference result)
        : HIRStatement
    {
        public HIRExpression Build { get; } = build;

        public HIR.Code.Reference Result { get; } = result;
    }
}
