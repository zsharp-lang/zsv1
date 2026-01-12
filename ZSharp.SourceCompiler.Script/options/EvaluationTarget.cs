namespace ZSharp.SourceCompiler.Script
{
    public enum EvaluationTarget
    {
        /// <summary>
        /// The compiler will evaluate each expression as it's compiled.
        /// </summary>
        Expression,

        /// <summary>
        /// The compiler will only evaluate the final statement in a script.
        /// </summary>
        Statement,
    }
}
