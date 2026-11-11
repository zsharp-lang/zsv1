namespace ZSharp.Lang
{
    /// <summary>
    /// Runs the build code of a definition at document level once, loads it, and sets
    /// what code refers to.
    /// </summary>
    public sealed class Define(HIRExpression build, HIR.Code.Reference result, string? name)
        : HIRStatement
    {
        public HIRExpression Build { get; } = build;

        public HIR.Code.Reference Result { get; } = result;

        /// <summary>
        /// The name the definition was defined with, if it has one.
        /// </summary>
        public string? Name { get; } = name;
    }
}
