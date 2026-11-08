namespace ZSharp.HIR.Code
{
    /// <summary>
    /// A variable of a function's code that takes its value from a call. How it takes it,
    /// from an argument, a default, or somewhere else entirely, is up to its kind.
    /// </summary>
    public interface Parameter
        : Variable
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

        /// <summary>
        /// Whether a function inside the code captures this parameter. The code it belongs
        /// to then stores its value so that it can be shared.
        /// </summary>
        public bool IsCaptured { get; set; }
    }

    /// <summary>
    /// A parameter that only a positional argument fills, or else its default value.
    /// </summary>
    public sealed class PositionalParameter(string name, Type? type)
        : Parameter
    {
        public PositionalParameter(string name)
            : this(name, null)
        {
        }

        public string Name { get; set; } = name;

        public Type? Type { get; set; } = type;

        public Expression? DefaultValue { get; set; }

        public bool IsCaptured { get; set; }
    }

    /// <summary>
    /// A parameter that only an argument with its name fills, or else its default value.
    /// </summary>
    public sealed class NamedParameter(string name, Type? type)
        : Parameter
    {
        public NamedParameter(string name)
            : this(name, null)
        {
        }

        public string Name { get; set; } = name;

        public Type? Type { get; set; } = type;

        public Expression? DefaultValue { get; set; }

        public bool IsCaptured { get; set; }
    }
}
