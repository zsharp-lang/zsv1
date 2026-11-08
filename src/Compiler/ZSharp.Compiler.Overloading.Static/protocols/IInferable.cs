namespace ZSharp.Compiler
{
    /// <summary>
    /// A candidate that takes type arguments it can infer from the arguments of a call, as
    /// a generic method does. Inferring them gives the candidate that competes with the
    /// others, so overloading compares what would actually be called.
    /// </summary>
    public interface IInferable
    {
        /// <returns>
        /// The candidate given the type arguments the arguments say, or
        /// <see langword="null"/> if they don't say them all.
        /// </returns>
        public CompilerObject? Infer(Argument[] arguments);
    }
}
