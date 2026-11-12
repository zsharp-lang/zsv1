namespace ZSharp.HIR
{
    /// <summary>
    /// Asks for a type to be inferred. It isn't a type: whoever loads the thing that has
    /// it infers the type, or fails if it can't. Loading it as a type is an error, so it
    /// never reaches a CO.
    /// </summary>
    public sealed class Infer
        : Type
    {
        public static Infer Value { get; } = new();

        private Infer()
        {
        }
    }
}
