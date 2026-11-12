namespace ZSharp.HIR.Code
{
    /// <summary>
    /// A variable of a function's code that takes its value from a call. How it takes it,
    /// from an argument, a default, or somewhere else entirely, is up to its kind.
    /// </summary>
    public interface Parameter
        : Definition
        , Variable
        , Instance
    {
        /// <summary>
        /// The name the outside sees: what a named argument names and what documents the
        /// parameter. The code may know it by an alias.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of its value, or <see langword="null"/> if a rule of what it belongs
        /// to completes it.
        /// </summary>
        public Type? Type { get; }

    }
}
